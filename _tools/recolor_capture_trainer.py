"""포획 화면 뒷모습 스프라이트(Sprite/Player/unnamed.png)의 모자 색을 바꾼다.

  빨간 모자 + 흰 앞판은 포켓몬스터 주인공(레드)의 상징적인 모자와 닮아 상업 출시 IP 위험이 있다
  (같은 화면의 원곡 추출 BGM/효과음은 0b3d162a 에서 교체). 포즈·실루엣은 그대로 두고 색만 바꾼다:
    빨강 → 올리브 녹색(명도 유지), 파란 문양 → 금색. 흰 앞판·머리카락은 건드리지 않는다.

  사용: python _tools/recolor_capture_trainer.py          # 미리보기 PNG 만 (스크래치)
        python _tools/recolor_capture_trainer.py --write  # 원본 덮어쓰기(되돌리기: git checkout)
"""
import colorsys
import sys

from PIL import Image

SRC = "Assets/@Resources/Sprite/Player/unnamed.png"
CAP_MAX_Y = 33          # 모자는 위쪽 띠에만 있다(아래쪽 갈색 조끼·장화의 붉은 기는 제외)
OLIVE_HUE = 75 / 360    # 올리브
GOLD_HUE = 44 / 360


def is_red(r, g, b):
    # 모자 빨강(201,49,49)·진홍 외곽(127,20,50)만 — 얼굴 음영(205,120,109 등)은 채도가 낮아 제외된다
    return r > 110 and g < r * 0.45 and b < r * 0.45


def is_blue(r, g, b):
    return b > 110 and b > r * 1.3 and b > g * 1.05


def shift(r, g, b, hue, sat_scale):
    h, l, s = colorsys.rgb_to_hls(r / 255, g / 255, b / 255)
    nr, ng, nb = colorsys.hls_to_rgb(hue, l, min(1.0, s * sat_scale))
    return round(nr * 255), round(ng * 255), round(nb * 255)


def main(write):
    im = Image.open(SRC).convert("RGBA")
    px = im.load()
    red = blue = 0
    for y in range(min(CAP_MAX_Y + 1, im.height)):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a < 16:
                continue
            if is_red(r, g, b):
                px[x, y] = shift(r, g, b, OLIVE_HUE, 0.55) + (a,)
                red += 1
            elif is_blue(r, g, b):
                px[x, y] = shift(r, g, b, GOLD_HUE, 1.0) + (a,)
                blue += 1
    print(f"recolored red={red} blue={blue}")
    if write:
        im.save(SRC)
        print("saved", SRC)
    else:
        out = sys.argv[sys.argv.index("--preview") + 1] if "--preview" in sys.argv else "trainer_preview.png"
        im.resize((im.width * 4, im.height * 4), Image.NEAREST).save(out)
        print("preview", out)


if __name__ == "__main__":
    main("--write" in sys.argv)
