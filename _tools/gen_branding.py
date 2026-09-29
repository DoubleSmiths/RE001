# -*- coding: utf-8 -*-
"""브랜딩 아트 생성기 — 앱 아이콘 / 타이틀 로고 / 타이틀 배경 / 스토어(Google Play · Steam) 그래픽.

## 왜
  게임에 앱 아이콘도 로고도 없고 타이틀 화면이 회색 자리표시자였다(Release_Readiness 6번).
  스토어 첫인상 자산을 **기존 게임 아트만으로 재현 가능하게** 만든다 — 손으로 그린 PSD 가 아니라
  이 스크립트가 원천이라, 캐릭터/로고 이름이 바뀌어도 다시 돌리면 된다.

## 원칙
  - 픽셀아트: 모든 확대는 NEAREST + 정수 배율. 장면은 "유닛(U)" 해상도로 그린 뒤 마지막에 U배로 키운다.
    타이틀 배경은 U=4px 로 배경 x1 · 영웅 x2 · 미니몬 x1 (게임 화면도 배경보다 캐릭터 픽셀이 굵다).
    ※ 축소(스팀 커뮤니티 아이콘 등 정수비가 아닌 것)만 LANCZOS, 나머지 축소는 BOX 로 도트를 보존한다.
  - 소스는 전부 기존 에셋: 영웅 Player_01 시트, 미니몬 Monster/Card, 배경 Background/RegionLayer(21000),
    글꼴 Font/UsingFont/DOSSaemmul.ttf. AI 생성 API 는 쓰지 않는다.
  - 결정론: 난수는 전부 시드 고정. 두 번 돌려도 같은 파일이 나온다.

## 출력
  Assets/@Resources/Sprite/Branding/   AppIcon, AppIcon_AdaptiveFG/BG, TitleLogo_KO/EN, TitleBG
  Documents/Store/Graphics/            play_*, steam_*

## 사용
  python _tools/gen_branding.py                 # 전부
  python _tools/gen_branding.py logo icon       # 일부만 (logo | icon | bg | store)
  python _tools/gen_branding.py --preview DIR   # 출력 대신 DIR 에 저장 (실험용)

## 라이선스 주의
  DOSSaemmul(© 2016 Damheo Lee) 은 폰트 파일에 라이선스 문구가 없다(fsType 0).
  로고에 글꼴 윤곽이 그대로 남으므로 출시 전에 상업 이용 조건을 확인할 것.
"""
import math
import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageEnhance, ImageFont

BASE = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
SPR = os.path.join(BASE, "Assets", "@Resources", "Sprite")
FONT_DIR = os.path.join(BASE, "Assets", "@Resources", "Font", "UsingFont")
OUT_BRAND = os.path.join(SPR, "Branding")
OUT_STORE = os.path.join(BASE, "Documents", "Store", "Graphics")
FONT_MAIN = os.path.join(FONT_DIR, "DOSSaemmul.ttf")  # 한글 + 라틴 (게임 KR 폰트와 동일)

_out = {"brand": OUT_BRAND, "store": OUT_STORE}  # --preview 로 바꿀 수 있다


# ───────────────────────────── 기본 픽셀 도구 ─────────────────────────────

def rgba(c, a=255):
    return (int(c[0]), int(c[1]), int(c[2]), a)


def load(*parts):
    return Image.open(os.path.join(SPR, *parts)).convert("RGBA")


def nn(im, k):
    """정수 배율 NEAREST 확대 — 픽셀아트는 이 함수로만 키운다."""
    return im.resize((im.width * k, im.height * k), Image.NEAREST)


def crop_alpha(im):
    return im.crop(im.getbbox())


def _srgb():
    """PNG 에 넣을 sRGB ICC 프로파일. lcms 가 헤더에 생성 시각을 넣어 매번 바이트가 달라지므로 시각/ID 를 고정한다(재현성)."""
    try:
        from PIL import ImageCms
        b = bytearray(ImageCms.ImageCmsProfile(ImageCms.createProfile("sRGB")).tobytes())
    except Exception:       # littlecms 가 없으면 프로파일 없이 저장 (기본 sRGB 로 취급된다)
        return None
    b[24:36] = (2026).to_bytes(2, "big") + (9).to_bytes(2, "big") + (29).to_bytes(2, "big") + bytes(6)
    b[84:100] = bytes(16)
    return bytes(b)


SRGB = _srgb()


def _rel(p):
    try:
        return os.path.relpath(p, BASE).replace("\\", "/")
    except ValueError:      # --preview 가 다른 드라이브를 가리킬 때
        return p


def save(im, kind, name):
    d = _out[kind]
    os.makedirs(d, exist_ok=True)
    p = os.path.join(d, name)
    if SRGB:
        im.save(p, icc_profile=SRGB)
    else:
        im.save(p)
    print("  wrote", _rel(p), im.size)
    return p


def shift(m, dx, dy):
    """배열을 (dx, dy) 만큼 밀고 빈 곳은 0/False 로 채운다 (roll 처럼 감기지 않는다)."""
    out = np.zeros_like(m)
    h, w = m.shape[:2]
    ys, yd = (slice(0, h - dy), slice(dy, h)) if dy >= 0 else (slice(-dy, h), slice(0, h + dy))
    xs, xd = (slice(0, w - dx), slice(dx, w)) if dx >= 0 else (slice(-dx, w), slice(0, w + dx))
    out[yd, xd] = m[ys, xs]
    return out


def dilate(m, r, shape="square"):
    """불리언 마스크 팽창. shape: square(모서리 각짐) | round(둥근 픽셀 윤곽)."""
    out = m.copy()
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            if shape == "round" and dx * dx + dy * dy > r * r + r:
                continue
            if dx or dy:
                out |= shift(m, dx, dy)
    return out


def pad(m, n):
    return np.pad(m, ((n, n), (n, n)))


BAYER4 = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]], np.float32) / 16.0 + 1 / 32.0


def bayer(w, h, ox=0, oy=0):
    return np.tile(BAYER4, (h // 4 + 2, w // 4 + 2))[oy % 4:oy % 4 + h, ox % 4:ox % 4 + w]


def dither_vgrad(w, h, stops, bands=14):
    """세로 그라디언트를 bands 색 팔레트 + 4x4 오더드 디더로 찍는다 (픽셀아트 하늘/초원).

    stops: [(t, (r,g,b)), ...]  t 는 0..1 (위 → 아래)
    """
    ys = np.arange(h) / max(h - 1, 1)
    ts = [s[0] for s in stops]
    pal = np.zeros((bands, 3), np.float32)
    for i in range(bands):
        t = i / (bands - 1)
        for c in range(3):
            pal[i, c] = np.interp(t, ts, [s[1][c] for s in stops])
    # 행별 팔레트 위치 — 정수부는 밴드, 소수부는 디더 임계
    tt = np.clip(ys, 0, 1) * (bands - 1)
    lo = np.floor(tt).astype(int)
    fr = tt - lo
    hi = np.minimum(lo + 1, bands - 1)
    thr = bayer(w, h)
    pick_hi = fr[:, None] > thr
    idx = np.where(pick_hi, hi[:, None], lo[:, None])
    out = np.zeros((h, w, 4), np.uint8)
    out[:, :, :3] = pal[idx].astype(np.uint8)
    out[:, :, 3] = 255
    return Image.fromarray(out, "RGBA")


def alpha_dither(a, w, h, levels=6, ox=0, oy=0):
    """0..1 알파 맵을 오더드 디더로 이진화한다 — 부드러운 그림자/비네팅을 픽셀 질감으로."""
    thr = bayer(w, h, ox, oy)
    q = np.floor(np.clip(a, 0, 1) * levels) / levels
    fr = np.clip(a, 0, 1) * levels - np.floor(np.clip(a, 0, 1) * levels)
    up = (fr > thr)
    return np.clip(q + up / levels, 0, 1)


def paint_mask(canvas, mask, color, x=0, y=0):
    """불리언 마스크 영역을 단색으로 칠한다 (canvas: RGBA PIL)."""
    layer = np.zeros(mask.shape + (4,), np.uint8)
    layer[mask] = rgba(color)
    canvas.alpha_composite(Image.fromarray(layer, "RGBA"), (x, y))


def sparkle(canvas, cx, cy, r, color=(255, 255, 255), core=(255, 255, 255)):
    """4방향 반짝이. r 은 팔 길이(px). 중심은 밝게, 팔은 색으로."""
    d = ImageDraw.Draw(canvas)
    d.point((cx, cy), fill=rgba(core))
    for k in range(1, r + 1):
        c = rgba(color if k > 1 or r < 3 else core)
        for dx, dy in ((k, 0), (-k, 0), (0, k), (0, -k)):
            d.point((cx + dx, cy + dy), fill=c)
    if r >= 3:
        for dx, dy in ((1, 1), (-1, 1), (1, -1), (-1, -1)):
            d.point((cx + dx, cy + dy), fill=rgba(color))


# ───────────────────────────── 스프라이트 소스 ─────────────────────────────

def hero_sprite(pose="ready"):
    """영웅(Player_01 — 게임 프리팹이 쓰는 백발 검사). 고정 셀에서 잘라낸다.

    ready: 공격 시트 0번 프레임 — 큰 검을 앞으로 겨눈 자세(키아트용, 89x96)
    idle : 대기 시트 0번 프레임 — 검을 내린 자세(50x106)
    """
    if pose == "ready":
        sheet, cell = "attack_Sheet.png", (0, 0)
    else:
        sheet, cell = "idle_Sheet.png", (0, 0)
    im = load("Player", "Player_01", sheet)
    r, c = cell
    return crop_alpha(im.crop((c * 176, r * 128, (c + 1) * 176, (r + 1) * 128)))


def _exterior(transparent):
    """투명 마스크에서 이미지 경계와 이어진 '바깥'만 True (내부 구멍과 구분)."""
    ext = np.zeros_like(transparent)
    ext[0, :] = transparent[0, :]
    ext[-1, :] = transparent[-1, :]
    ext[:, 0] |= transparent[:, 0]
    ext[:, -1] |= transparent[:, -1]
    while True:
        g = ext.copy()
        g[1:, :] |= ext[:-1, :]
        g[:-1, :] |= ext[1:, :]
        g[:, 1:] |= ext[:, :-1]
        g[:, :-1] |= ext[:, 1:]
        g &= transparent
        if (g == ext).all():
            return ext
        ext = g


def minimon_sprite(mon_id, hole_fill=(255, 248, 250)):
    """미니몬 카드 스프라이트. 라비(2002)는 흰 몸통이 배경 키잉으로 투명 구멍이 나 있어 흰색으로 메운다."""
    im = load("Monster", "Card", "Mon_%s.png" % mon_id)
    a = np.array(im)
    tr = a[:, :, 3] < 96
    holes = tr & ~_exterior(tr)
    a[holes] = rgba(hole_fill)
    # 배경 키잉 찌꺼기: 가장 바깥 경계에 붙은 회백색 픽셀(예: 버섯 밑단의 흰 줄)은 지운다
    for _ in range(2):
        op = a[:, :, 3] > 0
        outer = op & ~(shift(op, 1, 0) & shift(op, -1, 0) & shift(op, 0, 1) & shift(op, 0, -1))
        junk = outer & (a[:, :, :3].min(axis=2) >= 185)
        if not (junk & ~holes).any():
            break
        a[junk & ~holes, 3] = 0
    return crop_alpha(Image.fromarray(a, "RGBA"))     # 카드 하단 여백을 잘라 발이 맨 아래 행이 되게


def slime_kawaii():
    """게임의 귀여운 파랑 슬라임(분홍 꽃) — slime_monster.png 첫 프레임(24x18). 32행부터는 다음 프레임이다."""
    im = load("Monster", "slime_monster.png")
    return crop_alpha(im.crop((0, 0, 64, 32)))


# ───────────────────────────── 로고 ─────────────────────────────

LOGO_FILL = [(255, 251, 214), (255, 232, 118), (255, 201, 58), (255, 164, 28), (236, 112, 16)]  # 위 → 아래
LOGO_INNER = (150, 58, 14)      # 채움 바로 바깥 1px — 진한 주황갈색
LOGO_OUTLINE = (30, 22, 72)     # 두꺼운 남색 외곽
LOGO_EXTRUDE = (14, 10, 40)     # 아래로 빠지는 두께
LOGO_RIM = (214, 236, 255)      # 가장 바깥 1px 밝은 테두리 (어두운 배경에서 윤곽 유지)


def text_mask(text, size, gap=2, wave=(), grow=1, space=0.34, font=FONT_MAIN):
    """글자를 비트맵(안티앨리어싱 없음)으로 그려 불리언 마스크로 돌려준다.

    한글은 폰트 고유 advance(음절 칸)를 그대로 쓴다 — 자모 사이 틈이 넓은 폰트라 음절 간격을 줄이면 읽히지 않는다.
    고정폭 라틴은 'i' 옆이 휑해서 글리프 잉크 폭 + gap(px) 으로 붙인다.
    size 는 16 의 배수(32/48)여야 폰트의 16px 도트 격자에 맞아 획이 또렷하다.
    grow: 획을 사방으로 굵게 할 픽셀 수(가는 획 보강), wave: 글자별 y 오프셋 목록.
    """
    ft = ImageFont.truetype(font, size)
    cells = []
    for ch in text:
        if ch == " ":
            cells.append((None, size * space))
            continue
        im = Image.new("L", (size * 2, size * 2), 0)
        d = ImageDraw.Draw(im)
        d.fontmode = "1"
        d.text((size // 2, size // 2), ch, font=ft, fill=255, anchor="la")
        g = np.array(im) > 127
        ys, xs = np.nonzero(g)
        if ord(ch) > 0x2E80:   # 한글: 음절 칸 고정 폭
            cells.append((g[:, size // 2:size // 2 + int(ft.getlength(ch))], ft.getlength(ch)))
        else:                  # 라틴: 잉크 폭 기준
            cells.append((g[:, xs.min():xs.max() + 1], xs.max() - xs.min() + 1 + gap))
    n = grow + 2
    total_w = int(sum(c[1] for c in cells)) + 10 * n
    canvas = np.zeros((size * 2 + 4 * n, total_w), bool)
    x = 4 * n
    for i, (arr, adv) in enumerate(cells):
        if arr is not None:
            dy = wave[i % len(wave)] if wave else 0
            gm = dilate(arr, grow, "square") if grow else arr
            gh, gw = gm.shape
            yy = 2 * n + dy
            xx = int(round(x))
            hh, ww = min(gh, canvas.shape[0] - yy), min(gw, canvas.shape[1] - xx)
            canvas[yy:yy + hh, xx:xx + ww] |= gm[:hh, :ww]
        x += adv
    ys, xs = np.nonzero(canvas)
    return canvas[ys.min():ys.max() + 1, xs.min():xs.max() + 1]


def sticker(im, ol=2, rim=1):
    """스프라이트에 로고와 같은 남색 외곽 + 밝은 림을 둘러 로고의 일부처럼 보이게 한다."""
    a = np.array(im.convert("RGBA"))
    m = a[:, :, 3] > 0
    n = ol + rim + 1
    mp = pad(m, n)
    out = Image.new("RGBA", (mp.shape[1], mp.shape[0]), (0, 0, 0, 0))
    paint_mask(out, dilate(mp, ol + rim, "round") & ~dilate(mp, ol, "round"), LOGO_RIM)
    paint_mask(out, dilate(mp, ol, "round"), LOGO_OUTLINE)
    out.alpha_composite(Image.fromarray(np.pad(a, ((n, n), (n, n), (0, 0))), "RGBA"))
    return out


def render_logo(text, size=48, gap=2, wave=(0, 0, -1, -2, -2, -1, 0, 1), canvas=(336, 84), sparkles=True, mascot=True):
    """로고 한 장을 네이티브(작은) 해상도로 만든다. 호출부가 정수 배율로 키운다.

    레이어: 압출(아래 3px) → 림 → 남색 외곽(3px) → 진한 주황 안쪽선(1px) → 채움(위→아래 5톤) → 윗면 하이라이트
    """
    m = text_mask(text, size, gap, wave)
    n = 6
    mp = pad(m, n)
    h, w = mp.shape
    fill = mp
    inner = dilate(fill, 1, "round") & ~fill
    outl = dilate(fill, 3, "round") & ~dilate(fill, 1, "round")
    rim = dilate(fill, 4, "round") & ~dilate(fill, 3, "round")
    body = dilate(fill, 3, "round")
    ext = np.zeros_like(fill)
    for k in range(1, 4):
        ext |= shift(body, 0, k)
    ext &= ~body
    cv = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    lock = ~body & ~_exterior(~body)     # 획 안에 갇힌 작은 구멍(예: '미'의 ㅁ) — 림 위에 남색으로 메운다
    paint_mask(cv, dilate(ext, 1, "round") & ~body & ~ext, LOGO_RIM)
    paint_mask(cv, ext, LOGO_EXTRUDE)
    paint_mask(cv, rim & ~ext, LOGO_RIM)
    paint_mask(cv, lock, LOGO_OUTLINE)
    paint_mask(cv, outl, LOGO_OUTLINE)
    paint_mask(cv, inner, LOGO_INNER)
    ys, _ = np.nonzero(fill)
    y0, y1 = ys.min(), ys.max()
    arr = np.zeros((h, w, 4), np.uint8)
    for y in range(y0, y1 + 1):
        t = (y - y0) / max(y1 - y0, 1)
        band = min(int(t * len(LOGO_FILL)), len(LOGO_FILL) - 1)
        arr[y, fill[y]] = rgba(LOGO_FILL[band])
    top = fill & ~shift(fill, 0, 1)      # 위쪽 가장자리 1px 하이라이트
    arr[top] = rgba((255, 255, 240))
    bot = fill & ~shift(fill, 0, -1)     # 아래쪽 가장자리 1px 그늘
    arr[bot] = rgba((196, 84, 12))
    cv.alpha_composite(Image.fromarray(arr, "RGBA"))
    cw, ch = canvas
    out = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    ox, oy = (cw - w) // 2, (ch - h) // 2 + 2
    out.alpha_composite(cv, (ox, oy))
    if mascot:
        # 마지막 글자 위에 귀여운 슬라임이 앉아 있다
        sl = sticker(slime_kawaii())
        cols = np.nonzero(fill.any(axis=0))[0]
        x1 = cols.max()
        top_rows = [np.nonzero(fill[:, c])[0].min() for c in range(x1 - 10, x1 + 1) if fill[:, c].any()]
        ty = min(top_rows)
        sx = ox + x1 - sl.width + 6
        sy = oy + ty - sl.height + 4
        out.alpha_composite(sl, (sx, max(sy, 0)))
    if sparkles:
        sparkle(out, ox - 3, oy + n, 3, (255, 244, 170))
        sparkle(out, ox + w * 6 // 10, oy + 1, 2, (255, 244, 170))
    return out


def logos():
    """TitleLogo_KO / TitleLogo_EN — 1344x336 (4:1, 네이티브 336x84 를 x4)."""
    print("[logo]")
    for lang, text in (("KO", "미니몬 퀘스트"), ("EN", "Minimon Quest")):
        native = render_logo(text)
        save(nn(native, 4), "brand", "TitleLogo_%s.png" % lang)


# ───────────────────────────── 앱 아이콘 ─────────────────────────────

ICON_PALETTES = {
    # (중심, 중간, 가장자리) — 방사형. 파란 하늘색은 슬라임이 배경에 묻혀 48px 에서 안 읽혀서 골드로 확정했다.
    "gold": ((255, 247, 200), (255, 208, 78), (238, 128, 30)),
}


def sunburst(n, rays=12, palette="gold", center=(0.5, 0.5), levels=8):
    """n x n 방사형 햇살 배경. 광선은 두 톤이 번갈아, 거리는 levels 단계 + 4x4 디더로 끊어 찍는다."""
    c0, c1, c2 = ICON_PALETTES[palette]
    yy, xx = np.mgrid[0:n, 0:n]
    dx = (xx + 0.5) / n - center[0]
    dy = (yy + 0.5) / n - center[1]
    ang = np.arctan2(dy, dx)
    ray = np.floor((ang + math.pi) / (2 * math.pi) * rays * 2).astype(int) % 2
    r = np.clip(np.hypot(dx, dy) / 0.72, 0, 1)
    tt = r * (levels - 1)
    lo = np.floor(tt)
    up = (tt - lo) > bayer(n, n)
    t = np.clip((lo + up) / (levels - 1), 0, 1)
    col = np.zeros((n, n, 3), np.float32)
    for k in range(3):
        a = np.where(t < 0.5, c0[k] + (c1[k] - c0[k]) * (t / 0.5), c1[k] + (c2[k] - c1[k]) * ((t - 0.5) / 0.5))
        col[:, :, k] = a
    col[ray == 1] *= 0.93                        # 어두운 쪽 광선
    col[ray == 0] = np.minimum(col[ray == 0] * 1.03, 255)
    out = np.zeros((n, n, 4), np.uint8)
    out[:, :, :3] = np.clip(col, 0, 255).astype(np.uint8)
    out[:, :, 3] = 255
    return Image.fromarray(out, "RGBA")


HILL_TONES = [(158, 220, 98), (98, 182, 66), (62, 138, 58), (40, 100, 52)]


def draw_hill(canvas, top, bulge=10, x0=0, x1=None, seed=3, flowers=True):
    """캔버스 아래쪽에 볼록한 풀 언덕을 찍는다 (유닛 해상도). top: 중앙의 윗선 y."""
    rng = np.random.RandomState(seed)
    w, h = canvas.size
    x1 = x1 or w
    arr = np.array(canvas)
    for x in range(x0, x1):
        u = (x - w / 2) / (w / 2)
        yt = int(top + bulge * u * u)
        for y in range(yt, h):
            d = y - yt
            tone = 0 if d < 2 else 1 if d < 9 else 2 if d < 20 else 3
            arr[y, x] = rgba(HILL_TONES[tone])
    im = Image.fromarray(arr, "RGBA")
    d = ImageDraw.Draw(im)
    # 풀잎 · 꽃
    for _ in range(int(w / 5)):
        x = int(rng.randint(x0 + 2, x1 - 2))
        u = (x - w / 2) / (w / 2)
        yt = int(top + bulge * u * u)
        y = yt + int(rng.randint(3, 16))
        if y < h - 1:
            d.line([(x, y), (x, y - 2)], fill=rgba(HILL_TONES[0]))
    for _ in range(int(w / 26) if flowers else 0):
        x = int(rng.randint(x0 + 6, x1 - 6))
        u = (x - w / 2) / (w / 2)
        y = int(top + bulge * u * u) + int(rng.randint(5, 14))
        col = [(255, 255, 255), (255, 170, 200), (255, 226, 90)][int(rng.randint(0, 3))]
        d.point((x, y), fill=rgba(col))
        d.point((x, y - 1), fill=rgba(col))
    canvas.paste(im)


def contact_shadow(canvas, cx, cy, w, h, alpha=120, color=(24, 64, 40)):
    """발밑 타원 그림자 (캔버스 해상도, 단색 + 알파)."""
    yy, xx = np.mgrid[0:canvas.height, 0:canvas.width]
    m = ((xx - cx) / (w / 2)) ** 2 + ((yy - cy) / (h / 2)) ** 2 <= 1
    paint = np.zeros((canvas.height, canvas.width, 4), np.uint8)
    paint[m] = rgba(color, alpha)
    canvas.alpha_composite(Image.fromarray(paint, "RGBA"))


def icon_bg(n, palette="gold", rays=12, with_hill=True, hill_top=None):
    """아이콘 배경(불투명) n x n 유닛."""
    bg = sunburst(n, rays, palette, (0.5, 0.46))
    if with_hill:
        draw_hill(bg, hill_top if hill_top is not None else int(n * 0.865), bulge=int(n * 0.05))
    return bg


def icon_fg(size, hero_k, slime_k, hero_xy, slime_xy, shadow=True):
    """영웅 + 슬라임 전경(투명). size px 정사각, 배율/좌표는 px 기준."""
    fg = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    hero = nn(hero_sprite("ready"), hero_k)
    sl = nn(minimon_sprite("2001"), slime_k)
    hx, hy = hero_xy
    sx, sy = slime_xy
    if shadow:
        contact_shadow(fg, hx + hero.width * 0.42, hy + hero.height - hero_k, hero.width * 0.62, hero_k * 5, 120)
        contact_shadow(fg, sx + sl.width * 0.5, sy + sl.height - slime_k, sl.width * 0.86, slime_k * 5, 120)
    fg.alpha_composite(hero, (hx, hy))
    fg.alpha_composite(sl, (sx, sy))
    return fg


def app_icon(palette="gold"):
    """AppIcon 1024x1024 (불투명 · 텍스트 없음). 유닛 4px: 배경 x2U(햇살 8px 도트) · 영웅 x2U(8px) · 슬라임 x1U(4px)."""
    U = 4
    bg = nn(icon_bg(128, palette, 12, True, int(128 * 0.865)), 2)         # 256 유닛
    bg = bg.convert("RGBA")
    fg = icon_fg(256, 2, 1, (14, 30), (140, 178))
    for (x, y, r) in ((202, 24, 5), (232, 62, 3), (26, 44, 3), (238, 118, 2)):
        sparkle(fg, x, y, r, (255, 255, 255), (255, 255, 255))
    bg.alpha_composite(fg)
    return nn(bg, U).convert("RGB")


def adaptive_icons():
    """안드로이드 적응형 아이콘 432x432 — FG(투명, 핵심은 가운데 288 안), BG(불투명 햇살 + 언덕).

    시스템 마스크(원/스쿼클/물방울)가 어느 모양이든 살아남도록 영웅+슬라임 묶음 전체를
    지름 ~264px(66dp) 원 안에 넣는다. 영웅 x2px · 슬라임 x1px — 1024 아이콘의 8px/4px 비율과 같다.
    """
    bg = nn(icon_bg(108, "gold", 12, True, 74), 4)
    fg = icon_fg(432, 2, 1, (109, 112), (227, 255))
    for (x, y, r) in ((262, 122, 4), (168, 112, 2), (300, 200, 2)):
        sparkle(fg, x, y, r, (255, 255, 255), (255, 255, 255))
    return fg, bg.convert("RGB")


def mini_icon(n=32):
    """16/32px 용 도트 아이콘 — 영웅까지 넣으면 뭉개져서, 마스코트(분홍 꽃 슬라임)를 햇살 위에 크게 앉힌다."""
    bg = sunburst(n, 8, "gold", (0.5, 0.42), levels=5)
    draw_hill(bg, int(n * 0.80), bulge=2, seed=5, flowers=False)
    sl = slime_kawaii()
    x = (n - sl.width) // 2
    y = int(n * 0.80) + 3 - sl.height
    ImageDraw.Draw(bg).ellipse((x + 1, y + sl.height - 2, x + sl.width - 2, y + sl.height + 1), fill=rgba((30, 90, 40)))
    bg.alpha_composite(sl, (x, y))
    sparkle(bg, int(n * 0.80), int(n * 0.20), 2, (255, 255, 255), (255, 255, 255))
    return bg.convert("RGBA")


def icons():
    print("[icon]")
    save(app_icon("gold"), "brand", "AppIcon.png")
    fg, bg = adaptive_icons()
    save(fg, "brand", "AppIcon_AdaptiveFG.png")
    save(bg, "brand", "AppIcon_AdaptiveBG.png")


# ───────────────────────────── 장면 엔진 (타이틀 배경 · 스토어 키아트 공용) ─────────────────────────────
# 배경은 RegionLayer 21000(산들바람 숲) 레이어 3장 — 게임 전투 배경과 같은 그림이라 스토어 화면과 인게임이 이어진다.
#   Far  : 하늘 + 안개 낀 산 (불투명 512x341)      → 채도/명도를 올려 맑은 낮 하늘로 보정
#   Mid  : 나무 + 폐허 아치 + 바위 (투명 512x125)  → 멀리 보이는 목적지(아치)
#   Near : 큰 나무 + 바위 + 풀 (투명 512x126)      → 전경 프레임
# 캔버스는 "유닛" 해상도(작은 픽셀)로 그리고 마지막에 U배로 키운다.

SKY_TOP = (46, 108, 216)


def defringe(im, rounds=3):
    """크로마키 찌꺼기(자주/보라 테두리 픽셀)를 벗겨 낸다 — 투명과 맞닿은 보라빛 픽셀은 알파 0, 안쪽에 남은 건 이웃 색으로."""
    a = np.array(im)
    for _ in range(rounds):
        op = a[:, :, 3] > 0
        rgb = a[:, :, :3].astype(int)
        mag = (rgb[..., 0] > rgb[..., 1] + 12) & (rgb[..., 2] > rgb[..., 1] + 12) & op   # 경계는 더 엄격하게
        edge = op & ~(shift(op, 1, 0) & shift(op, -1, 0) & shift(op, 0, 1) & shift(op, 0, -1))
        kill = mag & edge
        if not kill.any():
            break
        a[kill, 3] = 0
    op = a[:, :, 3] > 0
    rgb = a[:, :, :3].astype(int)
    mag = (rgb[..., 0] > rgb[..., 1] + 25) & (rgb[..., 2] > rgb[..., 1] + 25) & op
    if mag.any():        # 안쪽에 갇힌 보라 픽셀: 자기 색의 G 채널로 채도를 죽여 어두운 회녹색으로
        g = rgb[..., 1]
        for c in (0, 2):
            a[..., c] = np.where(mag, np.minimum(rgb[..., c], g + 6), a[..., c])
    return Image.fromarray(a, "RGBA")


def region_layer(kind):
    im = load("Background", "RegionLayer", "RegionL_%s_21000.png" % kind)
    return defringe(im) if kind != "Far" else im


def far_layer():
    im = region_layer("Far").convert("RGB")
    im = ImageEnhance.Brightness(ImageEnhance.Color(im).enhance(1.9)).enhance(1.12)
    return im.convert("RGBA")


def tile_x(im, x0, w):
    """가로로 좌우반전 복제 타일링해서 x0(음수 가능)부터 w 폭을 잘라 낸다 — 반전 이음새라 끊김이 없다."""
    flip = im.transpose(Image.FLIP_LEFT_RIGHT)
    k0 = x0 // im.width
    k1 = (x0 + w - 1) // im.width
    strip = Image.new("RGBA", (im.width * (k1 - k0 + 1), im.height), (0, 0, 0, 0))
    for k in range(k0, k1 + 1):
        strip.paste(flip if k % 2 else im, ((k - k0) * im.width, 0))
    ox = x0 - k0 * im.width
    return strip.crop((ox, 0, ox + w, im.height))


def tile_seq(seq, x0, w):
    """(이미지, 반전여부) 목록을 순환하며 가로로 이어 붙여 x0(음수 가능)부터 w 폭을 잘라 낸다.

    Mid 스트립은 왼쪽 끝에 잘린 나무가 있고 오른쪽 끝은 깨끗하다 — 잘린 끝끼리는 반전 쌍으로 맞물려야 이음새가 티나지 않는다.
    """
    pw = seq[0][0].width
    ph = seq[0][0].height
    k0, k1 = x0 // pw, (x0 + w - 1) // pw
    strip = Image.new("RGBA", (pw * (k1 - k0 + 1), ph), (0, 0, 0, 0))
    for k in range(k0, k1 + 1):
        im, fl = seq[k % len(seq)]
        strip.paste(im.transpose(Image.FLIP_LEFT_RIGHT) if fl else im, ((k - k0) * pw, 0))
    ox = x0 - k0 * pw
    return strip.crop((ox, 0, ox + w, ph))


def pixel_cloud(w, h, seed):
    """3톤 픽셀 구름 (윗면 밝게, 아랫면은 평평하고 푸른 그늘). 유닛 해상도."""
    rng = np.random.RandomState(seed)
    yy, xx = np.mgrid[0:h, 0:w]
    base = int(h * 0.82)
    m = np.zeros((h, w), bool)
    x = int(h * 0.45)
    while x < w - int(h * 0.3):
        r = h * rng.uniform(0.26, 0.5)
        cy = base - r * 0.85 + rng.randint(0, 2)
        m |= (xx - x) ** 2 + (yy - cy) ** 2 <= r * r
        x += max(3, int(r * 0.9))
    m &= yy <= base
    out = np.zeros((h, w, 4), np.uint8)
    out[m] = rgba((188, 216, 244))                       # 그늘(밑면)
    out[m & ~shift(m, 0, -3)] = rgba((188, 216, 244))
    out[m & shift(m, 0, -3)] = rgba((222, 238, 252))     # 중간
    out[m & ~shift(m, 0, 4)] = rgba((240, 248, 255))     # 윗면
    out[m & ~shift(m, 0, 2)] = rgba((255, 255, 255))     # 맨 윗줄 하이라이트
    return Image.fromarray(out, "RGBA")


def blend_dither(dst, src, x, y, ramp):
    """src 를 dst 위 (x,y) 에 얹되 윗쪽 ramp 행 동안 알파를 0→1 로 올린다 — 오더드 디더라 경계가 픽셀 질감이다."""
    w, h = src.size
    a = np.ones((h, w), np.float32)
    a[:ramp] = (np.arange(ramp)[:, None] + 0.5) / ramp
    keep = a > bayer(w, h, x, y)
    s = np.array(src)
    s[..., 3] = np.where(keep, s[..., 3], 0)
    dst.alpha_composite(Image.fromarray(s, "RGBA"), (x, y))


def build_scene(w, h, far_y, mid_y, ground_y, mid_x=0, far_x=0, seed=7, clouds=(), ground_bottom=(14, 28, 24), wide=False):
    """하늘 · 산 · 나무/아치 · 초원을 유닛 해상도로 쌓는다 (캐릭터 제외).

    far_y   : Far 레이어 윗변 y (음수면 레이어 윗부분을 잘라 쓴다. 양수면 그 위는 하늘 그라디언트가 이어 붙는다)
    mid_y   : Mid 스트립 바닥선 y (나무/아치가 서 있는 지평선)
    ground_y: 초원이 시작하는 y
    """
    far = far_layer()
    top = np.array(far)[0, 4, :3]
    full = dither_vgrad(w, h, [(0, SKY_TOP), (max(far_y, 1) / h, tuple(int(v) for v in top)), (1.0, tuple(int(v) for v in top))], 16)
    for (cx, cy, cw, ch, sd) in clouds:
        full.alpha_composite(pixel_cloud(cw, ch, sd), (cx, cy))
    ft = tile_x(far, far_x, w)
    if far_y >= 0:
        blend_dither(full, ft, 0, far_y, 44)            # Far 의 알갱이 질감이 하늘로 스며들게
    else:
        full.alpha_composite(ft.crop((0, -far_y, w, far.height)), (0, 0))
    bot = np.array(far)[-1, 0, :3]
    yb = far_y + far.height
    if yb < h:
        full.paste(Image.new("RGBA", (w, h - yb), rgba(bot)), (0, yb))
    mid = region_layer("Mid")
    if wide:    # 넓은 캔버스: 아치가 반복되지 않게 [Mid, Near, Mid 반전] 순환 (Near 는 양끝이 깨끗하다)
        near = region_layer("Near")
        mt = tile_seq([(mid, False), (near, False), (mid, True)], mid_x, w)
    else:
        mt = tile_x(mid, mid_x, w)
    my = mid_y - mid.height
    if my >= 0:
        full.alpha_composite(mt, (0, my))
    else:
        full.alpha_composite(mt.crop((0, -my, w, mid.height)), (0, 0))
    draw_meadow(full, ground_y, seed, ground_bottom)
    return full


MEADOW = [(112, 184, 66), (96, 168, 60), (78, 146, 54), (56, 118, 50), (34, 80, 42)]


def smooth_noise(w, h, cell, seed):
    """cell 크기 격자 난수를 부드럽게 늘린 0..1 노이즈 (초원 얼룩용)."""
    rng = np.random.RandomState(seed)
    g = Image.fromarray((rng.rand(h // cell + 3, w // cell + 3) * 255).astype(np.uint8), "L")
    g = g.resize(((w // cell + 3) * cell, (h // cell + 3) * cell), Image.BICUBIC)
    return np.array(g)[:h, :w].astype(np.float32) / 255.0


def draw_meadow(canvas, y0, seed, bottom):
    """지평선 y0 아래를 풀밭으로 채운다: 위 밝은 초록 → 아래로 어두워지는 디더 그라디언트 + 얼룩 + 풀잎/꽃."""
    rng = np.random.RandomState(seed + 1)
    w, h = canvas.size
    gh = h - y0
    stops = [(0.0, MEADOW[0]), (0.25, MEADOW[1]), (0.55, MEADOW[3]), (0.8, MEADOW[4]), (1.0, bottom)]
    g = np.array(dither_vgrad(w, gh, stops, 18)).astype(np.float32)
    n = smooth_noise(w, gh, 14, seed + 2)
    lvl = np.digitize(n, [0.40, 0.62]) - 1                   # -1 / 0 / +1 세 톤의 얼룩
    fade = np.clip(1 - np.arange(gh)[:, None] / (gh * 0.8), 0, 1)
    g[..., :3] *= (1 + 0.075 * lvl * fade)[..., None]
    canvas.paste(Image.fromarray(np.clip(g, 0, 255).astype(np.uint8), "RGBA"), (0, y0))
    d = ImageDraw.Draw(canvas)
    light = (146, 208, 84)
    dark = MEADOW[3]
    for _ in range(int(w * gh / 90)):
        x = int(rng.randint(0, w))
        t = rng.rand() ** 1.4
        y = y0 + int(t * gh * 0.8) + 1
        if y >= h - 2:
            continue
        ln = 1 + int(rng.randint(0, 2 + int(t * 3)))
        c = light if rng.rand() < 0.5 else dark
        d.line([(x, y), (x, y - ln)], fill=rgba(c))
    for _ in range(int(w * gh / 700)):
        x = int(rng.randint(2, w - 2))
        y = y0 + int((rng.rand() ** 1.2) * gh * 0.7) + 3
        col = [(255, 255, 255), (255, 176, 206), (255, 228, 96), (176, 214, 255)][int(rng.randint(0, 4))]
        d.point((x, y), fill=rgba(col))
        if rng.rand() < 0.5:
            d.point((x, y - 1), fill=rgba(col))


def bezier(p0, p1, p2, p3, n=400):
    t = np.linspace(0, 1, n)[:, None]
    p0, p1, p2, p3 = (np.array(p, np.float32) for p in (p0, p1, p2, p3))
    return (1 - t) ** 3 * p0 + 3 * (1 - t) ** 2 * t * p1 + 3 * (1 - t) * t ** 2 * p2 + t ** 3 * p3


def draw_path(canvas, ctrl, w0, w1, seed=5, dirt=None):
    """베지어 중심선을 따라 폭이 w0→w1 로 넓어지는 흙길 (유닛 해상도). 원근: 아래(가까움)일수록 넓고 진하다."""
    dirt = dirt or [(220, 190, 124), (202, 164, 102), (178, 136, 84), (146, 108, 68)]
    rng = np.random.RandomState(seed)
    pts = bezier(*ctrl)
    n = len(pts)
    t = np.linspace(0, 1, n)
    hw = (w0 + (w1 - w0) * t ** 1.3) / 2
    hw *= 1 + 0.10 * np.convolve(rng.randn(n), np.ones(25) / 25, "same") * 4
    w, h = canvas.size
    xs0, xs1 = int(max(pts[:, 0].min() - hw.max() - 2, 0)), int(min(pts[:, 0].max() + hw.max() + 2, w))
    ys0, ys1 = int(max(pts[:, 1].min() - 2, 0)), int(min(pts[:, 1].max() + hw.max() + 2, h))
    gy, gx = np.mgrid[ys0:ys1, xs0:xs1]
    best = np.full(gx.shape, 1e9, np.float32)
    bt = np.zeros(gx.shape, np.float32)
    for i in range(n):
        d = (gx - pts[i, 0]) ** 2 + (gy - pts[i, 1]) ** 2
        # 폭 대비 정규화 거리 (가까울수록 작다)
        dn = np.sqrt(d) / hw[i]
        m = dn < best
        best = np.where(m, dn, best)
        bt = np.where(m, t[i], bt)
    rough = smooth_noise(xs1 - xs0, ys1 - ys0, 3, seed + 9)
    inside = best < 1.0 + (rough - 0.5) * 0.35
    edge = inside & ~(shift(inside, 1, 0) & shift(inside, -1, 0) & shift(inside, 0, 1) & shift(inside, 0, -1))
    # 톤: 깊이 t 에 따라 4단 + 노이즈 얼룩 (디더는 넓은 면에서 망사처럼 보여 쓰지 않는다)
    patch = smooth_noise(xs1 - xs0, ys1 - ys0, 6, seed + 3)
    idx = np.clip(np.round(np.clip(bt, 0, 1) * 3 + (patch - 0.5) * 1.7), 0, 3).astype(int)
    arr = np.array(canvas)
    pal = np.array(dirt, np.uint8)
    region = arr[ys0:ys1, xs0:xs1]
    col = pal[idx]
    edge_col = pal[np.clip(idx + 1, 0, 3)]
    out = region.copy()
    out[inside, :3] = col[inside]
    out[edge, :3] = edge_col[edge]
    out[inside | edge, 3] = 255
    # 자갈 알갱이
    sp = inside & ~edge & (rng.rand(*inside.shape) < 0.035)
    out[sp, :3] = pal[np.clip(idx[sp] - 1, 0, 3)]
    arr[ys0:ys1, xs0:xs1] = out
    canvas.paste(Image.fromarray(arr, "RGBA"))


def draw_tufts(canvas, y_base, height, color, seed=21, density=0.16):
    """화면 맨 아래 전경 풀 실루엣 (짙은 색 V자 풀잎 다발) — 하단을 액자처럼 두르되 버튼 자리는 비운다."""
    rng = np.random.RandomState(seed)
    w, h = canvas.size
    d = ImageDraw.Draw(canvas)
    x = 0
    while x < w:
        u = abs(x - w / 2) / (w / 2)              # 가장자리일수록 크고 빽빽
        if rng.rand() < density + 0.5 * u ** 2:
            ht = int(height * (0.35 + 0.65 * u ** 1.5) * (0.6 + 0.6 * rng.rand()))
            for k in range(int(2 + rng.randint(0, 3))):
                lean = int(rng.randint(-3, 4))
                d.line([(x + k * 2, y_base), (x + k * 2 + lean, y_base - ht + int(rng.randint(0, 4)))], fill=rgba(color))
        x += int(rng.randint(3, 8))
    d.rectangle([0, y_base, w, h], fill=rgba(color))


def overlay_ramp(canvas, y0, y1, a0, a1, color, levels=8):
    """y0→y1 구간에 알파 a0→a1 로 color 를 덮는다 (디더 비네팅 — 하단 UI 영역을 어둡고 고요하게)."""
    w, h = canvas.size
    a = np.zeros((h, w), np.float32)
    ys = np.arange(h)
    ramp = np.clip((ys - y0) / max(y1 - y0, 1), 0, 1) * (a1 - a0) + a0
    ramp[ys < y0] = 0
    a[:] = ramp[:, None]
    a = alpha_dither(a, w, h, levels)
    layer = np.zeros((h, w, 4), np.uint8)
    layer[..., :3] = color
    layer[..., 3] = (a * 255).astype(np.uint8)
    canvas.alpha_composite(Image.fromarray(layer, "RGBA"))


def feet_span(sp, rows=5):
    """스프라이트 맨 아래 rows 행의 알파 가로 범위 — 발밑 그림자 위치용."""
    a = np.array(sp)[-rows:, :, 3] > 0
    xs = np.nonzero(a.any(axis=0))[0]
    return xs.min(), xs.max()


def firefly(canvas, x, y, color=(255, 240, 140)):
    """반딧불: 밝은 점 + 흐린 십자 후광 (유닛 해상도)."""
    d = ImageDraw.Draw(canvas)
    halo = rgba(color, 90)
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        canvas.alpha_composite(Image.new("RGBA", (1, 1), halo), (x + dx, y + dy))
    d.point((x, y), fill=rgba((255, 252, 220)))


def place_sprite(scene, sp, x, y_feet, shadow=True):
    """유닛 해상도 장면에 스프라이트를 (x, 발 y) 로 놓고 발밑 타원 그림자를 깐다."""
    if shadow:
        x0, x1 = feet_span(sp)
        cx = x + (x0 + x1) / 2
        contact_shadow(scene, cx, y_feet - 1, (x1 - x0) * 1.05 + 6, max(6, sp.width // 12), 120)
    scene.alpha_composite(sp, (int(x), int(y_feet - sp.height)))


def title_bg():
    """TitleBG 1440x3040 — 유닛 4px(360x760). 위 35% 하늘(로고) · 가운데 영웅+미니몬+아치 · 아래 30% 어두운 초원(버튼)."""
    U = 4
    W, H = 360, 760
    ground = 492
    scene = build_scene(W, H, far_y=204, mid_y=ground + 6, ground_y=ground, mid_x=-30, far_x=60,
                        clouds=[(14, 30, 52, 17, 1), (296, 12, 40, 13, 4), (236, 58, 56, 18, 2)])   # 로고 자리(y 400~850px)는 비운다
    # 흙길: 폐허 아치(오른쪽 안쪽)에서 화면 왼쪽 밖으로 굽이쳐 내려온다 — 영웅이 바라보는 방향(→)이 목적지
    draw_path(scene, ((272, ground + 3), (274, ground + 40), (120, ground + 34), (-30, ground + 96)), 5, 70)
    hero = nn(hero_sprite("ready"), 2)
    place_sprite(scene, hero, 20, ground + 6)
    for mid, x, yb in (("2104", 176, ground + 28), ("2002", 118, ground + 35), ("2001", 150, ground + 50)):
        place_sprite(scene, minimon_sprite(mid), x, yb)
    overlay_ramp(scene, 522, H, 0.0, 0.88, (8, 20, 18))
    rng = np.random.RandomState(11)
    for _ in range(16):
        firefly(scene, int(rng.randint(10, W - 10)), int(rng.randint(ground + 30, ground + 170)))
    sparkle(scene, 206, ground - 182, 4, (255, 244, 170), (255, 255, 255))
    sparkle(scene, 214, ground - 168, 2, (255, 244, 170), (255, 255, 255))
    draw_tufts(scene, H - 8, 24, (6, 16, 14))
    return nn(scene, U).crop((0, 0, 1440, 3040)).convert("RGB")


def bgs():
    print("[bg]")
    save(title_bg(), "brand", "TitleBG.png")


# ───────────────────────────── 스토어 그래픽 (Google Play · Steam) ─────────────────────────────

def logo_native(lang, stacked=False):
    """로고 네이티브 해상도(투명 여백 제거). stacked 는 세로 캡슐용 두 줄 (Minimon / Quest)."""
    if not stacked:
        text = "미니몬 퀘스트" if lang == "KO" else "Minimon Quest"
        return crop_alpha(render_logo(text, canvas=(400, 120)))
    l1, l2 = ("미니몬", "퀘스트") if lang == "KO" else ("Minimon", "Quest")
    a = crop_alpha(render_logo(l1, canvas=(400, 120)))
    b = crop_alpha(render_logo(l2, canvas=(400, 120), mascot=False, sparkles=False))
    w = max(a.width, b.width)
    out = Image.new("RGBA", (w, a.height + b.height - 4), (0, 0, 0, 0))
    out.alpha_composite(b, ((w - b.width) // 2, a.height - 4))
    out.alpha_composite(a, ((w - a.width) // 2, 0))
    return out


def put(img, sp, k, x, y_feet, shadow=True, flip=False, cx=False):
    """px 캔버스에 스프라이트를 k배 확대해 놓는다 (x 는 왼쪽, cx=True 면 가운데 기준). 발밑에 접지 그림자."""
    if flip:
        sp = sp.transpose(Image.FLIP_LEFT_RIGHT)
    big = nn(sp, k)
    if cx:
        x = x - big.width // 2
    if shadow:
        x0, x1 = feet_span(sp)
        contact_shadow(img, x + (x0 + x1) * k / 2, y_feet - k, (x1 - x0) * k * 1.05 + 6 * k, max(6, big.width // 12), 120)
    img.alpha_composite(big, (int(x), int(y_feet - big.height)))
    return big


def scene_px(W, H, U, **kw):
    """W x H px 배경 장면(유닛 해상도). U px/유닛."""
    wu, hu = -(-W // U), -(-H // U)
    clouds = kw.pop("clouds", ())
    path = kw.pop("path", None)
    sc = build_scene(wu, hu, clouds=clouds, **kw)
    if path:
        draw_path(sc, *path)
    return sc


def finish(sc, U, W, H):
    return nn(sc, U).crop((0, 0, W, H))


def paste_logo(img, lg, k, x, y, cx=False):
    big = nn(lg, k)
    if cx:
        x = x - big.width // 2
    img.alpha_composite(big, (int(x), int(y)))
    return big


def party(img, k_hero, k_mon, hero_xy, mons, flip_hero=False):
    """영웅 + 미니몬 배치. mons: [(id, x, y_feet, flip)], 뒤→앞 순서."""
    hx, hy = hero_xy
    put(img, hero_sprite("ready"), k_hero, hx, hy, flip=flip_hero)
    for mid, x, yf, fl in mons:
        put(img, minimon_sprite(mid), k_mon, x, yf, flip=fl)


def banner_scene(W, H, U, horizon, peaks, mid_x=0, far_x=0, clouds=(), path=None, wide=False):
    """가로형 배너 배경. horizon: 나무/아치가 서는 지평선 px y, peaks: 먼 산 봉우리 px y."""
    return scene_px(W, H, U, far_y=peaks // U - 150, mid_y=horizon // U + 3, ground_y=horizon // U,
                    mid_x=mid_x, far_x=far_x, clouds=clouds, path=path, wide=wide)


def feature_graphic(lang):
    """Google Play 피처 그래픽 1024x500 — 가장자리는 비워 둔다(스토어가 잘라 낼 수 있다)."""
    W, H, U = 1024, 500, 2
    sc = banner_scene(W, H, U, horizon=352, peaks=126, mid_x=-25, far_x=30,
                      clouds=[(300, 12, 40, 13, 1), (430, 70, 34, 11, 3)])
    img = finish(sc, U, W, H).convert("RGBA")
    party(img, 3, 2, (676, 470), [("2104", 400, 448, False), ("2002", 190, 462, False), ("2001", 296, 484, False)], flip_hero=True)
    overlay_ramp(img, 400, H, 0.0, 0.6, (8, 20, 18), 6)
    paste_logo(img, logo_native(lang), 2, 46, 44)
    return img.convert("RGB")


def steam_header(lang):
    """Steam 헤더 캡슐 920x430 (스토어 목록에선 460x215 로 줄어 보인다 — 로고가 반 크기에서도 읽혀야 한다)."""
    W, H, U = 920, 430, 2
    sc = banner_scene(W, H, U, horizon=302, peaks=120, mid_x=-10, far_x=70,
                      clouds=[(150, 8, 40, 13, 2), (330, 50, 34, 11, 4)])
    img = finish(sc, U, W, H).convert("RGBA")
    party(img, 3, 2, (34, 420), [("2104", 690, 396, False), ("2002", 566, 408, False), ("2001", 626, 422, False)])
    overlay_ramp(img, 340, H, 0.0, 0.55, (8, 20, 18), 6)
    paste_logo(img, logo_native(lang), 2, 300, 34)
    return img.convert("RGB")


def steam_small_capsule():
    """Steam 작은 캡슐 462x174 — 로고가 주인공. 영웅은 오른쪽에서 상반신만, 슬라임이 모서리에서 빼꼼."""
    W, H, U = 462, 174, 2
    sc = banner_scene(W, H, U, horizon=128, peaks=48, mid_x=20, far_x=40, clouds=[(20, 2, 30, 10, 2)])
    img = finish(sc, U, W, H).convert("RGBA")
    put(img, hero_sprite("ready"), 2, 300, 196)
    put(img, minimon_sprite("2001"), 1, 356, 176)
    overlay_ramp(img, 120, H, 0.0, 0.5, (8, 20, 18), 6)
    paste_logo(img, logo_native("EN"), 1, 14, 42)
    return img.convert("RGB")


def steam_main_capsule(lang):
    """Steam 메인 캡슐 1232x706."""
    W, H, U = 1232, 706, 2
    sc = banner_scene(W, H, U, horizon=480, peaks=190, mid_x=-40, far_x=40,
                      clouds=[(30, 10, 46, 15, 1), (500, 100, 40, 13, 3), (520, 4, 34, 11, 5)])
    img = finish(sc, U, W, H).convert("RGBA")
    party(img, 4, 3, (70, 690), [("2104", 840, 640, False), ("2002", 600, 664, False), ("2001", 720, 690, False)])
    overlay_ramp(img, 560, H, 0.0, 0.55, (8, 20, 18), 6)
    paste_logo(img, logo_native(lang), 3, 172, 30)
    return img.convert("RGB")


def portrait_capsule(W, H, lang):
    """세로 캡슐 (748x896 · 600x900) — 두 줄 로고를 위에, 영웅과 미니몬을 아래에."""
    U = 2
    horizon = int(H * 0.64)
    sc = banner_scene(W, H, U, horizon=horizon, peaks=int(H * 0.36), mid_x=-4, far_x=90,
                      clouds=[(8, 6, 40, 13, 1), (int(W / U) - 50, 60, 34, 11, 3)])
    draw_path(sc, ((int(W / U * 0.66), horizon // U + 3), (int(W / U * 0.66), horizon // U + 30), (int(W / U * 0.3), horizon // U + 40), (-10, horizon // U + 80)), 4, 60)
    img = finish(sc, U, W, H).convert("RGBA")
    yf = int(H * 0.90)
    party(img, 3, 2, (int(W * 0.06), yf), [("2104", int(W * 0.54), yf - 38, False), ("2002", int(W * 0.40), yf - 26, False), ("2001", int(W * 0.48), yf - 4, False)])
    overlay_ramp(img, int(H * 0.72), H, 0.0, 0.55, (8, 20, 18), 6)
    lg = logo_native(lang, stacked=True)
    paste_logo(img, lg, 3, W // 2, 40, cx=True)
    return img.convert("RGB")


def steam_library_hero():
    """Steam 라이브러리 히어로 3840x1240 — 글자/로고 없음. 오른쪽에 파티, 왼쪽 아래는 Steam 이 로고를 올릴 자리라 비워 둔다."""
    W, H, U = 3840, 1240, 4
    sc = banner_scene(W, H, U, horizon=860, peaks=330, mid_x=120, far_x=0, wide=True,
                      clouds=[(120, 30, 46, 15, 1), (420, 70, 60, 19, 2), (720, 24, 40, 13, 3), (300, 130, 36, 12, 4)])
    img = finish(sc, U, W, H).convert("RGBA")
    party(img, 8, 4, (2640, 1170), [("2104", 2200, 1090, False), ("2002", 1780, 1120, False), ("2001", 2010, 1200, False)], flip_hero=True)
    overlay_ramp(img, 980, H, 0.0, 0.5, (8, 20, 18), 6)
    return img.convert("RGB")


def steam_library_logo():
    """Steam 라이브러리 로고 1280x720 투명 — 로고만 (정수 4배)."""
    lg = nn(logo_native("EN"), 4)
    out = Image.new("RGBA", (1280, 720), (0, 0, 0, 0))
    out.alpha_composite(lg, ((1280 - lg.width) // 2, (720 - lg.height) // 2))
    return out


def steam_page_background():
    """Steam 상점 페이지 배경 1438x810 — 콘텐츠 뒤에 깔리므로 어둡고 조용하게(캐릭터 없음)."""
    W, H, U = 1438, 810, 2
    sc = banner_scene(W, H, U, horizon=560, peaks=240, mid_x=60, far_x=120, wide=True, clouds=[(60, 20, 46, 15, 1), (420, 60, 40, 13, 3)])
    img = finish(sc, U, W, H).convert("RGBA")
    # 채도를 죽이고 남색으로 눌러 텍스트 뒤에서 조용하게
    arr = np.array(img).astype(np.float32)
    lum = arr[..., :3].mean(axis=2, keepdims=True)
    arr[..., :3] = arr[..., :3] * 0.35 + lum * 0.65
    arr[..., :3] = arr[..., :3] * 0.34 + np.array([10, 16, 30], np.float32) * 0.66
    img = Image.fromarray(np.clip(arr, 0, 255).astype(np.uint8), "RGBA")
    return img.convert("RGB")


def icon_square(size):
    """AppIcon 을 size 로 줄인 정사각형 (BOX 로 평균 — 픽셀 도트를 뭉개지 않고 줄인다)."""
    return app_icon("gold").resize((size, size), Image.BOX if 1024 % size == 0 else Image.LANCZOS)


def stores():
    print("[store]")
    save(icon_square(512).convert("RGBA"), "store", "play_icon_512.png")
    save(feature_graphic("EN"), "store", "play_feature_graphic_1024x500.png")
    save(feature_graphic("KO"), "store", "play_feature_graphic_1024x500_ko.png")
    save(steam_header("EN"), "store", "steam_header_920x430.png")
    save(steam_header("KO"), "store", "steam_header_920x430_ko.png")
    save(steam_small_capsule(), "store", "steam_small_capsule_462x174.png")
    save(steam_main_capsule("EN"), "store", "steam_main_capsule_1232x706.png")
    save(steam_main_capsule("KO"), "store", "steam_main_capsule_1232x706_ko.png")
    save(portrait_capsule(748, 896, "EN"), "store", "steam_vertical_capsule_748x896.png")
    save(portrait_capsule(600, 900, "EN"), "store", "steam_library_capsule_600x900.png")
    save(steam_library_hero(), "store", "steam_library_hero_3840x1240.png")
    save(steam_library_logo(), "store", "steam_library_logo_1280x720.png")
    save(steam_page_background(), "store", "steam_page_background_1438x810.png")
    save(icon_square(184).convert("RGB"), "store", "steam_community_icon_184x184.png")
    mini = mini_icon(32)
    save(mini, "store", "steam_client_icon_32.png")
    frames = {16: mini.resize((16, 16), Image.BOX), 32: mini,
              48: app_icon("gold").resize((48, 48), Image.LANCZOS).convert("RGBA"),
              256: icon_square(256).convert("RGBA")}
    ico = os.path.join(_out["store"], "steam_client_icon.ico")
    frames[256].save(ico, format="ICO", sizes=[(k, k) for k in sorted(frames)],
                     append_images=[frames[k] for k in (48, 32, 16)])
    print("  wrote", _rel(ico), "frames", sorted(frames))


if __name__ == "__main__":
    args = sys.argv[1:]
    if "--preview" in args:
        i = args.index("--preview")
        d = args[i + 1]
        _out["brand"] = _out["store"] = d
        del args[i:i + 2]
    todo = args or ["logo", "icon", "bg", "store"]
    bad = [t for t in todo if t not in ("logo", "icon", "bg", "store")]
    if bad:
        sys.exit("알 수 없는 대상: %s (logo | icon | bg | store)" % ", ".join(bad))
    if "logo" in todo:
        logos()
    if "icon" in todo:
        icons()
    if "bg" in todo:
        bgs()
    if "store" in todo:
        stores()
