# 02. Quest System

## 범위

- Quest / Condition / Notify / Unlock 데이터 계약
- 퀘스트 진행 상태 흐름
- QuestFollow UI 갱신
- 해금과 느낌표 전파
- 보상 지급과 저장

## 기준 자료

- PPT slide 22, 23, 24
- `Documents/기획안/RE001_기획문서_정리.md`
- `Documents/Docs/GameAnalysis.md`
- `Documents/기획안/설명/임시엑셀/임시엑셀_권장구조_정리.md`

## 핵심 코드

- `Assets/@Scripts/Managers/Contents/QuestManager.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`
- `Assets/@Scripts/UI/Popup/UI_TravelPopup.cs`
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`

## 현재 충돌 의심 지점

- `MULTI` 조건은 충족 누적만 있고 취소/역행 처리가 없다.
- `ITEM_COUNT`는 직전 퀘스트 기준 부족 시 롤백한다.
- `ITEM_REWARD`가 즉시 `UI_ItemPopup`을 띄워 다중 팝업 스팸 위험이 있다.
- 버튼 해금은 저장과 연출이 분리되어 있다.

## 이번 기능에서 결정할 것

1. 퀘스트 상태 모델
2. `MULTI` 누적과 롤백 정책
3. 보상 지급 시점과 팝업 규칙
4. 해금 저장과 연출의 관계
5. QuestFollow 표시/숨김/색상 규칙

## Spec Writer Input

```text
기능: Quest System
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 22, 23, 24)
보조 문서:
- Documents/기획안/RE001_기획문서_정리.md
- Documents/Docs/GameAnalysis.md
- Documents/기획안/설명/임시엑셀/임시엑셀_권장구조_정리.md
현재 코드 축:
- Assets/@Scripts/Managers/Contents/QuestManager.cs
- Assets/@Scripts/Managers/Contents/GameManager.cs
- Assets/@Scripts/Data/Data.Contents.cs

이번 작업은 구현이 아니라 정책 고정이다.
아래를 반드시 정리해라.
1. Quest / Condition / Notify / Unlock 데이터 계약
2. 퀘스트 상태 정의
3. 완료, 역행, 보상, 해금 정책
4. 저장 여부
5. QuestFollow UI 갱신 규칙
6. 느낌표와 버튼 해금 전파 규칙
7. 예외 상황
8. 구현 체크리스트
```

## Implement Reviewer Input

```text
기능: Quest System
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 22, 23, 24)
현재 코드 축:
- Assets/@Scripts/Managers/Contents/QuestManager.cs
- Assets/@Scripts/Managers/Contents/GameManager.cs
- Assets/@Scripts/Data/Data.Contents.cs
- Assets/@Scripts/UI/Scene/UI_GameScene.cs
- Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs

현재 코드 기준으로만 답해라.
아래를 반드시 정리해라.
1. 추천 클래스 책임 분리
2. Quest 상태 enum 또는 단계 모델
3. 완료/보상/해금/롤백 핵심 함수 목록
4. 현재 버그 가능성
5. 이벤트 흐름 충돌 지점
6. 테스트 항목
```
