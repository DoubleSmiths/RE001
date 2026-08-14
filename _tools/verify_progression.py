# -*- coding: utf-8 -*-
"""300레벨 진행 가능성 검증 — 세계 구조(마을/던전/몬스터/레벨곡선)의 무결성 점검.

확인하는 것
  1. 모든 전투 노드에 몬스터가 배치돼 있는가 (빈 던전 = 들어가도 아무것도 안 나옴)
  2. Lv1~300 모든 레벨에서 **진입 가능한 던전이 존재**하는가 (레벨 게이트 연속성)
  3. 각 레벨에서 레벨업까지 몇 마리를 잡아야 하는가 (난이도 곡선)

데이터를 고친 뒤 이걸 돌려서 진행이 막히는 구간이 없는지 본다.
실행: python _tools/verify_progression.py
"""
import json, io, os, sys

BASE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
D = os.path.join(BASE, "Assets", "@Resources", "Data", "JsonData")


def L(p, k):
    return json.load(io.open(os.path.join(D, p), encoding="utf-8-sig"))[k]


def main():
    nodes = L("TravelNodeData.json", "nodes")
    raw = L("TravelConditionData.json", "conditions")
    cond = {}
    if isinstance(raw, dict):
        cond = {int(k): v for k, v in raw.items()}
    else:
        for c in raw:
            cond.setdefault(c["NodeId"], []).append(c)
    creatures = L("CreatureData.json", "creatures")
    levels = {r["Level"]: r["RequiredExp"] for r in L("LevelData.json", "levels")}

    battle = []
    for n in nodes:
        if n.get("AreaType") != "Battle":
            continue
        req = 1
        for c in cond.get(n["Id"], []):
            if c["Type"] == "LEVEL_REQUIRED":
                req = max(req, int(c["Value"]))
        mons = [x for x in creatures if x["Habitat"] == n["Name"]]
        avg = sum(x["TotalExp"] for x in mons) / len(mons) if mons else 0.0
        battle.append((req, n["Name"], len(mons), avg))
    battle.sort()

    fail = False
    empty = [b for b in battle if b[2] == 0]
    print(f"전투 노드 {len(battle)}개 / 몬스터 없는 노드 {len(empty)}개")
    if empty:
        fail = True
        print("  [FAIL]", [e[1] for e in empty[:10]])

    gaps = []
    for lv in range(1, 301):
        if not [b for b in battle if b[0] <= lv and b[2] > 0]:
            gaps.append(lv)
    print(f"갈 곳 없는 레벨 {len(gaps)}개 {gaps[:10]}")
    if gaps:
        fail = True

    print("\n레벨별 진행")
    for lv in (1, 50, 100, 150, 200, 250, 300):
        avail = [b for b in battle if b[0] <= lv and b[2] > 0]
        best = max(avail, key=lambda b: b[3])
        print(f"  Lv{lv:3d}  진입가능 {len(avail):3d}노드  레벨업 {levels[lv]/max(1.0,best[3]):5.0f}킬  ({best[1]})")

    print("\n결과:", "❌ 진행 불가 구간 있음" if fail else "✅ 1~300레벨 전 구간 진행 가능")
    return 1 if fail else 0


if __name__ == "__main__":
    sys.exit(main())
