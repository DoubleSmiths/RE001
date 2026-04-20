# [슬라이드 5] 상단 재화 BAR (골드 / 결정 / 환경설정 버튼)

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
- 화면 **최상단 HUD BAR**로 플레이어 보유 재화량 표시 (항상 표시).
- 배경 프레임: `1_basic_button` (다크 브라운, 가로로 긴 띠).
- 구성: [금화 아이콘 + 금화량] → [결정 아이콘 + 결정량] → [환경설정 버튼]

### 1. 금화(Gold) TEXT
| 항목 | 값 |
|---|---|
| 폰트 | `PF_Stardust` (고정 — 언어 무관) |
| 본 색상 | `#FFFFFF` |
| Outline 색 | `#FFF600` |
| Outline alpha | `80/255` |
| Outline offset | `x=0.2, y=-0.2` |
| 아이콘 | `Icon38` (황금 동전) |

### 2. 결정(Crystal) TEXT
| 항목 | 값 |
|---|---|
| 폰트 | `PF_Stardust` (고정) |
| 본 색상 | `#00F3FF` |
| Outline 색 | `#FFF600` |
| Outline alpha | `80/255` |
| Outline offset | `x=0.2, y=-0.2` |
| 아이콘 | `Icon25` (하늘색 결정) |

### 3. Amount 변화 연출 (공통)
- Amount 변화 시 **Lerp**로 숫자 텍스트가 부드럽게 증가/감소.
- 증가: 텍스트가 `#00FF00` (초록)으로 잠깐 빛난 후 원 색 복귀.
- 감소: 텍스트가 `#FF0000` (빨강)으로 잠깐 빛난 후 원 색 복귀.

### 4. 환경설정 버튼
- 이미지: `1_armor` (우상단).
- 클릭 → 환경설정 Popup (`UI_SettingPopup`) 호출.
- **클릭 애니메이션 (DOTween Scale Yoyo)**:
  - Duration: 0.07s / Delay: 0 / Ease: OutQuad
  - Loops: 2 / Loop Type: Yoyo
  - TO: 0.75 (Uniform Scale)
  - 의미: 0.07s 사이 원 → 0.75배 축소 후 복귀, Yoyo 2회 (총 ~0.14s).

## 작업 지시
1. **현재 구현 상태 조사**
   - `UI_GameScene.cs`의 `MainGoldText` / `MainCrystalText` 바인딩, Lerp 갱신, 색상 플래시 로직 확인.
   - `GameManager.cs`의 `Gold` / `Crystal` property + 변경 이벤트 확인.
   - `UI_SettingPopup.cs` 존재 및 Setting 버튼 클릭 핸들러 확인.
   - `UI_GameScene.prefab`의 재화 BAR TMP_Text 폰트 & Outline 설정 확인.
   - 환경설정 버튼 prefab에 Scale DOTweenAnimation 존재 여부 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] 금화/결정 TMP_Text 폰트 `PF_Stardust` 고정?
   - [ ] 금화 본 색 `#FFFFFF`, 결정 본 색 `#00F3FF`?
   - [ ] TMP Outline 색 `#FFF600`, alpha 80, offset (0.2, -0.2)?
   - [ ] Amount 변화 시 Lerp 갱신 동작?
   - [ ] 증가 `#00FF00` / 감소 `#FF0000` 플래시 후 원색 복귀?
   - [ ] Setting 버튼 클릭 시 `UI_SettingPopup` 열림?
   - [ ] Setting 버튼에 Scale DOTweenAnimation (Duration 0.07, Ease OutQuad, Loops 2 Yoyo, To 0.75 Uniform) 붙어 있나?
   - [ ] Setting 버튼 클릭 애니메이션이 Inspector(DOTweenAnimation) 기반인가? 코드이면 이관.

3. **누락분만 구현**
   - 폰트/Outline 미준수 시 TMP 설정 교정.
   - Setting 버튼 클릭 애니메이션은 prefab의 DOTweenAnimation (autoPlay=false, Play on Click 바인딩)으로 구성.
   - Flash 색상이 스펙과 다르면 `#00FF00` / `#FF0000`로 조정.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일/Prefab 경로 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (L44~ Texts enum, L347~ Gold/Crystal 갱신, L372~ FlashCurrencyText, L675 OnClickSetting)
- `Assets/@Scripts/UI/Popup/UI_SettingPopup.cs`
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (Gold, Crystal)
- `Assets/@Resources/Prefab/UI/Scene/UI_GameScene.prefab`
- `Assets/@Resources/Font/PFStardust SDF.asset`
