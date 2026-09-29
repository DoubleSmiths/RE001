# 0027. 세이브 내구성 — 원자적 기록, 손상 격리, 모아 쓰기

- 상태: 채택
- 날짜: 2026-09-29

## 맥락

출시 준비 감사에서 세이브 경로에 데이터 손실 경로가 다섯 개 있었다.

1. `LoadGame` 이 PlayerPrefs `ISFIRST` 가 없으면 **세이브 파일을 지웠다.** `ISFIRST=0` 은 `PlayerPrefs.Save()` 없이 기록돼 첫 세션에 강제 종료되면 사라지고, PlayerPrefs(윈도우 레지스트리)와 세이브(LocalLow)가 따로 살기 때문에 클라우드로 세이브만 옮긴 PC·레지스트리 정리 후에도 진행이 삭제된다.
2. `File.WriteAllText` 로 본 파일을 직접 덮어썼다. 로드는 예외 처리가 없어 손상 JSON 이면 예외가 새고, 기본값 `_gameData` 가 다음 저장에서 손상본을 **빈 데이터로 덮어썼다.**
3. Newtonsoft 기본 `ObjectCreationHandling.Auto` 는 필드 초기값이 든 리스트에 로드 값을 이어 붙인다. `ActivationManager`(7칸)가 로드마다 7칸씩 늘어 개발 세이브가 56칸이었고 표시 플래그가 유실됐다.
4. `Equipment.EquipmentData`(get-only, 데이터표 행 참조)가 저장되고, 로드 때 그 사본이 **공유 EquipmentDic 객체를 덮어써** 밸런스 수정을 되돌렸다. `InventoryItem.IsNew` 프로퍼티도 필드와 이중 저장돼 로드 중 세터가 돌 수 있었다.
5. 세터마다 즉시 동기 기록 — 처치 1회에 3~6번, 자동물약 슬라이더 드래그 중 매 프레임.

## 결정

- 새 게임 판정은 **세이브 파일 존재 여부만** 본다. `ISFIRST` 는 더 이상 읽지도 쓰지도 않는다.
- 기록: `SaveData.json.tmp` 에 전부 쓰고 **fsync(`Flush(true)`)** 한 뒤 본 파일을 `.bak` 으로 옮기고 임시 파일을 본 파일로 이름 바꾼다. (`File.Replace` 대신 Delete/Move — IL2CPP·.NET Standard 2.1 에서 동작이 확실한 조합. fsync 가 없으면 전원 차단 시 방금 이름 바꾼 본 파일과 직전 .bak 이 모두 빈 파일일 수 있다)
- 읽기: 본 → `.tmp` → `.bak` 순. **파싱** 실패한 본/백업 파일은 지우지 않고 `SaveData.json(.bak).corrupt-yyyyMMdd-HHmmss` 로 격리한다. 파일이 잠겨 **읽기 자체**가 안 되면(백신·클라우드 동기화) 손상이 아니므로 격리하지 않고, 3회 재시도 후에도 안 되면 그 세션은 저장하지 않는다(`StopSavingThisSession`). 저장 실패는 로그만 남기고 다음 주기에 재시도(게임 진행을 끊지 않는다).
- 직렬화 설정 `ObjectCreationHandling.Replace`. 파생 참조 프로퍼티는 `[JsonIgnore]`.
- `SaveGame()` 은 **표시만** 하고 `Managers.Update → TickSave` 가 3초마다 한 번 기록한다. 일시정지·종료(`MarkSeen`)·새 게임 생성은 `SaveGameNow()` 로 즉시 기록.
- `GameData.SaveVersion`(현재 1) 과 `NormalizeSave()` — 옛 세이브의 늘어난 칸 정리, 명시적 null·짧은 고정 배열 보정, 레벨 상한 복구. 여러 번 돌려도 같은 결과여야 한다.
- 언어 기본값은 기기 언어(한국어 기기만 한국어). 설정 변경 시 `PlayerPrefs.Save()`.

## 결과

- 세이브를 초기화하는 도구는 본 파일만 지우면 `.bak` 에서 되살아난다 → 반드시 `GameManager.DeleteSaveFiles()` 를 쓴다(플레이 중이면 그 세션의 저장도 멈춘다 — 안 그러면 모아 쓰기·종료 저장이 곧바로 다시 쓴다).
- 저장 직후 파일을 읽어 확인하는 검증 코드는 `SaveGameNow()` 를 써야 한다(`SaveGame()` 은 최대 3초 지연).
- 강제 종료 시 최대 3초분 진행을 잃을 수 있다(의도된 절충 — 기록마다 fsync 하므로 너무 잦게 쓰지 않는다). 세션 중 중요한 순간이 생기면 `SaveGameNow()`.
- 세이브에 새 필드를 넣을 때: 참조형이면 `NormalizeSave` 에 null 보정을, 인덱스로 접근하는 고정 배열이면 `Fit` 을 추가한다. 데이터표에서 파생되는 값은 저장하지 않는다(`[JsonIgnore]`).
- 회귀 검사: `Test/Integration/Save - 내구성 검사` (플레이 모드, 실행 전 세이브를 복사해 두고 복원한다).
