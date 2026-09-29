using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Util;

public class UI_GameScene : UI_Scene
{
    #region Enum

    enum Images
    {
        CurrencyBar,
        MainGoldImg,
        MainCrystalImg,
        ButtomDisplayer,
        ButtonDisplayerBackground,
        Frame,
        Frame2,
        CharacterButtonIcon,
        CharacterBtnNewIcon,
        InventoryButtonIcon,
        PotionButtonIcon,
        EquipmentButtonIcon,
        SkillButtonIcon,
        BestiaryButtonIcon,
        BestiaryBtnNewIcon,
        BestiaryBtnFollowingBox,
        PlayerHPBarGroup,
        PlayerHPBarFrame,
        PlayerHPBarGuage,
        PlayerEXPBarGroup,
        PlayerEXPBarFrame,
        PlayerEXPBarGuage,
        PlayerPortraitFrame,
        PortraitMask,
        PortraitImg,
        Main_QuestFollow,
        Main_QuestFollowColor,
        ForFadeImage,
    }

    enum Texts
    {
        MainGoldText,
        MainCrystalText,
        CharacterButtonText,
        InventoryButtonText,
        InventoryButtonNewTxt,
        PotionButtonText,
        PotionButtonNewTxt,
        EquipmentButtonText,
        EquipmentButtonNewTxt,
        SkillButtonText,
        SkillButtonNewTxt,
        BestiaryButtonText,
        BestiaryBtnFollowingNumTxt,
        BestiaryBtnNewTxt,
        RankTxt,
        PlayerHPBarText,
        PlayerEXPTextFlat,
        PlayerEXPTextPercent,
        PlayerRankText,
        PlayerLevelText,
        Quest_NeedItemNameTxt,
        Quest_NeedItemAmountTxt,
    }

    enum Buttons
    {
        OptionBtn,
        GuildButton,
        TravelButtonOnBattle,
        CharacterButton,
        InventoryButton,
        PotionButton,
        EquipmentButton,
        SkillButton,
        BestiaryButton,
        ExitButton,
    }

    enum GameObjects
    {
        FIXED,
        MainGoldShow,
        MainCrystalShow,
        AreaBtnGroupSlot,
        OnBattleBtns,
        OnBattleMainBtnGroup,
        TravelButtonOnBattleQuestVFX,
        BTMButtonGroup,
        BTMButtonUnlockHelper,
        Layout,
        Dust_01,
        Ring_01,
        ExitQuestVFX,
        PlayerBar,
        PlayerHPBarGuageBox,
        PlayerEXPBarGuageBox,
        PlayerPortraitGroup,
        QuestFollow,
        TxtGroup,
        Quest_Done_Effect,
        GlowDot_Lens_01,
        GlowDot_Lens_02,
        GlowDot_02,
        BuffDisplayer,
    }

    #endregion


    UI_AreaBtnGroup _currentBtnGroup;
    int _followActiveQuestId = -1;
    int _followDoneFxQuestId = -1;   // 완료 연출을 이미 재생한 퀘스트 ID (완료 전이 시 1회만 재생)
    bool _isAreaTransitioning;

    private void Awake() { Init(); }

    // 재화 Lerp용 표시값
    float _displayGold;
    float _displayCrystal;

    // HP/EXP 이전값 추적
    float _prevHp  = -1f;
    float _prevExp = -1f;

    static readonly Color _barWhite = Color.white;
    static readonly Color _barHurt  = new Color32(0xFF, 0x00, 0x00, 0xFF); // FF0000
    static readonly Color _barHeal  = new Color32(0x58, 0xFF, 0x00, 0xFF); // 58FF00

    static readonly Color _goldColor = Color.white;                                  // #FFFFFF
    static readonly Color _crystalColor = new Color(0f, 0.953f, 1f, 80f / 225f);        // #00F3FF
    static readonly Color _increaseColor = new Color(0f, 1f, 0f);                       // #00FF00
    static readonly Color _decreaseColor = new Color(1f, 0f, 0f);                       // #FF0000


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // 폰트 등록 (숫자 텍스트는 Inspector에서 개별 지정)
        ApplyFont(GetText((int)Texts.CharacterButtonText), FontType.UI);
        ApplyFont(GetText((int)Texts.InventoryButtonText), FontType.UI);
        //ApplyFont(GetText((int)Texts.InventoryButtonNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.PotionButtonText), FontType.UI);
        //ApplyFont(GetText((int)Texts.PotionButtonNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.EquipmentButtonText), FontType.UI);
        //ApplyFont(GetText((int)Texts.EquipmentButtonNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.SkillButtonText), FontType.UI);
        //ApplyFont(GetText((int)Texts.SkillButtonNewTxt), FontType.UI);
        ApplyFont(GetText((int)Texts.BestiaryButtonText), FontType.UI);
        //ApplyFont(GetText((int)Texts.BestiaryBtnNewTxt), FontType.UI);
        //ApplyFont(GetText((int)Texts.PlayerRankText), FontType.UI);
        //ApplyFont(GetText((int)Texts.PlayerLevelText), FontType.UI);

        // 버튼 이벤트
        GetButton((int)Buttons.OptionBtn).gameObject.BindEvent(OnClickOption);
        GetButton((int)Buttons.CharacterButton).gameObject.BindEvent(OnClickCharacter);
        GetButton((int)Buttons.InventoryButton).gameObject.BindEvent(OnClickInventory);
        GetButton((int)Buttons.PotionButton).gameObject.BindEvent(OnClickPotion);
        GetButton((int)Buttons.EquipmentButton).gameObject.BindEvent(OnClickEquipment);
        GetButton((int)Buttons.SkillButton).gameObject.BindEvent(OnClickSkill);
        GetButton((int)Buttons.BestiaryButton).gameObject.BindEvent(OnClickBestiary);

        // Camera Setting
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;

        // 초기화 시 퀘스트 동행 UI는 비활성화 상태로 시작 (필요 시 활성화)
        GetObject((int)GameObjects.QuestFollow).gameObject.SetActive(false);

        // Fade
        //FadeInOut(GetImage((int)Images.ForFadeImage));

        Managers.Quest.OnFollowUIChanged += RefreshQuestFollow;
        Managers.Quest.OnBtnUnlocked += OnBtnUnlocked;
        Managers.Quest.OnNotifyChanged += RefreshQuestNotify;
        RefreshQuestFollow(Managers.Quest.CurrentQuestID);
        RefreshQuestNotify();

        // 슬라이드 10: 최하단 탭 버튼 하이라이트 되돌림을 위한 Popup 닫힘 구독
        Managers.UI.OnPopupClosed += OnBtmPopupClosed;

        SetInfo();
        RefreshTabLocks();
        RefreshBTMButtonGroup();
        RestoreTabNewBadges();   // 저장된 Potion/Equipment/Skill NEW 뱃지 복원
        // 이미 가진 것(장비/능력/발견 몬스터)에 해당하는 탭을 연다. 진행 중인 세이브도 여기서 복구된다.
        Managers.Game.RefreshEarnedTabUnlocks();
        RefreshBtmButtonsLockState();

        // TODO : 플레이어의 현재 구역에 따라 EnterArea 호출
        EnterArea(AreaType.Village1);

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Managers.Game != null)
        {
            Managers.Game.OnResourceChanged -= RefreshCurrency;
            Managers.Game.OnAdventurerRankChanged -= RefreshPlayerBar;
            Managers.Game.OnIdleBatch -= ShowIdleBatch;
            Managers.Game.OnLanguageChanged -= RefreshTexts;
            Managers.Game.OnInventoryNewChanged -= RefreshBTMButtonGroup;
            Managers.Game.OnItemChanged -= OnFollowItemChanged;
            Managers.Game.OnStatusPointChanged -= RefreshCharacterNewIcon;
        }

        if (Managers.MinMon != null)
        {
            Managers.MinMon.OnInterestChanged -= RefreshBestiaryNewIcon;
            Managers.MinMon.OnMaxStarReached -= OnMonsterMaxStar;
            Managers.MinMon.OnDiscovered -= OnMonsterDiscovered;
        }

        if (Managers.Quest != null)
        {
            Managers.Quest.OnFollowUIChanged -= RefreshQuestFollow;
            Managers.Quest.OnBtnUnlocked -= OnBtnUnlocked;
            Managers.Quest.OnNotifyChanged -= RefreshQuestNotify;
        }

        if (Managers.UI != null)
            Managers.UI.OnPopupClosed -= OnBtmPopupClosed;
    }

    // 퀘스트 느낌표(VFX) 갱신: 하단 나가기 버튼 / 전투 중 여정 버튼.
    // VFX 전용 키 또는 클릭 액션 키(BTN_*) 어느 쪽이 notify 목록에 있어도 켜진다. (UI_Village1BtnGroup과 동일 패턴)
    void RefreshQuestNotify()
    {
        if (Managers.Quest == null)
            return;

        bool exitOn = false;
        bool travelOn = false;
        foreach (var n in Managers.Quest.GetCurrentNotifyList())
        {
            if (n.BtnID == EXIT_QUEST_VFX || n.BtnID == BTN_EXIT)
                exitOn = true;
            if (n.BtnID == TRAVEL_BUTTON_ON_BATTLE_QUEST_VFX || n.BtnID == BTN_JOURNEY)
                travelOn = true;
        }

        GetObject((int)GameObjects.ExitQuestVFX)?.SetActive(exitOn);
        GetObject((int)GameObjects.TravelButtonOnBattleQuestVFX)?.SetActive(travelOn);
    }

    public void FadeInOut()
    {
        FadeInOut(GetImage((int)Images.ForFadeImage));
    }

    // ────────────────────────────────────────
    //  구역 전환
    // ────────────────────────────────────────

    /// <summary>페이드 없이 즉시 구역 전환 (씬 초기 로드용)</summary>
    /// <summary>방치 진행 알림(주기적). 골드·EXP 숫자는 이미 실시간으로 오르지만
    /// 무엇 때문에 오르는지 알 수 없어, 어디서 몇 마리를 잡고 있는지 묶어서 알린다.</summary>
    void ShowIdleBatch(GameManager.OfflineReward r, string area)
    {
        // 지역명 유무에 따라 문장 구조가 달라져(영문은 "in {지역}" 이 뒤에 붙는다) 서식 키를 둘로 나눈다.
        string msg = string.IsNullOrEmpty(area)
            ? string.Format(Util.Localize(LocalizeKey.IdleHuntFormat), r.Kills, r.Exp, r.Gold)
            : string.Format(Util.Localize(LocalizeKey.IdleHuntAreaFormat), area, r.Kills, r.Exp, r.Gold);
        Managers.UI.ShowToast(msg);
    }

    /// <summary>방치 보상 정산 결과 안내. 전용 팝업 대신 토스트를 재사용한다
    /// (ponytail: 전용 프리팹은 연출 스펙이 정해지면 그때).</summary>
    public void ShowOfflineReward(GameManager.OfflineReward r)
    {
        double h = r.Seconds / 3600.0;
        string time = h >= 1.0
            ? string.Format(Util.Localize(LocalizeKey.OfflineTimeHoursFormat), h)
            : string.Format(Util.Localize(LocalizeKey.OfflineTimeMinutesFormat), r.Seconds / 60.0);
        string msg = string.Format(Util.Localize(LocalizeKey.OfflineRewardFormat), time, r.Kills, r.Exp, r.Gold);
        if (r.Capped)
            msg += "\n" + string.Format(Util.Localize(LocalizeKey.OfflineRewardCapFormat), GameManager.OFFLINE_MAX_HOURS);
        Managers.UI.ShowToast(msg);
    }

    public void EnterArea(AreaType area)
    {
        // 구역이 바뀔 때마다 플레이어를 지면에 다시 세운다.
        //  Idle/Move 클립에는 위치 커브가 있고 Run/Attack에는 없다 → 마을에선 클립이 y를 잡아주지만
        //  전투(Run)에선 잡아주는 게 없어 씬에 저작된 옛 y가 그대로 남아 **공중을 걷고 있었다**.
        if (Managers.Game?.Player != null)
        {
            Vector3 p = Managers.Game.Player.transform.position;
            Managers.Game.Player.transform.position = new Vector3(p.x, GROUND_Y, p.z);
        }

        SwapAreaBtnGroup(area);
    }

    /// <summary>
    /// FadeOut → onBlack(배경 교체) → BtnGroup 교체 → FadeIn → PlayEnterAnimation 순으로 구역 전환.
    /// </summary>
    public bool CanEnterAreaTransition(AreaType area)
    {
        if (_isAreaTransitioning)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: already transitioning to another area. requested={area}");
            return false;
        }

        if (GetImage((int)Images.ForFadeImage) == null)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: fade image missing. requested={area}");
            return false;
        }

        if (GetObject((int)GameObjects.AreaBtnGroupSlot) == null)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: AreaBtnGroupSlot missing. requested={area}");
            return false;
        }

        string prefabName = $"UI_{area}BtnGroup";
        GameObject prefab = Managers.Resource.Load<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: prefab missing. prefab={prefabName}");
            return false;
        }

        if (prefab.GetComponent<UI_AreaBtnGroup>() == null)
        {
            Debug.LogWarning($"[UI_GameScene] Area transition blocked: UI_AreaBtnGroup missing on prefab. prefab={prefabName}");
            return false;
        }

        return true;
    }

    // 슬라이드 55 — Battle Scene 진입 시 Fade Out 0.5 / Fade In 1.0.
    // Fade In 절반(t=1.0)에 [지역명 UI] 표시. 배너 In Animation 종료(t=2.0)에 BG scroll + Player Idle→Run + 배틀 루프 시작.
    public bool EnterAreaWithFade(AreaType area, System.Action onBlack = null)
    {
        if (CanEnterAreaTransition(area) == false)
            return false;

        Image fade = GetImage((int)Images.ForFadeImage);
        if (fade == null)
            return false;

        _isAreaTransitioning = true;

        // 마을(Village*) 복귀 시 CurrentTravelAreaId 갱신 (HOME 버튼/사망 등 Travel popup 거치지 않는 경로).
        // Travel popup의 "현재 위치" 표시가 마지막 전투 행선지로 남아있던 버그 차단.
        // AreaType enum 이름과 TravelNode.AreaType 문자열을 매칭 — Village2/3 추가되어도 코드 수정 불필요.
        // Battle은 노드가 여러 개라 자동 매칭 X (TravelPopup.TravelToNode가 정확한 노드 id를 별도로 set함).
        if (area != AreaType.Battle)
        {
            string areaName = area.ToString();
            if (Managers.Data?.TravelNodeDic != null)
            {
                // 이미 CurrentTravelAreaId가 이 AreaType의 노드를 가리키고 있으면 그대로 둔다.
                //  마을이 여러 개인데 전부 AreaType이 Village1이라, 아래 "첫 번째 매칭 노드"
                //  탐색이 무조건 첫 마을(아스달)을 집어 **다른 마을로 이동해도 아스달로 튕겨나갔다.**
                //  (Travel popup은 이미 정확한 노드 id를 설정해 두므로 덮어쓸 이유가 없다)
                int cur = Managers.Game.CurrentTravelAreaId;
                bool curMatches = Managers.Data.TravelNodeDic.TryGetValue(cur, out var curNode)
                                  && curNode.AreaType == areaName;

                if (!curMatches)
                {
                    foreach (var node in Managers.Data.TravelNodeDic.Values)
                    {
                        if (node.AreaType == areaName)
                        {
                            Managers.Game.CurrentTravelAreaId = node.Id;
                            break;
                        }
                    }
                }
            }
        }

        // 슬라이드 69 — Battle 씬에서 나갈 때 transit 효과:
        //   monster Idle 고정 + BG scroll OFF + Player Run + 화면 밖 이동.
        //   Fade Out 동안 진행되며, SwapAreaBtnGroup(검은 화면 시점)에서 UI_BattleBtnGroup destroy → EndBattle 정리.
        var battleBtn = _currentBtnGroup as UI_BattleBtnGroup;
        if (battleBtn != null) battleBtn.BeginExitTransit();

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // Fade Out 0.5s
        seq.Append(fade.DOFade(1f, 0.5f).SetEase(Ease.InQuad).SetUpdate(true));

        // 검은 화면에서 영역 교체
        seq.AppendCallback(() =>
        {
            onBlack?.Invoke();
            if (SwapAreaBtnGroup(area) == false)
                Debug.LogWarning($"[UI_GameScene] SwapAreaBtnGroup failed during transition. area={area}");
        });

        // Fade In 1.0s
        seq.Append(fade.DOFade(0f, 1.0f).SetEase(Ease.InQuad).SetUpdate(true));

        if (area == AreaType.Battle)
        {
            // Battle 진입 — Fade In 50% (절대 t=1.0)에 UI_Toast(지역명 배너) 표시.
            // Toast prefab의 DOTweenAnimation Move 종료(슬라이드 55: 1초)에서 진입 완료 처리.
            seq.InsertCallback(1.0f, () => Managers.UI.ShowToast(GetCurrentAreaName()));
            seq.InsertCallback(2.0f, () =>
            {
                StartBattleSceneScroll();
                _currentBtnGroup?.PlayEnterAnimation();
            });
            seq.OnComplete(() => _isAreaTransitioning = false);
        }
        else
        {
            // 비-Battle (마을 등) — 기존 동작: fade 끝나면 바로 PlayEnterAnimation
            seq.OnComplete(() =>
            {
                _isAreaTransitioning = false;
                _currentBtnGroup?.PlayEnterAnimation();
            });
        }
        seq.OnKill(() => _isAreaTransitioning = false);
        return true;
    }

    string GetCurrentAreaName()
    {
        int id = Managers.Game?.CurrentTravelAreaId ?? 0;
        if (Managers.Data?.TravelNodeDic != null &&
            Managers.Data.TravelNodeDic.TryGetValue(id, out var node))
            return Util.Localize(node.Name);
        return string.Empty;
    }

    void StartBattleSceneScroll()
    {
        // 슬라이드 55/57/58 — 배경 스크롤 시작 + Player Idle→Run
        BackgroundScroller scroller = null;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == "CH1BG" || go.name == "CH1BG(Clone)")
            {
                scroller = go.GetComponent<BackgroundScroller>();
                if (scroller != null) break;
            }
        }
        if (scroller != null) scroller.IsScrolling = true;

        var player = Managers.Game?.Player;
        if (player != null && player.State != Define.State.Dead)
            player.State = Define.State.Moving;
    }

    bool SwapAreaBtnGroup(AreaType area)
    {
        if (_currentBtnGroup != null)
            Managers.Resource.Destroy(_currentBtnGroup.gameObject);

        GameObject slotObject = GetObject((int)GameObjects.AreaBtnGroupSlot);
        if (slotObject == null)
        {
            Debug.LogWarning($"[UI_GameScene] SwapAreaBtnGroup failed: AreaBtnGroupSlot missing. area={area}");
            _currentBtnGroup = null;
            return false;
        }

        Transform slot = slotObject.transform;
        string prefabName = $"UI_{area}BtnGroup";

        GameObject go = Managers.Resource.Instantiate(prefabName, slot);
        if (go == null)
        {
            Debug.LogWarning($"[UI_GameScene] SwapAreaBtnGroup failed: instantiate returned null. prefab={prefabName}");
            _currentBtnGroup = null;
            return false;
        }

        _currentBtnGroup = go.GetComponent<UI_AreaBtnGroup>();
        if (_currentBtnGroup == null)
        {
            Debug.LogWarning($"[UI_GameScene] SwapAreaBtnGroup failed: UI_AreaBtnGroup missing. prefab={prefabName}");
            Managers.Resource.Destroy(go);
            return false;
        }

        _currentBtnGroup.Setup(this);

        // 방치 보상은 "마지막으로 사냥하던 지역" 기준으로 정산하고(GameManager.ClaimOfflineReward),
        //  전투 밖에서만 방치 누적이 돈다(TickIdle — 전투 중엔 실제 전투가 보상한다).
        //  ⚠️ 예전엔 이 두 줄이 EnterArea 에만 있었는데 전투 진입은 전부 EnterAreaWithFade 라
        //     지역이 한 번도 기록되지 않아(LastBattleAreaId 0) 오프라인 보상·방치 누적이 실제로는 한 번도 돌지 않았다.
        //     두 경로가 모두 지나는 여기서 처리한다.
        if (area == AreaType.Battle)
            Managers.Game?.MarkBattleArea(Managers.Game.CurrentTravelAreaId);
        Managers.Game?.SetInBattleArea(area == AreaType.Battle);

        // 지역별 배경음 — 마을=BGM_Village1 / 필드(여정)=BGM_BWF. (잡기 전용 BGM_MinimonCapture는 별도)
        Managers.Sound?.PlayBgm(area == AreaType.Battle ? "BGM_BWF" : "BGM_Village1");

        Debug.Log($"[UI_GameScene] SwapAreaBtnGroup success. area={area}, prefab={prefabName}");
        return true;
    }

    // ────────────────────────────────────────
    //  SetInfo – 씬 진입 시 전체 갱신
    // ────────────────────────────────────────
    public void SetInfo()
    {
        // 재화 초기값 즉시 표시 (Lerp 없이)
        _displayGold = Managers.Game.Gold;
        _displayCrystal = Managers.Game.Crystal;
        GetText((int)Texts.MainGoldText).text = Managers.Game.Gold.ToString("N0");
        GetText((int)Texts.MainCrystalText).text = Managers.Game.Crystal.ToString("N0");

        RefreshPlayerBar();
        RefreshBestiaryFollowing();
        RefreshTexts();
        RefreshBTMButtonGroup();

        Managers.Game.OnResourceChanged += RefreshCurrency;
        Managers.Game.OnAdventurerRankChanged += RefreshPlayerBar;
        Managers.Game.OnIdleBatch += ShowIdleBatch;
        Managers.Game.OnLanguageChanged += RefreshTexts;
        Managers.Game.OnInventoryNewChanged += RefreshBTMButtonGroup;
        Managers.Game.OnItemChanged += OnFollowItemChanged;
    }

    // ────────────────────────────────────────
    //  재화
    // ────────────────────────────────────────
    void RefreshCurrency()
    {
        int newGold = Managers.Game.Gold;
        int newCrystal = Managers.Game.Crystal;

        // 증감 색상 플래시
        if (newGold != (int)_displayGold)
            FlashCurrencyText(GetText((int)Texts.MainGoldText), newGold > _displayGold, _goldColor);
        if (newCrystal != (int)_displayCrystal)
            FlashCurrencyText(GetText((int)Texts.MainCrystalText), newCrystal > _displayCrystal, _crystalColor);

        // Lerp 갱신
        DOTween.To(() => _displayGold, x =>
        {
            _displayGold = x;
            GetText((int)Texts.MainGoldText).text = ((int)x).ToString("N0");
        }, newGold, 0.5f).SetEase(Ease.OutQuad);

        DOTween.To(() => _displayCrystal, x =>
        {
            _displayCrystal = x;
            GetText((int)Texts.MainCrystalText).text = ((int)x).ToString("N0");
        }, newCrystal, 0.5f).SetEase(Ease.OutQuad);
    }

    void FlashCurrencyText(TMP_Text text, bool increased, Color originalColor)
    {
        Color flash = increased ? _increaseColor : _decreaseColor;
        text.DOKill();
        text.DOColor(flash, 0.1f)
            .OnComplete(() => text.DOColor(originalColor, 0.3f));
    }

    // ────────────────────────────────────────
    //  플레이어 바 (HP / EXP / 레벨)
    // ────────────────────────────────────────
    void RefreshPlayerBar()
    {
        int lv = Managers.Game.Level;
        GetText((int)Texts.PlayerLevelText).text = $"{Util.Localize(LocalizeKey.PlayerLvPrefix)}{lv}";
        GetText((int)Texts.PlayerRankText).text = Managers.Game.PlayerName;

        // 슬7: 모험가 등급 (0이면 BLANK 표기)
        var rankTxt = GetText((int)Texts.RankTxt);
        if (rankTxt != null)
        {
            int rank = Managers.Game.AdventurerRank;
            rankTxt.text = rank <= 0 ? "" : rank.ToString();
        }

        // TODO: 직업에 따라 PortraitImg 스프라이트 교체
        // Managers.Resource.LoadAsync<Sprite>($"Portrait_{jobType}", sprite =>
        //     GetImage((int)Images.PortraitImg).sprite = sprite);
    }

    // ────────────────────────────────────────
    //  텍스트 로컬라이징
    // ────────────────────────────────────────
    void RefreshTexts()
    {
        GetText((int)Texts.CharacterButtonText).text = Localize(LocalizeKey.BtmCharacterBtnText);
        GetText((int)Texts.InventoryButtonText).text = Localize(LocalizeKey.BtmInventoryBtnText);
        GetText((int)Texts.PotionButtonText).text = Localize(LocalizeKey.BtmPotionBtnText);
        GetText((int)Texts.EquipmentButtonText).text = Localize(LocalizeKey.BtmEquipmentBtnText);
        GetText((int)Texts.SkillButtonText).text = Localize(LocalizeKey.BtmSkillBtnText);
        GetText((int)Texts.BestiaryButtonText).text = Localize(LocalizeKey.BtmMinimonCodexBtnText);
        // PlayerLv prefix 등 동적 텍스트도 언어 변경 시 재갱신
        RefreshPlayerBar();
        RefreshQuestFollow(Managers.Quest.CurrentQuestID);   // 추적 재료 이름
    }

    public void RefreshBTMButtonGroup()
    {
        // 가방 버튼 NEW = 일반/귀중품 아이템 중 IsNew가 하나라도 있을 때만.
        //  (이전엔 연산자 우선순위로 일반아이템 절에 IsNew가 안 걸려 — 아이템 존재만으로 NEW가 안 꺼졌음)
        bool flag = false;
        foreach (InventoryItem item in Managers.Game.Inventory)
        {
            bool isNormal    = item.DataId > Define.ITEM_ID_MIN && item.DataId < Define.ITEM_ID_MIN + 1000;
            bool isImportant = item.DataId > Define.IMPORTANT_ITEM_ID_MIN && item.DataId < Define.IMPORTANT_ITEM_ID_MIN + 1000;
            if (item.IsNew && (isNormal || isImportant))
            {
                flag = true;
                break;
            }
        }

        GetText((int)Texts.InventoryButtonNewTxt)?.gameObject?.SetActive(flag);
    }

    public void UpdateHP(float current, float max)
    {
        // bar: fill Lerp
        Image bar = GetImage((int)Images.PlayerHPBarGuage);
        bar.DOKill();
        bar.DOFillAmount(max > 0 ? current / max : 0f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);

        // 텍스트 갱신
        TMP_Text hpText = GetText((int)Texts.PlayerHPBarText);
        hpText.text = $"{(int)current} / {(int)max}";

        // 텍스트 색상 플래시 (초기 설정 시 스킵)
        if (_prevHp >= 0f)
        {
            bool isHurt = current < _prevHp;
            Color flash = isHurt ? _barHurt : _barHeal;

            hpText.DOKill();
            hpText.DOColor(flash, 0.17f).SetUpdate(true)
                  .OnComplete(() => hpText.DOColor(_barWhite, 0.17f).SetUpdate(true));

            // 회복 시 텍스트 Scale 증감
            if (!isHurt)
                hpText.transform.DOScale(1.05f, 0.17f).SetUpdate(true)
                      .OnComplete(() => hpText.transform.DOScale(1f, 0.17f).SetUpdate(true));
        }

        _prevHp = current;
    }

    public void UpdateEXP(float current, float required)
    {
        // bar: fill Lerp
        Image bar = GetImage((int)Images.PlayerEXPBarGuage);
        bar.DOKill();
        bar.DOFillAmount(required > 0 ? current / required : 0f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);

        // 퍼센트 텍스트 (##.####% 형식)
        TMP_Text expText = GetText((int)Texts.PlayerEXPTextPercent);
        expText.text = required > 0 ? $"{current / required * 100f:F4}%" : "0.0000%";

        // EXP 증가 시 텍스트 플래시 + Scale
        if (_prevExp >= 0f && current > _prevExp)
        {
            expText.DOKill();
            expText.DOColor(_barHeal, 0.17f).SetUpdate(true)
                   .OnComplete(() => expText.DOColor(_barWhite, 0.17f).SetUpdate(true));

            expText.transform.DOScale(1.05f, 0.17f).SetUpdate(true)
                   .OnComplete(() => expText.transform.DOScale(1f, 0.17f).SetUpdate(true));
        }

        _prevExp = current;
    }

    // 슬라이드 68 — Level Up Event
    //   2-1. Player Level Up Text 팝업 (UI_PlayerLevelUpTextPopup.prefab)
    //   2-2. Player Level Up Effect (Player Level Up Ef.prefab)
    //   2-3. HP Now → MaxHp 회복 (PlayerController.RefreshStats에서 이미 처리) + 초록 회복 텍스트
    //   2-4/2-5. EXP/Level UI 갱신 (OnExpChanged/PlayerLevelText 핸들러가 처리)
    //   2-6. [상태] Button [+] icon Punch/Scale (Duration 0.3 OutQuad TO 0.2 Vibrato 10 Elasticity 1)
    public void OnLevelUp(int newLevel, int regenHp)
    {
        GetText((int)Texts.PlayerLevelText).text = $"{Util.Localize(LocalizeKey.PlayerLvPrefix)}{newLevel}";

        PlayerController player = Managers.Game?.Player;
        if (player != null)
        {
            Vector3 pos = player.transform.position;

            // 2-1. Level Up Text 팝업 — RectTransform 기반 UI prefab. Canvas(SceneUI) 하위로 parent + local 좌표 리셋.
            // prefab 내부 DOTweenAnimation 최대 anim 시간 ≈ 1.5s. 2s 후 자체 정리.
            var textPopup = Managers.Resource.Instantiate("UI_PlayerLevelUpTextPopup");
            if (textPopup != null)
            {
                textPopup.transform.SetParent(transform, worldPositionStays: false);
                var rt = textPopup.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = Vector2.zero;
                textPopup.transform.localScale = Vector3.one;
                Destroy(textPopup, 2f);
            }

            // 2-2. Level Up Effect — ParticleSystem 기반 world-space VFX. 3s 후 정리.
            var effect = Managers.Resource.Instantiate("Player Level Up Ef");
            if (effect != null)
            {
                effect.transform.position = pos;
                Destroy(effect, 3f);
            }

            // 2-3. HP 회복 텍스트 (초록). 회복량 0이면 popup 생략.
            if (regenHp > 0)
                Managers.Object.ShowHealFont(pos, regenHp, player.transform);
        }

        // 2-6. [상태] 버튼 [+] icon — active + Punch/Scale
        GameObject icon = GetImage((int)Images.CharacterBtnNewIcon)?.gameObject;
        if (icon != null)
        {
            icon.SetActive(true);
            icon.transform.DOKill();   // 진행 중인 punch 있으면 정리 (Restart)
            icon.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 10, 1f).SetEase(Ease.OutQuad);
        }
    }

    // 슬26: 포획 가능 미니몬 창고에 미니몬이 있으면 [도감] 버튼에 NEW 뱃지 + 저장 수 표기 + Punch 등장 anim
    void RefreshBestiaryNewIcon()
    {
        bool has = Managers.MinMon != null && Managers.MinMon.HasInterest;
        var icon = GetImage((int)Images.BestiaryBtnNewIcon)?.gameObject;
        if (icon != null)
        {
            icon.SetActive(has);
            if (has)
            {
                int count = Managers.MinMon.InterestedMinMons.Count;
                bool full = count >= Managers.MinMon.InterestCapacity;   // 저장칸 가득 (슬27)
                var numTxt = Util.FindChild<TMPro.TMP_Text>(icon, "BestiaryBtnFollowingNumTxt", true);
                if (numTxt != null)
                {
                    numTxt.text = count.ToString();
                    // 슬27: 저장칸 가득이면 count 빨강(FF0000), 여유면 기본 흰색 (창고 칸 수는 능력으로 증가 가능 — 슬23-4)
                    numTxt.color = full ? Util.HexToColor("FF0000") : Color.white;
                }
                // 슬26/27: 버튼 스프라이트 스왑 — 여유=11_orange_button_xs_empty(흰색) / 가득=1_orange_button_xs(FA5959)
                var btnImg = GetImage((int)Images.BestiaryBtnNewIcon);
                if (btnImg != null)
                {
                    var btnSprite = Managers.Resource.Load<Sprite>(full ? "1_orange_button_xs" : "11_orange_button_xs_empty");
                    if (btnSprite != null) btnImg.sprite = btnSprite;
                    btnImg.color = full ? Util.HexToColor("FA5959") : Color.white;
                }
                var box = Util.FindChild<UnityEngine.UI.Image>(icon, "BestiaryBtnFollowingBox", true);
                if (box != null) box.color = full ? Util.HexToColor("D14A4A") : Color.white;
                // 슬26 등장 Anim: Punch/Scale 0.3 (TO 0.3, Vibrato 10, Elasticity 1)
                icon.transform.DOKill();
                // 등장 기준 스케일 (값이 클수록 아이콘 큼). 0.35는 너무 작아 원래 0.5로 복원.
                const float BESTIARY_NEW_ICON_SCALE = 0.5f;
                icon.transform.localScale = new Vector3(BESTIARY_NEW_ICON_SCALE, BESTIARY_NEW_ICON_SCALE, 1f);
                icon.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 10, 1f);

                // 슬26 #3: 등장 Anim(0.3s)이 끝날 때 GlowDot_01을 slime_icon(Img_BestiaryBtnNewIcon_06) 중앙에 생성
                var slime = Util.FindChild<Transform>(icon, "Img_BestiaryBtnNewIcon_06", true);
                if (slime != null && slime.Find("GlowDot_01") == null)
                {
                    DOVirtual.DelayedCall(0.3f, () =>
                    {
                        if (slime == null || slime.Find("GlowDot_01") != null) return;
                        var glow = Managers.Resource.Instantiate("GlowDot_01");
                        if (glow == null) return;
                        glow.name = "GlowDot_01";
                        var grt = glow.GetComponent<RectTransform>();
                        if (grt == null) grt = glow.AddComponent<RectTransform>();
                        grt.SetParent(slime, false);
                        grt.anchoredPosition = Vector2.zero;
                    }).SetUpdate(true);
                }
            }
        }
        var txt = GetText((int)Texts.BestiaryBtnNewTxt)?.gameObject;
        if (txt != null) txt.SetActive(has);
    }

    void RefreshCharacterNewIcon()
    {
        GetImage((int)Images.CharacterBtnNewIcon).gameObject.SetActive(Managers.Game.StatusPoint > 0);
    }

    // ────────────────────────────────────────
    //  도감 – 동행 중인 미니몬 수
    // ────────────────────────────────────────
    void RefreshBestiaryFollowing()
    {
        int companionId = Managers.Game.CompanionMinMonId;
        bool hasCompanion = companionId != -1;

        GetObject((int)GameObjects.BTMButtonGroup); // 참조 확인용
        GetImage((int)Images.BestiaryBtnFollowingBox).gameObject.SetActive(hasCompanion);
        if (hasCompanion)
            GetText((int)Texts.BestiaryBtnFollowingNumTxt).text = "1";
    }

    // ────────────────────────────────────────
    //  탭 잠금
    // ────────────────────────────────────────
    void RefreshTabLocks()
    {
        // RefreshBtmButtonsLockState에서 해금 상태 기반으로 활성/비활성 처리하므로 별도 작업 불필요
    }

    void OnFollowItemChanged(int dataId)
    {
        Data.QuestData quest = Managers.Quest.GetCurrentFollowUI();
        if (quest == null) return;
        if (!int.TryParse(quest.FollowItemID, out int followId)) return;
        if (followId != dataId) return;

        RefreshQuestFollow(Managers.Quest.CurrentQuestID);
    }

    void RefreshQuestFollow(int questId)
    {
        Data.QuestData quest = Managers.Quest.GetCurrentFollowUI();
        GameObject followObj = GetObject((int)GameObjects.QuestFollow);

        if (quest == null)
        {
            followObj.SetActive(false);
            _followActiveQuestId = -1;
            return;
        }

        int itemId = int.Parse(quest.FollowItemID);
        int current = Managers.Game.GetItemCount(itemId);

        // 퀘스트 시트의 FollowItemName 은 한국어 표시 문자열이다 — 영어에서도 한글이 나왔다. 아이템 이름 키로 표시.
        GetText((int)Texts.Quest_NeedItemNameTxt).text = Managers.Data.ItemDic.TryGetValue(itemId, out var followItem)
            ? Util.Localize(followItem.Name)
            : quest.FollowItemName;

        // 보유 개수 색상: 0=빨강 / 0<보유<필요=주황 / 보유≥필요=초록. (TMP는 lime 등 미지원 → hex)
        // 표기는 (보유/필요) 한 쌍 괄호 + 슬래시, 전체에 색 적용 — 예: (1/2)
        int need = quest.FollowCount;
        string col = current == 0 ? "#FF0000" : (current < need ? "#FFA500" : "#00FF00");
        GetText((int)Texts.Quest_NeedItemAmountTxt).text = $"(<color={col}>{current}</color>/{need})";

        bool wasInactive = !followObj.activeSelf;
        if (!followObj.activeSelf)
        {
            followObj.SetActive(true);
            GetImage((int)Images.Main_QuestFollow).GetComponent<DOTweenAnimation>().DORestart();
        }
        else followObj.SetActive(true);

        bool isDone = current >= quest.FollowCount;
        ColorUtility.TryParseHtmlString($"#{(isDone ? Define.COMPLETE_COLOR : Define.QUEST_FOLLOW_DEFAULT_COLOR)}", out Color color);
        GetImage((int)Images.Main_QuestFollowColor).color = color;

        // 기획 수정안(2026-07-05): 완료 "전이" 시 1회 — Follow UI 애님 재생 + 완료 파티클 + Glow_Dot 이펙트.
        // (기존 조건 _followActiveQuestId != questId 는 같은 퀘스트 진행 중 완료되는 일반 경로에서 연출이 안 나가던 버그)
        if (isDone && _followDoneFxQuestId != questId)
        {
            _followDoneFxQuestId = questId;
            GetImage((int)Images.Main_QuestFollow).GetComponent<DOTweenAnimation>().DORestart();
            GetObject((int)GameObjects.Quest_Done_Effect).GetComponent<ParticleSystem>().Play();

            // 월드 ParticleSystem은 SSC 캔버스 위에 직접 렌더되지 않음(UIFx.cs 참조) → UIParticle 래퍼 경유
            UIFx.PlayParticle((RectTransform)followObj.transform, Vector2.zero, "GlowDot_01", 2f);
        }
        else if (!isDone && _followDoneFxQuestId == questId)
        {
            _followDoneFxQuestId = -1;   // 아이템 소모 등으로 완료 상태가 풀리면 재무장
        }

        _followActiveQuestId = questId;

        if (wasInactive)
            GetImage((int)Images.Main_QuestFollow).GetComponent<DOTweenAnimation>().DORestart();
    }

    void OnBtnUnlocked(string btnId)
    {
        if (btnId == UNLOCK_INVENTORY_BUTTON)
        {
            GetButton((int)Buttons.InventoryButton).gameObject.SetActive(true);
            GetButton((int)Buttons.InventoryButton).interactable = true;
            Managers.Game.UnlockBtmButton(TabType.Bag);
            RefreshBtmButtonsLockState();
            ShowNewBadge(TabType.Bag);
        }
        else if (btnId == UNLOCK_POTION_BUTTON)
        {
            // 서브퀘스트 완료로 silent 해금 — 해금 애니메이션/팝업 없이 하단 Potion 탭 활성화
            GetButton((int)Buttons.PotionButton).gameObject.SetActive(true);
            GetButton((int)Buttons.PotionButton).interactable = true;
            Managers.Game.UnlockBtmButton(TabType.Potion);
            RefreshBtmButtonsLockState();
            ShowNewBadge(TabType.Potion);
        }
    }

    void SetTabInteractable(Buttons btn, bool interactable)
    {
        var button = GetButton((int)btn);
        button.interactable = interactable;
        switch (btn)
        {
            case Buttons.CharacterButton:
                GetButton((int)Buttons.CharacterButton).gameObject.SetActive(interactable);
                GetImage((int)Images.CharacterBtnNewIcon).gameObject.SetActive(false);
                break;
            case Buttons.InventoryButton:
                GetButton((int)Buttons.InventoryButton).gameObject.SetActive(interactable);
                GetText((int)Texts.InventoryButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.PotionButton:
                GetButton((int)Buttons.PotionButton).gameObject.SetActive(interactable);
                GetText((int)Texts.PotionButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.EquipmentButton:
                //GetButton((int)Buttons.EquipmentButton).gameObject.SetActive(interactable);
                GetText((int)Texts.EquipmentButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.SkillButton:
                //GetButton((int)Buttons.SkillButton).gameObject.SetActive(interactable);
                GetText((int)Texts.SkillButtonNewTxt).gameObject.SetActive(false);
                break;
            case Buttons.BestiaryButton:
                GetButton((int)Buttons.BestiaryButton).gameObject.SetActive(interactable);
                GetText((int)Texts.BestiaryBtnNewTxt).gameObject.SetActive(false);
                GetImage((int)Images.BestiaryBtnNewIcon).gameObject.SetActive(false);
                break;
        }
    }

    // ────────────────────────────────────────
    //  New 뱃지
    // ────────────────────────────────────────
    public void ShowNewBadge(TabType tab)
    {
        switch (tab)
        {
            case TabType.Status:
                RefreshCharacterNewIcon();
                break;
            case TabType.Bag:
                GetText((int)Texts.InventoryButtonNewTxt).gameObject.SetActive(true);
                break;
            case TabType.Potion:
                GetText((int)Texts.PotionButtonNewTxt).gameObject.SetActive(true);
                Managers.Game.SetTabNew(TabType.Potion, true);       // 저장 — 재시작 후에도 on 유지
                break;
            case TabType.Equipment:
                GetText((int)Texts.EquipmentButtonNewTxt).gameObject.SetActive(true);
                Managers.Game.SetTabNew(TabType.Equipment, true);
                break;
            case TabType.Skill:
                GetText((int)Texts.SkillButtonNewTxt).gameObject.SetActive(true);
                Managers.Game.SetTabNew(TabType.Skill, true);
                break;
            case TabType.Bestiary:
                GetText((int)Texts.BestiaryBtnNewTxt).gameObject.SetActive(true);
                GetImage((int)Images.BestiaryBtnNewIcon).gameObject.SetActive(true);
                break;
        }
    }

    // ────────────────────────────────────────
    //  PlayerController 이벤트 연결
    // ────────────────────────────────────────
    public void BindPlayer(PlayerController player)
    {
        player.OnHpChanged += (cur, max) => UpdateHP(cur, max);
        player.OnExpChanged += (cur, req) => UpdateEXP(cur, req);

        // HP 바는 표시 전용 — 슬라이더 클릭으로 값이 조정되지 않게 입력만 차단.
        // (interactable=false는 비활성 틴트로 바가 어두워지므로, 컴포넌트 자체를 비활성화한다.
        //  표시는 PlayerHPBarGuage Image.fillAmount로 별도 구동되어 영향 없음)
        var hpSlider = GetImage((int)Images.PlayerHPBarGroup)?.GetComponent<Slider>();
        if (hpSlider != null)
        {
            hpSlider.interactable = true;   // 이전에 적용됐을 비활성 틴트 제거(노멀 색 복귀)
            hpSlider.enabled = false;       // 컴포넌트 비활성 → 클릭으로 값 조정 불가
        }
        player.OnLevelUp += (lv, regen) => OnLevelUp(lv, regen);
        UpdateHP(player.Hp, player.MaxHp);
        UpdateEXP(Managers.Game.Exp, player.ExpRequired);   // LoadGame된 현재 EXP로 슬라이더 초기화
        Managers.Game.OnStatusPointChanged += RefreshCharacterNewIcon;
        RefreshCharacterNewIcon();
        Managers.MinMon.OnInterestChanged += RefreshBestiaryNewIcon;
        RefreshBestiaryNewIcon();
        // 슬2: 몬스터 보라별 달성 → 완성도 스킬 등급 상승(도감 열려있으면 연출)
        Managers.MinMon.OnMaxStarReached += OnMonsterMaxStar;

        // 첫 몬스터 처치 → 도감 탭이 그 자리에서 열린다(다시 들어와야 보이지 않게).
        Managers.MinMon.OnDiscovered += OnMonsterDiscovered;

        // 슬40(능력#4 강화능력): 강화/물약 버프 → BuffDisplayer 노드에 UI_Buff 추가
        player.OnBuffApplied += OnBuffApplied;
        player.OnBuffCleared += OnBuffCleared;

        // 액티브 스킬바 — 전투 기록 아래 독립된 행동 영역. 마을이나 수동 능력이 없을 때는 숨긴다.
        if (_skillBar == null && player.AbilityRunner != null)
        {
            var host = GetImage((int)Images.ButtomDisplayer);
            if (host != null)
                _skillBar = UI_SkillBar.Create(host.transform, player.AbilityRunner);
        }
    }

    UI_SkillBar _skillBar;

    /// 능력 장착/해제 후 스킬바 재구성 (능력 팝업이 닫힐 때 호출).
    public void RefreshSkillBar() => _skillBar?.Refresh();

    // ── 슬40: 강화 버프 아이콘 (BuffDisplayer 노드에 UI_Buff 추가/제거) ──
    readonly System.Collections.Generic.Dictionary<int, UI_Buff> _buffItems = new();

    void OnBuffApplied(PlayerController.BuffInfo b)
    {
        var container = GetObject((int)GameObjects.BuffDisplayer);
        if (container == null) return;

        if (_buffItems.TryGetValue(b.Key, out var exist) && exist != null)
        {
            exist.Apply(b);   // 재발동 — 갱신
            return;
        }

        GameObject go = Managers.Resource.Instantiate("UI_Buff", container.transform);
        if (go == null) return;
        var ui = Util.GetOrAddComponent<UI_Buff>(go);
        ui.SetInfo(b, key => _buffItems.Remove(key));   // 지속 만료 시 사전 등록 해제
        _buffItems[b.Key] = ui;
    }

    void OnBuffCleared(int key)
    {
        if (_buffItems.TryGetValue(key, out var ui))
        {
            _buffItems.Remove(key);
            if (ui != null) ui.Remove();
        }
    }

    // 슬2: 몬스터가 보라별 최초 달성 시 — 도감 완성도 등급 상승 + (열려있으면)연출.
    void OnMonsterMaxStar(int dataId) => UI_BestiaryPopup.GrantMaxStarCompletion(dataId);

    // 몬스터를 처음 잡았을 때 — 도감 탭 개방 + NEW 뱃지.
    //  해금 자체는 AddKill이 이벤트 **전에** 끝내 두므로 여기서 반환값으로 가르면 안 된다
    //  (false가 돌아와 UI 갱신을 건너뛰면 탭 비트만 서고 버튼은 계속 숨어 있다).
    void OnMonsterDiscovered(int dataId)
    {
        Managers.Game.RefreshEarnedTabUnlocks();
        RefreshBtmButtonsLockState();
        ShowNewBadge(TabType.Bestiary);
    }

    // ────────────────────────────────────────
    //  버튼 핸들러
    // ────────────────────────────────────────
    void OnClickOption()
    {
        // 기획서 스펙: Scale / Duration 0.07 / OutQuad / Loops 2 / Yoyo / To 0.75
        GetButton((int)Buttons.OptionBtn).transform
            .DOScale(0.75f, 0.07f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);

        Managers.UI.ShowPopupUI<UI_SettingPopup>();
    }

    // ────────────────────────────────────────
    //  최하단 탭 버튼 (슬라이드 10)
    //  - 클릭 시 해당 Popup 열기 + 하이라이트(#FFDA01 Color + 1.1 Scale, 0.3s OutQuad)
    //  - 잠긴 버튼은 무반응
    //  - Popup 닫힘 시 하이라이트 되돌림
    // ────────────────────────────────────────
    static readonly Color BTM_HL_COLOR = new Color32(0xFF, 0xDA, 0x01, 0xFF);
    const float BTM_HL_SCALE = 1.1f;
    const float BTM_HL_DURATION = 0.3f;

    Color[] _btmBtnOriginalColor;
    Color[] _btmTextOriginalColor;
    System.Type _btmActivePopupType;
    int _btmActiveIdx = -1;

    void OnClickCharacter()
    {
        HandleBtmClick(TabType.Status, Buttons.CharacterButton, typeof(UI_CharacterPopup),
            () => Managers.UI.ShowPopupUI<UI_CharacterPopup>());
    }

    void OnClickInventory()
    {
        HandleBtmClick(TabType.Bag, Buttons.InventoryButton, typeof(UI_InventoryPopup),
            () => Managers.UI.ShowPopupUI<UI_InventoryPopup>().SetInfo());
    }

    void OnClickPotion()
    {
        HandleBtmClick(TabType.Potion, Buttons.PotionButton, typeof(UI_PotionPopup),
            () => Managers.UI.ShowPopupUI<UI_PotionPopup>().SetInfo());
    }

    void OnClickEquipment()
    {
        HandleBtmClick(TabType.Equipment, Buttons.EquipmentButton, typeof(UI_EquipmentPopup),
            () => Managers.UI.ShowPopupUI<UI_EquipmentPopup>());
    }

    void OnClickSkill()
    {
        HandleBtmClick(TabType.Skill, Buttons.SkillButton, typeof(UI_SkillBookPopup),
            () => Managers.UI.ShowPopupUI<UI_SkillBookPopup>().SetInfo());
    }

    void OnClickBestiary()
    {
        HandleBtmClick(TabType.Bestiary, Buttons.BestiaryButton, typeof(UI_BestiaryPopup),
            () => Managers.UI.ShowPopupUI<UI_BestiaryPopup>().SetInfo());
    }

    void HandleBtmClick(TabType tab, Buttons btnId, System.Type popupType, System.Action showAction)
    {
        if (Managers.Game.IsBtmButtonUnlocked(tab) == false)
            return; // 잠긴 탭은 무반응

        int idx = (int)btnId;
        if (popupType != null)
        {
            PlayBtmHighlight(btnId);
            _btmActivePopupType = popupType;
            _btmActiveIdx = idx;
        }
        showAction?.Invoke();

        // 저장형 NEW 뱃지(Potion/Equipment/Skill)는 탭을 열면 해제 + 저장(재시작 후에도 off 유지).
        ClearPersistedTabNew(tab);
    }

    // Potion/Equipment/Skill 탭은 NEW 뱃지를 GameManager 비트마스크로 저장 — 열면 해제+저장.
    //  (Bag/Bestiary/Status는 데이터 파생이라 여기서 건드리지 않음)
    void ClearPersistedTabNew(TabType tab)
    {
        switch (tab)
        {
            case TabType.Potion:    GetText((int)Texts.PotionButtonNewTxt).gameObject.SetActive(false); break;
            case TabType.Equipment: GetText((int)Texts.EquipmentButtonNewTxt).gameObject.SetActive(false); break;
            case TabType.Skill:     GetText((int)Texts.SkillButtonNewTxt).gameObject.SetActive(false); break;
            default: return;
        }
        Managers.Game.SetTabNew(tab, false);
    }

    // 재시작 시 저장된 NEW 뱃지(Potion/Equipment/Skill) 복원. Bag/Bestiary/Status는 각자 Refresh가 처리.
    void RestoreTabNewBadges()
    {
        GetText((int)Texts.PotionButtonNewTxt).gameObject.SetActive(Managers.Game.IsTabNew(TabType.Potion));
        GetText((int)Texts.EquipmentButtonNewTxt).gameObject.SetActive(Managers.Game.IsTabNew(TabType.Equipment));
        GetText((int)Texts.SkillButtonNewTxt).gameObject.SetActive(Managers.Game.IsTabNew(TabType.Skill));
    }

    // 각 Btm 버튼의 아이콘 Image (Button.colors가 안 덮어쓰는 별도 Image)
    Image GetBtmIcon(Buttons btnId)
    {
        switch (btnId)
        {
            case Buttons.CharacterButton: return GetImage((int)Images.CharacterButtonIcon);
            case Buttons.InventoryButton: return GetImage((int)Images.InventoryButtonIcon);
            case Buttons.PotionButton:    return GetImage((int)Images.PotionButtonIcon);
            case Buttons.EquipmentButton: return GetImage((int)Images.EquipmentButtonIcon);
            case Buttons.SkillButton:     return GetImage((int)Images.SkillButtonIcon);
            case Buttons.BestiaryButton:  return GetImage((int)Images.BestiaryButtonIcon);
        }
        return null;
    }

    // 각 Btm 버튼의 라벨 TMP_Text (아이콘과 함께 하이라이트 색 적용).
    TMP_Text GetBtmText(Buttons btnId)
    {
        switch (btnId)
        {
            case Buttons.CharacterButton: return GetText((int)Texts.CharacterButtonText);
            case Buttons.InventoryButton: return GetText((int)Texts.InventoryButtonText);
            case Buttons.PotionButton:    return GetText((int)Texts.PotionButtonText);
            case Buttons.EquipmentButton: return GetText((int)Texts.EquipmentButtonText);
            case Buttons.SkillButton:     return GetText((int)Texts.SkillButtonText);
            case Buttons.BestiaryButton:  return GetText((int)Texts.BestiaryButtonText);
        }
        return null;
    }

    void PlayBtmHighlight(Buttons btnId)
    {
        Button btn = GetButton((int)btnId);
        if (btn == null) return;
        Transform tr = btn.transform;
        Image icon = GetBtmIcon(btnId);
        CacheBtmOriginalColor(btnId, icon);

        tr.DOKill();
        tr.DOScale(BTM_HL_SCALE, BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
        if (icon != null)
        {
            icon.DOKill();
            icon.DOColor(BTM_HL_COLOR, BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        // 라벨 텍스트도 동일하게 #FFDA01로 (버튼만 변하고 텍스트는 안 변하던 문제 수정)
        TMP_Text txt = GetBtmText(btnId);
        if (txt != null)
        {
            CacheBtmTextColor(btnId, txt);
            txt.DOKill();
            txt.DOColor(BTM_HL_COLOR, BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    void RestoreBtmHighlight(Buttons btnId)
    {
        Button btn = GetButton((int)btnId);
        if (btn == null) return;
        Transform tr = btn.transform;
        Image icon = GetBtmIcon(btnId);

        tr.DOKill();
        tr.DOScale(1f, BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
        if (icon != null && _btmBtnOriginalColor != null)
        {
            int idx = (int)btnId;
            if (idx >= 0 && idx < _btmBtnOriginalColor.Length && _btmBtnOriginalColor[idx].a > 0f)
            {
                icon.DOKill();
                icon.DOColor(_btmBtnOriginalColor[idx], BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
            }
        }

        // 라벨 텍스트 색 원복
        TMP_Text txt = GetBtmText(btnId);
        if (txt != null && _btmTextOriginalColor != null)
        {
            int idx = (int)btnId;
            if (idx >= 0 && idx < _btmTextOriginalColor.Length && _btmTextOriginalColor[idx].a > 0f)
            {
                txt.DOKill();
                txt.DOColor(_btmTextOriginalColor[idx], BTM_HL_DURATION).SetEase(Ease.OutQuad).SetUpdate(true);
            }
        }
    }

    void CacheBtmTextColor(Buttons btnId, TMP_Text txt)
    {
        if (txt == null) return;
        int count = System.Enum.GetValues(typeof(Buttons)).Length;
        if (_btmTextOriginalColor == null || _btmTextOriginalColor.Length < count)
            _btmTextOriginalColor = new Color[count];
        int idx = (int)btnId;
        if (idx >= _btmTextOriginalColor.Length) return;
        if (_btmTextOriginalColor[idx].a <= 0f)
            _btmTextOriginalColor[idx] = txt.color;
    }

    void CacheBtmOriginalColor(Buttons btnId, Image img)
    {
        if (img == null) return;
        int count = System.Enum.GetValues(typeof(Buttons)).Length;
        if (_btmBtnOriginalColor == null || _btmBtnOriginalColor.Length < count)
            _btmBtnOriginalColor = new Color[count];
        int idx = (int)btnId;
        if (idx >= _btmBtnOriginalColor.Length) return;
        // 아이콘은 투명하지 않은 색상을 원본으로 캐시 (이미 하이라이트 진행 중인 상태 저장 방지)
        if (_btmBtnOriginalColor[idx].a <= 0f)
            _btmBtnOriginalColor[idx] = img.color;
    }

    void OnBtmPopupClosed(System.Type closedType)
    {
        if (_btmActivePopupType == null || closedType != _btmActivePopupType)
            return;
        if (_btmActiveIdx < 0)
            return;
        RestoreBtmHighlight((Buttons)_btmActiveIdx);
        _btmActivePopupType = null;
        _btmActiveIdx = -1;
    }

    void RefreshBtmButtonsLockState()
    {
        ApplyBtmLock(Buttons.CharacterButton, TabType.Status);
        ApplyBtmLock(Buttons.InventoryButton, TabType.Bag);
        ApplyBtmLock(Buttons.PotionButton, TabType.Potion);
        ApplyBtmLock(Buttons.EquipmentButton, TabType.Equipment);
        ApplyBtmLock(Buttons.SkillButton, TabType.Skill);
        ApplyBtmLock(Buttons.BestiaryButton, TabType.Bestiary);
    }

    void ApplyBtmLock(Buttons btnId, TabType tab)
    {
        Button btn = GetButton((int)btnId);
        if (btn == null) return;
        bool unlocked = Managers.Game.IsBtmButtonUnlocked(tab);
        // 잠긴 버튼은 완전히 숨김 (NEW 뱃지 등 포함된 루트 오브젝트 전체)
        btn.gameObject.SetActive(unlocked);
    }

#if UNITY_EDITOR
    // ────────────────────────────────────────
    //  [테스트] 재화 증감 애니메이션 확인
    //  Inspector에서 호출하거나 Update에서 키 입력으로 사용
    // ────────────────────────────────────────
    void Update()
    {


#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1)) TestGoldIncrease();
        if (Input.GetKeyDown(KeyCode.Alpha2)) TestGoldDecrease();
        if (Input.GetKeyDown(KeyCode.Alpha3)) TestCrystalIncrease();
        if (Input.GetKeyDown(KeyCode.Alpha4)) TestCrystalDecrease();

        if (Input.GetKeyDown(KeyCode.Alpha5)) TestHPDamage();
        if (Input.GetKeyDown(KeyCode.Alpha6)) TestHPHeal();
        if (Input.GetKeyDown(KeyCode.Alpha7)) TestEXPGain();

        if (Input.GetKeyDown(KeyCode.U)) TestUnlockAll();

        // [치트] K = 만능 키. 모든 컨텐츠 해금 (여정/장비/능력/도감/미니몬/재화/퀘스트).
        //  U(TestUnlockAll)는 하단 탭과 마을 버튼만 여는 반쪽짜리라 그대로 두고 따로 뒀다.
        if (Input.GetKeyDown(KeyCode.K)) CheatUnlockEverything();

        if (Input.GetKeyDown(KeyCode.T))
        {
            Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>().SetInfo("Travel_Icon", LocalizeKey.GuildText, LocalizeKey.TestKey);
        }

        if (Input.GetKeyDown(KeyCode.F1)) TestJumpToQuest(1);
        if (Input.GetKeyDown(KeyCode.F2)) TestJumpToQuest(2);
        if (Input.GetKeyDown(KeyCode.F3)) TestJumpToQuest(3);

        if (Input.GetKeyDown(KeyCode.A))
        {
            Managers.Game.AddItem(10001, 1); // 예시: 아이템 ID 10001을 1개 추가
        }
        if (Input.GetKeyDown(KeyCode.S))
            Managers.UI.ShowToast("산들바람 숲(잎구)");
        if (Input.GetKeyDown(KeyCode.E))
        {
            Managers.Game.AddItem(20001, 1);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Managers.Game.RemoveItem(10001, 1);
            Managers.Game.RemoveItem(20001, 1);

        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(10001);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            // TODO : 전투로 이동
            //Managers.Quest.CheckButtonClick(BTN_JOURNEY);
            //ClosePopupUI();
            Managers.UI.CloseAllPopupUI();

            UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
            gameScene?.EnterAreaWithFade(AreaType.Battle);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (Managers.Game.Player.State == State.Idle)
                Managers.Game.Player.State = State.Moving;
            else if (Managers.Game.Player.State == State.Moving)
                Managers.Game.Player.State = State.Attack;
            else if (Managers.Game.Player.State == State.Attack)
                Managers.Game.Player.State = State.Idle;
        }

#endif
    }

    [ContextMenu("Test/Unlock Bestiary Button")]
    public void TestUnlockBestiaryButton()
    {
        Managers.Game.UnlockBtmButton(TabType.Bestiary);
        RefreshBtmButtonsLockState();
        ShowNewBadge(TabType.Bestiary);
    }

    [ContextMenu("Test/Unlock All")]
    // [치트] 만능 키 — 실제 해금은 GameManager.UnlockEverything이 하고,
    //  여기서는 열린 내용이 **화면에 바로 보이도록** UI를 다시 그린다.
    //  (하단 탭 잠금, 마을 버튼 구성, 열려 있던 팝업 모두 갱신하지 않으면 다시 들어가야 보인다)
    [ContextMenu("Cheat/모든 컨텐츠 해금")]
    public void CheatUnlockEverything()
    {
        Managers.Game.UnlockEverything();

        RefreshBtmButtonsLockState();
        Managers.UI.CloseAllPopupUI();
        EnterArea(AreaType.Village1);   // 마을 버튼 그룹 재생성(연금공방/여정 반영)

        Managers.UI.ShowToast("[치트] 모든 컨텐츠 해금");
    }

    public void TestUnlockAll()
    {
        // 하단 탭 버튼 전체 해금
        foreach (TabType tab in System.Enum.GetValues(typeof(TabType)))
            Managers.Game.UnlockBtmButton(tab);

        // 마을 이동/연금공방 버튼 해금
        Managers.Game.AddUnlockedBtn(UNLOCK_TRAVEL_BUTTON);
        Managers.Game.AddUnlockedBtn(UNLOCK_ALCHEMY_BUTTON);
        Managers.Game.AddUnlockedBtn(BTN_COMMISION);   // 연금공방 제작의뢰(CustomOrder) 버튼 해금

        // 하단 탭 UI 즉시 갱신
        RefreshBtmButtonsLockState();

        // Village1 버튼 그룹 재생성 (연금공방 버튼 반영)
        EnterArea(AreaType.Village1);
    }

    [ContextMenu("Test/Jump to Quest 1")]
    public void TestJumpToQuest1() => TestJumpToQuest(1);

    [ContextMenu("Test/Jump to Quest 2")]
    public void TestJumpToQuest2() => TestJumpToQuest(2);

    [ContextMenu("Test/Jump to Quest 3")]
    public void TestJumpToQuest3() => TestJumpToQuest(3);

    void TestJumpToQuest(int questId)
    {
        Managers.Quest.JumpToQuest(questId);
    }

    [ContextMenu("Test/Gold +500")]
    public void TestGoldIncrease() => Managers.Game.Gold += 500;

    [ContextMenu("Test/Gold -500")]
    public void TestGoldDecrease() => Managers.Game.Gold = Mathf.Max(0, Managers.Game.Gold - 500);

    [ContextMenu("Test/Crystal +100")]
    public void TestCrystalIncrease() => Managers.Game.Crystal += 100;

    [ContextMenu("Test/Crystal -100")]
    public void TestCrystalDecrease() => Managers.Game.Crystal = Mathf.Max(0, Managers.Game.Crystal - 100);

    // ── HP / EXP 바 애니메이션 테스트 ──────────────────────────
    float _testHp  = 100f;
    float _testExp = 0f;
    const float TEST_MAX_HP  = 100f;
    const float TEST_MAX_EXP = 100f;

    [ContextMenu("Test/HP -30 (Hurt)")]
    public void TestHPDamage()
    {
        _testHp = Mathf.Max(0f, _testHp - 30f);
        UpdateHP(_testHp, TEST_MAX_HP);
    }

    [ContextMenu("Test/HP +20 (Heal)")]
    public void TestHPHeal()
    {
        _testHp = Mathf.Min(TEST_MAX_HP, _testHp + 20f);
        UpdateHP(_testHp, TEST_MAX_HP);
    }

    [ContextMenu("Test/EXP +25")]
    public void TestEXPGain()
    {
        _testExp = Mathf.Min(TEST_MAX_EXP, _testExp + 25f);
        UpdateEXP(_testExp, TEST_MAX_EXP);
    }
#endif
}
