# -*- coding: utf-8 -*-
"""v2 미션·출석·인앱 상품 표 생성기 (ADR0030, 설계서 4.5·4.7).

  MissionData.xlsx  — 일일(00시 초기화) 6 + 완료 상자, 주간(월요일 00시) 5, 가이드 40단계 사다리, 7일 출석.
  ProductData.xlsx  — 인앱 상품 목록(가격 표시·구성). 실제 결제는 스토어 계정이 생긴 뒤(P3) — 지금은 개발 빌드 모의 구매.

  수치를 바꿀 때는 여기를 고치고 다시 돌린 뒤 Unity Tools/ParseAllLocal.
    python _tools/gen_missions.py            # 두 엑셀을 쓴다
    python _tools/gen_missions.py --check    # 저장된 엑셀이 생성 결과와 같은지만 본다
"""
import argparse
import os
import sys

import openpyxl

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
EXCEL = os.path.join(ROOT, 'Assets', '@Resources', 'Data', 'Excel')

# 미션 이벤트 이름은 Define.MissionEvent 와 같아야 한다.
#  KillMonster BossClear NewStage Train CaptureTry Enhance Parry DailyAllClear DailyChest ReachLevel Discover Attendance DungeonClear BondRequest
TITLE = {
    'KillMonster':   'MissionKillFormat',
    'BossClear':     'MissionBossFormat',
    'NewStage':      'MissionStageFormat',
    'Train':         'MissionTrainFormat',
    'CaptureTry':    'MissionCaptureFormat',
    'Enhance':       'MissionEnhanceFormat',
    'Parry':         'MissionParryFormat',
    'DailyAllClear': 'MissionDailyAllFormat',
    'DailyChest':    'MissionDailyChestText',
    'ReachLevel':    'MissionLevelFormat',
    'Discover':      'MissionDiscoverFormat',
    'Attendance':    'AttendanceDayFormat',
    'DungeonClear':  'MissionDungeonFormat',
    'BondRequest':   'MissionBondFormat',
}

# 일일 — 크리스탈 80 + 완료 상자 40 (설계서 4.5)
#  요일 던전(ADR0034)·배달 의뢰(ADR0036)는 상자 뒤에 붙였다 — DataId 는 순번이라 가운데 끼우면 오늘 받은 상자(1007)가 다른 미션이 된다.
DAILY = [
    ('KillMonster', 300, 'Crystal', 15),
    ('BossClear',     1, 'Crystal', 10),
    ('Train',        20, 'Crystal', 10),
    ('CaptureTry',    1, 'Crystal',  5),
    ('Enhance',       1, 'Crystal', 10),
    ('Parry',        10, 'Crystal', 10),
    ('DailyChest',    8, 'Crystal', 40),   # 다른 일일 8개를 모두 받으면 열린다
    ('DungeonClear',  1, 'Crystal', 10),
    ('BondRequest',   1, 'Crystal', 10),
]
# 주간 — 크리스탈 300
WEEKLY = [
    ('BossClear',     20, 'Crystal', 60),
    ('NewStage',       3, 'Crystal', 60),
    ('CaptureTry',     3, 'Crystal', 60),
    ('DailyAllClear',  5, 'Crystal', 60),
    ('KillMonster', 1500, 'Crystal', 60),
]
# 가이드 — 한 번에 한 단계씩 보이는 사다리. 초반 길잡이 겸 무과금 크리스탈 수입(단계당 20~200).
GUIDE = [
    ('Train', 1, 20), ('BossClear', 1, 30), ('Parry', 1, 30), ('ReachLevel', 10, 30), ('NewStage', 3, 30),
    ('CaptureTry', 1, 30), ('Discover', 10, 40), ('Enhance', 1, 30), ('ReachLevel', 20, 40), ('NewStage', 5, 40),
    ('Train', 50, 40), ('Discover', 20, 50), ('ReachLevel', 30, 50), ('BossClear', 10, 50), ('NewStage', 10, 60),
    ('Parry', 20, 50), ('ReachLevel', 40, 60), ('Discover', 50, 80), ('Enhance', 10, 60), ('NewStage', 15, 80),
    ('ReachLevel', 50, 80), ('Train', 300, 80), ('BossClear', 50, 100), ('NewStage', 25, 100), ('ReachLevel', 70, 100),
    ('Discover', 100, 120), ('Parry', 100, 100), ('NewStage', 35, 120), ('ReachLevel', 100, 120), ('CaptureTry', 30, 120),
    ('NewStage', 50, 150), ('ReachLevel', 130, 150), ('Discover', 200, 150), ('NewStage', 65, 150), ('ReachLevel', 170, 170),
    ('NewStage', 80, 170), ('ReachLevel', 210, 180), ('NewStage', 100, 200), ('ReachLevel', 250, 200), ('NewStage', 125, 200),
]
# 가이드 사이에 나중에 끼운 단계 — (DataId, Order, 이벤트, 목표, 크리스탈). 기존 단계는 Order = 번호×10 이라 그 사이에 들어간다.
#  DataId 를 따로 둬서 받은 기록(Claimed = DataId)이 밀리지 않는다. 이미 지나간 유저에겐 이 단계가 다음 가이드로 뜬다(새 기능 안내).
GUIDE_EXTRA = [
    (3101, 25, 'DungeonClear', 1, 30),   # 요일 던전(ADR0034) — 보스 1회 다음
    (3102, 35, 'BondRequest', 1, 30),    # 마을 배달 의뢰(ADR0036) — 패링 1회 다음
]
# 출석 7일 반복: 50 → 50 → 만드라고라 2 → 100 → 50 → 50 → 300
ATTENDANCE = [('Crystal', 50), ('Crystal', 50), ('Mandrake', 2), ('Crystal', 100), ('Crystal', 50), ('Crystal', 50), ('Crystal', 300)]

MISSION_HEADER = [
    ('DataId', 'int', '번호'), ('Kind', 'string', '종류(Daily/Weekly/Guide/Attendance)'), ('Order', 'int', '순서'),
    ('Event', 'string', '이벤트(Define.MissionEvent)'), ('Goal', 'int', '목표'), ('RewardType', 'string', '보상(Crystal/Mandrake/Gold)'),
    ('RewardValue', 'int', '보상량'), ('TitleKey', 'string', '제목 로컬라이즈 키({0}=목표)'),
]


def mission_rows():
    rows = []
    for i, (ev, goal, rt, rv) in enumerate(DAILY, 1):
        rows.append((1000 + i, 'Daily', i, ev, goal, rt, rv, TITLE[ev]))
    for i, (ev, goal, rt, rv) in enumerate(WEEKLY, 1):
        rows.append((2000 + i, 'Weekly', i, ev, goal, rt, rv, TITLE[ev]))
    for i, (ev, goal, rv) in enumerate(GUIDE, 1):
        rows.append((3000 + i, 'Guide', i * 10, ev, goal, 'Crystal', rv, TITLE[ev]))
    for data_id, order, ev, goal, rv in GUIDE_EXTRA:
        rows.append((data_id, 'Guide', order, ev, goal, 'Crystal', rv, TITLE[ev]))
    for i, (rt, rv) in enumerate(ATTENDANCE, 1):
        rows.append((4000 + i, 'Attendance', i, 'Attendance', i, rt, rv, TITLE['Attendance']))
    return rows


# 인앱 상품 — 설계서 4.7. 성장·시즌 패스, 치장, 유물 소환권은 해당 시스템(P2)과 함께 넣는다.
#  설계서의 시작 패키지 '회복물약 10'은 뺐다 — 이 게임의 물약은 개수가 아니라 단계 선택 + 재사용 대기라 줄 게 없다.
#  Kind: Pack(1회) / StepUp(순서대로 1회) / Monthly(30일) / AdFree(영구) / Crystal(반복, 종류별 첫 구매 2배)
PRODUCT_HEADER = [
    ('DataId', 'int', '번호'), ('ProductKey', 'string', '스토어 상품 ID'), ('Kind', 'string', '종류'),
    ('PriceKRW', 'int', '가격(원)'), ('Crystal', 'int', '크리스탈'), ('Mandrake', 'int', '만드라고라'),
    ('NameKey', 'string', '이름 로컬라이즈 키'),
]
PRODUCTS = [
    (5001, 'starter_pack', 'Pack',      1100,  300, 5, 'ProductStarterText'),
    (5002, 'stepup_1',     'StepUp',    1100,  150, 0, 'ProductStepUp1Text'),
    (5003, 'stepup_2',     'StepUp',    3300,  450, 0, 'ProductStepUp2Text'),
    (5004, 'stepup_3',     'StepUp',    5500,  800, 0, 'ProductStepUp3Text'),
    (5005, 'monthly_pass', 'Monthly',   5500,  300, 0, 'ProductMonthlyText'),
    (5006, 'ad_free',      'AdFree',    9900,    0, 0, 'ProductAdFreeText'),
    (5101, 'crystal_60',   'Crystal',   1100,   60, 0, 'ProductCrystalFormat'),
    (5102, 'crystal_330',  'Crystal',   5500,  330, 0, 'ProductCrystalFormat'),
    (5103, 'crystal_700',  'Crystal',  11000,  700, 0, 'ProductCrystalFormat'),
    (5104, 'crystal_2200', 'Crystal',  33000, 2200, 0, 'ProductCrystalFormat'),
    (5105, 'crystal_3800', 'Crystal',  55000, 3800, 0, 'ProductCrystalFormat'),
    (5106, 'crystal_8000', 'Crystal', 110000, 8000, 0, 'ProductCrystalFormat'),
]


def build(path, header, rows, sheet):
    wb = openpyxl.Workbook()
    ws = wb.active
    ws.title = sheet
    ws.append([h[0] for h in header])
    ws.append([h[1] for h in header])
    ws.append([h[2] for h in header])
    for r in rows:
        ws.append(list(r))
    return wb


def read(path):
    wb = openpyxl.load_workbook(path)
    return [tuple(c for c in row) for row in wb.active.iter_rows(min_row=4, values_only=True)]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--check', action='store_true')
    args = ap.parse_args()

    targets = [
        (os.path.join(EXCEL, 'MissionData.xlsx'), MISSION_HEADER, mission_rows(), 'MissionData'),
        (os.path.join(EXCEL, 'ProductData.xlsx'), PRODUCT_HEADER, PRODUCTS, 'ProductData'),
    ]
    daily = sum(r[6] for r in mission_rows() if r[1] == 'Daily')
    weekly = sum(r[6] for r in mission_rows() if r[1] == 'Weekly')
    guide = sum(r[6] for r in mission_rows() if r[1] == 'Guide')
    print(f'일일 크리스탈 {daily} (상자 포함) · 주간 {weekly} · 가이드 {len(GUIDE) + len(GUIDE_EXTRA)}단계 합계 {guide} · 출석 7일 {ATTENDANCE}')

    ok = True
    for path, header, rows, sheet in targets:
        if args.check:
            same = os.path.exists(path) and read(path) == [tuple(r) for r in rows]
            print(('같음 ' if same else '다름 ') + os.path.basename(path))
            ok &= same
        else:
            build(path, header, rows, sheet).save(path)
            print('저장 ' + os.path.basename(path) + f' ({len(rows)}행)')
    sys.exit(0 if ok else 1)


if __name__ == '__main__':
    main()
