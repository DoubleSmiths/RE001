#!/usr/bin/env bash
# 생성된 고유 이미지를 게임에 반영하는 마무리 3단계.
#
# aether_unique.py가 PNG를 떨구는 것만으로는 인게임에 아무것도 안 바뀐다.
#   1) 시트의 IconLabel을 종별 카드(Mon_{id})로 돌린다
#   2) xlsx → JSON 재생성 (게임은 JSON만 읽는다)
#   3) 임포트 설정(픽셀아트) + 어드레서블 등록 (주소 없으면 런타임에서 못 찾는다)
#
# 이미지 생성이 끝날 때마다 다시 돌려도 안전하다(있는 카드만 반영, 나머지는 원형 유지).
# Unity Editor가 켜져 있어야 한다.
set -e
cd "$(dirname "$0")/.."
CLI="C:/Users/Mycom/AppData/Local/unity-cli/unity-cli.exe"

echo "[0/3] 대기 폴더(_generated) → Assets 일괄 이동"
# 생성 중에는 Assets 밖(_generated)에 쌓는다. 한 장씩 떨구면 Unity가 매번 자동 임포트를 돌아
# 에디터가 묶이고 unity-cli 리스너까지 죽는다. 여기서 한 번에 옮겨 임포트도 한 번만 돌게 한다.
moved=0
for pair in "Region:Assets/@Resources/Sprite/Map/Region" \
            "MonsterCard:Assets/@Resources/Sprite/Monster/Card"; do
    src="_generated/${pair%%:*}"
    dst="${pair##*:}"
    [ -d "$src" ] || continue
    mkdir -p "$dst"
    for f in "$src"/*.png; do
        [ -e "$f" ] || continue
        mv "$f" "$dst/" && moved=$((moved+1))
    done
done
echo "  이동 $moved장"

echo "[1/3] IconLabel → 종별 카드"
python _tools/repoint_monster_icons.py

echo "[2/3] xlsx → JSON 파싱"
"$CLI" menu "Tools/ParseAllLocal (로컬 전체 파싱)"
sleep 15

echo "[3/3] 임포트 설정 + 어드레서블 등록"
"$CLI" menu "Tools/Register Generated Icons (아이콘 임포트+어드레서블)"
sleep 30
"$CLI" console --lines 10 | grep -i IconReg | tail -1

echo
echo "카드 $(ls Assets/@Resources/Sprite/Monster/Card/*.png 2>/dev/null | wc -l)/503" \
     "· 배경 $(ls Assets/@Resources/Sprite/Map/Region/*.png 2>/dev/null | wc -l)/150"
echo "검증: 플레이 모드에서 Test/Integration/종합 시스템 검증"
