# -*- coding: utf-8 -*-
"""마을 건물(대장간·상점)에 필요한 그림을 만든다.

## 왜
  로비 3x2 중 남은 두 칸(TempButton1/2)에 대장간·상점을 넣는데, 협회·연금공방과 같은
  구조를 쓰려면 건물마다 네 종류의 그림이 필요하다.

    · 메뉴 아이콘   {Building}_Icon          64px, 배경 투명   ← 로비 버튼
    · 건물 배경     Background_{Building}    512x512 정사각    ← Village1_*_BG 프리팹의 _BG_Img
    · NPC 입상      {Npc}_Standing_Idle      배경 투명        ← 건물 안에 서 있는 사람
    · NPC 초상      {Npc}_Profile            배경 투명        ← [대화] 버튼 그림

  대장간은 **이미 다 있다** — Blacksmith_Standing_Idle / blacksmith_profile_Idle.
  둘 다 프레임을 옆으로 이어붙인 시트라, [대화] 버튼에 쓸 정지 그림 한 장만 잘라내면 된다
  (API 호출 없음). 그래서 실제 생성 대상은 대장간 배경/아이콘 + 상점 전부다.

사용:
  python _tools/aether_building.py            # 전부
  python _tools/aether_building.py --slice    # 프레임 잘라내기만 (API 미사용)
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

# 윈도우 콘솔은 기본이 cp949라 진행 로그의 em dash 하나에 UnicodeEncodeError로 죽는다.
#  (실제로 죽었다 — 배경 실행이라 exit 0으로 보였고 그림은 한 장도 안 나왔다)
for _s in (sys.stdout, sys.stderr):
    try:
        _s.reconfigure(encoding="utf-8", errors="replace")
    except AttributeError:
        pass

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from aether_icons import BASE, api_key                 # noqa: E402
from aether_unique import generate                     # noqa: E402
from remaster_anim import flood_mask                   # noqa: E402

STAGE = os.path.join(BASE, "_generated", "Building")
SPR = os.path.join(BASE, "Assets", "@Resources", "Sprite")
LIVE_BG = os.path.join(SPR, "Background", "Town")
LIVE_ICON = os.path.join(SPR, "Icon", "Menu Icon")
LIVE_NPC = os.path.join(SPR, "NPC")
LIVE_PORTRAIT = os.path.join(SPR, "UI", "Image", "NPC")

WORKERS = 2
SUBMIT_GAP = 6.0
BG_SIZE = 512        # Background_Guild / Background_Monastery와 같은 크기여야 배치가 안 어긋난다
ICON_SIZE = 64
PORTRAIT_H = 150     # blacksmith_profile_Idle 한 프레임 높이와 맞춘다
NPC_H = 134          # Blacksmith_Standing_Idle 높이

# 실내 배경 — 협회/수도원과 같은 톤이어야 한 마을로 보인다.
BG_STYLE = ("16-bit pixel art fantasy interior illustration, single room seen from the side, "
            "warm readable silhouette, no characters, no UI, no text, no border frame")

ICON_STYLE = ("16-bit pixel art game menu icon, single centered object, chunky readable pixels, "
              "bold dark outline, soft top-left light, flat plain magenta background, "
              "no text, no border frame")

# 배경을 확실히 지우려면 그림에 거의 없는 색을 깔아야 한다 — 패럴랙스 레이어와 같은 자홍 크로마키.
CHROMA = "flat plain magenta background"

NPC_STYLE = ("16-bit pixel art game character, full body standing idle, facing slightly left, "
             "cute anime style, chunky readable pixels, bold dark outline, "
             f"{CHROMA}, no text, no border frame, no ground shadow")

PORTRAIT_STYLE = ("16-bit pixel art game character portrait, upper body from the waist up, "
                  "facing the viewer, cute anime style, chunky readable pixels, bold dark outline, "
                  f"{CHROMA}, no text, no border frame")

# (파일명, 프롬프트, 종류, 출력 폴더)
ITEMS = [
    ("Background_Forge", f"{BG_STYLE}. A blacksmith's forge interior: a glowing coal hearth, "
     "a heavy anvil on a wooden block, hanging tongs and hammers, a quenching barrel, "
     "finished swords racked along the stone wall, sparks and warm orange light.",
     "bg", LIVE_BG),
    ("Background_Shop", f"{BG_STYLE}. A general goods shop interior: a long wooden counter, "
     "shelves of coloured potion bottles, sacks of herbs and ore, hanging lanterns, "
     "a scale and a coin box on the counter, cozy amber light.",
     "bg", LIVE_BG),

    ("Forge_Icon", f"{ICON_STYLE}. A blacksmith anvil with a hammer resting on it, "
     "small orange sparks, iron grey with warm highlights.",
     "icon", LIVE_ICON),
    ("Shop_Icon", f"{ICON_STYLE}. An open money pouch spilling gold coins next to a small "
     "wooden crate, warm brown and gold.",
     "icon", LIVE_ICON),

    ("Shopkeeper_Standing_Idle", f"{NPC_STYLE}. A cheerful young shopkeeper girl in a green "
     "apron over a cream blouse, short brown hair, holding a ledger, coin pouch at her belt.",
     "npc", LIVE_NPC),
    ("shopkeeper_profile", f"{PORTRAIT_STYLE}. A cheerful young shopkeeper girl in a green "
     "apron over a cream blouse, short brown hair, friendly smile.",
     "portrait", os.path.join(LIVE_PORTRAIT, "shopkeeper")),

    # 수도원은 그룹만 저작돼 있고 안에 서 있는 사람은 **연금술사 스프라이트**였다
    #  (연금공방 프리팹을 복제해 만든 흔적). 수도원에 맞는 사람으로 갈아끼운다.
    ("Priestess_Standing_Idle", f"{NPC_STYLE}. A calm young priestess in a white and pale-blue "
     "habit with a hood down, long silver hair, hands folded, a small golden pendant.",
     "npc", LIVE_NPC),
    ("priestess_profile", f"{PORTRAIT_STYLE}. A calm young priestess in a white and pale-blue "
     "habit, long silver hair, gentle serene expression, golden pendant at her collar.",
     "portrait", os.path.join(LIVE_PORTRAIT, "priestess")),
]

# 시트에서 잘라 쓸 것들 — (원본, 프레임수, 쓸 프레임, 결과 경로)
SLICES = [
    (os.path.join(LIVE_NPC, "blacksmith_profile_Idle.png"), 6, 0,
     os.path.join(LIVE_PORTRAIT, "blacksmith", "blacksmith_normal.png")),
]


def dechroma(im):
    """자홍 크로마키를 지운다. 색으로 먼저 훑고(사방이 막힌 웅덩이 대응) 테두리에서 번지게 마무리."""
    im = im.convert("RGBA")
    a = np.array(im)
    r, g, b = (a[:, :, i].astype(np.int32) for i in range(3))
    a[(r > 130) & (b > 130) & (g < 120) & (np.abs(r - b) < 90), 3] = 0
    return flood_mask(Image.fromarray(a, "RGBA"))


def save_bg(raw, path):
    """Background_Guild와 같은 512x512 정사각. 크기가 다르면 _BG_Img 배치가 어긋난다."""
    im = Image.open(io.BytesIO(raw)).convert("RGBA")
    w, h = im.size
    side = min(w, h)
    im = im.crop(((w - side) // 2, (h - side) // 2, (w - side) // 2 + side, (h - side) // 2 + side))
    im.resize((BG_SIZE, BG_SIZE), Image.NEAREST).save(path)


def save_icon(raw, path):
    im = dechroma(Image.open(io.BytesIO(raw)))
    box = im.getbbox()
    if box:
        im = im.crop(box)
    side = max(im.size)
    canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    canvas.paste(im, ((side - im.size[0]) // 2, (side - im.size[1]) // 2))
    canvas.resize((ICON_SIZE, ICON_SIZE), Image.NEAREST).save(path)


def save_cutout(raw, path, target_h):
    """NPC/초상 — 배경만 지우고 높이를 기존 아트에 맞춘다 (가로는 비율 유지)."""
    im = dechroma(Image.open(io.BytesIO(raw)))
    box = im.getbbox()
    if box:
        im = im.crop(box)
    w, h = im.size
    k = target_h / float(h)
    im.resize((max(1, int(w * k)), target_h), Image.NEAREST).save(path)


def do_slices():
    for src, frames, idx, dst in SLICES:
        if os.path.exists(dst):
            print(f"  건너뜀(이미 있음) {os.path.basename(dst)}")
            continue
        if not os.path.exists(src):
            print(f"  !원본 없음 {src}")
            continue
        im = Image.open(src).convert("RGBA")
        fw = im.size[0] // frames
        cut = im.crop((idx * fw, 0, (idx + 1) * fw, im.size[1]))
        box = cut.getbbox()
        if box:
            cut = cut.crop(box)
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        cut.save(dst)
        print(f"  잘라냄 {os.path.basename(src)} [{idx}] -> {dst}  {cut.size}")


def main():
    do_slices()
    if "--slice" in sys.argv:
        return

    os.makedirs(STAGE, exist_ok=True)
    todo = [it for it in ITEMS
            if not os.path.exists(os.path.join(STAGE, it[0] + ".png"))
            and not os.path.exists(os.path.join(it[3], it[0] + ".png"))]
    print(f"[building] 전체 {len(ITEMS)} / 생성 대상 {len(todo)} — 동시 {WORKERS}", flush=True)
    if not todo:
        return

    key = api_key()
    done = {"ok": 0, "fail": 0}
    lock = threading.Lock()
    submit_lock = threading.Lock()

    def work(job):
        idx, (name, prompt, kind, _live) = job
        with submit_lock:
            time.sleep(SUBMIT_GAP)
            print(f"[{idx}/{len(todo)}] {name} 제출", flush=True)
        try:
            raw = generate(name, prompt, key)
            if raw:
                path = os.path.join(STAGE, name + ".png")
                if kind == "bg":
                    save_bg(raw, path)
                elif kind == "icon":
                    save_icon(raw, path)
                elif kind == "npc":
                    save_cutout(raw, path, NPC_H)
                else:
                    save_cutout(raw, path, PORTRAIT_H)
                with lock:
                    done["ok"] += 1
                    print(f"    -> {name}  (완료 {done['ok']}/{len(todo)})", flush=True)
                return
        except Exception as e:
            with lock:
                print(f"  !! {name}: {type(e).__name__}: {e}", flush=True)
        with lock:
            done["fail"] += 1

    jobs = list(enumerate(todo, 1))
    with concurrent.futures.ThreadPoolExecutor(max_workers=WORKERS) as pool:
        list(pool.map(work, jobs))

    print(f"\n완료 {done['ok']} / 실패 {done['fail']}", flush=True)
    print(f"결과는 {STAGE} — 확인 후 _tools/aether_building.py --install 로 옮깁니다", flush=True)


def install():
    """스테이징(_generated/Building)의 결과물을 실제 에셋 폴더로 옮긴다."""
    moved = 0
    for name, _p, _k, live in ITEMS:
        src = os.path.join(STAGE, name + ".png")
        dst = os.path.join(live, name + ".png")
        if not os.path.exists(src) or os.path.exists(dst):
            continue
        os.makedirs(live, exist_ok=True)
        Image.open(src).save(dst)
        print(f"  설치 {name} -> {dst}")
        moved += 1
    print(f"설치 {moved}건")


if __name__ == "__main__":
    if "--install" in sys.argv:
        install()
    else:
        main()
