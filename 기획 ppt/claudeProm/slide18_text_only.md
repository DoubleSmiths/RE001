# [슬라이드 18] 아이템 아이콘 (공통 구성 + 부족/충족 색)

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 아이템 아이콘 공통 구조 (Layer)
1. 기본 아이콘 (아이템별 이미지)
2. Mask 처리 — 중심 정렬
3. 등급색 배경/테두리

### 소지품 Popup 수량 텍스트
- 위치: **하단 가운데**
- 폰트: `PF_Stardust` 고정 (숫자 규칙 슬라이드 3 연계)

### 아이템 상세정보 Popup — 사용/필요 아이템 색
- 부족: `#FF0000`
- 충족: `#00FF00`

### 상호작용
- 모든 아이콘 클릭 → 해당 아이템 상세정보 Popup 호출.

### 잠김/비활성 상태
- 기본 Icon + 불투명 검은색 layer.

## 작업 지시
1. **조사**
   - `UI_InvSlot.cs`/`UI_ImportantSlot.cs` 아이콘 구성.
   - 수량 텍스트 위치/폰트.
   - 상세정보 Popup 부족/충족 색 분기.
   - 클릭 → 상세 Popup 공통 헬퍼.

2. **체크리스트**
   - [ ] 기본 + Mask + 등급색 3 레이어?
   - [ ] Mask 중심 정렬?
   - [ ] 수량 TMP 하단 가운데 + PF_Stardust?
   - [ ] 부족 `#FF0000` / 충족 `#00FF00`?
   - [ ] 잠김 검은 오버레이?
   - [ ] 공통 아이템 클릭 헬퍼?

3. **누락분만 구현**
   - 공용 `UI_ItemIcon.prefab` 또는 SubItem 통합.
   - 수량 폰트 PF_Stardust 교체.
   - `SetShortageColor(bool enough)` 헬퍼.
   - 잠금 오버레이 공통 prefab 재사용.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`, `UI_ImportantSlot.cs`
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs` (부족/충족 색 예시)
- `Assets/@Resources/Prefab/UI/SubItem/`
