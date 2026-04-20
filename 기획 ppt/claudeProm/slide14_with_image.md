# [슬라이드 14] 상태정보(Status) Popup #4 — 능력치 14종 정의

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 14
- 첨부 이미지:
  - `기획 ppt/slide_images/slide14/slide14_shape000_Google_Shape_284_p9.png` — 상태정보 Popup 전체 + 능력 상세정보 확장 UI (공격/수비/보조 3섹션에 14종 능력치 배치)

먼저 Read로 위 이미지를 열어 각 능력치의 배치/섹션/기본값을 확인해.

## 공통 규약
- **CLAUDE.md** / **DOTween** 기존 DOTweenAnimation 우선 / **사용자 Unity 종료 금지**.

## 기획 스펙 요약

### 능력치 공통 규칙
- 괄호 `()` 안 수치는 **초기 설정 수치**.
- **치명타/물리저항/마법저항/회피율**: 100%를 넘는 수치 **표기는 그대로** 하되, **적용은 100%까지만**.
- 표기 시 소수점이 생기면 **괄호 안 표기된 소수점 자리수까지만 반올림**하여 표기/적용.
- 소수점이 없을 경우 소수점 표기 X.

### 능력치 14종 + 피해증폭률

**[공격 — Attack]**
| # | 이름 | 기본 | 설명 |
|---|---|---|---|
| 1 | 물리공격력 | `10` | 물리타입 공격 피해량에 영향 |
| 2 | 마법공격력 | `10` | 마법타입 공격 피해량에 영향 |
| 3 | 치명타 | `0.00%` | 공격이 치명타로 적중할 확률 (적용 캡 100%) |
| 4 | 치명타피해 | `150%` | 치명타 시 배율 (150% = 1.5배) |
| 5 | 명중률 | `80.0%` | 공격 적중 확률 |
| 6 | 공격속도 | `100%` | 기본공격 1회 Anim Speed |

※ **표시하지 않지만 피해증폭률 존재** — 방어력과 저항에 감소되기 **전** 피해량에 곱.

**[수비 — Defense]**
| # | 이름 | 기본 | 설명 |
|---|---|---|---|
| 7 | 최대체력 | — | 0 → 사망 |
| 8 | 방어력 | `0` | 최종 피해량에서 수치만큼 감소. 최종 음수 불가 |
| 9 | 물리저항 | `0.00%` | 물리 피해량 감소 % (캡 100%) |
| 10 | 마법저항 | `0.00%` | 마법 피해량 감소 % (캡 100%) |
| 11 | 회피율 | `0.00%` | 모든 공격 회피 % (캡 100%) |
| 12 | 회복력 | `0.0%` | 체력회복 효과 배율 |

**[보조 — Support]** — **플레이어 전용 속성**
| # | 이름 | 기본 | 설명 |
|---|---|---|---|
| 13 | 성장률 | `100%` | 레벨 경험치 획득 배율 |
| 14 | 획득률 | `100%` | 몬스터 드랍률 배율 |

※ 그 외 능력치는 **몬스터에게도 동일 적용**. 성장률/획득률만 플레이어 전용.

## 작업 지시
1. **조사**
   - `Assets/@Scripts/Controllers/CreatureController.cs` / `PlayerController.cs` 에 14종 Stat property/필드 존재 여부.
   - `Data.Contents.cs` 의 PlayerData/StatData에 14종 스펙 필드 유무.
   - `UI_CharacterPopup.cs` 에서 14종 수치 표기 TMP_Text 바인딩 유무.
   - 피해 계산 공식: `기본 * (증가율+1) * 증폭률` (슬라이드 15 연계).
   - 치명타/저항/회피율 cap 100% 적용 로직 유무.

2. **체크리스트**
   - [ ] 14종 스탯이 `CreatureController` (또는 `Stat`) 에 모두 있는가? (물공/마공/치명타/치피/명중/공속/최체/방어/물저/마저/회피/회복/성장/획득)
   - [ ] 피해증폭률(amplification) 필드 존재? 공식에 반영?
   - [ ] 치명타/물저/마저/회피율 cap 100% 처리?
   - [ ] 소수점 표기/반올림 유틸 존재?
   - [ ] 성장률/획득률은 플레이어 전용 플래그?
   - [ ] UI_CharacterPopup 하단 상세정보에 14종 TMP_Text 바인딩 + 실시간 갱신?
   - [ ] 공격/수비/보조 3섹션으로 UI 그룹핑?

3. **누락분만 구현**
   - 빠진 스탯 필드 추가 (public property + backing field + 이벤트).
   - cap 적용 함수 `Mathf.Min(value, 1f)` 유틸 or property setter.
   - 소수점 포맷 유틸: `FormatStat(float v, int decimals)`.
   - UI 바인딩 / Localize 키 추가.

4. **결과 보고**
   - 체크리스트 결과 + 수정 파일 목록 (CreatureController, StatData, UI_CharacterPopup 등).

## 관련 파일 힌트
- `Assets/@Scripts/Controllers/CreatureController.cs` (Stat 필드)
- `Assets/@Scripts/Controllers/PlayerController.cs`
- `Assets/@Scripts/Data/Data.Contents.cs`
- `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs`
- `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`
