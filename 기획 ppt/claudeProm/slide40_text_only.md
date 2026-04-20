# [슬라이드 40] 물약 상세정보 Popup #4 — 제작 애니메이션

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### [제작] 버튼 조건
- 제작되지 않은 상태일 때만 Anim 재생.
- Anim 재생 중 모든 버튼 비활성.

### Anim 시퀀스
**STEP 1 — 창 전체 Shake/Position**
- Duration 1.1s / OutQuad
- TO (1.5, 0, 0) / Vibrato 21 / Randomness 0

**STEP 2 — 창 전체 Punch/Scale**
- Duration 0.3s / OutQuad
- TO (0.2, 0.2, 1) / Vibrato 4 / Elasticity 1

### 파티클
- `FX_splash_spark_floor` — 창 정가운데
- `FX_Potion Upgrade` — 물약 아이콘

### 완료 후
- 아이템 정보 갱신.

## 작업 지시
1. **조사**
   - `UI_PotionDetailPopup._craftBusy` + Sequence.
   - `UIFx.PlayBurst` 호출.
   - 완료 후 갱신 함수.
   - prefab DOTweenAnimation Shake/Punch 구성 가능성.

2. **체크리스트**
   - [ ] 제작 안된 물약만 Anim?
   - [ ] Anim 중 버튼 비활성?
   - [ ] Shake 1.1s OutQuad (1.5,0,0) v21 r0?
   - [ ] Punch 0.3s OutQuad (0.2,0.2,1) v4 e1?
   - [ ] FX_splash_spark_floor 중앙?
   - [ ] FX_Potion Upgrade 아이콘?
   - [ ] 완료 후 갱신?
   - [ ] 이미 제작된 경우 처리?
   - [ ] prefab DOTweenAnimation 이관 검토?

3. **누락분만 구현**
   - 코드 Sequence → prefab DOTweenAnimation 이관.
   - FX prefab Addressable Instantiate.
   - 재클릭 토스트/비활성.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs`
- `Assets/@Scripts/UI/UIFx.cs`
- `Assets/@Resources/Prefab/FX/FX_splash_spark_floor.prefab`
- `Assets/@Resources/Prefab/FX/FX_Potion Upgrade.prefab`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`
