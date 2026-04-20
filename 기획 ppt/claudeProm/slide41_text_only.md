# [슬라이드 41] 물약창 Popup #1 — 기본 + 자동물약

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 용도 / 호출
- 연금공방 첫 물약 획득 시 [물약] 버튼 생성 → 클릭 → Popup.

### 리소스
- `Title_frame_0m`, `Hp_frame_short` (`#ADADAD`), `Frame_bg_big_2`, `Dead_bar`
- `General_Container_A`, `1_basic_button_s`, `Chat_Icon_Lock`, `Hp_orange` (alpha 220), `2_options_on` (`#E7E7E7`)
- `HP_Portrait_frame_bg`, `HP_Portrait_frame`
- `loading_rotate_0` + Animator Clip `Loading`

### Popup Open Anim (DOTweenAnimation)
- Move / 0.5s / OutBack / Loops 1
- From (x=0, y=40, z=0) — **위에서 아래로 내려옴** (다른 Popup과 방향 반대)

### 자동물약
- HP Ratio Slider, On/Off 버튼, 물약 상세정보 Popup 호출 버튼.
- 저장: PlayerPrefs/GameData.
- 기억 전: Slider 비활성 + Lock 활성.
- 기억 후: Slider 조작 가능 + "n%" 표기.

### 잠금 상태
- Chat_Icon_Lock 사용.

### New Text 조건
1. 물약 제작 시
2. 자동물약 기억 시

## 작업 지시
1. **조사**
   - `UI_PotionPopup.cs` 전체.
   - prefab 리소스 배치.
   - Slider/Lock 로직.
   - Open Anim.
   - NEW 트리거.

2. **체크리스트**
   - [ ] Move (from y=40 OutBack 0.5s)?
   - [ ] 리소스 10+종?
   - [ ] Slider 기억 전 비활성?
   - [ ] 기억 후 `n%`?
   - [ ] AutoBtn Animator 전환?
   - [ ] LockBg/LockIcon?
   - [ ] 첫 물약 시 [물약] 버튼 생성?
   - [ ] PlayerPrefs 저장?
   - [ ] NEW 2조건?

3. **누락분만 구현**
   - 리소스 누락 보완.
   - `MarkAutoPotionMemorized()` 외부 호출.
   - Open Anim prefab DOTweenAnimation 이관.
   - NEW `AddNewBadge()` 호출.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionPopup.prefab`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
