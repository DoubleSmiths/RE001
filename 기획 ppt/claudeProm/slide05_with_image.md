# [슬라이드 5] 상단 재화 BAR (골드 / 결정 / 환경설정 버튼)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 5
- 첨부 이미지:
  - `기획 ppt/slide_images/slide5/slide5_shape006_Google_Shape_95_p3.png` — Icon25 (결정/크리스탈 아이콘, 하늘색)
  - `기획 ppt/slide_images/slide5/slide5_shape007_Google_Shape_96_p3.png` — Icon38 (금화/골드 아이콘, 황금색)
  - `기획 ppt/slide_images/slide5/slide5_shape009_Google_Shape_98_p3.png` — `1_basic_button` (재화 BAR 배경 프레임, 다크 브라운)
  - `기획 ppt/slide_images/slide5/slide5_shape011_Google_Shape_100_p3.png` — `1_armor` (환경설정 버튼, 가방 모양 아이콘)
  - `기획 ppt/slide_images/slide5/slide5_shape012_Google_Shape_101_p3.png` — 인게임 적용 모습 (전체 BAR: 🪙999999 💎99999 [환경설정])
  - `기획 ppt/slide_images/slide5/slide5_shape015_Google_Shape_104_p3.png` — 금화 TEXT 샘플 (`999999`, 흰색 + 노란 외곽선)
  - `기획 ppt/slide_images/slide5/slide5_shape016_Google_Shape_105_p3.png` — 결정 TEXT 샘플 (`99999`, 시안 + 노란 외곽선)
  - `기획 ppt/slide_images/slide5/slide5_shape023_Google_Shape_112_p3.gif` — 금화 0 GIF (amount 변화 Lerp + 증감 색상 플래시 연출)
  - `기획 ppt/slide_images/slide5/slide5_shape024_Google_Shape_113_p3.png` — 환경설정 버튼 스틸샷

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

### 전체
- 화면 **최상단 BAR**로 플레이어 보유 재화량 표시.
- 항상 표시 (HUD), 배경 프레임 = `1_basic_button`.
- 구성 요소: [금화 아이콘 + 금화량] + [결정 아이콘 + 결정량] + [환경설정 버튼].

### 1. 금화(Gold) TEXT
| 항목 | 값 |
|---|---|
| 폰트 | `PF_Stardust` **고정** (언어 무관 — 숫자 전용 고정 폰트) |
| 본 색상 | `#FFFFFF` |
| Outline 색 | `#FFF600` |
| Outline alpha | `80` (255 기준) |
| Outline offset | `x=0.2, y=-0.2` |
| 아이콘 | `Icon38` (황금 동전) |

### 2. 결정(Crystal) TEXT
| 항목 | 값 |
|---|---|
| 폰트 | `PF_Stardust` **고정** |
| 본 색상 | `#00F3FF` |
| Outline 색 | `#FFF600` |
| Outline alpha | `80` |
| Outline offset | `x=0.2, y=-0.2` |
| 아이콘 | `Icon25` (하늘색 결정) |

### 3. Amount 변화 연출 (공통)
- Amount 변화 시 **Lerp**로 text 갱신 (숫자가 부드럽게 증가/감소).
- **증가 시**: 텍스트가 `#00FF00` (초록)으로 잠깐 빛남 → 원래 색으로 복귀.
- **감소 시**: 텍스트가 `#FF0000` (빨강)으로 잠깐 빛남 → 원래 색으로 복귀.

### 4. 환경설정 버튼
- 이미지: `1_armor` (가방/갑옷 아이콘 모양, 우상단 배치).
- 클릭 → 환경설정 Popup 호출 (`UI_SettingPopup`).
- **클릭 애니메이션 (DOTween Scale, Yoyo punch)**:
  - Duration: `0.07s`
  - Delay: `0`
  - Ease: `OutQuad`
  - Loops: `2`
  - Loop Type: `Yoyo`
  - TO: `0.75` (Uniform Scale — x/y/z 동일)
  - 의미: 0.07초간 원래 크기 → 0.75배로 줄어들었다가 다시 원래 크기로 Yoyo 2회 (총 0.14초 수축-복원).

## 작업 지시
1. **현재 구현 상태 먼저 조사**
   - `Assets/@Scripts/UI/Scene/UI_GameScene.cs`의 `MainGoldText` / `MainCrystalText` 바인딩 및 `FlashCurrencyText`, `DOTween.To Lerp` 로직 읽기.
   - `Assets/@Scripts/Managers/Contents/GameManager.cs`의 `Gold` / `Crystal` property + 이벤트(`OnGoldChanged` 등) 확인.
   - `UI_SettingPopup.cs` 존재 확인 + `UI_GameScene.cs:675` 근처의 Setting 버튼 클릭 핸들러 확인.
   - prefab `UI_GameScene.prefab`에서 재화 BAR 영역의 TMP_Text 컴포넌트에 **PF_Stardust 폰트 및 Outline 설정**이 되어 있는지 확인 (Material Preset).
   - 환경설정 버튼 오브젝트에 **DOTweenAnimation (Scale Yoyo 2회)** 컴포넌트가 이미 있는지 prefab yaml에서 `DOTweenAnimation` grep.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] 금화/결정 TMP_Text가 `PF_Stardust` 폰트로 고정되어 있는가? (언어 전환 시에도 유지)
   - [ ] 금화 본 색상 `#FFFFFF`, 결정 본 색상 `#00F3FF` 로 설정되어 있는가?
   - [ ] TMP Outline: 색 `#FFF600`, alpha 80/255, offset (0.2, -0.2) 로 설정되어 있는가? (Material Preset 또는 TMP 컴포넌트 Underlay)
   - [ ] Amount 변화 시 **Lerp 갱신** 동작 확인 (`DOTween.To` 0.5s OutQuad 이미 구현되어 있음) — 스펙에 Duration이 명시 안 되어 있으니 현재 값 유지로 OK.
   - [ ] 증가 시 `#00FF00` / 감소 시 `#FF0000` 플래시 후 원색 복귀가 잘 되는가? (`FlashCurrencyText` 로직 검증)
   - [ ] 환경설정 버튼 클릭 시 `UI_SettingPopup` 열림 확인.
   - [ ] 환경설정 버튼 오브젝트에 **Scale DOTweenAnimation (Duration 0.07, Ease OutQuad, Loops 2 Yoyo, To 0.75 Uniform)** 가 prefab에 붙어 있는가? 없다면 추가.
   - [ ] 환경설정 버튼 애니메이션이 **Inspector 설정(DOTweenAnimation)** 기반인가, 아니면 코드에서 `DOScale`로 만들고 있는가? 코드 구현이면 DOTween 규약에 따라 prefab 컴포넌트로 이관.

3. **누락분만 구현**
   - 폰트/Outline 속성이 기획서 스펙과 어긋나면 TMP 인스펙터 (또는 Material Preset) 수정.
   - 환경설정 버튼 클릭 애니메이션은 **prefab에 DOTweenAnimation 추가**(autoPlay=false, Play on Click은 별도 이벤트 바인딩으로 트리거).
   - FlashCurrencyText가 있으면 색상값만 스펙에 맞게 교정 (#00FF00 / #FF0000 확인).

4. **결과 보고**
   - 점검 체크리스트 결과 + 수정 파일/Prefab 경로를 간결히 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (L44~ Texts enum, L347~ Gold/Crystal 갱신, L372~ FlashCurrencyText, L675 OnClickSetting)
- `Assets/@Scripts/UI/Popup/UI_SettingPopup.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (Gold, Crystal property + 변경 이벤트)
- `Assets/@Resources/Prefab/UI/Scene/UI_GameScene.prefab` (재화 BAR 노드 + TMP + Setting 버튼 DOTweenAnimation)
- `Assets/@Resources/Font/PFStardust SDF.asset`
