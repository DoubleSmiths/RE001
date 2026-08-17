# -*- coding: utf-8 -*-
"""마을 아이콘 5종 + 지역 패럴랙스 레이어를 생성한다.

## 왜

**마을 아이콘** — 여정 창의 마을 5곳이 `Background_Village1` 한 장을 같이 쓴다.
  아스달·윈드포트·애시마인·프로스트키프·던스파이어가 화면에서 전부 같은 그림이다.

**패럴랙스 레이어** — 전투 배경이 지역마다 고유해지긴 했는데, 그 대가로 **한 장**이 됐다.
  원래 CH1BG는 layer01~05 + Ground를 서로 다른 속도로 흘려 입체감을 냈다(슬58).
  한 장짜리 배경은 통째로 같은 속도라 평평하다. 지역마다 3겹을 만들어 깊이를 되돌린다.
    · Far  : 하늘·먼 산 — 통짜 그림(불투명), 느리게
    · Mid  : 중간 지형 — 배경 지운 실루엣, 보통 속도
    · Near : 근경 수풀/바위 — 배경 지운 실루엣, 빠르게
  던전 125곳은 소속 지역 레이어를 물려받는다(BackgroundScroller가 ParentId로 폴백).
  → 25지역 x 3겹 = 75장이면 끝난다. 던전마다 만들면 375장이라 몇 시간이 더 든다.

사용:
  python _tools/aether_scene.py town     # 마을 아이콘 5
  python _tools/aether_scene.py layer    # 지역 패럴랙스 75
  python _tools/aether_scene.py layer --limit 3   # 먼저 한 지역만 보고 판단
이미 있으면 건너뛴다(중단 후 재실행 가능).
"""
import concurrent.futures
import io
import os
import sys
import threading
import time

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from aether_icons import BASE, MODEL, api_key, get, post, UA        # noqa: E402
from aether_unique import GATE, call, generate, load_json, localized  # noqa: E402
from remaster_anim import flood_mask                                # noqa: E402

STAGE = os.path.join(BASE, "_generated")
OUT_TOWN = os.path.join(STAGE, "Region")          # 마을도 Region_{id} 규칙을 그대로 쓴다
OUT_LAYER = os.path.join(STAGE, "RegionLayer")
LIVE_TOWN = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Map", "Region")
LIVE_LAYER = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Background", "RegionLayer")

WORKERS = 2
SUBMIT_GAP = 6.0
TOWN_SIZE = 256
LAYER_W = 512            # 가로로 흘릴 배경 — 폭을 살린다

TOWN_STYLE = ("16-bit pixel art fantasy town illustration, wide view of the settlement, "
              "no characters, no UI, no text, warm readable silhouette")

# 마을마다 성격을 다르게 — 이름만 넣으면 다섯 곳이 다시 비슷해진다.
TOWN_LOOK = {
    11000: "an old inland capital of pale stone, tiled roofs, a central bell tower, green fields around",
    12000: "a windy seaside port, docks and sailing ships, wheeling gulls, tall windmills on the cliff",
    13000: "a soot-stained mining town, ash grey sky, chimneys and ore carts, glowing forge light",
    14000: "a frozen keep in deep snow, thick ice walls, aurora sky, frost-covered pine",
    15000: "a radiant spire city at dawn, white marble towers, golden light, floating banners",
}

LAYER_SPEC = [
    ("Far", "distant background layer only: sky, far mountains or far skyline, hazy and desaturated, "
            "nothing in the foreground, fills the whole frame"),
    ("Mid", "midground scenery elements only (trees, rocks, ruins) arranged in a horizontal row, "
            "isolated on a flat pure magenta background, nothing else"),
    ("Near", "large foreground scenery elements only (big trees, boulders, grass clumps) in a horizontal row, "
             "darker and higher contrast than midground, "
             "isolated on a flat pure magenta background, nothing else"),
]

LAYER_STYLE = ("16-bit pixel art side-scrolling game background layer, wide horizontal composition, "
               "no characters, no UI, no text")


def town_items():
    loc = localized()
    nodes = load_json("TravelNodeData.json")
    out = []
    for n in nodes:
        if n["Category"] != "TOWN":
            continue
        kor = loc.get(n["Name"], n["Name"])
        look = TOWN_LOOK.get(n["Id"], "a fantasy town")
        out.append((f"Region_{n['Id']}", f"{TOWN_STYLE}. The town '{kor}' — {look}."))
    return out


def layer_items():
    loc = localized()
    nodes = load_json("TravelNodeData.json")
    out = []
    for n in nodes:
        if n["Category"] != "EXPLORE" or n["ParentId"] != 0:
            continue                       # 지역만 — 던전은 부모 레이어를 물려받는다
        kor = loc.get(n["Name"], n["Name"])
        for tag, spec in LAYER_SPEC:
            out.append((f"RegionL_{tag}_{n['Id']}",
                        f"{LAYER_STYLE}. Region '{kor}'. Draw the {spec}."))
    return out


def save_town(raw, path):
    im = Image.open(io.BytesIO(raw)).convert("RGBA")
    w, h = im.size
    k = TOWN_SIZE / float(max(w, h))
    im.resize((max(1, int(w * k)), max(1, int(h * k))), Image.NEAREST).save(path)


def save_layer(raw, path, tag):
    im = Image.open(io.BytesIO(raw)).convert("RGBA")
    if tag != "Far":
        # 근·중경은 뒤가 비쳐야 겹이 산다. 배경은 우리가 고른 크로마키(자홍)이므로
        #  연결성을 따지지 않고 **색으로 전부** 지운다.
        #  ⚠️ flood fill만 쓰면 나무 사이처럼 사방이 막힌 자홍 웅덩이가 남는다(실제로 남았다).
        a = np.array(im)
        r, g, b = (a[:, :, i].astype(np.int32) for i in range(3))
        a[(r > 130) & (b > 130) & (g < 120) & (np.abs(r - b) < 90), 3] = 0
        im = flood_mask(Image.fromarray(a, "RGBA"))
        box = im.getbbox()
        if box:
            im = im.crop((0, box[1], im.size[0], box[3]))   # 세로 여백만 잘라낸다
    w, h = im.size
    k = LAYER_W / float(w)
    im.resize((LAYER_W, max(1, int(h * k))), Image.NEAREST).save(path)


def main():
    what = sys.argv[1] if len(sys.argv) > 1 else "town"
    if what == "town":
        items, out_dir, live = town_items(), OUT_TOWN, LIVE_TOWN
    elif what == "layer":
        items, out_dir, live = layer_items(), OUT_LAYER, LIVE_LAYER
    else:
        raise SystemExit("사용법: python _tools/aether_scene.py [town|layer]")

    if "--limit" in sys.argv:
        items = items[:int(sys.argv[sys.argv.index("--limit") + 1])]

    os.makedirs(out_dir, exist_ok=True)
    todo = [(n, p) for n, p in items
            if not os.path.exists(os.path.join(out_dir, n + ".png"))
            and not os.path.exists(os.path.join(live, n + ".png"))]
    print(f"[{what}] 전체 {len(items)} / 생성 대상 {len(todo)} — 동시 {WORKERS}", flush=True)
    if not todo:
        return

    key = api_key()
    done = {"ok": 0, "fail": 0}
    lock = threading.Lock()
    submit_lock = threading.Lock()

    def work(job):
        idx, name, prompt = job
        with submit_lock:
            time.sleep(SUBMIT_GAP)
            print(f"[{idx}/{len(todo)}] {name} 제출", flush=True)
        try:
            raw = generate(name, prompt, key)
            if raw:
                path = os.path.join(out_dir, name + ".png")
                if what == "town":
                    save_town(raw, path)
                else:
                    save_layer(raw, path, name.split("_")[1])
                with lock:
                    done["ok"] += 1
                    print(f"    -> {name}  (완료 {done['ok']}/{len(todo)})", flush=True)
                return
        except Exception as e:
            with lock:
                print(f"  !! {name}: {type(e).__name__}: {e}", flush=True)
        with lock:
            done["fail"] += 1

    jobs = [(i, n, p) for i, (n, p) in enumerate(todo, 1)]
    with concurrent.futures.ThreadPoolExecutor(max_workers=WORKERS) as pool:
        list(pool.map(work, jobs))
    print(f"[{what}] 완료 {done['ok']} / 실패 {done['fail']}", flush=True)


if __name__ == "__main__":
    main()
