# -*- coding: utf-8 -*-
"""몬스터/미니몬 도감 카드를 **실제 인게임 아트에서** 만든다.

## 왜 다시 만드나
  카드는 이름("심연의 슬라임")을 프롬프트로 넣어 503장을 따로 생성했었다. 그런데 인게임
  몬스터는 아키타입 30종의 애니메이션을 쓴다. 두 생성물이 서로 무관하니, 도감에서 본 그림과
  잡아서 데리고 다니는 미니몬이 **딴 생물**이었다.

## 어떻게
  카드를 그 몬스터가 실제로 쓰는 아키타입의 Idle 프레임에서 뽑는다 → 100% 일치한다.
  대신 그대로 쓰면 한 아키타입을 공유하는 20종이 전부 같은 그림이 되므로,
  이름의 **심도 수식어**를 색 변주로 옮긴다(생성 프롬프트가 쓰던 것과 같은 축).
    어린=밝고 선명 / 심연의=검푸르게 / 전설의=금빛 …

  ※ 이름까지 완전히 같은 짝이 200종 있다(어린 슬라임 x2 — 레벨대만 다른 같은 종).
    그건 원래 같은 생물이므로 같은 그림이 맞다.

사용: python _tools/gen_moncards.py         → Assets/@Resources/Sprite/Monster/Card/Mon_{id}.png
      python _tools/gen_moncards.py --dry
"""
import io
import json
import os
import sys

import numpy as np
from PIL import Image

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
JSON_DIR = os.path.join(BASE, "Assets", "@Resources", "Data", "JsonData")
ANIM = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Monster", "Anim")
CARD = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Monster", "Card")

SIZE = 96

# (밝기, 채도, RGB 곱) — 수식어가 곧 시각 변주. 생성 프롬프트가 쓰던 축과 같게 맞췄다.
GRADE = {
    "어린":      (1.18, 1.15, (1.00, 1.02, 0.98)),
    "거친":      (0.92, 0.75, (1.02, 0.98, 0.92)),
    "굶주린":    (0.82, 0.60, (0.98, 0.98, 1.00)),
    "성난":      (1.02, 1.20, (1.25, 0.85, 0.85)),
    "사나운":    (0.96, 1.25, (1.20, 0.95, 0.80)),
    "오래된":    (0.88, 0.55, (1.05, 1.00, 0.85)),
    "타락한":    (0.86, 1.05, (1.05, 0.80, 1.20)),
    "심연의":    (0.62, 0.80, (0.80, 0.85, 1.25)),
    "왕관을 쓴": (1.12, 1.10, (1.22, 1.10, 0.78)),
    "전설의":    (1.25, 1.20, (1.30, 1.15, 0.70)),
}


def load(name):
    with io.open(os.path.join(JSON_DIR, name), encoding="utf-8-sig") as f:
        d = json.load(f)
    return d[list(d.keys())[0]]


def grade(im, key):
    """수식어에 맞춰 색을 민다. 알파는 건드리지 않는다."""
    if key not in GRADE:
        return im
    bright, sat, mul = GRADE[key]
    a = np.array(im).astype(np.float32)
    rgb, alpha = a[:, :, :3], a[:, :, 3:]
    lum = rgb.mean(axis=2, keepdims=True)
    rgb = lum + (rgb - lum) * sat            # 채도
    rgb = rgb * bright * np.array(mul)       # 밝기 + 색조
    return Image.fromarray(
        np.concatenate([np.clip(rgb, 0, 255), alpha], axis=2).astype(np.uint8), "RGBA")


def fit(im, size=SIZE):
    box = im.getbbox()
    if box:
        im = im.crop(box)
    w, h = im.size
    k = size / float(max(w, h))
    im = im.resize((max(1, int(w * k)), max(1, int(h * k))), Image.NEAREST)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.paste(im, ((size - im.size[0]) // 2, (size - im.size[1]) // 2))
    return canvas


def main():
    dry = "--dry" in sys.argv
    creatures = {r["DataId"]: r for r in load("CreatureData.json")}
    loc = {r["TextKey"]: r["Kor"] for r in load("LocalizationData.json")}

    os.makedirs(CARD, exist_ok=True)
    src_cache = {}
    made = skipped = 0
    missing = set()

    for r in load("MinMonData.json"):
        arche = r.get("PetAnimLabel") or ""
        src = os.path.join(ANIM, arche, "Idle_0.png")
        if not os.path.exists(src):
            missing.add(arche)
            skipped += 1
            continue

        c = creatures.get(r["DataId"])
        kor = loc.get(c["Name"], "") if c else ""
        mod = next((m for m in GRADE if kor.startswith(m + " ")), "")

        key = (arche, mod)
        if key not in src_cache:
            src_cache[key] = fit(grade(Image.open(src).convert("RGBA"), mod))

        if not dry:
            src_cache[key].save(os.path.join(CARD, "Mon_%d.png" % r["DataId"]))
        made += 1

    print("%s 카드 %d장 (아키타입x수식어 조합 %d종) / 건너뜀 %d"
          % ("[dry]" if dry else "[적용]", made, len(src_cache), skipped))
    if missing:
        print("  아트 없는 아키타입:", sorted(missing))


if __name__ == "__main__":
    main()
