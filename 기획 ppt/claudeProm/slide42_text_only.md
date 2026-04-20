# [슬라이드 42] 물약창 Popup #2 — 사용 / 쿨타임 / Finish Anim

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 물약 사용 규칙
1. 쿨다운 Slider (위→아래) 표시. Fill color `#FFFFFF` alpha 200.
2. 사용 시 즉각 이펙트 표시.
3. 쿨다운 중 버튼 `interactable=false`.
4. 쿨다운 끝 → Finish Anim.

### 자동물약
- OFF: 버튼 클릭 시 사용.
- ON: 조건 충족 시 자동 사용 + 클릭도 가능.

### HP 물약 이펙트
- `HP Potion Ef` — 캐릭터 몸 가운데.

### Finish Anim
- Image Sprite(None) `#FFFFFF` alpha 80
- DOTween Fade 0.3s InBack TO 0.

## 작업 지시
1. **조사**
   - `UI_PotionPopup.UsePotion` / `OnCooldownFinished`.
   - `SpawnUseFXForSlot` 위치.
   - Slider Fill 색.
   - 자동 트리거 (HP Ratio).

2. **체크리스트**
   - [ ] count 감소 + 쿨다운 시작?
   - [ ] Fill `#FFFFFF alpha 200`?
   - [ ] Slider value 1→0?
   - [ ] 쿨다운 중 interactable=false?
   - [ ] Finish Anim Fade 0.3s InBack TO 0?
   - [ ] 버튼 재활성?
   - [ ] HP Potion Ef 캐릭터 몸?
   - [ ] 자동물약 ON 자동 사용?
   - [ ] 자동 ON 상태 클릭 가능?

3. **누락분만 구현**
   - HP Ratio 기반 자동 트리거.
   - HP Potion Ef 스폰 위치.
   - Finish Anim prefab DOTweenAnimation 이관.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Scripts/UI/UIFx.cs`
- `Assets/@Scripts/Controllers/PlayerController.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionPopup.prefab`
- `Assets/@Resources/Prefab/FX/HP Potion Ef.prefab`
