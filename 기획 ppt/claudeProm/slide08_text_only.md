# [슬라이드 8] Popup 선행 작업 — 공통 BlackBG_Layer

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**.
  - 단일 트윈은 prefab의 `DOTweenAnimation` 우선.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (이미지 없이 텍스트로 완전 기술)

### 기획 의도
Popup이 열릴 때 뒤의 메인 화면을 어둡게 덮어 유저가 Popup에 더 집중하게 한다.

### 레이어 구조
```
Main Screen (뒤)
└─ BlackBG_Layer (검정 오버레이, 중간)
   └─ Popup_Layer (실제 Popup UI, 앞)
```
**모든 Popup**은 자신의 하위에 스크린 전체 크기의 `BlackBG_Layer`를 반드시 갖는다.

### BlackBG_Layer 속성
| 항목 | 값 |
|---|---|
| Sprite | None (solid color Image) |
| Color | `#000000`, alpha `220/255` |
| 크기 | 스크린 전체 (Canvas 풀 stretch) |
| RaycastTarget | true (클릭 감지) |

### DOTween Fade (prefab에 DOTweenAnimation 컴포넌트로)
- Type: Fade
- Duration: 0.3s / Delay: 0 / Ease: OutQuad / Loops: 1
- From: 0 (알파 0 시작)
- Target alpha: 220/255
- On Enable → Restart (팝업 열릴 때 자동 재생)
- On Disable → Rewind (닫힐 때 역재생)

### 클릭으로 닫기
- BlackBG_Layer 클릭 → 해당 Popup Disable 애니메이션 재생 후 닫기.
- **생성 후 0.5초 동안은 클릭 무시** (실수 방지).
- 0.5초 경과 후부터 정상적으로 클릭 시 닫기.

## 작업 지시
1. **현재 구현 상태 조사**
   - `UI_Popup` 베이스 클래스에 공용 BlackBG 처리 유무 확인.
   - `UI_CustomOrderPopup.cs` (L149~), `UI_PotionPopup.cs` 의 개별 BlackBG 구현 확인.
   - 다른 Popup들(`UI_SettingPopup`, `UI_DialoguePopup`, `UI_PotionDetailPopup`, `UI_SkillDetailPopup` 등)에 BlackBG 유무 `Grep`.
   - Popup prefab들에 `BlackBG_Layer` 자식 오브젝트 유무 yaml 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] 공용 BlackBG_Layer 시스템 (UI_Popup 또는 UIManager 레벨)?
   - [ ] 모든 Popup prefab이 BlackBG_Layer 자식 보유?
   - [ ] Sprite None, Color `#000000 alpha 220/255`, 풀 스트레치?
   - [ ] DOTweenAnimation(Fade 0.3s OutQuad Loops=1 From=0) 부착?
   - [ ] On Enable→Restart / On Disable→Rewind?
   - [ ] 클릭 시 Disable 애님 후 ClosePopupUI?
   - [ ] 생성 후 0.5초 클릭 무시?
   - [ ] 중복 구현된 개별 BlackBG 코드는 공용 시스템으로 통합?

3. **누락분만 구현 / 공통 시스템 승격**
   - `Assets/@Resources/Prefab/UI/Common/BlackBG_Layer.prefab` 공용 prefab 신설:
     - 풀 스트레치 Image (Sprite=None, Color 000000 alpha 0)
     - Button 컴포넌트
     - DOTweenAnimation(Fade 0.3s OutQuad From=0 To=220/255, AutoPlay Off, Link OnEnable/OnDisable)
   - UI_Popup 베이스에 공용 로직:
     - OnEnable에서 BlackBG 스폰 (없으면)
     - 클릭 무시 타이머 (`Time.unscaledTime + 0.5f`)
     - BlackBG Button onClick → ClosePopupUI (타이머 체크)
   - 개별 Popup의 기존 BlackBG 중복 구현 제거 또는 공용 레이어 참조로 리팩토링.
   - DOTween은 prefab의 DOTweenAnimation 컴포넌트로 구성 (코드 DOFade 이관).

4. **결과 보고**
   - 체크리스트 결과 + 공용 prefab 경로 + UI_Popup 변경 + 리팩토링된 Popup 목록 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/UI_Popup.cs`
- `Assets/@Scripts/Managers/Core/UIManager.cs`
- `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs` (L149~ 개별 BlackBG)
- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_DialoguePopup.cs`
- `Assets/@Scripts/UI/Popup/UI_SettingPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/*.prefab`
