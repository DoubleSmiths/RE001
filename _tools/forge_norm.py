# -*- coding: utf-8 -*-
"""우리 녹화 프레임을 원작 영상의 자(344x854)에 맞춰 얹는다.

두 프로젝트는 같은 캔버스(180x440 유닛)를 쓰지만 화면 해상도가 달라
1유닛이 원작에선 1.9111px, 우리에겐 5.1818px다. 그냥 리사이즈하면
레터박스 때문에 몇 %가 어긋나 "우리 UI가 작다" 같은 헛다리를 짚게 된다.
**캔버스 중앙을 기준으로** 환산해야 같은 자가 된다.

  python _tools/forge_norm.py --our 0181 --ref 190 --out cmp.png     # 나란히 + 겹침
  python _tools/forge_norm.py --our 0181 --norm-only --out n.png     # 정규화만
"""
import argparse
import os

import numpy as np
from PIL import Image

OUR_DIR = r'C:\stopresent\Unity\RE001\Screenshots\demo30'
REF_DIR = (r'C:\Users\Mycom\AppData\Local\Temp\claude\C--stopresent-Unity-RE001'
           r'\d1f53458-5ce3-4e20-bc6c-9d7a066c54a5\scratchpad\ref30')

REF_W, REF_H = 344, 854
REF_CX, REF_CY = 172.0, 427.0          # 원작 캔버스 중앙 (화면 정중앙)
OUR_W, OUR_H = 1080, 2640
OUR_CX, OUR_CY = 540.0, 1320.0         # 우리 캔버스 중앙
SCALE = 1.9111 / 5.1818                # 유닛당 픽셀 비 = 0.36883


def normalize(img):
    """우리 프레임 -> 원작과 같은 자의 344x854."""
    w = int(round(OUR_W * SCALE))
    h = int(round(OUR_H * SCALE))
    small = img.resize((w, h), Image.LANCZOS)
    ox = int(round(OUR_CX * SCALE - REF_CX))
    oy = int(round(OUR_CY * SCALE - REF_CY))
    out = Image.new('RGB', (REF_W, REF_H), (0, 0, 0))
    out.paste(small, (-ox, -oy))
    return out


def load_our(n):
    return Image.open(os.path.join(OUR_DIR, '%04d.png' % int(n))).convert('RGB')


def load_ref(n):
    return Image.open(os.path.join(REF_DIR, '%03d.png' % int(n))).convert('RGB')


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--our', required=True)
    ap.add_argument('--ref')
    ap.add_argument('--out', required=True)
    ap.add_argument('--norm-only', action='store_true')
    ap.add_argument('--crop', help='x,y,w,h (원작 좌표계)')
    ap.add_argument('--zoom', type=int, default=1)
    a = ap.parse_args()

    ours = normalize(load_our(a.our))

    if a.norm_only or not a.ref:
        img = ours
    else:
        ref = load_ref(a.ref)
        # 왼쪽 원작 / 가운데 우리 / 오른쪽 차이(절대값)
        d = np.abs(np.asarray(ref).astype(int) - np.asarray(ours).astype(int)).astype(np.uint8)
        img = Image.new('RGB', (REF_W * 3 + 8, REF_H), (40, 40, 40))
        img.paste(ref, (0, 0))
        img.paste(ours, (REF_W + 4, 0))
        img.paste(Image.fromarray(d), (REF_W * 2 + 8, 0))

    if a.crop:
        x, y, w, h = [int(v) for v in a.crop.split(',')]
        if not a.norm_only and a.ref:
            # 세 판 모두 같은 자리를 잘라 세로로 쌓는다
            parts = [img.crop((x + i * (REF_W + 4) + (4 if i == 2 else 0), y,
                               x + i * (REF_W + 4) + (4 if i == 2 else 0) + w, y + h))
                     for i in range(3)]
            img = Image.new('RGB', (w, h * 3 + 8), (40, 40, 40))
            for i, p in enumerate(parts):
                img.paste(p, (0, i * (h + 4)))
        else:
            img = img.crop((x, y, x + w, y + h))

    if a.zoom > 1:
        img = img.resize((img.width * a.zoom, img.height * a.zoom), Image.NEAREST)

    img.save(a.out)
    print(a.out, img.size)


if __name__ == '__main__':
    main()
