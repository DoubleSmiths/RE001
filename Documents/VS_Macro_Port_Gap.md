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
| Equip Stat Cal | 장착 장비 스탯 합산·내구도 표시 | **미이식** | 합산 UI/로직 없음(EquipmentData.BaseAtk 필드만, Data.Contents.cs:82) |
| Equip Stat Cal After Enh | 강화 후 스탯 재계산 | **부분** | 강화 로직 O(GameManager.cs:521 ForgeEquipment + EquipEnhanceData), **합산·표시 UI 없음** |
| EquipSkill Text | 장비 고유스킬 텍스트 | **미이식** | 장비 고유스킬 데이터/UI 부재 |
| Selected Equip Kind Name | 선택 장비 종류명 | **미이식** | EquipSlotType enum만(Define.cs), 표시 UI 없음 |
| Inv Imp-Item Amount Check | 중요(퀵)아이템 보유수 | 이식됨 | UI_InventoryPopup.cs:188 OnClickImportantBagBtn, UI_ImportantSlot |
| Inv Item Amount Check | 인벤 아이템 보유수 | 이식됨 | GameManager.GetItemCount, UI_InventoryPopup.cs:99-141 |
| Inv Item Info | 아이템 상세 정보 | 이식됨 | UI_ItemPopup.cs:100-132 SetInfo |
| Inventory Slot | 인벤 슬롯(아이콘/개수/등급색/잠금) | 이식됨 | UI_InvSlot.cs:76/103/110 |
| Choose Item Box | 아이템 박스 선택(zoom/pan 연출) | **부분** | 클릭→상세 흐름 O(UI_InvSlot.cs:117), 카메라 zoom/pan 연출 미확인 |
| Use Item | 아이템 사용·소모 | **부분** | RemoveItem·물약 UsePotion(UI_PotionPopup:442) O, 일반 인벤 "사용" 버튼 흐름 미확인 |
| Loot Display Box | 전투 획득 전리품 표시 | 이식됨 | UI_BattleBtnGroup.cs:589 → UI_BattleDropItem.cs:76 |
| Monster Item Drop Box | 몬스터 드롭 아이템 박스 | 이식됨 | MonsterController.cs:182 → UI_BattleDropItem / UI_MonsterItemDropListBox.cs:51 |
| Monster Money Drop Box | 몬스터 골드 드롭 박스 | **부분** | 골드적립·HUD카운트업 O(MonsterController:177, UI_GameScene:461), 별도 드롭박스 슬롯 UI 미확인 |
| Req Item Box | 요구 아이템 박스(부족색) | 이식됨 | UI_SkillDetailPopup.cs:378, UI_PotionDetailPopup.cs:601 |
| Skill Level Text | 스킬 레벨/최대 텍스트 | 이식됨 | UI_SkillBookSkill.cs:102-109, UI_SkillDetailPopup.cs:866-879 |
| Skill Lvl_ReqItem | 스킬 레벨업 요구 아이템 | 이식됨 | UI_SkillDetailPopup.cs:378 ReqItem 박스군 + RefreshSourceUI:422 |
| Skill_List_AttackDefenseIcon | 스킬목록 공/방 아이콘 | 이식됨(재설계) | 능력 Role 카테고리로 통합: UI_SkillBookPopup.cs:266 PassFilter |
| Skill_List_AttackSkillIcon | 공격스킬 아이콘 | 이식됨(재설계) | UI_SkillBookPopup.cs:42,67 필터 + AbilityManager.Role |
| Skill_List_SkillIcon | 스킬 아이콘 | 이식됨(재설계) | UI_SkillBookSkill.cs:67 SetInfo + Role 필터 |
| PotionCraft_ReqItem | 물약제작 요구 재료(부족색) | 이식됨 | UI_PotionDetailPopup.cs:601 UpdateReqItems / :644 HasEnoughMaterials |
| Potion Cooldown | 물약 재사용 대기 슬라이더 | 이식됨 | UI_PotionPopup.cs:442/496-517(쿨바 fill 1→0, finishAnim) |

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

### 미이식 (신규 C# 구현 필요) — 13
1. **전투 스킬 시스템** — `DoubleAttack`(확률 2회), `Smash`(확률 ×1.5), `BeforeHit_Skills`(디스패처). RE001엔 실행로직(SkillBase/DoSkill) 부재, `BattleMath.Resolve(skillMul)` 훅만 있음.
2. **속성 데미지 텍스트** — `WaterTextShow`(물/마법 파랑). `DamageFontMode`에 속성 모드 추가 필요.
3. **장비 시스템 UI** — `Equip Arrow/Icon/Stat Cal/Stat Cal After Enh/EquipSkill Text/Selected Equip Kind Name`(6종). ⚠️ **로직·데이터는 이미 존재**(GameManager.cs:496 `EquipItem`, :521 `ForgeEquipment`, EquipmentDic/EquipEnhanceDic) — 빠진 건 **장비창 팝업 UI**(슬롯 화살표/아이콘/스탯 합산표시/강화후 표시/고유스킬텍스트/종류명). 즉 UI만 새로 만들면 됨. (대형)
4. **Poten Slot** — 소울 필요 잠재 장착 슬롯(완전 부재).
5. **Path_INFO** — 길드/지역 정보 패널(게이지·색상, 완전 부재).
6. **Quest Marker** — isOnPlace 위치 마커 토글.
7. **Monster State_Anchored** — UI(스크린스페이스) 몬스터 변형 + Money 보상.
8. **Btm_UI_Text** — DORestartById 하단 텍스트 애니.
9. **BtnSelectSound** — 버튼 클릭 SFX (SoundManager 연결 필요).
10. **Duri Text** — 장비 내구도 색상텍스트(데이터/이벤트만 존재).

### 부분이식 (보강 필요) — 9
- **SFX Macro_RE / MissText&Sound / BtnSelectSound** — **사운드 시스템 자체가 stub**(SoundManager의 Play/PlayOneShot 전부 주석). → SFX C# 정식화가 공통 선결.
- **PhysicalTextShow** — 데미지폰트에 속성별 색 모드 추가.
- **Use Item / Choose Item Box** — 물약 외 일반 소비아이템 "사용" 버튼 흐름, 아이템 박스 선택 zoom/pan 연출.
- **Equip Stat Cal After Enh** — 강화 로직(ForgeEquipment)은 있으나 강화 후 스탯 합산·표시 UI(장비창 의존).
- **Monster Money Drop Box** — 골드 적립·HUD는 있으나 전투 중 골드 드롭박스 슬롯 UI.
- **StoryFollowup / Unlock_Tuto / MapSelect_CurrentLocationSet** — 스토리 씬오브젝트 토글·수치게이트·마커 이동 보강.
- **Btm_UI_Change / UI New Txt** — in/out·뱃지 DOTween 애니(현재 정적 토글). → DOTweenAnimation 컴포넌트로.
- **Background On_Off / Monster Money Drop** — 토글/UI 표시 등가 확인.

### 우선순위 제안
1. **SFX C# 이식**(SoundManager 정식화) — SFX/Miss/BtnSelect 3개 갭을 한 번에 해소(저비용·고효과). *현재 SFX는 프리팹 playOnAwake로만 울림.*
2. **전투 스킬 시스템**(DoubleAttack/Smash 등) — 전투 깊이.
3. **속성 데미지 텍스트**(Physical/Water 색).
4. **장비 시스템 UI**(장비창/강화) — 대형, 별도 진행.

> 주의: "이식됨"은 기능 동등성 기준이며, "추정"은 명칭 매칭 기반(코드 정밀확인 권장). 배치2 일부(Skill_List 아이콘 등)는 UI_SkillBookSkill 존재로 이식 추정.
