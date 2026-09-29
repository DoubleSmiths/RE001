# -*- coding: utf-8 -*-
"""미니몬 포획 BGM · 포획 성공 징글 — 오리지널 칩튠 작곡/합성기.

## 왜
  기존 BGM_Minion_battle.mp3 / SFX_MinimonCatch.mp3 는 저작권이 있는 타 게임 음원이라 상업 출시에 쓸 수 없다.
  대체 음원을 **이 스크립트가 원천**이 되도록 새로 작곡해 합성한다. 기존 파일은 열지도 참고하지도 않았다 —
  멜로디·화성은 아래 악보(PHRASES / JINGLE)가 처음이다.

## 곡
  BGM_MinimonCapture  168 BPM · 32마디(45.71초) 무한 루프. 포획 미니게임 — 긴장되지만 장난스럽게.
      A  (E단조, 1~8)    트레시요(3+3+2) 리듬의 상승 아르페지오 동기, iv → V7 로 조인다
      A' (9~16)          리드를 12.5% 듀티로 바꿔 반복, 뒤 4마디는 D(=G장조의 V)로 올라가 전조를 준비
      B  (G장조, 17~24)  통통 튀는 시퀀스 — 장조로 트이는 구간, 펄스2 가 코드톤 화음으로 받친다
      B' (25~32)         B 반복 후 Em–D–C–B7(안달루시안 진행)로 단조 복귀 → 1마디로 이어진다
  SFX_MinimonCaught   C장조 상승 아르페지오 → IV–V–I 종지 → 반짝이는 으뜸화음, 2.24초 자연 감쇠.

## 음원 구성 (NES 4채널 흉내)
  펄스1 리드(25%/12.5% 듀티, 딜레이 비브라토) · 펄스2 아르페지오/화음(50%) · 삼각파 베이스(4비트 32계단) · 노이즈 드럼.
  펄스는 PolyBLEP 로 앨리어싱을 줄이고, 모든 음은 수 ms 램프로 시작·끝나며 자기 칸 안에서 끝난다(클릭 없음).
  루프는 원형 버퍼로 렌더한다 — 끝을 넘친 드럼 꼬리는 맨 앞에 겹치므로 이음매도 곡 속의 평범한 한 지점이다.
  라우드니스는 ffmpeg ebur128 로 재서 맞춘다: BGM -16 LUFS, SFX -14 LUFS, 트루피크 ≤ -1 dBTP.

## 사용
  python _tools/gen_chiptune.py                 # 두 파일 생성 + 측정(길이/LUFS/피크/루프 이음매)
  python _tools/gen_chiptune.py --preview DIR   # + DIR 에 BGM 2회 반복 wav (이음매를 귀로 확인)
  필요: numpy, PATH 의 ffmpeg(libmp3lame). 노이즈 시드 고정 → 결정론적.

## 루프 주의
  MP3 는 인코더 지연/패딩을 LAME 헤더로만 알린다. **Unity 임포터는 이 헤더를 무시한다** — MP3 로 구운 BGM 이
  Unity 에서 2,016,000 → 2,018,304 샘플(+52ms)로 늘어 루프마다 틈이 났다(2026-09-29 실측).
  그래서 루프 BGM 은 무손실 WAV(16bit)로 굽고 압축은 Unity 임포터(Vorbis)에 맡긴다 — 샘플 수가 그대로다.
  (이 ffmpeg 에는 libvorbis 가 없고 내장 vorbis 인코더는 실험용이다.) 한 번 울리는 징글은 MP3 그대로 둔다.
"""
import os
import re
import shutil
import subprocess
import sys

import numpy as np

SR = 44100
SEED = 20260929
BASE = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
SOUND = os.path.join(BASE, "Assets", "@Resources", "Sound")
OUT_BGM = os.path.join(SOUND, "BGM", "BGM_MinimonCapture.wav")   # 루프 — WAV (위 '루프 주의')
OUT_SFX = os.path.join(SOUND, "SFX", "UI", "Minimon", "SFX_MinimonCaught.mp3")
FFMPEG = shutil.which("ffmpeg")

#region 악보 표기
# 토큰 = 음이름:16분음표칸수.  '-' 는 쉼표, 'E5+G5+C6' 은 펄스 한 채널로 도는 60Hz 아르페지오 화음.
PITCH = {"C": 0, "D": 2, "E": 4, "F": 5, "G": 7, "A": 9, "B": 11}


def midi(name):
    """'F#5' → 78 (C4 = 60)."""
    m = re.fullmatch(r"([A-G])([#b]?)(\d)", name)
    return PITCH[m[1]] + {"#": 1, "b": -1, "": 0}[m[2]] + 12 * (int(m[3]) + 1)


def hz(m):
    return 440.0 * 2.0 ** ((np.asarray(m, float) - 69) / 12)


def parse(seq, steps=16):
    """'E5:3 G5:3 -:2 …' → [(midi | (midi, …) | None, 칸수)]. 합이 steps 가 아니면 실패 — 마디 오타를 잡는다."""
    out = []
    for tok in seq.split():
        name, n = tok.split(":")
        pitch = None if name == "-" else tuple(map(midi, name.split("+")))
        out.append((pitch[0] if pitch and len(pitch) == 1 else pitch, int(n)))
    total = sum(n for _, n in out)
    assert total == steps, f"{total}칸 != {steps}칸: {seq}"
    return out


def chord(sym):
    """'Em' / 'B7' / 'C' → (루트 pitch class, 구성음 간격)."""
    m = re.fullmatch(r"([A-G][#b]?)(m|7)?", sym)
    return midi(m[1] + "0") % 12, {"m": (0, 3, 7), "7": (0, 4, 7, 10), None: (0, 4, 7)}[m[2]]
#endregion

#region 음원
def _blep(t, dt):
    """PolyBLEP 보정항 — 위상 0 의 계단을 2샘플에 걸쳐 둥글린다."""
    y = np.zeros_like(t)
    m = t < dt
    x = t[m] / dt[m]
    y[m] = x + x - x * x - 1
    m = t > 1 - dt
    x = (t[m] - 1) / dt[m]
    y[m] = x * x + x + x + 1
    return y


def pulse(f, duty):
    """펄스파. f = 샘플별 주파수. 평균 0(DC 없음), 피크투피크 1."""
    dt = f / SR
    t = np.cumsum(dt) % 1.0
    y = np.where(t < duty, 1.0, -1.0) + _blep(t, dt) - _blep((t - duty) % 1.0, dt)
    return (y - (2 * duty - 1)) * 0.5


def square(f):
    return pulse(f, 0.5)


TRI = np.r_[np.arange(15, -1, -1), np.arange(16)] / 15.0 - 0.5   # NES 삼각파: 4비트 32계단, 평균 0


def triangle(f):
    return TRI[(np.cumsum(f / SR) % 1.0 * 32).astype(int)]


def noise(rng, n, rate):
    """샘플앤홀드 1비트 노이즈 — NES 노이즈 채널 흉내. rate 가 낮을수록 거칠고 어둡다."""
    return rng.choice((-0.5, 0.5), int(n * rate / SR) + 1)[(np.arange(n) * rate / SR).astype(int)]


def tone(osc, pitch, n, gap=0.012, attack=0.003, decay=0.1, sustain=0.7, release=0.008, vib=0.0):
    """n 샘플 칸에 음 하나. 어택→감쇠→서스테인 후 릴리스로 정확히 0, 칸 끝 gap 초는 비운다 — 칸 밖으로 새지 않는다."""
    n_rel = int(release * SR)
    n_on = max(n - n_rel - int(gap * SR), 1)
    t = np.arange(n_on + n_rel) / SR
    if isinstance(pitch, tuple):   # 한 채널 화음: 1/60초마다 구성음을 돈다
        f = hz(pitch)[(t * 60).astype(int) % len(pitch)]
    else:
        f = np.full(t.size, hz(pitch))
    if vib:   # 딜레이 비브라토: 0.15초 뒤부터 0.15초에 걸쳐 5.5Hz 로 들어온다
        f = f * (1 + vib * np.sin(2 * np.pi * 5.5 * t) * np.clip(t / 0.15 - 1, 0, 1))
    e = np.minimum(sustain + (1 - sustain) * np.exp(-t / decay), t / attack)
    e[n_on:] *= np.linspace(1, 0, n_rel + 1)[1:]
    return osc(f) * e


def hit_env(n, tau):
    """타악기 엔벨로프: 1ms 어택, 지수 감쇠, 마지막 5ms 에서 0."""
    t = np.arange(n) / SR
    e = np.minimum(np.exp(-t / tau), t / 0.001)
    k = int(0.005 * SR)
    e[-k:] *= np.linspace(1, 0, k)
    return e


def kick(rng):   # 삼각파 피치 드롭 + 짧은 노이즈 클릭
    n = int(0.16 * SR)
    t = np.arange(n) / SR
    return triangle(50 + 130 * np.exp(-t / 0.02)) * hit_env(n, 0.05) + 0.5 * noise(rng, n, 5000) * hit_env(n, 0.004)


def snare(rng):   # 노이즈 + 삼각파 몸통
    n = int(0.2 * SR)
    t = np.arange(n) / SR
    return noise(rng, n, 9000) * hit_env(n, 0.045) + 0.6 * triangle(170 + 60 * np.exp(-t / 0.01)) * hit_env(n, 0.02)


def hat(rng, tau):
    n = int(6 * tau * SR)
    return noise(rng, n, 22050) * hit_env(n, tau)


def crash(rng):
    n = int(1.2 * SR)
    return noise(rng, n, 15000) * hit_env(n, 0.3)


KIT = {   # 드럼 패턴 문자 → [(소리, 레벨)]
    "K": [(kick, 1.3)],
    "S": [(snare, 0.7)],
    "h": [(lambda r: hat(r, 0.018), 0.3)],
    "o": [(lambda r: hat(r, 0.06), 0.25)],
    "X": [(kick, 1.3), (crash, 0.35)],
}


def lowpass(x, fc=12000, taps=63, loop=False):
    """NES 출력단 같은 완만한 저역통과(윈도 싱크 FIR). loop 면 원형으로 걸어 이음매를 보존한다."""
    k = np.sinc(2 * fc / SR * (np.arange(taps) - taps // 2)) * np.hanning(taps)
    h = taps // 2
    pad = (x[-h:], x, x[:h]) if loop else (np.zeros(h), x, np.zeros(h))
    return np.convolve(np.concatenate(pad), k / k.sum(), mode="valid")
#endregion

#region 시퀀서
def place(buf, y, at):
    buf[at:at + y.size] += y


def play(buf, events, pos, step, osc, **kw):
    """events 를 pos(칸 인덱스)부터 차례로 buf 에 더한다. step = 한 칸의 샘플 수."""
    for pitch, n in events:
        if pitch is not None:
            a = round(pos * step)
            place(buf, tone(osc, pitch, round((pos + n) * step) - a, **kw), a)
        pos += n


def beat(buf, pattern, pos, step, rng):
    for i, ch in enumerate(pattern):
        for sound, level in KIT.get(ch, ()):
            place(buf, level * sound(rng), round((pos + i) * step))


def voicing(sym, lo):
    """루트를 [lo, lo+12) 에 두고 위로 쌓는다. 3화음이면 옥타브를 더해 4음."""
    root, ivs = chord(sym)
    r = lo + (root - lo) % 12
    v = [r + i for i in ivs]
    return v if len(v) == 4 else v + [r + 12]


def harmony(lead, sym):
    """리드보다 단3도 이상 아래의 가장 가까운 코드 구성음 — 늘 협화. 같은 음이 이어지면 하나로 묶는다."""
    root, ivs = chord(sym)
    pcs = {(root + i) % 12 for i in ivs}
    out = []
    for m, n in lead:
        h = None if m is None else next(x for x in range(m - 3, m - 16, -1) if x % 12 in pcs)
        if out and out[-1][0] == h:
            out[-1] = (h, out[-1][1] + n)
        else:
            out.append((h, n))
    return out


def bassline(sym, mode):
    """8분음표 베이스. oct = 옥타브 튕기기(긴장), walk = 1-5-8-5(경쾌)."""
    r = 36 + chord(sym)[0]   # C2..B2
    pat = {"oct": (0, 12), "walk": (0, 7, 12, 7)}[mode]
    return [(r + pat[i % len(pat)], 2) for i in range(8)]
#endregion

#region BGM 악보
BPM = 168
DRUM = {
    "A":  "K.h.S.h.K.K.S.h.",
    "Ax": "X.h.S.h.K.K.S.h.",
    "Af": "K.h.S.h.K.SSS.SS",
    "B":  "K.o.S.o.K.oKS.o.",
    "Bx": "X.o.S.o.K.oKS.o.",
    "Bf": "K.o.S.o.K.S.SSSS",
}
PHRASES = {   # 4마디 단위: (코드, 펄스2, 베이스, 드럼, 리드)
    "A1": ("Em Em C D", "arp", "oct", "Ax A A A", [
        "E5:3 G5:3 B5:2 A5:2 G5:2 F#5:2 G5:2",
        "E5:3 B4:3 E5:2 F#5:8",
        "E5:3 G5:3 C6:2 B5:2 A5:2 G5:2 A5:2",
        "F#5:3 D5:3 F#5:2 A5:8"]),
    "A2": ("Em Em Am B7", "arp", "oct", "A A A Af", [
        "B5:3 G5:3 E5:2 B5:2 C6:2 B5:2 A5:2",
        "G5:3 E5:3 G5:2 A5:2 G5:2 F#5:2 E5:2",
        "A5:3 C6:3 E6:2 D6:2 C6:2 B5:2 A5:2",
        "B5:6 A5:2 F#5:4 D#5:4"]),
    "A3": ("Em Em C D", "arp", "oct", "A A A Af", [   # 전조 준비: D 로 올라가 G장조의 V 에서 멈춘다
        "B5:3 G5:3 E5:2 B5:2 C6:2 B5:2 A5:2",
        "G5:3 E5:3 G5:2 B5:2 C6:2 D6:2 E6:2",
        "E6:6 D6:2 C6:4 G5:4",
        "A5:3 B5:3 C6:2 D6:8"]),
    "B1": ("G D Em C", "harm", "walk", "Bx B B B", [
        "G5:2 B5:2 D6:3 B5:1 C6:2 B5:2 A5:2 G5:2",
        "F#5:2 A5:2 D6:3 A5:1 B5:2 A5:2 G5:2 F#5:2",
        "G5:2 B5:2 E6:3 B5:1 C6:2 B5:2 A5:2 G5:2",
        "A5:2 B5:2 C6:4 E6:4 D6:4"]),
    "B2": ("G D C D", "harm", "walk", "B B B Bf", [
        "G5:2 B5:2 D6:3 B5:1 C6:2 B5:2 A5:2 G5:2",
        "F#5:2 A5:2 D6:3 A5:1 B5:2 A5:2 G5:2 F#5:2",
        "E5:2 G5:2 C6:3 G5:1 A5:2 B5:2 C6:2 D6:2",
        "E6:3 D6:3 C6:2 A5:4 F#5:4"]),
    "B3": ("Em D C B7", "harm", "oct", "A A A Af", [   # 안달루시안 하행으로 단조 복귀, D# 이 1마디 E 로 풀린다
        "B5:3 G5:3 E5:2 B5:2 C6:2 B5:2 A5:2",
        "A5:3 F#5:3 D5:2 A5:2 B5:2 A5:2 G5:2",
        "G5:3 E5:3 C5:2 G5:2 A5:2 G5:2 A5:2",
        "B5:3 A5:3 G5:2 F#5:2 E5:2 D#5:4"]),
}
SONG = [("A1", .25), ("A2", .25), ("A1", .125), ("A3", .125), ("B1", .25), ("B2", .25), ("B1", .25), ("B3", .25)]   # (구간, 리드 듀티)
MIX = {"lead": 0.9, "pulse2": 0.4, "bass": 0.9, "drums": 1.2}


def render_bgm():
    rng = np.random.default_rng(SEED)
    step = SR * 60 / BPM / 4
    total = round(len(SONG) * 4 * 16 * step)
    tr = {k: np.zeros(total + SR) for k in MIX}
    pos = 0
    for name, duty in SONG:
        chords, p2, bmode, drums, bars = PHRASES[name]
        for sym, bar, pat in zip(chords.split(), bars, drums.split(), strict=True):
            lead = parse(bar)
            play(tr["lead"], lead, pos, step, lambda f: pulse(f, duty), decay=0.12, vib=0.009)
            if p2 == "arp":
                v = voicing(sym, 55)   # 루트 G3..F#4 — 베이스 위, 리드(5옥타브) 아래
                play(tr["pulse2"], [(v[i % 4], 1) for i in range(16)], pos, step, square, gap=0.02, decay=0.05, sustain=0.45)
            else:
                play(tr["pulse2"], harmony(lead, sym), pos, step, square, gap=0.015, decay=0.25, sustain=0.6)
            play(tr["bass"], bassline(sym, bmode), pos, step, triangle, gap=0.045, sustain=1.0)
            beat(tr["drums"], DRUM[pat], pos, step, rng)
            pos += 16
    mix = sum(MIX[k] * tr[k] for k in MIX)
    mix[:SR] += mix[total:]   # 원형 루프: 끝을 넘친 꼬리를 맨 앞에 겹친다
    mix = mix[:total]
    return lowpass(mix - mix.mean(), loop=True)   # 삼각파 음 끝의 잔여 DC 제거(루프라 상수 빼기가 정확)
#endregion

#region 징글 악보 (0.08초 칸, 12칸 달리기 + 16칸 울림 = 2.24초)
J_STEP = 0.08 * SR
JINGLE = [   # (채널, 음원, 달리기, 마지막 울림, 추가 인자)
    ("lead", lambda f: pulse(f, 0.25), "G4:1 C5:1 E5:1 G5:1 C6:2 G5:1 C6:1 A5:2 B5:2", "C6:16", {"vib": 0.009}),
    ("pulse2", square, "E4:1 G4:1 C5:1 E5:1 E5:4 F5:2 G5:2", "C5+E5+G5:16", {}),
    ("bass", triangle, "C3:4 C3:2 G2:2 F2:2 G2:2", "C3:16", {"sustain": 1.0}),
]
J_DRUM = "h.h.S...S.S.X..............."


def render_jingle():
    rng = np.random.default_rng(SEED)
    tr = {k: np.zeros(round(28 * J_STEP)) for k in MIX}
    ring = {"gap": 0.0, "decay": 0.35, "sustain": 0.0, "release": 0.03}   # 마지막 화음: 자연 감쇠로 무음까지
    for key, osc, run, last, kw in JINGLE:
        play(tr[key], parse(run, 12), 0, J_STEP, osc, gap=0.01, **kw)
        play(tr[key], parse(last, 16), 12, J_STEP, osc, **{**kw, **ring})
    beat(tr["drums"], J_DRUM, 0, J_STEP, rng)
    return lowpass(sum(MIX[k] * tr[k] for k in MIX))
#endregion

#region 마스터링 / 검증
RAW = ["-f", "f32le", "-ar", str(SR), "-ac", "1"]


def ff(args, x=None):
    r = subprocess.run([FFMPEG, "-hide_banner", "-nostats", *args], capture_output=True,
                       input=None if x is None else x.astype("<f4").tobytes())
    if r.returncode:
        sys.exit(r.stderr.decode(errors="replace")[-2000:])
    return r


def loudness(x):
    """ffmpeg ebur128 → (통합 LUFS, 트루피크 dBTP)."""
    err = ff([*RAW, "-i", "-", "-af", "ebur128=peak=true", "-f", "null", "-"], x).stderr.decode()
    return float(re.findall(r"I:\s+(-?[\d.]+) LUFS", err)[-1]), float(re.findall(r"Peak:\s+(-?[\d.]+) dBFS", err)[-1])


def master(x, target, path):
    """192kbps MP3 로 굽고 디코드한 결과로 다시 재서, target LUFS 와 트루피크 ≤ -1 dBTP 를 둘 다 만족할 때까지 게인을 고친다.
    (MP3 가 고역을 깎아 라우드니스가 조금 변하므로 굽기 전 측정만으로는 맞지 않는다.)"""
    gain = 10 ** ((target - loudness(x)[0]) / 20)
    codec = ["-c:a", "pcm_s16le"] if path.endswith(".wav") else ["-c:a", "libmp3lame", "-b:a", "192k"]
    for _ in range(5):
        ff(["-y", *RAW, "-i", "-", *codec, path], x * gain)
        y = np.frombuffer(ff(["-i", path, *RAW[:2], "-ac", "1", "-"]).stdout, "<f4")
        assert y.size == x.size, f"{path}: 디코드 길이 {y.size} != {x.size} — 갭리스(LAME) 헤더가 안 먹었다"
        lufs, tp = loudness(y)
        db = min(target - lufs, -1.05 - tp)   # 피크가 막으면 목표보다 조금 작게 둔다
        if tp <= -1.0 and abs(db) < 0.1:
            return y
        gain *= 10 ** (db / 20)
    sys.exit(f"{path}: 라우드니스/피크가 수렴하지 않음 ({lufs} LUFS, {tp} dBTP)")


def report(name, y, loop=False):
    I, tp = loudness(y)
    print(f"{name}: {y.size / SR:.3f}s ({y.size} smp)  {I:.1f} LUFS  true peak {tp:.1f} dBTP"
          f"  sample peak {20 * np.log10(np.abs(y).max()):.2f} dBFS  DC {y.mean():+.1e}")
    if loop:
        k = int(0.01 * SR)
        rms = lambda a: 20 * np.log10(np.sqrt(np.mean(np.square(a, dtype=float))) + 1e-12)
        d = np.abs(np.diff(y))
        jump = abs(float(y[0]) - float(y[-1]))   # 2회 반복 시 이음매에서 한 샘플 사이의 도약
        print(f"  loop seam |first-last| {jump:.4f} (click if > 0.05; in-track |d| median {np.median(d):.4f},"
              f" p99 {np.percentile(d, 99):.4f})  RMS first 10ms {rms(y[:k]):.1f} dBFS / last 10ms {rms(y[-k:]):.1f} dBFS")
        assert jump < 0.05, "루프 이음매에 클릭"


def main():
    if not FFMPEG:
        sys.exit("ffmpeg 가 PATH 에 없다")
    bgm = master(render_bgm(), -16, OUT_BGM)
    report(os.path.relpath(OUT_BGM, BASE), bgm, loop=True)
    report(os.path.relpath(OUT_SFX, BASE), master(render_jingle(), -14, OUT_SFX))
    if "--preview" in sys.argv:
        out = os.path.join(sys.argv[sys.argv.index("--preview") + 1], "BGM_MinimonCapture_x2.wav")
        ff(["-y", *RAW, "-i", "-", out], np.tile(bgm, 2))
        print("preview:", out)
#endregion


if __name__ == "__main__":
    main()
