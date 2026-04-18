# 01. Popup 공통 규칙

## 범위

- 공통 열기/닫기 애니메이션
- 배경 클릭 닫기
- 버튼 클릭 애니메이션
- 중복 오픈/중복 닫기 방지
- 이벤트 구독/해제 규칙

## 기준 자료

- PPT slide 8, 10
- `Documents/기획안/RE001_기획문서_정리.md`

## 핵심 코드

- `Assets/@Scripts/UI/Popup/UI_Popup.cs`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`

## 현재 충돌 의심 지점

- 팝업마다 `_openFromY`, `_closeToY`, 직접 `DOAnchorPos` 방식이 혼재한다.
- `Init()`에서 이벤트를 구독하고 `OnDestroy()`에서 해제하는 패턴이 제각각이다.
- `SetInfo()`에서 상태를 초기화하는 팝업과 현재 상태를 유지해야 하는 팝업이 섞여 있다.

## 이번 기능에서 결정할 것

1. 공통 오픈/닫기 규칙과 예외 허용 범위
2. 배경 클릭 닫기 금지 팝업 조건
3. 버튼 클릭 애니메이션 공통 적용 위치
4. 이벤트 구독/해제 표준 패턴
5. 중복 오픈/빠른 연타 방지 정책

## Spec Writer Input

```text
기능: Popup 공통 규칙
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 8, 10)
보조 문서: Documents/기획안/RE001_기획문서_정리.md
현재 코드 축:
- Assets/@Scripts/UI/Popup/UI_Popup.cs
- Assets/@Scripts/UI/Scene/UI_GameScene.cs

이번 작업은 구현이 아니라 정책 고정이다.
아래를 반드시 정리해라.
1. 공통 열기/닫기 애니메이션 규칙
2. 배경 마스크와 배경 클릭 닫기 규칙
3. 버튼 클릭 애니메이션 공통 규칙
4. 팝업 상태 정의와 중복 오픈/중복 닫기 예외
5. 이벤트 구독/해제 규칙
6. 저장이 필요한 팝업 상태와 저장이 필요 없는 팝업 상태
7. UI 즉시 갱신 조건
8. 구현 체크리스트
```

## Implement Reviewer Input

```text
기능: Popup 공통 규칙
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 8, 10)
현재 코드 축:
- Assets/@Scripts/UI/Popup/UI_Popup.cs
- Assets/@Scripts/UI/Scene/UI_GameScene.cs

현재 코드 기준으로만 답해라.
아래를 반드시 정리해라.
1. 공통 base에서 책임져야 할 범위
2. 개별 popup이 override해야 할 범위
3. 중복 오픈/빠른 연타/중복 이벤트 구독 위험
4. 상태 enum 또는 플래그 구조
5. 핵심 함수 분리안
6. 회귀 테스트 항목
```
