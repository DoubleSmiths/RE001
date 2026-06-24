# VS 매크로 → C# 이식 갭 분석 (RE001_Share → RE001)

> 원본 프로토타입 `RE001_Share`(Visual Scripting/Bolt, `Assets/Macro/*.asset` 71개)와 C# 이식본 `RE001`(`Assets/@Scripts`)을 대조.
> **방침**: RE001은 VS 미사용 — 로직=C#, 연출=DOTweenAnimation. 따라서 아래 **미이식/부분이식 = C# 신규 구현 백로그**.
> 작성 2026-06-24. (VS 매크로는 동작 스펙 참조용. 변수/이벤트명은 C#에서 다수 리네임됨.)

---

## 배치 1 — 전투 / FX / 엔티티 (28종)

| 매크로 | 목적 | 상태 | 근거(RE001) |
|---|---|---|---|
| BattleRelate/BaseDMG_Cal | BASE 데미지(공격력×배율, 타입분기) | 이식됨 | BattleMath.cs:47 |
| BattleRelate/CRITCheck | 치명타 RNG→배수 | 이식됨 | BattleMath.cs:41,47 |
| BattleRelate/DMG_RNG | 최종 데미지 분산(0.8~1.2)+물/마 분기 | 이식됨 | BattleMath.cs:56,40,51 |
| BattleRelate/MissCheck | 명중−회피 빗나감 | 이식됨 | BattleMath.cs:34-36 |
| BattleRelate/MonsterDEF_RES_Check | 방어·물/마저항 차감 | 이식됨 | BattleMath.cs:51-53 |
| BattleRelate/skill/BeforeHit_Skills | 피격 전 스킬 디스패처 | **미이식** | 없음 |
| BattleRelate/skill/DoubleAttack | 확률 2회 타격 | **미이식** | 없음 |
| BattleRelate/skill/Smash | 확률 ×1.5 + SFX/이펙트 | **미이식** | 없음 (skillMul 훅만 미사용) |
| BattleRelate/textShow/BasicATTTextShow | 일반 데미지 숫자 | 이식됨 | DamageFont.cs(Normal), ObjectManager.ShowDamageFont |
| BattleRelate/textShow/CRITTextShow | 치명타 숫자+스타일 | 이식됨 | DamageFont.cs:65 |
| BattleRelate/textShow/MissText &Sound | MISS 텍스트+사운드 | **부분** | 텍스트 O / **미스 사운드 X** |
| BattleRelate/textShow/PhysicalTextShow | 물리 속성색 데미지 | **부분** | 데미지폰트 O / **속성색 모드 없음** |
| BattleRelate/textShow/WaterTextShow | 물(마법) 속성색 데미지 | **미이식** | 없음 |
| Basic_Hit_FX | 피격 FX 활성/타이머 | 이식됨 | ObjectManager.cs:175,182 |
| Basic_Hit_FX_Child | 피격 FX 위치 랜덤 | 이식됨 | ObjectManager.cs:178,185 |
| SFX Macro_RE | SFX clip/pitch 세팅 후 재생 | **부분** | SoundManager 골격만, **Play 전부 주석=실재생 X** |
| Emoj | OnEnable Animator 1회 재생 | 이식됨 | MonsterController.cs:219 |
| ApplyAtt | 장비/속성 능력치 적용 | 이식됨(추정) | PlayerController.RefreshStats, MonsterController.SetInfo |
| Add Kill Count | 처치수+1, 상한 분기 | 이식됨 | MinMonManager.cs:237 |
| Player Macro | 플레이어 상태머신+애니 | 이식됨 | PlayerController(State/Engage/Battle) |
| Monster State | 몬스터 상태머신+스탯초기화 | 이식됨 | MonsterController |
| Monster State_Anchored | UI(스크린스페이스) 몬스터+Money | **미이식** | 없음(월드 방식만) |
| Monster Scrolling | 몬스터 좌측 스크롤 | 이식됨 | MonsterController.cs:99 |
| BG Scrolling | 배경 스크롤+wrap | 이식됨 | BackgroundScroller.cs:78 |
| Background On_Off | 배경 토글 | **부분** | EnterAreaWithFade BG 교체로 대체 |
| Background State | 배경 스크롤 시작/정지 | 이식됨 | BackgroundScroller.IsScrolling |
| Ground Scrolling | 지면 스크롤+wrap | 이식됨 | BackgroundScroller(_layerSpeeds) |
| Ground State | 지면 스크롤 시작/정지 | 이식됨 | BackgroundScroller.IsScrolling |

## 배치 2 — 장비 / 인벤 / 아이템 / 스킬 / 물약 (23종)

| 매크로 | 목적 | 상태 | 근거(RE001) |
|---|---|---|---|
| Equip Arrow | 장비창 화살표로 슬롯 전환 | **미이식** | 장비창 UI 부재(장착 로직만 GameManager.cs:496 EquipItem) |
| Equip Icon | 장비 슬롯 아이콘/등급 글로우 | **미이식** | 장비 슬롯 UI 부재 |
| Equip Stat Cal | 장비 기본스탯+강화단계+내구도로 스탯 산출 | **미이식** | 스탯 계산 로직 없음. GameManager.cs:543 `Durability=MaxDurability` 스텁뿐. 미구현 TODO 명시(IntegrationTestMenu.cs:136) |
| Equip Stat Cal After Enh | 강화 적용 후 스탯 재계산 | **미이식** | 강화 UI(T1→T3)·대장간 미구현(ForgeEquipment 로직만 존재) |
| EquipSkill Text | 장비 고유스킬 텍스트 | **미이식** | 장비 고유스킬 데이터/UI 부재 |
| Selected Equip Kind Name | 장비 종류 인덱스→표시명 | **미이식** | EquipSlotType enum만, 이름 매핑 소비처 없음 |
| Inv Imp-Item Amount Check | 중요(퀵)아이템 보유수 | **부분** | UI_ImportantSlot.cs:82(Count 공유). 중요 전용 경로 없음(TODO UI_InventoryPopup.cs:219) |
| Inv Item Amount Check | 인벤 아이템 보유수 | 이식됨 | GameManager.cs:594 GetItemCount, UI_InvSlot.cs:88 |
| Inv Item Info | 아이템 상세(이름/등급명+색/설명/장소/보유수)+현지화 | 이식됨 | UI_ItemPopup.cs:100-131, 등급색 :164 |
| Inventory Slot | 슬롯(아이콘/수량/등급색/NEW/block/클릭) | 이식됨 | UI_InvSlot.cs:77-131 |
| Choose Item Box | 사용/제작용 아이템 선택 슬롯(필요수량 lime/red, UseItemPopupShow) | **미이식** | UseItemPopupShow·아이템선택→사용팝업 흐름 전부 없음 |
| Use Item | 아이템 소비(UseAmount 차감) | **부분** | GameManager.cs:577 RemoveItem 인프라만. 유저 "사용" 경로 없음(소비=제작/퀘스트만) |
| Loot Display Box | 루팅 표시(아이콘/+N/글로우/플래시) | 이식됨 | UI_BattleDropItem.cs, 생성 UI_BattleBtnGroup.cs:589-607 |
| Monster Item Drop Box | 도감 드롭행(아이콘/확률%/킬카운트 점진공개) | **부분** | UI_MonsterItemDropListBox.cs:51-98. **킬카운트 단계공개 없음**→기록도 100% 일괄 해금 |
| Monster Money Drop Box | 도감 골드 드롭행(MoneyAmount/Chance/Reveal%) | **미이식** | Money 드롭 박스·로직 전무(드롭은 아이템만) |
| Req Item Box | 제작 필요재료 박스(보유/필요·부족 빨강) | 이식됨 | UI_PotionDetailPopup.cs:601-642 (lime 미사용, 박스→ItemPopup 미연결) |
| Skill Level Text | 스킬 레벨 "현재/최대"(MAX·현지화) | **부분** | 레벨 표시는 능력 시스템 재구현 UI_SkillBookSkill.cs:102. VS식 current/max·MAX 분기는 미대응(등급/기억력 모델) |
| Skill Lvl_ReqItem | 스킬 레벨업 필요재료(소비) | **부분** | ReqItemBox 패턴(UI_SkillDetailPopup 슬49)은 있으나 **스킬 레벨업 재료소비 로직 미구현**(능력=기억력 점유 모델 :666) |
| Skill_List_AttackDefenseIcon | 스킬목록 칸(공/방 변형) | **부분(재설계)** | 능력/기억력 시스템으로 치환. 공/방 구분 슬롯 분류 없음 |
| Skill_List_AttackSkillIcon | 스킬목록 칸(공격 변형) | **부분(재설계)** | 동상. 아이콘/클릭/레벨은 옮겨졌으나 분류 없음 |
| Skill_List_SkillIcon | 스킬리스트 칸(아이콘/ON·OFF/클릭) | **부분(재설계)** | UI_SkillBookSkill.cs:67-135. VS `Skill_ON_OFF` 활성토글 → 능력 기억/망각(OnClickMemorize)으로 의미 다르게 대체 |
| PotionCraft_ReqItem | 물약 제작 필요재료(부족 빨강) | 이식됨 | UI_PotionDetailPopup.cs:601-642, 소비 GameManager.cs:664-670 |
| Potion Cooldown | 물약 쿨다운 슬라이더 | 이식됨 | UI_PotionPopup.cs:71-97,199-230 (cooldownBar/finishAnim) |

## 배치 3 — 퀘스트 / 도감 / UI / 언어 (20종)

| 매크로 | 목적 | 상태 | 근거(RE001) |
|---|---|---|---|
| Beastiary Box | 도감 박스(별/공명도/처치수/소환) | 이식됨 | UI_BestiaryBox, UI_BestiaryPopup, UI_MinionInfoPopup |
| Req Minion Box | 스킬 필요 미니몬 박스 | 이식됨 | UI_SkillDetailPopup.cs:1205-1273 |
| Poten Slot | 소울 필요 잠재 장착 슬롯 | **미이식** | 없음(소울코스트/FindReqSoul 0건) |
| Quest Follow UI | HUD 메인퀘 추적 | 이식됨 | UI_GameScene:746-783, QuestManager |
| Quest Item Check | 필요아이템 충족→진행·저장 | 이식됨 | QuestManager:145-217 |
| Quest Marker | isOnPlace 마커 토글 | **미이식** | 없음 |
| StoryFollowup | 스토리별 대화/오브젝트 토글 | **부분** | SubQuest/대화선택만, 씬오브젝트 on/off 테이블·targetSetup 없음 |
| Unlock_Tuto | 진행 후 튜토 노출 | **부분** | 팝업·트리거 O, Story>=N 수치게이트 아님(unlock-event 구동) |
| MapSelect_CurrentLocationSet | 현재위치+마커 이동 | **부분** | 위치텍스트·선택프레임 O, NowPlace 마커 reparent 없음 |
| Path_INFO | 길드/지역 게이지·색상 패널 | **미이식** | 없음(Guild/Gauge 0건) |
| Btm_UI_Change | 하단탭 in/out 애니 | **부분** | 하이라이트(DOScale/DOColor)로 대체, in/out 코루틴 없음 |
| Btm_UI_Text | DORestartById 텍스트 애니 | **미이식** | 없음(정적 텍스트) |
| BtnSelectSound | 버튼클릭 SFX | **미이식** | PlayButtonClick 정의만, 호출처 없음+SFX stub |
| Duri Text | 장비 내구도 색상텍스트 | **미이식/부분** | 데이터/이벤트만, UI는 stub(UI_ItemPopup) |
| Font Setting_DialogueName | 대화 이름 폰트 | 이식됨 | UI_DialoguePopup:90, FontManager |
| Font Setting_DialogueText | 대화 본문 폰트 | 이식됨 | UI_DialoguePopup:88 |
| Font Setting_UI-Text | 언어별 UI폰트 | 이식됨 | UI_Base.ApplyFont, FontManager:33 |
| Select Language | 언어 전환+폰트 | 이식됨 | UI_SettingPopup:137-149, GameManager:321 |
| UI Language | 언어별 텍스트 룩업 | 이식됨 | Util.Localize, DataManager(LocalizationDic) |
| UI New Txt | "New" 뱃지 play/pause | **부분** | SetActive 토글만, DOTween 애니 없음 |

---

## ★ 미구현 백로그 (미이식 + 부분이식 종합)

### ✅ 이번 세션 완료
- **SFX C# 이식** — `SFX Macro_RE / MissText&Sound / BtnSelectSound` 갭 해소. SoundManager 정식화(주석 해제+게이트) + 65클립 어드레서블(PreLoad) 등록 + 전투/물약/미니몬/UI 트리거 + **CreatureData 데이터 드리븐 몬스터 사운드**(AttackSfx/OnHitSfx/DieSfx).

### 미이식 (신규 C# 구현 필요)
1. **전투 스킬 시스템** — `DoubleAttack`(확률 2회), `Smash`(확률 ×1.5), `BeforeHit_Skills`(디스패처). 실행로직(SkillBase/DoSkill) 부재, `BattleMath.Resolve(skillMul)` 훅만 있음.
2. **속성 데미지 텍스트** — `WaterTextShow`(물/마법 파랑). `DamageFontMode`에 속성 모드 추가 필요.
3. **장비 시스템 (전체)** — `Equip Arrow/Icon/Stat Cal/Stat Cal After Enh/EquipSkill Text/Selected Equip Kind Name`(6종). 데이터/로더/시트파서는 갖췄으나(능력 "필요장비" 슬49 참조용으로만), **장착·스탯계산·강화·대장간 UI는 부재**(IntegrationTestMenu.cs:136 미구현 TODO 명시, GameManager.cs:543 내구도 스텁뿐). (대형)
4. **Choose Item Box** — 사용/제작용 아이템 선택 슬롯(UseItemPopupShow·아이템선택→사용팝업 흐름 부재).
5. **Monster Money Drop Box** — 도감 골드 드롭행(Money 드롭 박스·로직 전무).
6. **Poten Slot** — 소울 필요 잠재 장착 슬롯.
7. **Path_INFO** — 길드/지역 정보 패널(게이지·색상).
8. **Quest Marker** — isOnPlace 위치 마커 토글.
9. **Monster State_Anchored** — UI(스크린스페이스) 몬스터 변형 + Money 보상.
10. **Btm_UI_Text** — DORestartById 하단 텍스트 애니.
11. **Duri Text** — 장비 내구도 색상텍스트(데이터/이벤트만 존재, UI stub).

### 부분이식 (보강 필요)
- **PhysicalTextShow** — 데미지폰트에 속성별 색 모드 추가(현재 Normal/Crit/Miss/Heal만).
- **Use Item** — `RemoveItem` 소비 인프라만, 유저 "사용" 경로 없음(소비=제작/퀘스트만).
- **Inv Imp-Item Amount Check** — 중요아이템 전용 경로 없음(Count 공유, TODO).
- **Monster Item Drop Box** — 킬카운트 점진공개 없음(기록도 100% 일괄 해금).
- **Skill_List 계열(아이콘 3종·Skill Level Text·Skill Lvl_ReqItem)** — **"능력/기억력 시스템"으로 재설계**됨. 아이콘/클릭/레벨 표시는 옮겨졌으나 VS의 `Skill_ON_OFF` 활성토글·current/max 레벨텍스트·레벨업 재료소비는 의미 다른 메커니즘(기억/망각, 기억력 점유)으로 치환.
- **StoryFollowup / Unlock_Tuto / MapSelect_CurrentLocationSet** — 스토리 씬오브젝트 토글·수치게이트·마커 이동 보강.
- **Btm_UI_Change / UI New Txt** — in/out·뱃지 DOTween 애니(현재 정적 토글) → DOTweenAnimation 컴포넌트로.
- **Background On_Off** — 단일 토글 등가(현재 EnterAreaWithFade BG 교체로 대체).

### 우선순위 제안
1. ✅ **SFX C# 이식** — 완료.
2. **전투 스킬 시스템**(DoubleAttack/Smash/BeforeHit) — 전투 깊이.
3. **속성 데미지 텍스트**(Physical/Water 색).
4. **장비 시스템**(장착/스탯계산/강화·대장간 UI) — 대형, 별도 진행.

> 주의: "이식됨"은 기능 동등성 기준. 스킬 계열은 **능력/기억력으로 재설계**되어 1:1 포팅이 아님(부분이식으로 표기). VS 원본 식별자(EquipChange/Skill_ON_OFF 등)는 C#에서 전면 리네임/재모델링됨.
