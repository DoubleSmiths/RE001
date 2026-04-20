# [슬라이드 36] 제작의뢰 #3 — 물약/기술 상세정보 호출

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 36
- 첨부 이미지 (`기획 ppt/slide_images/slide36/`, 총 6개):
  - 해금/잠금 상태, 물약 상세정보 Popup Anim, 기술 상세정보 Popup Anim, 물약 아이콘 등급별 변경 예시

먼저 `기획 ppt/slide_images/slide36/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 상태 표시
- **해금 안됨** (상호작용 불가) — 슬라이드 35 잠금 시각.
- **해금됨** (상호작용 가능) — 물약/기술 선택 가능.

### 클릭 동작
- 물약 LIST 중 클릭 → **해당 물약 상세정보 Popup** 호출.
- 기술 LIST 중 클릭 → **해당 기술 상세정보 Popup** 호출.

### 기술/물약 상세정보 Popup Anim (공통, DOTweenAnimation)
- Type: Move / Duration: 0.5s / Delay: 0 / Ease: OutBack / Loops: 1
- From: `(x=0, y=-120, z=0)`
- OnActive → Restart, OnDisable → Rewind

### 물약 아이콘
- **현재 등급에 따라 변함** (1~3단계 → 다음 등급 1단계 전환).

## 현재 구현 힌트 (memory)
- `UI_PotionDetailPopup.cs`, `UI_SkillDetailPopup.cs` 신설 완료.
- Addressable 등록 완료.
- `UI_CustomOrderPopup.BindItemClick` 에서 해금된 아이템만 onClick 바인딩 → `OpenPotionDetail(30001)` / `OpenSkillDetail(0)`.
- Popup 진입 애님 이미 OutBack 0.5s 구현.

## 작업 지시
1. **조사**
   - `UI_CustomOrderPopup.BindItemClick` 로직 확인 (해금 아이템만 onClick).
   - `UI_PotionDetailPopup.cs` / `UI_SkillDetailPopup.cs` Open Anim (Move y=-120 OutBack 0.5s) 여부.
   - 물약 아이콘이 등급/단계 변화에 따라 스프라이트 교체되는 로직 (등급별 Sprite 경로 매핑) 여부.

2. **체크리스트**
   - [ ] 해금 상태에 따라 클릭 가능/불가?
   - [ ] 물약 클릭 → UI_PotionDetailPopup 호출?
   - [ ] 기술 클릭 → UI_SkillDetailPopup 호출?
   - [ ] 두 Popup 모두 Move Anim (from y=-120 OutBack 0.5s) DOTweenAnimation?
   - [ ] OnDisable Rewind?
   - [ ] 물약 아이콘이 등급/단계 변화에 따라 스프라이트 자동 교체?

3. **누락분만 구현**
   - 등급/단계별 물약 아이콘 Sprite 경로 매핑 (`Define.POTION_ICON_PATHS[grade, step]` 등).
   - PotionData/SkillData에 `IconSpriteName` 필드 확보.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs` (BindItemClick)
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_SkillDetailPopup.cs`
- `Assets/@Scripts/Data/Data.Contents.cs` (PotionData, SkillData)
