# -*- coding: utf-8 -*-
"""장비 강화 요구 재료를 채운다 — EquipEnhanceData의 빈 열을 실제 값으로.

## 왜
  EquipEnhanceData에 RequiredMaterialId / RequiredMaterialCount 열은 있는데
  **540행 전부 0**이었다. 강화가 골드와 주문서만 먹고 재료는 아무것도 안 썼다.
  대장간에 보이던 "12/5"는 강화 재료가 아니라 **복구용 상수**(젤리 퐁 5개)였다.

## 설계
  · 재료 계열은 장비 티어가 요구하는 레벨대에 맞춘다 — 그 구간에서 실제로 사냥해 얻는 재료여야
    "가서 잡고 → 강화한다"가 성립한다(제작 레시피 gen_forge_recipes와 같은 규칙).
      숲 11001~11010 (1~100) / 동굴 11011~11020 (101~200) / 성소 11021~11030 (201~)
  · 부위마다 재료를 어긋나게 골라 계열 10종이 골고루 쓰이게 한다.
  · 수량은 강화 단계가 오를수록 늘린다(1강은 가볍게, 9강은 묵직하게).

## 곁들여: 단계별 보너스도 평평했다
  30001의 1~9강 물공 보너스가 전부 6으로 같았다. 뒤로 갈수록 오르는 맛이 없다.
  단계에 비례해 키운다(기본치의 25%씩 가산) — 데이터만 바꾸므로 코드는 그대로 동작한다.

사용: python _tools/gen_enhance_materials.py   → xlsx 갱신 후 Unity에서 Tools/ParseAllLocal
"""
import io
import json
import os

import openpyxl

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
XL = os.path.join(BASE, "Assets", "@Resources", "Data", "Excel", "EquipEnhanceData.xlsx")
JS = os.path.join(BASE, "Assets", "@Resources", "Data", "JsonData")

MAT_ID0 = 11001


def load(name):
    with io.open(os.path.join(JS, name), encoding="utf-8-sig") as f:
        d = json.load(f)
    return d[list(d.keys())[0]]


def tier_of(equip_id):
    """장비 ID → 티어(0~9). 30001~30006이 티어0, 그 다음 6개가 티어1 …"""
    return max(0, (equip_id - 30001) // 6)


def slot_of(equip_id):
    return (equip_id - 30001) % 6


def family_base(tier):
    lv = 1 if tier == 0 else tier * 30 + 1
    if lv <= 100:
        return MAT_ID0            # 숲
    if lv <= 200:
        return MAT_ID0 + 10       # 동굴
    return MAT_ID0 + 20           # 성소


def material_for(equip_id, level):
    """강화 단계마다 재료를 조금씩 바꿔 한 종만 갈아 넣지 않게 한다."""
    b = family_base(tier_of(equip_id))
    return b + (slot_of(equip_id) * 2 + (level - 1)) % 10


def count_for(tier, level):
    """1강 3개 → 9강 30개 근처. 티어가 높을수록 더 든다."""
    return 3 + (level - 1) * 2 + tier * 2


def main():
    items = {r["DataId"] for r in load("InventoryItem.json")}
    wb = openpyxl.load_workbook(XL)
    ws = wb.worksheets[0]
    head = [c.value for c in ws[1]]
    col = {name: i + 1 for i, name in enumerate(head)}

    changed = 0
    missing = set()
    for row in range(4, ws.max_row + 1):
        eq = ws.cell(row=row, column=col["EquipDataId"]).value
        lv = ws.cell(row=row, column=col["Level"]).value
        if eq is None or lv is None:
            continue

        t = tier_of(int(eq))
        mat = material_for(int(eq), int(lv))
        if mat not in items:
            missing.add(mat)

        ws.cell(row=row, column=col["RequiredMaterialId"], value=mat)
        ws.cell(row=row, column=col["RequiredMaterialCount"], value=count_for(t, int(lv)))

        # 단계별 보너스가 전부 같았다 — 단계에 비례해 키운다.
        base = ws.cell(row=row, column=col["AtkBonus"]).value or 0
        atk = int(round(base * (1 + 0.25 * (int(lv) - 1)))) if base else 0
        if base:
            ws.cell(row=row, column=col["AtkBonus"], value=atk)

        # ⚠️ 치명타·마공 보너스가 전 단계 0이었다 — 강화해도 물공 하나만 올랐다.
        #  영상의 장비 능력치 표기는 "물리공격력 +11 / 치명타 +11% / 마법공격력 +10"으로
        #  **세 스탯이 함께** 붙는다. 물공을 주축으로 두고 나머지를 같이 올린다.
        #    치명타 : 단계당 1%p (9강에서 +9%) — 영상의 +11%대와 같은 자릿수
        #    마공   : 물공의 90% (물리 장비도 마공을 조금 갖는 원작 표기를 따른다)
        ws.cell(row=row, column=col["CritBonus"], value=round(1.0 * int(lv), 1))
        ws.cell(row=row, column=col["MagicAtkBonus"], value=int(round(atk * 0.9)))
        changed += 1

    wb.save(XL)
    print(f"EquipEnhanceData.xlsx — {changed}행에 요구 재료·단계별 보너스 기입")
    if missing:
        print(f"  ⚠️ 아이템 데이터에 없는 재료 ID: {sorted(missing)}")
    else:
        print("  요구 재료 전부 InventoryItem에 존재")


if __name__ == "__main__":
    main()
