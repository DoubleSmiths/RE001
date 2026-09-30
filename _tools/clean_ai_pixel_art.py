# -*- coding: utf-8 -*-
"""AI 로 만든 픽셀 그림 다듬기 — 'AI 티'(번진 가장자리·색 잡음·외톨이 점)를 줄인다.

  AI 픽셀아트는 한 장에 수천 색, 가장자리 20~30% 가 반투명으로 번져 있다(직접 그린 주인공 시트는 약 31색·반투명 0%).
  격자(한 칸 = 몇 화소)는 같은 몬스터 안에서도 프레임마다 2~9 로 들쭉날쭉해 격자 맞추기는 하지 않는다 — 깨짐만 늘어난다.
  하는 일(그림 크기·위치·파일 이름은 그대로 → .meta·스프라이트 칸·피벗 불변):
    1. 알파 이진화(128 기준) — 번진 가장자리를 끊는다. 속이 반투명인 그림(유령 등)은 건너뛴다(SEMI_INTERIOR_MAX).
    2. 색 줄이기 — CIELAB k-평균, 초기값은 '먼 색부터'(드문 강조색 — 흰 얼굴·노란 등불 — 이 한 칸을 얻는다),
       대표색은 실제 있던 색. 애니메이션은 원형(폴더)마다 팔레트 하나로 — 프레임 사이 색이 흔들리지 않게.
    3. 외톨이 점 지우기 — 상하좌우 이웃이 모두 투명한 불투명 점.
  배경은 색을 줄여도 거의 달라 보이지 않아(칠한 듯한 밀도는 그대로) 다루지 않는다.

    python _tools/clean_ai_pixel_art.py --preview <dir>   # 사본 + 전후 비교 시트
    python _tools/clean_ai_pixel_art.py --write           # 제자리에 덮어쓴다(되돌리기는 git)
"""
import argparse
import glob
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SPRITE = os.path.join(ROOT, "Assets", "@Resources", "Sprite")

ALPHA_CUT = 128
SEMI_INTERIOR_MAX = 0.10     # 속(네 이웃이 모두 보이는 점)의 반투명 비율이 이보다 크면 알파는 건드리지 않는다
K_ANIM, K_CARD, K_ICON = 40, 32, 28
MAX_FIT = 60000              # k-평균에 쓰는 고유 색 표본 수(무작위, 시드 고정)

# (이름, 경로 패턴, k, 묶음) — 묶음이 "folder" 면 폴더마다 팔레트 하나
TARGETS = [
    ("anim", "Monster/Anim/*/*.png", K_ANIM, "folder"),
    ("card", "Monster/Card/*.png", K_CARD, None),
    ("generated", "Monster/Generated/*.png", K_CARD, None),
    ("equip", "Icon/Equipment/**/Equip3*.png", K_ICON, None),
    ("skill", "Icon/Skill/**/*.png", K_ICON, None),
    ("relic", "Icon/Relic/**/*.png", K_ICON, None),
    ("material", "Icon/Material/**/*.png", K_ICON, None),
]


def to_lab(rgb):
    c = rgb / 255.0
    c = np.where(c > 0.04045, ((c + 0.055) / 1.055) ** 2.4, c / 12.92)
    m = np.array([[0.4124, 0.3576, 0.1805], [0.2126, 0.7152, 0.0722], [0.0193, 0.1192, 0.9505]])
    xyz = c @ m.T / np.array([0.95047, 1.0, 1.08883])
    f = np.where(xyz > 0.008856, np.cbrt(xyz), 7.787 * xyz + 16 / 116)
    return np.stack([116 * f[:, 1] - 16, 500 * (f[:, 0] - f[:, 1]), 200 * (f[:, 1] - f[:, 2])], 1)


def palette(rgb_rows, k, seed=0):
    """불투명 화소(N,3)들 → k색 팔레트(실제 있던 색)."""
    uniq, cnt = np.unique(rgb_rows, axis=0, return_counts=True)
    if len(uniq) <= k:
        return uniq.astype(np.uint8)
    if len(uniq) > MAX_FIT:
        rng = np.random.default_rng(seed)
        pick = rng.choice(len(uniq), MAX_FIT, replace=False, p=cnt / cnt.sum())
        uniq, cnt = uniq[pick], cnt[pick]
    lab = to_lab(uniq.astype(float))
    w = np.sqrt(cnt.astype(float))
    cent = [lab[int(np.argmax(cnt))]]
    d2 = np.sum((lab - cent[0]) ** 2, 1)
    for _ in range(1, k):
        i = int(np.argmax(d2 * w))
        cent.append(lab[i])
        d2 = np.minimum(d2, np.sum((lab - lab[i]) ** 2, 1))
    cent = np.array(cent)
    for _ in range(10):
        lbl = ((lab[:, None, :] - cent[None, :, :]) ** 2).sum(2).argmin(1)
        for j in range(k):
            m = lbl == j
            if m.any():
                cent[j] = (lab[m] * w[m, None]).sum(0) / w[m].sum()
    dist = ((lab[:, None, :] - cent[None, :, :]) ** 2).sum(2)
    lbl = dist.argmin(1)
    out = []
    for j in range(k):
        m = np.where(lbl == j)[0]
        if len(m):
            out.append(uniq[m[np.argmin(dist[m, j])]])
    return np.array(out, dtype=np.uint8)


def semi_interior_ratio(alpha):
    vis = alpha > 0
    inner = vis.copy()
    inner[1:, :] &= vis[:-1, :]
    inner[:-1, :] &= vis[1:, :]
    inner[:, 1:] &= vis[:, :-1]
    inner[:, :-1] &= vis[:, 1:]
    semi = (alpha > 16) & (alpha < 240)
    return (semi & inner).sum() / max(1, vis.sum())


def clean(a, pal, keep_alpha):
    """a: (H,W,4) uint8 → 다듬은 (H,W,4)."""
    alpha = a[..., 3]
    vis = alpha >= ALPHA_CUT if not keep_alpha else alpha > 0
    rgb = a[..., :3].reshape(-1, 3)
    flat_vis = vis.reshape(-1)
    out = np.zeros_like(a)
    if flat_vis.any():
        uniq, inv = np.unique(rgb[flat_vis], axis=0, return_inverse=True)
        idx = ((to_lab(uniq.astype(float))[:, None, :] - to_lab(pal.astype(float))[None, :, :]) ** 2).sum(2).argmin(1)
        mapped = pal[idx][inv.reshape(-1)]
        tmp = out[..., :3].reshape(-1, 3)
        tmp[flat_vis] = mapped
        out[..., :3] = tmp.reshape(out[..., :3].shape)
    out[..., 3] = np.where(vis, alpha if keep_alpha else 255, 0)
    # 외톨이 점
    v = out[..., 3] > 0
    n = np.zeros(v.shape, int)
    n[1:, :] += v[:-1, :]
    n[:-1, :] += v[1:, :]
    n[:, 1:] += v[:, :-1]
    n[:, :-1] += v[:, 1:]
    lone = v & (n == 0)
    out[lone] = 0
    return out


def jobs():
    for name, pat, k, group in TARGETS:
        files = sorted(glob.glob(os.path.join(SPRITE, pat), recursive=True))
        files = [f for f in files if not os.path.basename(f).startswith("_raw_")]   # 원본 아틀라스 — 게임이 쓰지 않는다(참조 0)
        files = [f for f in files if Image.open(f).size[0] >= 48]   # 32×32 이하는 원래 구매 에셋(손으로 그린 것)
        if group == "folder":
            by = {}
            for f in files:
                by.setdefault(os.path.dirname(f), []).append(f)
            for d, fs in sorted(by.items()):
                yield name, k, fs
        else:
            for f in files:
                yield name, k, [f]


def run(preview_dir=None, write=False, limit=None):
    stats = {}
    flagged = []
    done = 0
    for name, k, files in jobs():
        arrs = [np.array(Image.open(f).convert("RGBA")) for f in files]
        keep = [semi_interior_ratio(a[..., 3]) > SEMI_INTERIOR_MAX for a in arrs]
        rows = np.concatenate([a[..., :3][a[..., 3] >= ALPHA_CUT] for a in arrs])
        pal = palette(rows, k)
        for f, a, kp in zip(files, arrs, keep):
            if kp:
                flagged.append(os.path.relpath(f, SPRITE))
            b = clean(a, pal, kp)
            before = len(np.unique(a[..., :3][a[..., 3] >= ALPHA_CUT], axis=0))
            after = len(np.unique(b[..., :3][b[..., 3] > 0], axis=0))
            s = stats.setdefault(name, [0, 0, 0])
            s[0] += 1; s[1] += before; s[2] += after
            if preview_dir:
                dst = os.path.join(preview_dir, os.path.relpath(f, SPRITE))
                os.makedirs(os.path.dirname(dst), exist_ok=True)
                Image.fromarray(b, "RGBA").save(dst, optimize=True)
            if write:
                Image.fromarray(b, "RGBA").save(f, optimize=True)
        done += 1
        if limit and done >= limit:
            break
    for name, (n, b, a) in stats.items():
        print(f"{name:10s} {n:4d}장  평균 색 {b / n:8.0f} → {a / n:5.0f}")
    print(f"알파 유지(속이 반투명) {len(flagged)}장" + (": " + ", ".join(flagged[:8]) + (" …" if len(flagged) > 8 else "") if flagged else ""))


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--preview")
    ap.add_argument("--write", action="store_true")
    ap.add_argument("--limit", type=int)
    args = ap.parse_args()
    if not args.preview and not args.write:
        for name, pat, k, group in TARGETS:
            print(name, pat, "k=%d" % k, "(폴더마다 팔레트)" if group else "")
        sys.exit(0)
    run(args.preview, args.write, args.limit)
