# -*- coding: utf-8 -*-
"""대장간 장비 제작 레시피 생성 — ForgeRecipeData.xlsx

## 왜 제작인가
장비 60종을 만들어 놓고 획득 경로가 없었다(시작 지급 6종이 전부).
드롭으로 주면 CreatureData.DropItemId가 이미 재료에 쓰이고 있어 컬럼을 늘려야 하고,
확률 드롭은 방치형에서 스트레스가 된다.
반면 **재료는 이미 쌓이기만 하고 쓸 데가 없었고**(999개까지 찬다),
대장간 프리팹엔 쓰이지 않는 CraftingPanel이 있다. 제작이 기존 자산을 가장 잘 쓴다.

## 설계
장비 60종 = 6부위 × 10티어. 티어 t = (DataId - 30001) // 6, 부위 = (DataId - 30001) % 6.
등급은 t 0~1 Basic / 2~3 Abnormal / 4~5 Rare / 6~7 Exceptional / 8 Legendary / 9 Mystic.

**요구 레벨**: t0=1, 이후 30레벨 간격 (t9 = 271). 300레벨 곡선에 고르게 깔린다.

**재료 계열**: 실제 드롭 레벨대에 맞춘다 (측정값)
  11001~11010 숲   : Lv 1~104
  11011~11020 동굴 : Lv 104~205
  11021~11030 성소 : Lv 205~298
  → t0~t3(Lv1~91) 숲 / t4~t6(Lv121~181) 동굴 / t7~t9(Lv211~271) 성소
  그 티어에 갈 수 있으면 그 재료도 이미 모았다는 뜻이라 "못 만드는 레시피"가 안 생긴다.

**재료 종류**: 부위마다 다른 3종을 요구한다. 한 재료만 갈면 나머지 29종이 또 죽는다.
  같은 계열 안에서 부위별로 어긋나게 골라 골고루 쓰이게 한다.

**수량/골드**: 티어에 따라 증가. 골드는 강화 비용과 같은 자릿수를 유지해
  제작이 강화를 완전히 대체하지 않게 한다.

사용: python _tools/gen_forge_recipes.py
이후 Unity에서 Tools/ParseAllLocal 로 JSON 재생성 필요.
"""
import io
import os

import openpyxl

BASE = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
EXCEL = os.path.join(BASE, "Assets", "@Resources", "Data", "Excel")

EQUIP_ID0 = 30001
SLOTS = 6
TIERS = 10
MAT_ID0 = 11001

# 티어 → 요구 레벨
def req_level(t):
    return 1 if t == 0 else t * 30 + 1

# 티어 → 재료 계열 시작 id (숲/동굴/성소)
def family_base(t):
    lv = req_level(t)
    if lv <= 100:
        return MAT_ID0            # 11001~11010 숲
    if lv <= 200:
        return MAT_ID0 + 10       # 11011~11020 동굴
    return MAT_ID0 + 20           # 11021~11030 성소


# 부위별로 어긋나게 3종 — 계열 10종이 골고루 쓰이도록
def materials(t, slot):
    b = family_base(t)
    return [b + (slot * 2 + off) % 10 for off in (0, 1, 5)]


def counts(t):
    """주 재료는 넉넉히, 보조는 적게. 티어마다 증가."""
    return [8 + t * 4, 5 + t * 3, 3 + t * 2]


def gold(t):
    return 200 * (t + 1) * (t + 1)


HEADER = ["DataId", "EquipDataId", "ReqLevel", "Gold",
          "Mat1Id", "Mat1Count", "Mat2Id", "Mat2Count", "Mat3Id", "Mat3Count"]
KOR = ["ID", "결과 장비", "요구 레벨", "골드",
       "재료1", "개수1", "재료2", "개수2", "재료3", "개수3"]


def main():
    wb = openpyxl.Workbook()
    ws = wb.active
    ws.title = "ForgeRecipeData"

    for c, v in enumerate(HEADER, 1):
        ws.cell(1, c, v)
    for c, v in enumerate(KOR, 1):
        ws.cell(3, c, v)

    row = 4
    n = 0
    for t in range(TIERS):
        for slot in range(SLOTS):
            equip = EQUIP_ID0 + t * SLOTS + slot
            mats = materials(t, slot)
            cnts = counts(t)
            vals = [40000 + n, equip, req_level(t), gold(t),
                    mats[0], cnts[0], mats[1], cnts[1], mats[2], cnts[2]]
            for c, v in enumerate(vals, 1):
                ws.cell(row, c, v)
            row += 1
            n += 1

    path = os.path.join(EXCEL, "ForgeRecipeData.xlsx")
    wb.save(path)
    print("ForgeRecipeData.xlsx 생성 — 레시피 %d개" % n)

    # 요약 출력 (사람이 곡선을 눈으로 확인할 수 있게)
    print("\n티어  요구Lv    골드   재료(계열)")
    for t in range(TIERS):
        m = materials(t, 0)
        fam = {MAT_ID0: "숲", MAT_ID0 + 10: "동굴", MAT_ID0 + 20: "성소"}[family_base(t)]
        print("  t%d  %6d  %6d   %s %s x%s" % (t, req_level(t), gold(t), fam, m, counts(t)))


if __name__ == "__main__":
    main()
