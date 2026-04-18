# 05. Travel Popup

## 범위

- 3계층 내비게이션 모델
- 현재 위치, 선택 가능, 입장 가능 상태
- 조건 평가
- 데이터 기반 리스트 생성
- 지역 진입과 저장

## 기준 자료

- PPT slide 43, 44, 45, 46
- `Documents/기획안/설명/임시엑셀/TravelData_계층형_재설계_설명.md`

## 핵심 코드

- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_MapSelectPopup.cs`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`

## 현재 충돌 의심 지점

- 조건 타입이 제한적이다.
- `QUEST_CLEAR`가 `CurrentQuestID > questId` 의미로 구현되어 있다.
- `SceneName`이 `AreaType`과 문자열 일치해야만 진입 가능하다.
- 실제 엔트리 이름은 `UI_MapSelectPopup`, 실제 로직은 `UI_TravelPopup`이다.

## 이번 기능에서 결정할 것

1. 대분류/중분류/소분류 상태 모델
2. 선택 가능과 입장 가능의 차이
3. 조건 타입 확장 여부
4. `QUEST_CLEAR` 의미
5. `SceneName <-> AreaType` 실패 처리
6. 명칭 유지 또는 통일 여부

## Spec Writer Input

```text
기능: Travel Popup
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 43~46)
보조 문서:
- Documents/기획안/설명/임시엑셀/TravelData_계층형_재설계_설명.md
현재 코드 축:
- Assets/@Scripts/UI/Popup/UI_TravelPopup.cs
- Assets/@Scripts/UI/Popup/UI_MapSelectPopup.cs
- Assets/@Scripts/UI/Scene/UI_GameScene.cs
- Assets/@Scripts/Managers/Contents/GameManager.cs

이번 작업은 구현이 아니라 정책 고정이다.
아래를 반드시 정리해라.
1. 3계층 이동 상태 정의
2. 현재 위치, 선택 가능, 입장 가능 상태 정의
3. 조건 평가 규칙
4. 저장 여부
5. UI 갱신 규칙
6. 예외 상황
7. 구현 체크리스트
```

## Implement Reviewer Input

```text
기능: Travel Popup
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 43~46)
현재 코드 축:
- Assets/@Scripts/UI/Popup/UI_TravelPopup.cs
- Assets/@Scripts/UI/Popup/UI_MapSelectPopup.cs
- Assets/@Scripts/UI/Scene/UI_GameScene.cs
- Assets/@Scripts/Managers/Contents/GameManager.cs
- Assets/@Scripts/Data/Data.Contents.cs
- Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs

현재 코드 기준으로만 답해라.
아래를 반드시 정리해라.
1. 3계층 상태 모델
2. 조건 평가 함수 구조
3. SceneName과 AreaType 매핑 위험
4. 선택 복원과 무효 선택 보정 로직
5. 실제 지역 진입 흐름
6. 테스트 항목
```
