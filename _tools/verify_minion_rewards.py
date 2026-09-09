"""포획 보상의 도달성·데이터 연결 검증. --json은 Unity 파싱 결과를 검사한다."""
import argparse
import json
from pathlib import Path

import openpyxl

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / 'Assets/@Resources/Data'


def load(name, runtime=False):
    if runtime:
        return next(iter(json.loads((DATA / 'JsonData' / (name + '.json')).read_text(encoding='utf-8-sig')).values()))
    wb = openpyxl.load_workbook(DATA / 'Excel' / (name + '.xlsx'), read_only=True, data_only=True)
    values = list(wb.active.values)
    wb.close()
    start = 1 if name == 'LocalizationData' else 3
    return [dict(zip(values[0], row)) for row in values[start:] if row[0] is not None]


def verify(runtime=False):
    creatures = load('CreatureData', runtime)
    minions = {r['DataId']: r for r in load('MinMonData', runtime)}
    skills = load('SkillData', runtime)
    ids = {r['DataId']: r for r in skills}
    assert len(ids) == len(skills), '중복 능력 ID'
    missing = [m for m in minions if m + 4000 not in ids]
    assert not missing, f'고유능력 보상 누락 {len(missing)}/{len(minions)}종: {missing[:8]}'
    localize = {r['TextKey']: r for r in load('LocalizationData', runtime)}
    for mid in minions:
        s = ids[mid + 4000]
        assert s['Source'] == 'Minion' and s['RequiredMinionId'] == mid, mid
        assert s['MemoryCapacity'] > 0 and s['MaxLevel'] >= 2, mid
        assert s['IconLabel'], mid
        for col in ('Name', 'Description'):
            assert s[col] in localize and localize[s[col]]['Kor'] and localize[s[col]]['Eng'], (mid, col)
    starter_habitat = next(c['Habitat'] for c in creatures if c['DataId'] == 2001)
    starters = [c['DataId'] for c in creatures if c['Habitat'] == starter_habitat]
    assert len(starters) == 4, starters
    for mid in starters:
        assert minions[mid].get('FirstInterestKillCount') == 3, mid
        assert minions[mid].get('FirstCatchChance') == 1, mid
        s = ids[mid + 4000]
        assert s['TriggerType'] == 'Active' and s['SkillActiveType'] == 'Repeat', (mid, s['TriggerType'])
    # 첫 포획부터 직접 쓰는 선택이 생기며, 기존 패시브 성장 보상은 그대로 남는다.
    effects = {ids[mid + 4000]['Effect'] for mid in starters}
    assert {'Heal', 'BonusDamage', 'Buff'} <= effects, effects
    for sid, stat, bonus, effect, effect_stat, value, duration in (
            (6001, 'MaxHp', 30, 'Heal', 'MaxHp', 18, 0),
            (6002, 'AtkSpeed', 10, 'Buff', 'AtkSpeed', 30, 8)):
        s = ids[sid]
        assert s['Category'] == 'Memory' and s['StatType'] == stat and s['StatBonus'] == bonus, sid
        assert s['MaxLevel'] == 2 and s['MemoryCapacity'] == 2, sid
        assert (s['Effect'], s['EffectStat'], s['EffectValue'], s['EffectDuration'], s['Cooldown']) == (
            effect, effect_stat, value, duration, 18), sid
    for m in minions.values():
        assert 0 <= m['CatchChance'] <= 1
        if m['DataId'] not in starters:
            assert not m.get('FirstInterestKillCount') and not m.get('FirstCatchChance'), m['DataId']
    print(f'PASS: 고유능력 {len(minions)}/{len(minions)}, 첫 지역 {len(starters)}종 포획, 효과 {sorted(effects)}')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--json', action='store_true')
    verify(parser.parse_args().json)
