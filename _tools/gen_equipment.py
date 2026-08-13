# -*- coding: utf-8 -*-
"""장비 60종(6슬롯 × 10티어) + 강화(+1~+9) 테이블 + 이름 로컬라이즈 생성.

설계
----
티어 t는 플레이어 레벨 30t 구간을 담당한다(몬스터 심층 지역과 같은 30레벨 단위).
장비 한 세트(6슬롯)를 모두 갖추면 그 구간 플레이어 기본 공격력의 약 35%를 더한다.
  → 장비가 성장의 보조축이 되도록. 100%를 넘기면 레벨업이 무의미해지고,
    10% 수준이면 장비를 갈아입을 이유가 없어진다.

슬롯 가중치(합=1): 무기가 절반 가까이를 차지하고 방어구가 나머지를 나눠 갖는다.
등급은 티어를 따라 Basic → Mystic 으로 올라간다(EquipGrade enum 순서).

강화는 기존 30001(목검)의 비율을 그대로 따른다: 단계당 기본 공격력의 50%,
+9까지 누적하면 기본값의 4.5배. (기존 데이터: BaseAtk 10 / 단계당 5)

아이콘은 현재 프로젝트에 실재하는 스프라이트를 슬롯별로 순환 사용한다.
(Phase 2-4에서 AetherForge 생성 아이콘으로 교체 예정)

실행: python _tools/gen_equipment.py  → 이후 Unity에서 Tools/ParseAllLocal
주의: 기존 시트를 덮어쓰지 않고 행 추가/갱신만 한다.
"""
import os
import openpyxl

BASE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
EXCEL = os.path.join(BASE, "Assets", "@Resources", "Data", "Excel")

TIERS = 10
SET_RATIO = 0.35          # 풀세트가 더해주는 기본 공격력 비율
ENH_MAX = 9

# (슬롯enum, 가중치, 한글, 영문, 아이콘 후보)
SLOTS = [
    ("Weapon",    0.35, "검",   "Sword",   ["Sword01", "Sword03", "Sword 02", "Icon_Sword_2", "Sword 32 (5)"]),
    ("Helmet",    0.10, "투구", "Helm",    ["helmet_01", "Helmet 32 (27)"]),
    ("Armor",     0.15, "갑옷", "Armor",   ["Torso 32 (17)", "Torso 32 (27)"]),
    ("Gloves",    0.12, "장갑", "Gloves",  ["Vambrace 32 (52)"]),
    ("Legs",      0.12, "각반", "Greaves", ["Grieves 32 (9)"]),
    ("Accessory", 0.16, "부적", "Amulet",  ["Accessories 32 (1)", "Cloak 32 (16)"]),
]

TIER_NAME = [("낡은", "Worn"), ("청동", "Bronze"), ("강철", "Steel"), ("은빛", "Silver"),
             ("미스릴", "Mithril"), ("용린", "Dragonscale"), ("성흔", "Sacred"),
             ("심연", "Abyssal"), ("여명", "Dawn"), ("신화", "Mythic")]

GRADE_BY_TIER = ["Basic", "Basic", "Abnormal", "Abnormal", "Rare", "Rare",
                 "Exceptional", "Exceptional", "Legendary", "Mystic"]

PBANDS = [(40, 20, 3, 2, 1), (100, 35, 5, 3, 2), (180, 60, 8, 5, 3),
          (250, 100, 13, 8, 5), (300, 160, 20, 12, 8)]


def player_atk():
    hp, st = 100, 15
    out = {}
    for lv in range(1, 301):
        if lv > 1:
            for end, ghp, gst, _, _ in PBANDS:
                if lv <= end:
                    st += gst
                    break
        out[lv] = st
    return out


PATK = player_atk()


def build():
    items, enh = [], []
    for t in range(1, TIERS + 1):
        lv = 30 * t
        budget = SET_RATIO * PATK[lv]
        for si, (slot, w, kor, eng, icons) in enumerate(SLOTS):
            did = 30001 + (t - 1) * len(SLOTS) + si
            atk = max(1, round(budget * w))
            crit = round(1.5 * t, 1) if slot == "Accessory" else (round(1.0 * t, 1) if slot == "Gloves" else 0)
            matk = round(atk * 0.5) if slot == "Accessory" else (round(atk * 0.3) if slot == "Helmet" else 0)
            tk, te = TIER_NAME[t - 1]
            # 아이콘: AetherForge 생성분(Equip{DataId})이 있으면 그것을, 없으면 기존 에셋을 쓴다.
            gen = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Icon",
                               "Equipment", "Generated", f"Equip{did}.png")
            sprite = f"Equip{did}" if os.path.exists(gen) else icons[(t - 1) % len(icons)]
            items.append(dict(
                id=did, slot=slot, grade=GRADE_BY_TIER[t - 1],
                sprite=sprite, dur=100 + t * 10,
                atk=atk, crit=crit, matk=matk,
                kor=f"{tk} {kor}", eng=f"{te} {eng}", tier=t, lv=lv))
            for l in range(1, ENH_MAX + 1):
                enh.append(dict(equip=did, level=l,
                                atk=max(1, round(atk * 0.5)),
                                crit=round(crit * 0.1, 2),
                                matk=round(matk * 0.5)))
    return items, enh


def write_equipment(items):
    path = os.path.join(EXCEL, "EquipmentData.xlsx")
    wb = openpyxl.load_workbook(path)
    ws = wb[wb.sheetnames[0]]
    hdr = {ws.cell(1, c).value: c for c in range(1, ws.max_column + 1)}
    have = {ws.cell(r, hdr["DataId"]).value: r for r in range(4, ws.max_row + 1)}

    row = ws.max_row + 1
    added = 0
    for it in items:
        r = have.get(it["id"])
        if r is None:
            r = row
            row += 1
            added += 1
        vals = {"DataId": it["id"], "Name": f'Equip{it["id"]}NameText', "SlotType": it["slot"],
                "Grade": it["grade"], "SpriteName": it["sprite"], "MaxDurability": it["dur"],
                "BaseAtk": it["atk"], "BaseCritRate": it["crit"], "BaseMagicAtk": it["matk"]}
        for name, col in hdr.items():
            if name in vals:
                ws.cell(r, col, vals[name])
    wb.save(path)
    print(f"EquipmentData.xlsx: 총 {len(items)}종 (신규 {added})")


def write_enhance(enh):
    path = os.path.join(EXCEL, "EquipEnhanceData.xlsx")
    wb = openpyxl.load_workbook(path)
    ws = wb[wb.sheetnames[0]]
    hdr = {ws.cell(1, c).value: c for c in range(1, ws.max_column + 1)}

    # 기존 데이터 행 전부 제거 후 재작성 (장비별 9단계를 일관되게 유지)
    if ws.max_row >= 4:
        ws.delete_rows(4, ws.max_row - 3)
    for i, e in enumerate(enh, start=1):
        r = 3 + i
        ws.cell(r, hdr["DataId"], i)
        ws.cell(r, hdr["EquipDataId"], e["equip"])
        ws.cell(r, hdr["Level"], e["level"])
        ws.cell(r, hdr["AtkBonus"], e["atk"])
        ws.cell(r, hdr["CritBonus"], e["crit"])
        ws.cell(r, hdr["MagicAtkBonus"], e["matk"])
        ws.cell(r, hdr["RequiredMaterialId"], 0)
        ws.cell(r, hdr["RequiredMaterialCount"], 0)
    wb.save(path)
    print(f"EquipEnhanceData.xlsx: {len(enh)}행 (장비 {len(enh) // ENH_MAX}종 × +{ENH_MAX})")


def write_localization(items):
    path = os.path.join(EXCEL, "LocalizationData.xlsx")
    wb = openpyxl.load_workbook(path)
    ws = wb[wb.sheetnames[0]]
    have = {ws.cell(r, 1).value for r in range(2, ws.max_row + 1)}
    row = ws.max_row + 1
    added = 0
    for it in items:
        key = f'Equip{it["id"]}NameText'
        if key in have:
            continue
        ws.cell(row, 1, key); ws.cell(row, 2, it["kor"]); ws.cell(row, 3, it["eng"])
        row += 1
        added += 1
    wb.save(path)
    print(f"LocalizationData.xlsx: 장비 이름 +{added}개")


def main():
    items, enh = build()
    write_equipment(items)
    write_enhance(enh)
    write_localization(items)

    print("\n티어별 풀세트 효과")
    for t in (1, 5, 10):
        ti = [i for i in items if i["tier"] == t]
        s = sum(i["atk"] for i in ti)
        s9 = s + sum(max(1, round(i["atk"] * 0.5)) * ENH_MAX for i in ti)
        lv = 30 * t
        print(f"  T{t:2d}(Lv{lv:3d}) 세트 공격력 +{s:5,} (+9강 {s9:6,}) / 플레이어 기본 {PATK[lv]:5,}"
              f"  → 기본대비 {s / PATK[lv] * 100:.0f}% (+9강 {s9 / PATK[lv] * 100:.0f}%)")


if __name__ == "__main__":
    main()
