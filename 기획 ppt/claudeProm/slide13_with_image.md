# [슬라이드 13] 상태정보(Status) Popup #3 — 능력 상세정보 UI

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 13
- 첨부 이미지 (`기획 ppt/slide_images/slide13/` 폴더, 총 15개):
  - `slide13_shape047_그림_9.png`, `slide13_shape051_그림_22.png` 및 기타 — [능력 상세정보] 버튼 클릭 후 변경된 프레임 색/글로우 예시, role_icon_assassin/gladiator/darkmage 아이콘, Decor_Title_options_long, shop_tab_focus, 1_plus 버튼 등

먼저 `기획 ppt/slide_images/slide13/` 내 파일을 `ls`로 확인 후 모든 png/gif를 Read로 열어 시각적으로 파악할 것.

## 공통 규약
- **CLAUDE.md** 준수 / **DOTween**: 기존 DOTweenAnimation 우선, 코드 Sequence 지양.
- **사용자 Unity 종료 금지** / 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약

### [능력 상세정보] 버튼 클릭 시
- **프레임 색 변화**:
  - `Frame_bg_big` 색: `#FFD000` (노랑)으로 변환
  - Mask 처리된 `shop_frame_glow` (색 `#000000`) **추가** (Mask 색 `#FFBA00`, alpha 160)
- **상태정보 Popup창 하단에 추가창 Active** (능력 상세정보 확장 UI 출현)

### 역할 아이콘
- `role_icon_assassin` — Color `#F57A6E` (핑크 적)
- `role_icon_gladiator` — Color `#65A9FD` (파랑)
- `role_icon_darkmage` — Color `#C6FFD7` (민트 녹색)
- `Decor_Title_options_long` — 섹션 제목 데코
- 섹션별 색상: `#FAAFA4`, `#9EE0FF`, `#C6FFD7` (공격/수비/보조와 대응 가능성)

### 포커스/탭
- `shop_tab_focus` — 현재 선택된 탭 강조

### Text 색상 규칙
- Text Color: `#FFFFFF`
- Text Outline: `#FF0000` (x=0.1, y=-0.1)
- Text Color: `#00CDFF`
- Text Color: `#FFFFFF` / Outline `#FFC000` (x=0.2, y=-0.2)
- Text Color: `#F57A6E`
- Text Color: `#87BBFD`
- Text Color: `#C6FFD7`

### 최하단 버튼 (능력점수 할당)
- 능력점수 = 0 일 때: 비활성/회색 스타일
- 능력점수 > 0 일 때: 활성 (`1_plus` 스프라이트 활성)

## 작업 지시
1. **조사**
   - `UI_CharacterPopup.cs` [능력 상세정보] 버튼 핸들러 유무 확인.
   - prefab에서 Frame_bg_big 색 변경 로직(색 토글) 유무.
   - shop_frame_glow 자식 추가 / Mask 색 `#FFBA00` alpha 160 적용 유무.
   - role_icon_assassin/gladiator/darkmage 스프라이트 존재 여부.
   - `Decor_Title_options_long`, `shop_tab_focus`, `1_plus` 리소스 여부.

2. **체크리스트**
   - [ ] [능력 상세정보] 버튼 핸들러가 추가창 Active + Frame 색 변경?
   - [ ] Frame_bg_big → `#FFD000` 토글 구현?
   - [ ] shop_frame_glow (색 `#000000`, Mask `#FFBA00` alpha 160) 추가?
   - [ ] role_icon 3종 스프라이트 바인딩 + 색상 지정?
   - [ ] Decor_Title_options_long 섹션 데코 배치?
   - [ ] Text 색상 규칙 7종 적용?
   - [ ] 1_plus 버튼이 능력점수 >0일 때만 활성?
   - [ ] shop_tab_focus로 현재 탭 강조?

3. **누락분만 구현**
   - Frame 색 변경은 `DOColor` 코드보다 prefab의 `DOTweenAnimation` Color 트윈 선호.
   - 추가창 Active/InActive 토글 함수 `ShowAbilityDetailPanel(bool on)` 신설 (이미 있으면 유지).
   - Text 색상은 TMP material preset 또는 런타임 color 할당.

4. **결과 보고**
   - 체크리스트 결과 + 수정 prefab/파일 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
- `Assets/@Resources/Sprite/` (role_icon_assassin/gladiator/darkmage, Decor_Title_options_long, shop_tab_focus, 1_plus, Frame_bg_big, shop_frame_glow)
