# [슬라이드 8] Popup 선행 작업 — 공통 BlackBG_Layer

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 8
- 첨부 이미지:
  - `기획 ppt/slide_images/slide8/slide8_shape003_Google_Shape_124_p4.gif` — Popup 열리기 전 정상 화면 (마을 풍경, 플레이어 BAR 밝음)
  - `기획 ppt/slide_images/slide8/slide8_shape005_Google_Shape_126_p4.png` — Popup 열린 후 비교 화면 (전체가 어두운 검은 레이어로 뒤덮임 = BlackBG_Layer 적용 상태)

먼저 Read 도구로 두 이미지를 비교해서 "열기 전 vs 열린 후" 전체 밝기 차이를 확인해.

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**. 기획/디자이너가 Inspector로 값을 조정할 수 없고 중복 재생·값 불일치가 생기기 쉬움.
  - 단일 트윈이 필요하면 Inspector 조정 가능한 `DOTweenAnimation`을 prefab에 추가하는 쪽을 우선 고려. 정 필요할 때만 코드 API 사용.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지** (taskkill 금지). 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약

### 기획 의도
**플레이어가 해당 Popup에 더 집중할 수 있도록** Popup이 열릴 때 뒤의 메인 화면을 어두운 오버레이로 덮는다.

### 레이어 구조 (Hierarchy 순서)
```
Main Screen          ← 게임 원본 화면 (뒤)
BlackBG_Layer        ← 검정 오버레이 (중간)
Popup_Layer          ← 실제 Popup UI (앞)
```
→ **모든 Popup**은 자신의 하위에 스크린 전체 크기의 `BlackBG_Layer` 를 가지고 있어야 한다.

### BlackBG_Layer 속성
| 항목 | 값 |
|---|---|
| Sprite | `None` (빈 Image — solid color로 동작) |
| Color | `#000000`, alpha `220/255` |
| 크기 | 스크린 전체 (Canvas 풀 stretch) |
| RaycastTarget | `true` (클릭 감지용) |

### DOTween Fade Animation
- Component: **DOTweenAnimation** (**prefab에 컴포넌트로** 부착 — 코드 Sequence 지양)
- Type: `Fade`
- Duration: `0.3s`
- Delay: `0`
- Ease: `OutQuad`
- Loops: `1`
- From: `0` (알파 0에서 시작)
- Target alpha: `220/255` (스펙의 "Color alpha 220")
- **On Enable → Restart**: 팝업 열릴 때 자동 재생
- **On Disable → Rewind**: 팝업 닫힐 때 역재생 (알파 0 복귀)

### 클릭으로 닫기
- `BlackBG_Layer` 클릭 시 해당 Popup의 **Disable 애니메이션 재생 후** Popup 닫기 (`ClosePopupUI`).
- 단, **BlackBG_Layer 생성 후 0.5초 동안은 클릭 무시**. 0.5초 뒤부터 클릭 시 닫기 동작.
  - (의도: Popup 열자마자 실수로 닫히는 것 방지)

## 작업 지시
1. **현재 구현 상태 먼저 조사**
   - `UI_Popup` 기본 클래스 (`Assets/@Scripts/UI/UI_Popup.cs` 또는 `UI_Base` 계열) 읽고 공용 BlackBG 처리 유무 확인.
   - `UI_CustomOrderPopup.cs` (L149~)의 개별 BlackBG 구현 확인 (현재 `CustomOrderPopupBackground` Image를 0→220/255로 DOFade).
   - `UI_PotionPopup.cs` 의 BlackBG 구현 확인.
   - 다른 Popup들 (`UI_SettingPopup`, `UI_DialoguePopup`, `UI_PotionDetailPopup`, `UI_SkillDetailPopup` 등)에 BlackBG가 있는지 `Grep`으로 전수 조사.
   - 현재 prefab들 (`Assets/@Resources/Prefab/UI/Popup/*.prefab`) 에 `BlackBG_Layer` 오브젝트가 있는지 yaml에서 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] 공용 `BlackBG_Layer` 시스템이 `UI_Popup` 베이스 클래스 (또는 UIManager)에 있는가?
   - [ ] 모든 Popup prefab이 `BlackBG_Layer` 자식을 가지는가?
   - [ ] BlackBG 속성: Sprite None, Color `#000000 alpha 220/255`, 스크린 전체 stretch?
   - [ ] BlackBG 오브젝트에 `DOTweenAnimation` 컴포넌트 (Fade 0.3s OutQuad Loops=1 From=0) 부착?
   - [ ] On Enable → Restart / On Disable → Rewind 설정?
   - [ ] BlackBG 클릭 시 Popup 닫기 (Disable 애님 후 `Managers.UI.ClosePopupUI()`)?
   - [ ] 생성 후 0.5초 동안 클릭 무시 처리?
   - [ ] 개별 Popup에 중복 구현된 BlackBG 코드가 있으면 공용 시스템으로 통합?

3. **누락분만 구현 / 공통 시스템으로 승격**
   - **공용 BlackBG_Layer prefab 생성** (`Assets/@Resources/Prefab/UI/Common/BlackBG_Layer.prefab`):
     - 루트 Image (Sprite=None, Color 000000 alpha 0) + 스크린 풀 stretch (anchor 0,0 ~ 1,1 offset 0)
     - `Button` 컴포넌트 (raycastTarget 수단으로 사용)
     - `DOTweenAnimation` (Fade, 0.3s OutQuad, From 0 To 220/255, AutoPlay Off, Loops 1, **Link=OnEnable/OnDisable** 설정)
   - `UI_Popup` 기본 클래스에 BlackBG 관련 공통 로직 추가:
     - `OnEnable()`에서 BlackBG 자동 스폰 (이미 prefab에 있으면 유지)
     - `_blackBgReadyTime = Time.unscaledTime + 0.5f` 로 클릭 무시 기간 기록
     - BlackBG Button onClick → `if (Time.unscaledTime < _blackBgReadyTime) return; else ClosePopupUI()`
     - `ClosePopupUI` 직전 DOTweenAnimation Rewind 후 완료 시 실제 Destroy
   - 기존 Popup 개별 BlackBG 구현 제거 또는 공용 레이어를 참조하도록 리팩토링 (`UI_CustomOrderPopup`, `UI_PotionPopup` 등).
   - DOTween 규약: 반드시 **prefab의 DOTweenAnimation** 으로 구성. 코드에서 `DOFade` 직접 호출은 보조적으로만 (예: 현재 UI_CustomOrderPopup의 L156 `bg.DOFade(220f/255f, 0.3f)` 는 DOTweenAnimation 컴포넌트로 이관).

4. **결과 보고**
   - 점검 체크리스트 결과 + 공용 prefab 경로 + UI_Popup 변경사항 + 개별 Popup 리팩토링 목록 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/UI_Popup.cs` (베이스 클래스)
- `Assets/@Scripts/Managers/Core/UIManager.cs` (Popup 스택)
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs` (L149~ BlackBG 개별 구현)
- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_DialoguePopup.cs`
- `Assets/@Scripts/UI/Popup/UI_SettingPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/*.prefab` (모든 팝업 prefab — BlackBG_Layer 자식 유무 확인)
