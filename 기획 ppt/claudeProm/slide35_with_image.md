# [슬라이드 35] 제작의뢰(CustomOrder) Popup #2 — 물약/기술 분기 + 해금

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 35
- 첨부 이미지 (`기획 ppt/slide_images/slide35/`, 총 6개):
  - 물약/기술 버튼 눌렀을 때 Frame 색 변화, 물약/기술 리스트 배치, 해금 연출 예시

먼저 `기획 ppt/slide_images/slide35/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### [물약] / [기술] 버튼 눌렀을 때
1. 선택된 버튼의 `Frame_square_768` 색 → `#F5DD6C` 변환.
2. 두 버튼 공통 Anim (DOTweenAnimation Move):
   - Duration: 0.5s / Ease: OutBack
   - TO: `y=-40` (기존 y=17 → y=-40으로 이동)

### 물약 / 기술 배치
- [물약] 클릭 시: 왼쪽부터 **회복물약, 신속물약, 행운물약** 순으로 배치.
- [기술] 클릭 시: **[물약 주입기]** 기술 배치.
- 각 아이템 등장 Anim (DOTweenAnimation Scale):
  - Duration: 0.4s / Ease: OutBack
  - From: `1.8` (큰 상태에서 축소하며 등장)

### 해금 상태
- 초기: **회복물약만 해금**.
- 신속물약, 행운물약, 물약주입기 기술: 잠김.
- 잠김 시각: `Sprite None (000000, alpha 180)` + `Chat_Icon_Lock (FFFFFF, alpha 80)`.
- 해금 조건: **모험가 등급에 따라 해금**.

### 해금 모션 (최초로 해당 컨텐츠 접할 시)
- **Chat_Icon_Lock Shake/Position**:
  - Duration: 0.7s / Delay: 0.5s / Ease: OutQuad
  - TO: (x=1, y=1, z=0), Vibrato 48, Randomness 59
- **Sprite None + Chat_Icon_Lock Fade + Scale** (Delay 1.2s):
  - Fade: Duration 0.5s, Delay 1.2s
  - Scale: Duration 0.5s, Delay 1.2s, Ease OutBack, TO: 2 (2배로 커지며 사라짐)
- Anim 종료 후 플레이어 상호작용 가능 + **NEW 표시** 생성.

### 하단 연동
- 제작의뢰가 해금될 때 하단의 **[물약] 버튼도 NEW 표시와 함께 해금**.

## 현재 구현 힌트 (memory)
- `UI_CustomOrderPopup.cs` ExpandGroup(groupIndex) 구현: 버튼 색 F5DD6C, DOAnchorPosY(-40) OutBack 0.5s, 그룹 SetActive, PlayItemsScaleIn (localScale=1.8 → DOScale(1, 0.4, OutBack)).
- InitLockStates: HP 해금, SPD/Luck/AutoPotion 잠금. ApplyLockVisual이 autoPlay DOTweenAnimation DestroyImmediate + lockbg 180 / lockimg 80 적용.

## 작업 지시
1. **조사**
   - `UI_CustomOrderPopup.cs` ExpandGroup / ResetBottomButtons / InitLockStates / ApplyLockVisual 로직.
   - prefab에 Mythic 버튼 DOTweenAnimation (Move TO y=-40) 유무 — 현재 코드 DOAnchorPosY 사용.
   - Scale 등장 Anim (From 1.8) prefab DOTweenAnimation 유무.
   - 해금 모션 (Shake + Fade + Scale) 트리거 로직 유무.
   - 해금 조건 (`Managers.Game.AdvGrade` 등) 연동 확인.

2. **체크리스트**
   - [ ] 물약/기술 버튼 클릭 시 Frame 색 #F5DD6C?
   - [ ] 두 버튼 Move DOTweenAnimation (TO y=-40, 0.5s OutBack)?
   - [ ] 물약 3종/기술 1종 배치?
   - [ ] 각 아이템 Scale from 1.8 DOTweenAnimation?
   - [ ] 초기 해금: 회복물약만?
   - [ ] 잠김 시각: lockbg #000000 alpha 180 + lockimg Chat_Icon_Lock alpha 80?
   - [ ] 해금 조건 모험가등급 연동?
   - [ ] 해금 모션 Shake + Fade + Scale 연출?
   - [ ] 해금 후 NEW 표시 출현?
   - [ ] 제작의뢰 해금 시 하단 [물약] 버튼 해금 + NEW?

3. **누락분만 구현**
   - DOAnchorPosY 등 코드 트윈을 prefab DOTweenAnimation으로 이관 (선택).
   - 해금 모션 함수 `PlayUnlockReveal(Transform lockbg, Transform lockimg, Action onDone)` 신설.
   - `Managers.Game.AdvGrade` 기반 해금 조건 체크.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CustomOrderPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (AdvGrade)
