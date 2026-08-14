# -*- coding: utf-8 -*-
"""몬스터 500종 + 심층 지역 노드 + 이름 로컬라이즈 일괄 생성.

설계
----
지역 아트(배경 스프라이트)는 기존 3개 지역분만 존재하므로 **새 지역을 만들지 않고**
각 지역에 심층(depth) 전투 노드를 추가해 레벨 구간을 나눈다. 스프라이트는 지역 것을 재사용.

  산들바람 숲   depth 1~10 → Lv   1~100
  속삭이는 동굴 depth 1~10 → Lv 101~200
  폐허된 성소   depth 1~10 → Lv 201~300

ObjectManager.PickFieldMonsterId는 현재 노드 Name과 부모 노드 Name을 키로 몬스터를 고르므로,
몬스터의 Habitat을 각 depth 노드의 Name 키로 지정하면 해당 구간에서만 등장한다.
심층 진입은 TravelCondition의 LEVEL_REQUIRED로 잠근다(= 난이도 게이트).

밸런스 (재미 규칙)
-----------------
플레이어 스탯 곡선(_tools/gen_player_status.py와 동일 밴드)을 기준으로 상대값을 잡는다.
  몬스터 HP  = 플레이어 공격력 × 처치타수(일반 5 / 희귀 10 / 보스 30)
  몬스터 공격 = 플레이어 HP / 22 + 플레이어 방어  → 피격 22회쯤 버티는 체감
  몬스터 경험치 = 해당 레벨 요구경험치 / 목표 처치수(Lv1 10킬 → Lv300 60킬)
즉 레벨이 올라도 "몇 대 때리면 죽고 몇 대 맞으면 죽는지"가 일정하게 유지된다.

실행: python _tools/gen_monsters.py   → 이후 Unity에서 Tools/ParseAllLocal
주의: 기존 시트를 덮어쓰지 않고 **행 추가/갱신만** 한다(사용자 수정분 보존).
"""
import os
import openpyxl

BASE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
EXCEL = os.path.join(BASE, "Assets", "@Resources", "Data", "Excel")

TOTAL = 500
EXISTING = [2001, 2002, 2003]          # 기존 3종은 건드리지 않고 depth1에 편입
NEW_START_ID = 2101

# ── 지역 정의 ────────────────────────────────────────────────────────────────
REGIONS = [
    dict(node=21000, key="BreezingWindForest", kor="산들바람 숲",   eng="Breezing Wind Forest",
         sprite="BreezingWindForest_Entry_Img", lv0=1,   lv1=100,
         bases=[("슬라임","Slime"),("숲거미","Forest Spider"),("들늑대","Wild Wolf"),
                ("독버섯","Toadstool"),("바람정령","Wind Sprite"),("멧돼지","Boar"),
                ("까마귀","Crow"),("덩굴손","Creeper"),("여우","Fox"),("말벌","Hornet")]),
    dict(node=22000, key="WhisperingCave", kor="속삭이는 동굴", eng="Whispering Cave",
         sprite="WhisperingCave_Entry_Img", lv0=101, lv1=200,
         bases=[("동굴박쥐","Cave Bat"),("두더지","Mole"),("광부망령","Miner Wraith"),
                ("수정골렘","Crystal Golem"),("지네","Centipede"),("이끼괴물","Moss Horror"),
                ("석상병","Stone Sentry"),("그림자","Shade"),("도롱뇽","Salamander"),
                ("광충","Glow Worm")]),
    dict(node=23000, key="RuinedSanctum", kor="폐허가 된 성소", eng="Ruined Sanctum",
         sprite="RuinedSanctum_Entry_Img", lv0=201, lv1=300,
         bases=[("망령","Revenant"),("석상기사","Stone Knight"),("타락사제","Fallen Priest"),
                ("키메라","Chimera"),("가고일","Gargoyle"),("수호자","Warden"),
                ("환영","Phantasm"),("타락정령","Tainted Spirit"),("제단수호물","Altar Ward"),
                ("공허체","Void Thing")]),
]
DEPTHS = 10

# 심도별 수식어 (앞쪽=약함)
MODS = [("어린", "Young"), ("거친", "Feral"), ("굶주린", "Starving"), ("성난", "Enraged"),
        ("사나운", "Savage"), ("오래된", "Ancient"), ("타락한", "Corrupted"), ("심연의", "Abyssal"),
        ("왕관을 쓴", "Crowned"), ("전설의", "Legendary")]

# 기존 애니메이션 자산 순환 사용 (Phase4에서 개별 애니로 교체 예정)
ANIMS = [("Slime", "Slime"), ("Slime(Antenna)", "Slime_Antenna_Idle_4"), ("Goblin", "goblin_monster_0")]


# ── 플레이어 곡선 (gen_player_status.py와 동일) ──────────────────────────────
PBANDS = [(40, 20, 3, 2, 1), (100, 35, 5, 3, 2), (180, 60, 8, 5, 3),
          (250, 100, 13, 8, 5), (300, 160, 20, 12, 8)]
LBANDS = [(40, 1.20), (70, 1.12), (110, 1.09), (160, 1.07),
          (210, 1.055), (260, 1.045), (300, 1.035)]


def player_curve():
    hp, st, mg, df = 100, 15, 10, 0
    out = {}
    for lv in range(1, 301):
        if lv > 1:
            for end, ghp, gst, gmg, gdf in PBANDS:
                if lv <= end:
                    hp += ghp; st += gst; mg += gmg; df += gdf
                    break
        out[lv] = (hp, st, df)
    return out


def exp_curve():
    out, e = {}, 100.0
    for lv in range(1, 301):
        if lv > 1:
            for end, r in LBANDS:
                if lv <= end:
                    e *= r
                    break
        out[lv] = e
    return out


PC = player_curve()
EC = exp_curve()


def target_kills(lv):
    return 10 + 50 * (lv - 1) / 299.0


def make_monsters():
    """500종 산출. 기존 3종은 목록에 포함하되 시트 갱신 대상에서 제외."""
    per_depth = TOTAL // (len(REGIONS) * DEPTHS)      # 16
    remainder = TOTAL - per_depth * len(REGIONS) * DEPTHS   # 20 → 앞 depth에 1마리씩 추가
    out = []
    did = NEW_START_ID
    slot = 0
    for reg in REGIONS:
        span = (reg["lv1"] - reg["lv0"] + 1) / DEPTHS      # 10레벨
        for d in range(DEPTHS):
            n = per_depth + (1 if slot < remainder else 0)
            slot += 1
            hab = f'{reg["key"]}Depth{d + 1}Text'
            base_lv = reg["lv0"] + int(span * d)
            for k in range(n):
                lv = min(300, base_lv + int(span * k / max(1, n - 1)) if n > 1 else base_lv)
                rarity = "Boss" if k == n - 1 else ("Rare" if k % 5 == 4 else "Common")
                bkor, beng = reg["bases"][k % len(reg["bases"])]
                mkor, meng = MODS[d]
                anim, icon = ANIMS[(did + k) % len(ANIMS)]
                out.append(dict(lv=lv, rarity=rarity, habitat=hab, region=reg,
                                kor=f"{mkor} {bkor}", eng=f"{meng} {beng}",
                                anim=anim, icon=icon, depth=d + 1))
                did += 1
    # DataId 재부여 (기존 3종 회피)
    did = NEW_START_ID
    for m in out:
        m["id"] = did
        did += 1
    return out


HITS = {"Common": 5, "Rare": 10, "Boss": 30}
ATK_MUL = {"Common": 1.0, "Rare": 1.25, "Boss": 1.6}
EXP_MUL = {"Common": 1.0, "Rare": 3.0, "Boss": 8.0}
GOLD_MUL = {"Common": 1.0, "Rare": 3.0, "Boss": 10.0}
ENC = {"Common": 1.0, "Rare": 0.4, "Boss": 0.08}


def stats(m):
    php, patk, pdef = PC[m["lv"]]
    hp = round(patk * HITS[m["rarity"]])
    atk = round((php / 22.0 + pdef) * ATK_MUL[m["rarity"]])
    dfn = round(patk * 0.12)
    exp = round(EC[m["lv"]] / target_kills(m["lv"]) * EXP_MUL[m["rarity"]])
    # 골드는 경험치와 분리한다. 경험치는 지수 곡선이지만 아이템 가격은 저작값이라,
    # 골드까지 지수로 불리면 경제가 즉시 무의미해지고 RewardGold(int32)도 넘친다.
    # Lv1 일반=5(기존 슬라임과 동일) → Lv300 일반=379 수준의 완만한 선형.
    gold = max(1, round(5 * (1 + (m["lv"] - 1) * 0.25) * GOLD_MUL[m["rarity"]]))
    return hp, atk, dfn, max(1, exp), gold


# ── 시트 기록 ────────────────────────────────────────────────────────────────
def write_creatures(monsters):
    path = os.path.join(EXCEL, "CreatureData.xlsx")
    wb = openpyxl.load_workbook(path)
    ws = wb[wb.sheetnames[0]]
    hdr = {ws.cell(1, c).value: c for c in range(1, ws.max_column + 1)}

    # 기존 3종을 depth1 서식지로 재지정 (안 하면 지역 전체에 Lv1 몬스터가 섞여 나옴)
    for r in range(4, ws.max_row + 1):
        if ws.cell(r, hdr["DataId"]).value in EXISTING:
            ws.cell(r, hdr["Habitat"], "BreezingWindForestDepth1Text")

    row = ws.max_row + 1
    for m in monsters:
        hp, atk, dfn, exp, gold = stats(m)
        vals = {
            "DataId": m["id"], "Name": f'Monster{m["id"]}NameText', "PrefabLabel": "Monster",
            "IconLabel": m["icon"], "MaxHp": hp, "PhysicalAtk": atk, "Def": dfn, "MoveSpeed": 0,
            "TotalExp": exp, "RewardGold": gold, "DropItemId": 10001 if m["id"] % 2 else 10002,
            "DropCount": 1, "DropChance": 1, "MagicAtk": 0, "CritRate": 0, "CritDamage": 150,
            "Accuracy": 80, "AtkSpeed": 100, "PhysicalResist": 0, "MagicResist": 0, "Evasion": 0,
            "RecoveryRate": 0, "UniqueSkillName": "0", "UniqueSkillDesc": "0",
            "AttackSfx": "Slime_Attack", "OnHitSfx": "Slime_OnHit", "DieSfx": "Slime_Die",
            "AnimController": m["anim"], "Habitat": m["habitat"], "DmgAmp": 0,
            "Rarity": m["rarity"], "MonsterAttackDelay": 1.5, "EncounterRate": ENC[m["rarity"]],
            "InterestRate": 0.5, "MaxKillCount": 100, "SpawnY": 2.3, "DamageFontYOffset": 2.5,
        }
        for name, col in hdr.items():
            if name in vals:
                ws.cell(row, col, vals[name])
        row += 1
    wb.save(path)
    print(f"CreatureData.xlsx: +{len(monsters)}종 (총 {len(monsters) + len(EXISTING)}종)")


def write_travel():
    path = os.path.join(EXCEL, "TravelData.xlsx")
    wb = openpyxl.load_workbook(path)
    node = wb["TravelNode"]
    cond = wb["TravelCondition"]

    existing_ids = {node.cell(r, 1).value for r in range(2, node.max_row + 1)}
    nrow = node.max_row + 1
    crow = cond.max_row + 1
    added = 0
    for reg in REGIONS:
        span = (reg["lv1"] - reg["lv0"] + 1) / DEPTHS
        for d in range(DEPTHS):
            nid = reg["node"] + 100 + d * 10       # 21100, 21110, ... (기존 21100=depth1)
            key = f'{reg["key"]}Depth{d + 1}Text'
            if nid not in existing_ids:
                node.cell(nrow, 1, nid)
                node.cell(nrow, 2, reg["node"])
                node.cell(nrow, 3, "EXPLORE")
                node.cell(nrow, 4, key)
                node.cell(nrow, 5, reg["sprite"])
                node.cell(nrow, 6, "Battle")
                node.cell(nrow, 7, f'BTN_MAP_{reg["key"].upper()}_D{d + 1}')
                node.cell(nrow, 8, f'BTN_STAGE_{reg["key"].upper()}_D{d + 1}')
                nrow += 1
                added += 1
            else:
                # 기존 입구 노드(21100 등)는 이름만 depth1 키로 통일
                for r in range(2, node.max_row + 1):
                    if node.cell(r, 1).value == nid:
                        node.cell(r, 4, key)
            if d > 0:   # depth2부터 레벨 게이트
                need = reg["lv0"] + int(span * d)
                cond.cell(crow, 1, nid)
                cond.cell(crow, 2, "LEVEL_REQUIRED")
                cond.cell(crow, 3, str(need))
                cond.cell(crow, 4, need)
                cond.cell(crow, 5, f"레벨 {need} 이상이어야 들어갈 수 있습니다.")
                crow += 1
    wb.save(path)
    print(f"TravelData.xlsx: 심층 노드 +{added}개, 레벨 조건 추가")


def write_localization(monsters):
    path = os.path.join(EXCEL, "LocalizationData.xlsx")
    wb = openpyxl.load_workbook(path)
    ws = wb[wb.sheetnames[0]]
    have = {ws.cell(r, 1).value for r in range(2, ws.max_row + 1)}
    row = ws.max_row + 1

    def add(key, kor, eng):
        nonlocal row
        if key in have:
            return
        ws.cell(row, 1, key); ws.cell(row, 2, kor); ws.cell(row, 3, eng)
        have.add(key)
        row += 1

    for m in monsters:
        add(f'Monster{m["id"]}NameText', m["kor"], m["eng"])
    for reg in REGIONS:
        for d in range(DEPTHS):
            depth_kor = "입구" if d == 0 else f"{d + 1}층"
            add(f'{reg["key"]}Depth{d + 1}Text', f'{reg["kor"]} {depth_kor}',
                f'{reg["eng"]} {"Entrance" if d == 0 else f"F{d + 1}"}')
    wb.save(path)
    print(f"LocalizationData.xlsx: 이름/지역 키 추가 (현재 {row - 2}행)")


def main():
    ms = make_monsters()
    assert len(ms) + len(EXISTING) >= 500, len(ms)
    write_creatures(ms)
    write_travel()
    write_localization(ms)

    print("\n밸런스 샘플 (일반 몬스터 기준)")
    for lv in (1, 40, 100, 200, 300):
        php, patk, pdef = PC[lv]
        sample = dict(lv=lv, rarity="Common")
        hp, atk, dfn, exp, gold = stats(sample)
        print(f"  Lv{lv:3d} 몬스터 HP {hp:7,} ATK {atk:6,} EXP {exp:15,} "
              f"| 플레이어 HP {php:6,} ATK {patk:5,} → 처치 {hp / max(1, patk):.1f}타, "
              f"레벨업 {EC[lv] / exp:.0f}킬")


if __name__ == "__main__":
    main()
