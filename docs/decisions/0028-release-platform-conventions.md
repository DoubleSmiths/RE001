# 0028. 출시 플랫폼 규약 — 식별자·화면·뒤로가기·빌드

- 상태: 채택
- 날짜: 2026-09-29

## 맥락

스팀(Windows)·구글 플레이(Android) 동시 출시를 앞두고 감사한 결과, 플레이어 설정이 템플릿 기본값
(`DefaultCompany`/`RE001`/`com.DefaultCompany.2D-URP`, 자동회전, 1920x1080 전체화면, targetSdk 자동)이었고
뒤로가기/ESC·종료 경로가 없었다. 화면비 고정(`FixedResolution`)은 GameScene 에만 붙어 있었고 `Start` 에서
한 번만 계산했으며, URP 가 `OnPreCull` 을 부르지 않아 여백이 지워지지 않았다. 구글 플레이는 2026-08-31 부터
신규 앱·업데이트에 targetSdk 36 을 요구한다.

## 결정

- **식별자(임시, 사용자 확정 전)**: 회사 `DoubleSmiths`(GitHub 조직명), 제품 `Minimon Quest`, 패키지 `com.doublesmiths.minimonquest`,
  버전 `1.0.0`(코드 1). 한국어 기기 앱 이름은 `Plugins/Android/LocalizedAppName.androidlib` 의 `values-ko`.
- **화면**: 세로 고정. 모든 씬의 메인 카메라에 `FixedResolution` 을 자동 부착(1440x3040 비율), 화면 크기가 바뀌면
  다시 계산, 여백은 `@LetterboxCamera`(cullingMask 0, 검정)가 지운다. 모든 UI 캔버스는 Screen Space - Camera.
  안드로이드는 노치 영역 밖에만 렌더(`renderOutsideSafeArea` 끔)해 안전 영역 코드를 두지 않는다.
- **PC**: 창 모드 기본, 크기 조절 가능, 단일 인스턴스. 첫 실행에 모니터 높이 85% 세로 창. 설정의 '전체 화면'은
  테두리 없는 전체화면(좌우 여백). Alt+Enter 도 동작.
- **뒤로가기/ESC**(`UIManager.OnBackPressed`): 맨 위 팝업은 그 팝업의 X·나가기·바깥 배경 버튼 핸들러를 그대로 누른다
  (`UI_Popup.OnBackPressed`, 노드 이름 규칙: `xbutton/exit/close` > `…Background`). 팝업이 없으면 화면의
  `…ExitButton`(건물 나가기) → `ReturnButton`(전투 귀환, 2단계 확인 내장). 최상위에서 안드로이드는 2초 안에 두 번
  누르면 종료, PC 는 설정창(종료 버튼 포함).
- **공통 런타임**: 60fps, 화면 꺼짐 방지, 릴리스 빌드는 `Debug.Log` 출력 억제(경고·에러는 유지), 언어 기본값은 기기 언어.
- **빌드**: targetSdk 36 고정. 구글 플레이는 AAB(`BuildScript.AndroidBundle`), 업로드 키는 환경변수로만 주입하고
  빌드 후 ProjectSettings 에서 경로를 되돌린다. 산출물 이름 `MinimonQuest.*`.

## 결과

- 식별자를 바꾸면 PC 세이브·PlayerPrefs 경로(`LocalLow/<회사>/<제품>`)가 바뀐다 — **출시 후에는 바꾸지 않는다.**
  구글 플레이 패키지명은 출시 후 변경 불가.
- 새 팝업은 닫기 노드 이름을 규칙(`Xbutton`/`…Exit…`/`…Close…`/`…Background`)대로 지어야 뒤로가기가 동작한다.
  닫으면 안 되는 연출 팝업은 그런 이름의 클릭 노드를 두지 않는다.
- 새 씬은 메인 카메라에 `MainCamera` 태그만 있으면 된다. 카메라 rect 를 직접 바꾸는 코드를 추가하지 않는다.
- 새 UI 캔버스를 Overlay 로 만들면 여백 위로 넘친다 — Screen Space - Camera + `Camera.main`.
