# 03. Inventory / Item Detail

## 범위

- 인벤토리 슬롯 상태
- 일반 아이템과 귀중품 분리
- 정렬 규칙
- `NEW` 배지 정책
- 아이템 상세 팝업 표기 항목

## 기준 자료

- PPT slide 17, 18, 19, 20, 21
- `Documents/기획안/RE001_기획문서_정리.md`
- `Documents/Docs/GameAnalysis.md`
- `Documents/기획안/설명/임시엑셀/임시엑셀_권장구조_정리.md`

## 핵심 코드

- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_ItemPopup.cs`
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`
- `Assets/@Scripts/UI/SubItem/UI_ImportantSlot.cs`
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs`

## 현재 충돌 의심 지점

- 아이템 분류가 ID 범위 하드코딩이다.
- 정렬이 원본 인벤토리 리스트를 직접 변경한다.
- `NEW`는 슬롯 클릭으로만 내려간다.
- 빈 귀중품 슬롯 클릭 방어가 약하다.

## 이번 기능에서 결정할 것

1. 슬롯 상태와 초기 오픈 칸
2. 일반/귀중품 분리 기준
3. 정렬이 전역 상태를 바꿔도 되는지
4. `NEW` 해제 조건
5. 상세 팝업 표기 항목과 사용 버튼 규칙

## Spec Writer Input

```text
기능: Inventory / Item Detail
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 17, 18, 19, 20, 21)
보조 문서:
- Documents/기획안/RE001_기획문서_정리.md
- Documents/Docs/GameAnalysis.md
- Documents/기획안/설명/임시엑셀/임시엑셀_권장구조_정리.md
현재 코드 축:
- Assets/@Scripts/Managers/Contents/GameManager.cs
- Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs
- Assets/@Scripts/UI/Popup/UI_ItemPopup.cs

이번 작업은 구현이 아니라 정책 고정이다.
아래를 반드시 정리해라.
1. 슬롯 상태 정의
2. 일반 아이템과 귀중품 분리 규칙
3. 정렬 규칙
4. NEW 표시/해제 규칙
5. 아이템 상세 팝업 표기 항목
6. 저장 여부
7. UI 갱신 규칙
8. 구현 체크리스트
```

## Implement Reviewer Input

```text
기능: Inventory / Item Detail
기준 문서: Documents/기획안/기획서v2_04.16.pptx (slide 17, 18, 19, 20, 21)
현재 코드 축:
- Assets/@Scripts/Managers/Contents/GameManager.cs
- Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs
- Assets/@Scripts/UI/Popup/UI_ItemPopup.cs
- Assets/@Scripts/UI/SubItem/UI_InvSlot.cs
- Assets/@Scripts/UI/SubItem/UI_ImportantSlot.cs
- Assets/@Scripts/UI/Scene/UI_GameScene.cs

현재 코드 기준으로만 답해라.
아래를 반드시 정리해라.
1. 인벤토리 상태와 데이터 흐름
2. NEW 배지와 이벤트 흐름
3. ID 범위 하드코딩 위험
4. 정렬과 상세 팝업의 충돌 가능성
5. 빈 슬롯 클릭 예외 처리
6. 테스트 항목
```
