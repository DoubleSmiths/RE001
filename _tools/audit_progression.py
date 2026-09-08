"""현재 런타임 데이터의 성장·제작·물약·여정 연결을 검사한다. 데이터를 바꾸지 않는다."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / 'Assets/@Resources/Data/JsonData'


def rows(name):
    return next(iter(json.loads((DATA / (name + '.json')).read_text(encoding='utf-8-sig')).values()))


def main():
    items = {r['DataId'] for name in ('InventoryItem', 'RequisiteItem') for r in rows(name)}
    for name in ('HPPotion', 'SpeedPotion'):
        for r in rows(name):
            assert len(r['NeededItemIDs']) == len(r['NeededItemAmounts']), r
            assert all(i in items for i in r['NeededItemIDs']), (name, r['DataId'])
    nodes = {r['Id']: r for r in rows('TravelNodeData')}
    gates = {r['NodeId']: r for r in rows('TravelConditionData')}
    assert all(r['ParentId'] == 0 or r['ParentId'] in nodes for r in nodes.values())
    assert all(r['Type'] == 'LEVEL_REQUIRED' and 1 <= r['Value'] <= 300 for r in gates.values())
    levels = {r['Level']: r['RequiredExp'] for r in rows('LevelData')}
    assert set(levels) == set(range(1, 301))
    monsters = rows('CreatureData')
    print('level,node,expected_kills_per_level,expected_gold_per_kill')
    for lv in (1, 10, 31, 60, 100, 200, 299):
        candidates = []
        for n in nodes.values():
            if n['AreaType'] != 'Battle':
                continue
            chain = n
            unlocked = True
            while chain:
                if gates.get(chain['Id'], {}).get('Value', 1) > lv:
                    unlocked = False
                chain = nodes.get(chain['ParentId'])
            pool = [m for m in monsters if m['Habitat'] == n['Name']]
            if not unlocked or not pool:
                continue
            weight = sum(m['EncounterRate'] for m in pool)
            exp = sum(m['TotalExp'] * m['EncounterRate'] for m in pool) / weight
            gold = sum(m['RewardGold'] * m['EncounterRate'] for m in pool) / weight
            candidates.append((exp, n['Id'], gold))
        exp, node, gold = max(candidates)
        print(f'{lv},{node},{levels[lv] / exp:.2f},{gold:.2f}')
    recipes = rows('ForgeRecipeData')
    assert all(r['Gold'] > 0 and 1 <= r['ReqLevel'] <= 300 for r in recipes)
    print(f'PASS: 8 potion rows, {len(nodes)} travel nodes, {len(gates)} level gates, {len(recipes)} recipes')


if __name__ == '__main__':
    main()
