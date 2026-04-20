# [슬라이드 14] 상태정보(Status) Popup #4 — 능력치 14종 정의

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 (텍스트 전용)

### 공통 규칙
- 괄호 `()` 안 수치는 **초기값**.
- **치명타/물리저항/마법저항/회피율**: 100%를 넘는 수치 **표기는 그대로** 하되 **적용은 100%까지만**.
- 표기 시 소수점은 괄호에 명시된 자리수까지만 반올림 → 적용/표기.
- 소수점 없으면 소수점 표기 X.

### 공격
1. 물리공격력 (10) — 물리타입 공격 피해량
2. 마법공격력 (10) — 마법타입 공격 피해량
3. 치명타 (0.00%) — 치명타 적중 확률, 적용 캡 100%
4. 치명타피해 (150%) — 치명타 배율 (1.5배)
5. 명중률 (80.0%) — 공격 적중 확률
6. 공격속도 (100%) — 기본공격 Anim Speed

※ **피해증폭률** (표시 안함) — 방어력/저항 감소 **전** 피해량에 곱.

### 수비
7. 최대체력 — 0 → 사망
8. 방어력 (0) — 최종 피해량 감소. 음수 불가.
9. 물리저항 (0.00%) — 물리 피해 % 감소, 캡 100%
10. 마법저항 (0.00%) — 마법 피해 % 감소, 캡 100%
11. 회피율 (0.00%) — 공격 회피 %, 캡 100%
12. 회복력 (0.0%) — 회복 효과 배율

### 보조 (플레이어 전용)
13. 성장률 (100%) — 레벨 경험치 획득 배율
14. 획득률 (100%) — 몬스터 드랍률 배율

※ 그 외 능력치는 몬스터에게도 동일 적용.

## 작업 지시
1. **조사**
   - `CreatureController` / `PlayerController` 에 14종 스탯 필드 유무.
   - `Data.Contents.cs` PlayerData/StatData 필드 유무.
   - `UI_CharacterPopup` 에서 14종 TMP 바인딩 유무.
   - 공식 `기본 * (증가율+1) * 증폭률` 적용 여부.
   - 치명타/저항/회피 cap 100% 로직.

2. **체크리스트**
   - [ ] 14종 스탯 존재?
   - [ ] 피해증폭률 필드 + 공식 반영?
   - [ ] 치명타/물저/마저/회피 cap 100%?
   - [ ] 소수점 반올림 유틸?
   - [ ] 성장률/획득률 플레이어 전용?
   - [ ] UI 14종 TMP 바인딩 + 실시간 갱신?
   - [ ] 공격/수비/보조 3섹션 그룹핑?

3. **누락분만 구현**
   - 빠진 스탯 필드 추가 + 이벤트.
   - cap 적용 (`Mathf.Min`).
   - 소수점 포맷 유틸 `FormatStat(float v, int decimals)`.
   - UI 바인딩/Localize 추가.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일.

## 관련 파일 힌트
- `Assets/@Scripts/Controllers/CreatureController.cs`
- `Assets/@Scripts/Controllers/PlayerController.cs`
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
