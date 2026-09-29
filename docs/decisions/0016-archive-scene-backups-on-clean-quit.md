# 0016. 정상 종료 확정 시 씬 백업을 보존 이동한다

- 상태: 채택
- 날짜: 2026-09-10
- 보완: ADR0015

## 재검증 결과

플레이하지 않은 에디터를 창 닫기로 종료했을 때와 달리, 플레이 후 정상 종료에서는 `Temp/__Backupscenes/0.backup`이 남았다. 추가 옵션 없이 다시 실행해 `Recovering Scene Backups` 대화상자를 직접 확인했다. 따라서 ADR0015의 창 닫기만으로 충분하다는 판단은 철회한다. DX11 설정과 별개인 백업 수명 문제다.

## 결정

`SceneBackupArchive`가 `EditorApplication.quitting`에 등록하여 정상 종료 확정 시 남은 백업 디렉터리를 프로젝트 안의 `Documents/UnityRecovery/<GUID>/`로 옮긴다. 파일 삭제나 원래 씬 덮어쓰기는 하지 않는다. 경로가 프로젝트 안인지와 경로상의 재분석 지점을 검사하며, 실패 시 백업을 원위치에 남기고 경고한다.

종료 취소 및 크래시의 복구 자료를 건드리지 않기 위해 `wantsToQuit`나 에디터 시작 훅은 사용하지 않는다. `quitting`은 종료 취소가 불가능한 단계에 발생하며 강제 종료/크래시에는 발생하지 않는다([Unity 공식 문서](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/EditorApplication-quitting.html)). 기존 실행기는 이미 남은 백업을 처리하는 보완 경로로 유지한다.

## 검증 기준

컴파일 성공과 플레이 후 실제 창 종료에서 보관 로그/백업 내용 보존을 확인하고, 실행기나 강제 API 옵션 없이 다시 열어 복구 대화상자 및 Recovery 씬 생성이 없는지 확인한다. 세이브와 PlayerPrefs는 검사 전 상태로 복원한다.

2026-09-10 실측 PASS: 종료 후 원래 백업 경로 없음, 보관본 SHA256 일치, 일반 재시작 `GameScene`/DX11/편집 모드 확인. 수정 전 같은 순서에서는 복구 대화상자가 재현됐다.
