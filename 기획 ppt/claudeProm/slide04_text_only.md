# [슬라이드 4] UI Feedback Guide — NEW 버튼 / [!] 버튼

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**. 기획/디자이너가 Inspector로 값을 조정할 수 없고 중복 재생·값 불일치가 생기기 쉬움.
  - 단일 트윈이 필요하면 Inspector 조정 가능한 `DOTweenAnimation`을 prefab에 추가하는 쪽을 우선 고려. 정 필요할 때만 코드 API 사용.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지** (taskkill 금지). 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (이미지 없이 텍스트로 완전 기술)

### 1. NEW 버튼 (뱃지)
| 항목 | 값 |
|---|---|
| 용도 | 아이템/기능이 새로 생겼을 때 표시 |
| 사라짐 조건 | 해당 버튼 클릭 or Popup 닫기 |
| 폰트 | `DOSSaemmul` (고정) |
| 색상 | `#FFFF00` |
| 텍스트 | "NEW" (영문 고정) |
| 표시 위치 | 각 개별 기획서에 서술 |

- **DOTween 애니메이션 (Move)**:
  - Duration: 0.5s / Ease: OutQuad / Loops: -1 / Loop Type: Restart
  - TO: (x=1, y=2, z=0), Snapping ON
  - 의미: 원점에서 (1,2,0)으로 미세하게 이동·스냅 반복되어 "눈에 띄게 흔들리는" 느낌.

### 2. [!] 버튼 (퀘스트 가이드 느낌표 아이콘)
| 항목 | 값 |
|---|---|
| 용도 | 메인/서브 퀘스트의 다음 행선지를 마킹 |
| 이미지 | `gui_atlas` 아틀라스의 `exclamation_mark01` |
| 색상 | `#FFE300` (황금 노랑) 또는 `#14FF00` (녹색) — 용도 분류 |
| 표시/비표시 | 퀘스트 번호(진행 상태)에 맞춰 on/off |
| 표시 위치 | 각 개별 기획서에 서술 |

- **DOTween 애니메이션 (Punch/Rotation)**:
  - Duration: 1.5s / Ease: OutQuad / Loops: -1 / Loop Type: Restart
  - TO: (x=0, y=0, z=15) (Z축 회전 15도)
  - Vibrato: 10 / Elasticity: 0
  - 의미: Z축으로 ±15도 범위를 1.5초 주기로 흔들림, 튕김 없음(elasticity=0).

## 작업 지시
1. **현재 구현 상태 조사**
   - `Grep`으로 `NewTxt`, `AlchemyButtonNewTxt` 등 NEW 관련 식별자를 모두 찾아 어느 UI에 있는지 파악.
   - 대상 prefab에서 NEW 오브젝트에 `DOTweenAnimation` 컴포넌트가 이미 붙어 있는지 확인.
   - `Grep`으로 `exclamation_mark`, `QuestGuide` 등 [!] 관련 구현 존재 여부 확인.
   - `gui_atlas` 아틀라스 슬라이싱에 `exclamation_mark01`이 있는지 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] NEW 공용 prefab 존재? 중복 구성이면 통합 여부 검토.
   - [ ] NEW 폰트/색상(DOSSaemmul, #FFFF00) 일관 설정?
   - [ ] NEW 오브젝트에 Move Loops=-1 / 0.5s / OutQuad / TO(1,2,0) Snapping DOTweenAnimation?
   - [ ] NEW 자동 해제 로직(버튼 클릭/Popup 닫힘)?
   - [ ] [!] 공용 prefab/컴포넌트 존재?
   - [ ] [!] 스프라이트 `gui_atlas.exclamation_mark01` 참조?
   - [ ] [!] 오브젝트에 Punch/Rotation Loops=-1 / 1.5s / OutQuad / TO(0,0,15) Vibrato=10 Elasticity=0 DOTweenAnimation?
   - [ ] [!] 퀘스트 번호에 따른 표시/비표시 제어 시스템?

3. **누락분만 구현**
   - NEW/[!] 모두 **재사용 가능한 공용 prefab**으로 (이미 있으면 유지).
   - 애니메이션은 **DOTween 규약**에 따라 prefab의 `DOTweenAnimation` 컴포넌트로 구현 (코드 Sequence 금지).
   - NEW 자동 해제는 PlayerPrefs 또는 `GameData`에 확인 플래그 저장하는 공용 헬퍼로.
   - [!] 퀘스트 가이드는 QuestManager 부재 시 최소 스텁 설계 후 보고.

4. **결과 보고**
   - 체크리스트 결과 + 수정/신규 파일 + 변경된 prefab 경로 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs`
- `Assets/@Scripts/UI/SubItem/UI_BattleBtnGroup.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs`
- `Assets/@Scripts/UI/SubItem/UI_InvSlot.cs`, `UI_ImportantSlot.cs`
- `Assets/@Scripts/Utils/Define.cs`
- `Assets/IMPORTS/DarkFantasyGUI/GUI/Textures/gui_atlas.psd` (exclamation_mark01)
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (퀘스트 상태 필드)
