# [슬라이드 10] 최하단 버튼 (6종 탭: 상태 / 가방 / 물약 / 장비 / 기술 / 도감)

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다.
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**.
  - 단일 트윈은 prefab의 `DOTweenAnimation` 우선.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 스펙 (이미지 없이 텍스트로 완전 기술)

### 전체 규칙
1. 최하단 버튼 총 **6종**.
2. 순서 **고정**: 상태 → 가방 → 물약 → 장비 → 기술 → 도감 (좌→우).
3. 게임 첫 시작: **[상태]만 해금**, 나머지는 **퀘스트 진행도에 따라 순차 해금**.
4. 각 버튼 클릭 시 연관된 Popup 호출.

### 버튼 ↔ Popup 매핑
| 버튼 | 호출 Popup |
|---|---|
| 상태 | `UI_CharacterPopup` |
| 가방 | `UI_InventoryPopup` |
| 물약 | `UI_PotionPopup` |
| 장비 | `UI_EquipmentPopup` |
| 기술 | `UI_SkillPopup` |
| 도감 | `UI_BestiaryPopup` |

### 잠금 상태
- 해금되지 않은 버튼은 어두운 오버레이 + 잠금 아이콘 (슬라이드 35 잠금 시각 스펙 차용 제안).
- 잠긴 버튼 클릭 시 무반응(또는 SFX만).

### 하이라이트 Anim (버튼 클릭 → Popup 열림 동안)
- **Color**: Duration 0.3s, Ease OutQuad, TO `#FFDA01`
- **Scale**: Duration 0.3s, Ease OutQuad, TO 1.1 (Uniform)
- Popup 닫힘 시 역재생 (원색 / Scale 1.0).

## 작업 지시
1. **현재 구현 상태 조사**
   - `UI_GameScene.cs`:
     - Images enum: 6개 버튼 아이콘 확인.
     - L170~ 6개 버튼 BindEvent (이미 존재).
     - OnClickX 핸들러 — TODO 스텁 유무 확인 (Potion/Equipment/Skill/Bestiary).
   - prefab의 6개 버튼에 Color/Scale DOTweenAnimation 유무.
   - `Managers.Game` 해금 상태 플래그 유무.
   - 6개 Popup 클래스/prefab 존재 여부.

2. **기획서 대비 누락 점검 체크리스트**
   - [ ] 6개 OnClickX 핸들러가 모두 Popup을 여는가? (일부 TODO)
   - [ ] 각 버튼에 Color DOTweenAnimation (0.3s OutQuad TO #FFDA01)?
   - [ ] 각 버튼에 Scale DOTweenAnimation (0.3s OutQuad TO 1.1)?
   - [ ] Popup 열림/닫힘에 따라 Highlight Restart/Rewind 연동?
   - [ ] 해금 상태 저장 시스템 (`GameData.UnlockedBtmButtons` 등)?
   - [ ] 첫 시작 시 [상태]만 해금?
   - [ ] 잠긴 버튼 시각(어두운 오버레이 + 잠금 아이콘)?
   - [ ] 잠긴 버튼 클릭 시 무반응?
   - [ ] 퀘스트 진행도 ↔ 해금 매핑 데이터?

3. **누락분만 구현**
   - OnClickX TODO → 실제 Popup 호출로 연결.
   - 하이라이트는 각 버튼 prefab에 DOTweenAnimation 2개 (Color + Scale, AutoPlay Off) 추가. `HighlightButton(int idx, bool on)` 헬퍼로 외부 제어.
   - Popup 닫힘은 `Managers.UI.OnPopupClosed` 이벤트 또는 UIManager 콜백으로 감지.
   - 해금: `GameData.UnlockedBtmButtons` 비트마스크 / bool 배열, `RefreshBtmButtonsLockState()` 갱신. 잠금 시각은 슬라이드 35 스펙 차용.

4. **결과 보고**
   - 체크리스트 결과 + 수정 OnClickX + prefab 애니메이션 + 해금 시스템 요약.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (L21~ Images, L170~ 6개 버튼 바인딩, L615~ OnClickX)
- `Assets/@Scripts/Managers/Contents/GameManager.cs`
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Scripts/UI/Popup/UI_EquipmentPopup.cs` (존재 여부 확인)
- `Assets/@Scripts/UI/Popup/UI_SkillPopup.cs` (존재 여부 확인)
- `Assets/@Scripts/UI/Popup/UI_BestiaryPopup.cs` (존재 여부 확인)
- `Assets/@Resources/Prefab/UI/Scene/UI_GameScene.prefab`
