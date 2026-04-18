# RE001 Phase 1 Dual-AI Runbook

## 목적

기획서 기준으로 현재 구현 상태를 검증하고, 누락 정책과 수정 범위를 기능 단위로 고정한다.

## 기준 문서

- 원본 기준: `Documents/기획안/기획서v2_04.16.pptx`
- 보조 해석: `Documents/기획안/RE001_기획문서_정리.md`
- 보조 해석: `Documents/Docs/GameAnalysis.md`
- 데이터 규약: `Documents/기획안/설명/임시엑셀/임시엑셀_권장구조_정리.md`
- Travel 규약: `Documents/기획안/설명/임시엑셀/TravelData_계층형_재설계_설명.md`

## 역할

### 총괄 AI
- 기능을 하나씩 선택한다.
- 분석 AI와 구현 AI에 줄 입력문을 만든다.
- 두 결과를 합쳐 구현 가능한 결정안으로 정리한다.

### 분석 AI
- 기획 누락 정책을 고정한다.
- 상태, 예외, 저장 여부, UI 갱신 규칙을 정리한다.
- 구현 구조 제안은 하지 않는다.

### 구현 AI
- 현재 코드 기준으로 수정 범위를 정리한다.
- 클래스 구조, 상태 흐름, 위험 포인트, 테스트 항목을 정리한다.
- 새 기획을 만들지 않는다.

## 작업 순서

1. `spec/`의 해당 기능 브리프를 연다.
2. `Spec Writer Input`을 분석 AI에 보낸다.
3. `Implement Reviewer Input`을 구현 AI에 보낸다.
4. 총괄 AI가 아래 형식으로 통합한다.

통합 형식:
- 확정된 정책
- 현재 코드 충돌 지점
- 수정 대상 클래스
- 구현 전 확인 질문
- 완료 기준

## 1차 기능 순서

1. Popup 공통 규칙
2. Quest System
3. Inventory / Item Detail
4. Potion Popup
5. Travel Popup

## 고정 체크 포인트

- 저장/복원:
  `CurrentQuestID`, `UnlockedBtnIds`, `CurrentHPPotionDataId`, `CurrentTravelAreaId`, `InventoryItem.IsNew`
- 실시간 UI 갱신:
  `OnNotifyChanged`, `OnFollowUIChanged`, `OnBtnUnlocked`, `OnItemChanged`, `OnInventoryNewChanged`, `OnLanguageChanged`, `OnHPPotionChanged`
- 회귀 위험:
  중복 팝업, 빠른 연타, 잠금 상태 클릭, 빈 슬롯 클릭, 데이터 오타, 프리팹 누락

## 현재 코드에서 먼저 주의할 점

- 워크트리가 깨끗하지 않다.
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`

구현 단계에서는 기존 미커밋 변경을 덮어쓰지 않는다.
