# [슬라이드 48] 전투 UI 기획서 #2 — 콤보 시스템

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 타격 정의
- 히트(hit) 수. 빗나가거나 회피 시 타격 아님.

### 콤보 진행
1. 타격 시 [공세 단계(콤보)] 상승.
2. 단계별 특정 상태 돌입 (무딘, 숙련된, ...).
3. 상태별 버프:
   - 공격속도 증가
   - 피해증폭률 증가
4. 상태마다 **타이머(ComboGuage)** 시간 + **타격별 획득 시간** 존재.
5. 타격마다 획득 시간 추가. **타이머 시간 초과 불가** (clamp).
6. 타이머 0 → **슬로우 모션 + 이펙트 + 초기화**.

### 상태 이름
- Lv0~Lv6 7단계 (슬라이드 47 ComboLv0~Lv6Text 대응).

## 작업 지시
1. **조사**
   - `PlayerController`/`ComboManager` 유무.
   - `OnHitLanded` 이벤트.
   - ComboGuage UI.
   - 단계별 데이터 구조.
   - 슬로우 + FX 연출.

2. **체크리스트**
   - [ ] 타격 (hit, miss/dodge 제외)?
   - [ ] `OnHitLanded` 이벤트?
   - [ ] ComboLevel 상태 (0~6)?
   - [ ] 단계별 데이터 JSON?
   - [ ] 타이머 0 초기화 + 슬로우 + FX?
   - [ ] 공격속도/피해증폭률 버프 적용?
   - [ ] ComboGuage value 반영?
   - [ ] ComboText/ComboLv#Text 갱신?

3. **누락분만 구현**
   - `ComboManager` 신설:
     - Level, Timer, HitGain[], MaxTimer[], Buff[]
     - `OnHit()` → Timer += HitGain, Clamp MaxTimer, 단계 상승.
     - Update Timer 감소, 0 → Reset + 슬로우 + FX.
     - 버프 이벤트로 Creature 스탯 반영.
   - ComboGuage Slider 바인딩.
   - 슬로우 FX 파티클.

4. **결과 보고**
   - 체크리스트 결과 + 파일 + 데이터 구조 설계.

## 관련 파일 힌트
- `Assets/@Scripts/Controllers/PlayerController.cs`
- `Assets/@Scripts/Managers/Contents/`
- `Assets/@Scripts/UI/Scene/UI_BattleScene.cs`
- `Assets/@Resources/Data/ComboData.json`
