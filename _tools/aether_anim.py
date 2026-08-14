# -*- coding: utf-8 -*-
"""AetherForge API로 몬스터 애니메이션 아틀라스(가로 4프레임 스트립)를 생성한다.

aether_icons.py 가 정적 아이콘 1장을 만든다면, 이쪽은 **한 줄에 4프레임**짜리
스프라이트 시트를 받아 4장으로 잘라 저장한다 (Idle / Move / Attack / Die).

프레임 정렬이 핵심이다.
  4칸을 각각 따로 크롭하면 프레임마다 캐릭터 크기·위치가 달라져 애니메이션이 덜덜 떨린다.
  → 4칸의 알파 bbox를 **합집합**으로 구해 같은 사각형으로 잘라낸다.
  → 바닥은 pivot(bottom-middle)에 맞춰 아래를 기준으로 정렬한다.

사용:
  python _tools/aether_anim.py --test            # 1종만 (품질 확인용)
  python _tools/aether_anim.py                   # 아키타입 30종 x 4상태
  python _tools/aether_anim.py --only slime      # 이름 부분일치로 일부만
이미 파일이 있으면 건너뛴다(중단 후 재실행 가능).
"""
import io
import os
import sys
import time

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from aether_icons import (BASE, MODEL, MONSTER_ARCHETYPES, SUBMIT_GAP, api_key,  # noqa: E402
                          get, post, UA)
import urllib.request  # noqa: E402

OUT = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Monster", "Anim")
FRAMES = 4
FRAME_H = 128           # 최종 프레임 높이(px). 폭은 비율 유지.

# 상태별 연출 지시. 4프레임 안에서 동작이 읽혀야 하므로 "무엇이 변하는지"를 명시한다.
STATES = {
    "Idle":   "standing still and breathing, body gently bobbing up and down",
    "Move":   "walking forward in a side-scrolling walk cycle, legs alternating",
    "Attack": "winding up and lunging forward in an attack, striking at the end",
    "Die":    "being defeated: staggering, collapsing, and lying dead on the last frame",
}

STYLE = ("16-bit pixel art game sprite sheet, {n} animation frames in a single horizontal row, "
         "evenly spaced, each frame the same size, character always facing right and "
         "the same scale and standing on the same ground line in every frame, "
         "chunky readable pixels, bold dark outline, flat plain solid background, "
         "no text, no numbers, no frame borders, no grid lines")


def build_prompt(base, motion):
    return (f"{STYLE.format(n=FRAMES)}. A fantasy RPG monster, {base}, side view. "
            f"Animation: {motion}.")


def bg_mask(im, tol=42):
    """네 모서리 색을 배경으로 보고 유사색 알파 0 처리. aether_icons와 같은 규칙."""
    im = im.convert("RGBA")
    px = im.load()
    w, h = im.size
    corners = [px[0, 0], px[w - 1, 0], px[0, h - 1], px[w - 1, h - 1]]
    bg = tuple(sum(c[i] for c in corners) // 4 for i in range(3))
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if abs(r - bg[0]) + abs(g - bg[1]) + abs(b - bg[2]) < tol * 3:
                px[x, y] = (r, g, b, 0)
    return im


def slice_strip(raw_bytes, out_dir, name):
    """가로 스트립 → 프레임 4장. 공통 bbox로 잘라 정렬을 유지한다."""
    im = bg_mask(Image.open(io.BytesIO(raw_bytes)))
    w, h = im.size
    cw = w // FRAMES
    cells = [im.crop((i * cw, 0, (i + 1) * cw, h)) for i in range(FRAMES)]

    # 각 셀의 내용 bbox 합집합 → 모든 프레임을 같은 창으로 잘라낸다(떨림 방지)
    boxes = [c.getbbox() for c in cells]
    boxes = [b for b in boxes if b]
    if not boxes:
        return 0
    L = min(b[0] for b in boxes)
    T = min(b[1] for b in boxes)
    R = max(b[2] for b in boxes)
    B = max(b[3] for b in boxes)

    scale = FRAME_H / float(B - T)
    fw = max(1, int(round((R - L) * scale)))
    n = 0
    for i, c in enumerate(cells):
        f = c.crop((L, T, R, B)).resize((fw, FRAME_H), Image.NEAREST)
        f.save(os.path.join(out_dir, f"{name}_{i}.png"))
        n += 1
    return n


def generate(name, prompt, key):
    job = post("/generate/image", {"prompt": prompt, "ai_model": MODEL}, key)
    jid = job.get("job_id")
    if not jid:
        print(f"  !! {name}: job_id 없음 {job}")
        return None
    for _ in range(90):
        time.sleep(4)
        st = get(f"/job/{jid}", key)
        if st.get("status") == "Succeed":
            urls = st.get("image_urls") or []
            if not urls:
                return None
            req = urllib.request.Request(urls[0], headers={"User-Agent": UA})
            with urllib.request.urlopen(req, timeout=180) as r:
                return r.read()
        if st.get("status") in ("Failed", "Error"):
            print(f"  !! {name}: {st}")
            return None
    print(f"  !! {name}: timeout")
    return None


def main():
    test = "--test" in sys.argv
    only = None
    if "--only" in sys.argv:
        only = sys.argv[sys.argv.index("--only") + 1].lower()

    items = []
    for region, bases in MONSTER_ARCHETYPES:
        for i, b in enumerate(bases):
            if only and only not in b.lower():
                continue
            items.append((f"Mon_{region}_{i}", b))
    if test:
        items = items[:1]

    key = api_key()
    os.makedirs(OUT, exist_ok=True)
    todo = []
    for name, base in items:
        for state, motion in STATES.items():
            d = os.path.join(OUT, name)
            if os.path.exists(os.path.join(d, f"{state}_{FRAMES - 1}.png")):
                continue
            todo.append((name, base, state, motion))

    print(f"생성 대상 {len(todo)}건 (아키타입 {len(items)}종)")
    for idx, (name, base, state, motion) in enumerate(todo, 1):
        d = os.path.join(OUT, name)
        os.makedirs(d, exist_ok=True)
        print(f"[{idx}/{len(todo)}] {name} {state} ...", flush=True)
        raw = generate(f"{name}/{state}", build_prompt(base, motion), key)
        if raw:
            n = slice_strip(raw, d, state)
            # 원본 스트립도 남긴다 — 자르기 결과가 이상할 때 눈으로 확인할 근거
            with open(os.path.join(d, f"_raw_{state}.png"), "wb") as f:
                f.write(raw)
            print(f"    -> {n} 프레임", flush=True)
        time.sleep(SUBMIT_GAP)


if __name__ == "__main__":
    main()
