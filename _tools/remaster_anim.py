# -*- coding: utf-8 -*-
"""몬스터 애니 프레임을 _raw_*.png에서 다시 뽑는다 — 배경 제거·크기·정렬을 한 번에.

API 재생성 없이 저장해 둔 원본 스트립만으로 돌아간다.

## 고친 것 세 가지

1) **몸이 뚫려 보이던 문제**
   기존 `bg_mask`는 네 모서리 평균색과 비슷한 픽셀을 **이미지 전체에서** 지웠다.
   회색 가고일이 푸른 배경 위에 있으면 몸통 회색까지 배경으로 판정돼 몸에 구멍이 뚫린다.
   → 테두리에서 시작하는 **flood fill**로 바꿨다. 배경은 바깥과 이어져 있고 몸 안쪽은 아니므로
     같은 색이어도 몸은 살아남는다. 연결성이 보장하니 임계값도 조일 수 있다(42 → 22).

2) **상태마다 몬스터 크기가 달라지던 문제**
   기존 `slice_strip`은 상태별 내용 높이의 **최댓값**을 FRAME_H에 맞췄다. 그래서 한 프레임에만
   섬광 이펙트가 붙어도 그 상태 전체가 축소돼 몬스터가 유독 작아 보였다(실측 최대 4.5배).
   → 기준을 **중앙값**으로 바꿨다. 이펙트가 낀 프레임 하나는 중앙값을 밀지 못한다.
   상태별 정규화 자체는 남긴다 — 생성 그림의 확대 배율이 상태마다 실제로 다르기 때문이다
   (실측: 어떤 몬스터는 Idle 155px인데 OnHit이 4프레임 모두 880px이었다).

3) **상태마다 캔버스가 달라 발 위치가 튀던 문제**
   → 한 몬스터의 모든 상태·프레임을 **같은 캔버스**에 가로 중앙·세로 하단으로 올린다.

사용:
    python _tools/remaster_anim.py            # 전체 적용
    python _tools/remaster_anim.py --dry      # 바뀔 내용만 출력
    python _tools/remaster_anim.py --only Mon_RuinedSanctum_4
"""
import os
import sys
from collections import deque
from statistics import median

from PIL import Image

try:                                  # 있으면 연결 성분 라벨링으로 빠르게 — 없으면 순수 파이썬
    import cv2
    import numpy as np
except ImportError:
    cv2 = np = None

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ANIM = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Monster", "Anim")

FRAMES = 4
FRAME_H = 128            # Idle 기준 내용 높이(px)
STATES = ["Idle", "Move", "Attack", "OnHit", "Die"]
BG_TOL = 22              # flood fill 임계(채널합 기준 tol*3)
# 캔버스 상한(FRAME_H 배수). 캔버스는 투명 여백만 늘릴 뿐 몬스터가 화면에 그려지는 크기는
# 배율 k가 정하므로, 넉넉히 잡아 타격 이펙트가 잘리지 않게 둔다(피벗이 BottomCenter라 발끝은 그대로).
CANVAS_CAP = 5.0


def flood_mask(im, tol=BG_TOL):
    """테두리에서 번져 들어간 배경만 지운다. 몸 안쪽은 배경색과 같아도 남는다."""
    if np is not None and cv2 is not None:
        return _flood_mask_fast(im, tol)
    return _flood_mask_slow(im, tol)


def _flood_mask_fast(im, tol, local=10):
    """테두리에서 **이웃 픽셀 차이를 따라** 번져 들어간 영역만 배경으로 본다.

    고정 색 기준(모서리 평균 ± tol)만 쓰면 비네트 그라데이션이 있는 그림에서 바깥쪽 옅은 띠가
    살아남는다. 그 띠는 눈에 거의 안 보이는데 bbox는 크게 잡혀서, 그 상태만 크게 축소돼
    **몬스터가 유독 작게** 보였다(실측: 196px짜리 거미가 880px로 잡혔다).
    → cv2.floodFill을 고정 범위(FIXED_RANGE) 없이 쓰면 이웃과의 차이로 번지므로
      완만한 그라데이션은 끝까지 따라가고, 몬스터 외곽의 뚜렷한 경계에서 멈춘다.
    """
    im = im.convert("RGBA")
    a = np.array(im)
    rgb = np.ascontiguousarray(a[:, :, :3])
    h, w = rgb.shape[:2]

    mask = np.zeros((h + 2, w + 2), np.uint8)
    flags = 4 | (255 << 8) | cv2.FLOODFILL_MASK_ONLY
    d = (local,) * 3
    step = max(1, min(h, w) // 32)
    seeds = ([(x, 0) for x in range(0, w, step)] + [(x, h - 1) for x in range(0, w, step)]
             + [(0, y) for y in range(0, h, step)] + [(w - 1, y) for y in range(0, h, step)])
    for sx, sy in seeds:
        if mask[sy + 1, sx + 1]:
            continue
        cv2.floodFill(rgb, mask, (sx, sy), 0, d, d, flags)

    bgm = mask[1:-1, 1:-1].astype(bool)

    # 그라데이션을 따라가다 보면 몬스터 안쪽까지 새는 경우가 있다 —
    # 모서리 색과 확연히 다른 픽셀은 되살린다(몸은 배경색과 크게 다르다).
    corners = [a[0, 0, :3], a[0, -1, :3], a[-1, 0, :3], a[-1, -1, :3]]
    bg = np.mean(np.stack(corners), axis=0)
    far = np.abs(a[:, :, :3].astype(np.int32) - bg.astype(np.int32)).sum(axis=2) > tol * 9
    bgm &= ~far

    a[bgm, 3] = 0
    return Image.fromarray(a, "RGBA")


def _flood_mask_slow(im, tol):
    im = im.convert("RGBA")
    w, h = im.size
    px = im.load()
    corners = [px[0, 0], px[w - 1, 0], px[0, h - 1], px[w - 1, h - 1]]
    bg = tuple(sum(c[i] for c in corners) // 4 for i in range(3))
    lim = tol * 3

    def near(x, y):
        r, g, b, _ = px[x, y]
        return abs(r - bg[0]) + abs(g - bg[1]) + abs(b - bg[2]) < lim

    seen = bytearray(w * h)
    q = deque()
    for x in range(w):
        for y in (0, h - 1):
            i = y * w + x
            if not seen[i] and near(x, y):
                seen[i] = 1
                q.append((x, y))
    for y in range(h):
        for x in (0, w - 1):
            i = y * w + x
            if not seen[i] and near(x, y):
                seen[i] = 1
                q.append((x, y))

    while q:
        x, y = q.popleft()
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if 0 <= nx < w and 0 <= ny < h:
                i = ny * w + nx
                if not seen[i] and near(nx, ny):
                    seen[i] = 1
                    q.append((nx, ny))

    for y in range(h):
        for x in range(w):
            if seen[y * w + x]:
                r, g, b, _ = px[x, y]
                px[x, y] = (r, g, b, 0)
    return im


def find_frame_spans(im):
    """투명 세로 여백으로 실제 프레임 x구간을 찾는다(폭 균등분할이 아니다)."""
    w, h = im.size
    alpha = im.split()[3]
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

    runs = [r for r in runs if r[1] - r[0] >= max(4, w // 200)]
    if not runs:
        return []

    # 한 캐릭터가 내부 여백(벌린 다리 등)으로 쪼개졌으면 다시 붙인다.
    if len(runs) > FRAMES:
        gaps = sorted(runs[i + 1][0] - runs[i][1] for i in range(len(runs) - 1))
        thresh = gaps[-(FRAMES - 1)] if len(gaps) >= FRAMES - 1 else 0
        merged = [list(runs[0])]
        for a, b in runs[1:]:
            if a - merged[-1][1] < thresh:
                merged[-1][1] = b
            else:
                merged.append([a, b])
        runs = [tuple(r) for r in merged]

    # 그래도 개수가 안 맞으면 **내용 구간**을 균등 분할한다(전체 폭 분할보다 훨씬 낫다).
    if len(runs) != FRAMES:
        L, R = runs[0][0], runs[-1][1]
        step = (R - L) / float(FRAMES)
        runs = [(int(L + i * step), int(L + (i + 1) * step)) for i in range(FRAMES)]

    return runs


def cut_frames(path):
    """원본 스트립 → 내용만 남긴 프레임 4장(아직 배율·정렬 전)."""
    im = flood_mask(Image.open(path))
    spans = find_frame_spans(im)
    if not spans:
        return []
    h = im.size[1]
    out = []
    for a, b in spans:
        cell = im.crop((a, 0, b, h))
        bb = cell.getbbox()
        out.append(cell.crop(bb) if bb else None)
    return [c for c in out if c is not None]


def remaster(mon_dir, dry=False):
    cuts = {}
    for st in STATES:
        raw = os.path.join(mon_dir, "_raw_%s.png" % st)
        if not os.path.exists(raw):
            continue
        fr = cut_frames(raw)
        if len(fr) == FRAMES:
            cuts[st] = fr

    if "Idle" not in cuts:
        return None, "Idle 없음"

    # ── 배율은 **몬스터당 하나**. 상태마다 맞추면 그 상태만 커지거나 작아진다. ──
    #  기준은 이펙트가 끼지 않는 상태들의 프레임 높이 중앙값.
    #    · OnHit — 타격 섬광이 붙어 bbox가 3~4배까지 뛴다
    #    · Die   — 쓰러지는 자세라 뒤 프레임이 원래 낮다
    ref_states = [s for s in ("Idle", "Move", "Attack") if s in cuts]
    ref = median([f.size[1] for s in ref_states for f in cuts[s]])
    if ref <= 0:
        return None, "기준 높이 0"
    k = FRAME_H / float(ref)

    scaled = {}
    for st, frames in cuts.items():
        scaled[st] = [
            f.resize((max(1, int(round(f.size[0] * k))),
                      max(1, int(round(f.size[1] * k)))), Image.NEAREST)
            for f in frames
        ]

    # ── 캔버스는 몬스터당 하나 — 상태가 바뀌어도 발 위치와 화면 크기가 튀지 않게. ──
    cap = int(FRAME_H * CANVAS_CAP)
    ch = min(cap, max(f.size[1] for fs in scaled.values() for f in fs))
    cw = min(cap, max(f.size[0] for fs in scaled.values() for f in fs))

    if dry:
        rel = {st: round(max(f.size[1] for f in fs) / float(FRAME_H), 2)
               for st, fs in scaled.items()}
        return (cw, ch, rel), None

    for st, fs in scaled.items():
        for i, f in enumerate(fs):
            canvas = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
            w, h = f.size
            # 캔버스를 넘치면 **잘라낸다**. 줄이면 그 프레임만 몬스터가 작아지므로,
            # 발끝(하단)과 가로 중앙을 지키고 넘치는 이펙트 쪽만 버린다.
            if w > cw:
                f = f.crop(((w - cw) // 2, 0, (w - cw) // 2 + cw, h))
                w = cw
            if h > ch:
                f = f.crop((0, h - ch, w, h))
                h = ch
            canvas.paste(f, ((cw - w) // 2, ch - h))   # 가로 중앙 / 세로 하단(발끝 정렬)
            canvas.save(os.path.join(mon_dir, "%s_%d.png" % (st, i)))

    return (cw, ch, None), None


def main():
    dry = "--dry" in sys.argv
    only = None
    if "--only" in sys.argv:
        only = sys.argv[sys.argv.index("--only") + 1]

    dirs = sorted(d for d in os.listdir(ANIM) if os.path.isdir(os.path.join(ANIM, d)))
    if only:
        dirs = [d for d in dirs if d == only]

    done = skipped = 0
    for d in dirs:
        res, err = remaster(os.path.join(ANIM, d), dry)
        if err:
            print("  !! %s: %s" % (d, err))
            skipped += 1
            continue
        done += 1
        if dry or only:
            print("%-28s 캔버스=%dx%d %s" % (d, res[0], res[1], res[2] or ""))
    print("\n%s  처리 %d건 / 건너뜀 %d건" % ("[dry]" if dry else "[적용]", done, skipped))


if __name__ == "__main__":
    main()
