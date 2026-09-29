# 몬스터 외 AI 이미지 점검·재생성

2026-09-29. 사용자 지정 방식: **문제 있는 이미지부터 전체 순차 수정, 문제 이미지는 모두 AI로 재생성**.
내장 image_gen 편집 도구를 사용했다. 기존 RGB를 알파만 바꿔 복원하는 방식은 사용하지 않았다.
내보내기 도구는 생성된 결과의 크기·여백만 기존 소비 규격에 맞춘다.

## 결과

생성 스크립트의 출력 경로와 해당 폴더의 PNG **401장**을 목록화하고 알파 합성 미리보기로 점검했다.
재료 폴더의 추가 파일처럼 생성 경로만으로 개별 출처를 확정할 수 없는 파일도 점검 범위에 포함한다.

| 범위 | 점검 | 재생성·교체 |
|---|---:|---:|
| 장비 아이콘 | 60 | 18 |
| 스킬 아이콘 | 67 | 25 |
| 재료 아이콘 | 31 | 9 |
| 지역 미리보기 | 155 | 7 |
| 전투 배경 레이어 | 75 | 2 |
| 마을 배경 | 5 | 2 |
| 대장간·상점 내부 | 2 | 0 |
| 상점·수녀 NPC/초상 및 메뉴 아이콘 | 6 | 0 |
| 합계 | **401** | **63** |

주요 결함은 금속·돌·스킬 메달 내부의 투명 구멍, 하늘·지형의 대규모 알파 결손,
조각난 화산·성당 장면, 광산 마을의 큰 단색 여백이었다.
나머지 338장은 이번 정적 시각 점검에서 동일한 수정이 필요한 결함을 확인하지 않아 유지했다.

**원본 뷰에서 보이는 자홍색만으로 결함을 판정하지 않는다.** NPC 원본의 자홍색 상당수는
알파 0 영역의 RGB였으며 실제 합성 화면에는 나타나지 않았다. 보라 장비·비늘·마법 문양도
의도된 색이라 이를 오염으로 지우지 않았다.

## 확인 화면과 기록

- [수정 전후 63장 비교](comparison.html): 밝은/어두운/초록 배경 선택.
- [전체 401장 목록](index.html): 종류별 필터, 실제 Assets PNG 표시.
- `inventory-before.json` / `inventory.json`: 전후 크기·알파·자홍 후보 픽셀 수.
- `before/`: 교체 전 PNG. `generated/`: 내장 도구로 생성한 선택 원본.
- [repairs.json](repairs.json): 교체 대상과 각 이미지에 사용한 전체 프롬프트.
- [verification.json](verification.json): 오프라인 검증 결과.

PNG 파일명·원래 크기·기존 `.meta`를 유지했다. 스프라이트 GUID, PPU, 피벗, 슬라이싱,
Addressables와 데이터는 바꾸지 않았다. 아이콘은 64×64 내에서 가로세로 비율을 유지하고
2px 이상 여백을 확보했다. 완성 배경은 전체 불투명, 중경 레이어는 나무·바위 사이 투명을 유지한다.

Unity Editor와 문서에 명시된 unity-cli 실행 파일을 사용할 수 없어 **실제 임포트·콘솔·플레이 검증은 미실시**다.
Editor 실행 후 refresh/compile → error 콘솔 → 가방·스킬·재료·여정·마을·전투 배경을 확인해야 한다.

## 재현

저장소 루트, Python 3 + Pillow / Windows PowerShell + System.Drawing:

```powershell
py -3.12 _tools/inspect_ai_repairs.py
powershell -NoProfile -ExecutionPolicy Bypass -File _tools/export_ai_repairs.ps1 -Apply
py -3.12 _tools/audit_ai_images.py
powershell -NoProfile -ExecutionPolicy Bypass -File _tools/preview_ai_images.ps1
py -3.12 _tools/build_ai_comparison.py
py -3.12 _tools/verify_ai_repairs.py
```

`-Apply`를 생략하면 `preview/`에만 내보낸다. 원본 시트의 해시와 기존 메타 해시를 먼저 확인한다.
