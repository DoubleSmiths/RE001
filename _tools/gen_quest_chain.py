# -*- coding: utf-8 -*-
"""메인 퀘스트를 300레벨까지 잇는다.

## 왜
  퀘스트가 7번("폴짝 젤리 5개 보고")에서 끝나 있었다. 그 뒤로는 아무 목표도 안 뜬다.
  레벨은 300까지 올라가는데 안내가 12레벨 언저리에서 멎는 셈이다.

## 어떻게
  이미 있는 3~7번의 리듬을 그대로 이어간다 — **모아 오기 → 협회 보고**의 반복.
  지역 25곳 중 미풍의 숲(21000)은 3~7번이 이미 쓰므로 나머지 24곳에 두 개씩 붙인다.
    · 모아 오기(ITEM_COUNT) : 그 지역 몬스터가 실제로 떨구는 재료(CreatureData.DropItemId)
    · 보고(BTN_CLICK)       : BTN_GUILD_QUEST — 4·7번과 같은 방식
  요구 수량은 지역 레벨대에 비례해 늘린다.
  보상은 강화 주문서 등급을 레벨대에 맞춰 준다(10011 → 10012 → 10013).

  Quest_Notify에도 같이 넣는다. 안 넣으면 어디로 가야 하는지 화살표가 안 뜬다.

사용: python _tools/gen_quest_chain.py       → QuestData.xlsx 갱신 후 Unity에서 ParseAllLocal
      python _tools/gen_quest_chain.py --dry
"""
import io
import json
import os
import sys
from collections import Counter

import openpyxl

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
JSON_DIR = os.path.join(BASE, "Assets", "@Resources", "Data", "JsonData")
XL = os.path.join(BASE, "Assets", "@Resources", "Data", "Excel", "QuestData.xlsx")

FIRST_NEW_ID = 8          # 1~7은 손으로 만든 도입부 — 건드리지 않는다
SKIP_REGION = 21000       # 미풍의 숲은 3~7번이 이미 쓴다


def load(name):
    with io.open(os.path.join(JSON_DIR, name), encoding="utf-8-sig") as f:
        d = json.load(f)
    return d[list(d.keys())[0]]


def build_plan():
    loc = {r["TextKey"]: r["Kor"] for r in load("LocalizationData.json")}
    nodes = load("TravelNodeData.json")
    creatures = load("CreatureData.json")
    conds = load("TravelConditionData.json")

    level_of = {c["NodeId"]: c["Value"] for c in conds if c["Type"] == "LEVEL_REQUIRED"}

    regions = [n for n in nodes if n["Category"] == "EXPLORE" and n["ParentId"] == 0]
    dungeons_of = {}
    for n in nodes:
        if n["ParentId"] != 0:
            dungeons_of.setdefault(n["ParentId"], []).append(n)

    # Habitat은 던전의 Name 키다 → 지역이 품은 던전들의 몬스터를 모아 드롭 재료를 고른다.
    hab_to_region = {}
    for rg in regions:
        for d in dungeons_of.get(rg["Id"], []):
            hab_to_region[d["Name"]] = rg["Id"]

    drops = {}
    for c in creatures:
        rid = hab_to_region.get(c.get("Habitat"))
        if rid and c.get("DropItemId"):
            drops.setdefault(rid, Counter())[c["DropItemId"]] += 1

    def region_level(rg):
        # 지역 자체에 레벨 조건이 없으면 소속 던전 중 가장 낮은 조건을 쓴다.
        cand = [level_of[rg["Id"]]] if rg["Id"] in level_of else []
        cand += [level_of[d["Id"]] for d in dungeons_of.get(rg["Id"], []) if d["Id"] in level_of]
        return min(cand) if cand else 1

    plan = []
    ordered = sorted((rg for rg in regions if rg["Id"] != SKIP_REGION), key=region_level)
    qid = FIRST_NEW_ID
    for rg in ordered:
        lv = region_level(rg)
        kor = loc.get(rg["Name"], rg["Name"])
        if rg["Id"] not in drops:
            continue
        item = drops[rg["Id"]].most_common(1)[0][0]
        count = 3 + lv // 20 * 2                       # 13레벨 3개 → 300레벨 33개
        scroll = 10011 if lv < 100 else 10012 if lv < 200 else 10013
        first_dungeon = sorted(dungeons_of.get(rg["Id"], []), key=lambda d: d["Id"])
        plan.append({
            "collect_id": qid,
            "report_id": qid + 1,
            "region": rg,
            "level": lv,
            "kor": kor,
            "item": item,
            "count": count,
            "scroll": scroll,
            "click_btn": rg.get("ClickBtnId") or "",
            "enter_btn": first_dungeon[0].get("EnterBtnId") if first_dungeon else "",
        })
        qid += 2
    return plan


def item_names():
    loc = {r["TextKey"]: r["Kor"] for r in load("LocalizationData.json")}
    return {r["DataId"]: loc.get(r.get("Name"), r.get("Name")) for r in load("InventoryItem.json")}


def clear_generated(ws, id_col=1):
    """FIRST_NEW_ID 이상 행을 지운다 — 다시 돌려도 중복이 쌓이지 않게."""
    for row in range(ws.max_row, 1, -1):
        v = ws.cell(row=row, column=id_col).value
        if isinstance(v, int) and v >= FIRST_NEW_ID:
            ws.delete_rows(row)


def main():
    dry = "--dry" in sys.argv
    plan = build_plan()
    names = item_names()

    if dry:
        for p in plan[:6]:
            print("Q%-3d %-14s Lv%-3d %s x%d  보상=%s"
                  % (p["collect_id"], p["kor"], p["level"], names.get(p["item"], p["item"]),
                     p["count"], names.get(p["scroll"])))
        print("...")
        print("퀘스트 %d개 추가 (ID %d~%d), 지역 %d곳"
              % (len(plan) * 2, plan[0]["collect_id"], plan[-1]["report_id"], len(plan)))
        return

    wb = openpyxl.load_workbook(XL)
    q, notify, unlock = wb["Quest"], wb["Quest_Notify"], wb["Quest_Unlock"]
    for ws in (q, notify, unlock):
        clear_generated(ws)

    for p in plan:
        nm = names.get(p["item"], str(p["item"]))
        # 모아 오기
        q.append([p["collect_id"], f"{p['kor']} 조사", "ITEM_COUNT", p["item"], p["count"],
                  p["item"], nm, p["count"], None, None, None])
        # 협회 보고
        q.append([p["report_id"], f"{p['kor']} 완료 보고", "BTN_CLICK", "BTN_GUILD_QUEST", 0,
                  p["item"], nm, p["count"], None, None, None])

        order = 1
        for btn in ("BTN_JOURNEY", "BTN_EXPLORE", p["click_btn"], p["enter_btn"]):
            if btn:
                notify.append([p["collect_id"], order, btn])
                order += 1
        for i, btn in enumerate(("BTN_BATTLE_SCENE_JOURNEY", "BTN_GUILD", "BTN_GUILD_QUEST"), 1):
            notify.append([p["report_id"], i, btn])

        unlock.append([p["report_id"], "ITEM_REMOVE", p["item"]])
        unlock.append([p["report_id"], "ITEM_REWARD", p["scroll"]])

    wb.save(XL)
    print("QuestData.xlsx — 퀘스트 %d개(ID %d~%d) 추가. Unity에서 Tools/ParseAllLocal 필요"
          % (len(plan) * 2, plan[0]["collect_id"], plan[-1]["report_id"]))


if __name__ == "__main__":
    main()
