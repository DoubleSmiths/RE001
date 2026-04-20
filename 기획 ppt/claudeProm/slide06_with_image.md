# [슬라이드 6] Player BAR (HP / EXP / 초상화 / 직업 / 레벨 / 모험가 등급)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 6
- 첨부 이미지:
  - `기획 ppt/slide_images/slide6/slide6_shape000_그림_2.png` — Player BAR 전체 인게임 적용 모습 (S+ / 여행자 / Lv.999 / 100/100 / 00.0000%)
  - `기획 ppt/slide_images/slide6/slide6_shape007_그림_4.png` — `Hp_frame_Empty` (Player BAR 메인 프레임, 다크 브라운)
  - `기획 ppt/slide_images/slide6/slide6_shape008_그림_6.png` — HP 배경 슬릿 (어두운 브라운 가로 띠)
  - `기획 ppt/slide_images/slide6/slide6_shape010_그림_9.png` — `Hp_red` (HP 바 스프라이트, 빨강 F00000 계열)
  - `기획 ppt/slide_images/slide6/slide6_shape012_그림_12.png` — `1-EG` (EXP 바 스프라이트, 초록)
  - `기획 ppt/slide_images/slide6/slide6_shape014_그림_15.png` — `HP_Portrait_frame` (원형 초상화 프레임)
  - `기획 ppt/slide_images/slide6/slide6_shape016_그림_18.png` — `Player_01_Port` (기본 플레이어 초상화 — 직업에 따라 교체)
  - `기획 ppt/slide_images/slide6/slide6_shape020_그림_61.gif` — HP_BAR hurt 애니메이션 (65/65, 빨강 플래시)
  - `기획 ppt/slide_images/slide6/slide6_shape021_그림_63.gif` — EXP_BAR 애니메이션 (40.0000%, 초록 플래시 + scale)
  - `기획 ppt/slide_images/slide6/slide6_shape025_그림_70.gif` — HP_BAR heal 애니메이션 (65/65, 초록 플래시 + scale)
  - `기획 ppt/slide_images/slide6/slide6_shape029_그림_75.png` — 완성된 Player BAR 조합 스틸샷
  - `기획 ppt/slide_images/slide6/slide6_shape045_그림_121.png` — 긴 직업명 예시 ("엄청난직업이름임 ∶ LV.999") — 직업 글자수에 따라 레벨 표기 밀림

먼저 Read 도구로 위 이미지들을 모두 열어 시각적으로 확인한 뒤 작업을 시작해.

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**. 기획/디자이너가 Inspector로 값을 조정할 수 없고 중복 재생·값 불일치가 생기기 쉬움.
  - 단일 트윈이 필요하면 Inspector 조정 가능한 `DOTweenAnimation`을 prefab에 추가하는 쪽을 우선 고려. 정 필요할 때만 코드 API 사용.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지** (taskkill 금지). 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약

### 전체 구성 (좌→우)
```
┌──────────────┐  [모험가등급] [직업] ┆ [Lv.###]
│   초상화     │  ████████ 현재HP / 최대HP
│ (HP_Portrait)│  ████     ##.####%
└──────────────┘
```
- 베이스 프레임: `Hp_frame_Empty`
- 초상화 프레임: `HP_Portrait_frame` (원형)
- 초상화: `Player_01_Port` (기본) — **추후 직업 전직 시 해당 직업 Portrait로 교체**

### 1. 모험가 등급
- 범위: `0 ~ 99`
- 표기: `0`일 경우 `[    ]` BLANK로 (등급이 없는 상태 표시)
- 그 외: `S+`, `A`, `B` 등 등급 문자열

### 2. 직업 (Job)
- 플레이어의 현재 직업 표기 (예: `여행자`).
- **직업 글자 수에 따라 레벨(Lv.###) 표기가 오른쪽으로 밀리는 동적 레이아웃** — 직업 텍스트가 길어지면 레벨이 밀려서 전체가 중앙 또는 우측으로 정렬되는 구조.

### 3. 레벨
- 범위: `1 ~ 999`
- 표기: `Lv.###` (예: `Lv.999`)
- 폰트: 슬라이드 3 스펙에 따라 **언어 무관 DOSSaemmul 고정**.

### 4. 텍스트 공통 Outline
- 색상: `#62007B` (보라)
- Offset: `x=1, y=1`

### 5. HP 텍스트
- 형식: `현재HP / 최대HP` (예: `100 / 100`)

### 6. EXP 텍스트
- 형식: `##.####%` (소수점 **4자리** 고정, 예: `00.0000%`, `40.0000%`)

### 7. HP_BAR Anim (피해 시 — hurt)
- Bar sprite는 **Lerp** 처리 (Fill Amount 부드럽게 감소).
- 색상 플래시:
  - 0.17초 동안 `#FF0000` (빨강) 으로 변환
  - 이어서 0.17초 동안 `#FFFFFF` (흰색) 으로 복귀
  - 총 ~0.34초

### 8. HP_BAR Anim (회복 시 — heal)
- Bar sprite는 **Lerp** 처리 (Fill Amount 부드럽게 증가).
- 색상 플래시:
  - 0.17초 동안 `#58FF00` (초록) 으로 변환
  - 이어서 0.17초 동안 `#FFFFFF` (흰색) 으로 복귀
- **Scale**도 동일 타이밍으로 증가 → 복귀 (펀치 느낌).

### 9. EXP_BAR Anim (경험치 획득 시)
- Bar sprite는 Lerp 처리 (Fill Amount 부드럽게 증가).
- 색상 플래시 (heal과 동일):
  - 0.17초 `#58FF00` → 0.17초 `#FFFFFF`
- Scale도 증가 → 복귀 (heal 동일).

## 작업 지시
1. **현재 구현 상태 먼저 조사**
   - `Assets/@Scripts/UI/Scene/UI_GameScene.cs` L44~ Texts enum에서 `PlayerRankText`, `PlayerLevelText` 확인.
   - L104~ GameObjects enum에서 `PlayerBar`, `PlayerHPBarGuageBox` 확인.
   - L402~ `RefreshPlayerBar()` 메서드의 갱신 로직 확인 (현재 레벨/이름 갱신은 존재, 직업/등급 갱신 여부?).
   - `UI_GameScene.prefab`에서 HP Bar 오브젝트에 **DOTweenAnimation (색상 플래시 / Scale)** 컴포넌트가 붙어 있는지 prefab yaml에서 `DOTweenAnimation` grep.
   - `PlayerController` 또는 `GameManager`에 `OnPlayerDamaged`, `OnPlayerHealed`, `OnPlayerExpGain` 이벤트 존재 여부 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] 모험가 등급 텍스트 존재 + 0일 때 `[    ]` BLANK 표기 처리?
   - [ ] 직업 텍스트 (`JobText` 또는 등가)가 별도 TMP_Text로 존재하고 `Managers.Game.JobName` (또는 등가) 참조?
   - [ ] 레벨 표기 `Lv.###` 형식 (현재 `Lv.{lv}` 로 구현됨 — 999 자리 유지 확인).
   - [ ] 직업 글자 수 변화에 따라 레벨이 오른쪽으로 밀리는 **동적 레이아웃**(HorizontalLayoutGroup + ContentSizeFitter 등)?
   - [ ] 텍스트 Outline 색 `#62007B`, offset (1, 1)로 설정?
   - [ ] 초상화가 `Player_01_Port` 기본, 직업별 교체 API 존재? (예: `SetPortrait(int jobId)`)
   - [ ] HP 텍스트 형식 `현재 / 최대` (예: `100 / 100`)?
   - [ ] EXP 텍스트 형식 `##.####%` (소수점 4자리)?
   - [ ] HP Bar Fill Lerp 갱신?
   - [ ] HP Bar hurt 연출: 0.17s 빨강(#FF0000) → 0.17s 흰색(#FFFFFF)?
   - [ ] HP Bar heal 연출: 0.17s 초록(#58FF00) → 0.17s 흰색 + Scale 증가→복귀?
   - [ ] EXP Bar 연출: 0.17s 초록(#58FF00) → 0.17s 흰색 + Scale 증가→복귀?
   - [ ] 각 Bar 애니메이션이 prefab의 **DOTweenAnimation 컴포넌트**로 구성되어 있는가? (코드 Sequence면 이관)

3. **누락분만 구현**
   - HP/EXP Bar 애니메이션은 **prefab에 DOTweenAnimation 2개씩** (Color 플래시용 + Scale Punch용) 추가하고, `DOPlayById("Hurt")`, `DOPlayById("Heal")` 방식으로 코드에서 ID 지정 트리거.
   - 단, 색상 2단 플래시(빨강/초록 → 흰색)는 **단일 DOColor로 0.34s 총 길이**가 아니라 **0.17s × 2 Yoyo** 로 `From=흰색, To=빨강/초록, Loops=2, LoopType=Yoyo` 방식으로 구성 권장. (DOTweenAnimation 설정으로 가능)
   - Lerp 갱신은 `Image.fillAmount`에 `DOTween.To`를 코드에서 사용해도 OK (Fill Amount는 Inspector DOTweenAnimation이 지원하지만 매번 목표값이 달라지므로 코드 트리거가 합리적).
   - 직업 전직/교체 구조가 아직 없다면 최소 `SetJob(int jobId)` 스텁 API만 설계 후 보고.
   - 동적 레이아웃은 직업 TMP + 구분자 `∶` + 레벨 TMP를 `HorizontalLayoutGroup`으로 묶고 부모에 `ContentSizeFitter`로 해결.

4. **결과 보고**
   - 점검 체크리스트 결과 + 수정/신규 파일 + 변경된 prefab 경로 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (L44~ Texts enum, L104~ GameObjects enum, L402 RefreshPlayerBar)
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (Level, PlayerName, Hp, MaxHp, Exp, MaxExp, Grade, JobId/JobName 확인)
- `Assets/@Scripts/Controllers/PlayerController.cs` (OnPlayerDamaged, OnPlayerHealed, OnPlayerLevelUp, OnPlayerDataUpdated 이벤트)
- `Assets/@Resources/Prefab/UI/Scene/UI_GameScene.prefab` (PlayerBar / PlayerHPBarGuageBox 노드)
- `Assets/@Resources/Sprite/` (Hp_frame_Empty, Hp_red, 1-EG, HP_Portrait_frame, Player_01_Port 스프라이트 경로)
