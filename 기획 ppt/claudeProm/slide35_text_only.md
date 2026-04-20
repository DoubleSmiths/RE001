# [슬라이드 35] 제작의뢰(CustomOrder) Popup #2 — 물약/기술 분기 + 해금

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 버튼 클릭 시
- 선택 버튼 Frame_square_768 색 → `#F5DD6C`.
- 두 버튼 공통 Move DOTweenAnimation: 0.5s OutBack, TO y=-40.

### 배치
- [물약] 클릭: 왼쪽부터 회복물약 / 신속물약 / 행운물약.
- [기술] 클릭: [물약 주입기].
- 각 Scale DOTweenAnimation: 0.4s OutBack, From 1.8.

### 해금 상태
- 초기 해금: 회복물약만.
- 잠김: Sprite None `#000000 alpha 180` + Chat_Icon_Lock `#FFFFFF alpha 80`.
- 조건: 모험가 등급.

### 해금 모션 (최초 접할 시)
- Chat_Icon_Lock Shake/Position: 0.7s / Delay 0.5s / OutQuad / TO (1,1,0) / Vib 48 / Rand 59.
- Sprite None + Chat_Icon_Lock Fade(0.5s, Delay 1.2s) + Scale(0.5s Delay 1.2 OutBack TO=2).
- 끝나면 NEW 표시 + 상호작용 가능.

### 하단 연동
- 제작의뢰 해금 시 [물약] 버튼도 NEW + 해금.

## 작업 지시
1. **조사**
   - `UI_CustomOrderPopup.cs` ExpandGroup/InitLockStates/ApplyLockVisual.
   - prefab DOTweenAnimation Move/Scale 유무.
   - 해금 모션 로직.
   - `Managers.Game.AdvGrade` 연동.

2. **체크리스트**
   - [ ] 버튼 클릭 Frame 색 `#F5DD6C`?
   - [ ] Move TO y=-40 DOTweenAnimation?
   - [ ] 물약 3종/기술 1종 배치?
   - [ ] Scale from 1.8 DOTweenAnimation?
   - [ ] 초기 해금 회복물약만?
   - [ ] 잠김 시각 (alpha 180 + alpha 80)?
   - [ ] 모험가등급 연동?
   - [ ] 해금 Shake + Fade + Scale?
   - [ ] 해금 후 NEW?
   - [ ] [물약] 버튼 연동 해금?

3. **누락분만 구현**
   - 코드 트윈 → prefab DOTweenAnimation 이관.
   - `PlayUnlockReveal(lockbg, lockimg, onDone)` 헬퍼.
   - 모험가등급 기반 해금 조건.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CustomOrderPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
