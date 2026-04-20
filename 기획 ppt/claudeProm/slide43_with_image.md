# [슬라이드 43] 여정(Travel) Popup #1 — 기본 구성

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 43
- 첨부 이미지 (`기획 ppt/slide_images/slide43/`, 총 12개):
  - Popup 전체 레이아웃, `Frame_square_256`, `Title_frame_0L`, `pick_hero_frame_2`, `Title_frame_m`, `Decor_description_line`, `Frame_bg_big`, `Img_Field`, `Img_Town`, `T_gray_button_s`, `Shop_tab` (`#959595`), TravelText/PlaceName/TownText/ExploreText/HeadOutText/BackBtnText 예시

먼저 `기획 ppt/slide_images/slide43/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### Popup Open Anim (DOTweenAnimation)
- Move / Duration 0.5s / Delay 0 / Ease OutBack / Loops 1
- From `(x=0, y=-120, z=0)`
- OnDisable → PlayBackwards

### 리소스
- `Frame_square_256` — 메인 프레임
- `Title_frame_0L` — 큰 타이틀 프레임
- `pick_hero_frame_2` — 캐릭터 프레임 (현재 위치 표시)
- `Title_frame_m` — 중간 타이틀
- `Decor_description_line` — 장식 라인
- `Frame_bg_big` — 배경
- `Img_Field` — 필드 아이콘 (alpha 80, Mat `Path_Image_Shadow`)
- `Img_Town` — 마을 아이콘
- `T_gray_button_s` — 버튼
- `Shop_tab` — `#959595`

### Text 필드
- `TravelText` — "여정" 타이틀
- `PlaceName` — **유동적** (현재 플레이어가 있는 장소)
- `TownText`, `ExploreText`, `HeadOutText`, `BackBtnText`

### 미해결 사항 (기획서 주석)
- "장소마다 MAP_IMG 연결 해야 하는데 어떻게 할 건지" — 장소별 맵 이미지 매핑 방식 미정. 구현 시 `PlaceData.MapImageName` 필드 추가 제안.

## 작업 지시
1. **조사**
   - `UI_TravelPopup.cs` 존재 여부 (memory에 따르면 스텁만 존재했음, `UI_MapSelectPopup.cs` 내 `UI_TravelPopup` 클래스 존재 확인됨).
   - prefab 존재 여부 (`UI_TravelPopup.prefab` 또는 `UI_MapSelectPopup.prefab`).
   - 리소스 배치 (Frame/Title/Hero frame/Decor/BG/Img_Field/Img_Town/버튼).
   - `Managers.Game.CurrentPlaceName` / `CurrentPlaceId` 유무.
   - 장소별 맵 이미지 로딩 구조.

2. **체크리스트**
   - [ ] UI_TravelPopup 클래스 + prefab 존재?
   - [ ] Open Anim Move (from y=-120 OutBack 0.5s) DOTweenAnimation?
   - [ ] OnDisable PlayBackwards?
   - [ ] 리소스 10+종 배치?
   - [ ] TravelText/PlaceName/TownText/ExploreText/HeadOutText/BackBtnText 바인딩?
   - [ ] PlaceName은 현재 플레이어 위치에 따라 동적 변경?
   - [ ] Img_Field Mat `Path_Image_Shadow` + alpha 80?
   - [ ] Shop_tab 색 #959595?
   - [ ] 슬라이드 8 BlackBG_Layer 공통 처리?

3. **누락분만 구현**
   - UI_TravelPopup.cs + prefab 신설 or 기존 스텁 확장.
   - `PlaceData.MapImageName` 필드 추가 + Addressable 로딩.
   - Open Anim prefab DOTweenAnimation (AutoPlay Off, Link OnEnable).
   - 장소 리스트 스크롤 (슬라이드 44 연계).

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs` (또는 UI_MapSelectPopup.cs)
- `Assets/@Scripts/Utils/Define.cs` (L329 Travel UI 미구현 주석)
- `Assets/@Resources/Prefab/UI/Popup/UI_TravelPopup.prefab`
- `Assets/@Scripts/Data/Data.Contents.cs` (PlaceData)
