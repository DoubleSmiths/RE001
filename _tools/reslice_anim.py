# -*- coding: utf-8 -*-
"""저장해 둔 원본 아틀라스(_raw_*.png)로 프레임을 다시 자른다.

왜 필요했나 — 예전 자르기는 이미지 폭을 그냥 FRAMES등분했다. 그런데 생성 결과는 캐릭터가
**가운데 일부 구간에만** 몰려 있고 양옆에 큰 여백이 남는다.

  실측(Mon_BreezingWindForest_0 / Move, 1536x1024)
    실제 프레임 : 231-484 · 513-744 · 774-999 · 1035-1259
    4등분 경계  : 0-384   · 384-768 · 768-1152 · 1152-1536

전부 어긋나 프레임이 반씩 잘리고 옆 프레임 조각이 섞였다. 애니메이션이 덜덜 떨리고
몸이 잘려 보이던 진짜 원인이다.

원본을 그때그때 남겨 뒀으므로 **API 재생성 없이** 다시 자르기만 하면 된다.

사용: python _tools/reslice_anim.py [--dry]
"""
import os
import sys

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from aether_anim import OUT, FRAMES, bg_mask, find_frame_spans, slice_strip  # noqa: E402


def main():
    dry = "--dry" in sys.argv
    if not os.path.isdir(OUT):
        raise SystemExit(f"원본 폴더 없음: {OUT}")

    total = fixed = skipped = 0
    for name in sorted(os.listdir(OUT)):
        d = os.path.join(OUT, name)
        if not os.path.isdir(d):
            continue
        for f in sorted(os.listdir(d)):
            if not f.startswith("_raw_") or not f.endswith(".png"):
                continue
            state = f[len("_raw_"):-len(".png")]
            raw = os.path.join(d, f)
            total += 1

            im = bg_mask(Image.open(raw))
            spans = find_frame_spans(im)
            if len(spans) != FRAMES:
                print(f"  ?? {name}/{state}: 프레임 {len(spans)}개만 검출 — 건너뜀", flush=True)
                skipped += 1
                continue

            if dry:
                w = im.size[0]
                old = [(i * (w // FRAMES), (i + 1) * (w // FRAMES)) for i in range(FRAMES)]
                if spans != old:
                    print(f"  {name}/{state}: 실제={spans} 옛경계={old}", flush=True)
                    fixed += 1
                continue

            n = slice_strip(Image.open(raw), d, state)
            if n == FRAMES:
                fixed += 1
            else:
                skipped += 1

    print(f"\n원본 {total}건 / {'차이 있는' if dry else '다시 자른'} {fixed}건 / 건너뜀 {skipped}건")


if __name__ == "__main__":
    main()
