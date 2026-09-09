# 포획 보상 및 Recovery 검증 — 2026-09-09

## 2026-09-10 재검증 및 정정

- **복구 반복 재현:** 플레이 → 창 닫기 후에도 백업이 남았으며 일반 재실행에서 `Recovering Scene Backups`와 `Assets/_Recovery/0 (24).unity`가 확인됐다. 아래 이전 기록의 창 닫기만으로 해결된다는 결론은 불완전했다.
- **추가 수정:** `SceneBackupArchive`가 정상 종료 확정 이벤트에서 백업을 별도 GUID 폴더로 보존 이동한다. ADR0016 참조.
- **종료 실측:** 플레이 후 정상 종료 로그에 `[SceneBackupArchive] 정상 종료 백업 보관`이 남았고 `Temp/__Backupscenes/0.backup`은 없어졌다. 보관된 파일의 SHA256은 종료 전 원본과 일치했다.
- **일반 재시작 PASS:** 실행기와 강제 그래픽 API 옵션을 사용하지 않고 실행했다. 복구 안내/Recovery 씬 없이 `Assets/@Scenes/GameScene.unity`, `Direct3D11`, `isPlaying=false`를 확인했다.
- **게임 회귀:** 컴파일 성공, 503/503 데이터 검사, 포획/대여/기억력/쿨타임/영구화/보상 복원/버프 회귀 PASS. 실제 UI 객체로 대여 능력의 기억중 배지 비표시, 점유 0, 공명도 25%, 미보유 180% 효과 미리보기 및 사용 권한 미부여를 추가 검증해 PASS.
- **잔여 사항:** 시작 시 Package Manager `path undefined` 오류는 남아 있다. 팝업을 즉시 닫는 UI 검증에서 삭제된 대상을 참조하는 DOTween 경고가 발생했다. 별도 새 게임 시작→종료 검증에서는 플레이 오류/경고가 없었다. Android 연결 기기가 없어 실기기 검증은 미실시.
- 세이브와 PlayerPrefs를 검사 직전 상태로 복원했다. 상세 증거는 `Documents/Verification-20260909-Gameplay/recheck-20260910/`에 보관한다.

## 변경

- 고유능력 보상 데이터 누락 501종을 보완했다. 전체 503종에 보상을 연결했으며 새 효과는 7개 유형을 공유한다.
- 첫 지역 4종은 미보유 상태에서 종별 3회 처치부터 관심을 보장하고 첫 포획 확률은 100%다. 재포획 규칙은 유지한다.
- 첫 동행은 기억력 점유 없이 고유능력을 빌려 쓰며, 공명도 100%에서 영구 습득한다. 실제 기억 상태와 대여 상태를 UI에서 구분한다.
- 공격/회복은 플레이어 능력치에 비례한다. 미니몬 액티브는 쿨타임을 공유하고, 교체로 초기화되지 않는다. 무관한 습득 알림은 진행 중인 버프를 취소하지 않는다.
- 발견한 마물의 보상 효과를 포획 전에 확인할 수 있다. 결과창에서 바로 동행할 수 있으며 능력 아이콘과 공명도 진행도를 데이터에 맞춘다.

## 실행 검증

- Unity 6000.3.10f1, Windows Editor. `editor refresh --compile` 완료, 마지막 기능 검증 플레이의 오류 콘솔은 `[]`였다.
- 시작 시 별도로 `[Package Manager Window] path argument undefined` 오류가 재현된다. 이번 게임 코드 컴파일 오류와 Recovery 원인은 아니며 패키지 창 오류 자체는 미해결이다.
- `Tools/ParseAllLocal (로컬 전체 파싱)` 후 `verify_minion_rewards.py`의 xlsx/JSON 검사 통과: 503/503 연결, 초반 4종의 효과 다양성 및 첫 포획 설정.
- 기존 SkillData 74행(헤더 포함), LocalizationData 1337행, MinMonData 506행의 모든 원래 셀을 git HEAD와 비교해 보존 확인.
- `audit_progression.py` 통과: 물약 8행, 여정 155노드, 레벨 게이트 148개, 제작식 60개. 기존 성장 수치는 유지했다.
- `_tools/verify_companion_rewards.cs`를 실제 플레이에서 실행해 관심/창고/첫 포획, 대여/기억력, 교체 쿨타임, 4회 포획 영구화, 기존 세이브 누락 보상, 중복 방지, 레벨 스케일링, 버프 유지/해제를 검증했다. PASS.

## 정상 속도 관찰

원래 세이브와 PlayerPrefs를 백업한 후 별도 새 게임으로 실행했다. 치트 해금이나 진행도/재료 주입 없이 실제 UI 이벤트를 사용했고 `Time.timeScale=1`을 확인했다.

- 첫 길드 안내를 마친 후 첫 사냥 지역 진입.
- 사냥 진입 후 13.0초, 2회 처치에서 관심 발생(확률 조기 발생). 3회 보장은 별도 회귀 검증으로 확인했다.
- 어린 슬라임(2101) 첫 포획 성공, 공명도 25%.
- 결과창의 부르기 버튼으로 즉시 동행. 대여 능력 6101 활성, 영구 습득 false, 점유 기억력 0.
- 스킬바 클릭으로 회복 사용: HP 179→180(최대 180에 제한), 쿨타임 18초. 별도 회귀에서는 최대 HP 비례 회복량을 검증했다.
- 플레이 종료/재진입 후 동행 2101과 대여 능력 및 기억력 0 상태 유지.
- 원래 SaveData.json 복원 SHA256: `CB727D514D88ABBAC1635E9A15EB67215376606120A11C2B4BB066182EE1FC38`. PlayerPrefs 백업도 복원했다.

Android ADB 연결 기기는 없었다. 실기기에서 제작·강화·포획·축성·다음 지역을 전부 관통하는 검증과 장기 재미/밸런스 평가는 이번 결과에 포함하지 않는다. 첫 포획과 전투 보상을 연결한 개선이며 전체 게임의 재미를 보장하는 결론은 아니다.

## Recovery

- 수정 전 재실행 로그에 `Assets/_Recovery/0 (23).unity`, `Temp/__Backupscenes/0.backup` 로드가 확인됐다.
- CLI `File/Exit`/`EditorApplication.Exit` 종료 후에도 백업이 남았다. 실제 창 Alt+F4 종료에서는 백업이 정상 정리됐다. 자동화 규칙에 반영했다.
- `open_unity.ps1`은 실행 중이면 무변경 종료하며, 종료 상태의 잔여 백업은 `Documents/UnityRecovery/` 아래 고유 폴더로 보존한다. 기존 Recovery 씬 사본은 유지했다.
- DX11 실행기 경로에서 정식 `GameScene` 로드, Recovery 로드 없음 확인.
- 실행 옵션 없이 일반 실행에서도 `SystemInfo.graphicsDeviceType=Direct3D11`, `Assets/@Scenes/GameScene.unity`, `isPlaying=false`를 확인했다.
- 과거 실행기에 기록된 DX12 GPU 오류를 피하도록 Windows 기본 API를 DX11로 지정했다. 이번에 GPU 크래시 자체를 재현한 것은 아니며, 반복 복구의 원인을 GPU 하나로 단정하지 않는다.

상세 로그·캡처·세이브 백업은 무시된 `Documents/Verification-20260909-Gameplay/`에 보관한다. 결정은 ADR0014·0015를 참조한다.
