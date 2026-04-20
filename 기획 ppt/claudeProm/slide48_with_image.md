# [슬라이드 48] 전투 UI 기획서 #2 — 콤보 시스템

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 48
- 첨부 이미지 (`기획 ppt/slide_images/slide48/`, 총 1개):
  - `slide48_shape000_그림_12.gif` — 콤보 시스템 연출 GIF (공세 단계 변화, 게이지, 타이머)

먼저 `기획 ppt/slide_images/slide48/slide48_shape000_그림_12.gif` 를 Read로 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 용어 정의
- **[타격]**: 어떠한 공격행위가 아닌 **히트(hit)** 수.
  - 빗나가거나 적이 회피 시 타격으로 치지 않음.

### 콤보 진행 규칙
1. 플레이어가 적을 **타격할 때마다 [공세 단계(콤보)]가 오름**.
2. 공세 단계에 따라 특정 상태 (무딘, 숙련된, ...) 에 **돌입**.
3. 돌입한 특정 상태에 따라 플레이어는 긍정적 효과:
   - a. 공격속도 증가
   - b. 피해증폭률 증가
4. 특정 상태마다 **타이머(ComboGuage) 시간**과 **타격별 획득 시간**이 각각 다름.
5. 플레이어는 타격마다 돌입한 특정 상태의 타격별 획득 시간을 얻음.
   - 이 시간은 **해당 상태의 타이머 시간을 초과할 수 없음** (clamp max).
6. **타이머 시간이 0이 되면** 잠깐의 슬로우 모션 + 이펙트와 함께 **모든 공세 단계와 특정 상태를 잃음** (초기화).

### 상태 이름 (기획서 예시)
- 무딘, 숙련된, ... (구체 명칭/단계 수는 슬라이드 47의 `ComboLv0Text ~ ComboLv6Text` 7단계로 추정)

## 작업 지시
1. **조사**
   - `PlayerController` 또는 `ComboManager` 유무.
   - 타격 이벤트 (`OnHitLanded`) 유무.
   - ComboGuage UI 오브젝트 (Slider 또는 Image Fill) 유무.
   - 공세 단계별 데이터 (타이머 시간, 획득 시간, 버프 값) 구조.
   - 슬로우 모션 + 이펙트 연출 유무 (Time.timeScale 조정 + FX).

2. **체크리스트**
   - [ ] 타격 정의 (hit, miss/dodge 제외)?
   - [ ] `OnHitLanded` 이벤트?
   - [ ] ComboLevel 상태 변수 (0~6)?
   - [ ] 단계별 타이머 / 획득 시간 / 버프 데이터 (JSON 또는 Define)?
   - [ ] 타이머 0 → 초기화 + 슬로우 + FX?
   - [ ] 공격속도 버프 적용?
   - [ ] 피해증폭률 버프 적용?
   - [ ] ComboGuage UI value 실시간 반영?
   - [ ] ComboText/ComboLv#Text TMP 갱신?

3. **누락분만 구현**
   - `ComboManager` 신설 (또는 PlayerController 확장):
     - `Level`, `Timer`, `HitGain[level]`, `MaxTimer[level]`, `Buff[level]` 데이터
     - `OnHit()` → Timer += HitGain[level], Clamp to MaxTimer, 단계 상승 체크.
     - Update에서 Timer 감소, 0 → Reset + 슬로우(Time.timeScale=0.3 0.3s) + FX.
     - 공격속도/피해증폭률 이벤트로 Creature 스탯에 반영.
   - ComboGuage UI Slider value 바인딩 + 단계 이름 텍스트 매핑.
   - 슬로우 FX 파티클 prefab Addressable.

4. **결과 보고**
   - 체크리스트 결과 + 파일 + 콤보 데이터 구조 설계.

## 관련 파일 힌트
- `Assets/@Scripts/Controllers/PlayerController.cs`
- `Assets/@Scripts/Managers/Contents/` (ComboManager 신설 후보)
- `Assets/@Scripts/UI/Scene/UI_BattleScene.cs`
- `Assets/@Resources/Data/ComboData.json` (신설 후보)
