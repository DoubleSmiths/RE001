#!/usr/bin/env bash
# 강화 연출을 다시 녹화하고 원작과 대조한다. 한 번 고칠 때마다 이걸 돌린다.
#   bash _tools/forge_record.sh [출력이름]
set -u
CLI="C:/Users/Mycom/AppData/Local/unity-cli/unity-cli.exe"
OUT="${1:-AI/forge_ref/cmp.png}"
cd "$(dirname "$0")/.." || exit 1

# 이미 플레이 중이면 그대로, 아니면 켠다
if ! echo 'return Application.isPlaying ? 1 : 0;' | "$CLI" exec 2>/dev/null | grep -q '^1'; then
  "$CLI" editor play --wait >/dev/null 2>&1 || { echo "플레이 모드 진입 실패"; exit 1; }
  sleep 4
fi

rm -rf Screenshots/demo30
"$CLI" menu "Test/Forge/연출 전 구간 녹화 (30초)" >/dev/null 2>&1

for _ in $(seq 1 45); do
  n=$(ls Screenshots/demo30 2>/dev/null | wc -l)
  [ "$n" -ge 440 ] && break
  sleep 2
done
sleep 3
echo "녹화 $(ls Screenshots/demo30 | wc -l)장"

PYTHONIOENCODING=utf-8 python _tools/forge_compare.py --out "$OUT"
