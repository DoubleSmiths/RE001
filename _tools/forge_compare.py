# -*- coding: utf-8 -*-
"""강화 연출을 원작 영상과 자동으로 대조한다.

한 번 고칠 때마다 눈으로 훑는 대신 이걸 돌린다:
  1) 우리 녹화를 원작과 같은 자(344x854)로 정규화
  2) 무대 휘도 곡선에서 **망치 3타**를 찾아 그걸로 시간축을 맞춤
  3) 사건별 시각·강도를 표로 비교
  4) 정렬된 프레임 스트립 이미지를 남김

  python _tools/forge_compare.py --out AI/forge_ref/cmp.png
"""
import argparse
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from forge_norm import load_our, load_ref, normalize, REF_W, REF_H, OUR_DIR  # noqa: E402

# ⚠️ 녹화 프레임률은 고정이 아니다 — 캡처가 무거워 런마다 15~17fps 사이로 흔들린다.
#    15로 못 박아 뒀더니 517장 녹화본에서 타격 간격이 1.00초 대신 1.20초로 읽혔다.
#    실제 장수 / 녹화 길이로 매번 다시 구한다.
DEMO_SECONDS = 30.0
OUR_FPS = 15.0   # 실행 시 실제 장수로 덮어쓴다
REF_FPS = 10.0
STAGE = (slice(380, 500), slice(110, 240))   # 모루 둘레 — 타격 섬광이 여기서 터진다


def curve(loader, lo, hi, norm=False):
    """(프레임번호, 전체휘도, 무대휘도) 목록."""
    out = []
    for n in range(lo, hi):
        try:
            im = loader(n)
        except (IOError, OSError):
            break
        if norm:
            im = normalize(im)
        a = np.asarray(im.convert('L')).astype(float)
        out.append((n, a.mean(), a[STAGE].mean()))
    return out


def blackout(rows, fps):
    """전체 휘도가 절반 아래로 떨어지는 첫 프레임 = 무대 암전."""
    base = np.median([r[1] for r in rows[:10]])
    for n, m, _ in rows:
        if m < base * 0.65:
            return n
    return None


def strikes(rows, fps, after, span=6.0, thresh=60.0):
    """무대 휘도의 국소 봉우리 = 망치 타격. 0.5초 안의 중복은 하나로 묶는다."""
    peaks = []
    lo = after
    hi = after + int(span * fps)
    win = max(1, int(0.25 * fps))
    for i, (n, _, s) in enumerate(rows):
        if not (lo <= n <= hi) or s < thresh:
            continue
        lo_i, hi_i = max(0, i - win), min(len(rows), i + win + 1)
        if s >= max(r[2] for r in rows[lo_i:hi_i]):
            if peaks and (n - peaks[-1]) < 0.5 * fps:
                continue
            peaks.append(n)
    return peaks


def summarize(rows, fps, tag):
    bo = blackout(rows, fps)
    if bo is None:
        return None
    st = strikes(rows, fps, bo)
    # 백색광: 전체 휘도 최대
    tail = [r for r in rows if r[0] > bo]
    peak = max(tail, key=lambda r: r[1]) if tail else None
    # 결과 안정: 백색광 이후 1.5초 지난 지점의 전체/무대 휘도
    settle = None
    if peak:
        want = peak[0] + int(2.0 * fps)
        for r in tail:
            if r[0] >= want:
                settle = r
                break
    return dict(tag=tag, fps=fps, blackout=bo, strikes=st, peak=peak, settle=settle, rows=rows,
                stamps=STAMPS if tag == '우리' else None)


STAMPS = {}


def load_stamps():
    """녹화기가 남긴 프레임별 실제 시각. 없으면 평균 fps로 물러선다."""
    p = os.path.join(OUR_DIR, 'frames.tsv')
    if not os.path.exists(p):
        return {}
    out = {}
    for i, line in enumerate(open(p, encoding='utf-8')):
        if i == 0:
            continue
        a = line.split('	')
        if len(a) >= 2:
            out[int(a[0])] = float(a[1])
    return out


def rel(s, n):
    if s.get('stamps'):
        st = s['stamps']
        if n in st and s['blackout'] in st:
            return st[n] - st[s['blackout']]
    return (n - s['blackout']) / s['fps']


def report(a, b):
    """a=원작, b=우리"""
    L = []
    L.append('%-26s %14s %14s %9s' % ('사건', '원작', '우리', '차이'))
    L.append('-' * 68)

    def row(name, va, vb, unit='s', fmt='%+.2f'):
        if va is None or vb is None:
            L.append('%-26s %14s %14s %9s' % (name, va, vb, '?'))
            return
        L.append('%-26s %14s %14s %9s' %
                 (name, (fmt % va) + unit, (fmt % vb) + unit, '%+.2f' % (vb - va)))

    L.append('%-26s %14s %14s' % ('암전(기준 0)', 'f%d' % a['blackout'], 'f%d' % b['blackout']))
    for i in range(max(len(a['strikes']), len(b['strikes']))):
        na = a['strikes'][i] if i < len(a['strikes']) else None
        nb = b['strikes'][i] if i < len(b['strikes']) else None
        row('망치 %d타' % (i + 1),
            rel(a, na) if na else None, rel(b, nb) if nb else None)
    row('백색광 정점', rel(a, a['peak'][0]) if a['peak'] else None,
        rel(b, b['peak'][0]) if b['peak'] else None)
    L.append('')
    L.append('%-26s %14s %14s %9s' % ('밝기', '원작', '우리', '차이'))
    L.append('-' * 68)
    if a['peak'] and b['peak']:
        L.append('%-26s %14.1f %14.1f %9.1f' % ('백색광 전체휘도', a['peak'][1], b['peak'][1], b['peak'][1] - a['peak'][1]))
    if a['settle'] and b['settle']:
        L.append('%-26s %14.1f %14.1f %9.1f' % ('결과화면 전체휘도', a['settle'][1], b['settle'][1], b['settle'][1] - a['settle'][1]))
        L.append('%-26s %14.1f %14.1f %9.1f' % ('결과화면 무대휘도', a['settle'][2], b['settle'][2], b['settle'][2] - a['settle'][2]))

    # 담금질 구간 — 마지막 타격에서 백색광 정점까지 무대가 얼마나 달아오르는가
    def charge(s):
        if not s['strikes'] or not s['peak']:
            return None
        lo, hi = s['strikes'][-1], s['peak'][0]
        seg = [r for r in s['rows'] if lo <= r[0] <= hi]
        return ((hi - lo) / s['fps'], seg[0][2], max(r[2] for r in seg)) if seg else None

    ca, cb = charge(a), charge(b)
    if ca and cb:
        L.append('')
        L.append('%-26s %14s %14s %9s' % ('담금질(막타→백색광)', '원작', '우리', '차이'))
        L.append('-' * 68)
        L.append('%-26s %13.2fs %13.2fs %9.2f' % ('  걸린 시간', ca[0], cb[0], cb[0] - ca[0]))
        L.append('%-26s %14.1f %14.1f %9.1f' % ('  무대 시작휘도', ca[1], cb[1], cb[1] - ca[1]))
        L.append('%-26s %14.1f %14.1f %9.1f' % ('  무대 최대휘도', ca[2], cb[2], cb[2] - ca[2]))
    return '\n'.join(L)


def frame_at(s, t):
    """암전 기준 t초에 해당하는 프레임 번호."""
    st = s.get('stamps')
    if st and s['blackout'] in st:
        want = st[s['blackout']] + t
        best, bd = s['blackout'], 1e9
        for n, v in st.items():
            d = abs(v - want)
            if d < bd:
                bd, best = d, n
        return best
    return int(round(s['blackout'] + t * s['fps']))


def strip(a, b, out, marks=None, crop=(60, 330, 230, 210), zoom=2):
    """암전을 0으로 맞춘 같은 시각의 프레임을 위=원작 / 아래=우리 로 늘어놓는다."""
    if marks is None:
        marks = [0.0, 0.5, 0.9, 1.4, 1.9, 2.4, 2.9, 3.6, 4.3, 4.7, 5.2, 6.0, 7.0, 8.5]
    x, y, w, h = crop
    sheet = Image.new('RGB', (w * len(marks), h * 2 + 6), (40, 40, 40))
    for i, t in enumerate(marks):
        rn = int(round(a['blackout'] + t * a['fps']))
        on = frame_at(b, t)
        try:
            sheet.paste(load_ref(rn).crop((x, y, x + w, y + h)), (i * w, 0))
        except (IOError, OSError):
            pass
        try:
            sheet.paste(normalize(load_our(on)).crop((x, y, x + w, y + h)), (i * w, h + 6))
        except (IOError, OSError):
            pass
    sheet = sheet.resize((sheet.width * zoom, sheet.height * zoom), Image.NEAREST)
    sheet.save(out)
    return [round(t, 2) for t in marks]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--out', default='AI/forge_ref/cmp.png')
    ap.add_argument('--marks')
    a = ap.parse_args()

    import glob
    shots = len(glob.glob(os.path.join(OUR_DIR, '*.png')))
    fps = (shots / DEMO_SECONDS) if shots else OUR_FPS
    print('녹화 %d장 -> %.2f fps' % (shots, fps))
    print()

    global STAMPS
    STAMPS = load_stamps()
    if STAMPS:
        print('프레임 시각 기록 %d개 사용' % len(STAMPS))

    ref = summarize(curve(load_ref, 150, 300), REF_FPS, '원작')
    our = summarize(curve(load_our, 120, 460, norm=True), fps, '우리')
    if ref is None or our is None:
        print('암전을 찾지 못했다 — 녹화 구간을 확인하라')
        return

    print(report(ref, our))
    marks = [float(v) for v in a.marks.split(',')] if a.marks else None
    os.makedirs(os.path.dirname(a.out) or '.', exist_ok=True)
    used = strip(ref, our, a.out, marks)
    print('\n스트립: %s   (암전 기준 %s초)' % (a.out, used))


if __name__ == '__main__':
    main()
