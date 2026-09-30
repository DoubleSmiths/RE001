<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-07-05 | Updated: 2026-07-08 -->

# Popup

## Purpose
UIManager 팝업 스택 위에서 동작하는 모달/비모달 팝업 20종이 모인 디렉토리. 모든 클래스는 `UI_Popup`(→ `UI_Base`)을 상속하며, enum 기반 Bind로 프리팹 노드를 잡고 `SetInfo(...)`로 데이터를 주입받는다. 도감(Bestiary/MinMon), 물약/능력 제작(연금공방), 장비, 인벤토리, 여정(이동), NPC 대화, 설정 등 게임의 거의 모든 콘텐츠 시스템의 상세 UI가 여기에 있다. 팝업 열림 중에는 `Time.timeScale = 0`이므로 연출은 전부 unscaled 시간(`WaitForSecondsRealtime`, DOTween `SetUpdate(true)`) 기반이다.

## Key Files
| File | Description |
|------|-------------|
| `UI_Popup.cs` | 모든 팝업의 base 클래스. `Managers.UI.SetCanvas`/`ClosePopupUI(this)`, `_frameRT` 기준 `OpenAnimation()`(DOAnchorPosY, OutBack, `_openFromY/_closeToY`로 방향 커스텀), `OnClickBG()`(역방향 애니 후 닫기), `ClosePanelWithAnim()` 제공. 오픈 후 0.5초 배경클릭 닫기 가드(`CLOSE_LOCK_DURATION`/`IsCloseAllowed`, 기획서1 슬8) — 전 팝업 공통 |
| `UI_AttactedPopup.cs` | [미니몬 관심] 비모달 Side 알림. `SetInfo(int dataId, bool storageFull)` — 우측 슬라이드인 → 3초 → 슬라이드아웃 후 `Managers.Resource.Destroy`로 자기파괴. 팝업 스택/timeScale 미사용, `Managers.UI.ShowAttractedPopup`로 직접 생성 |
| `UI_BestiaryPopup.cs` | 마물 도감 팝업. 지역별 몬스터 그리드(`REGION_STUB` 스텁 데이터), 완성도 + 도감 스킬 4종(HybridKinship/MasterHandler/Retrievion/MemoryExpansion). 슬롯 Emoj(느낌표/물음표) 노출 타이밍 분기(슬28~29 — 느낌표는 prefab Move delay 0.8s에 맞춤). 기록도/공명도/등급은 `Managers.MinMon`으로 위임하는 static 헬퍼(`GetRecordRate`/`GetSync` 등) 제공. `SetInfo()`, `PlayGradeUp(int dataId, int newStar)`, `RefreshCompanionVisuals()` |
| `UI_CharacterPopup.cs` | 캐릭터 상태(스탯) 팝업(슬11~15). STR/VIT/DEX/MAG 투자 포인트 + PATK/MATK/HP/DEF의 기본·증가·부스트 분해 표시와 상세(치명타/명중/저항 등) 토글. 스탯 라벨 26종은 프리팹 복붙 오저작 교정을 위해 코드에서 `SetTextLocalized`로 세팅, 상세 펼침 시 섹션 스페이서 동기화(`RefreshSectionSpacers` — 반대편 컬럼을 밀어냄), 서브텍스트 페이드인(0.4s OutQuad, 딜레이 0.1/0.2/0.3). `SetInfo(PlayerController player)` |
| `UI_CustomOrderPopup.cs` | 연금공방 제작의뢰(CUSTOM ORDER) 팝업(슬34~36). 물약/기술 버튼 그룹 토글(`_currentGroup`), DialogueAnimator 타이핑 대사(NPC 대사는 kortext/engtext 노드를 `ApplyTalkLanguage()`로 언어 분기), 전면 로컬라이즈(`RefreshTexts`), 물약 프레임 → `UI_PotionDetailPopup`, 자동물약 기술 카드 → `UI_SkillDetailPopup`(`AbilityManager.AUTO_POTION_SKILL_ID`=5009). 해금 모션은 Scale 1→2 + Lock Fade out 동시(0.5s, 슬35) 후 NEW 텍스트 재노출. `SetInfo()` |
| `UI_DialoguePopup.cs` | NPC 대화창. `Managers.Data.DialogueScriptDic`의 groupKey로 라인 목록을 받아 DialogueAnimator로 타이핑/스킵/다음 진행. `SetInfo(string groupKey, string speakerName, Sprite portrait = null, Action onFinished = null, bool randomSingleLine = false, string portraitAnimState = "recep_normal")` |
| `UI_EquipmentPopup.cs` | 장비 팝업 1차 구현 — 7슬롯(Weapon/Helmet/Cloak/Armor/Glove/Boots/Ring) 리스트 네비게이션(한 번에 한 슬롯 패널만 표시)만 구현. 장착/강화/상세는 후속. enum `OpenHelemtListBtn` 오타는 프리팹 이름 호환을 위해 의도적으로 유지 |
| `UI_InventoryPopup.cs` | 소지품 인벤토리 팝업(슬19~20). `Managers.Game.Inventory`를 일반/귀중품(5슬롯)으로 분류해 `UI_InvSlot`/`UI_ImportantSlot`을 `MakeSubItem`으로 생성 — 일반 슬롯은 무제한(기본 25칸, 초과 시 `SPARE_SLOT_COUNT`=5 여유분 포함 동적 확장, ScrollView 스크롤). 정렬 버튼(클릭 애님 Scale 0.9/0.05s/Yoyo), 귀중품 박스 타이틀은 프리팹에 TMP가 없어 런타임 생성(`CreateRequisiteTitle` → "RequisiteTitleText" + Underlay 9F8320), NEW 뱃지(`OnInventoryNewChanged` 구독), 닫을 때 `ClearAllItemNew()`. `SetInfo()` |
| `UI_ItemPopup.cs` | 아이템 상세 팝업(슬21). `ItemDic`/`RequisiteItemDic`에서 이름·등급·발견장소·설명·보유수량 표시. 타이틀 Glow는 항상 등급색 유지, 이미지 후광(`ItemPopupImgGroupGlow`)/shine은 ①퀘스트 클리어 보상으로 열릴 때(`showGlow=true` — QuestManager/SubQuestManager ITEM_REWARD 경로) ②귀중품(`EquipGrade.REQUISITE`)일 때만 표시. 하단 ButtonGroup은 항상 숨김. 등급/발견장소 라벨 + 사용/사용불가 버튼 라벨 6종(MOR/Scroll/SOM)은 `RefreshTexts()`로 로컬라이즈. `SetInfo(int id, bool showGlow = false)`. 긴 설명(통행증 4~6줄)은 넘치는 만큼 `ItemPopupMain` 높이를 늘린다(창 안 요소는 모두 위 기준, `ITEM_DESC_BOTTOM_PAD`) |
| `UI_MinionInfoPopup.cs` | [나의 미니몬] 팝업. 포획한 미니몬의 능력치 등급(outline)/공명도/사용 스킬/동행(부르기·동행중) 토글. `SetInfo(int dataId)`, 포획 결과 연출 `PlayCatchResult(bool first, bool improved)` |
| `UI_MissionPopup.cs` | v2 미션·출석 창(ADR0030, 빌더 `Tools/Remake V2/Build Mission Popup` — 틀은 UI_ShopPopup 복제, 줄·탭은 상태 창 긴 버튼 복제). 탭 일일/주간/가이드/출석, 줄을 누르면 받기, 창 그림의 오른쪽 아래 자리(원래 가방 정리 버튼)는 `모두 받기`. 줄은 `MissionRowTemplate` 복제본을 재사용하고 누름은 줄마다 한 번만 건다 다섯째 탭 `상점`(ShopManager): 핫타임(광고 남으면 광고, 아니면 크리스탈)·무료 크리스탈·오프라인 2배·월정액·만드라고라·인앱 상품. 상점 탭에서는 제목이 상점이고 `모두 받기`를 숨긴다(줄 동작이 광고·소비·결제라서). 여섯째 탭 `던전`(ADR0034, `Tab` 은 끝에만 붙인다 — 바로가기가 번호로 저장): 요일 던전 3줄(무료 → 광고 → 크리스탈 순) + 설명 줄, 오늘의 던전·최고 기록 표시, 무료가 남으면 탭 이름 옆 빨간 '!'. 누르면 `DungeonManager.TryEnter` 뒤 창을 닫고 `EnterAreaWithFade(Battle)`. 일곱째 탭 `유물`(ADR0035): 1회·10회·광고 소환, `확률 보기`(등급·종류별 확률·천장 — 법정 공개 화면), 30종 목록(줄 아이콘 `MissionRowIcon` — `SetRow(..., icon)` 이 켜고 왼쪽 글자를 민다, 방금 얻은 것 NEW/UP). 여덟째 탭 `마을`(ADR0036): 오늘의 배달 의뢰 3줄(배달/받기) + NPC 5명 호감도·혜택·선물 줄. 탭 8개는 빌더가 4×2 그리드(`GridLayoutGroup`)로 놓는다. 줄 왼쪽 글자는 두 줄까지 줄여 넣고(위아래 여백 1.5 — 픽셀 글꼴이 줄 간격보다 커서), 그래도 넘치면 말줄임(`Ellipsis`). 던전 탭 아래쪽(ADR0037): 시련 줄(현재 단계·배수 / 도전·요구 레벨·최고 단계)과 심연 줄(최고 층 / 도전·전 스테이지 돌파) — 요일 던전과 같은 `EnterBattle(tryEnter)`(예약 후 창을 닫고 전투로). 창 제목은 '던전'. 시련을 도전할 수 있으면 던전 탭 '!'. 아홉째 탭 `패스`(ADR0039): 맨 위 줄 = 돌파 수 + 프리미엄(가격 — 누르면 결제, `claim:false` 라 '모두 받기'가 누르지 않는다) / 5곳마다 한 칸(무료 · 프리미엄, 사기 전엔 프리미엄을 회색으로). 탭 줄은 5개씩 두 줄. 열째 탭 `업적`(ADR0040): 맨 위 줄 = 다는 칭호 · 모은 수(누르면 다음 칭호, `claim:false`) / 분야마다 지금 단계 한 줄(왼쪽 이름·보상·칭호, 오른쪽 진행 수). 미션 제목의 목표 수는 자릿점(N0). 마을 탭 맨 위 줄 = 비번 일정(ADR0041, 누르면 사냥 → 채집 → 마을 일, `claim:false`). 마을 탭 사람 줄: 볼 차례면 '새 이야기!', 좋아하는 재료를 줄 수 있으면 '♡ 재료' + 선물 칸 ♡, 좋아하는 선물이면 알림(ADR0042). 열한째 탭 `의상`(ADR0043, 탭 줄은 이제 6개씩 두 줄): 맨 위 줄 = 모은 수 / 8벌 — 줄 아이콘 `CostumeManager.Preview`(HUD 초상화를 그 색으로), 오른쪽 착용 중 / 입기 / 크리스탈 값(한 번 누르면 '다시 눌러 구매', 두 번째에 산다 — `_armedCostume`) / 달성 조건. 입은 의상은 `DataId` 로 비교한다. |
| `UI_RelicSummonPopup.cs` | v2 유물 소환 연출(ADR0045, 빌더 `Tools/Remake V2/Build Relic Summon Popup` — 미션 창 틀 복제). `SetInfo(pulls, again)`: 카드(`SummonCardTemplate` 복제 — 빛줄기·등급 빛·칸·아이콘·이름·NEW/Lv/MAX)가 0.12초 간격으로 튀어나온다. 전설은 빛줄기 회전·흔들림·큰 소리, 1회는 카드 크게. 아래 줄 천장·환급, [N회 더](광고 소환은 없음). 배경 = 건너뛰기 → 닫기. 미션 창 `ShowPulls` 가 연다. |
| `UI_MonsterDetailPopup.cs` | 도감 몬스터 상세 팝업. 능력치 14종·드롭 아이템·몬스터 스킬 잠금 표시, [내 미니몬] 버튼 → `UI_MinionInfoPopup`. 몬스터 IMG는 `CreatureData.AnimController`를 Animator에 주입해 해당 몬스터 Idle 애님 반복 재생(프리팹 기본 컨트롤러=슬라임은 백업 후 복원), 잠금 문구는 로컬라이즈(`MinimonSkillLockedText`/`Link100RequiredText`). `SetInfo(int dataId)`, `RefreshCompanionState()` |
| `UI_PotionDetailPopup.cs` | 물약 상세정보 팝업(슬36~40). HP/SPD 물약을 내부 `PotionView`로 정규화, 설명 ↔ 필요재료 토글, 다음 단계 미리보기, 제작 버튼 처리(제작 시퀀스는 try/finally로 DOTween 안전화). 미제작 SPD는 설명 분기(`SpdPotionRevealed`로 판정 → `SPDPotion0DescText`), 재료 충족=00FF00/부족=FF0000 + 부족 시 아이콘 위 반투명 검정 오버레이(`_reqItemDims`), 게이지 점멸은 `_blinkExtras` 리스트로 세그먼트 포함(StartBlink 첫 줄이 StopBlink 호출하므로 거기서 비우지 않음 — 슬39 주의). 물약 이미지 후광(`GlowMask_1`)/shine(`PotionDetailPopupshineeffect`)은 표시하지 않음 — 타이틀 후광만 유지(UI_ItemPopup과 동일 정책). `SetInfo(int dataId = -1, bool fromCustomOrder = true)` |
| `UI_PotionPopup.cs` | 물약창 팝업(슬41~42). HP/Speed 슬롯 + 쿨타임 슬라이더(위→아래) + Finish Anim + 자동물약 On/Off와 HP Ratio 슬라이더(기억 전/후 분기). 슬롯 하단 [정보] 버튼(`Buttons.Button`/`Button_1`) → `UI_PotionDetailPopup`(설명 전용, `fromCustomOrder: false`). HP/SPD 쿨타임은 PlayerController에 영속(`StartHpPotionCooldown`/`StartSpdPotionCooldown`, 재오픈 시 `*CooldownNormalized`로 복원). AutoBtn NEW: 자동물약 기억 후 클릭(확인) 전까지 노출 — PlayerPrefs `AutoPotionNewSeen` 키로 영속, `MarkAutoPotionMemorized()` 시 리셋. `RefreshTexts()`로 타이틀(`LocalizeKey.BtmPotionBtnText`)·정보 라벨 언어 처리, `OnLanguageChanged` 구독/해제. `SetInfo()`, `MarkAutoPotionMemorized()` |
| `UI_SettingPopup.cs` | 설정 팝업. 언어(한/영) 선택과 BGM/SFX/데미지텍스트/카메라쉐이크 토글 — 전부 `Managers.Game`의 플래그(`IsBGSoundOn` 등)를 읽고 쓴다 |
| `UI_SkillBookPopup.cs` | [능력 목록] 팝업. AbilityManager 기반 보유 능력 아이콘(`UI_SkillBookSkill` 동적 생성) + 기억력(used/max) 표시, 능력 종류 필터 5버튼(프리팹 authored DOTweenAnimation 트리거). 아이콘 클릭 → 능력 정보창. `SetInfo()`, `RefreshUI()` |
| `UI_SkillDetailPopup.cs` | [능력 정보창](슬45~50) — 디렉토리 최대 파일. 능력 상세 + 기억(Memorize)/망각, 다음 등급 미리보기 토글, 요구 재료·장비·미니몬 게이팅 박스(재료 클릭 → `UI_ItemPopup` 아이템 상세, 장비 클릭 → 전용 팝업 미구현이라 `Managers.UI.ShowToast` 최소구현), 설명 길이에 따른 프레임 자동 사이즈. 설명 텍스트는 TopLeft 정렬(슬46/48), 등급에 따라 변하는 수치(확률·쿨타임·효과치)는 `<color=yellow>`(슬48). `SetInfo(int skillId = -1)`, `SkillId` 프로퍼티. `Managers.Ability` 의존 |
| `UI_TamePopup.cs` | [미니몬 잡기!] 포획 미니게임 팝업(슬32~37). 로딩→등장→길들이기/만드라고라 분기→성공/실패 연출 시퀀스, 확률/천장 판정은 `Managers.MinMon.TryCatch`. 잡기 연출: Lure 애님+Emoj, 순서3-1(효과 有)/3-2 텍스트 분기(성공 시 70:30, 실패 시 30:70 — 슬32), 실패 시 좌우 반전(Monster Turn 0.8s)→0.3초 뒤 Runaway 도주(슬36), 확인 라벨("정말로?"/"아직은!") 로컬라이즈. 전용 BGM `PushBgm` 후 OnDestroy에서 복귀. `SetInfo(int dataId)` |
| `UI_TravelPopup.cs` | 여정(장소 이동) 팝업(슬44~47). `TravelNodeData` 기반 TOWN/EXPLORE 대분류 → 마을/필드 중분류 → 목적지 소분류 버튼 트리(`UI_TownSelectButton`/`UI_FieldSelectButton`/`UI_MapSelectButton` 동적 생성), 퀘스트 VFX(`Managers.Quest` notify — 대분류 카테고리 버튼은 `TownCategoryButtonQuestVFX`=BTN_TOWN / `FieldCategoryButtonQuestVFX`=BTN_EXPLORE 키, 동적 목적지 버튼은 ClickBtnId/EnterBtnId 매칭). 상단 미리보기 `RefreshPlacePreview()`(선택 노드 > 현재 위치 > 첫 TOWN 노드 순), [출발] 버튼은 행선지 선택 시 FFA300+Interactive(슬46), 현재 위치/행선지 색상 규칙. 해금 조건 프레임워크(슬47): `TravelConditionData`의 KILL_COUNT(몬스터 누적 처치)/RANK(모험가 등급) 판정, 조건 미작성·미구현 타입은 해금(true) 폴백 — 미해금 노드는 목록에 미표시. `SetInfo()`, `RefreshUI()` override |
| `UI_UnlockButtonPopup.cs` | 기능(버튼) 해금 튜토리얼 팝업. TMP 글자별 알파 typewriter 연출(LateUpdate에서 vertex color 강제), 클릭 시 타이핑 중이면 스킵·완료 상태면 닫기. `SetInfo(string iconSprite, string iconNameId, string iconDescId)` |
| `UI_WarningPopup.cs` | [포획 창고 가득] 비모달 경고. `SetInfo(int dataId)` — AttactedPopup과 동일한 우측 슬라이드인/아웃 후 자기파괴, 버튼 없음. `Managers.UI.ShowWarningPopup(dataId)`로 표시 |

## For AI Agents

### Working In This Directory
- 새 팝업은 `UI_Popup` 상속 + `Awake() => Init();` + `base.Init()` 체크가 기본 골격. `Init()`에서 `BindImage/BindText/BindButton/BindObject(typeof(enum))`으로 프리팹 노드를 바인딩한다 — enum 멤버 이름은 프리팹 GameObject 이름과 완전 일치해야 하므로 프리팹의 오타(`OpenHelemtListBtn` 등)도 enum에 그대로 유지한다.
- 열기/닫기는 `Managers.UI.ShowPopupUI<T>()` / `ClosePopupUI()`로만. 예외는 비모달 알림 2종(`UI_AttactedPopup`, `UI_WarningPopup`)으로, 전투 중 게임을 멈추지 않기 위해 스택/timeScale을 쓰지 않고 `Managers.UI.ShowAttractedPopup`/`ShowWarningPopup`으로 직접 생성 후 스스로 `Managers.Resource.Destroy`한다.
- 데이터 주입은 팝업 생성 직후 `SetInfo(...)` 호출 컨벤션. 시그니처는 팝업마다 다르다(위 표 참조).
- 전투 중 도감·마물 상세·포획·나의 미니몬이 스택에 있으면 전투를 정지한다(`UIManager.IsBattlePaused`, ADR0026). 다른 팝업과 마을 진행은 유지한다. UIManager가 팝업 Animator/저작 Tween을 독립 시간으로 설정하며, 코드 연출도 `WaitForSecondsRealtime`과 `.SetUpdate(true)`를 사용한다.
- 진입/닫기 애니메이션: `_frameRT`에 프레임 RectTransform을 넣고 `OpenAnimation()` 호출, 배경 버튼에 `BindEvent(OnClickBG)` 연결(역방향 애니 후 닫기). 등장 방향은 `_openFromY/_closeToY`로 조정(예: SettingPopup은 +350).
- 배경클릭 닫기는 오픈 후 0.5초 동안 잠김(`UI_Popup.IsCloseAllowed`, 기획서1 슬8) — `OnClickBG`가 자동 처리하므로 base를 쓰면 별도 코드 불필요. 커스텀 닫기 핸들러를 만들 때는 `IsCloseAllowed`를 직접 체크한다.
- 언어 변경 대응(전 팝업 통일 패턴): 정적 라벨은 `RefreshTexts()`에 모으고 `Init()`에서 호출 + `Managers.Game.OnLanguageChanged += RefreshTexts` 구독, 반드시 `OnDestroy()`에서 해제. 텍스트는 `Util.Localize(key)` + `ApplyFont(tmp, FontType.UI)`. `Define.LocalizeKey`에 아직 없는 신규 키는 문자열 키로 `Util.Localize("...")` 호출(Tools/GenerateLocalizeKeys 재실행 시 상수로 교체 가능).
- 연출은 프리팹에 authored된 `DOTweenAnimation`을 코드에서 트리거(`DORestart`/`DORewind`/`DOPlayBackwards`)하는 것이 프로젝트 관례. 코드로 대체할 때는 `[DOTweenAnimation 규약 예외 — 의도적 코드 대체]` 주석으로 사유를 남긴다(UI_AttactedPopup, UI_SkillDetailPopup 참고).
- .cs는 UTF-8(BOM 없음). 게임 수치는 JSON(`Managers.Data.*Dic`)에서 로드하고 하드코딩하지 않는다.

### Common Patterns
- **enum Bind 패턴**: `#region Enum`에 Images/Texts/Buttons/GameObjects enum 선언 → `Bind*(typeof(...))` → `GetImage/GetText/GetButton/GetObject((int)Enum.Member)`로 접근. 클릭은 `gameObject.BindEvent(handler)`.
- **SubItem 동적 생성**: 리스트형 UI는 `Managers.UI.MakeSubItem<T>(parent)`로 `UI_SubItem`을 생성하고 각 아이템의 `SetInfo(...)`로 채운다(Inventory→UI_InvSlot, SkillBook→UI_SkillBookSkill, Travel→UI_TownSelectButton 등). 기존 자식은 `Managers.Resource.Destroy`로 정리.
- **타이핑(typewriter) 연출**: NPC 대사류는 공용 `DialogueAnimator` 컴포넌트(`GetOrAddComponent`)의 `PlayTyping`/`SkipTyping` 사용(Dialogue/Tame/CustomOrder). UnlockButtonPopup은 TMP vertex alpha를 직접 조작하는 자체 구현. 공통 UX: 타이핑 중 클릭=전체 표시 스킵, 완료 후 클릭=다음/닫기.
- **비모달 자기소멸 알림**: 슬라이드인 → `WaitForSecondsRealtime` 유지 → 슬라이드아웃 → `OnComplete(() => Managers.Resource.Destroy(gameObject))`.
- **등급 색상/이름**: `Util.GetGradeColor(EquipGrade)` + 한/영 등급명 스위치(UI_ItemPopup의 `GetGradeName` 참고).
- **미니몬 데이터 접근**: 기록도/공명도/등급 조회는 `UI_BestiaryPopup`의 static 헬퍼를 경유(내부적으로 `Managers.MinMon` 위임) — `UI_MinionInfoPopup`이 `using static UI_BestiaryPopup`으로 재사용.

### 스펙 출처(기획서 매핑)
이 폴더의 구현/주석에 나오는 "슬N"은 구글 슬라이드 기획서 슬라이드 번호다 — **기획서1** = 미니몬퀘스트 기획서(70장), **기획서2** = 54장짜리 후속 덱. 에디터 내 스펙 미러(슬라이드별 요구사항 체크)는 `Assets/@Scripts/Editor/QuestVerifyMenu.cs`에 있다. 이 폴더 파일별 매핑:

| File | 기획서 슬라이드 |
|------|----------------|
| `UI_Popup.cs` | 기획서1 슬8·11·21, 기획서2 슬42 |
| `UI_BestiaryPopup.cs` | 기획서2 슬28·38 |
| `UI_CharacterPopup.cs` | 기획서1 슬11~15 |
| `UI_CustomOrderPopup.cs` | 기획서1 슬34~36 |
| `UI_DialoguePopup.cs` | 기획서1 슬31~32, 수정안 슬1 |
| `UI_InventoryPopup.cs` | 기획서1 슬19 |
| `UI_ItemPopup.cs` | 기획서1 슬21 |
| `UI_PotionDetailPopup.cs` | 기획서1 슬36~40 |
| `UI_SettingPopup.cs` | 기획서1 슬9 |
| `UI_SkillBookPopup.cs` | 기획서2 슬42~44 |
| `UI_SkillDetailPopup.cs` | 기획서2 슬45~46 |
| `UI_TamePopup.cs` | 기획서2 슬37 |

## Dependencies

### Internal
- **Managers**: `Managers.UI`(팝업 스택/SetCanvas/MakeSubItem), `Managers.Data`(CreatureDic, ItemDic, RequisiteItemDic, DialogueScriptDic, HP/SPD 물약 등 JSON 딕셔너리), `Managers.Game`(Inventory, Language, 설정 플래그, OnLanguageChanged/OnInventoryNewChanged 이벤트), `Managers.Resource`(Load/Destroy), `Managers.Sound`(PlaySFX/PushBgm), `Managers.MinMon`(도감/포획), `Managers.Ability`(능력 목록/기억), `Managers.Quest`(여정 퀘스트 VFX)
- **Data 클래스**: `CreatureData`, `ItemData`, `MinMonData`, `TravelNodeData`, `DialogueScriptLine`, `InventoryItem`
- **UI 계층**: `UI_Base`(Bind/Get/BindEvent), SubItem들(`UI_InvSlot`, `UI_ImportantSlot`, `UI_SkillBookSkill`, `UI_TownSelectButton`, `UI_FieldSelectButton`, `UI_MapSelectButton`)
- **기타**: `Define`(LocalizeKey, FontType, EquipGrade, Language, 아이템 ID 상수), `Util`(Localize, ApplyFont, GetGradeColor, FindChild), `DialogueAnimator`, `PlayerController`(CharacterPopup)

### External
- **DOTween** (`DG.Tweening`) — 전 팝업의 진입/닫기/연출 트윈, `DOTweenAnimation` 프리팹 트리거
- **TextMeshPro** (`TMPro`) — 텍스트 표시 및 vertex color 조작(typewriter)
- **UnityEngine.UI** — Image/Button/Slider/ScrollRect

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->

- `UI_BestiaryPopup`는 지역 헤더 전체와 기존 스크롤 높이를 유지하면서 Viewport 위아래 한 행 여유 안의 지역만 마물 칸을 생성한다. 첫 가시 지역은 SetInfo 안에서 채우며, 스크롤·화면 크기 변경 시 재계산한다. 화면 밖의 FindBox는 null이지만 등급/완성도 갱신은 계속 진행하고, 재진입 시 최신 데이터를 표시한다. 언어 변경은 25개 지역명을 포함하며 ScrollRect·언어 이벤트는 OnDestroy에서 해제한다.

- 2026-09-10: 해금 안내는 마을 버튼에 로드된 Sprite를 받으며 문자열 SetInfo도 호환한다. 본문은 부모 폭 안에서 줄바꿈하고 언어 변경 시 제목/본문/폰트를 함께 갱신한다. 이전 타이핑 코루틴·Tween을 종료하고 새 문장의 알파 배열을 독립적으로 캡처한다.

- 2026-09-10: 장비 목록은 제작 요구 레벨 후 DataId로 정렬한다. 제작 상세 본문의 재료 링크는 `UI_ItemLinkHandler`로 아이템 상세를 열며 상위 ScrollRect의 드래그를 유지한다. 아이템 출처는 `ItemSourceGuide`로 층·마물을 표시하고 열린 상태의 언어 변경에 대응한다.
- 2026-09-10: 축성 창은 처치로 숙련도를 얻는 방법을 표시한다. 미보유 능력도 아이콘 클릭 시 상세 설명·기억 비용을 먼저 보여주며, 숙련도는 상세 창의 축성 버튼에서만 소비한다.

- 2026-09-09: 첫 발견부터 미니몬 능력 효과를 미리 보여준다. `UI_MinionInfoPopup`은 포획 결과 연출 종료 후 부르기 버튼을 표시한다. 동행 중인 대여 능력은 별도 기억/망각 불가이며 설명과 버튼에 동행 상태를 표시한다. 공명도 100% 영구 습득 규칙은 유지한다.
