# [슬라이드 43] 여정(Travel) Popup #1 — 기본 구성

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### Popup Open Anim (DOTweenAnimation)
- Move / 0.5s / OutBack / Loops 1
- From (0, -120, 0)
- OnDisable PlayBackwards

### 리소스
- `Frame_square_256`, `Title_frame_0L`, `pick_hero_frame_2`, `Title_frame_m`
- `Decor_description_line`, `Frame_bg_big`
- `Img_Field` (alpha 80, Mat `Path_Image_Shadow`)
- `Img_Town`, `T_gray_button_s`
- `Shop_tab` (`#959595`)

### Text
- `TravelText`, `PlaceName` (유동적), `TownText`, `ExploreText`, `HeadOutText`, `BackBtnText`

### 미해결
- 장소별 MAP_IMG 연결 방식 미정 — `PlaceData.MapImageName` 제안.

## 작업 지시
1. **조사**
   - `UI_TravelPopup.cs` / prefab 존재.
   - 리소스 배치.
   - 현재 위치 property.
   - 맵 이미지 로딩.

2. **체크리스트**
   - [ ] UI_TravelPopup 클래스 + prefab?
   - [ ] Move (from y=-120 OutBack 0.5s)?
   - [ ] OnDisable PlayBackwards?
   - [ ] 리소스 10+종?
   - [ ] 6종 Text 바인딩?
   - [ ] PlaceName 동적?
   - [ ] Img_Field Mat Path_Image_Shadow + alpha 80?
   - [ ] Shop_tab #959595?
   - [ ] BlackBG_Layer 공통 처리?

3. **누락분만 구현**
   - UI_TravelPopup + prefab 신설/확장.
   - PlaceData.MapImageName + Addressable.
   - Open Anim prefab DOTweenAnimation.
   - 장소 리스트 (슬라이드 44).

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Scripts/Utils/Define.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_TravelPopup.prefab`
- `Assets/@Scripts/Data/Data.Contents.cs`
