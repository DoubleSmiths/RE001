# 미니몬퀘스트 part2 기획서 ↔ RE001(C#) 갭 백로그

> 작성: 2026-06-26. 기준 = 기획서 part2(도감/미니몬기록/관심/잡기/능력) + RE001_Share(원본, Visual Scripting).
> 작업 대상 = RE001 (C# 포트, branch Optimization). 4개 분석 에이전트 팀 결과 종합.
> ⚠️ **이 환경엔 C# 컴파일러/Unity 빌드/LSP가 없음** → 코드 변경은 "정독 기준 안전"이며 최종 컴파일·플레이테스트는 Unity 에디터에서 필요.

전체 갭: **56건** (P0×4, P1×19, P2×18, P3×15). 도메인별 상세는 scratchpad gaps_01~04 참조.

---

## ✅ 이번 세션 적용 (리뷰어 APPROVE / 정독 검증 — Unity 컴파일·플레이테스트 필요)

`UI_BestiaryPopup.cs`:
1. **완성도 보라별 항시 idle Anim 활성** (도감#2/미니몬기록#8) — `ApplyCompletionStarAnim()` 본문 주석 해제. 필드·null가드·OnDestroy kill·호출부(open 419/등급상승 713) 모두 기존 존재.
2. **도감 동행 ICON 실시간 갱신** (도감#6) — `OnCompanionChanged` 구독 추가(기존 `OnInterestChanged` 패턴 대칭, OnDestroy 해제 포함).

`UI_TamePopup.cs`:
3. **잡기#4 시도횟수 표기 정정** — `시도/남은(ceil-tame)` → `시도/확정천장(ceil)`. "3/10번째에 확정" 스펙 부합. 미사용 `remaining` 변수 제거.
4. **잡기#10 만드라고라 포물선 버그 수정** — `DOAnchorPos`+`DOAnchorPosY`가 같은 anchoredPosition.Y 동시 구동 충돌 → `DOJumpAnchorPos` 단일 호(arc) 트윈으로 교체.

→ **요청: Unity에서 컴파일 + 도감 보라별 회전/동행 토글, 잡기 버튼 "시도/확정" 표기, 만드라고라 포물선 확인.**

---

## 🟥 당신의 설계 결정 필요 (이게 정해지면 코드 구현 가능)

| # | 항목 | 현재 동작 | 질문 | 출처 |
|---|---|---|---|---|
| D1 | 포획 성공 시 TameCount(천장 카운터) | 누적 유지(리셋 안함) | 포획 성공 시 0으로 리셋(재포획 시 다시 처음부터)? 아니면 누적 유지(한번 쌓은 천장 진척 유지)? | 잡기#4, MinMonManager.cs:134 |
| D2 | 별 4단계 승급 조건 | 3조건 독립 합산(기록도100+공명도100+AllSS = 만족 개수) | "한 단계씩 승급"이 순서 누적(1단계 먼저)인가 현재처럼 합산인가? | 미니몬기록#8, MinMonManager.cs:306 |
| D3 | 창고 가득 시 알림 | `UI_WarningPopup`(별도 prefab) | 스펙 관심#3은 `UI_AttactedPopup` 빨강 버전처럼 보임. 같은 팝업 색만 변경 vs 다른 팝업? | 관심#3, MonsterController.cs:202 |
| D4 | 기록도 중간단계(60%) | 2단계만(0%=전잠금 / >0=전해금) | 기록도별 해금 정보가 "몬스터마다 다름"인데, 단순 3-tier(0=??? / 0<x<100=기초정보O·드롭"?" / 100=전부)로 통일할지, 아니면 몬스터별 데이터 설계할지? | 미니몬기록#3 |
| D5 | 부분/완벽획득 표기 위치 | 미니몬기록 창에만 | 나의미니몬 창에도 동일 표기 넣을지? | 미니몬기록#4#5 |

---

## 🟧 코드 구현 가능 (다음 배치 — 당신 플레이테스트 전제)

- **[P1] 능력#3 능력공격 명중/회피 판정** — AbilityRunner.cs:122 BonusDamage가 raw(dice 없음). BattleMath.Resolve 경로화. ※완전판은 능력별 effect anim 타이밍 필요(아래 asset-blocked).
- **[P2] 확률발동 stateful 6종**(연속n치명타/연속n회피/사망공격/특정능력후 등) — AbilityRunner.cs:52 카운트형 상태추적 추가.
- **[P2] effect형(Heal/BonusDamage/Proc) 레벨 스케일** — AbilityManager.cs:258. EffectValue/ProcChance/Duration 레벨 미곱셈.
- **[P2] 능력 다중 Role 멀티스탯 효과** — ApplyTo StatType 단일 → 다중화.

(잡기#4 표기·잡기#10 포물선은 ✅ 적용 완료. 잡기 Lure 70/30은 🟥/ℹ️로 이동 — 효과 데이터 미정의.)

---

## 🟨 Unity 에디터/에셋 필요 (prefab 노드·스프라이트·머티리얼·애님클립·VFX)

- **[P1] 능력별 발동 VFX**(플레이어 위치, layer 앞/뒤) — VFX prefab 필요. 능력#4
- **[P3] Emoj_Shy 프리팹** 등록(관심#2), 능력 IconLabel 스프라이트(SkillData), CreatureData 미니몬 아이콘 프리로드(정보창#6 흰박스), MinMonData 2001/2002 IconLabel 비어있음(도감#4)
- **[P2] 나의미니몬 5등급 Text_ShineGold 머티리얼/Layer2 UIMask** — 폰트 아틀라스 깨짐 보류 중. 머티리얼 프리셋 재제작.
- **[P3] 능력정보창 정적 데모텍스트 제거**(prefab 노드), 전용 글로우노드(shop_frame_glow #636363, 슬52 2개추가)
- **[P3] 도감#4 별등급 숫자 표기**(마물상세정보 기획서 기준), 미니몬 동행 위치 미니몬별 오프셋(MinMonData 필드화)
- **검증 필요**: Popup OnDisable DoPlayBackwards 역재생, DOTweenAnimation autoPlay 노드 위치(미니몬기록#7 하단UI, 잡기#6 Loading)

---

## 🟦 선행 대형 시스템 의존 (part2 밖)

- **[P1] 능력#1 장비 능력 +9 영구화** (착용=무점유 기억/+9 도달 시 기술창 잔존+점유화/동일능력 2개+9 단계↑) — **장비 강화 시스템/대장간 UI** 가동 후 완결. AbilityManager.cs:156
- **[P1] 능력#1 전직(Class) 출처 능력 획득** — 전직 시스템 + PlayerStatusData.Class 연동 필요. 직업명 "여행자" 고정.
- **[P2] 능력정보창#7 능력단계표**(착용+강화+내구→레벨), #6/#7 다박스(2~5개) — 장비 영구화 연동.

---

## ℹ️ 오탐(코드 정독으로 확인 — 수정 불필요)

- **미니몬기록#3 기록도 단계별 ??? "2단계만"**: 오탐. `statsUnlocked=rec>0` + `lootUnlocked=rec>=100`로 **이미 3-tier**. rec0=전부???+IMG검정, 0<rec<100=기초정보O·드롭"?"(RefreshDropList unlocked=false), rec100=전부. (Agent가 lootUnlocked 라인 누락) → D4 해소, 변경 불필요. ※단 "몬스터별 stat 개별 해금 임계"는 데이터 설계 미구현(별건).
- **미니몬기록 [나의미니몬] 버튼 sync>0 판정**: `GetSyncRate = CaughtCount×0.25` → `sync>0 ⟺ CaughtCount>0 ⟺ IsOwned`. 수학적 동일 → 버그 아님.
- **도감#4 Interactive 미니몬보유 조건**: 보유 시점엔 사냥(AddKill)으로 이미 rec>0 → `rec==0 && 보유` 정상플레이 도달 불가. 현재 `rec>0` 충분. (강제 시 line148 "미발견" 렌더와 충돌)
- **잡기 Lure 70/30**: 현재 코드는 `success==효과有` 단일 롤(TryCatch) 모델. 스펙의 2단계(효과 판정 후 70/30) 구현은 **"효과 여부" 결정 데이터 소스가 미정의**(스펙 미명시) → 🟥 설계 필요. 현 단일롤은 플레이 가능한 단순화. Emoj_Dots 순서도 이 모델에 맞춰 "생각 텀 먼저" 의도적 선택(코드 주석 명시).
- **시간발동 쿨감소 규칙**: ContextOk 실패 시 continue로 사실상 충족. 경미.

---

## 이미 스펙대로 구현됨 (검증 완료, 참고)

도감: 보라별→좌우순환 등급상승+RotPtr저장, NEW뱃지, 기록도 3분류, 보라별 4단계 순차연출+Flash, 동행 frame색/ActiveEf, 별 항시Anim(box), 세이브 단일소스.
미니몬: 공명도25%×4=100%+영구습득, AllSS→Lv2, 별 CheckMaxStar, 능력치 6등급 outline, 부르기/동행 토글+자동교체, RollGrade(40/30/15/8/5/2), 재포획 Max상승+향상/실패 연출, 동행 자동공격(명중=플레이어,타격→콤보).
관심/잡기: RollInterest→창고순서저장→이벤트, 능력보너스 슬롯, 버튼 !/? 상태, 성공확률/GuaranteedCatch/만드라고라, 타이핑, Setup/Lure/성공실패/만드라고라 연출, 세이브.
능력: 기억력 점유/회복/재능, 4분류, 단계 StatBonus×Lv, 확률/시간 발동 프레임워크+훅, 버프아이콘 UI(BuffData시트), 능력목록 필터+게이지, 능력정보창 출처5종+미니몬/재료/장비박스+기억/망각 3상태.
