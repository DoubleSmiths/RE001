# [슬라이드 40] 물약 상세정보 Popup #4 — 제작 애니메이션

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 40
- 첨부 이미지 (`기획 ppt/slide_images/slide40/`, 총 4개):
  - 제작된 상태/안된 상태 예시, FX_splash_spark_floor, FX_Potion Upgrade, 제작 Anim 연출

먼저 `기획 ppt/slide_images/slide40/` 내 모든 이미지를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### [제작] 버튼 클릭 시 조건
- **해당 물약이 제작되어 있지 않은 상태** 일 때만 제작 Anim 재생.
- **이미 제작된 상태**면 별도 흐름 (기획서에 명시 안됨 — 1차로는 재생 안함 또는 토스트).
- **Anim 재생 중 모든 버튼 비활성화**.

### Anim 시퀀스

**STEP 1 — [창 전체 Shake/Position]**
- Duration: 1.1s
- Ease: OutQuad
- TO: `(x=1.5, y=0, z=0)`
- Vibrato: 21, Randomness: 0

**STEP 2 — [창 전체 Punch/Scale]** (Shake 끝난 뒤)
- Duration: 0.3s
- Ease: OutQuad
- TO: `(x=0.2, y=0.2, z=1)`
- Vibrato: 4, Elasticity: 1

### 파티클
- `FX_splash_spark_floor` — 물약 상세정보창 **정 가운데**.
- `FX_Potion Upgrade` — 물약 **아이콘** 부분에.

### 완료 후
- DOTween Anim 종료 시 **모든 아이템 정보 갱신** (다음 단계/등급 데이터로).

## 현재 구현 힌트 (memory)
- `UI_PotionDetailPopup._craftBusy` 플래그로 모든 버튼 비활성.
- 현재 Shake `DOShakeAnchorPos(1.1, (1.5,0,0), 21)` → Punch `DOPunchScale((0.2,0.2,1), 0.3, 4, 1)` 코드 Sequence 존재.
- FX는 UIFx.PlayBurst (Splash + Sparkle) 로 대체 구현됨.

## 작업 지시
1. **조사**
   - `UI_PotionDetailPopup._craftBusy` + Sequence 로직 (L470 근처).
   - `UIFx.PlayBurst` 호출 로직 (L440~).
   - 완료 후 `RefreshAll()` 또는 등가 갱신 함수.
   - prefab에 Shake/Punch DOTweenAnimation 구성 가능 여부 (Shake는 DOTweenAnimation이 지원).

2. **체크리스트**
   - [ ] 제작되지 않은 물약에만 제작 Anim 재생?
   - [ ] Anim 재생 중 모든 버튼 비활성화?
   - [ ] Shake 1.1s OutQuad TO (1.5, 0, 0) Vibrato 21 Rand 0?
   - [ ] Punch 0.3s OutQuad TO (0.2, 0.2, 1) Vibrato 4 Elasticity 1?
   - [ ] FX_splash_spark_floor (정 가운데) 스폰?
   - [ ] FX_Potion Upgrade (아이콘) 스폰?
   - [ ] Anim 종료 후 아이템 정보 갱신?
   - [ ] 이미 제작된 물약 재클릭 시 처리?
   - [ ] prefab DOTweenAnimation Shake/Punch 이관 검토?

3. **누락분만 구현**
   - 코드 Sequence → prefab DOTweenAnimation Shake + Punch 이관 (가능 시).
   - FX prefab 없으면 `FX_splash_spark_floor`, `FX_Potion Upgrade` Addressable 확인 후 Instantiate.
   - 이미 제작된 물약 재클릭 시 토스트/버튼 비활성 추가.

4. **결과 보고**
   - 체크리스트 결과 + 파일/prefab.

## 관련 파일 힌트
- `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs` (L440~ FX, L470~ Sequence)
- `Assets/@Scripts/UI/UIFx.cs` (Splash / Sparkle 프리셋)
- `Assets/@Resources/Prefab/FX/FX_splash_spark_floor.prefab`
- `Assets/@Resources/Prefab/FX/FX_Potion Upgrade.prefab`
- `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`
