# [슬라이드 6] Player BAR (HP / EXP / 초상화 / 직업 / 레벨 / 모험가 등급)

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**. 기획/디자이너가 Inspector로 값을 조정할 수 없고 중복 재생·값 불일치가 생기기 쉬움.
  - 단일 트윈이 필요하면 Inspector 조정 가능한 `DOTweenAnimation`을 prefab에 추가하는 쪽을 우선 고려. 정 필요할 때만 코드 API 사용.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지** (taskkill 금지). 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (이미지 없이 텍스트로 완전 기술)

### 전체 구성
- 좌측에 원형 초상화 (HP_Portrait_frame + Player_01_Port).
- 우측 상단: `[모험가등급] [직업] ∶ Lv.###`
- 우측 중단: HP 바 (Hp_red 스프라이트) + 텍스트 `현재HP / 최대HP`
- 우측 하단: EXP 바 (1-EG 스프라이트, 초록) + 텍스트 `##.####%`
- 베이스 프레임: `Hp_frame_Empty` (다크 브라운 전체 프레임).

### 1. 모험가 등급
- 범위 0 ~ 99.
- `0`일 때 `[    ]` BLANK로 표기 (등급 없음 상태).
- 그 외: `S+`, `A`, `B` 등 등급 문자열.

### 2. 직업
- 현재 직업명 표기 (예: `여행자`).
- **직업 글자 수가 길어지면 레벨(Lv.###)이 오른쪽으로 밀리는 동적 레이아웃**.

### 3. 레벨
- 범위 1 ~ 999.
- 형식: `Lv.###` (예: `Lv.999`).
- 폰트: 슬라이드 3 스펙 — 언어 무관 DOSSaemmul 고정.

### 4. 텍스트 공통 Outline
- 색상: `#62007B` (보라), Offset (x=1, y=1).

### 5. HP 텍스트
- 형식: `현재HP / 최대HP` (예: `100 / 100`).

### 6. EXP 텍스트
- 형식: `##.####%` (소수점 4자리 고정).

### 7. 초상화
- 기본: `Player_01_Port`.
- 추후 직업 전직 시 **해당 직업의 Portrait로 교체**.

### 8. HP_BAR Anim (피해 — hurt)
- Bar sprite **Lerp** 감소 (fillAmount).
- 색상: 0.17s 동안 `#FF0000` (빨강) → 0.17s 동안 `#FFFFFF` (흰색) 복귀.

### 9. HP_BAR Anim (회복 — heal)
- Bar sprite Lerp 증가.
- 색상: 0.17s `#58FF00` (초록) → 0.17s `#FFFFFF`.
- **Scale 펀치** (증가 후 복귀) 동일 타이밍.

### 10. EXP_BAR Anim
- Bar sprite Lerp 증가.
- 색상: 0.17s `#58FF00` → 0.17s `#FFFFFF`.
- **Scale 펀치** 동일 타이밍 (heal과 동일 규칙).

## 작업 지시
1. **현재 구현 상태 조사**
   - `UI_GameScene.cs` Texts enum의 `PlayerRankText`, `PlayerLevelText` + GameObjects enum의 `PlayerBar`, `PlayerHPBarGuageBox` 확인.
   - `RefreshPlayerBar()` 메서드 갱신 범위 확인 (현재 레벨/이름은 있음).
   - prefab의 HP/EXP Bar 오브젝트에 DOTweenAnimation 컴포넌트 유무.
   - `PlayerController`의 OnPlayerDamaged/Healed/ExpGain 이벤트 존재 여부.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] 모험가 등급 TMP + 0일 때 `[    ]` BLANK 처리?
   - [ ] 직업 TMP 존재 + `Managers.Game.JobName` 참조?
   - [ ] 레벨 `Lv.###` 형식 확인 (999 자리 유지)?
   - [ ] 직업 글자수 변화에 동적 레이아웃 (HorizontalLayoutGroup + ContentSizeFitter)?
   - [ ] 텍스트 Outline 색 `#62007B`, offset (1, 1)?
   - [ ] 초상화 직업별 교체 API (`SetPortrait(int jobId)`)?
   - [ ] HP 텍스트 `100 / 100` 형식?
   - [ ] EXP 텍스트 `##.####%` 형식 (소수점 4자리)?
   - [ ] HP Bar fillAmount Lerp 갱신?
   - [ ] HP hurt: 0.17s #FF0000 → 0.17s #FFFFFF?
   - [ ] HP heal: 0.17s #58FF00 → 0.17s #FFFFFF + Scale 펀치?
   - [ ] EXP: 0.17s #58FF00 → 0.17s #FFFFFF + Scale 펀치?
   - [ ] Bar 애니메이션이 DOTweenAnimation 컴포넌트 기반?

3. **누락분만 구현**
   - 색상 플래시는 DOTweenAnimation (`From=흰색, To=빨강/초록, Duration=0.17, Loops=2, LoopType=Yoyo`) 으로 구성.
   - fillAmount Lerp은 `DOTween.To` 코드 사용 가능 (목표값 가변).
   - 직업 전직 구조 없으면 `SetJob(int jobId)` 스텁만 설계 후 보고.
   - 동적 레이아웃은 [직업TMP] - [∶] - [레벨TMP] 를 HorizontalLayoutGroup + ContentSizeFitter로.

4. **결과 보고**
   - 체크리스트 결과 + 수정/신규 파일 + 변경 prefab 경로 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (L44~ Texts, L104~ GameObjects, L402 RefreshPlayerBar)
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (Level, PlayerName, Hp, MaxHp, Exp, MaxExp, Grade, JobId/JobName)
- `Assets/@Scripts/Controllers/PlayerController.cs` (OnPlayerDamaged, OnPlayerHealed, OnPlayerLevelUp, OnPlayerDataUpdated)
- `Assets/@Resources/Prefab/UI/Scene/UI_GameScene.prefab`
- `Assets/@Resources/Sprite/` (Hp_frame_Empty, Hp_red, 1-EG, HP_Portrait_frame, Player_01_Port)
