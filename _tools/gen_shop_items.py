# -*- coding: utf-8 -*-
"""마을 상점의 판매 목록 시트를 만든다.

## 왜
  골드 소비처가 강화(GameManager:888)와 주문제작(:1202) 둘뿐이라, 사냥으로 골드는 계속
  쌓이는데 쓸 데가 없었다. 상점이 그 출구다.

## 무엇을 파는가 — 그리고 무엇을 안 파는가
  판다  : 재료 30종(11001~11030), 강화 주문서 4종(10011~10014)
  안 판다: **장비**. 장비 획득은 대장간 제작으로 몰아야 재료가 쌓이기만 하는 문제가 풀리고,
           두 건물이 서로 역할을 먹지 않는다. 물약도 뺐다 — 연금공방 주문제작이 이미 담당한다.

## 값 매기기
  재료는 등급(Grade)이 곧 티어다. 구매가는 등급별 기본가에 티어 계수를 곱하고,
  되팔기가는 구매가의 30%로 둔다. 사고팔기를 반복해 돈을 버는 짓이 안 되게 하려면
  되팔기가 < 구매가여야 한다 — 같거나 크면 무한 루프가 생긴다.

  해금 레벨은 재료가 나오는 지역 티어를 따라간다. 1티어(11001~11010)는 처음부터,
  2티어(11011~11020)는 40레벨, 3티어(11021~11030)는 90레벨.

사용: python _tools/gen_shop_items.py   → 이후 Unity에서 Tools/ParseAllLocal
"""
import io
import json
import os

import openpyxl

BASE = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
XLSX = os.path.join(BASE, "Assets", "@Resources", "Data", "Excel", "ShopItemData.xlsx")
ITEM_JSON = os.path.join(BASE, "Assets", "@Resources", "Data", "JsonData", "InventoryItem.json")

HEADER = ["DataId", "ItemId", "Price", "SellPrice", "UnlockLevel"]
HEADER_KOR = ["ID", "아이템", "구매가", "되팔기가", "해금 레벨"]

# 등급(Grade) 기본가 — Basic/Rare/Unique/Legend/Mythic 순
GRADE_PRICE = [120, 400, 1200, 3600, 10000]

# 재료 티어별 해금 레벨. 상점 목록이 처음부터 30종이면 고를 수가 없다.
def unlock_for(item_id):
    if item_id < 11011:
        return 1
    if item_id < 11021:
        return 40
    return 90


# 강화 주문서 — 등급이 곧 단계다. 재료보다 비싸게 둬야 "사서 강화"가 공짜 루트가 안 된다.
SCROLLS = {
    10011: (600, 1),
    10012: (2400, 25),
    10013: (9000, 60),
    10014: (30000, 120),
}

SELL_RATE = 0.3   # 되팔기가 = 구매가 * 0.3. 1.0에 가까우면 무한 차익 루프가 생긴다.


def load_items():
    with io.open(ITEM_JSON, encoding="utf-8-sig") as f:
        data = json.load(f)
    key = list(data)[0]
    return {r["DataId"]: r for r in data[key]}


def build_rows():
    items = load_items()
    rows = []
    data_id = 60000

    # 재료 — 11001~11030
    for item_id in sorted(k for k in items if 11000 < k < 12000):
        grade = int(items[item_id].get("Grade", 0) or 0)
        price = GRADE_PRICE[min(grade, len(GRADE_PRICE) - 1)]
        rows.append([data_id, item_id, price, int(price * SELL_RATE), unlock_for(item_id)])
        data_id += 1

    # 강화 주문서
    for item_id, (price, lv) in sorted(SCROLLS.items()):
        if item_id not in items:
            print(f"  !아이템 없음, 건너뜀: {item_id}")
            continue
        rows.append([data_id, item_id, price, int(price * SELL_RATE), lv])
        data_id += 1

    return rows


def main():
    rows = build_rows()

    wb = openpyxl.Workbook()
    ws = wb.active
    ws.title = "ShopItemData"
    # 파서가 4번째 행(index 3)부터 읽는다 — 다른 시트와 같은 머리 3줄 구조를 지킨다.
    ws.append(HEADER)
    ws.append([None] * len(HEADER))
    ws.append(HEADER_KOR)
    for r in rows:
        ws.append(r)
    wb.save(XLSX)

    mats = sum(1 for r in rows if 11000 < r[1] < 12000)
    print(f"ShopItemData.xlsx 생성 — {len(rows)}행 (재료 {mats}, 주문서 {len(rows) - mats})")
    print(f"  가격대 {min(r[2] for r in rows):,} ~ {max(r[2] for r in rows):,} 골드")
    print("→ Unity에서 Tools/ParseAllLocal 필요")


if __name__ == "__main__":
    main()
