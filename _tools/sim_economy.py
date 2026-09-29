"""v2 경제 시뮬레이터 — 레벨 곡선 배수·훈련 비용을 정하는 근거를 만든다. 데이터를 바꾸지 않는다.

현재 데이터(LevelData/CreatureData/TravelNode/TravelCondition JSON)에서 레벨마다 '해금된 최선의 사냥터'의
처치당 경험치·골드를 구하고(audit_progression.py 와 같은 규칙), 플레이어 유형별 하루 처치 수로 레벨 도달 일수를 낸다.

  하루 처치 수 = 접속 중 사냥(분 × ACTIVE_KPM) + 오프라인 정산(회수 × 8시간 × 900마리/시간 × 50%)
  (오프라인 규칙은 GameManager: OFFLINE_KILL_INTERVAL 4초, OFFLINE_RATE 0.5, OFFLINE_MAX_HOURS 8)

  사용: python _tools/sim_economy.py            # 현재 곡선 vs v2 곡선 비교표
        python _tools/sim_economy.py --md       # 설계서에 붙일 마크다운 표
"""
import math
import sys

import audit_progression as a

ACTIVE_KPM = 12            # 접속 중 분당 처치(메뉴·연출 포함 실측 감안, 이론치 15)
OFFLINE_KILLS_PER_CLAIM = 8 * 900 * 0.5   # 3,600
PLAYERS = {                # (접속 사냥 분/일, 오프라인 정산 회수/일)
    "가벼움": (20, 1),
    "보통":   (60, 2),
    "헤비":   (180, 2),
}

# v2 레벨 곡선: 필요 경험치 × m(L). 초반은 빠르게 두고 20~100 사이에서 늘어난 뒤 평탄해진다.
CURVE_MAX, CURVE_TAU = 18.0, 40.0


def curve_mult(level):
    return 1.0 + (CURVE_MAX - 1.0) * (1.0 - math.exp(-(level - 1) / CURVE_TAU))


# 훈련(골드 강화): 레벨 L 로 올리는 비용 = BASE + COEF × L^POW, 상한 = 플레이어 레벨 × CAP_PER_LEVEL
TRAIN_BASE, TRAIN_COEF, TRAIN_POW, TRAIN_STATS, CAP_PER_LEVEL = 10, 0.8, 1.65, 4, 2


def train_cost(level):
    return round(TRAIN_BASE + TRAIN_COEF * level ** TRAIN_POW)


def best_node_rates():
    nodes = {r['Id']: r for r in a.rows('TravelNodeData')}
    gates = {r['NodeId']: r for r in a.rows('TravelConditionData')}
    mons = a.rows('CreatureData')
    pools = {}
    for m in mons:
        pools.setdefault(m['Habitat'], []).append(m)
    rates = {}
    for lv in range(1, 301):
        best = None
        for n in nodes.values():
            if n['AreaType'] != 'Battle' or n['Name'] not in pools:
                continue
            ch, ok = n, True
            while ch:
                if gates.get(ch['Id'], {}).get('Value', 1) > lv:
                    ok = False
                ch = nodes.get(ch['ParentId'])
            if not ok:
                continue
            pool = pools[n['Name']]
            w = sum(m['EncounterRate'] for m in pool)
            exp = sum(m['TotalExp'] * m['EncounterRate'] for m in pool) / w
            gold = sum(m['RewardGold'] * m['EncounterRate'] for m in pool) / w
            if best is None or exp > best[0]:
                best = (exp, gold)
        rates[lv] = best
    return rates


def cumulative(rates, levels, mult):
    """레벨 L 에 도달하기까지 누적 처치·골드."""
    kills = gold = 0.0
    out = {1: (0.0, 0.0)}
    for lv in range(1, 300):
        exp, g = rates[lv]
        k = levels[lv] * mult(lv) / exp
        kills += k
        gold += k * g
        out[lv + 1] = (kills, gold)
    return out


def day_of(kills, player):
    active_min, claims = PLAYERS[player]
    per_day = active_min * ACTIVE_KPM + claims * OFFLINE_KILLS_PER_CLAIM
    first_session = 30 * ACTIVE_KPM          # 첫날은 오프라인이 없다 — 30분 플레이
    if kills <= first_session:
        return kills / first_session * (30 / 1440)
    return 1 + (kills - first_session) / per_day


def train_total(cap):
    return TRAIN_STATS * sum(train_cost(L) for L in range(1, cap + 1))


def main():
    md = "--md" in sys.argv
    levels = {r['Level']: r['RequiredExp'] for r in a.rows('LevelData')}
    rates = best_node_rates()
    now = cumulative(rates, levels, lambda lv: 1.0)
    v2 = cumulative(rates, levels, curve_mult)
    marks = (20, 30, 50, 100, 150, 200, 250, 300)

    head = "| 레벨 | 배수 | 누적 처치(현재) | 누적 처치(v2) | 누적 골드(v2) | 가벼움 | 보통 | 헤비 | 훈련 전부 올린 비용(상한 레벨×2) |"
    print(head)
    print("|---:|---:|---:|---:|---:|---:|---:|---:|---:|")
    for lv in marks:
        k0, _ = now[lv]
        k, g = v2[lv]
        days = [day_of(k, p) for p in PLAYERS]
        cell = lambda d: f"{d * 24:.1f}시간" if d < 1 else f"{d:.1f}일"
        print(f"| {lv} | {curve_mult(lv):.1f} | {k0:,.0f} | {k:,.0f} | {g:,.0f} | "
              + " | ".join(cell(d) for d in days) + f" | {train_total(lv * CAP_PER_LEVEL):,.0f} |")
    if not md:
        print(f"\n하루 처치: " + ", ".join(f"{p} {m * ACTIVE_KPM + c * OFFLINE_KILLS_PER_CLAIM:,.0f}"
                                         for p, (m, c) in PLAYERS.items()))
        print("훈련 비용 표본:", ", ".join(f"L{L}={train_cost(L):,}" for L in (1, 10, 50, 100, 300, 600)))


if __name__ == "__main__":
    main()
