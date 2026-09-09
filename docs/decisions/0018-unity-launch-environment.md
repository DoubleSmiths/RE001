# 0018. Unity 실행 시 Windows 공용 경로 보완

- 상태: 채택
- 날짜: 2026-09-10

## 맥락

Unity 6000.3.10f1의 Package Manager가 목록·설정 요청을 모두 500으로 실패하며 `The "path" argument must be of type string. Received undefined`를 출력했다. 실행 중 Unity/UPM 프로세스에서 ALLUSERSPROFILE이 누락됐고, 설치된 UPM app.js의 getGlobalConfigRoot가 이 환경 변수로 path.join을 호출함을 확인했다. manifest나 프로젝트 경로 오류가 아니었다.

## 결정

`_tools/open_unity.ps1`은 ALLUSERSPROFILE이 비어 있을 때만 Windows CommonApplicationData 폴더를 조회·검증해 프로세스 환경에 지정한다. 시스템/사용자 환경 변수, 패키지 버전, Unity 설치 파일은 변경하지 않는다.

## 결과

해당 자동화 환경에서 Unity를 시작할 때 런처를 사용한다. 이미 실행 중인 UPM은 기존 환경을 갖고 있으므로 정상 재시작이 필요하다. 2026-09-10 재실행 후 목록·설정 조회가 200으로 바뀌었고 컴파일 오류가 없었다. 일반 Windows 환경에서 기본 변수가 존재하면 기존 값을 그대로 사용한다.
