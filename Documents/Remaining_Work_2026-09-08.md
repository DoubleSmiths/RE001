# 잔여 작업 실행 기록 — 2026-09-08

사용자 요청: Hook failed 원인 해결, 아래 우선순위대로 순차 수행.
기존 사용자 변경·세이브를 보존한다. 완료와 미검증을 구분한다.

- [x] 0. 훅 오류: 기존 SessionStart JSON 수정 유지 확인, 회귀 검사 통과(새 오류 발생 시 재진단)
- [x] 1. 정상 성장: 새 게임 → 장비 제작·강화 → 포획·동행 → 능력 습득 → 다음 지역
- [ ] 2. Android: 빌드 → 설치·실행 → 터치·복귀·저장·전투 검증
- [x] 3. 밸런스: 성장·제작·강화·물약 경제 측정 후 필요한 수치 수정
- [x] 4. 여정·직업: 현재 구현과 원안 차이 확정, 결정 기록 및 반영
- [x] 5. UI·한영: 표시 문자열·실제 조작 경로 정리, 회귀 검증

## 시작 상태

- 기준 HEAD: f175e52b. 기존 폰트·Addressables·ProjectSettings 등의 변경은 사용자 변경으로 보존.
- Unity 6000.3.10f1, 에디터 PID 54176, GameScene, 편집 모드.
- 기존 FullSystemAudit는 세이브에 값을 직접 기록하므로 사용자 세이브에서 실행하지 않는다.
- 기존 FullPlaythroughTest는 레벨·재료를 직접 설정하므로 정상 성장 검증을 대신하지 않는다.

## 증거·진행

- 훅: `_tools/repair_codex_session_start_json.py`는 Already repaired 및 성공·실패 응답 검사 통과.
  Ponytail `tests/hooks.test.js`도 hook compatibility checks passed. 별도 사용자 세션에서 훅 수정을 진행 중이므로 중복 수정하지 않음.
- 사용자 세이브와 PlayerPrefs는 `Documents/Verification-20260908/SaveData.original.json`, `PlayerPrefs.original.reg`에 백업.
  세이브 SHA256 일치 확인 후 임시 초기 세이브로 교체했다. **작업 종료 전 반드시 원본 복원**.
- 초기 정상 상태: Lv1 / Gold0 / Quest1 / AllUnlocked=false.
- 실제 길드 UI 대화 → 여정·가방 해금 → 첫 던전 21010 입장 → 실제 4처치로 Lv2, 젤리 퐁 획득, Quest4 도달.
- 협회 보고를 실제 UI로 완료하고 Quest6으로 진행. 발견: AdventurerRank=0 유지.
  회귀 검사에서 `completed reports=1, saved rank=0` 실패를 재현했다.
- 승급 수정: 완료한 협회 보고 수로 저장 등급을 복구하고 HUD에 이벤트 통지. ADR0009.
  `editor refresh --compile` 완료 → console error 0 → 플레이 재진입 → `verify_adventurer_rank.cs` PASS(완료보고1/저장1/HUD1).
- `natural_hunt.cs`: 실제 전투를 시간만 10배 가속, 통상 여정 버튼 경로로 해금 지역 이동. 레벨·경험치·골드·재료 직접 지급 없음.
  첫 구간 Lv13 / 75처치 / Gold921 확인. 스크립트 재컴파일 후 이어서 진행 중.
- Android adb devices 결과 연결 기기 0대. 사용자에게 USB 디버깅 기기 연결 요청함.
- 도구 주의: unity-cli C#의 PowerShell 인용부호 손실 방지를 위해 `_tools/unity_exec.py` 사용.
  코루틴 실행은 CLI의 `--allow-async` 명시 필요. 드물게 heartbeat 등록이 일시적으로 사라지므로 status 확인 후 재시도.
- 검증용 dynamic coroutine의 버튼 대기 부족으로 `Sequence contains no matching element` 한 번 발생(게임 코드 오류 아님).
  이후 버튼 출현 대기 방식의 `natural_report.cs`로 교체하여 Quest6 도달 확인.
- 실제 전투로 Lv31/219처치/Gold5248 도달. 이후 첫 지역 재료 사냥으로 Lv32.
- 고블린2003 포획 성공(CaughtCount1), 기록창을 통한 동행 설정 및 CompanionController 생성 확인.
  결과창은 의도적으로 동행 버튼을 숨기므로 검증 스크립트의 시간 초과를 수정했다.
- 보고7→9→11 완료, Quest13/Rank4 확인. 의뢰의 ITEM_REMOVE가 해당 재료 전량을 소모하는 현상은 경제 단계에서 검토.
- 발견/수정: UI_Village1BtnGroup의 IsUnlockAnimTarget에 대장간/상점/수도원이 빠져,
  저장 해금 이후에도 같은 세션에서 아이콘이 숨겨짐. 공통 필터에 3종 추가, 회귀 검사 작성.
  앞선 상점/제작 자동화 실패는 이 숨겨진 버튼과 재료 부족 때문이며 성공으로 세지 않는다.
- 수정 후 시설 표시 회귀 PASS/컴파일 오류0. 커밋61241db3.
- 상점 실제 구매12개(1440G) → 청동 검30007 제작(800G 및 재료), 장착 확인.
  강화 재료10개 추가 구매(1200G) → 실제 +1 성공, 주문서12→11, 강화120G 차감. 최종 Gold4462.
- 수도원 실제 축성 클릭 → Class능력5004 습득, 숙련도100→0. 앞서 실제 여정으로 여러 상위 지역 진입도 확인.
  입력은 UI 이벤트 전달이며 실제 화면 좌표·터치는 Android 단계에 남는다. 시간가속 외 진행도 지급은 하지 않음.
- 단계2: 빌드 GameScene의 Player/HUD/Combo 연결과 방치 보상 누락 수정 중. TitleScene에서 로드 후 Start에서 공통 연결.
- 단계2: 초기 에디터 종료 지연으로 한 차례 프로젝트 열기 실패 후 정상 재시도. APK 생성 성공(162,133,439 bytes).
  연결 기기0대이므로 설치/터치/백그라운드 복귀 검증은 미완료. 후속 수정 후 APK 재빌드 예정.
- 단계3: 의뢰 초과재료 보존 회귀 PASS(10→7,3→0,증명서). 신속물약40001→40002 제작 시 정상젤리10개 소비 PASS.
  8개 물약/155여정노드/148레벨조건/60레시피 정합성 검사 통과. Progression_Balance 문서 참조.
- 단계4: ADR0011에 현재 레벨 게이트와 Traveler+직업능력 축성 규칙 확정. 이전 QUEST_CLEAR 설명을 대체.
- 단계5: 기존 번역 1,308행을 보존하고 제작·강화·장비 상세 표시용 28키 추가.
  프리팹 고정 한국어와 중복 한영 제목을 정리하고 제작 버튼 폭·재료 스크롤 높이를 보완.
  한영 전환 실제 캡처와 `verify_localization.cs`(장비/대장간/강화/직업) PASS.
- 최종 에디터 확인: 컴파일 완료, 플레이 검사 후 console error `[]`.
  Player/AbilityRunner/Combo/HUD 연결, 의뢰 제출량, 신속 물약 제작 검사 모두 PASS.
- 독립 리뷰: 런타임 연결/의뢰 제출 경로 결함 없음. 신속 물약 xlsx는 K4:K7만 변경,
  기존 번역 셀 보존과 추가 키의 한영 포맷 인자 일치를 확인.
- 커밋: cafc899b(승급), 61241db3(시설), f508fc82(빌드 초기화), 0c7ddffc(의뢰·물약), c067f5e0(UI).
- Android 중간 재빌드: Succeeded, 2분28초, 배치 종료 코드0. APK 162,145,591 bytes.
  ZIP CRC 검사와 ARM64 libil2cpp 포함 확인. adb 연결 기기0대 유지.
  SHA256 `00921138a40e8542dbc0da7f9faddcb5fae8244bb9520e978cbc390a92074644`.
- 원본 SaveData 복원 완료. SHA256 `cb727d514d88abbac1635e9a15eb67215376606120a11c2b4bb066182ee1fc38` 일치.
  PlayerPrefs 11개 값도 원본과 정확히 일치 확인. 검증 후 세이브·설정은 별도 백업으로 보존.
- 추가 훅 수리: Git Bash 절대 경로·PowerShell 호출 연산자·LF·Unix PATH·진단 stderr 교정.
  보안 후크의 비호환 metrics/rewakeSummary만 제거하고 차단 피드백은 보존.
  22개 후크가 exit0/빈 stderr/단일 JSON 또는 빈 stdout/timeout 내 완료, 캐시 원복0 확인.
  원본 옆 백업7개 보존. SDK 설치와 조건부 네트워크 보안 리뷰는 이 격리 검사에서 제외.
  재발 시 `python _tools/verify_codex_plugin_hooks.py`로 감지하고
  `python _tools/repair_codex_plugin_hooks.py --apply`로 백업 후 복구한다.
  이전 수정의 원복은 확인했지만 원복 주체는 로그로 특정하지 못했다.
- 추가 빌드 오류 해결: 미참조 Assets/Test/UI.prefab의 사라진 서브그래프378개가
  Visual Scripting 링커 스캔을 깨뜨렸다. 원본 prefab/meta는 Documents/Legacy에 바이트 그대로 보존.
  ADR0012 및 `verify_legacy_ui_archive.py` PASS. 실제 게임 VS 컴포넌트는 유지.
- 최종 배포용 APK: `Builds/Android/RE001.apk`, 162,404,559 bytes.
  `android-clean-build.log`: Succeeded, 3분18초, **오류0 / 경고21**, 종료 코드0.
  link.xml 정상 생성, APK CRC·ARM64 검사 PASS.
  SHA256 `44325f87fd0540fd40753bda53dc3cd32b23de75396578d75b44d58a505bfa8a`.
  추가 빌드 종료 후 원본 세이브와 PlayerPrefs 11개 일치 재확인.

## 남은 외부 검증

Android 실기기가 연결되지 않아 설치·터치·백그라운드 복귀·기기 저장 복원은 검증하지 못했다.
후반 장기 체감 밸런스도 실기기 플레이 데이터를 더 모은 뒤 판단한다.
에디터는 최종 배치 빌드 종료 상태이며 사용자 세이브로 플레이를 다시 실행하지 않았다.
