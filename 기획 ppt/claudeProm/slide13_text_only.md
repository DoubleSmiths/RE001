# [슬라이드 13] 상태정보(Status) Popup #3 — 능력 상세정보 UI

## 공통 규약
- **CLAUDE.md** / **DOTween**: 기존 DOTweenAnimation 우선, 코드 Sequence 지양.
- **사용자 Unity 종료 금지** / 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (텍스트 전용)

### [능력 상세정보] 버튼 클릭 시
- `Frame_bg_big` 색: `#FFD000` 으로 변환
- `shop_frame_glow` (색 `#000000`) 를 Mask(색 `#FFBA00`, alpha 160) 처리하여 추가
- Popup 하단에 **추가창 Active** (능력 상세정보 확장 UI 출현)

### 역할 아이콘 & 색상
| 아이콘 | Color |
|---|---|
| `role_icon_assassin` | `#F57A6E` |
| `role_icon_gladiator` | `#65A9FD` |
| `role_icon_darkmage` | `#C6FFD7` |
| `Decor_Title_options_long` | 섹션 제목 데코 |
| 섹션 색상 | `#FAAFA4`, `#9EE0FF`, `#C6FFD7` |

### 탭 포커스
- `shop_tab_focus` — 현재 선택 탭 강조

### Text 색상
- `#FFFFFF` + Outline `#FF0000` (x=0.1, y=-0.1)
- `#00CDFF`
- `#FFFFFF` + Outline `#FFC000` (x=0.2, y=-0.2)
- `#F57A6E`, `#87BBFD`, `#C6FFD7`

### 최하단 버튼 (`1_plus`)
- 능력점수 = 0: 비활성/회색
- 능력점수 > 0: 활성 (1_plus 스프라이트)

## 작업 지시
1. **조사**
   - `UI_CharacterPopup.cs` [능력 상세정보] 버튼 핸들러 유무.
   - prefab Frame_bg_big 색 변경 로직 유무.
   - shop_frame_glow 추가/Mask 적용 유무.
   - role_icon / Decor_Title / shop_tab_focus / 1_plus 리소스 유무.

2. **체크리스트**
   - [ ] [능력 상세정보] 버튼 핸들러가 추가창 Active + Frame 색 변경?
   - [ ] Frame_bg_big → `#FFD000` 토글?
   - [ ] shop_frame_glow + Mask(`#FFBA00` alpha 160) 추가?
   - [ ] role_icon 3종 바인딩 + 색상?
   - [ ] Decor_Title_options_long 배치?
   - [ ] Text 색상 규칙 적용?
   - [ ] 1_plus 버튼 조건부 활성?
   - [ ] shop_tab_focus 적용?

3. **누락분만 구현**
   - Frame Color 변경은 prefab `DOTweenAnimation` Color 트윈으로.
   - `ShowAbilityDetailPanel(bool on)` 함수 신설/유지.
   - Text 색상은 TMP material 또는 런타임 color.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/prefab 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
- `Assets/@Resources/Sprite/` (role_icon_assassin/gladiator/darkmage, Decor_Title_options_long, shop_tab_focus, 1_plus, Frame_bg_big, shop_frame_glow)
