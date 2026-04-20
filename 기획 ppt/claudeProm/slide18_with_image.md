# [슬라이드 18] 아이템 아이콘 (공통 구성 + 부족/충족 색)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 18
- 첨부 이미지 (`기획 ppt/slide_images/slide18/` 7개):
  - `slide18_shape025_그림_4.png`, `slide18_shape026_그림_6.png` 외 — 아이템 아이콘 구성(기본+Mask+등급색), 소지품 Popup 수량 표시, 상세정보 Popup의 부족/충족 색 예시

먼저 `기획 ppt/slide_images/slide18/` 내 모든 파일을 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 아이템 아이콘 공통 구조 (Layer)
1. **기본 아이콘** (아이템별 이미지)
2. **Mask 처리** — 아이콘 위치는 중심(center)
3. **해당 아이템 등급색** — 배경 또는 테두리로 적용

### 소지품 Popup 아이콘 추가 구성
- **수량 텍스트**: 해당 아이템 소지 개수.
  - 위치: **하단 가운데**
  - 폰트: `PF_Stardust` **고정** (슬라이드 3 숫자 규칙 연계)

### 아이템 상세정보 Popup 사용/필요 아이템 색상
- **부족**: `#FF0000`
- **충족**: `#00FF00`
- 부족한 경우 수량 텍스트를 `#FF0000`로 표시, 충족 시 `#00FF00`.

### 상호작용
- **모든 아이템 아이콘은 상호작용 가능**. 누르면 해당 아이템의 **상세정보 Popup** 호출.

### (Inactive 상태 예시 — 불투명 검은색 layer)
- 기획서에 "기본 Icon + 불투명 검은색 layer" 언급 — 아마도 **잠긴/비활성** 상태 표시용 (어두운 오버레이).

## 작업 지시
1. **조사**
   - `UI_InvSlot.cs`, `UI_ImportantSlot.cs` 의 아이콘 구성 확인 (기본 Image + Mask + 등급 Glow/Border).
   - 수량 텍스트 위치/폰트 확인.
   - 상세정보 Popup(슬라이드 21 연계)에서 필요재료 부족/충족 색 변경 로직 확인.
   - 아이템 클릭 → 상세정보 Popup 호출 공통 헬퍼 확인.

2. **체크리스트**
   - [ ] 아이콘 = 기본 Image + Mask + 등급색 구성?
   - [ ] Mask 중심 정렬?
   - [ ] 수량 TMP 하단 가운데, PF_Stardust 고정 폰트?
   - [ ] 부족 텍스트 `#FF0000` / 충족 `#00FF00` 분기?
   - [ ] 잠김/비활성 아이콘에 불투명 검은 오버레이?
   - [ ] 모든 아이콘 클릭 → 상세정보 Popup 호출 (공통 헬퍼)?

3. **누락분만 구현**
   - 공용 `UI_ItemIcon.prefab` 신설 또는 기존 SubItem 통합 검토.
   - 수량 TMP 폰트 PF_Stardust 교체.
   - `SetShortageColor(bool enough)` 헬퍼 (부족/충족 색 분기).
   - 잠금 오버레이 공통 prefab 재사용 (슬라이드 35 스펙 차용).

4. **결과 보고**
   - 체크리스트 결과 + 수정 prefab/파일.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`
- `Assets/@Scripts/UI/SubItem/UI_ImportantSlot.cs`
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs` (재료 아이콘 부족/충족 색 예시 L117 부근)
- `Assets/@Resources/Prefab/UI/SubItem/`
