# 1차 기능 사이클

기준 문서:
- `Documents/기획안/기획서v2_04.16.pptx`

보조 문서:
- `Documents/기획안/RE001_기획문서_정리.md`
- `Documents/Docs/GameAnalysis.md`
- `Documents/기획안/설명/임시엑셀/임시엑셀_권장구조_정리.md`
- `Documents/기획안/설명/임시엑셀/TravelData_계층형_재설계_설명.md`

작업 순서:
1. Popup 공통 규칙
2. Quest System
3. Inventory / Item Detail
4. Potion Popup
5. Travel Popup

기능별 핵심 코드 축:
- Popup 공통: `Assets/@Scripts/UI/Popup/UI_Popup.cs`, `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- Quest: `Assets/@Scripts/Managers/Contents/QuestManager.cs`, `Assets/@Scripts/Managers/Contents/GameManager.cs`
- Inventory / Item Detail: `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`, `Assets/@Scripts/UI/Popup/UI_ItemPopup.cs`
- Potion: `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`, `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- Travel: `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`, `Assets/@Scripts/UI/Popup/UI_MapSelectPopup.cs`, `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`

이번 사이클의 원칙:
- 기능 하나씩 끝낸다.
- 분석 AI는 정책만 고정한다.
- 구현 AI는 현재 코드 기준으로만 번역한다.
- 총괄 AI가 미결정 이슈를 없앤 뒤 구현 단계로 넘긴다.
