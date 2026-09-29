"""스토어 스크린샷 합성 — 게임 캡처(세로)를 스토어 규격에 맞춰 캡션과 함께 배치한다.

  입력: Documents/Store/Screenshots/raw/<ko|en>/<이름>.png  (게임뷰 1080x2340 세로 캡처, 게임 언어별)
  출력: Documents/Store/Screenshots/play_<언어>_<이름>.png   1080x2160 (구글 플레이: 긴 변 ≤ 짧은 변 x2)
        Documents/Store/Screenshots/steam_<언어>_<이름>.png  1920x1080 (스팀: 16:9)

  캡션은 CAPTIONS 에서 이름으로 찾는다(없으면 캡션 없이 배치).
  사용: python _tools/compose_store_screenshots.py
"""
import os
from PIL import Image, ImageDraw, ImageFilter, ImageFont

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RAW = os.path.join(BASE, "Documents", "Store", "Screenshots", "raw")
OUT = os.path.join(BASE, "Documents", "Store", "Screenshots")
FONT = os.path.join(BASE, "Assets", "@Resources", "Font", "UsingFont", "DOSSaemmul.ttf")
KEYART = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Branding", "TitleBG.png")
LOGO = {"ko": os.path.join(BASE, "Assets", "@Resources", "Sprite", "Branding", "TitleLogo_KO.png"),
        "en": os.path.join(BASE, "Assets", "@Resources", "Sprite", "Branding", "TitleLogo_EN.png")}

# 스토어에 올리지 않는 캡처 — 03_capture 는 포획 화면 연출(빨간 모자·"야생의 ~이 나타났다!")이
#  포켓몬스터와 닮아 IP 검토 전까지 대표 이미지에서 뺀다(원본은 raw 에 남겨 둔다).
EXCLUDE = {"03_capture"}

# 이름 → (한국어, 영어) 캡션
CAPTIONS = {
    "01_title":    ("귀여운 미니몬과 함께하는 모험", "An adventure with cute Minimons"),
    "02_battle":   ("콤보와 치명타로 몰아치는 실시간 전투", "Real-time battles with combos and crits"),
    "03_capture":  ("쓰러뜨린 마물을 붙잡아 동료로", "Capture defeated monsters as companions"),
    "04_bestiary": ("503종의 마물을 기록하는 도감", "A bestiary of 503 monsters"),
    "05_forge":    ("장비를 제작하고 +9까지 강화", "Craft gear and enhance it up to +9"),
    "06_skills":   ("기억력 안에서 능력을 골라 조합", "Mix and match abilities within your memory"),
    "07_travel":   ("여정을 떠나 새 지역을 개척", "Journey onward to unlock new regions"),
    "08_village":  ("마을에서 의뢰를 받고 성장", "Take on guild missions and grow"),
}

GOLD = (255, 214, 102)
SHADOW = (26, 18, 40)


def pixel_text(text, size, color=GOLD, outline=SHADOW, px=3):
    """픽셀 폰트를 작은 크기로 그린 뒤 정수배 확대 — 번짐 없는 도트 글씨 + 외곽선."""
    ft = ImageFont.truetype(FONT, size)
    l, t, r, b = ft.getbbox(text)
    w, h = r - l + 4, b - t + 4
    mask = Image.new("L", (w, h), 0)
    d = ImageDraw.Draw(mask)
    d.fontmode = "1"
    d.text((2 - l, 2 - t), text, font=ft, fill=255)
    big = mask.resize((w * px, h * px), Image.NEAREST)
    pad = px * 2
    out = Image.new("RGBA", (big.width + pad * 2, big.height + pad * 2), (0, 0, 0, 0))
    ol = Image.new("RGBA", big.size, outline + (255,))
    for dx in range(-pad, pad + 1, px):
        for dy in range(-pad, pad + 1, px):
            out.paste(ol, (pad + dx, pad + dy), big)
    out.paste(Image.new("RGBA", big.size, color + (255,)), (pad, pad), big)
    return out


def backdrop(size):
    """키 아트를 크게 흐려 깐 배경 — 캡처가 주인공이 되도록 어둡게."""
    art = Image.open(KEYART).convert("RGB")
    W, H = size
    scale = max(W / art.width, H / art.height)
    art = art.resize((int(art.width * scale) + 1, int(art.height * scale) + 1), Image.LANCZOS)
    x, y = (art.width - W) // 2, (art.height - H) // 2
    art = art.crop((x, y, x + W, y + H)).filter(ImageFilter.GaussianBlur(18))
    return Image.blend(art, Image.new("RGB", size, (14, 12, 24)), 0.55)


def framed(shot, height):
    """캡처를 높이에 맞춰 줄이고 테두리를 두른다."""
    w = round(shot.width * height / shot.height)
    img = shot.resize((w, height), Image.LANCZOS)
    border = 6
    f = Image.new("RGB", (w + border * 2, height + border * 2), (240, 200, 110))
    f.paste(Image.new("RGB", (w + 2, height + 2), SHADOW), (border - 1, border - 1))
    f.paste(img, (border, border))
    return f


def fit_caption(text, max_w):
    for size, px in ((16, 4), (16, 3), (14, 3), (12, 3), (12, 2)):
        cap = pixel_text(text, size, px=px)
        if cap.width <= max_w:
            return cap
    return cap


def play(shot, caption, lang):
    W, H = 1080, 2160
    canvas = backdrop((W, H))
    top = 260
    fr = framed(shot, H - top - 40)
    if fr.width > W - 40:
        fr = framed(shot.resize((W - 52, round(shot.height * (W - 52) / shot.width))), round((H - top - 40) * 0.98))
    canvas.paste(fr, ((W - fr.width) // 2, top))
    if caption:
        cap = fit_caption(caption, W - 60)
        canvas.paste(cap, ((W - cap.width) // 2, (top - cap.height) // 2), cap)
    return canvas


def steam(shot, caption, lang):
    W, H = 1920, 1080
    canvas = backdrop((W, H))
    fr = framed(shot, H - 80)
    x = W - fr.width - 140
    canvas.paste(fr, (x, 40))
    logo = Image.open(LOGO[lang]).convert("RGBA")
    lw = min(760, x - 160)
    logo = logo.resize((lw, round(logo.height * lw / logo.width)), Image.NEAREST)
    canvas.paste(logo, ((x - lw) // 2, 260), logo)
    if caption:
        cap = fit_caption(caption, x - 120)
        canvas.paste(cap, ((x - cap.width) // 2, 260 + logo.height + 80), cap)
    return canvas


def main():
    os.makedirs(OUT, exist_ok=True)
    for lang in ("ko", "en"):
        folder = os.path.join(RAW, lang)
        if not os.path.isdir(folder):
            continue
        for name in sorted(f[:-4] for f in os.listdir(folder) if f.endswith(".png")):
            if name in EXCLUDE:
                continue
            shot = Image.open(os.path.join(folder, name + ".png")).convert("RGB")
            cap = CAPTIONS.get(name, ("", ""))[0 if lang == "ko" else 1]
            play(shot, cap, lang).save(os.path.join(OUT, f"play_{lang}_{name}.png"))
            steam(shot, cap, lang).save(os.path.join(OUT, f"steam_{lang}_{name}.png"))
            print("composed", lang, name)


if __name__ == "__main__":
    main()
