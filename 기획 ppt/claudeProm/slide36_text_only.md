# [슬라이드 36] 제작의뢰 #3 — 물약/기술 상세정보 호출

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 상태
- 해금 안됨 → 상호작용 불가.
- 해금됨 → 클릭 가능.

### 클릭 동작
- 물약 LIST → 물약 상세정보 Popup 호출.
- 기술 LIST → 기술 상세정보 Popup 호출.

### 상세정보 Popup Anim (공통 DOTweenAnimation)
- Move / 0.5s / OutBack / Loops 1
- From (0, -120, 0)
- OnActive Restart, OnDisable Rewind.

### 물약 아이콘
- 현재 등급/단계에 따라 스프라이트 변경.

## 작업 지시
1. **조사**
   - `UI_CustomOrderPopup.BindItemClick` 해금 아이템 onClick.
   - `UI_PotionDetailPopup.cs` / `UI_SkillDetailPopup.cs` Open Anim.
   - 등급/단계 Sprite 매핑.

2. **체크리스트**
   - [ ] 해금 상태별 클릭 가능/불가?
   - [ ] 물약 → PotionDetail 호출?
   - [ ] 기술 → SkillDetail 호출?
   - [ ] Move (from y=-120 OutBack 0.5s)?
   - [ ] OnDisable Rewind?
   - [ ] 물약 아이콘 등급/단계 교체?

3. **누락분만 구현**
   - 등급/단계 Sprite 경로 매핑.
   - Data에 IconSpriteName 필드.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_SkillDetailPopup.cs`
- `Assets/@Scripts/Data/Data.Contents.cs`
