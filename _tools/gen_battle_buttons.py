"""전투 입력 버튼 그림(ADR0033) — 방패(막기) 원형 버튼, AUTO 버튼 바탕. 도트 느낌이 나게 작은 캔버스에 직접 찍는다.

  python _tools/gen_battle_buttons.py        # Assets/@Resources/Sprite/UI/Battle/ 에 Btn_Guard.png, Btn_Auto.png 를 쓴다
  python _tools/gen_battle_buttons.py --preview <out.png>   # 4배 확대 미리보기

Unity 가져오기는 Point 필터·압축 없음(빌더 Tools/Remake V2/Build Battle Input 이 설정한다).
"""
import math
import os
import sys

from PIL import Image

OUT_DIR = os.path.join(os.path.dirname(__file__), '..', 'Assets', '@Resources', 'Sprite', 'UI', 'Battle')

OUTLINE = (18, 14, 12, 255)
BRONZE_D = (92, 62, 34, 255)
BRONZE = (156, 110, 58, 255)
BRONZE_L = (214, 170, 96, 255)
INNER_D = (30, 26, 38, 235)
INNER = (44, 38, 56, 235)
STEEL_D = (120, 128, 142, 255)
STEEL = (188, 196, 208, 255)
STEEL_L = (236, 240, 246, 255)
BLUE_D = (34, 70, 128, 255)
BLUE = (52, 104, 176, 255)
BLUE_L = (86, 146, 214, 255)
GOLD = (246, 206, 92, 255)


def guard_button(size=48):
    im = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    px = im.load()
    c = (size - 1) / 2.0
    r = size / 2.0 - 0.5
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - c, y - c)
            if d > r:
                continue
            if d > r - 1.2:
                px[x, y] = OUTLINE
            elif d > r - 4.2:
                # 청동 테 — 왼쪽 위가 밝다
                light = (x - c) * -0.7 + (y - c) * -0.7
                px[x, y] = BRONZE_L if light > 6 else BRONZE if light > -6 else BRONZE_D
            elif d > r - 5.2:
                px[x, y] = OUTLINE
            else:
                px[x, y] = INNER if (y - c) < 4 else INNER_D

    # 히터 방패: 위는 평평, 옆은 곧게 내려오다 아래 한 점으로 모인다
    top, straight, tip = 11, 25, 38
    half = 11
    def half_width(y):
        if y < top or y > tip:
            return -1
        if y <= straight:
            return half
        t = (y - straight) / (tip - straight)
        return half * math.cos(t * math.pi / 2)

    cx = size // 2
    for y in range(size):
        hw = half_width(y)
        if hw < 0:
            continue
        for x in range(size):
            dx = x + 0.5 - cx
            if abs(dx) > hw + 0.5:
                continue
            edge = hw + 0.5 - abs(dx)
            if edge < 1.0 or y == top:
                px[x, y] = OUTLINE
            elif edge < 2.6 or y <= top + 1:
                px[x, y] = STEEL_L if dx < 0 else STEEL if y < straight else STEEL_D
            else:
                px[x, y] = BLUE_L if dx < -1 else BLUE if dx < 1 else BLUE_D
    # 가운데 금색 줄과 가로 띠(문장)
    for y in range(top + 3, tip - 3):
        if half_width(y) > 3:
            px[cx - 1, y] = GOLD
            px[cx, y] = GOLD
    for x in range(cx - 7, cx + 7):
        px[x, 18] = GOLD
        px[x, 19] = GOLD
    # 반짝임
    for (x, y) in [(cx - 7, top + 3), (cx - 6, top + 3), (cx - 7, top + 4)]:
        px[x, y] = STEEL_L
    return im


def auto_button(w=48, h=20):
    im = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    px = im.load()
    rad = 5
    for y in range(h):
        for x in range(w):
            # 둥근 모서리 사각형
            qx = min(x, w - 1 - x)
            qy = min(y, h - 1 - y)
            if qx < rad and qy < rad and math.hypot(rad - qx - 0.5, rad - qy - 0.5) > rad:
                continue
            border = qx < 1 or qy < 1 or (qx < rad and qy < rad and math.hypot(rad - qx - 0.5, rad - qy - 0.5) > rad - 1.2)
            if border:
                px[x, y] = OUTLINE
            elif qx < 3 or qy < 3:
                px[x, y] = BRONZE_L if y < h / 2 else BRONZE_D
            else:
                px[x, y] = INNER if y < h / 2 else INNER_D
    return im


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    g, a = guard_button(), auto_button()
    if '--preview' in sys.argv:
        out = sys.argv[sys.argv.index('--preview') + 1]
        sheet = Image.new('RGBA', (g.width * 4 + a.width * 4 + 16, g.height * 4), (90, 120, 140, 255))
        sheet.alpha_composite(g.resize((g.width * 4, g.height * 4), Image.NEAREST), (0, 0))
        sheet.alpha_composite(a.resize((a.width * 4, a.height * 4), Image.NEAREST), (g.width * 4 + 16, 0))
        sheet.save(out)
        print(out)
        return
    g.save(os.path.join(OUT_DIR, 'Btn_Guard.png'))
    a.save(os.path.join(OUT_DIR, 'Btn_Auto.png'))
    print('wrote', OUT_DIR)


if __name__ == '__main__':
    main()
