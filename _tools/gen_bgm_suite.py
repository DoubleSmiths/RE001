# -*- coding: utf-8 -*-
"""타이틀 · 마을 · 필드 · 보스 BGM 4곡 — 오리지널 칩튠 작곡/합성기.

## 왜
  기존 BGM_BreezeForest / BGM_Village1 / BGM_BWF 는 출처를 알 수 없는 음원이라 상업 출시에 쓸 수 없다.
  대체 음원을 **이 스크립트가 원천**이 되도록 새로 작곡해 합성한다. 기존 파일은 열지도 참고하지도 않았고,
  다른 게임 음악(포켓몬·젤다·FF·드퀘·마리오 등)의 선율을 인용하거나 흉내 내지 않았다 — 멜로디·화성은 아래 악보가 처음이다.
  파일 이름은 코드가 이미 부르는 이름을 그대로 쓴다(BGM_Boss 만 새 곡). 합성·마스터링 부품은 gen_chiptune.py 를 가져다 쓴다.

## 곡 (모노 16bit WAV · 원형 루프 — 끝이 곡 속의 평범한 한 지점처럼 처음으로 이어진다)
  BGM_BreezeForest  타이틀. D장조 126 BPM 4/4 · 28마디(53.33초). 희망찬 모험 + 약간의 그리움. 첫 화면에서 처음 듣는 곡.
      I  (1~4)    D 페달 위 D–E/D–G/D → A7sus4–A7: II장조(리디안 #4, G#)가 '해 뜨는' 느낌. 리드는 3–5–8 상승 분산화음을 한 음 올려 되풀이
      A  (5~12)   훅: 부점(3+1) 스킵으로 D6 까지 뛰어올랐다 내려앉고, 7마디는 그 스킵을 뒤집어 내려온다.
                  펄스2 는 말발굽(8분+16분 둘) 아르페지오, 베이스 1-5-8-5. 반종지(A7) 뒤 F#6 정점을 찍고 D 로 닫는다
      B  (13~20)  그리움: Gmaj7–A–F#m7–Bm7 / Em7–A/G–Gm6(차용 iv)–A7. 리드를 50% 듀티로 둥글게, 펄스2 는 뜯는 분산화음
      A' (21~28)  훅 재현 — 펄스2 가 3도 아래 화음, 베이스 8분 드라이브. D6 으로 닫고 I 에서 숨을 고른 뒤 다시 돈다
  BGM_Village1      마을. F장조 6/8 (점4분 = 72) · 40마디(66.67초). 아늑하고 따뜻하게 — 장비·상점·대화 중 오래 깔린다.
      A  (1~8)    시칠리아노 리듬(점8분+16분+8분)의 '미-파-미' 꾸밈 동기. 펄스2 8분 분산화음, 베이스 4분+8분 흔들림
      A' (9~16)   같은 선율 + 펄스2 대선율(점4분 3·6도, 리드가 멈출 때 움직인다). 끝을 F7 로 돌려 B 로 넘어간다
      B  (17~24)  Bb 에서 시작하는 수다스러운 구간: 8분 셋 오름 + 4분 착지. 펄스2 는 뒷박 '짠짠' 반주, 리드 25%
      A''(25~32)  선율 재현, 펄스2 가 8분 늦게 따라 부르는 메아리
      C  (33~40)  D단조로 그늘을 드리운 A 동기 변주(A7 의 C# 이 단조 색) → Bb–Gm7–C7 으로 F 에 돌아온다
  BGM_BWF           필드/자동전투. A장조 144 BPM 4/4 · 40마디(66.67초). 몇 시간씩 켜 두는 곡이라 활기차되 지치지 않게:
                    12.5% 듀티 리드는 쓰지 않는다(25%/50% 교대), 하이햇은 여린 8분 뒷박, 16분 아르페지오는 B' 8마디만.
      A  (1~8)    훅: 첫 박을 쉬고 8분 셋으로 치고 오르는 동기. 펄스2 는 8분 '근-5-3-5', 베이스 옥타브 8분
      A' (9~16)   훅 반복 — 펄스2 가 3도 아래 화음, 베이스 1-5-8-5
      B  (17~24)  D–E–C#m7–F#m / Bm7–E–A/C#·D–E7. 엇박(8분+4분 당김) 이웃음 동기를 한 도씩 옮긴다. 리드 50%, 펄스2 뒷박
      B' (25~32)  B 반복 — 펄스2 16분 아르페지오로 곡의 정점, 탐 필인으로 내려앉는다
      C  (33~40)  쉬어 가는 구간: Dmaj7–C#m7–Bm7–A 하행 반복, 리드 50%·저음역, 드럼은 킥+여린 햇만. E7 에서 빌드업 → 1마디
  BGM_Boss          스테이지 보스(신규). C단조 184 BPM 4/4 · 32마디(41.74초). 빠르고 몰아붙이는 긴장.
      I  (1~4)    말발굽(16분 둘+8분) 페달 리프 Cm–Cm–Db7(나폴리 7화음)–G7. 리드는 12.5% 로 리프 윗선을 한 옥타브 위에서 겹친다
      A  (5~12)   주제: 옥타브 도약 뒤 이끎음(B)–b6(Ab) 꾸밈. 펄스2 16분 오스티나토, 베이스 옥타브 8분
      A' (13~20)  주제 반복 — 리드 12.5%, 펄스2 3도 아래 화음, 베이스 말발굽, 16분 하이햇
      B  (21~28)  Ab–Bb–Cm 으로 긴 음이 한 단씩 오르는 서사(F6 정점) → Db–G7
      T  (29~32)  F6 정점에서 Ab–Bb 분산화음으로 내려왔다가 Bdim7(→ G 베이스, G7b9)로 다시 조여 B5→C6 이끎음 해결로 1마디에

## 음원 구성 (gen_chiptune.py 와 같은 NES 4채널 흉내)
  펄스1 리드 · 펄스2 반주(아르페지오/화음/대선율/메아리) · 삼각파 베이스 · 노이즈 드럼(+삼각파 킥·탐).
  한 채널 화음, PolyBLEP, 칸 안에서 끝나는 램프, 원형 버퍼 렌더, 12kHz 출력단 저역통과는 gen_chiptune 과 같은 부품이다.
  악보: 4마디 구간 = (코드, 펄스2, 베이스, 드럼, 리드). 코드는 마디당 한 토큰, 'Em7,A7' 은 마디를 반씩 나누고
  'A/C#' 은 베이스가 C# 을 짚는다. 펄스2·베이스는 패턴 이름(코드를 따라 음이 정해진다)이거나 직접 쓴 4마디 악보다.
  form = [(구간, 리드 듀티[, 리드 음량])] — 50% 듀티가 25% 보다 크게 들려서, 쉬어 가는 구간은 음량으로 한 칸 낮춘다.
  라우드니스는 ffmpeg ebur128 로 재서 맞춘다: -16 LUFS, 트루피크 ≤ -1 dBTP.

## 왜 WAV
  Unity 임포터는 MP3 의 LAME 갭리스 헤더를 무시해 인코더 지연/패딩만큼 길어지고, 루프마다 틈이 난다
  (gen_chiptune.py '루프 주의', 2026-09-29 실측 +52ms). 그래서 16bit WAV 로 굽고 압축은 Unity 임포터(Vorbis)에 맡긴다.

## 사용
  python _tools/gen_bgm_suite.py                          # Assets/@Resources/Sound/BGM 에 4곡 + 측정 출력
  python _tools/gen_bgm_suite.py --out DIR --preview DIR  # 다른 폴더로, + 루프 2회 반복 wav(이음매를 귀로 확인)
  출력: 곡마다 길이/LUFS/트루피크/DC, 루프 이음매, 강도(음표·드럼 온셋/초, 스펙트럴 플럭스, 스펙트럼 중심).
  필드가 보스보다 약하지 않으면 실패한다. 필요: numpy, PATH 의 ffmpeg. 노이즈 시드 고정 → 두 번 돌려도 바이트가 같다.
"""
import argparse
import os
import re
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gen_chiptune import (BASE, FFMPEG, KIT, RAW, SEED, SR, ff, hat, hit_env, kick, master, midi, noise,   # noqa: E402
                          parse, place, play, pulse, report, snare, triangle, lowpass)

OUT_DIR = os.path.join(BASE, "Assets", "@Resources", "Sound", "BGM")
ECHO = 0.45   # 메아리 음량(리드 대비)

#region 화성 · 반주
QUAL = {   # 코드 성질 → 루트로부터의 반음 간격
    "": (0, 4, 7), "m": (0, 3, 7), "7": (0, 4, 7, 10), "m7": (0, 3, 7, 10), "maj7": (0, 4, 7, 11),
    "m6": (0, 3, 7, 9), "7sus4": (0, 5, 7, 10), "dim7": (0, 3, 6, 9),
}


def chord(sym):
    """'F#m7' · 'A/C#' → (루트 pc, 간격, 베이스 pc). 슬래시 뒤는 베이스가 짚는 음."""
    m = re.fullmatch(r"([A-G][#b]?)([^/]*)(?:/([A-G][#b]?))?", sym)
    return midi(m[1] + "4") % 12, QUAL[m[2]], midi((m[3] or m[1]) + "4") % 12


def voicing(sym, lo):
    """루트를 [lo, lo+12) 에 두고 위로 쌓은 4음 (3화음이면 루트 옥타브를 더한다)."""
    root, ivs, _ = chord(sym)
    r = lo + (root - lo) % 12
    v = [r + i for i in ivs]
    return v + [r + 12] if len(v) == 3 else v


def bass_tone(sym, deg, lo):
    """베이스 도수 → 음. R = 베이스음([lo, lo+12), 슬래시 반영) · 8 = R 옥타브 위 · 3/5 = R 위 가장 가까운 코드 3음/5음."""
    root, ivs, b = chord(sym)
    r = lo + (b - lo) % 12
    if deg in "R8":
        return r + 12 * (deg == "8")
    return r + (root + ivs[{"3": 1, "5": 2}[deg]] - r) % 12


def comp_tone(sym, k, lo):
    """펄스2 패턴 기호 → 음. 숫자 = 보이싱 인덱스(0 = 루트), c = 한 채널 화음(60Hz 아르페지오)."""
    v = voicing(sym, lo)
    return tuple(v[:3]) if k == "c" else v[int(k)]


def realize(pat, syms, steps, note):
    """반주 패턴 한 마디('0:1 2:1 …') → [(pitch, 칸)]. 기호는 그 칸에 울리는 코드로 note(코드, 기호) 가 음으로 바꾼다."""
    out, s = [], 0
    for tok in pat.split():
        k, n = tok.split(":")
        out.append((None if k == "-" else note(syms[s * len(syms) // steps], k), int(n)))
        s += int(n)
    assert s == steps, f"{s}칸 != {steps}칸: {pat}"
    return out


def harmony(lead, syms, steps):
    """리드보다 단3도 이상 아래의 가장 가까운 코드 구성음(그 칸의 코드) — 늘 협화. 같은 음이 이어지면 하나로 묶는다."""
    out, s = [], 0
    for m, n in lead:
        h = None
        if m is not None:
            root, ivs, _ = chord(syms[s * len(syms) // steps])
            pcs = {(root + i) % 12 for i in ivs}
            h = next(x for x in range(m - 3, m - 16, -1) if x % 12 in pcs)
        if out and out[-1][0] == h:
            out[-1] = (h, out[-1][1] + n)
        else:
            out.append((h, n))
        s += n
    return out


def delayed(events, d, total):
    """events 를 d 칸 늦추고 total 칸에서 자른다 — 구간 밖으로 새지 않는 메아리."""
    out, s = [(None, d)], d
    for m, n in events:
        n = min(n, total - s)
        if n <= 0:
            break
        out.append((m, n))
        s += n
    return out
#endregion

#region 드럼
def tom(rng, f0):
    """삼각파 피치 드롭 탐 + 짧은 노이즈 — 필인용."""
    n = int(0.22 * SR)
    t = np.arange(n) / SR
    return triangle(f0 * (1 + 0.5 * np.exp(-t / 0.04))) * hit_env(n, 0.08) + 0.25 * noise(rng, n, 7000) * hit_env(n, 0.008)


DRUMS = {**KIT,   # gen_chiptune 의 K S h o X 에 여린 소리와 탐을 더한다
    "k": [(kick, 0.8)],                          # 여린 킥
    "s": [(snare, 0.35)],                        # 여린 스네어(림 느낌)
    "x": [(lambda r: hat(r, 0.012), 0.16)],      # 여린 닫힌 햇
    "T": [(lambda r: tom(r, 200), 0.8)],
    "M": [(lambda r: tom(r, 150), 0.8)],
    "L": [(lambda r: tom(r, 110), 0.8)],
}


def beat(buf, pattern, pos, step, rng):
    for i, ch in enumerate(pattern):
        for sound, level in DRUMS.get(ch, ()):
            place(buf, level * sound(rng), round((pos + i) * step))
#endregion

#region 악보 — 타이틀 (D장조 126 BPM, 28마디)
ARP = {"gap": 0.02, "decay": 0.05, "sustain": 0.45}    # 짧게 튕기는 반주
PICK = {"gap": 0.015, "decay": 0.15, "sustain": 0.5}   # 뜯는 분산화음
HARM = {"gap": 0.015, "decay": 0.25, "sustain": 0.6}   # 리드 아래 화음

T_HOOK = [
    "D5:3 E5:1 F#5:3 A5:1 D6:6 C#6:2",
    "B5:4 A5:2 F#5:2 A5:8",
    "B5:3 A5:1 G5:3 D5:1 G5:6 A5:2",
    "G5:4 E5:2 C#5:2 E5:8",
    "D5:3 E5:1 F#5:3 A5:1 D6:6 C#6:2",
    "B5:4 D6:2 E6:2 F#6:6 E6:2",
    "D6:4 B5:2 G5:2 E6:4 C#6:4",
    "D6:12 -:4",
]
TITLE = {
    "name": "BGM_BreezeForest", "bpm": 126, "steps": 16, "lo": (55, 39),   # lo = (펄스2 보이싱, 베이스) 루트 하한
    "mix": {"lead": 0.9, "pulse2": 0.35, "bass": 0.9, "drums": 1.0},
    "lead_kw": {"decay": 0.14, "vib": 0.009},
    "p2": {
        "arp": ("0:1 1:1 2:1 3:1 " * 4, 0.5, ARP),
        "gallop": ("0:2 1:1 2:1 3:2 1:1 2:1 " * 2, 0.5, ARP),
        "pick": ("0:2 2:2 1:2 3:2 " * 2, 0.5, PICK),
        "harm": (None, 0.5, HARM),
    },
    "bass": {"pedal": "R:2 8:2 " * 4, "walk": "R:2 5:2 8:2 5:2 " * 2, "lyric": "R:6 5:2 8:4 5:4", "drive": "R:2 R:2 8:2 R:2 " * 2},
    "drums": {
        "i":  "k...h...k...h...",
        "if": "k...h...s.s.sSSS",
        "ax": "X.h.S.h.K.hKS.h.",
        "a":  "K.h.S.h.K.hKS.h.",
        "af": "K.h.S.h.K.T.M.L.",
        "b":  "K.x.s.x.K.k.s.x.",
        "bf": "K.x.s.x.K.k.S.SS",
        "e":  "K.h.S.h.X.......",
    },
    "phrases": {   # 4마디 단위: (코드, 펄스2, 베이스, 드럼, 리드)
        "I": ("D E/D G/D A7sus4,A7", "arp", "pedal", "i i i if", [   # 리디안 II 로 해가 뜬다
            "F#5:6 A5:2 D6:8",
            "G#5:6 B5:2 E6:8",
            "D6:6 B5:2 G5:4 B5:4",
            "A5:6 D6:2 C#6:4 E5:4"]),
        "A1": ("D Bm7 G A7", "gallop", "walk", "ax a a a", T_HOOK[:4]),
        "A2": ("D Bm7 Em7,A7 D", "gallop", "walk", "a a a af", T_HOOK[4:]),
        "B1": ("Gmaj7 A F#m7 Bm7", "pick", "lyric", "b b b b", [
            "B5:6 A5:2 B5:4 D6:4",
            "C#6:6 B5:2 A5:8",
            "A5:6 E5:2 A5:4 C#6:4",
            "D6:6 C#6:2 B5:8"]),
        "B2": ("Em7 A/G Gm6 A7sus4,A7", "pick", "lyric", "b b b bf", [   # Gm6 의 Bb 이 그리움, C# 이 D 로 풀린다
            "G5:6 F#5:2 G5:4 B5:4",
            "A5:6 G5:2 A5:4 C#6:4",
            "D6:6 Bb5:2 G5:4 E5:4",
            "A5:6 G5:2 E5:4 C#5:4"]),
        "A3": ("D Bm7 G A7", "harm", "drive", "ax a a a", T_HOOK[:4]),
        "A4": ("D Bm7 Em7,A7 D", "harm", "drive", "a a a e", T_HOOK[4:]),
    },
    "form": [("I", .25, .85), ("A1", .25), ("A2", .25), ("B1", .5, .75), ("B2", .5, .75), ("A3", .25), ("A4", .25)],   # (구간, 리드 듀티[, 음량])
}
#endregion

#region 악보 — 마을 (F장조 6/8, 점4분 = 72 → 4분 108, 40마디)
V_A = [
    "A5:3 Bb5:1 A5:2 F5:4 C5:2",
    "D5:4 F5:2 Bb5:6",
    "A5:3 Bb5:1 A5:2 G5:4 D5:2",
    "E5:4 G5:2 C6:6",
    "A5:3 Bb5:1 A5:2 F5:4 C5:2",
    "D5:4 F5:2 C6:6",
    "D6:4 C6:2 Bb5:4 G5:2",
    "A5:4 G5:2 F5:6",
]
VILLAGE = {
    "name": "BGM_Village1", "bpm": 108, "steps": 12, "lo": (53, 41), "echo": 2,
    "mix": {"lead": 0.85, "pulse2": 0.4, "bass": 0.9, "drums": 0.75},
    "lead_kw": {"decay": 0.2, "sustain": 0.6, "vib": 0.012},
    "p2": {
        "roll": ("0:2 1:2 2:2 3:2 2:2 1:2", 0.5, {"gap": 0.015, "decay": 0.12, "sustain": 0.5}),
        "pa": ("-:2 1:2 2:2 -:2 1:2 2:2", 0.25, {"gap": 0.03, "decay": 0.06, "sustain": 0.3}),
        "counter": (None, 0.5, {"decay": 0.3, "sustain": 0.6, "vib": 0.006}),
    },
    "bass": {"lilt": "R:4 5:2 8:4 5:2", "calm": "R:6 5:6"},
    "drums": {
        "p":  "k.x.x.s.x.x.",
        "pf": "k.x.x.s.x.ss",
        "b":  "k.x.k.s.x.x.",
        "q":  "k.....x.x.x.",
        "qf": "k.....s.s.ss",
    },
    "phrases": {
        "A1": ("F Bb Gm7 C7", "roll", "lilt", "p p p p", V_A[:4]),
        "A2": ("F Dm7 Bb,C7 F", "roll", "lilt", "p p p pf", V_A[4:]),
        "A3": ("F Bb Gm7 C7", [   # 대선율: 리드가 길게 끄는 동안 움직인다
            "C5:6 A4:6",
            "Bb4:6 D5:2 C5:2 D5:2",
            "D5:6 Bb4:6",
            "Bb4:6 C5:2 D5:2 E5:2"], "lilt", "p p p p", V_A[:4]),
        "A4": ("F Dm7 Bb,C7 F,F7", [
            "F5:6 A4:6",
            "F4:6 A4:2 C5:2 A4:2",
            "D5:6 E5:6",
            "C5:6 A4:6"], "lilt", "p p p pf", V_A[4:7] + ["A5:4 G5:2 F5:4 Eb5:2"]),   # F7 의 Eb → B 의 D
        "B1": ("Bb F/A Gm7 C7", "pa", "lilt", "b b b b", [
            "F5:2 Bb5:2 D6:2 C6:4 Bb5:2",
            "A5:4 C6:2 F5:6",
            "G5:2 Bb5:2 D6:2 C6:4 Bb5:2",
            "Bb5:4 G5:2 E5:6"]),
        "B2": ("Bb Am7 Gm7 C7sus4,C7", "pa", "lilt", "b b b pf", [
            "F5:2 Bb5:2 D6:2 C6:4 Bb5:2",
            "C6:4 E6:2 A5:6",
            "Bb5:2 A5:2 G5:2 A5:4 Bb5:2",
            "C6:6 Bb5:4 G5:2"]),
        "A5": ("F Bb Gm7 C7", "echo", "lilt", "p p p p", V_A[:4]),
        "A6": ("F Dm7 Bb,C7 F", "echo", "lilt", "p p p pf", V_A[4:]),
        "C1": ("Dm Gm A7 Dm", "roll", "calm", "q q q q", [   # A 동기를 단조로
            "F5:3 G5:1 F5:2 D5:4 A4:2",
            "Bb4:4 D5:2 G5:6",
            "E5:3 F5:1 E5:2 C#5:4 A4:2",
            "D5:4 F5:2 A5:6"]),
        "C2": ("Bb Gm7 Bb,C7 C7sus4,C7", "roll", "calm", "q q q qf", [
            "D6:3 C6:1 Bb5:2 F5:4 D5:2",
            "Bb5:4 A5:2 G5:6",
            "F5:4 D5:2 E5:4 G5:2",
            "C6:6 G5:4 Bb5:2"]),
    },
    "form": [("A1", .5, .9), ("A2", .5, .9), ("A3", .5, .85), ("A4", .5, .85), ("B1", .25), ("B2", .25),
             ("A5", .25), ("A6", .25), ("C1", .5, .8), ("C2", .5, .8)],
}
#endregion

#region 악보 — 필드 (A장조 144 BPM, 40마디)
F_HOOK = [
    "-:2 E5:2 A5:2 B5:2 C#6:4 A5:4",
    "D6:2 C#6:2 B5:2 A5:2 F#5:4 A5:4",
    "-:2 C#5:2 F#5:2 G#5:2 A5:4 F#5:4",
    "G#5:2 F#5:2 E5:2 F#5:2 G#5:4 B5:4",
    "-:2 E5:2 A5:2 B5:2 C#6:4 E6:4",
    "F#6:2 E6:2 D6:2 C#6:2 B5:4 A5:4",
    "B5:2 C#6:2 D6:4 E6:2 D6:2 B5:4",
    "A5:12 -:4",
]
F_B = [
    "F#5:2 A5:4 F#5:2 B5:4 A5:4",
    "G#5:2 B5:4 G#5:2 C#6:4 B5:4",
    "E5:2 G#5:4 E5:2 B5:4 G#5:4",
    "A5:2 C#6:4 A5:2 F#5:8",
    "D6:2 C#6:2 B5:2 A5:2 B5:4 F#5:4",
    "G#5:2 A5:2 B5:2 E6:6 D6:4",
    "C#6:4 E6:4 D6:4 F#6:4",
    "E6:6 D6:2 B5:4 G#5:4",
]
F_C = [
    "A5:8 F#5:4 E5:4",
    "G#5:8 E5:4 C#5:4",
    "F#5:8 D5:4 B4:4",
    "C#5:4 E5:4 A5:8",
    "C#6:8 A5:4 F#5:4",
    "B5:8 G#5:4 E5:4",
    "A5:8 F#5:4 D5:4",
    "E5:4 A5:4 G#5:4 B5:4",
]
FIELD = {
    "name": "BGM_BWF", "bpm": 144, "steps": 16, "lo": (55, 40),   # 펄스2 루트 G3~ — E·F#m 화음이 베이스에 묻히지 않게
    "mix": {"lead": 0.9, "pulse2": 0.35, "bass": 0.9, "drums": 0.9},
    "lead_kw": {"decay": 0.12, "sustain": 0.65, "vib": 0.008},
    "p2": {
        "pump": ("0:2 2:2 1:2 2:2 " * 2, 0.5, {"gap": 0.02, "decay": 0.06, "sustain": 0.4}),
        "off": ("-:2 c:2 " * 4, 0.5, {"gap": 0.03, "decay": 0.05, "sustain": 0.3}),
        "arp": ("0:1 1:1 2:1 3:1 " * 4, 0.5, ARP),
        "pick": ("0:2 2:2 1:2 3:2 " * 2, 0.5, PICK),
        "harm": (None, 0.5, HARM),
    },
    "bass": {"drive": "R:2 8:2 " * 4, "walk": "R:2 5:2 8:2 5:2 " * 2, "calm": "R:4 5:4 8:4 5:4"},
    "drums": {
        "ax": "X.x.S.x.K.xKS.x.",
        "a":  "K.x.S.x.K.xKS.x.",
        "af": "K.x.S.x.K.xKS.SS",
        "bx": "X.x.S.xKK.x.S.x.",
        "b":  "K.x.S.xKK.x.S.x.",
        "bf": "K.x.S.x.K.T.M.L.",
        "c":  "k...x...k...x...",
        "cf": "k...x...s.s.sSSS",
    },
    "phrases": {
        "A1": ("A D F#m E", "pump", "drive", "ax a a a", F_HOOK[:4]),
        "A2": ("A D Bm7,E7 A", "pump", "drive", "a a a af", F_HOOK[4:]),
        "A3": ("A D F#m E", "harm", "walk", "ax a a a", F_HOOK[:4]),
        "A4": ("A D Bm7,E7 A", "harm", "walk", "a a a af", F_HOOK[4:]),
        "B1": ("D E C#m7 F#m", "off", "walk", "bx b b b", F_B[:4]),
        "B2": ("Bm7 E A/C#,D E7", "off", "walk", "b b b af", F_B[4:]),
        "B3": ("D E C#m7 F#m", "arp", "drive", "bx b b b", F_B[:4]),
        "B4": ("Bm7 E A/C#,D E7", "arp", "drive", "b b b bf", F_B[4:]),
        "C1": ("Dmaj7 C#m7 Bm7 A", "pick", "calm", "c c c c", F_C[:4]),   # 쉬어 가기
        "C2": ("Dmaj7 C#m7 Bm7 E7sus4,E7", "pick", "calm", "c c c cf", F_C[4:]),
    },
    "form": [("A1", .25), ("A2", .25), ("A3", .25), ("A4", .25), ("B1", .5, .8), ("B2", .5, .8),
             ("B3", .25), ("B4", .25), ("C1", .5, .6), ("C2", .5, .6)],
}
#endregion

#region 악보 — 보스 (C단조 184 BPM, 32마디)
B_A = [
    "C5:2 C6:2 B5:2 G5:2 Ab5:4 G5:2 F5:2",
    "Eb5:2 F5:2 Eb5:2 D5:2 C5:4 G4:4",
    "C5:2 C6:2 Bb5:2 Ab5:2 G5:4 F5:2 Eb5:2",
    "F5:4 D5:2 Bb4:2 B4:4 D5:4",
    "C5:2 C6:2 B5:2 G5:2 Ab5:4 G5:2 F5:2",
    "Ab5:2 C6:2 Bb5:2 Ab5:2 G5:4 F5:2 Ab5:2",
    "F5:4 Ab5:4 B5:4 D6:4",
    "C6:8 -:4 G4:2 G4:2",
]
BOSS = {
    "name": "BGM_Boss", "bpm": 184, "steps": 16, "lo": (55, 36),
    "mix": {"lead": 0.85, "pulse2": 0.4, "bass": 1.0, "drums": 1.15},
    "lead_kw": {"decay": 0.1, "sustain": 0.75, "vib": 0.007},
    "p2": {
        "ost": ("0:1 1:1 2:1 1:1 " * 4, 0.25, {"gap": 0.012, "decay": 0.04, "sustain": 0.45}),
        "harm": (None, 0.5, HARM),
        "counter": (None, 0.25, {"gap": 0.01, "decay": 0.06, "sustain": 0.55}),
    },
    "bass": {"gallop": "R:1 R:1 8:2 " * 4, "oct": "R:2 8:2 " * 4, "drive": "R:2 R:2 8:2 R:2 " * 2},
    "drums": {
        "rx": "X.h.S.hKK.h.S.hK",
        "r":  "K.h.S.hKK.h.S.hK",
        "dx": "XhhhShhKKhhhShhK",
        "d":  "KhhhShhKKhhhShhK",
        "f":  "K.S.S.SSTTMMLLKK",
    },
    "phrases": {
        "I": ("Cm Cm Db7 G7", [   # 말발굽 페달 리프 — 리드가 윗선을 한 옥타브 위에서 겹친다
            "C4:1 C4:1 C5:2 C4:1 C4:1 Bb4:2 C4:1 C4:1 G4:2 Ab4:2 G4:2",
            "C4:1 C4:1 C5:2 C4:1 C4:1 Bb4:2 C4:1 C4:1 Eb5:2 D5:2 B4:2",
            "Db4:1 Db4:1 Db5:2 Db4:1 Db4:1 B4:2 Db4:1 Db4:1 Ab4:2 Bb4:2 Ab4:2",
            "G3:1 G3:1 G4:2 G3:1 G3:1 F4:2 G3:1 G3:1 B3:2 D4:2 F4:2"], "gallop", "rx r r f", [
            "-:2 C6:4 Bb5:4 G5:2 Ab5:2 G5:2",
            "-:2 C6:4 Bb5:4 Eb6:2 D6:2 B5:2",
            "-:2 Db6:4 B5:4 Ab5:2 Bb5:2 Ab5:2",
            "-:2 G5:4 F5:4 B4:2 D5:2 F5:2"]),
        "A1": ("Cm Cm Ab,Fm7 Bb,G7", "ost", "oct", "rx r r r", B_A[:4]),
        "A2": ("Cm Fm Db,G7 Cm", "ost", "oct", "r r r f", B_A[4:]),
        "A3": ("Cm Cm Ab,Fm7 Bb,G7", "harm", "gallop", "dx d d d", B_A[:4]),
        "A4": ("Cm Fm Db,G7 Cm", "harm", "gallop", "d d d f", B_A[4:7] + ["C6:12 -:4"]),
        "B1": ("Ab Bb Cm Cm", "ost", "drive", "rx r r r", [
            "Eb5:4 Ab5:4 C6:8",
            "D6:6 C6:2 Bb5:8",
            "Eb6:6 D6:2 C6:4 G5:4",
            "C6:8 Bb5:4 G5:4"]),
        "B2": ("Ab Bb Db G7", "ost", "drive", "r r r f", [
            "Ab5:4 C6:4 Eb6:8",
            "F6:6 Eb6:2 D6:8",
            "F6:4 Eb6:4 Db6:4 Ab5:4",
            "G5:4 B5:4 D6:4 F6:4"]),
        "T": ("Ab Bb Bdim7 Bdim7/G", "ost", "gallop", "rx r r f", [   # 정점에서 내려왔다 감7 로 다시 조여 B5 → C6
            "Eb6:4 C6:4 Ab5:4 Eb5:4",
            "D6:4 Bb5:4 F5:4 D5:4",
            "B4:4 D5:4 F5:4 Ab5:4",
            "D5:4 F5:4 Ab5:4 B5:4"]),
    },
    "form": [("I", .125, 1.1), ("A1", .25), ("A2", .25), ("A3", .125, 1.15), ("A4", .125, 1.15), ("B1", .25), ("B2", .25), ("T", .25)],
}
#endregion

SUITE = [TITLE, VILLAGE, FIELD, BOSS]

#region 렌더
def render(t):
    """곡 → (원형 루프 모노 파형(마스터링 전), (음표 온셋/초, 드럼 타격/초))."""
    rng = np.random.default_rng(SEED)
    steps, step = t["steps"], SR * 60 / t["bpm"] / 4
    total = round(4 * len(t["form"]) * steps * step)
    tr = {k: np.zeros(total + 4 * SR) for k in t["mix"]}
    lo2, lob = t["lo"]
    notes = hits = pos = 0
    for name, duty, *vol in t["form"]:
        chords, p2, bass, drums, lead = t["phrases"][name]
        lv = vol[0] if vol else 1.0   # 구간별 리드 음량 (NES 채널 볼륨처럼)
        mode = "counter" if isinstance(p2, list) else p2
        pat2, duty2, kw2 = t["p2"].get(mode, (None, duty, t["lead_kw"]))   # 메아리는 리드 음색 그대로
        ev = {"lead": [], "pulse2": [], "bass": []}
        for i, (syms, dr) in enumerate(zip([c.split(",") for c in chords.split()], drums.split(), strict=True)):
            bar = parse(lead[i], steps)
            ev["lead"] += bar
            if mode == "counter":
                ev["pulse2"] += parse(p2[i], steps)
            elif mode == "harm":
                ev["pulse2"] += harmony(bar, syms, steps)
            elif mode != "echo":
                ev["pulse2"] += realize(pat2, syms, steps, lambda s, k: comp_tone(s, k, lo2))
            ev["bass"] += parse(bass[i], steps) if isinstance(bass, list) else \
                realize(t["bass"][bass], syms, steps, lambda s, k: bass_tone(s, k, lob))
            pat = t["drums"][dr]
            assert len(pat) == steps, f"드럼 {dr}: {len(pat)}칸 != {steps}칸"
            beat(tr["drums"], pat, pos + i * steps, step, rng)
            hits += sum(c in DRUMS for c in pat)
        if mode == "echo":
            ev["pulse2"] = delayed(ev["lead"], t["echo"], 4 * steps)
        level2 = ECHO * lv if mode == "echo" else 1.0
        play(tr["lead"], ev["lead"], pos, step, lambda f: lv * pulse(f, duty), **t["lead_kw"])
        play(tr["pulse2"], ev["pulse2"], pos, step, lambda f: level2 * pulse(f, duty2), **kw2)
        play(tr["bass"], ev["bass"], pos, step, triangle, gap=0.04, sustain=1.0)
        notes += sum(m is not None for v in ev.values() for m, _ in v)
        pos += 4 * steps
    mix = sum(t["mix"][k] * tr[k] for k in t["mix"])
    mix[:4 * SR] += mix[total:]   # 원형 루프: 끝을 넘친 꼬리(드럼·크래시)를 맨 앞에 겹친다
    mix = mix[:total]
    sec = total / SR
    return lowpass(mix - mix.mean(), loop=True), (notes / sec, hits / sec)


def spectral(y):
    """10ms 간격 로그 스펙트럼의 양(+)의 변화량 평균(플럭스 — 소리가 얼마나 자주·크게 바뀌나)과 스펙트럼 중심(Hz)."""
    n = 1024
    mag = np.abs(np.fft.rfft(np.lib.stride_tricks.sliding_window_view(y, n)[::441] * np.hanning(n)))
    p = (mag ** 2).sum(0)
    return float(np.maximum(np.diff(np.log1p(mag), axis=0), 0).sum(1).mean()), float(p @ np.fft.rfftfreq(n, 1 / SR) / p.sum())
#endregion


def main():
    ap = argparse.ArgumentParser(description="타이틀·마을·필드·보스 BGM 생성")
    ap.add_argument("--out", default=OUT_DIR, help="WAV 를 쓸 폴더 (기본: Assets/@Resources/Sound/BGM)")
    ap.add_argument("--preview", metavar="DIR", help="루프를 2회 이어 붙인 확인용 wav 를 쓸 폴더")
    a = ap.parse_args()
    if not FFMPEG:
        sys.exit("ffmpeg 가 PATH 에 없다")
    os.makedirs(a.out, exist_ok=True)
    stats = {}
    for t in SUITE:
        x, dens = render(t)
        path = os.path.join(a.out, t["name"] + ".wav")
        y = master(x, -16, path)
        report(os.path.basename(path), y, loop=True)
        stats[t["name"]] = (*dens, *spectral(y))
        print("  intensity: {:.1f} notes/s  {:.1f} drum hits/s  flux {:.1f}  centroid {:.0f} Hz".format(*stats[t["name"]]))
        if a.preview:
            os.makedirs(a.preview, exist_ok=True)
            out = os.path.join(a.preview, t["name"] + "_x2.wav")
            ff(["-y", *RAW, "-i", "-", out], np.tile(y, 2))
            print("  preview:", out)
    field, boss = stats[FIELD["name"]], stats[BOSS["name"]]
    assert all(f < b for f, b in zip(field[:3], boss[:3])), f"필드가 보스보다 약하지 않다: {field} vs {boss}"


if __name__ == "__main__":
    main()
