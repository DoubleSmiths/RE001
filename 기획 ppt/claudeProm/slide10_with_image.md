# [슬라이드 10] 최하단 버튼 (6종 탭: 상태 / 가방 / 물약 / 장비 / 기술 / 도감)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 10
- 첨부 이미지:
  - `기획 ppt/slide_images/slide10/slide10_shape003_그림_2.png` — 6개 탭 버튼 일렬 (상태 / 가방 / 물약 / 장비 / 기술 / 도감) — 순서 고정
  - `기획 ppt/slide_images/slide10/slide10_shape004_그림_4.png` — 화면 전체 예시 (최하단에 6개 버튼 배치)
  - `기획 ppt/slide_images/slide10/slide10_shape011_그림_17.gif` — 버튼 클릭 하이라이트 애니메이션 (상태 버튼이 노랗게 변하며 확대되는 연출)

먼저 Read 도구로 위 이미지들을 모두 열어 시각적으로 확인한 뒤 작업을 시작해.

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다.
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**.
  - 단일 트윈은 prefab의 `DOTweenAnimation` 우선.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 요약

### 전체 규칙
1. 최하단 버튼은 총 **6종류**.
2. 버튼의 순서는 **상태 → 가방 → 물약 → 장비 → 기술 → 도감** (좌→우, 고정).
3. 게임 첫 시작 시 **[상태] 버튼만 해금**된 상태. 나머지는 **퀘스트 진행도에 따라 순차 해금**.
4. 각 버튼은 클릭 시 **연관된 Popup창**을 호출.

### 버튼 ↔ Popup 매핑
| 버튼 | 아이콘 | 호출 Popup |
|---|---|---|
| 상태 | 캐릭터 | `UI_CharacterPopup` |
| 가방 | 가방 | `UI_InventoryPopup` |
| 물약 | 물약병 | `UI_PotionPopup` |
| 장비 | 검+방패 | `UI_EquipmentPopup` |
| 기술 | 검 교차 | `UI_SkillPopup` |
| 도감 | 날개(새) | `UI_BestiaryPopup` |

### 잠금(Lock) 상태 표시
- 해금되지 않은 버튼은 일반적으로 **어두운 오버레이 + 잠금 아이콘**(슬라이드 35 CustomOrder의 lockbg/lockimg 스펙과 유사 추정) 또는 비활성 Silhouette.
- 구체 잠금 시각 스펙은 슬라이드 10에 명시되지 않음 — 슬라이드 35의 공용 잠금 표시(검정 alpha 180 + Chat_Icon_Lock 80) 패턴을 따르는 것을 제안.

### 하이라이트 Anim (버튼 클릭 → Popup 열림 상태)
- 클릭된 버튼은 팝업이 열려 있는 동안 **하이라이트** 상태 유지 → Popup 닫히면 원 상태 복귀.
- **DOTween Color**
  - Duration: `0.3s`
  - Ease: `OutQuad`
  - TO: `#FFDA01` (황금 노랑)
- **DOTween Scale**
  - Duration: `0.3s`
  - Ease: `OutQuad`
  - TO: `1.1` (Uniform Scale)
- Popup 닫힘 시 Color=원색 / Scale=1.0 으로 역 재생 (Rewind 또는 역 트윈).

## 작업 지시
1. **현재 구현 상태 먼저 조사**
   - `Assets/@Scripts/UI/Scene/UI_GameScene.cs`:
     - L21~ Images enum: `CharacterButtonIcon`, `CharacterBtnNewIcon` 등 6개 버튼 아이콘 존재 확인.
     - L170~ 6개 버튼 BindEvent 확인 (`CharacterButton`, `InventoryButton`, `PotionButton`, `EquipmentButton`, `SkillButton`, `BestiaryButton`).
     - L678~ 각 `OnClickX()` 핸들러 — 현재 어떤 Popup을 여는지 확인. 일부는 `TODO: UI_PotionPopup 열기` 주석일 수 있음.
   - `UI_GameScene.prefab` 에서 6개 버튼 오브젝트에 **DOTweenAnimation (Color 0.3s TO #FFDA01, Scale 0.3s TO 1.1)** 컴포넌트가 있는지 확인.
   - `Managers.Game.CurrentQuestId` 또는 `Managers.Game.UnlockedBtmButtons` 같은 해금 상태 플래그 존재 여부 확인.
   - `UI_CharacterPopup`, `UI_InventoryPopup`, `UI_PotionPopup`, `UI_EquipmentPopup`, `UI_SkillPopup`, `UI_BestiaryPopup` 6개 Popup 클래스/prefab 존재 여부 확인.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] 6개 버튼 모두 OnClick 핸들러가 해당 Popup을 여는가? (현재 일부는 TODO)
     - Character / Inventory: 구현됨
     - Potion / Equipment / Skill / Bestiary: TODO 스텁 가능성
   - [ ] 6개 버튼 각각에 Color DOTweenAnimation (0.3s OutQuad TO #FFDA01) 부착?
   - [ ] 6개 버튼 각각에 Scale DOTweenAnimation (0.3s OutQuad TO 1.1) 부착?
   - [ ] 하이라이트 재생 트리거: 해당 Popup이 열릴 때 Restart, 닫힐 때 Rewind?
   - [ ] 해금 상태 시스템: `Managers.Game` 에 각 버튼 해금 여부 플래그 / 저장?
   - [ ] 첫 시작 시 [상태] 버튼만 해금 / 나머지 잠금 상태?
   - [ ] 잠긴 버튼 시각(어두운 오버레이 + 잠금 아이콘)?
   - [ ] 잠긴 버튼 클릭 시 무반응 또는 "해금 조건 안내" 대화?
   - [ ] 퀘스트 진행도에 따라 해금 조건이 데이터로 정의되어 있는가? (QuestData 또는 Define 상수)

3. **누락분만 구현**
   - OnClickX TODO 스텁을 실제 Popup 호출로 연결 (해당 Popup이 이미 있으면 `Managers.UI.ShowPopupUI<UI_XPopup>().SetInfo()`).
   - 하이라이트 애니메이션을 **각 버튼 prefab에 DOTweenAnimation 2개** (Color + Scale, AutoPlay Off) 추가. Trigger는 외부에서 `DOPlayById("Highlight")` / `DORewindById("Highlight")` 로 제어.
   - UI_GameScene에 `HighlightButton(int btnIndex, bool on)` 헬퍼 추가 → 각 OnClickX에서 `on=true`로 호출, Popup 닫힐 때 `on=false` 호출.
   - Popup 닫힘 감지는 `UIManager` 콜백 또는 `Managers.UI.OnPopupClosed += ...` 이벤트 사용.
   - 해금 시스템: `Managers.Game.UnlockedBtmButtons` 비트마스크 또는 bool 배열. `GameData`에 저장. `RefreshBtmButtonsLockState()` 헬퍼로 잠긴 버튼 오버레이 갱신 (슬라이드 35 잠금 시각 스펙 차용).
   - 잠긴 버튼 클릭 시 무반응 + SFX만 재생 (또는 안내 토스트 — 기획서에 명시 없음, 1차로는 무반응).

4. **결과 보고**
   - 점검 체크리스트 결과 + 수정된 OnClickX + 추가된 prefab DOTweenAnimation + 해금 시스템 개요 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (L21~ Images, L170~ Buttons 바인딩, L615~ OnClickX 핸들러)
- `Assets/@Scripts/Managers/Contents/GameManager.cs` (해금 상태 저장)
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_EquipmentPopup.cs` (존재 여부 확인)
- `Assets/@Scripts/UI/Popup/UI_SkillPopup.cs` (존재 여부 확인)
- `Assets/@Scripts/UI/Popup/UI_BestiaryPopup.cs` (존재 여부 확인)
- `Assets/@Resources/Prefab/UI/Scene/UI_GameScene.prefab` (BTMButtonGroup 노드)
