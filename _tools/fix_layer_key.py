# -*- coding: utf-8 -*-
"""배경 겹(중경/근경)에 남은 자홍 크로마키를 마저 지운다.

## 왜
  생성 프롬프트가 "flat pure magenta background"를 요구하고, 받은 그림을 테두리에서
  번져 들어가는 flood fill로 지웠다. 그런데 flood fill은 **바깥과 이어진 곳만** 지운다 —
  나무 사이나 캐노피 아래처럼 사방이 막힌 자홍 웅덩이는 그대로 남는다.
  인게임 배경에 자홍 얼룩이 그대로 보였다.

  몸 사진과 달리 여기서는 배경색이 **의도적으로 고른 크로마키**다. 그림 안에 순수 자홍이
  나올 일이 없으니, 연결성을 따질 것 없이 색으로 전부 지우는 편이 맞다.

사용: python _tools/fix_layer_key.py [--dry]
"""
import os
import sys

import numpy as np
from PIL import Image

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DIR = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Background", "RegionLayer")


def is_key(rgb):
    """자홍(R·B 높고 G 낮음)만 고른다. 초록·갈색·회색 지형은 걸리지 않는다."""
    r = rgb[:, :, 0].astype(np.int32)
    g = rgb[:, :, 1].astype(np.int32)
    b = rgb[:, :, 2].astype(np.int32)
    return (r > 130) & (b > 130) & (g < 120) & (np.abs(r - b) < 90)


def main():
    dry = "--dry" in sys.argv
    total = touched = 0
    for name in sorted(os.listdir(DIR)):
        if not name.endswith(".png") or "_Far_" in name:
            continue                        # 원경은 불투명 한 장 그대로 쓴다
        total += 1
        path = os.path.join(DIR, name)
        a = np.array(Image.open(path).convert("RGBA"))
        m = is_key(a[:, :, :3]) & (a[:, :, 3] > 0)
        if not m.any():
            continue
        touched += 1
        pct = m.mean() * 100
        if dry:
            print("%-30s 남은 키 %.1f%%" % (name, pct))
            continue
        a[m, 3] = 0

        # 키를 지우고 나면 아래위에 투명 여백이 생긴다. 겹은 **바닥을 맞춰** 배치하므로
        #  여백을 남기면 나무가 지면에서 떠 보인다(실제로 떴다) → 내용에 맞게 다시 자른다.
        im = Image.fromarray(a, "RGBA")
        box = im.getbbox()
        if box:
            im = im.crop((0, box[1], im.size[0], box[3]))
        im.save(path)

    print("%s 겹 %d장 중 %d장에서 자홍 잔여 제거" % ("[dry]" if dry else "[적용]", total, touched))


if __name__ == "__main__":
    main()
