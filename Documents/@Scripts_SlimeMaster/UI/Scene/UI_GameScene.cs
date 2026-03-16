using Data;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class UI_GameScene : UI_Scene
{
    #region UI 기능 리스트
    // 정보 갱신
    // GoldValueText : 습득 골드 연결 (골드를 먹을때 마다 리프레쉬)
    // KillValueText : 죽인 적 수 연결 (킬 할 때 마다 리프레시) 
    // WaveValueText : 현재 진행중인 웨이브 수
    // TimeLimitValueText : 다음 웨이브 시작 까지 남은 시간 (초마다 갱신)
    // SupportSkillCardListObject : 영혼상점의 서포트 카드가 들어가는 부모개체
    // ResetCostValueText : 영혼 상점의 서포트 카드 리스트 리셋 비용 (-n으로 표기, 리셋마다 150씩 늘어남, 기본 150 )
    // TotalDamageContentObject: UI_SkillDamageItem의 부모개체

    //  BattleSkillCountValueText : 배틀 스킬의 개수 표시
    //  SupportSkillCountValueText : 서포트 스킬의 개수 표시
    // SupportSkillListScrollContentObject : 배운 서포트 스킬들이 들어갈 부모개체

    // 웨이브 알람
    // MonsterAlarmObject : 다음 웨이브가 시작 3초전 3초간 출력 (애니 추가 예정)
    // BossAlarmObject : 보스전 시작 3초전 3초간 출력 (애니 추가 예정)

    // 엘리트 정보
    // EliteInfoObject : 엘리트 등장 시 활성화
    // EliteHpSliderObject : 등장한 엘리트의 현재 체력% 연결
    // EliteNameValueText : 등장한 엘리트의 이름

    // 보스 정보
    // BossInfoObject : 보스 등장 시 활성화
    // BossHpSliderObject : 등장한 보스의 현재 체력% 연결
    // BossNameValueText : 등장한 보스의 이름


    // 로컬라이징
    // CardListResetText : 초기화
    // MonsterCommentText : 몬스터가 몰려옵니다
    // BossCommentText : 보스 등장
    // OwnBattleSkillInfoText : 전투 스킬
    // OwnSupportSkillInfoText : 서포트 스킬

    #endregion

    #region Enum
    enum GameObjects
    {
        ExpSliderObject, // 슬라이더로 변경 필요
        WaveObject,
        SoulImage,
        OnDamaged,
        SoulShopObject,
        SupportSkillCardListObject,
        OwnBattleSkillInfoObject,
        //TotalDamageContentObject,
        SupportSkillListScrollObject,
        SupportSkillListScrollContentObject,
        WhiteFlash,
        MonsterAlarmObject,
        BossAlarmObject,

        EliteInfoObject,
        EliteHpSliderObject,
        BossInfoObject,
        BossHpSliderObject,

        BattleSkillSlotGroupObject,
    }
    enum Buttons
    {
        PauseButton,
        DevelopButton,
        SoulShopButton,
        CardListResetButton,
        SoulShopCloseButton,
        TotalDamageButton,
        SupportSkillListButton,
        SoulShopLeadButton,
        SoulShopBackgroundButton,
    }
    enum Texts
    {
        WaveValueText,
        TimeLimitValueText,
        SoulValueText,
        KillValueText,
        CharacterLevelValueText,
        CardListResetText,
        ResetCostValueText,
        //BattleSkillCountValueText,
        SupportSkillCountValueText,

        EliteNameValueText,
        BossNameValueText,

        MonsterCommentText,
        BossCommentText,
    }
    enum Images
    {
        BattleSkilI_Icon_0,
        BattleSkilI_Icon_1,
        BattleSkilI_Icon_2,
        BattleSkilI_Icon_3,
        BattleSkilI_Icon_4,
        BattleSkilI_Icon_5,
        SupportSkilI_Icon_0,
        SupportSkilI_Icon_1,
        SupportSkilI_Icon_2,
        SupportSkilI_Icon_3,
        SupportSkilI_Icon_4,
        SupportSkilI_Icon_5,
    }
    enum AlramType
    {
        wave,
        boss
    }
    #endregion

    GameManager _game;
    Coroutine _coWaveAlarm;

    bool _isSupportSkillListButton = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Object Bind

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.PauseButton).gameObject.BindEvent(OnClickPauseButton);
        GetButton((int)Buttons.PauseButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SoulShopButton).gameObject.BindEvent(OnClickSoulShopButton);
        GetButton((int)Buttons.CardListResetButton).gameObject.BindEvent(OnClickCardListResetButton);
        GetButton((int)Buttons.CardListResetButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SoulShopLeadButton).gameObject.BindEvent(OnClickSoulShopButton);
        GetButton((int)Buttons.SoulShopLeadButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SoulShopCloseButton).gameObject.BindEvent(OnClickSoulShopCloseButton);
        GetButton((int)Buttons.SoulShopCloseButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.TotalDamageButton).gameObject.BindEvent(OnClickTotalDamageButton);
        GetButton((int)Buttons.TotalDamageButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SupportSkillListButton).gameObject.BindEvent(OnClickSupportSkillListButton);
        GetButton((int)Buttons.SupportSkillListButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SoulShopCloseButton).gameObject.SetActive(false); // 영혼 상점 버튼 초기 상태
        GetButton((int)Buttons.SoulShopLeadButton).gameObject.SetActive(true); // 영혼 상점 버튼 초기 상태
        GetButton((int)Buttons.SoulShopBackgroundButton).gameObject.BindEvent(OnClickSoulShopCloseButton);
        GetButton((int)Buttons.SoulShopBackgroundButton).gameObject.SetActive(false); // 영혼 상점 배경
        GetObject((int)GameObjects.OwnBattleSkillInfoObject).gameObject.SetActive(false); // 배틀 스킬 리스트 초기 상태
        GetObject((int)GameObjects.SupportSkillListScrollObject).gameObject.SetActive(false); // 서포트 스킬 리스트 초기 상태
        _isSupportSkillListButton = false;

        GetObject((int)GameObjects.MonsterAlarmObject).gameObject.SetActive(false); // 몬스터 웨이브 알람 초기 상태
        GetObject((int)GameObjects.BossAlarmObject).gameObject.SetActive(false); // 보스 알람 초기 상태

        GetObject((int)GameObjects.EliteInfoObject).gameObject.SetActive(false); // 엘리트 정보(체력바) 초기 상태
        GetObject((int)GameObjects.BossInfoObject).gameObject.SetActive(false); // 보스 정보(체력바) 초기 상태

        #endregion

        _game = Managers.Game;

        Managers.Game.Player.OnPlayerDataUpdated = OnPlayerDataUpdated;
        //Managers.Game.Player.OnMonsterDataUpdated = OnOnMonsterDataUpdated;
        Managers.Game.Player.OnPlayerLevelUp = OnPlayerLevelUp;
        Managers.Game.Player.OnPlayerDamaged = OnDamaged;

        GetComponent<Canvas>().worldCamera = Camera.main; 

        return true;
    }

    private void Awake()
    {
        Init();
        Managers.Game.Player.Skills.UpdateSkillUi += OnLevelUpSkill;
        Managers.Game.Player.OnPlayerMove += OnPlayerMove;
        Refresh();
    }

    private void OnDestroy()
    {
        if (Managers.Game.Player != null)
        {
            Managers.Game.Player.Skills.UpdateSkillUi -= OnLevelUpSkill;
            Managers.Game.Player.OnPlayerMove -= OnPlayerMove;
        }
    }



    public void SetInfo() // 데이터 받아올때
    {
        Refresh();
    }

    void Refresh() // 데이터 갱신
    {
        // 보유 스킬 정보 갱신
        SetBattleSkill();
        GetObject((int)GameObjects.ExpSliderObject).GetComponent<Slider>().value = _game.Player.ExpRatio;
        GetText((int)Texts.CharacterLevelValueText).text = $"{_game.Player.Level}";
        GetText((int)Texts.KillValueText).text = $"{_game.Player.KillCount}";
        GetText((int)Texts.SoulValueText).text = _game.Player.SoulCount.ToString();
        // 웨이브 리프레시 버그 대응
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.WaveObject).GetComponent<RectTransform>());
    }

    void OnLevelUpSkill()
    {
        ClearSkillSlot();

        //배틀스킬아이콘
        List<SkillBase> activeSkills = Managers.Game.Player.Skills.SkillList.Where(skill => skill.IsLearnedSkill).ToList();
        for (int i = 0; i < activeSkills.Count; i++)
            AddSkillSlot(i, activeSkills[i].SkillData.IconLabel);

        //GetText((int)Texts.BattleSkillCountValueText).text = activeSkills.Count.ToString();

        //서포트스킬 
        int index = 6;// 서포트스킬 enum이 6번부터 시작함
        int count = Mathf.Min(6, Managers.Game.Player.Skills.SupportSkills.Count);
        for (int i = 0; i < count; i++)
            AddSkillSlot(i + index, Managers.Game.Player.Skills.SupportSkills[i].IconLabel);

        GetText((int)Texts.SupportSkillCountValueText).text = Managers.Game.Player.Skills.SupportSkills.Count.ToString();

    }

    public void SetBattleSkill()
    {

        GameObject container = GetObject((int)GameObjects.BattleSkillSlotGroupObject);
        //초기화
        foreach (Transform child in container.transform)
            Managers.Resource.Destroy(child.gameObject);

        //전투스킬
        foreach (SkillBase skill in Managers.Game.Player.Skills.ActivatedSkills)
        {
            UI_SkillSlotItem item = Managers.UI.MakeSubItem<UI_SkillSlotItem>(container.transform);
            item.GetComponent<UI_SkillSlotItem>().SetUI(skill.SkillData.IconLabel, skill.Level);
        }

    } // 배틀 스킬 리스트 정보 갱신

    void OnClickPauseButton() // 일시 정지 버튼
    {
        Managers.Sound.PlayButtonClick();
        Managers.UI.ShowPopupUI<UI_PausePopup>();
    }

    void OnClickSoulShopButton() // 영혼 상점 버튼
    {
        Managers.Sound.Play(Define.Sound.Effect, "PopupOpen_SoulShop");
        //상점 활성화
        SetActiveSoulShop(true);

        if (Managers.Game.ContinueInfo.SoulShopList.Count == 0)
            ResetSupportCard();
        else
            LoadSupportCard();

        Refresh();
    }

    void OnClickCardListResetButton() // 영혼상점 리스트 리셋 버튼
    {
        Managers.Sound.PlayButtonClick();
        // 서포트 카드 리스트 내용 리셋
        int cardListResetCost = (int)Define.SOUL_SHOP_COST_PROB[6];
        if (Managers.Game.Player.SoulCount >= cardListResetCost)
        {
            Managers.Game.Player.SoulCount -= cardListResetCost;
            ResetSupportCard();
        }   
    }

    void OnClickCardListResetTestButton() // 테스트용 영혼상점 리스트 리셋 버튼 
    {
        Managers.Sound.PlayButtonClick();

        // 서포트 카드 리스트 내용 리셋
        ResetSupportCard();
    }

    void OnClickSoulShopCloseButton() // 영혼 상점 닫기 버튼
    {
        Managers.Sound.PlayButtonClick();

        //상점 비활성화
        SetActiveSoulShop(false);
    }

    public void OnPlayerMove()
    {
        Vector2 uiPos = GetObject((int)GameObjects.SoulImage).transform.position;
        Managers.Game.SoulDestination = uiPos;
    }


    void OnClickTotalDamageButton() // 토탈 데미지 버튼
    {
        Managers.Sound.PlayButtonClick();
        Managers.UI.ShowPopupUI<UI_TotalDamagePopup>().SetInfo();
    }

    void ResetSupportSkillList()
    {
        GameObject container = GetObject((int)GameObjects.SupportSkillListScrollContentObject);
        container.DestroyChilds();

        List<Data.SupportSkillData> temp = Managers.Game.Player.Skills.SupportSkills.OrderByDescending(x => x.DataId).ToList();
        foreach (Data.SupportSkillData skill in temp)
        {
            UI_SupportSkillItem item = Managers.UI.MakeSubItem<UI_SupportSkillItem>(container.transform);
            ScrollRect scrollRect = GetObject((int)GameObjects.SupportSkillListScrollObject).GetComponent<ScrollRect>();
            item.SeteInfo(skill, this.transform, scrollRect);
            Managers.Game.SoulShopList.Add(skill);
        }
    }

    void ResetSupportCard()
    {
        #region 서포트스킬 카드 네장 보여주기
        GameObject supportSkillContainer = GetObject((int)GameObjects.SupportSkillCardListObject);
        supportSkillContainer.DestroyChilds();
        Managers.Game.SoulShopList.Clear();

        foreach (SupportSkillData supportSkill in Managers.Game.Player.Skills.RecommendSupportkills())
        {
            supportSkill.IsPurchased = false;
            UI_SupportCardItem skillData = Managers.UI.MakeSubItem<UI_SupportCardItem>(supportSkillContainer.transform);
            skillData.SetInfo(supportSkill);
        }

        #endregion
    }

    void LoadSupportCard()
    {
        #region 서포트스킬 카드 네장 보여주기
        GameObject supportSkillContainer = GetObject((int)GameObjects.SupportSkillCardListObject);
        supportSkillContainer.DestroyChilds();

        foreach (SupportSkillData supportSkill in Managers.Game.SoulShopList)
        {
            UI_SupportCardItem skillData = Managers.UI.MakeSubItem<UI_SupportCardItem>(supportSkillContainer.transform);
            skillData.SetInfo(supportSkill);
        }

        #endregion
    }

    void AddSkillSlot(int index, string iconLabel)
    {
        GetImage(index).sprite = Managers.Resource.Load<Sprite>(iconLabel);
        GetImage(index).enabled = true;
    }

    void ClearSkillSlot()
    {
        int count = Enum.GetValues(typeof(Images)).Length;
        for (int i = 0; i < count; i++)
        {
            GetImage(i).enabled = false;
        }
    }
    void SetActiveSoulShop(bool isActive)
    {
        //float height = 0;
        if (isActive)
        { 
            SoulShopInit(); // 영혼 상점 초기 상태
            GetComponent<Canvas>().sortingOrder = Define.UI_GAMESCENE_SORT_OPEN;
        }
        else
        {
            GetComponent<Canvas>().sortingOrder = Define.UI_GAMESCENE_SORT_CLOSED;

            // 영혼 상점 초기 상태
            GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1); 
            GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0); 
            GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0); 
            GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); 

            PopupOpenAnimation(GetObject((int)GameObjects.SoulShopObject));
            GetButton((int)Buttons.SoulShopCloseButton).gameObject.SetActive(false); // 영혼 상점 버튼 비활성화
            GetButton((int)Buttons.SoulShopLeadButton).gameObject.SetActive(true);
            GetButton((int)Buttons.SoulShopBackgroundButton).gameObject.SetActive(false); // 영혼 상점 배경
            GetObject((int)GameObjects.OwnBattleSkillInfoObject).gameObject.SetActive(false); // 배틀 스킬 리스트 비활성화
            GetObject((int)GameObjects.SupportSkillListScrollObject).gameObject.SetActive(false); // 서포트 스킬 리스트 비활성화
            _isSupportSkillListButton = false;
            Managers.UI.IsActiveSoulShop = false;
        }
    }

    void SoulShopInit() // 영혼 상점 초기 상태
    {
        // 영혼 상점 플로팅
        GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1); 
        GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0); 
        GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0); 
        GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 700); 

        PopupOpenAnimation(GetObject((int)GameObjects.SoulShopObject));
        GetButton((int)Buttons.SoulShopCloseButton).gameObject.SetActive(true); // 영혼 상점 버튼 활성화
        GetButton((int)Buttons.SoulShopLeadButton).gameObject.SetActive(false);
        GetButton((int)Buttons.SoulShopBackgroundButton).gameObject.SetActive(true); // 영혼 상점 배경
        GetObject((int)GameObjects.OwnBattleSkillInfoObject).gameObject.SetActive(true); // 배틀 스킬 리스트 활성화
        PopupOpenAnimation(GetObject((int)GameObjects.OwnBattleSkillInfoObject));
        GetObject((int)GameObjects.SupportSkillListScrollObject).gameObject.SetActive(false); // 서포트 스킬 리스트 비활성화
        _isSupportSkillListButton = false;
        Managers.UI.IsActiveSoulShop = true;
    }

    void OnClickSupportSkillListButton() // 서포트 스킬 리스트 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (_isSupportSkillListButton) // 이미 눌렀다면 닫기
            SoulShopInit(); // 영혼 상점 초기 상태
        else // 누르지 않았다면 서포트 스킬 리스트 열기
        {
            // 서포트 스킬 리스트 상태
            GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1); 
            GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0); 
            GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0); 
            GetObject((int)GameObjects.SoulShopObject).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 1120); 

            PopupOpenAnimation(GetObject((int)GameObjects.SoulShopObject));
            GetObject((int)GameObjects.OwnBattleSkillInfoObject).gameObject.SetActive(false); // 배틀 스킬 리스트 비활성화
            GetObject((int)GameObjects.SupportSkillListScrollObject).gameObject.SetActive(true); // 서포트 스킬 리스트 활성화
            _isSupportSkillListButton = true;

            ResetSupportSkillList();
        }
    }

    #region Actions
    void OnPlayerDataUpdated()
    {
        GetObject((int)GameObjects.ExpSliderObject).GetComponent<Slider>().value = _game.Player.ExpRatio;
        GetText((int)Texts.KillValueText).text = $"{_game.Player.KillCount}";
        GetText((int)Texts.SoulValueText).text = _game.Player.SoulCount.ToString();
    }

    void OnPlayerLevelUp()
    {
        if (Managers.Game.IsGameEnd) return;

        List<SkillBase> list = Managers.Game.Player.Skills.RecommendSkills();

        if (list.Count > 0)
        {
            Managers.UI.ShowPopupUI<UI_SkillSelectPopup>();
        }

        //GetObject((int)GameObjects.ExpSliderObject).GetComponent<Slider>().value = (float)_game.Player.Exp / _game.Player.TotalExp;
        GetObject((int)GameObjects.ExpSliderObject).GetComponent<Slider>().value = _game.Player.ExpRatio;
        GetText((int)Texts.CharacterLevelValueText).text = $"{_game.Player.Level}";
    }

    public void MonsterInfoUpdate(MonsterController monster)
    {
        if (monster.ObjectType == Define.ObjectType.EliteMonster)
        {
            if (monster.CreatureState != Define.CreatureState.Dead)
            {
                GetObject((int)GameObjects.EliteInfoObject).SetActive(true);
                GetObject((int)GameObjects.EliteHpSliderObject).GetComponent<Slider>().value = monster.Hp / monster.MaxHp;
                GetText((int)Texts.EliteNameValueText).text = monster.CreatureData.DescriptionTextID;
            }
            else
            {
                GetObject((int)GameObjects.EliteInfoObject).SetActive(false);
            }
        }
        else if (monster.ObjectType == Define.ObjectType.Boss)
        {
            if (monster.CreatureState != Define.CreatureState.Dead)
            {
                GetObject((int)GameObjects.BossInfoObject).SetActive(true);
                GetObject((int)GameObjects.BossHpSliderObject).GetComponent<Slider>().value = monster.Hp / monster.MaxHp;
                GetText((int)Texts.BossNameValueText).text = monster.CreatureData.DescriptionTextID;
            }
            else
            {
                GetObject((int)GameObjects.BossInfoObject).SetActive(false);
            }
        }
    }

    public void OnWaveStart(int currentStageIndex)
    {
        GetText((int)Texts.WaveValueText).text = currentStageIndex.ToString();
    }

    public void OnSecondChange(int time)
    {
        //웨이브 시작 3초전 웨이브 알람
        if (time == 3 && Managers.Game.CurrentWaveIndex < 9)
        {
            StartCoroutine(SwitchAlarm(AlramType.wave));
        }

        //보스 등장 3초전 보스 알람
        if (_game.CurrentWaveData.BossId.Count > 0)
        {
            int bossGenTime = Define.BOSS_GEN_TIME;
            if (time == bossGenTime)
                StartCoroutine(SwitchAlarm(AlramType.boss));
        }
        GetText((int)Texts.TimeLimitValueText).text = time.ToString();
        if (time == 0)
            GetText((int)Texts.TimeLimitValueText).text = "";
    }

    public void OnWaveEnd()
    {
        // 몬스터 웨이브 알람 
        GetObject((int)GameObjects.MonsterAlarmObject).gameObject.SetActive(false);
    }

    public void OnDamaged()
    {
        StartCoroutine(CoBloodScreen());
    }
    public void DoWhiteFlash()
    {
        StartCoroutine(CoWhiteScreen());
    }

    #endregion

    IEnumerator CoBloodScreen()
    {
        Color targetColor = new Color(1, 0, 0, 0.3f);

        yield return null;
        Sequence seq = DOTween.Sequence();

        seq.Append(GetObject((int)GameObjects.OnDamaged).GetComponent<Image>().DOColor(targetColor, 0.3f))
            .Append(GetObject((int)GameObjects.OnDamaged).GetComponent<Image>().DOColor(Color.clear, 0.3f)).OnComplete(() => { });
    }

    IEnumerator CoWhiteScreen()
    {
        Color targetColor = new Color(1, 1, 1, 1f);

        yield return null;
        Sequence seq = DOTween.Sequence();

        seq.Append(GetObject((int)GameObjects.WhiteFlash).GetComponent<Image>().DOFade(1, 0.1f))
            .Append(GetObject((int)GameObjects.WhiteFlash).GetComponent<Image>().DOFade(0, 0.2f)).OnComplete(() => { });
    }

    IEnumerator SwitchAlarm(AlramType type)
    {
        switch (type)
        {
            case AlramType.wave:
                Managers.Sound.Play(Define.Sound.Effect, "Warning_Wave");
                GetObject((int)GameObjects.MonsterAlarmObject).gameObject.SetActive(true);
                yield return new WaitForSeconds(3f);
                GetObject((int)GameObjects.MonsterAlarmObject).gameObject.SetActive(false);
                break;
            case AlramType.boss:
                Managers.Sound.Play(Define.Sound.Effect, "Warning_Boss");
                GetObject((int)GameObjects.BossAlarmObject).gameObject.SetActive(true);
                yield return new WaitForSeconds(3f);
                GetObject((int)GameObjects.BossAlarmObject).gameObject.SetActive(false);
                break;
        }
    }


}
