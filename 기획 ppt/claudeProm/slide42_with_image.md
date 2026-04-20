# [슬라이드 42] 물약창 Popup #2 — 사용 / 쿨타임 / Finish Anim

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 42
- 첨부 이미지 (`기획 ppt/slide_images/slide42/`, 총 4개):
  - 자동물약 On/Off 예시, 물약 사용 시 쿨타임 Slider, HP Potion Ef 이펙트, Finish Anim

먼저 `기획 ppt/slide_images/slide42/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 물약 사용 규칙
1. **사용 시 재사용 대기 시간(쿨다운)에 따라 위에서 아래로 줄어드는 Slider 표시**.
   - Slider Fill color: `#FFFFFF` alpha 200
2. 사용 시 즉각 **사용된 물약에 해당되는 이펙트** 표시.
3. 재사용 대기 시간 중 물약 버튼 `interactable = false`.
4. 재사용 대기 시간이 끝나면 **Finish Anim** 재생.

### 자동물약
- **자동물약 사용 X**: 버튼 클릭 시 사용.
- **자동물약 사용 O**: 조건(HP Ratio 등)에 맞춰 **자동 사용** (+ 클릭 시도 사용 가능).

### HP 물약 사용 시 이펙트
- `HP Potion Ef` — 캐릭터 몸 한 가운데 생성.

### Finish Anim
- Image: Sprite(None), color `#FFFFFF` alpha 80
- DOTween Fade
  - Duration: 0.3s
  - Ease: InBack
  - TO: 0 (알파 0으로)

## 현재 구현 힌트 (memory)
- `UI_PotionPopup.UsePotion`: count-1, cooldown=cooldownMax, button interactable=false, cooldownBar 활성화(value=1).
- Update: `cooldown -= unscaledDeltaTime`, `cooldownBar.value = cooldown/cooldownMax` (1→0).
- `OnCooldownFinished`: Finish Anim image alpha 80/255 → `DOFade(0, 0.3s, InBack)` + 버튼 재활성.
- `SpawnUseFXForSlot`: HP 슬롯 `FxType.Heal`, Speed 슬롯 `FxType.Sparkle` (UIFx.PlayBurst로 대체 구현).
- 자동 트리거는 TODO (PlayerController HP 연동 미구현).

## 작업 지시
1. **조사**
   - `UI_PotionPopup.UsePotion`, `OnCooldownFinished` 로직.
   - `SpawnUseFXForSlot` 이펙트 스폰 위치.
   - Slider Fill color `#FFFFFF alpha 200` 설정 여부.
   - 자동 트리거 로직 (PlayerController HP Ratio 연동) 구현 여부.

2. **체크리스트**
   - [ ] 사용 시 count 감소 + cooldown 시작?
   - [ ] Slider Fill color `#FFFFFF alpha 200`?
   - [ ] 쿨다운 Slider 위→아래 (value 1→0)?
   - [ ] 쿨다운 중 interactable=false?
   - [ ] 쿨다운 종료 시 Finish Anim (Sprite None FFFFFF alpha 80 → DOFade 0.3s InBack TO=0)?
   - [ ] 버튼 재활성?
   - [ ] HP 물약: HP Potion Ef 캐릭터 몸 가운데 스폰?
   - [ ] 자동물약 ON + 조건 충족: 자동 사용?
   - [ ] 자동물약 ON 상태에서도 클릭 사용 가능?

3. **누락분만 구현**
   - 자동 트리거 로직: `PlayerController.OnHpChanged` 구독 → `_hpRatio * MaxHp` 비교 → UsePotion 자동 호출.
   - HP Potion Ef 파티클 스폰 위치 (캐릭터 몸 가운데: `PlayerController.transform.position + Vector3.up * bodyCenterOffset`).
   - Finish Anim은 prefab DOTweenAnimation Fade (Duration 0.3 InBack TO 0) 이관 가능.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionPopup.cs`
- `Assets/@Scripts/UI/UIFx.cs`
- `Assets/@Scripts/Controllers/PlayerController.cs` (OnHpChanged)
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionPopup.prefab`
- `Assets/@Resources/Prefab/FX/HP Potion Ef.prefab` (존재 여부 확인)
