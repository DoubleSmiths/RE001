# 04. Potion Popup

## 범위

- 제작의뢰 해금 규칙
- 포션 메인 팝업 상태
- 포션 상세 팝업 상태
- 제작 성공/실패 규칙
- 자동물약 저장과 사용 규칙

## 기준 자료

- PPT slide 34, 35, 36, 37, 38, 39, 40, 41, 42
- `Documents/기획안/설명/임시엑셀/임시엑셀_권장구조_정리.md`

## 핵심 코드

- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs`

## 현재 충돌 의심 지점

- 메인 포션 팝업은 상태를 하드코딩하고, 상세 팝업은 실제 데이터 기준으로 동작한다.
- 자동포션은 PlayerPrefs UI 상태만 있고 실제 사용 로직은 비어 있다.
- 속도 포션은 UI만 있고 데이터/사용/제작 경로가 없다.
- `OnHPPotionChanged`를 듣는 UI가 적다.

## 이번 기능에서 결정할 것

1. 메인/상세 팝업 단일 상태 소스
2. 회복물약과 속도물약 해금 규칙
3. 기본/설명/다음단계/제작중 상태 정의
4. 자동포션 저장과 트리거 정책
5. 제작 성공/실패 피드백 규칙

## Spec Writer Input

```text
기능: Potion Popup
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 34~42)
보조 문서:
- Documents/기획안/설명/임시엑셀/임시엑셀_권장구조_정리.md
현재 코드 축:
- Assets/@Scripts/UI/Popup/UI_PotionPopup.cs
- Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs
- Assets/@Scripts/Managers/Contents/GameManager.cs

이번 작업은 구현이 아니라 정책 고정이다.
아래를 반드시 정리해라.
1. 물약 해금 규칙
2. 메인 팝업 상태 정의
3. 상세 팝업 상태 정의
4. 제작 성공/실패/재료 부족 규칙
5. 자동물약 저장 여부와 UI 갱신 규칙
6. 예외 상황
7. 구현 체크리스트
```

## Implement Reviewer Input

```text
기능: Potion Popup
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 34~42)
현재 코드 축:
- Assets/@Scripts/UI/Popup/UI_PotionPopup.cs
- Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs
- Assets/@Scripts/Managers/Contents/GameManager.cs

현재 코드 기준으로만 답해라.
아래를 반드시 정리해라.
1. 메인/상세 팝업 상태 소스 통합 구조
2. 제작과 재료 차감 핵심 함수 목록
3. 자동포션 상태 관리 구조
4. 현재 하드코딩/스텁 위험
5. 이벤트 동기화 포인트
6. 테스트 항목
```
