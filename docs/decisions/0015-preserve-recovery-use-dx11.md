# 0015. 복구 백업을 보존하고 Windows 에디터는 DX11을 사용한다

- 상태: 채택
- 날짜: 2026-09-09

> 2026-09-10 정정: 플레이 후에는 실제 창 닫기에서도 백업이 잔류했다. 종료 방식만으로 해결된다는 판단은 ADR0016으로 대체한다.

## 맥락

사용자가 매번 Recovery로 열리는 문제의 수정을 요청했다. 일반 DX12 실행 후 정상 종료와 재실행에서도 남아 있는 `Temp/__Backupscenes/0.backup`으로부터 새 `Assets/_Recovery/0 (23).unity`가 생성됐다. 이는 게임 C# 컴파일 실패에 따른 Safe Mode와 별개다. 기존 `open_unity.bat`에는 과거 DX12 fence 오류 기록과 DX11 우회가 있었지만 복구 백업을 삭제했고, 일반 실행의 기본 API에는 적용되지 않았다. 현재 재현에서 GPU 크래시 자체까지 다시 확인한 것은 아니다.

## 결정

- Windows PlayerSettings의 자동 그래픽 API를 끄고 Direct3D11을 지정한다. Android API는 유지한다.
- 배치 실행기는 PowerShell 실행기로 위임한다. 프로젝트 버전에 맞는 에디터를 찾아 DX11로 실행한다.
- Unity가 하나라도 실행 중이면 파일에 손대지 않는다. 종료 상태에서 남은 씬 백업은 삭제하지 않고 프로젝트의 무시된 `Documents/UnityRecovery/` 고유 폴더로 옮긴다. 경로 범위와 재분석 지점을 검사한다.
- 기존 `Assets/_Recovery` 사본을 덮어쓰거나 삭제하지 않는다. 필요 시 보관된 백업과 함께 복원 자료로 사용할 수 있다.
- 이번 환경에서는 CLI `File/Exit`와 `EditorApplication.Exit` 후 백업이 남았고, 실제 창을 Alt+F4로 닫으면 백업이 정리됐다. 에이전트 종료 경로는 창 닫기로 통일한다. GPU 문제만이 이번 복구 반복의 원인이라고 단정하지 않는다.

## 검증

실행 중 무변경, 종료 후 백업 보존, DX11 시작, 정식 씬 시작 및 컴파일/플레이 모드를 실측한다. 결과는 `Documents/Gameplay_Recovery_Verification_20260909.md`에 기록한다.

Unity는 Windows 자동 API를 끄면 목록의 첫 API를 에디터에도 사용한다: [공식 그래픽 API 설정 문서](https://docs.unity.cn/Manual/configure-graphicsAPIs.html).
