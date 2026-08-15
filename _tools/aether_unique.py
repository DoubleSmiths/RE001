# -*- coding: utf-8 -*-
"""고유 이미지 생성 — 지역 배경 25종 / 몬스터 도감 아이콘 503종.

## 왜
  · 탐험 노드 150개가 배경 **6종**을 돌려쓰고 있었다(45개 노드가 같은 그림).
  · 몬스터 503종이 아이콘 **31종**을 공유한다(한 아이콘을 22종이 같이 쓴다).
    도감을 열면 같은 그림이 줄줄이 늘어선다.

## 어떻게
  배경  : 지역 25곳의 한글 이름을 그대로 프롬프트에 넣어 분위기를 살린다.
  몬스터: 이름이 "심도 수식어 + 원형"(예: 심연의 슬라임)이라 **수식어가 곧 시각 변주**다.
          어린 → 작고 밝게 / 심연의 → 어둡고 불길하게 / 전설의 → 화려하게.
          원형만 쓰면 503종이 30종처럼 보인다 — 수식어를 프롬프트에 반드시 싣는다.

데이터에서 이름을 읽어오므로 시트가 바뀌면 그대로 따라간다.

사용:
  python _tools/aether_unique.py bg        # 지역 배경 25
  python _tools/aether_unique.py moncard   # 몬스터 도감 아이콘 503
이미 파일이 있으면 건너뛴다(중단 후 재실행 가능).
"""
import concurrent.futures
import io
import json
import os
import sys
import threading
import time

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from aether_icons import BASE, MODEL, api_key, get, post, UA  # noqa: E402
import urllib.error    # noqa: E402
import urllib.request  # noqa: E402

JSON_DIR = os.path.join(BASE, "Assets", "@Resources", "Data", "JsonData")
OUT_BG = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Map", "Region")
OUT_MON = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Monster", "Card")

# 4로 돌렸더니 완료 51건에 429가 43건 — 거절당한 뒤 백오프로 기다리는 시간이 더 컸다.
# 동시 수를 낮추고 제출 간격을 벌리는 편이 결과적으로 빠르다.
WORKERS = 2
SUBMIT_GAP = 6.0
BG_SIZE = 256      # 지역 배경(세로 화면 상단 뷰) — 원본 1024를 축소
MON_SIZE = 96      # 도감 카드 아이콘

BG_STYLE = ("16-bit pixel art side-scrolling game background, wide landscape, "
            "layered parallax depth, no characters, no UI, no text, "
            "muted palette with one dominant accent color")

MON_STYLE = ("16-bit pixel art game bestiary portrait, single creature centered, "
             "full body side view facing right, chunky readable pixels, bold dark outline, "
             "flat plain background, no text, no border frame")

# 심도 수식어 → 시각 변주. 이게 503종을 서로 다르게 만드는 핵심이다.
MODIFIER_LOOK = {
    "어린":      "small, young, bright cheerful colors, soft rounded shapes",
    "거친":      "scruffy, battle-worn, dusty fur or hide, scars",
    "굶주린":    "gaunt and bony, hungry glowing eyes, ragged silhouette",
    "성난":      "enraged posture, red glowing eyes, aggressive stance, steam",
    "사나운":    "savage, bristling spikes and fangs, torn ears, wild mane",
    "오래된":    "ancient, mossy and weathered, cracked surface, faded colors",
    "타락한":    "corrupted, sickly purple veins, oozing dark matter, twisted limbs",
    "심연의":    "abyssal, deep indigo and black, void mist, glowing pale eyes",
    "왕관을 쓴": "crowned, wearing a golden crown, regal ornaments, proud posture",
    "전설의":    "legendary, radiant golden aura, ornate markings, majestic scale",
}


def load_json(name):
    with io.open(os.path.join(JSON_DIR, name), encoding="utf-8-sig") as f:
        d = json.load(f)
    return d[list(d.keys())[0]]


def localized():
    """TextKey → 한글"""
    return {r["TextKey"]: r["Kor"] for r in load_json("LocalizationData.json")}


# 던전 심도(D1~D5) 분위기 — 같은 지역이라도 깊이 들어갈수록 달라 보이게.
DEPTH_LOOK = [
    "entrance area, bright daylight, open sky, inviting",
    "deeper in, dappled light, denser terrain",
    "midway, overcast and shadowed, ruins scattered about",
    "deep interior, dark, glowing hazards, oppressive",
    "innermost lair, ominous red-black sky, monumental silhouettes, boss arena",
]


def bg_items():
    """탐험 노드 150개 — 지역 25 + 던전 125. 던전은 소속 지역 이름 + 심도로 변주한다."""
    loc = localized()
    nodes = load_json("TravelNodeData.json")
    region_name = {n["Id"]: loc.get(n["Name"], n["Name"])
                   for n in nodes if n["Category"] == "EXPLORE" and n["ParentId"] == 0}

    out = []
    depth_seen = {}
    for n in nodes:
        if n["Category"] != "EXPLORE":
            continue
        kor = loc.get(n["Name"], n["Name"])
        if n["ParentId"] == 0:
            desc = f"The place is '{kor}' — a fantasy RPG region."
        else:
            # 같은 부모 안에서 몇 번째 던전인지로 심도를 잡는다(D1~D5).
            i = depth_seen.get(n["ParentId"], 0)
            depth_seen[n["ParentId"]] = i + 1
            look = DEPTH_LOOK[min(i, len(DEPTH_LOOK) - 1)]
            parent = region_name.get(n["ParentId"], "")
            desc = f"A dungeon area '{kor}' inside the region '{parent}' — {look}."
        out.append((f"Region_{n['Id']}", f"{BG_STYLE}. {desc}"))
    return out


def mon_items():
    loc = localized()
    out = []
    for c in load_json("CreatureData.json"):
        kor = loc.get(c["Name"], "")
        if not kor:
            continue
        mod, base = "", kor
        for m in MODIFIER_LOOK:
            if kor.startswith(m + " "):
                mod, base = m, kor[len(m) + 1:].strip()
                break
        look = MODIFIER_LOOK.get(mod, "")
        desc = f"a fantasy RPG monster, {base}"
        if look:
            desc += f", {look}"
        out.append((f"Mon_{c['DataId']}", f"{MON_STYLE}. {desc}."))
    return out


def postprocess(raw, out_path, size):
    """배경 제거 없이 축소 (배경은 그대로, 몬스터는 네 모서리 색 제거)."""
    im = Image.open(io.BytesIO(raw)).convert("RGBA")
    if size == MON_SIZE:
        px = im.load()
        w, h = im.size
        corners = [px[0, 0], px[w - 1, 0], px[0, h - 1], px[w - 1, h - 1]]
        bg = tuple(sum(c[i] for c in corners) // 4 for i in range(3))
        for y in range(h):
            for x in range(w):
                r, g, b, a = px[x, y]
                if abs(r - bg[0]) + abs(g - bg[1]) + abs(b - bg[2]) < 126:
                    px[x, y] = (r, g, b, 0)
        box = im.getbbox()
        if box:
            im = im.crop(box)
    w, h = im.size
    scale = size / float(max(w, h))
    im = im.resize((max(1, int(w * scale)), max(1, int(h * scale))), Image.NEAREST)
    im.save(out_path)


# ── 공용 스로틀 게이트 ────────────────────────────────────────────
#  429는 이 계정의 **롤링 쿼터**다. 항목별로 각자 백오프하면 워커들이 번갈아 계속 두드려
#  한도가 열리지 않고, 재시도 6회를 다 태우고 실패로 떨어진다(완료 0 / 429 36건을 그렇게 봤다).
#  제출뿐 아니라 **작업 조회(GET)도 한도를 먹는다** — 4초마다 90번씩 도니 그쪽이 오히려 많다.
#  그래서 429를 보면 한 워커가 아니라 **전원이 같이 쉰다**. 성공하면 쿨다운을 되돌린다.
class Gate:
    def __init__(self):
        self.lock = threading.Lock()
        self.until = 0.0
        self.cool = 60.0     # 다음 429에 쉴 시간
        self.hits = 0

    def wait(self):
        while True:
            with self.lock:
                remain = self.until - time.monotonic()
            if remain <= 0:
                return
            time.sleep(min(remain, 5.0))

    def trip(self, what):
        with self.lock:
            self.hits += 1
            self.until = max(self.until, time.monotonic() + self.cool)
            cool = self.cool
            self.cool = min(self.cool * 2, 900.0)   # 최대 15분
        print(f"  ~~ 429 — 전 작업 {cool:.0f}s 정지 ({what})", flush=True)

    def ok(self):
        with self.lock:
            if self.cool > 60.0:
                self.cool = max(60.0, self.cool / 2)


GATE = Gate()


def call(fn, what, tries=40):
    """429는 실패가 아니라 '기다려라'다. 한도가 열릴 때까지 버틴다(항목을 버리지 않는다)."""
    for _ in range(tries):
        GATE.wait()
        try:
            r = fn()
            GATE.ok()
            return r
        except urllib.error.HTTPError as e:
            if e.code == 429:
                GATE.trip(what)
                continue
            if e.code in (500, 502, 503, 504):
                time.sleep(10)
                continue
            raise
        except (urllib.error.URLError, TimeoutError, OSError):
            time.sleep(10)
    print(f"  !! {what}: 재시도 소진", flush=True)
    return None


POLL_GAP = 10   # 조회도 한도를 먹는다 — 4초는 너무 잦았다


def generate(name, prompt, key):
    job = call(lambda: post("/generate/image", {"prompt": prompt, "ai_model": MODEL}, key), f"{name} 제출")
    if not job:
        return None
    jid = job.get("job_id")
    if not jid:
        return None
    for _ in range(60):
        time.sleep(POLL_GAP)
        st = call(lambda: get(f"/job/{jid}", key), f"{name} 조회", tries=10)
        if st is None:
            continue
        if st.get("status") == "Succeed":
            urls = st.get("image_urls") or []
            if not urls:
                return None

            def fetch():
                req = urllib.request.Request(urls[0], headers={"User-Agent": UA})
                with urllib.request.urlopen(req, timeout=180) as r:
                    return r.read()
            return call(fetch, f"{name} 다운로드", tries=10)
        if st.get("status") in ("Failed", "Error"):
            return None
    return None


def main():
    what = sys.argv[1] if len(sys.argv) > 1 else "bg"
    if what == "bg":
        items, out_dir, size = bg_items(), OUT_BG, BG_SIZE
    elif what == "moncard":
        items, out_dir, size = mon_items(), OUT_MON, MON_SIZE
    else:
        raise SystemExit("사용법: python _tools/aether_unique.py [bg|moncard]")

    if "--limit" in sys.argv:
        items = items[:int(sys.argv[sys.argv.index("--limit") + 1])]

    os.makedirs(out_dir, exist_ok=True)
    todo = [(n, p) for n, p in items if not os.path.exists(os.path.join(out_dir, n + ".png"))]
    print(f"[{what}] 전체 {len(items)} / 생성 대상 {len(todo)} — 동시 {WORKERS}", flush=True)

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
                postprocess(raw, os.path.join(out_dir, name + ".png"), size)
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

    print(f"\n완료 {done['ok']} / 실패 {done['fail']}", flush=True)


if __name__ == "__main__":
    main()
