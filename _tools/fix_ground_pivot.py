# -*- coding: utf-8 -*-
"""캐릭터 스프라이트의 pivot을 "보이는 발밑"으로 맞춘다 (.meta 직접 수정).

왜 필요한가
  pivot이 셀의 bottom-center면 transform.y = **셀 바닥**이지 발바닥이 아니다.
  아트마다 셀 아래 투명 여백이 제각각이라(플레이어 10px, 레거시 슬라임 48px, 생성 몬스터 23px)
  전부 같은 y에 세워도 누구는 뜨고 누구는 파묻힌다.
  → pivot이 여백을 흡수하면 Define.GROUND_Y **하나로** 모두 정렬된다.

떨림 방지
  여백은 시트 전체에서 **가장 낮은 값(공통 바닥)** 을 쓴다. 셀마다 따로 잡으면 프레임마다
  pivot이 달라져 애니메이션이 위아래로 떤다. 프레임 간 높이 차이는 점프·호흡 같은
  의도된 움직임이므로 그대로 둬야 한다.

사용:  python _tools/fix_ground_pivot.py [--dry]
"""
import io
import os
import re
import sys

from PIL import Image

BASE = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
SPR = os.path.join(BASE, "Assets", "@Resources", "Sprite")

# 지면에 서는 캐릭터만. UI 아이콘·이펙트는 건드리지 않는다(pivot이 바뀌면 배치가 다 틀어진다).
TARGET_DIRS = [
    os.path.join(SPR, "Player"),
    os.path.join(SPR, "Monster", "CH1"),
    os.path.join(SPR, "Monster", "Anim"),
    os.path.join(SPR, "Monster", "Generated"),
]
# NPC는 제외한다 — 마을에 손으로 배치된 오브젝트라 pivot을 바꾸면 배치가 통째로 어긋난다.
# 지금 떠 보이거나 파묻힌다는 보고도 없다. 문제가 생기면 그때 같은 방식으로 포함하면 된다.

ALIGN_CUSTOM = 9


# 배경을 지운 AI 생성 이미지에는 캐릭터 아래로 반투명 잔여 픽셀·점 몇 개가 남는다.
# 그걸 "발밑"으로 잡으면 pivot이 실제보다 낮아져 캐릭터가 공중에 뜬다.
# → 어느 정도 불투명하고(ALPHA_MIN) 그런 픽셀이 한 줄에 충분히 모여 있어야(ROW_MIN_RATIO) 내용으로 친다.
ALPHA_MIN = 40
ROW_MIN_RATIO = 0.03


def bottom_pad(img, box):
    """box=(l,t,r,b) PIL 좌표. 아래에서부터 '내용 없음'인 행 수."""
    cell = img.crop(box)
    w, h = cell.size
    px = cell.load()
    need = max(2, int(w * ROW_MIN_RATIO))
    pad = 0
    for row in range(h - 1, -1, -1):
        solid = 0
        for x in range(w):
            if px[x, row][3] >= ALPHA_MIN:
                solid += 1
                if solid >= need:
                    break
        if solid >= need:
            break
        pad += 1
    return pad if pad < h else None      # 전부 투명한 셀은 무시


def parse_rects(meta_text):
    """sprites: 블록의 각 항목에서 rect 를 뽑는다. 순서 = 블록 등장 순서."""
    rects = []
    for m in re.finditer(
        r"rect:\s*\n\s*serializedVersion: 2\s*\n\s*x:\s*([-\d.]+)\s*\n\s*y:\s*([-\d.]+)\s*\n"
        r"\s*width:\s*([-\d.]+)\s*\n\s*height:\s*([-\d.]+)", meta_text):
        rects.append(tuple(float(g) for g in m.groups()))
    return rects


def compute_ratio(png_path, meta_text):
    img = Image.open(png_path).convert("RGBA")
    W, H = img.size

    # spriteMode 1(Single)이면 Unity는 **텍스처 전체**를 쓰고 sprites: 목록은 무시한다.
    #  Single로 바꾼 뒤에도 예전 슬라이스 rect가 meta에 남아 있는 파일이 있어서,
    #  그걸 기준으로 계산하면 엉뚱한 pivot이 나온다(실제로 그래서 몬스터가 떠 있었다).
    m = re.search(r"\n\s*spriteMode:\s*(\d+)", meta_text)
    multi = bool(m) and m.group(1) == "2"

    rects = parse_rects(meta_text) if multi else []
    if not rects:
        rects = [(0.0, 0.0, float(W), float(H))]

    best = None
    for (x, y, w, h) in rects:
        if w <= 0 or h <= 0:
            continue
        # Unity rect.y 는 텍스처 **아래**에서부터 → PIL 좌표로 뒤집는다
        top = H - y - h
        box = (int(x), int(top), int(x + w), int(top + h))
        if box[0] < 0 or box[1] < 0 or box[2] > W or box[3] > H:
            continue
        pad = bottom_pad(img, box)
        if pad is None:
            continue
        r = pad / h
        best = r if best is None else min(best, r)
    return best


def apply(meta_text, ratio):
    """alignment/pivot 을 Custom + (0.5, ratio) 로. 이미 그 값이면 그대로."""
    py = ("%.6f" % ratio).rstrip("0").rstrip(".") or "0"
    out = re.sub(r"(\n\s*)alignment: \d+", r"\g<1>alignment: %d" % ALIGN_CUSTOM, meta_text)
    out = re.sub(r"(\n\s*)pivot: \{x: [-\d.]+, y: [-\d.]+\}",
                 r"\g<1>pivot: {x: 0.5, y: %s}" % py, out)
    out = re.sub(r"(\n\s*)spritePivot: \{x: [-\d.]+, y: [-\d.]+\}",
                 r"\g<1>spritePivot: {x: 0.5, y: %s}" % py, out)
    return out


def main():
    dry = "--dry" in sys.argv
    changed = skipped = 0
    for d in TARGET_DIRS:
        if not os.path.isdir(d):
            continue
        for root, _, files in os.walk(d):
            for fn in files:
                if not fn.lower().endswith(".png"):
                    continue
                png = os.path.join(root, fn)
                meta = png + ".meta"
                if not os.path.exists(meta):
                    skipped += 1
                    continue
                text = io.open(meta, encoding="utf-8").read()
                try:
                    ratio = compute_ratio(png, text)
                except Exception as e:
                    print("  !! %s: %s" % (fn, e))
                    skipped += 1
                    continue
                if ratio is None:
                    skipped += 1
                    continue
                new = apply(text, ratio)
                if new == text:
                    skipped += 1
                    continue
                rel = os.path.relpath(png, BASE).replace("\\", "/")
                print("  %-70s pivotY=%.4f" % (rel, ratio))
                if not dry:
                    io.open(meta, "w", encoding="utf-8", newline="\n").write(new)
                changed += 1
    print("\n수정 %d / 건너뜀 %d%s" % (changed, skipped, "  (dry-run)" if dry else ""))


if __name__ == "__main__":
    main()
