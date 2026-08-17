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
import concurrent.futures
import io
import os
import sys
import threading
import time

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from aether_icons import (BASE, MODEL, MONSTER_ARCHETYPES, SUBMIT_GAP, api_key,  # noqa: E402
                          get, post, UA)
import urllib.error    # noqa: E402
import urllib.request  # noqa: E402

OUT = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Monster", "Anim")
FRAMES = 4
FRAME_H = 128           # 최종 프레임 높이(px). 폭은 비율 유지.

# 동시 실행 수. 이미지 한 장 렌더에 2~4분씩 걸려 직렬로 돌리면 53건에 2.6시간이 든다.
#  제출은 SUBMIT_GAP으로 직렬화하고 **대기만 겹친다** — 429를 피하면서 전체 시간을 줄이는 지점.
WORKERS = 2   # 게이트가 실제 발사율을 정하므로 워커는 적게

# 상태별 연출 지시. 4프레임 안에서 동작이 읽혀야 하므로 "무엇이 변하는지"를 명시한다.
STATES = {
    "Idle":   "standing still and breathing, body gently bobbing up and down",
    "Move":   "walking forward in a side-scrolling walk cycle, legs alternating",
    "Attack": "winding up and lunging forward in an attack, striking at the end",
    "Die":    "being defeated: staggering, collapsing, and lying dead on the last frame",
    # 피격. 전용 아틀라스가 없던 시절엔 Idle 첫 프레임 한 장으로 때웠는데, 그러면 달려오던
    # 몬스터가 맞는 순간 **정지한 아이들 포즈로 굳어** 애니메이션이 고장난 것처럼 보였다.
    "OnHit":  "recoiling from a hit: head snapping back, body flinching backward, "
              "then starting to recover on the last frame",
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


def find_frame_spans(im):
    """투명 세로 여백으로 실제 프레임 x구간을 찾는다.

    ⚠️ 예전엔 이미지 폭을 그냥 4등분했다. 그런데 생성 결과는 캐릭터 4마리가 **가운데 일부
       구간에만** 몰려 있고 양옆에 큰 여백이 남는다(실측: 1536px 폭에 내용은 231~1259).
       4등분 경계(0/384/768/1152)와 실제 프레임(231-484, 513-744, 774-999, 1035-1259)이
       전부 어긋나 프레임이 반씩 잘리고 옆 프레임 조각이 섞여 들어갔다.
       → 애니메이션이 덜덜 떨리고 몸이 잘려 보이던 진짜 원인.
    """
    w, h = im.size
    alpha = im.split()[3]
    # 열마다 불투명 픽셀이 있는지
    occupied = [alpha.crop((x, 0, x + 1, h)).getextrema()[1] > 0 for x in range(w)]

    runs, start = [], None
    for x, on in enumerate(occupied):
        if on and start is None:
            start = x
        elif not on and start is not None:
            runs.append((start, x))
            start = None
    if start is not None:
        runs.append((start, w))

    # 아주 얇은 조각(노이즈)은 버린다
    runs = [r for r in runs if r[1] - r[0] >= max(4, w // 200)]
    if not runs:
        return []

    # 한 캐릭터가 내부 여백(벌린 다리 등)으로 쪼개졌으면 다시 붙인다.
    #  기준: 프레임 사이 진짜 간격보다 확실히 좁은 틈.
    if len(runs) > FRAMES:
        gaps = sorted(runs[i + 1][0] - runs[i][1] for i in range(len(runs) - 1))
        # 프레임이 FRAMES개면 진짜 경계 간격은 큰 쪽 FRAMES-1개다. 그보다 좁으면 같은 캐릭터.
        thresh = gaps[-(FRAMES - 1)] if len(gaps) >= FRAMES - 1 else 0
        merged = [list(runs[0])]
        for a, b in runs[1:]:
            if a - merged[-1][1] < thresh:
                merged[-1][1] = b
            else:
                merged.append([a, b])
        runs = [tuple(r) for r in merged]

    # 그래도 개수가 안 맞으면 내용 전체 구간을 균등 분할한다(전체 폭 분할보다 훨씬 낫다).
    if len(runs) != FRAMES:
        L, R = runs[0][0], runs[-1][1]
        step = (R - L) / float(FRAMES)
        runs = [(int(L + i * step), int(L + (i + 1) * step)) for i in range(FRAMES)]

    return runs


def slice_strip(raw_bytes_or_image, out_dir, name):
    """가로 스트립 → 프레임 4장. 프레임마다 실제 위치를 찾아 자르고, 같은 창 크기로 맞춘다."""
    im = raw_bytes_or_image
    if isinstance(im, (bytes, bytearray)):
        im = Image.open(io.BytesIO(im))
    im = bg_mask(im)

    spans = find_frame_spans(im)
    if not spans:
        return 0

    h = im.size[1]
    cells = [im.crop((a, 0, b, h)) for a, b in spans]

    # 프레임별 내용 bbox → 폭·높이는 최대값으로 통일하고 **아래를 맞춘다**(발끝 정렬).
    #  가로는 각자 중심을 맞춘다. 이렇게 해야 서 있는 위치가 흔들리지 않는다.
    boxes = []
    for c in cells:
        b = c.getbbox()
        boxes.append(b if b else (0, 0, c.size[0], h))

    fw_src = max(b[2] - b[0] for b in boxes)
    fh_src = max(b[3] - b[1] for b in boxes)
    if fw_src <= 0 or fh_src <= 0:
        return 0

    scale = FRAME_H / float(fh_src)
    out_w = max(1, int(round(fw_src * scale)))

    n = 0
    for i, (c, b) in enumerate(zip(cells, boxes)):
        content = c.crop(b)
        canvas = Image.new("RGBA", (fw_src, fh_src), (0, 0, 0, 0))
        cw, ch = content.size
        canvas.paste(content, ((fw_src - cw) // 2, fh_src - ch))   # 가로 중앙 / 세로 하단
        canvas.resize((out_w, FRAME_H), Image.NEAREST).save(
            os.path.join(out_dir, f"{name}_{i}.png"))
        n += 1
    return n


# 서버가 간헐적으로 504/502/429를 뱉는다. 재시도 없이 그냥 두면 **한 번의 오류가 전체 실행을 죽인다**
#  (실제로 62/116 지점에서 504 하나로 7시간짜리 작업이 통째로 멈춰 있었다).
RETRY_STATUS = (429, 500, 502, 503, 504)


def with_retry(fn, what, tries=6):
    """일시적 오류면 백오프 후 재시도. 끝내 실패하면 None."""
    delay = 5
    for i in range(tries):
        try:
            return fn()
        except urllib.error.HTTPError as e:
            if e.code not in RETRY_STATUS:
                print(f"  !! {what}: HTTP {e.code} (재시도 안 함)", flush=True)
                return None
            print(f"  .. {what}: HTTP {e.code} — {delay}s 후 재시도 ({i+1}/{tries})", flush=True)
        except Exception as e:                       # URLError, 타임아웃, 끊긴 소켓 등
            print(f"  .. {what}: {type(e).__name__} — {delay}s 후 재시도 ({i+1}/{tries})", flush=True)
        time.sleep(delay)
        delay = min(delay * 2, 120)
    print(f"  !! {what}: 재시도 모두 실패", flush=True)
    return None


# 페이싱은 aether_unique의 공용 게이트를 그대로 쓴다.
#  항목별 백오프만으로는 정지가 풀리는 순간 워커들이 함께 쏴서 서로를 429로 밀어내고,
#  쿨다운이 최대치까지 올라 통째로 멈춘다(실제로 그렇게 242장에서 멎었다).
#  조회(GET)도 한도를 먹으므로 폴링 간격도 늘린다.
from aether_unique import call as gated_call, POLL_GAP  # noqa: E402


def generate(name, prompt, key):
    job = gated_call(lambda: post("/generate/image", {"prompt": prompt, "ai_model": MODEL}, key),
                     f"{name} 제출")
    if not job:
        return None
    jid = job.get("job_id")
    if not jid:
        print(f"  !! {name}: job_id 없음 {job}")
        return None

    for _ in range(60):
        time.sleep(POLL_GAP)
        st = gated_call(lambda: get(f"/job/{jid}", key), f"{name} 조회", tries=10)
        if st is None:
            continue                                  # 조회 실패는 다음 폴링에서 다시 본다
        if st.get("status") == "Succeed":
            urls = st.get("image_urls") or []
            if not urls:
                return None

            def fetch():
                req = urllib.request.Request(urls[0], headers={"User-Agent": UA})
                with urllib.request.urlopen(req, timeout=180) as r:
                    return r.read()
            return gated_call(fetch, f"{name} 다운로드", tries=10)
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

    print(f"생성 대상 {len(todo)}건 (아키타입 {len(items)}종) — 동시 {WORKERS}개", flush=True)

    done = {"ok": 0, "fail": 0}
    failed = []
    lock = threading.Lock()
    submit_lock = threading.Lock()

    def work(job):
        idx, name, base, state, motion = job
        d = os.path.join(OUT, name)
        os.makedirs(d, exist_ok=True)

        # 제출만 직렬화한다 — 한꺼번에 몰아넣으면 429가 난다(아이콘 생성 때 겪었다).
        #  렌더링 대기는 병렬로 겹쳐야 전체 시간이 준다.
        with submit_lock:
            time.sleep(SUBMIT_GAP)
            print(f"[{idx}/{len(todo)}] {name} {state} 제출", flush=True)

        # 한 건이 어떤 이유로 터지든 **다음 건으로 넘어간다**. 실패분은 재실행하면 다시 시도된다
        # (완성된 상태는 todo에서 빠지므로 이어받기가 공짜다).
        try:
            raw = generate(f"{name}/{state}", build_prompt(base, motion), key)
            if raw:
                n = slice_strip(raw, d, state)
                # 원본 스트립도 남긴다 — 자르기 결과가 이상할 때 눈으로 확인할 근거
                with open(os.path.join(d, f"_raw_{state}.png"), "wb") as f:
                    f.write(raw)
                with lock:
                    done["ok"] += 1
                    print(f"    -> {name} {state} {n}프레임  (완료 {done['ok']}/{len(todo)})", flush=True)
                return
            reason = "결과 없음"
        except Exception as e:
            reason = f"{type(e).__name__}: {e}"

        with lock:
            done["fail"] += 1
            failed.append(f"{name}/{state}")
            print(f"  !! {name}/{state} 건너뜀: {reason}", flush=True)

    jobs = [(i, n, b, s, m) for i, (n, b, s, m) in enumerate(todo, 1)]
    with concurrent.futures.ThreadPoolExecutor(max_workers=WORKERS) as pool:
        list(pool.map(work, jobs))

    print(f"\n완료 {done['ok']} / 실패 {done['fail']}", flush=True)
    if failed:
        print("실패분(재실행하면 다시 시도):", ", ".join(failed[:20]), flush=True)


if __name__ == "__main__":
    main()
