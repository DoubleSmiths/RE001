# -*- coding: utf-8 -*-
"""효과음 56종 — 자체 합성 칩튠 SFX 생성기.

## 왜
  기존 효과음 56개는 출처·라이선스를 알 수 없어 상업 출시에 쓸 수 없다. 이 스크립트가 **원천**이 되도록 전부 새로 합성한다.
  기존 파일은 길이만 쟀고 내용은 열지도 참고하지도 않았다 — 소리는 쓰임(어느 코드가 언제 부르나)만 보고 설계했다.
  BGM(gen_chiptune.py · gen_bgm_suite.py)과 같은 NES 풍 팔레트·부품을 쓴다.
  SFX_MinimonCaught(gen_chiptune.py 가 만든다)와 BGM 은 대상이 아니다.

## 파일 이름·확장자·폴더는 그대로
  Unity .meta GUID 와 어드레서블 키(= 파일 이름)가 바뀌면 안 된다. SOUNDS 의 경로는 지금 파일 그대로이고,
  --write 는 이미 있는 파일만 덮어쓴다(없으면 실패). .wav = 16bit PCM 모노 44.1kHz,
  .mp3 = 같은 WAV 를 ffmpeg libmp3lame 128kbps 로 굽는다.

## 팔레트
  펄스(12.5/25/50%, PolyBLEP) · NES 4비트 삼각파(저음·몸통) · 1비트 노이즈(레이트를 쓸면 '휙') · 피치 스윕 ·
  60Hz 한 채널 화음 아르페지오 · 딜레이 비브라토 · 메아리. 펄스·삼각파·출력단 저역통과·악보(parse/play/tone)는
  gen_chiptune 부품을 그대로 쓴다. 1kHz 위 삼각파(종·물방울·휘파람)만 BLEP 사각파를 적분한 대역 제한판(tri_hi) —
  NES 삼각파의 계단이 높은 음에서 접혀 끽끽거린다.
  금속 = 비정수 배음비의 펄스쌍(살짝 어긋나 맥놀이) + 링변조 삼각파. 묵직한 타격 = 저음 사각파의 홀수 배음
  (폰 스피커가 100Hz 아래를 못 내도 무게가 들린다). 90Hz 아래는 마스터링에서 걷어 낸다.

## 음량 (finish)
  90Hz 아래(DC·서브베이스) 제거 → 12kHz 출력단 저역통과(BGM 과 같은 부품) → 꼬리 무음 자르기 → 페이드 3ms/5ms
  → 피크 -1dBFS 정규화 → 분류별 RMS 목표까지 **내린다**(올리지 않는다 — 피크가 먼저 막으면 목표보다 조용하다).
    ui -18 dBFS RMS (버튼·블립·발소리)  ·  hit -14 (타격·충돌)  ·  jingle -16 (징글·팡파르·스팅)
  목표는 **가장 센 0.2초 구간의 RMS**(loud)에 건다(풀스케일 사인 = -3dB). 파일 전체 RMS 로 맞추면 긴 울림 꼬리가 있는
  모루·종은 과하게 커지고 짧은 타격만 작아진다. 표에는 파일 전체 RMS 와 loud 를 둘 다 찍는다.
  MP3 는 구운 뒤 디코드해 다시 잰다 — 128kbps 가 고역을 깎아 약 0.4dB 작아지고 피크는 굽기마다 조금 흔들리므로,
  목표 음량과 피크 ≤ -1dBFS 를 둘 다 만족할 때까지 게인을 고쳐 다시 굽는다(gen_chiptune.master 와 같은 방식).

## 강화 연출 (UI_EnhanceProgressPopup) — 겹쳐 울리는 소리는 조를 맞춘다
  대장장이 클립 이벤트: SmashSound 0.083/1.083/2.083초(판이 떠는 Shake 는 각 +0.25초), Curious 0.25,
  Serious 3.75, Success 3.833 | Fail 3.9, 클립이 끝나면 결과 화면에서 SFX_Upgrade | SFX_Fail.
  → EnhanceHammer 는 0.25초에 '땡'(앞은 휘두르는 바람), Serious 는 3음 없는 C 5도(C장조 성공·C단조 실패 모두와 어울림),
    Success(Base)·Upgrade 는 C장조, Fail 은 Eb → C.

## 사용
  python _tools/gen_sfx.py                         # 계획만 출력(아무것도 쓰지 않는다)
  python _tools/gen_sfx.py --preview DIR           # DIR 에 상대 경로 그대로 + 측정 표 + DIR/overview.png(스펙트로그램 모음)
  python _tools/gen_sfx.py --write                 # Assets/@Resources/Sound 에 덮어쓴다
  python _tools/gen_sfx.py --preview DIR --only SFX_BtnSelect,Slime_OnHit
  필요: numpy, PATH 의 ffmpeg(libmp3lame), (overview 만) Pillow. 소리마다 시드 고정 → 두 번 돌려도 같다.
"""
import argparse
import os
import subprocess
import sys
import tempfile
import wave
import zlib

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gen_chiptune import (BASE, FFMPEG, SEED, SR, crash, hz, lowpass, midi, parse, play, pulse,   # noqa: E402
                          snare, square, triangle)

SOUND = os.path.join(BASE, "Assets", "@Resources", "Sound")
TARGET = {"ui": -18.0, "hit": -14.0, "jingle": -16.0}   # 분류별 RMS 목표(dBFS, 가장 센 0.2초 — loud_db)
BIG = {"SFX_NewContents", "SFX_Success(Base)", "SFX_Success2"}   # 3.5초까지 허용하는 '큰 순간' 징글, 나머지는 3.0초

#region 부품
def n_(sec):
    return int(round(sec * SR))


def t_(sec):
    return np.arange(n_(sec)) / SR


def N(name):
    """'C6' → Hz."""
    return float(hz(midi(name)))


def const(f, sec):
    return np.full(n_(sec), float(f))


def glide(f0, f1, sec, curve=1.0):
    """f0 → f1 지수 스윕(음정으로는 직선). curve > 1 이면 끝에서 빨라진다."""
    return f0 * (f1 / f0) ** (np.linspace(0, 1, n_(sec)) ** curve)


def fade_end(x, sec=0.003):
    """끝 3ms 를 0 으로 — 잘린 자리에서 딸깍하지 않게."""
    k = min(n_(sec), x.size)
    x[-k:] *= np.linspace(1, 0, k)
    return x


def env(sec, a=0.002, tau=0.1, hold=0.0):
    """a 초 직선 어택 → hold 초 유지 → 시정수 tau 지수 감쇠."""
    t = t_(sec)
    return fade_end(np.minimum(t / a, 1.0) * np.exp(-np.maximum(t - a - hold, 0.0) / tau))


def swell(sec, peak, tau):
    """peak 초까지 2차 곡선으로 차올랐다가 지수 감쇠 — 바람·부풂."""
    t = t_(sec)
    return fade_end(np.where(t < peak, (t / peak) ** 2, np.exp(-(t - peak) / tau)))


def nz(rng, rate, sec=None):
    """1비트 샘플앤홀드 노이즈(NES 노이즈 채널). rate = 상수(sec 필요) 또는 샘플별 배열 —
    레이트를 쓸어 올리면 '휘익', 낮추면 어두워진다."""
    rate = const(rate, sec) if sec is not None else np.asarray(rate, float)
    idx = np.cumsum(rate / SR).astype(int)
    return rng.choice((-0.5, 0.5), idx[-1] + 1)[idx]


def band(x, lo=None, hi=None, width=0.5):
    """FFT 대역 필터(0위상). lo 아래·hi 위를 width 옥타브 코사인 램프로 깎는다. lo 를 주면 DC 도 0.
    ponytail: 고정 차단 주파수만 — 시간에 따라 변하는 음색은 노이즈 레이트 스윕으로 낸다."""
    m = 1 << int(np.ceil(np.log2(x.size + SR // 4)))   # 0.25초 여유 — 원형 합성곱 꼬리가 앞으로 되감기지 않게
    lf = np.log2(np.maximum(np.fft.rfftfreq(m, 1 / SR), 1e-3))
    g = np.ones_like(lf)
    if lo:
        g *= 0.5 - 0.5 * np.cos(np.pi * np.clip((lf - np.log2(lo)) / width + 1, 0, 1))
    if hi:
        g *= 0.5 - 0.5 * np.cos(np.pi * np.clip((np.log2(hi) - lf) / width + 1, 0, 1))
    return np.fft.irfft(np.fft.rfft(x, m) * g, m)[:x.size]


def mix(*parts):
    """(시작 초, 파형[, 게인]) 조각들을 더한다. 길이 = 가장 늦게 끝나는 조각."""
    b = np.zeros(max(n_(p[0]) + p[1].size for p in parts))
    for at, x, *g in parts:
        b[n_(at):n_(at) + x.size] += (g[0] if g else 1.0) * x
    return b


def echo(x, d=0.1, g=0.4, k=3):
    """d 초 간격으로 k 번 되울리는 메아리(한 번마다 g 배)."""
    return mix(*[(d * i, x, g ** i) for i in range(k + 1)])


def melody(spec, step, osc=square, **kw):
    """'C6:1 E6:1 G6:4' → 파형(한 칸 = step 초). gen_chiptune 의 parse·play·tone 그대로라
    'C5+E5+G5' 한 채널 화음(60Hz 아르페지오)·딜레이 비브라토(vib)도 된다."""
    steps = sum(int(tok.split(":")[1]) for tok in spec.split())
    b = np.zeros(n_(steps * step) + 2)   # +2: 칸 경계 반올림 여유
    play(b, parse(spec, steps), 0, step * SR, osc, **kw)
    return b


def jingle(run, last, step, osc, ring=0.35, **kw):
    """달리는 음들(run) + 서스테인 없이 ring 시정수로 자연 감쇠하는 마지막 음(last) — gen_chiptune 징글과 같은 방식."""
    tail = melody(last, step, osc, **{**kw, "gap": 0.0, "decay": ring, "sustain": 0.0, "release": 0.03})
    return np.concatenate([melody(run, step, osc, **kw), tail]) if run else tail


def p12(f):
    return pulse(f, 0.125)


def p25(f):
    return pulse(f, 0.25)


def tri_hi(f):
    """높은 음용 삼각파: BLEP 사각파를 적분한 대역 제한 삼각파. NES 4비트 삼각파는 1kHz 위에서 계단이 접혀 끽끽거린다
    (저음·몸통은 NES 삼각파 그대로)."""
    y = np.cumsum(square(f) * 4 * f / SR)
    return y - y.mean()


def blip(f0, sec, duty=0.25, tau=None, f1=None):
    """짧은 펄스음. f1 을 주면 f0 → f1 로 휜다."""
    return pulse(glide(f0, f1 or f0, sec), duty) * env(sec, 0.001, tau or sec / 3)


def bubble(f0, sec=0.05, rise=1.8):
    """물방울 '뽁': 위로 휘는 삼각파."""
    return tri_hi(glide(f0, f0 * rise, sec, 0.7)) * env(sec, 0.001, sec / 3)


def thump(f0, f1, sec, tau=0.08, drop=0.03):
    """타격의 몸통: 삼각파 피치 드롭 f0 → f1 (폰 스피커를 위해 100Hz 근처 이상)."""
    t = t_(sec)
    return triangle(f1 + (f0 - f1) * np.exp(-t / drop)) * env(sec, 0.001, tau)


def whoosh(rng, sec, r0, r1, peak=0.5, tau=0.1, lo=None, hi=None):
    """바람 소리: 노이즈 레이트를 r0 → r1 로 쓸고 peak(길이 비율)에서 가장 세다."""
    x = nz(rng, glide(r0, r1, sec)) * swell(sec, peak * sec, tau)
    return band(x, lo, hi) if lo or hi else x


def osc_at(osc, f, sec, ph):
    """고정 음정 f 를 위상 ph(주기 비율)에서 시작 — 부분음들이 같은 위상으로 한꺼번에 튀어 피크만 솟지 않게."""
    k = int(ph * SR / f)
    return osc(const(f, sec + k / SR))[k:]


def clang(f0, sec, tau=0.3, ratios=(1.0, 1.47, 2.09, 2.76, 3.52), duty=0.25, beat=0.003, bite=1.0):
    """금속 울림: 비정수 배음비의 펄스쌍(살짝 어긋나 맥놀이), 높은 배음일수록 빨리 사그라든다.
    부분음마다 시작 위상을 황금비로 흩는다. bite = 어택의 링변조 삼각파('챙') 세기."""
    t = t_(sec)
    y = np.zeros(t.size)
    for i, r in enumerate(ratios):
        e = np.exp(-t / (tau / (1 + 0.6 * i))) * 0.7 ** i
        for j, s in enumerate((-1, 1)):
            y += e * osc_at(lambda f: pulse(f, duty), f0 * r * (1 + s * beat * (1 + i)), sec, 0.618 * (2 * i + j) % 1)
    ring = osc_at(tri_hi, f0 * 0.73, sec, 0.25) * osc_at(tri_hi, f0 * 2.31, sec, 0.6) * np.exp(-t / 0.025)
    return fade_end((y / len(ratios) + 2.0 * bite * ring) * np.minimum(t / 0.0015, 1.0))


def bell(f0, sec, tau=0.3):
    """맑은 종: 삼각파 기음 + 12.5% 펄스 옥타브 + 2.76배 삼각파(금방 사그라든다)."""
    t = t_(sec)
    y = (tri_hi(const(f0, sec)) * np.exp(-t / tau)
         + 0.45 * osc_at(p12, 2 * f0, sec, 0.5) * np.exp(-t / (0.6 * tau))
         + 0.35 * tri_hi(const(2.76 * f0, sec)) * np.exp(-t / (0.25 * tau)))
    return fade_end(y * np.minimum(t / 0.001, 1.0))


def chord_pad(notes, sec, peak, tau, osc=square):
    """한 채널 화음(60Hz 로 구성음을 돈다)이 peak 초까지 부풀었다 사라진다."""
    t = t_(sec)
    return osc(np.array([N(x) for x in notes])[(t * 60).astype(int) % len(notes)]) * swell(sec, peak, tau)


def sparkle(rng, notes, step, sec, duty=0.125, tail=0.06):
    """반짝이: notes 중 무작위 음 알갱이를 step 초마다 뿌리고 점점 여리게."""
    return mix(*[(i * step, blip(N(notes[rng.integers(len(notes))]), tail, duty, tau=0.015), (1 - i * step / sec) ** 1.5)
                 for i in range(int(sec / step))])


def crackle(rng, sec, per_sec, rate=14000, tau=0.003):
    """드문드문 튀는 노이즈 알갱이(불똥·바삭)."""
    return mix((sec, np.zeros(1)), *[(at, nz(rng, rate, 0.02) * env(0.02, 0.0003, tau), rng.uniform(0.3, 1.0))
                                     for at in rng.uniform(0, sec, int(per_sec * sec))])


def drops(rng, n, sec, lo=700, hi=2200, start=0.05):
    """흩어지는 물방울: 앞에 몰리고 뒤로 갈수록 드물고 여리게."""
    ats = start + (sec - start - 0.08) * (np.arange(n) / n) ** 1.4 + rng.uniform(0, 0.02, n)
    return mix(*[(at, bubble(rng.uniform(lo, hi), rng.uniform(0.03, 0.06), rng.uniform(1.4, 2.2)), 1 - at / sec) for at in ats])


def boom(rng, sec=0.5):
    """묵직한 충격: 저음 사각파 피치 드롭(홀수 배음이 폰에서도 무게로 들린다) + 삼각파 몸통 + 부서지는 노이즈."""
    t = t_(sec)
    body = square(95 + 160 * np.exp(-t / 0.05)) * env(sec, 0.001, 0.11)
    crunch = band(nz(rng, glide(9000, 1500, sec)) * env(sec, 0.001, 0.07), 200, 6000)
    return mix((0, body, 0.7), (0, thump(320, 100, sec, 0.12, 0.03), 0.9), (0, crunch, 0.8))


SOUNDS = []   # (Sound 기준 상대 경로, 분류, 원래 길이, 목표 길이, 합성 함수)


def sfx(path, cat, was, aim=None):
    def reg(fn):
        SOUNDS.append((path, cat, was, aim or was, fn))
        return fn
    return reg
#endregion

#region 물약
def gulp(f):
    """꿀꺽: 아래로 휘는 삼각파."""
    return triangle(glide(f, f * 0.45, 0.09, 0.6)) * env(0.09, 0.003, 0.03)


def bubbles(rng, freqs, gap=0.035):
    return mix(*[(gap * i + rng.uniform(0, 0.01), bubble(f, 0.045), 1.0) for i, f in enumerate(freqs)])


@sfx("SFX/Item/Potion/SFX_RedPotion.mp3", "ui", 0.42)
def red_potion(rng):   # 꿀꺽 + 뽀글 셋 + 솔→레 로 부드럽게 오르는 회복음
    heal = square(glide(N("G5"), N("D6"), 0.2, 0.8)) * env(0.2, 0.006, 0.06)
    return mix((0, gulp(420), 0.9), (0.05, bubbles(rng, (620, 820, 700)), 0.5), (0.17, heal, 0.35))


@sfx("SFX/Item/Potion/SFX_RedPotion2.mp3", "ui", 0.42)
def red_potion2(rng):   # 더 낮은 꿀꺽 + 뽀글 둘 + '미→라' 두 음
    heal = melody("E6:1 A6:4", 0.045, p25, gap=0.004, decay=0.06, sustain=0.15)
    return mix((0, gulp(380), 0.9), (0.05, bubbles(rng, (900, 1100)), 0.5), (0.14, heal, 0.35))


@sfx("SFX/Item/Potion/SFX_RedPotion3.mp3", "ui", 0.35)
def red_potion3(rng):   # 꿀꺽 없이 뽀글 넷 + 한 옥타브 미끄러져 오르는 삼각파 + 끝의 반짝
    heal = triangle(glide(N("C5"), N("C6"), 0.14, 0.7)) * env(0.14, 0.004, 0.05)
    return mix((0, bubbles(rng, (700, 950, 800, 1150), 0.03), 0.6), (0.13, heal, 0.6), (0.2, blip(N("C7"), 0.08, 0.125), 0.15))


def speed(rng, starts, notes, sec):
    """빠르게 차오르는 '쉬익' + 가속하는 펄스 스윕(C4→C7) → 반짝이."""
    parts = []
    for at in starts:
        parts.append((at, whoosh(rng, 0.38, 1500, 16000, peak=0.8, tau=0.06, lo=400), 0.8))
        parts.append((at + 0.05, p12(glide(N("C4"), N("C7"), 0.3, 1.6)) * swell(0.3, 0.27, 0.02), 0.3))
    return mix(*parts, (starts[-1] + 0.33, sparkle(rng, notes, 0.03, sec), 0.45))


@sfx("SFX/Item/Potion/SFX_SpeedPotion.mp3", "jingle", 1.14)
def speed_potion(rng):
    return speed(rng, (0.0,), ("C7", "E7", "G7", "C8"), 0.7)


@sfx("SFX/Item/Potion/SFX_SpeedPotion2.mp3", "jingle", 1.35)
def speed_potion2(rng):   # 두 번 몰아치는 바람 + D장조 반짝이
    return speed(rng, (0.0, 0.2), ("A6", "D7", "F#7", "A7"), 0.75)
#endregion

#region 몬스터 (503종이 전부 Slime_* 를 같이 쓴다)
@sfx("SFX/Monster/Slime_Attack.mp3", "hit", 0.69)
def slime_attack(rng):   # 달려드는 바람 + 작은 '철퍽' + 으르렁(올라갔다 내려오는 25% 펄스, 28Hz 떨림) + 거친 숨
    sec = 0.55
    t = t_(sec)
    wob = np.sin(2 * np.pi * 28 * t)
    growl = pulse(150 * (1 + 0.5 * np.sin(np.pi * np.clip(t / 0.4, 0, 1))) * (1 + 0.05 * wob), 0.25) * (1 + 0.35 * wob) * swell(sec, 0.06, 0.15)
    breath = band(nz(rng, glide(3000, 6000, sec)) * swell(sec, 0.05, 0.12), 300, 3500)
    squelch = triangle(glide(320, 160, 0.07)) * env(0.07, 0.002, 0.02)
    return mix((0, whoosh(rng, 0.16, 2000, 9000, peak=0.8, tau=0.03, lo=500), 0.5), (0.04, squelch, 0.5),
               (0.06, growl, 0.9), (0.06, breath, 0.45))


@sfx("SFX/Monster/Slime_Die.mp3", "hit", 0.60)
def slime_die(rng):   # '뽁'(위로 튀는 삼각파 + 노이즈 톡) → 뚝 떨어지는 25% 펄스 → 흙먼지. 가장 자주 듣는 보상음이라 짧고 둥글게
    pop = triangle(glide(250, 1100, 0.035, 0.6)) * env(0.035, 0.0005, 0.015)
    click = nz(rng, 16000, 0.012) * env(0.012, 0.0003, 0.002)
    fall = p25(glide(1300, 240, 0.24, 0.8)) * env(0.24, 0.002, 0.09)
    dust = band(nz(rng, 5000, 0.5) * env(0.5, 0.01, 0.12), 400, 4000)
    return mix((0, click, 0.6), (0, pop, 1.0), (0.035, fall, 0.55), (0.05, dust, 0.5))


@sfx("SFX/Monster/Slime_Move.mp3", "ui", 0.56)
def slime_move(rng):   # 말랑한 한 걸음: 눌리는 '뭉'(아래로) + 되부푸는 '뽀'(위로) + 잔떨림
    squash = triangle(glide(320, 170, 0.14, 0.7)) * env(0.14, 0.004, 0.05)
    wet = band(nz(rng, 3500, 0.14) * env(0.14, 0.003, 0.03), 250, 2500)
    spring = triangle(glide(200, 290, 0.1)) * env(0.1, 0.006, 0.035)
    wobble = triangle(glide(240, 270, 0.12)) * env(0.12, 0.01, 0.04)
    return mix((0, squash, 1.0), (0, wet, 0.35), (0.14, spring, 0.6), (0.27, wobble, 0.25))


@sfx("SFX/Monster/Slime_OnHit.mp3", "hit", 0.25)
def slime_onhit(rng):   # 칼이 닿는 '딱' + '퍽' + 삼각파 몸통. 끊임없이 울리므로 7kHz 위를 덜어 귀가 덜 피곤하게
    sec = 0.22
    crack = band(nz(rng, 14000, sec) * env(sec, 0.0005, 0.006), 800, 7000)
    body = band(nz(rng, 5000, sec) * env(sec, 0.001, 0.06), 300, 3000)
    # 몸통은 160Hz 위(폰 스피커에서도 '퍽'), '딱' 6ms 뒤에 받쳐 두 피크가 겹쳐 솟지 않게
    return mix((0, crack, 0.45), (0, body, 0.8), (0.006, thump(420, 160, sec, 0.09, 0.015), 1.0))
#endregion

#region 플레이어
def footfall(rng, rate):
    """흙길 한 걸음: 뒤꿈치 + 앞꿈치(28ms 뒤) 두 번의 둔한 노이즈 '톡' + 작은 삼각파 '툭'."""
    def tick():
        return band(nz(rng, rate * rng.uniform(0.9, 1.1), 0.07) * env(0.07, 0.002, 0.014), 150, 2200)
    return mix((0, tick()), (0.028, tick(), 0.55), (0, thump(190, 120, 0.06, 0.02, 0.01), 0.35))


def walk(rng, times, rate):
    return mix(*[(at + rng.uniform(0, 0.012), footfall(rng, rate), rng.uniform(0.75, 1.0)) for at in times])


@sfx("SFX/Player/Footsteps2.mp3", "ui", 0.99)
def footsteps2(rng):
    return walk(rng, (0.0, 0.4, 0.8), 3000)


@sfx("SFX/Player/Footsteps3.mp3", "ui", 1.24)
def footsteps3(rng):   # 느린 걸음, 더 어두운 흙
    return walk(rng, (0.0, 0.5, 1.0), 2600)


@sfx("SFX/Player/SFX_Footstep.mp3", "ui", 1.45)
def footstep(rng):   # 네 걸음, 조금 밝은 흙
    return walk(rng, (0.0, 0.38, 0.76, 1.14), 3400)


@sfx("SFX/Player/PlayerSkill/Sound_Smash Impact.wav", "hit", 0.52)
def smash_impact(rng):
    return boom(rng, 0.5)


@sfx("SFX/Player/PlayerSkill/Sound_Smash.mp3", "hit", 1.07)
def smash(rng):   # 크게 휘두르는 '후웅' → 쾅 → 우르릉 잔향
    rumble = band(nz(rng, 2500, 0.6) * env(0.6, 0.02, 0.18), 150, 1500)
    return mix((0, whoosh(rng, 0.38, 1200, 12000, peak=0.85, tau=0.04, lo=300), 0.6), (0.34, boom(rng, 0.6), 1.0), (0.36, rumble, 0.45))


@sfx("SFX/Player/PlayerSkill/Water Spell Impact.mp3", "hit", 0.94)
def water_impact(rng):   # 낮은 '풍' + 촤악(밝은 노이즈가 어두워진다) + 흩어지는 물방울
    splash = band(nz(rng, glide(16000, 4000, 0.55)) * env(0.55, 0.002, 0.1), 700, 9000)
    return mix((0, thump(260, 110, 0.2, 0.06, 0.02), 0.8), (0, splash, 0.8), (0.03, drops(rng, 14, 0.85), 0.5))


def water_whip(rng, sw, f0, n_drops, sec):
    """물채찍: 휘두르는 바람 + 휘감기는 12.5% 펄스 스윕 → 끝에서 '짝' → 물 튀는 소리."""
    t = t_(sw)
    whirl = p12(glide(f0, f0 * 3, sw, 2) * (1 + 0.03 * np.sin(2 * np.pi * 40 * t))) * swell(sw, 0.9 * sw, 0.02)
    crack = band(nz(rng, 20000, 0.05) * env(0.05, 0.0003, 0.006), 1500, 11000)
    slosh = band(nz(rng, glide(9000, 2500, 0.35)) * env(0.35, 0.01, 0.1), 600, 6000)
    return mix((0, whoosh(rng, sw, 1500, 14000, peak=0.9, tau=0.03, lo=500), 0.7), (0, whirl, 0.25),
               (sw - 0.01, crack, 0.5), (sw, slosh, 0.5), (sw + 0.02, drops(rng, n_drops, sec - sw - 0.02), 0.45))


@sfx("SFX/Player/PlayerSkill/WaterWhip.mp3", "hit", 0.74)
def water_whip_a(rng):
    return water_whip(rng, 0.22, 300, 8, 0.72)


@sfx("SFX/Player/PlayerSkill/WaterWhip_Ori.mp3", "hit", 0.59)
def water_whip_ori(rng):   # 더 짧고 높게
    return water_whip(rng, 0.16, 420, 5, 0.57)


@sfx("SFX/Player/Sound_Miss.mp3", "ui", 0.97, 0.8)
def miss(rng):   # 허공을 가르는 '휙'(아래로 쓸린다) + 김빠지는 '뿌우'(A4→D4, 7Hz 흔들림)
    t = t_(0.7)
    deflate = square(glide(N("A4"), N("D4"), 0.7, 1.3) * (1 + 0.015 * np.sin(2 * np.pi * 7 * t))) * env(0.7, 0.01, 0.2)
    return mix((0, whoosh(rng, 0.3, 7000, 2500, peak=0.35, tau=0.08, lo=600, hi=8000), 0.8), (0.12, band(deflate, None, 3000), 0.3))


@sfx("SFX/Player/Sound_PlayerAttack.mp3", "hit", 0.65, 0.4)
def player_attack(rng):   # 밝고 짧은 '슉' + 칼날의 가는 '칭'(12.5% 펄스 둘, 맥놀이). 탭마다 울리므로 짧게
    shing = (p12(glide(2500, 3300, 0.3)) + p12(glide(2510, 3320, 0.3))) * env(0.3, 0.01, 0.06)
    return mix((0, whoosh(rng, 0.17, 4000, 15000, peak=0.55, tau=0.035, lo=1200, hi=11000), 1.0), (0.05, shing, 0.12))


@sfx("SFX/Player/Sound_PlayeronHit.wav", "hit", 1.12, 0.5)
def player_onhit(rng):   # '퍽' + 아래로 떨어지며 30Hz 로 떨리는 25% 펄스('아야')
    t = t_(0.42)
    ouch = p25(glide(900, 180, 0.42, 0.8) * (1 + 0.04 * np.sign(np.sin(2 * np.pi * 30 * t)))) * env(0.42, 0.002, 0.13)
    hit = band(nz(rng, 7000, 0.12) * env(0.12, 0.0005, 0.02), 400, 6000)
    return mix((0, thump(300, 110, 0.2, 0.06, 0.02), 1.0), (0, hit, 0.4), (0.02, ouch, 0.6))


@sfx("SFX/SFX_Birdfly-short.mp3", "ui", 1.82)
def birdfly(rng):   # 새 세 마리가 날아오른다: 날갯짓(짧은 노이즈 부풂)이 빠르게 이어지다 멀어지고, 짧은 '짹'
    flaps = []
    for start, rate, n in ((0.0, 13, 20), (0.1, 16, 24), (0.28, 11, 16)):
        for k in range(n):
            flap = band(nz(rng, rng.uniform(5000, 9000), 0.05) * swell(0.05, 0.012, 0.012), 500, 5000)
            g = min(1, (k + 1) / 3) * (1 - k / n) ** 1.2 * rng.uniform(0.6, 1.0)
            flaps.append((max(0.0, start + k / rate + rng.uniform(-0.008, 0.008)), flap, g))
    chirp = p12(glide(2600, 3700, 0.05, 0.5)) * env(0.05, 0.002, 0.015)
    return mix(*flaps, (0.05, chirp, 0.2), (0.22, chirp, 0.15), (0.5, chirp, 0.12))
#endregion

#region 미니몬 (UI_TamePopup)
@sfx("SFX/UI/Minimon/SFX_Charm.mp3", "ui", 0.40)
def charm(rng):   # 길들이기 시도 '반짝반짝': E장조 음을 빠르게 튀기고 + 높은 '샤랑' 공기 + 끝의 작은 종
    tw = melody("E6:1 G#6:1 B6:1 E7:1 B6:1 E7:1 G#7:1 E7:2", 0.035, p25, gap=0.003, decay=0.04, sustain=0.2)
    air = band(nz(rng, 22050, 0.3) * swell(0.3, 0.08, 0.08), 6000)
    return mix((0, tw, 0.8), (0.03, air, 0.15), (0.3, bell(N("E7"), 0.12, 0.05), 0.2))


@sfx("SFX/UI/Minimon/SFX_LureFail.wav", "ui", 0.31)
def lure_fail(rng):   # 효과 없음 — 둔한 '뿌-': 단2도로 부딪히는 사각파 둘이 살짝 처지고, 2.2kHz 위를 깎아 둔하게
    f = glide(N("D4"), N("D4") * 0.94, 0.28)   # D3 이면 에너지 절반이 폰 스피커 아래로 빠진다
    return band((square(f) + pulse(f * 1.06, 0.25)) * env(0.28, 0.004, 0.08, hold=0.1), None, 2200)


@sfx("SFX/UI/Minimon/SFX_LureHit.wav", "ui", 0.31)
def lure_hit(rng):   # 흥미 — 긍정의 '핑': 솔 → 5도 위 레 종소리
    return mix((0, blip(N("G6"), 0.07, 0.25, tau=0.025), 0.6), (0.055, bell(N("D7"), 0.25, 0.08), 0.9))


@sfx("SFX/UI/Minimon/SFX_Minimon_Appear.wav", "jingle", 0.82)
def minimon_appear(rng):   # '뽕' 튀어나오고 → 위로 미끄러지는 '뾰잉' → D장조 차임(끝음 비브라토) + 삼각파 화음 받침
    pop = triangle(glide(300, 1400, 0.04, 0.5)) * env(0.04, 0.001, 0.015)
    slide = p25(glide(N("A5"), N("D6"), 0.07, 0.5)) * env(0.07, 0.003, 0.05)
    chime = jingle("D6:1 F#6:1 A6:1", "D7:10", 0.05, p25, gap=0.004, decay=0.1, sustain=0.4, vib=0.012, ring=0.18)
    pad = melody("D5+F#5+A5:13", 0.05, triangle, decay=0.2, sustain=0.0, release=0.02)
    return mix((0, pop, 0.9), (0.03, slide, 0.5), (0.09, chime, 0.6), (0.09, pad, 0.35),
               (0.25, sparkle(rng, ("D7", "F#7", "A7"), 0.04, 0.3), 0.2))


@sfx("SFX/UI/Minimon/SFX_Minimon_Click.mp3", "ui", 0.24)
def minimon_click(rng):   # 작은 '톡' + 여린 메아리 둘
    return echo(blip(N("A6"), 0.07, 0.25, tau=0.022, f1=N("C#7")), 0.07, 0.3, 2)


@sfx("SFX/UI/Minimon/SFX_MinimonEat.mp3", "ui", 2.01)
def minimon_eat(rng):   # 냠냠 네 번(삼각파 '냠' + 바삭한 알갱이) → 꿀꺽 → 뿌듯한 두 음
    def munch(p):
        crunch = band(mix((0, crackle(rng, 0.12, 180, 12000)), (0, nz(rng, 7000, 0.14) * env(0.14, 0.002, 0.03), 0.8)), 500, 6000)
        return mix((0, triangle(glide(p, p * 0.7, 0.1)) * env(0.1, 0.004, 0.04), 0.9), (0.01, crunch, 0.6))
    bites = [(0.36 * i + rng.uniform(0, 0.03), munch(rng.uniform(260, 340)), 1 - 0.1 * i) for i in range(4)]
    swallow = triangle(glide(300, 700, 0.12, 0.7)) * env(0.12, 0.005, 0.05)
    return mix(*bites, (1.55, swallow, 0.7), (1.7, melody("E6:1 A6:4", 0.06, p25, gap=0.005, decay=0.08, sustain=0.2), 0.35))


@sfx("SFX/UI/Minimon/SFX_MinimonRunaway.wav", "ui", 0.62)
def minimon_runaway(rng):   # 후다닥: 떨리며 곤두박질치는 12.5% 펄스 + 멀어지는 바람 + 종종걸음
    t = t_(0.5)
    zip_ = p12(glide(1800, 260, 0.5, 0.7) * (1 + 0.05 * np.sin(2 * np.pi * 24 * t))) * env(0.5, 0.004, 0.16)
    patter = mix(*[(0.045 * i, band(nz(rng, 9000, 0.03) * env(0.03, 0.001, 0.006), 1000, 6000), 1 - i / 7) for i in range(6)])
    return mix((0, zip_, 0.55), (0, whoosh(rng, 0.4, 12000, 2500, peak=0.1, tau=0.12, lo=800), 0.45), (0.02, patter, 0.35))


@sfx("SFX/UI/Minimon/SFX_MinimonThink.wav", "ui", 0.26)
def minimon_think(rng):   # '흠?': 미 에 머물다 라 로 말꼬리를 올린다(삼각파 + 여린 50% 펄스)
    sec = 0.24
    t = t_(sec)
    f = N("E5") * 2 ** (5 / 12 * np.clip((t - 0.1) / 0.08, 0, 1))
    return fade_end((triangle(f) + 0.35 * square(f)) * np.minimum(t / 0.006, 1) * np.exp(-np.maximum(t - 0.16, 0) / 0.025))


@sfx("SFX/UI/Minimon/SFX_UseHappyMandrake.mp3", "jingle", 0.98)
def use_happy_mandrake(rng):   # 신나는 짧은 징글(F장조): 통통 튀는 분산화음이 C7 에 내려앉아 떨린다
    lead = jingle("C6:1 F6:1 A6:1 C7:2 A6:1 Bb6:1", "C7:7", 0.065, p25, gap=0.006, decay=0.08, sustain=0.5, vib=0.012, ring=0.2)
    bass = jingle("F4:2 A4:2 C5:3", "F4:7", 0.065, triangle, gap=0.01, sustain=0.8, ring=0.2)
    return mix((0, lead, 0.6), (0, bass, 0.6), (0.4, sparkle(rng, ("F7", "A7", "C8"), 0.04, 0.35), 0.15))
#endregion

#region 대장간 · 금속
def anvil(rng, sec, f0, tau=0.4):
    """모루가 부딪는 '쾅': 금속 울림 + 삼각파 몸통 + 먼지 노이즈."""
    dust = band(nz(rng, glide(8000, 2000, 0.4)) * env(0.4, 0.001, 0.06), 300, 7000)
    return mix((0, clang(f0, sec, tau, bite=0.5), 0.9), (0, thump(260, 100, 0.3, 0.09, 0.03), 0.8), (0, dust, 0.6))


@sfx("SFX/UI/SFX_AnvilFall.mp3", "hit", 1.57)
def anvil_fall(rng):   # 떨어지는 휘파람(아래로 쓸리는 삼각파 + 여린 사각파, 가까워질수록 커진다) → 쾅! 모루
    sec = 0.7
    t = t_(sec)
    f = glide(2400, 650, sec, 1.3) * (1 + 0.008 * np.sin(2 * np.pi * 6 * t))
    whistle = (tri_hi(f) + 0.3 * square(f)) * np.minimum(t / 0.05, 1) * (0.35 + 0.65 * t / sec)
    return mix((0, fade_end(whistle), 0.5), (sec, anvil(rng, 1.0, 560), 1.0))


@sfx("SFX/UI/SFX_AnvilFloor.mp3", "hit", 1.83)
def anvil_floor(rng):   # 무거운 모루가 바닥에 '쿵-쾅' + 두 번 작게 튀는 '깡, 깡'
    return mix((0, anvil(rng, 1.6, 380, 0.5), 1.0), (0, boom(rng, 0.4), 0.6),
               (0.22, clang(390, 0.5, 0.15), 0.3), (0.36, clang(395, 0.4, 0.1), 0.15))


@sfx("SFX/UI/SFX_EnhanceHammer.mp3", "hit", 1.77, 1.5)
def enhance_hammer(rng):   # 휘두르는 바람(0~0.25초) → 0.25초에 '땡!'(판이 떠는 Shake 와 맞춘다) → 튀어 오른 망치 '팅' → 울림
    return mix((0, whoosh(rng, 0.25, 1500, 7000, peak=0.95, tau=0.02, lo=400), 0.4), (0.25, clang(880, 1.25, 0.35), 1.0),
               (0.25, thump(300, 120, 0.15, 0.04, 0.02), 0.5), (0.4, clang(885, 0.4, 0.08, bite=0.5), 0.25),
               (0.26, crackle(rng, 0.3, 30, 16000), 0.2))


@sfx("SFX/UI/SFX_LightHammer.mp3", "hit", 0.81)
def light_hammer(rng):   # 가벼운 쇠 '팅': 높은 금속 울림, 몸통 없이
    tick = band(nz(rng, 16000, 0.02) * env(0.02, 0.0003, 0.003), 2000)
    return mix((0, clang(1500, 0.7, 0.17, bite=0.4), 1.0), (0, tick, 0.2))


@sfx("SFX/UI/SFX_SwordBlock.mp3", "hit", 0.67)
def sword_block(rng):   # 막기(피치 0.85)·패링(1.25) 공용 '챙!': 밝은 노이즈 딱 + 높고 가는 금속 울림(25% 펄스, 빠른 맥놀이)
    crack = band(nz(rng, 20000, 0.04) * env(0.04, 0.0003, 0.004), 2000, 11000)
    ring = clang(1250, 0.6, 0.2, ratios=(1.0, 1.52, 2.08, 2.71, 3.3), beat=0.006, bite=0.3)   # 12.5% 는 피크만 솟아 작게 들린다
    return mix((0, crack, 0.3), (0, ring, 1.0), (0, thump(400, 180, 0.06, 0.02, 0.01), 0.2))


@sfx("SFX/UI/SFX_WeaponBreak.mp3", "hit", 1.20)
def weapon_break(rng):   # 우지끈: 균열 '딱' + 깨지는 금속 '쨍' + 꺾이며 떨어지는 펄스 + 사방으로 튀는 파편 '팅팅팅'
    crack = band(nz(rng, 18000, 0.08) * env(0.08, 0.0005, 0.015), 1000, 11000)
    snap = p12(glide(1600, 300, 0.25, 0.5)) * env(0.25, 0.001, 0.07)
    shatter = band(nz(rng, 12000, 1.0) * env(1.0, 0.002, 0.16), 1500, 11000)
    shards = [(at, clang(rng.uniform(2200, 5200), 0.12, 0.03, ratios=(1.0, 1.47, 2.3), beat=0.01, bite=0.3), (1 - at / 1.15) ** 1.2)
              for at in 0.05 + 0.9 * (np.arange(22) / 22) ** 1.6 + rng.uniform(0, 0.02, 22)]
    return mix((0, crack, 0.4), (0, clang(900, 0.5, 0.14, bite=0.5), 0.7), (0.01, snap, 0.45), (0, shatter, 0.35), (0, mix(*shards), 0.5))


@sfx("SFX/UI/SFX_FlameOn.mp3", "jingle", 1.57)
def flame_on(rng):   # 화로 점화: 빨려 드는 '화륵' → 낮은 '훅' → 일렁이는 불길(어두운 노이즈 + 잦아드는 불똥)
    sec = 1.25
    t = t_(sec)
    flicker = 1 + 0.25 * np.sin(2 * np.pi * 7 * t + 3 * np.sin(2 * np.pi * 2.3 * t))
    roar = band(nz(rng, 3500, sec), 120, 2500) * swell(sec, 0.05, 0.45) * flicker
    embers = crackle(rng, sec, 25, 14000)
    embers *= np.exp(-np.arange(embers.size) / SR / 0.6)
    return mix((0, whoosh(rng, 0.32, 800, 9000, peak=0.85, tau=0.05, lo=150), 0.7), (0.27, thump(220, 100, 0.25, 0.08, 0.04), 0.6),
               (0.27, roar, 0.7), (0.3, embers, 0.35))
#endregion

#region UI
@sfx("SFX/UI/SFX_BtnSelect.mp3", "ui", 0.17)
def btn_select(rng):   # 가장 자주 듣는 소리 — 짧고 부드러운 '또롱'(미 → 시, 7kHz 위를 덜어 둥글게)
    return band(mix((0, blip(N("E6"), 0.035, 0.5, tau=0.012), 0.7), (0.03, blip(N("B6"), 0.11, 0.25, tau=0.03), 0.8)), None, 7000)


@sfx("SFX/UI/SFX_Popup.mp3", "ui", 2.22, 0.4)
def popup(rng):   # 창이 열리는 '휙' + 위로 튀는 두 음 '뾰롱'
    return mix((0, whoosh(rng, 0.14, 3000, 12000, peak=0.85, tau=0.03, lo=1500), 0.5),
               (0.1, blip(N("G5"), 0.05, 0.25, tau=0.02, f1=N("D6")), 0.6), (0.15, blip(N("G6"), 0.22, 0.25, tau=0.06), 0.55))


@sfx("SFX/UI/SFX_ItemPopup.mp3", "jingle", 1.15, 1.1)
def item_popup(rng):   # 아이템 획득: '뽕' + '따-단!'(B 화음 → E 화음, 이끎음 D# 이 E 로 풀린다) + 반짝이
    pop = triangle(glide(400, 1600, 0.035, 0.5)) * env(0.035, 0.001, 0.012)
    lead = jingle("D#6:2", "E6:18", 0.05, p25, gap=0.01, decay=0.08, sustain=0.5, vib=0.01, ring=0.25)
    chords = jingle("B4+D#5+F#5:2", "E5+G#5+B5:18", 0.05, square, gap=0.01, decay=0.08, sustain=0.5, ring=0.25)
    return mix((0, pop, 0.8), (0.03, lead, 0.6), (0.03, chords, 0.3), (0.16, sparkle(rng, ("B6", "E7", "G#7"), 0.035, 0.45), 0.2))


@sfx("SFX/UI/SFX_NextPage.mp3", "ui", 1.10, 0.3)
def next_page(rng):   # 종이 넘기는 '팔락'(위로 쓸리는 밝은 노이즈) + 여린 '톡'
    flip = band(nz(rng, glide(6000, 14000, 0.14)) * swell(0.14, 0.04, 0.035), 1500, 9000)
    return mix((0, flip, 0.7), (0.08, blip(N("A5"), 0.15, 0.5, tau=0.03), 0.3))


@sfx("SFX/UI/SFX_CraftComplete.mp3", "jingle", 1.52)
def craft_complete(rng):   # 제작 완료: 종 '딩-딩!' + G장조 분산화음이 올라가 울린다
    flo = jingle("G5:1 B5:1 D6:1", "G6:20", 0.05, p25, gap=0.004, decay=0.06, sustain=0.5, vib=0.01, ring=0.3)
    bass = jingle("G4:3", "D5:20", 0.05, triangle, gap=0.01, sustain=0.8, ring=0.3)
    return mix((0, bell(N("D7"), 0.5, 0.15), 0.6), (0.13, bell(N("G7"), 0.7, 0.2), 0.7), (0.3, flo, 0.55), (0.3, bass, 0.5))


@sfx("SFX/UI/SFX_PotionUpgrade.mp3", "jingle", 3.29, 1.9)
def potion_upgrade(rng):   # 보글보글 끓어오르다(점점 촘촘하고 높게) → '뽀옹' → A장조 화음이 반짝
    x = np.arange(18) / 18
    rise = mix(*[(0.9 * (1 - (1 - v) ** 1.6), bubble(400 * 4 ** v * rng.uniform(0.9, 1.1), 0.05, 1.8), 0.5 + 0.5 * v) for v in x])
    blup = square(glide(N("A4"), N("A5"), 0.12, 0.6)) * env(0.12, 0.002, 0.05)
    lead = jingle("A5:1 C#6:1 E6:1", "A6:14", 0.05, p25, gap=0.004, decay=0.06, sustain=0.5, vib=0.01, ring=0.3)
    pad = melody("A4+C#5+E5:17", 0.05, triangle, decay=0.35, sustain=0.0, release=0.03)
    return mix((0, rise, 0.6), (0.92, blup, 0.55), (0.98, lead, 0.6), (0.98, pad, 0.35),
               (1.05, sparkle(rng, ("A6", "C#7", "E7"), 0.035, 0.6), 0.2))


@sfx("SFX/UI/SFX_Upgrade.mp3", "jingle", 2.24, 2.0)
def upgrade(rng):   # 강화 성공: C장조 음계를 달려 올라 C7 으뜸화음이 울린다(강화 연출에서 Success(Base) 와 겹쳐도 같은 조)
    run = jingle("C5:1 D5:1 E5:1 F5:1 G5:1 A5:1 B5:1 C6:1 E6:1 G6:1", "C7:40", 0.04, p25, gap=0.004, decay=0.05, sustain=0.6, vib=0.01, ring=0.4)
    chord = jingle("-:10", "C6+E6+G6:40", 0.04, square, ring=0.45)
    bass = jingle("C4:4 G4:4 -:2", "C4:40", 0.04, triangle, gap=0.01, sustain=0.8, ring=0.4)
    return mix((0, run, 0.6), (0, chord, 0.3), (0, bass, 0.55), (0.4, crash(rng), 0.12))
#endregion

#region 징글 · 팡파르 · 스팅
@sfx("SFX/UI/SFX_Rankup.mp3", "jingle", 2.67, 2.3)
def rankup(rng):   # 승급 팡파르(D장조): 분산화음으로 뛰어올라 음계로 한 옥타브를 더 오른다 — 스네어 롤 → 크래시
    s = 0.07
    lead = jingle("A4:1 D5:1 F#5:1 A5:2 G5:1 F#5:1 G5:1 A5:1 B5:1 C#6:1", "D6:20", s, p25, gap=0.008, decay=0.08, sustain=0.6, vib=0.01, ring=0.5)
    chords = jingle("D5+F#5+A5:5 G4+B4+D5:3 A4+C#5+E5:3", "D5+F#5+A5:20", s, square, gap=0.01, decay=0.08, sustain=0.5, ring=0.45)
    bass = jingle("D4:2 A3:3 G3:3 A3:3", "D4:20", s, triangle, gap=0.015, sustain=0.9, ring=0.5)
    drums = mix(*[(s * (5 + i), snare(rng), 0.2 + 0.08 * i) for i in range(6)], (s * 11, crash(rng), 0.35))
    return mix((0, lead, 0.6), (0, chords, 0.25), (0, bass, 0.55), (0, drums, 0.6))


@sfx("SFX/UI/SFX_Success(Base).mp3", "jingle", 5.86, 3.3)
def success_base(rng):   # 강화 성공 팡파르(C장조) — Serious(C 5도) 0.083초 뒤에 겹쳐 울린다. 당당하게 올라 E6 에서 길게 빛난다
    s = 0.085
    lead = jingle("G4:1 C5:1 E5:1 G5:3 E5:1 G5:2 C6:4 B5:1 C6:1 D6:2", "E6:22", s, p25, gap=0.01, decay=0.1, sustain=0.6, vib=0.01, ring=0.6)
    chords = jingle("C5+E5+G5:9 F5+A5+C6:4 G4+B4+D5:4", "C5+E5+G5:22", s, square, gap=0.012, decay=0.1, sustain=0.5, ring=0.55)
    bass = jingle("C4:3 E4:3 G3:3 F3:4 G3:4", "C4:22", s, triangle, gap=0.02, sustain=0.9, ring=0.6)
    drums = mix(*[(s * (13 + i), snare(rng), 0.25 + 0.1 * i) for i in range(4)], (s * 17, crash(rng), 0.4))
    return mix((0, lead, 0.6), (0, chords, 0.25), (0, bass, 0.55), (0, drums, 0.6),
               (s * 17, sparkle(rng, ("C7", "E7", "G7"), 0.05, 1.0), 0.15))


@sfx("SFX/UI/SFX_Success2.mp3", "jingle", 5.87, 1.5)
def success2(rng):   # 구매·보상·던전 입장(9곳) — 짧고 밝은 G장조 팡파르
    s = 0.06
    lead = jingle("D5:1 G5:1 B5:1 D6:2 B5:1 D6:1", "G6:16", s, p25, gap=0.006, decay=0.08, sustain=0.6, vib=0.01, ring=0.3)
    chords = jingle("G4+B4+D5:5 G4+C5+E5:1 A4+D5+F#5:1", "B4+D5+G5:16", s, square, gap=0.01, decay=0.08, sustain=0.5, ring=0.3)
    bass = jingle("G3:3 B3:2 C4:1 D4:1", "G3:16", s, triangle, gap=0.01, sustain=0.9, ring=0.35)
    return mix((0, lead, 0.6), (0, chords, 0.25), (0, bass, 0.55), (s * 7, crash(rng), 0.2), (s * 7, snare(rng), 0.3))


@sfx("SFX/UI/SFX_NewContents.mp3", "jingle", 8.52, 3.3)
def new_contents(rng):   # 새 기능 해금: 노이즈가 차오르며 스네어 롤 → Bb–C–D(bVI–bVII–I) 위로 이웃음 동기가 계단처럼 오른다 → D장조 대단원
    s, t0 = 0.075, 0.5
    lead = jingle("D6:1 C6:1 D6:1 F6:3 E6:1 D6:1 E6:1 G6:3 F#6:1 E6:1 F#6:1", "A6:22", s, p25, gap=0.006, decay=0.08, sustain=0.6, vib=0.01, ring=0.55)
    chords = jingle("Bb4+D5+F5:6 C5+E5+G5:6 D5+F#5+A5:3", "D5+F#5+A5:22", s, square, gap=0.01, decay=0.08, sustain=0.5, ring=0.5)
    bass = jingle("Bb3:6 C4:6 D4:3", "D4:22", s, triangle, gap=0.015, sustain=0.9, ring=0.55)
    roll = mix(*[(0.04 * i, snare(rng), 0.1 + 0.03 * i) for i in range(12)])
    return mix((0, whoosh(rng, 0.5, 1000, 14000, peak=0.98, tau=0.02, lo=300), 0.4), (0, roll, 0.8),
               (t0, lead, 0.6), (t0, chords, 0.25), (t0, bass, 0.55), (t0 + 15 * s, crash(rng), 0.3),
               (t0 + 15 * s, sparkle(rng, ("D7", "F#7", "A7"), 0.05, 1.2), 0.15))


@sfx("SFX/UI/SFX_Fail.mp3", "jingle", 2.49, 0.6)
def fail(rng):   # 부드러운 '어-어': Eb → C 두 음(강화 실패 때는 Serious 의 C 5도 위에서 C단조가 된다). 50% 사각파 + 옥타브 아래 삼각파
    a = jingle("Eb5:2", "C5:6", 0.075, square, gap=0.015, decay=0.15, sustain=0.5, vib=0.015, ring=0.18)
    b = jingle("Eb4:2", "C4:6", 0.075, triangle, gap=0.015, sustain=1.0, ring=0.2)
    return band(mix((0, a, 0.45), (0, b, 0.7)), None, 5000)


@sfx("SFX/UI/SFX_Fail2.mp3", "jingle", 6.91, 1.7)
def fail2(rng):   # 큰 실패(강화 실패): 둔탁한 '쿵' + 셋온음으로 꺾여 내려가는 선율 위 감화음 → 낮게 가라앉는다
    s = 0.09
    lead = jingle("C5:2 G4:2 F#4:2", "C4:12", s, p25, gap=0.01, decay=0.1, sustain=0.6, vib=0.02, ring=0.35)
    under = jingle("Eb4+F#4+A4:6", "C4+Eb4+F#4:12", s, square, gap=0.01, decay=0.15, sustain=0.4, ring=0.35)
    bass = jingle("C4:6", "F#3:12", s, triangle, gap=0.01, sustain=1.0, ring=0.4)
    return mix((0, boom(rng, 0.4), 0.5), (0.02, lead, 0.6), (0.02, under, 0.2), (0.02, bass, 0.5))


@sfx("SFX/UI/SFX_CuriousCaution2.mp3", "jingle", 3.36, 1.9)
def curious_caution2(rng):   # 수상한 동기: 감7 분산화음으로 오르다 반음 이웃음에서 멈칫, 메아리 — 망치질 밑에 깔린다(C 삼각파 지속음)
    lead = jingle("C5:1 Eb5:1 F#5:1 A5:2 G#5:1", "A5:6", 0.11, p25, gap=0.01, decay=0.08, sustain=0.5, vib=0.02, ring=0.3)
    t = t_(1.43)
    drone = triangle(const(N("C4"), 1.43)) * (0.6 + 0.4 * np.sin(2 * np.pi * 4 * t)) * env(1.43, 0.05, 0.6)
    return mix((0, echo(lead, 0.22, 0.35, 2), 0.6), (0, drone, 0.4))


@sfx("SFX/UI/SFX_FairySerious.mp3", "jingle", 8.06, 2.8)
def fairy_serious(rng):   # 요정의 진지한 순간(A단조): 높고 가는 12.5% 선율이 메아리치며 내려오고, 삼각파가 i–VII–V–i 를 딛는다
    s = 0.12
    lead = jingle("E6:2 D6:1 C6:1 B5:4 A5:2 G#5:2", "A5:8", s, p12, gap=0.01, decay=0.2, sustain=0.5, vib=0.012, ring=0.4)
    bass = jingle("A3:4 G3:4 E3:4", "A3:8", s, triangle, gap=0.02, sustain=1.0, ring=0.5)
    glint = melody("A6+C7+E7:4", s, p12, decay=0.15, sustain=0.0)   # 요정 반짝임(한 채널 화음)
    return mix((0, echo(lead, 0.18, 0.4, 2), 0.55), (0, bass, 0.6), (0, glint, 0.15), (s * 12, glint, 0.12))


@sfx("SFX/UI/SFX_Serious.mp3", "jingle", 1.87, 1.6)
def serious(rng):   # 진지한 '둥-': C·G 사각파(3음 없는 5도 — 뒤이어 겹치는 C장조 성공음·C단조 실패음 모두와 어울림)
    sec = 1.6       # + 삼각파 몸통 + 크래시, 위에서 C6·G6 트레몰로가 부풀었다 사라진다
    t = t_(sec)
    hit = (square(const(N("C4"), sec)) + square(const(N("G4"), sec))) * env(sec, 0.002, 0.35)   # 폰에서도 들리게 한 옥타브 위, 무게는 아래 삼각파
    trem = (p25(const(N("C6"), sec)) + p25(const(N("G6"), sec))) * (0.5 + 0.5 * np.sin(2 * np.pi * 12 * t)) * swell(sec, 0.35, 0.4)
    return mix((0, hit, 0.45), (0, thump(260, 131, 0.5, 0.2, 0.03), 0.35), (0, crash(rng), 0.25), (0, trem, 0.12))


@sfx("SFX/UI/SFX_SharpThin.mp3", "ui", 2.15, 0.9)
def sharp_thin(rng):   # 강화 창이 어두워질 때 가늘고 날카로운 '티잉': C7 12.5% 펄스 둘(4Hz 맥놀이) + 2.76배 삼각파 어택
    sec = 0.9
    ring = (p12(const(N("C7"), sec)) + p12(const(N("C7") * 1.002, sec))) * env(sec, 0.0005, 0.22)
    return mix((0, ring, 1.0), (0, triangle(const(N("C7") * 2.76, sec)) * env(sec, 0.0005, 0.04), 0.5))


@sfx("SFX/UI/SFX_HolyShine.mp3", "jingle", 5.91, 2.4)
def holy_shine(rng):   # 축복의 빛: Dmaj9 분산화음이 메아리치며 솟고, 높은 '샤아' 공기와 50% 펄스 화음이 부풀었다 사라진다
    arp = melody("D5:1 F#5:1 A5:1 C#6:1 E6:1 F#6:1 A6:1 C#7:1 E7:6", 0.06, p25, gap=0.004, decay=0.12, sustain=0.2, vib=0.01)
    air = band(nz(rng, 22050, 2.2), 5000) * swell(2.2, 0.8, 0.4)
    return mix((0, echo(arp, 0.12, 0.45, 4), 0.55), (0, chord_pad(("D5", "F#5", "A5"), 2.2, 0.7, 0.5), 0.15), (0.1, air, 0.12))


@sfx("SFX/UI/SFX_HolyShine2.mp3", "jingle", 5.90, 2.4)
def holy_shine2(rng):   # 축성: 위에서 쏟아지는 A 리디안 글리산도 + 비브라토 낀 50% 펄스 '성가대' 화음 + 끝의 맑은 종
    harp = melody("E7:1 D#7:1 C#7:1 B6:1 A6:1 E6:1 D#6:1 C#6:1 B5:1 A5:5", 0.05, p12, gap=0.003, decay=0.1, sustain=0.2)
    vib = 1 + 0.006 * np.sin(2 * np.pi * 5 * t_(2.2))
    choir = chord_pad(("A4", "C#5", "E5"), 2.2, 0.9, 0.5, osc=lambda f: square(f * vib))
    return mix((0, echo(harp, 0.1, 0.4, 3), 0.5), (0.1, choir, 0.18), (1.0, bell(N("A6"), 1.2, 0.35), 0.35),
               (0.2, sparkle(rng, ("A6", "C#7", "E7"), 0.06, 1.0), 0.1))
#endregion

#region 마스터링 · 입출력
def db(v):
    return 20 * np.log10(max(float(v), 1e-12))


def peak_db(x):
    return db(np.abs(x).max())


def rms_db(x):
    return db(np.sqrt(np.mean(np.square(x, dtype=float))))


def loud_db(x):
    """가장 센 0.2초 구간의 RMS(짧은 소리는 파일 전체) — 귀가 소리 크기를 쌓는 시간 규모.
    파일 전체 RMS 는 긴 울림 꼬리(모루·종)에 깎여, 그걸로 맞추면 짧은 타격만 커진다.
    ponytail: 주파수 가중(K-weighting) 없는 단순 RMS — 쓰는 팔레트가 비슷해 분류 안에서는 충분하다."""
    k = min(n_(0.2), x.size)
    c = np.concatenate([[0.0], np.cumsum(np.square(x, dtype=float))])
    return db(np.sqrt((c[k:] - c[:-k]).max() / k))


def finish(x, cat):
    """90Hz 아래 제거 → 12kHz 출력단 → 꼬리 무음(-54dB) 자르기 → 페이드 3ms/5ms → 피크 -1dBFS
    → 가장 센 0.2초의 RMS 가 분류 목표가 되도록 내린다(올리지 않는다 — 피크가 먼저 막으면 목표보다 조용하다)."""
    x = lowpass(band(x, lo=90), 12000)
    x = x[:np.nonzero(np.abs(x) > np.abs(x).max() * 10 ** (-54 / 20))[0][-1] + 1].copy()
    fi, fo = n_(0.003), n_(0.005)
    x[:fi] *= np.linspace(0, 1, fi)
    x[-fo:] *= np.linspace(1, 0, fo)
    x *= 10 ** (-1 / 20) / np.abs(x).max()
    return x * 10 ** (min(TARGET[cat] - loud_db(x), 0.0) / 20)


def write_wav(path, x):
    with wave.open(path, "wb") as w:   # 16bit PCM 모노 44.1kHz
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(np.round(np.clip(x, -1, 1) * 32767).astype("<i2").tobytes())


def encode(path, x):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    if path.endswith(".wav"):
        return write_wav(path, x)
    with tempfile.TemporaryDirectory() as d:
        tmp = os.path.join(d, "tmp.wav")
        write_wav(tmp, x)
        subprocess.run([FFMPEG, "-y", "-loglevel", "error", "-i", tmp, "-codec:a", "libmp3lame", "-b:a", "128k", path], check=True)


def decode(path):
    r = subprocess.run([FFMPEG, "-v", "error", "-i", path, "-f", "f32le", "-ac", "1", "-ar", str(SR), "-"],
                       capture_output=True, check=True)
    return np.frombuffer(r.stdout, "<f4").astype(float)


def stem(rel):
    return os.path.splitext(os.path.basename(rel))[0]


def render(path, cat, fn):
    """합성 → 마스터링 → 굽기 → 디코드한 결과로 다시 잰다. MP3 는 고역을 깎아 조금 작아지고(약 -0.4dB)
    오버슈트로 피크가 오를 수 있어, 목표 음량과 피크 ≤ -1dBFS 를 둘 다 만족할 때까지 게인을 고쳐 다시 굽는다(gen_chiptune.master 와 같은 방식)."""
    x = finish(fn(np.random.default_rng([SEED, zlib.crc32(stem(path).encode())])), cat)
    for _ in range(8):
        encode(path, x)
        y = decode(path)
        ft, fp = TARGET[cat] - loud_db(y), -1.0 - peak_db(y)   # 목표까지 남은 dB, 피크 여유 dB
        if fp >= 0 and (abs(ft) < 0.1 or (ft > 0 and fp < 0.5)):   # 목표에 닿았거나, 피크가 막아 더 못 올린다(MP3 오버슈트가 굽기마다 흔들려 0.5dB 여유)
            return y
        x = x * 10 ** ((min(ft, fp) - 0.02) / 20)
    sys.exit(f"{path}: 음량/피크가 수렴하지 않음 (loud {loud_db(y):.1f}, peak {peak_db(y):.1f})")


def overview(rows, path):
    """리뷰용 한 장: 소리마다 로그 주파수 스펙트로그램(60Hz~16kHz, 70dB 폭) + 파형 엔벨로프 + 0.1초 눈금(0.5초는 길게).
    칸마다 가로축이 그 소리 길이다 — 길이는 글자로 읽는다."""
    try:
        from PIL import Image, ImageDraw
    except ImportError:
        print("Pillow 가 없어 overview.png 는 건너뛴다")
        return
    W, H, S, L, cols, nfft = 360, 160, 100, 36, 4, 1024
    w = W - 8
    stops = np.array([[0, 0, 4], [80, 18, 123], [221, 81, 58], [252, 253, 191]], float)   # 마그마 비슷한 4색
    img = Image.new("RGB", (cols * W, -(-len(rows) // cols) * H), (20, 20, 26))
    d = ImageDraw.Draw(img)
    bins = np.round(np.geomspace(16000, 60, S) * nfft / SR).astype(int)
    for i, (rel, cat, was, aim, y) in enumerate(rows):
        x0, y0 = (i % cols) * W + 4, (i // cols) * H
        edges = np.linspace(0, y.size, w + 1).astype(int)
        frames = np.pad(y, nfft // 2)[edges[:-1, None] + np.arange(nfft)] * np.hanning(nfft)
        mag = 20 * np.log10(np.abs(np.fft.rfft(frames, axis=1))[:, bins].T + 1e-9)
        v = np.clip((mag - mag.max()) / 70 + 1, 0, 1) * (len(stops) - 1)
        k = np.minimum(v.astype(int), len(stops) - 2)
        rgb = stops[k] + (stops[k + 1] - stops[k]) * (v - k)[..., None]
        img.paste(Image.fromarray(rgb.astype(np.uint8), "RGB"), (x0, y0 + 16))
        mid = y0 + 16 + S + 2 + L // 2
        for c, (a, b) in enumerate(zip(edges[:-1], edges[1:])):
            h = int(np.abs(y[a:max(b, a + 1)]).max() * L / 2)
            d.line([(x0 + c, mid - h), (x0 + c, mid + h)], fill=(120, 200, 255))
        sec = y.size / SR
        for j in range(int(sec * 10) + 1):
            tx = x0 + int(j / 10 / sec * (w - 1))
            d.line([(tx, y0 + H - 5 - (3 if j % 5 else 7)), (tx, y0 + H - 5)], fill=(200, 200, 200))
        d.text((x0, y0 + 2), f"{stem(rel)[:26]}  {sec:.2f}s  pk {peak_db(y):.1f}  loud {loud_db(y):.1f}", fill=(235, 235, 235))
    img.save(path)
    print("overview:", path)
#endregion


def main():
    ap = argparse.ArgumentParser(description="효과음 56종 합성: 플래그 없이 돌리면 계획만 출력한다")
    g = ap.add_mutually_exclusive_group()
    g.add_argument("--preview", metavar="DIR", help="DIR 에 Sound 기준 상대 경로 그대로 쓰고 표 + overview.png")
    g.add_argument("--write", action="store_true", help="Assets/@Resources/Sound 의 원래 파일을 덮어쓴다")
    ap.add_argument("--only", metavar="NAME[,NAME]", help="파일 이름(확장자 빼고, 대소문자 무시)으로 일부만")
    a = ap.parse_args()

    todo = SOUNDS
    if a.only:
        want = {s.strip().lower() for s in a.only.split(",")}
        todo = [s for s in SOUNDS if stem(s[0]).lower() in want]
        missing = want - {stem(s[0]).lower() for s in todo}
        if missing:
            sys.exit(f"없는 이름: {', '.join(sorted(missing))}")

    if not (a.preview or a.write):
        print(f"{len(todo)}개: --preview DIR 로 들어 보고 --write 로 덮어쓴다 (BGM, SFX_MinimonCaught 는 대상 아님)")
        for rel, cat, was, aim, _ in todo:
            ok = os.path.exists(os.path.join(SOUND, *rel.split("/")))
            print(f"  {rel:<52} {cat:<6} {was:5.2f}s -> {aim:4.2f}s  RMS {TARGET[cat]:.0f}{'' if ok else '  (원래 파일 없음!)'}")
        return
    if not FFMPEG:
        sys.exit("ffmpeg 가 PATH 에 없다")

    root = SOUND if a.write else a.preview
    rows = []
    print(f"{'name':<28} {'cat':<6} {'was':>5} {'aim':>5} {'sec':>5} {'peak':>6} {'RMS':>6} {'loud':>6} {'(tgt)':>6}")
    for rel, cat, was, aim, fn in todo:
        dst = os.path.join(root, *rel.split("/"))
        if a.write and not os.path.exists(dst):
            sys.exit(f"{rel}: 원래 파일이 없다: 새 이름은 GUID, 어드레서블 키가 없다")
        y = render(dst, cat, fn)
        sec = y.size / SR
        assert sec <= (3.5 if stem(rel) in BIG else 3.0), f"{rel}: {sec:.2f}초, 상한 초과"
        assert abs(y.mean()) < 1e-3, f"{rel}: DC {y.mean():+.1e}"
        rows.append((rel, cat, was, aim, y))
        print(f"{stem(rel):<28} {cat:<6} {was:5.2f} {aim:5.2f} {sec:5.2f} {peak_db(y):6.1f} {rms_db(y):6.1f} {loud_db(y):6.1f} {TARGET[cat]:6.0f}")
    if a.preview:
        overview(rows, os.path.join(a.preview, "overview.png"))


# 대상 목록 자체 점검 — 56개, 경로 중복 없음, BGM·SFX_MinimonCaught 없음
assert len(SOUNDS) == 56 and len({s[0] for s in SOUNDS}) == 56, len(SOUNDS)
assert not any(s[0].startswith("BGM/") or stem(s[0]) == "SFX_MinimonCaught" for s in SOUNDS)

if __name__ == "__main__":
    main()
