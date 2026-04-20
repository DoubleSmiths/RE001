# [슬라이드 41] 물약창 Popup #1 — 기본 + 자동물약

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 41
- 첨부 이미지 (`기획 ppt/slide_images/slide41/`, 총 17개):
  - Popup 전체, 자동물약 화면, 리소스 모음 (`Title_frame_0m`, `Hp_frame_short`, `Frame_bg_big_2`, `Dead_bar`, `General_Container_A`, `1_basic_button_s`, `Chat_Icon_Lock`, `Hp_orange`, `2_options_on`, `HP_Portrait_frame_bg/_frame`, `loading_rotate_0`), 자동물약 조건 HP Ratio Slider, 잠금 상태 예시, New Text 조건 예시

먼저 `기획 ppt/slide_images/slide41/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 용도 / 호출
- 연금공방에서 **첫 물약 획득** 시 하단에 [물약] 버튼 생성 → 클릭 → 물약창 Popup 호출.

### 리소스
- `Title_frame_0m` — 제목 프레임
- `Hp_frame_short` — `#ADADAD`
- `Frame_bg_big_2`
- `Dead_bar` — 구분자
- `General_Container_A`
- `1_basic_button_s` — 소형 버튼
- `Chat_Icon_Lock` — 잠금 아이콘
- `Hp_orange` — alpha 220
- `2_options_on` — `#E7E7E7`
- `HP_Portrait_frame_bg`, `HP_Portrait_frame`
- `loading_rotate_0` + Animator Clip `Loading`

### Popup Open Anim (DOTweenAnimation)
- Move / Duration 0.5s / Delay 0 / Ease OutBack / Loops 1
- From: `(x=0, y=40, z=0)` (주의: 아래가 아니라 위로 40에서 시작 — 다른 Popup과 방향 다름)

### 자동물약 기능
- **HP Ratio Slider** — 자동 사용 트리거 %
- **자동물약 On/Off Btn**
- **물약 상세정보 Popup 호출** 버튼
- 위 정보는 **저장됨** (PlayerPrefs 또는 GameData)

### 자동물약 기억 전 / 후
- 기억 전: HP Ratio Slider 비활성 (interactable=false) + AutoPotion Gauge Lock 활성.
- 기억 후: Slider 조절 가능 + `_hpRatioText`에 "n%" 표시.

### 잠금 상태
- Chat_Icon_Lock 사용.

### New Text 조건
1. 물약 제작 시
2. 자동물약 기억 시

## 현재 구현 힌트 (memory)
- `UI_PotionPopup.cs` (GUID 31776a1f7ab90514eb9bfa085065cf4a) 구현됨.
- Slot 구조체: button/potionImg/cooldownBar/finishAnim/leftTxt/dataId/cooldown/count/unlocked.
- PlayerPrefs: `PotionPopup.AutoOn/HpRatio/Memorized`.
- HP 해금 5개, 쿨다운 5s. Speed 잠금.
- 진입 애님: From y=+40 OutBack 0.5s.
- BlackBG fade 0.3s 220/255.
- `CreateLockOverlay`: LockBg 000000 180, LockIcon Chat_Icon_Lock FFFFFF 80.

## 작업 지시
1. **조사**
   - `UI_PotionPopup.cs` 전체.
   - prefab 리소스 배치 (Title/Hp_frame_short/Frame_bg_big_2/Dead_bar/General_Container_A/1_basic_button_s/Chat_Icon_Lock/Hp_orange/2_options_on/HP_Portrait_frame/loading_rotate_0).
   - HP Ratio Slider + AutoBtn + Lock 상태 로직.
   - Open Anim prefab DOTweenAnimation 또는 코드 DOTween.
   - NEW Text 조건 2종 트리거 위치.

2. **체크리스트**
   - [ ] Popup Open Move (from y=40 OutBack 0.5s) DOTweenAnimation?
   - [ ] 리소스 10+종 배치?
   - [ ] HP Ratio Slider 기억 전 interactable=false?
   - [ ] 기억 후 `n%` 텍스트 표기?
   - [ ] 자동물약 On/Off Animator 전환?
   - [ ] 잠금 슬롯 LockBg/LockIcon 적용?
   - [ ] 연금공방 첫 물약 획득 시 [물약] 버튼 생성?
   - [ ] PlayerPrefs/GameData 저장 (AutoOn/HpRatio/Memorized)?
   - [ ] NEW 조건: 물약 제작 시 + 자동물약 기억 시?

3. **누락분만 구현**
   - 리소스 누락 배치 보완.
   - `MarkAutoPotionMemorized()` 외부 호출 (물약주입기 해금 시점).
   - Open Anim prefab DOTweenAnimation 이관 (코드 경우).
   - NEW 트리거 지점에 `AddNewBadge()` 호출.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionPopup.prefab` (또는 UI_PotionPopupON.prefab)
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
