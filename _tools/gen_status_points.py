# -*- coding: utf-8 -*-
"""레벨업에 능력점수(StatusPoint)를 붙인다.

배경
    캐릭터창의 스탯 분배(GameData.StatInvest, PlayerController.STAT_PER_POINT)는
    이미 만들어져 있는데 **쓸 포인트가 들어오는 경로가 없었다.**
      - PlayerStatusData 의 StatusPoint 컬럼이 300레벨 전부 0
      - PlayerController.CheckLevelUp 은 레벨만 올리고 포인트를 주지 않는다
      - StatusPoint 를 올리는 코드는 에디터 테스트 메뉴(IntegrationTestMenu,
        QuestVerifyMenu)뿐 — 즉 실제 플레이에서는 영원히 0이었다
    그래서 300레벨을 올려도 플레이어가 고를 것이 하나도 없었다.

    레벨로 변하는 것도 14개 스탯 중 4개(HP·근력·마력·방어)뿐이라, 나머지 성장은
    이 포인트로 플레이어가 직접 고르게 하는 것이 원래 설계였다고 본다.

지급 규칙
    레벨당 3점, 10레벨마다 +7점(구간 목표를 만든다). Lv1 은 시작 레벨이라 0.
    → Lv300 까지 누적 299*3 + 29*7 = 1100 점.
      STAT_PER_POINT = {근력2, 체력20, 명중1, 마력2} 이므로 전부 체력에 넣으면
      +22,000 HP (Lv300 기본 22,780) — 기본치와 비슷한 규모로, 분배가 의미를 갖되
      기본 성장을 압도하지는 않는 선.

    수치는 여기서 정하지 말고 **시트에서 조절**하라는 것이 이 프로젝트 규칙이므로,
    이 스크립트는 시트를 채우는 용도이고 이후 조정은 xlsx 를 직접 고쳐도 된다.

실행
    python _tools/gen_status_points.py          # 미리보기
    python _tools/gen_status_points.py --write  # xlsx 에 반영
    이후 Unity 에서 `Tools/ParseAllLocal (로컬 전체 파싱)`.
"""
import sys
import os

import openpyxl

XLSX = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..',
                    'Assets', '@Resources', 'Data', 'Excel', 'PlayerStatusData.xlsx')

PER_LEVEL = 3
MILESTONE_EVERY = 10
MILESTONE_BONUS = 7


def grant_for(level):
    """그 레벨에 **도달할 때** 받는 점수. Lv1 은 시작 레벨이라 0."""
    if level <= 1:
        return 0
    return PER_LEVEL + (MILESTONE_BONUS if level % MILESTONE_EVERY == 0 else 0)


def main(write):
    wb = openpyxl.load_workbook(XLSX)
    ws = wb[wb.sheetnames[0]]
    head = [c.value for c in ws[1]]
    for n in ('Level', 'StatusPoint'):
        if n not in head:
            raise SystemExit('컬럼 없음: %s' % n)
    c_lv, c_sp = head.index('Level') + 1, head.index('StatusPoint') + 1

    total = 0
    rows = 0
    preview = []
    for r in range(4, ws.max_row + 1):          # 1행 헤더 / 2행 타입 / 3행 한글설명
        lv = ws.cell(r, c_lv).value
        if lv in (None, ''):
            continue
        lv = int(lv)
        g = grant_for(lv)
        total += g
        rows += 1
        if write:
            ws.cell(r, c_sp).value = g
        if lv in (1, 2, 9, 10, 11, 50, 100, 200, 300):
            preview.append((lv, g, total))

    print('=== 능력점수 지급 ===')
    print('  레벨당 %d점, %d레벨마다 +%d점' % (PER_LEVEL, MILESTONE_EVERY, MILESTONE_BONUS))
    print('  대상 행: %d' % rows)
    print()
    print('  레벨   지급   누적')
    for lv, g, acc in preview:
        print('  %4d  %4d  %5d' % (lv, g, acc))
    print()
    print('  Lv300 누적 총점: %d' % total)

    if not write:
        print()
        print('※ 미리보기만 했다. 반영하려면 --write')
        return

    wb.save(XLSX)
    print()
    print('반영 완료 — PlayerStatusData.xlsx %d행' % rows)
    print('Unity 에서 `Tools/ParseAllLocal (로컬 전체 파싱)` 을 돌릴 것.')


if __name__ == '__main__':
    main('--write' in sys.argv)
