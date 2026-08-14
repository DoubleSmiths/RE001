# 0001. 게임 데이터의 단일 원천은 로컬 엑셀이다

- 상태: 채택
- 날짜: 2026-08-14

## 맥락

데이터 파이프라인에 구글 시트 경로와 로컬 xlsx 경로가 함께 있었다.
`DataTransformer.s_useGoogleSheets = false`로 구글 경로는 이미 꺼져 있었지만,
`GS_Parse*` 7종·`GSHEET_*` 상수 15개·전용 메뉴 2개가 코드에 남아 있었고
`Tools/GenerateLocalizeKeys`는 **여전히 구글 시트를 다운로드**하고 있었다.

죽은 경로가 남아 있으면 누군가 메뉴를 잘못 눌렀을 때 최신 로컬 데이터가
동기화되지 않은 옛 시트로 덮어써진다. 실제로 로컬 파서만 최신화된 상태였다.

## 결정

`Assets/@Resources/Data/Excel/*.xlsx`가 **유일한 원천**이다.
구글 시트 경로는 코드에서 완전히 제거했다(약 500줄).
`GenerateLocalizeKeys`도 로컬 `LocalizationData.xlsx`를 읽는다.

## 결과

- 데이터를 고치려면 xlsx를 고치고 `Tools/ParseAllLocal`을 실행한다. **게임은 JSON만 읽는다.**
- 대량 생성·수정은 `_tools/gen_*.py` 생성기를 남겨 재현 가능하게 한다.
  손으로 수백 행을 넣지 않는다.
- 새 JSON을 추가하면 Addressable 등록이 필요하다. 누락 시 `[DataManager] JSON load 실패` 로그.
- 구글 시트를 되살리려면 먼저 시트를 로컬 xlsx와 동기화해야 한다. 안 하면 데이터가 퇴행한다.
