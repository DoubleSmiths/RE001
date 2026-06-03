using System.Globalization;
using Data;
using UnityEngine;
using static Define;

public class UI_MonsterDetailPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        MonsterDetailPopupSummonedEf,
        MonsterDetailPopupSummonedEfImage,
        MonsterDetailPopupSummonedEfMask,
        MonsterDetailPopupSummonedEfMaskImage1,
        MonsterDetailPopupSummonedEfMaskImage2,
        MonsterDetailPopupFrame,
        MonsterDetailPopupMonsterBox,
        MonsterDetailPopupMonsterBoxMask,
        MonsterDetailPopupMonsterImage,
        AllSStar,
        MonsterDetailPopupSkillBox,
        MonsterDeatilPopupMonsterSkillFrame,
        EFrame,
        MonsterDeatilPopupMonsterSkillBackground,
        ClaimedIcon,
        ClaimedIconFrame,
        LevelIcon,
        LevelIconFrame,
        MonsterSkillLockBackground,
        MonsterSkillLockImage,
        MonsterDetailPopupInfoBox,
        MonsterDetailPopupActiveBlock,
        MonsterDetailPopupDropItemBox,
        ScrollView,
        Viewport,
        EndLineImage,
        DropItemBoxImage,
        MonsterDetailPopupCheckMinionButtonImage,
        MonsterDetailPopupCheckMask,
        Img_Item_03,
        Img_Item_01,
        Xbutton,
    }

    enum Texts
    {
        MonsterDetailPopupSummonedEfImageText,
        MonsterDetailPopupTitle,
        MonsterNumberText,
        SYNCText,
        ClaimedIconText,
        LevelIconText,
        LevelText,
        MonsterDetailPopupSkillText,
        MonsterDetailPopupSkillBoxFullLockQuestion,
        MonsterDetailPopupSkillBoxFullLockReq,
        MonsterDetailPopupSkillName,
        MonsterDetailPopupNameText,
        MonsterDetailPopupPlaceTitleText,
        MonsterDetailPopupPlaceText,
        MonsterDetailPopupHPText,
        MonsterDetailPopupDEFText,
        // 슬8: 기획서 14개 능력치 중 prefab에 없던 3개 노드 (물리저항/마법저항/마법공격력)
        MonsterDetailPopupP_RESText,
        MonsterDetailPopupM_RESText,
        MonsterDetailPopupM_ATKText,
        MonsterDetailPopupDamageText,
        MonsterDetailPopupASText,
        MonsterDetailPopupACCText,
        MonsterDetailPopupEXPText,
        MonsterDetailPopupKillCountText,
        MonsterDetailPopupAPPText,
        MonsterDetailPopupATTText,
        MonsterDetailPopupActiveText,
        MonsterDetailPopupInfoBoText,
        DropItemBoxText,
        MonsterDetailPopupCheckMaskText,
        Txt_Xbutton_01,
    }

    enum Buttons
    {
        MonsterDetailPopupBackground,
        SkillIcon,
        MonsterDetailPopupCheckMinionButton,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        MonsterDetailPopup,
        MonsterDeatilPopupMonsterSkill,
        SkillDetailActiveEf,
        MonsterDetailPopupSkillBoxLockGroup,
        MonsterDetailPopupSkillBoxFullLockGroup,
        GlowDot_01_5,
        Dust_01,
        Ring_01,
        Panel_MonsterDetailPopupInfo,
        Content,
    }

    #endregion


    int _dataId;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.MonsterDetailPopup).GetComponent<RectTransform>();
        // 진입 Anim은 MonsterDetailPopupFrame의 prefab DOTweenAnimation(autoPlay=1, isFrom)이 담당 — BestiaryPopup과 동일 패턴
        GetButton((int)Buttons.MonsterDetailPopupBackground).gameObject.BindEvent(OnClickBG);
        GetImage((int)Images.Xbutton).gameObject.BindEvent(OnClickBG);
        GetButton((int)Buttons.SkillIcon).gameObject.BindEvent(OnClickSkill);
        GetButton((int)Buttons.MonsterDetailPopupCheckMinionButton).gameObject.BindEvent(OnClickMinionButton);

        return true;
    }

    bool _skillUnlocked;

    // 슬라이드 76/77: 미니몬 기록 기본 정보 + 기초정보&확률 텍스트 + 동적 전리품 리스트
    public void SetInfo(int dataId)
    {
        _dataId = dataId;

        CreatureData data = null;
        Managers.Data.CreatureDic.TryGetValue(dataId, out data);
        int rec = UI_BestiaryPopup.GetRecordRate(dataId); // 분석률(기록도)

        // 슬8: 타이틀 = "미니몬 기록" (MinimonEntryText). prefab 하드코딩("마물 상세정보") 교체.
        var title = GetText((int)Texts.MonsterDetailPopupTitle);
        if (title != null)
        {
            title.text = Util.Localize(LocalizeKey.MinimonEntryText);
            ApplyFont(title, Define.FontType.UI);
        }

        // 슬8: 기초정보 헤더 / 소환 효과 문구 — prefab 하드코딩(영어) 교체해 한/영 전환.
        var infoHeader = GetText((int)Texts.MonsterDetailPopupInfoBoText);
        if (infoHeader != null)
        {
            infoHeader.text = Util.Localize(LocalizeKey.InfoAndStatsText);
            ApplyFont(infoHeader, Define.FontType.UI);
        }
        var activeTxt = GetText((int)Texts.MonsterDetailPopupActiveText);
        if (activeTxt != null)
        {
            activeTxt.text = Util.Localize(LocalizeKey.SummonedBonusText);
            ApplyFont(activeTxt, Define.FontType.UI);
        }

        // 슬라이드 77: "NO." 고정 + CreatureData ID 앞 2 뺀 3자리 (2001 → NO.001)
        var numTxt = GetText((int)Texts.MonsterNumberText);
        if (numTxt != null)
        {
            numTxt.text = $"NO.{(dataId % 1000):000}";
            ApplyFont(numTxt, Define.FontType.UI);
        }

        int sync = UI_BestiaryPopup.GetSync(dataId); // 공명도 (슬라이드 79)

        // 슬라이드 78: 기록도 0%면 정보 잠금 — 몬스터 IMG 검정 + 기초정보 "???"
        bool statsUnlocked = rec > 0;
        bool lootUnlocked  = rec >= 100;

        // 몬스터 IMG (IconLabel). 잠금 시 검정 처리(슬라이드 78 #1), 해금 시 색 복귀.
        var monImg = GetImage((int)Images.MonsterDetailPopupMonsterImage);
        if (monImg != null)
        {
            if (data != null && !string.IsNullOrEmpty(data.IconLabel))
            {
                Sprite sprite = Managers.Resource.Load<Sprite>(data.IconLabel);
                if (sprite != null) { monImg.sprite = sprite; monImg.preserveAspect = true; }
            }
            monImg.color = statsUnlocked ? Color.white : new Color32(0, 0, 0, 255);
        }

        // 슬8/77/78: 기초정보 & 확률 (InfoAndStatsText). 기획서 14개 능력치 전부 표기.
        //  - 라벨은 LocalizationData로 한/영 자동 전환, 값만 잠금 시 "???"
        //  - 기호: 이름/서식지 = "○", 그 외 능력치 = "◆"
        //  - 색: 86FFFA (분석률만 기록도 색 별도)
        bool u = statsUnlocked && data != null;
        SetStatLine(Texts.MonsterDetailPopupNameText, "○", LocalizeKey.MinimonNameText,        u ? data.Name                            : "???");
        SetHabitat(u ? data : null);
        SetStatLine(Texts.MonsterDetailPopupHPText,   "◆", LocalizeKey.MaxHitPointText,        u ? Fmt(data.MaxHp)                      : "???");
        SetStatLine(Texts.MonsterDetailPopupDEFText,  "◆", LocalizeKey.DefenseText,            u ? Fmt(data.Def)                        : "???");
        SetStatLine(Texts.MonsterDetailPopupP_RESText,"◆", LocalizeKey.PhysicalResistanceText, u ? $"{Fmt(data.PhysicalResist)}%"        : "???");
        SetStatLine(Texts.MonsterDetailPopupM_RESText,"◆", LocalizeKey.MagicalResistanceText,  u ? $"{Fmt(data.MagicResist)}%"           : "???");
        SetStatLine(Texts.MonsterDetailPopupDamageText,"◆",LocalizeKey.PhysicalAttackText,     u ? Fmt(data.PhysicalAtk)                : "???");
        SetStatLine(Texts.MonsterDetailPopupM_ATKText,"◆", LocalizeKey.MagicalAttackText,      u ? Fmt(data.MagicAtk)                   : "???");
        SetStatLine(Texts.MonsterDetailPopupASText,   "◆", LocalizeKey.AttackSpeedText,        u ? Fmt(data.AtkSpeed)                   : "???");
        SetStatLine(Texts.MonsterDetailPopupACCText,  "◆", LocalizeKey.AccuracyText,           u ? Fmt(data.Accuracy)                   : "???");
        SetStatLine(Texts.MonsterDetailPopupEXPText,  "◆", LocalizeKey.EXPText,                u ? Fmt(data.TotalExp)                   : "???");
        SetStatLine(Texts.MonsterDetailPopupAPPText,  "◆", LocalizeKey.EncounterText,          u ? $"{Fmt(data.EncounterRate * 100f)}%" : "???");
        SetStatLine(Texts.MonsterDetailPopupATTText,  "◆", LocalizeKey.AttentionRateText,      u ? $"{Fmt(data.InterestRate * 100f)}%"  : "???");

        // 분석률(기록도) — 항상 표기. 100% 미만 EC9907 / 100% 00FF00 (슬8 LogText / 슬78)
        SetStatLine(Texts.MonsterDetailPopupKillCountText, "◆", LocalizeKey.LogText, $"{rec}%",
                rec >= 100 ? Util.HexToColor("00FF00") : Util.HexToColor("EC9907"));

        // 슬8: 공명도 = "{LinkText}:{공명도}%" (콜론 뒤 공백 없음). 100% 미만 FF820B / 100% 30FF00.
        //  (100% 시 MAT Text_ShineGreen 지정 스펙 — 머티리얼 에셋이 Resources 밖이라 런타임 로드 보류)
        var syncTxt = GetText((int)Texts.SYNCText);
        if (syncTxt != null)
        {
            syncTxt.text = $"{Util.Localize(LocalizeKey.LinkText)}:{sync}%";
            syncTxt.color = sync >= 100 ? Util.HexToColor("30FF00") : Util.HexToColor("FF820B");
            ApplyFont(syncTxt, Define.FontType.UI);
        }

        RefreshSkillGate(sync);
        RefreshLearned(dataId);
        RefreshDropList(data, lootUnlocked);
        RefreshMinionButton(sync > 0); // 슬라이드 81: 포획 = 공명도 > 0
    }

    bool _captured;

    // 슬라이드 81: [나의 미니몬] 버튼 — 포획 유무에 따른 분기
    //  포획 X : interactive 불가, gray mask, glow 000000, "포획 필요" 류 안내
    //  포획 O : interactive 가능, mask 제거, glow 8E5E00, "내 미니몬" Outline F5A200 α146 (0.1,-0.1)
    void RefreshMinionButton(bool captured)
    {
        _captured = captured;

        var btn = GetButton((int)Buttons.MonsterDetailPopupCheckMinionButton);
        if (btn != null) btn.interactable = captured;

        // gray 마스크 (포획 전엔 보이게)
        var mask = GetImage((int)Images.MonsterDetailPopupCheckMask);
        if (mask != null) mask.gameObject.SetActive(!captured);

        // shop_frame_glow 색 (포획 O: 8E5E00 / 포획 X: 000000)
        var glow = GetImage((int)Images.MonsterDetailPopupCheckMinionButtonImage);
        if (glow != null)
            glow.color = captured ? Util.HexToColor("8E5E00") : new Color32(0, 0, 0, 255);

        var maskTxt = GetText((int)Texts.MonsterDetailPopupCheckMaskText);
        if (maskTxt != null)
        {
            maskTxt.text = captured ? "내 미니몬" : "포획 필요"; // 이것도 나중엔 Localization 해야겠지..
            ApplyFont(maskTxt, Define.FontType.UI);
            // Outline (있을 경우): 포획 O 시 F5A200 α146 (0.1,-0.1) / 포획 X 시 끄기
            var outline = maskTxt.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                outline.effectColor = captured
                    ? new Color32(0xF5, 0xA2, 0x00, 146)
                    : new Color32(0, 0, 0, 0);
                outline.effectDistance = new Vector2(0.1f, -0.1f);
            }
        }
    }

    void OnClickMinionButton()
    {
        if (!_captured)
        {
            // 슬라이드 81: 포획 안한 미니몬 — Interactive X
            Debug.Log("[MonsterDetail] 미니몬 미포획 — 버튼 클릭 무시");
            return;
        }
        // 슬라이드 84~88: [나의 미니몬] 창 호출 (UI_MinionInfoPopup)
        Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(_dataId);
    }

    // 슬라이드 79/80: 공명도 100% 달성 시 영구 학습. 모든 능력치 SS면 Lv.2(완벽획득).
    //  부분획득: 부분획득 / +1등급(00FF00) / Frame 36FF00 (슬라이드 79)
    //  완벽획득: 완벽획득 / +2등급(FFFFFF) / Frame FF0000 (슬라이드 80)
    void RefreshLearned(int dataId)
    {
        bool learned = UI_BestiaryPopup.IsUniqueSkillLearned(dataId);
        bool perfect = learned && UI_BestiaryPopup.IsAllSS(dataId);

        GetImage((int)Images.ClaimedIcon).gameObject.SetActive(learned);
        GetImage((int)Images.ClaimedIconFrame).gameObject.SetActive(learned);
        GetImage((int)Images.LevelIcon).gameObject.SetActive(learned);
        GetImage((int)Images.LevelIconFrame).gameObject.SetActive(learned);

        // 프레임(HP_MP_little_frame) 색: 부분획득 36FF00 / 완벽획득 FF0000
        var claimedFrame = GetImage((int)Images.ClaimedIconFrame);
        if (claimedFrame != null && learned)
            claimedFrame.color = perfect ? Util.HexToColor("FF0000") : Util.HexToColor("36FF00");

        var claimedTxt = GetText((int)Texts.ClaimedIconText);
        if (claimedTxt != null)
        {
            claimedTxt.gameObject.SetActive(learned);
            if (learned)
            {
                claimedTxt.text = perfect ? "완벽획득" : "부분획득";
                ApplyFont(claimedTxt, Define.FontType.UI);
            }
        }

        var lvlIconTxt = GetText((int)Texts.LevelIconText);
        if (lvlIconTxt != null)
        {
            lvlIconTxt.gameObject.SetActive(learned);
            if (learned)
            {
                // LevelIconText 노란 등급 표기 — 완벽 시 Lv.2
                lvlIconTxt.text = perfect ? "<color=yellow>2</color>등급" : "<color=yellow>1</color>등급";
                ApplyFont(lvlIconTxt, Define.FontType.UI);
            }
        }

        var lvlTxt = GetText((int)Texts.LevelText);
        if (lvlTxt != null)
        {
            lvlTxt.gameObject.SetActive(learned);
            if (learned)
            {
                lvlTxt.text = perfect ? "+2등급" : "+1등급";
                lvlTxt.color = perfect ? Color.white : Util.HexToColor("00FF00");
                ApplyFont(lvlTxt, Define.FontType.UI);
            }
        }
    }

    // 슬라이드 79: 공명도에 따른 고유기술 게이팅
    //  sync == 0     : FullLock (정보 완전 가림, interactive X)
    //  0 < sync < 100 : Lock (정보는 보이나 interactive X)
    //  sync == 100   : 해금 (interactive O — 클릭 시 기술 상세창)
    void RefreshSkillGate(int sync)
    {
        bool fullLock = sync <= 0;
        bool unlocked = sync >= 100;
        _skillUnlocked = unlocked;

        GetObject((int)GameObjects.MonsterDetailPopupSkillBoxFullLockGroup).SetActive(fullLock);
        GetObject((int)GameObjects.MonsterDetailPopupSkillBoxLockGroup).SetActive(!fullLock && !unlocked);

        // 잠금 동안 어둡게 덮는 배경/이미지 (해금 시 제거)
        GetImage((int)Images.MonsterSkillLockBackground).gameObject.SetActive(!unlocked);
        GetImage((int)Images.MonsterSkillLockImage).gameObject.SetActive(!unlocked);

        var skillBtn = GetButton((int)Buttons.SkillIcon);
        if (skillBtn != null) skillBtn.interactable = unlocked;

        // FullLock "?" + 안내문(6C6C6C, 슬라이드 78)
        var q = GetText((int)Texts.MonsterDetailPopupSkillBoxFullLockQuestion);
        if (q != null) { q.text = "?"; ApplyFont(q, Define.FontType.UI); }
        var req = GetText((int)Texts.MonsterDetailPopupSkillBoxFullLockReq);
        if (req != null) { req.color = Util.HexToColor("6C6C6C"); ApplyFont(req, Define.FontType.UI); }

        // 슬라이드 79: 고유기술 이름 — CreatureData.UniqueSkillName (FullLock 시엔 위 UI가 덮음)
        var skillName = GetText((int)Texts.MonsterDetailPopupSkillName);
        if (skillName != null)
        {
            skillName.text = UI_BestiaryPopup.GetUniqueSkillName(_dataId);
            ApplyFont(skillName, Define.FontType.UI);
        }
    }

    void OnClickSkill()
    {
        if (!_skillUnlocked)
        {
            // 슬라이드 78/79: 공명도 100% 전까지 고유기술 interactive X
            Debug.Log("[MonsterDetail] 고유기술 잠금 상태 — 클릭 무시");
            return;
        }
        // 슬라이드 78: 해금 시 클릭 → 해당 기술정보 창
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(_dataId);
    }

    // 슬라이드 77: 전리품 동적 리스트 (UI_MonsterItemDropListBox)
    void RefreshDropList(CreatureData data, bool unlocked)
    {
        Transform content = GetObject((int)GameObjects.Content).transform;

        // prefab 템플릿 포함 기존 자식 제거
        for (int i = content.childCount - 1; i >= 0; i--)
            Managers.Resource.Destroy(content.GetChild(i).gameObject);

        if (data == null || data.DropItemId == 0)
            return;

        ItemData item = null;
        Managers.Data.ItemDic.TryGetValue(data.DropItemId, out item);

        var box = Managers.UI.MakeSubItem<UI_MonsterItemDropListBox>(content, pooling: false);
        box.SetInfo(item, data.DropCount, data.DropChance, unlocked);
    }

    // 슬8: "{기호} {로컬라이즈 라벨}: {값}" 형식으로 한 줄 전체를 새로 구성.
    //  기존엔 prefab에 하드코딩된 라벨(○ HP: 등)을 그대로 보존해 한/영 전환이 안 됐음 → 라벨을 LocalizationData로 교체.
    //  줄바꿈 접미사("\n")는 prefab 레이아웃 유지를 위해 보존. 값 색은 미지정 시 86FFFA(InfoAndStatsText).
    void SetStatLine(Texts t, string symbol, string locKey, string value, Color? color = null)
    {
        var tmp = GetText((int)t);
        if (tmp == null) return; // prefab 노드 미존재(예: 신설 전 P_RES/M_RES/M_ATK) 시 무시

        bool nl = (tmp.text ?? "").EndsWith("\n");
        tmp.text = $"{symbol} {Util.Localize(locKey)}: {value}" + (nl ? "\n" : "");
        //tmp.color = color ?? Util.HexToColor("86FFFA");
        tmp.fontSizeMax = 6; // 슬8: 이름/스탯 폰트 크기 통일(이름 노드가 7이라 더 컸던 문제)
        ApplyFont(tmp, Define.FontType.UI);
    }

    // 슬8: ○ 서식지. prefab은 라벨(PlaceTitleText)/값(PlaceText)이 별도 노드라 값이 x=41에 고정 →
    //  라벨과 값 사이가 크게 벌어졌음(기획서는 "○ 서식지: 산들바람 숲" 처럼 붙어있음).
    //  → 라벨+값을 한 노드(PlaceTitleText)에 합쳐 표기하고 PlaceText는 비움.
    //  값은 CreatureData.Habitat에 담긴 LocalizationData TextKey를 다시 Localize → 값까지 한/영 지원.
    void SetHabitat(CreatureData data)
    {
        string val = data == null ? "???"
                   : string.IsNullOrEmpty(data.Habitat) ? "-"
                   : Util.Localize(data.Habitat);

        var titleTxt = GetText((int)Texts.MonsterDetailPopupPlaceTitleText);
        if (titleTxt != null)
        {
            titleTxt.text = $"○ {Util.Localize(LocalizeKey.HabitatText)}: {val}";
            //titleTxt.color = Util.HexToColor("86FFFA");
            titleTxt.fontSizeMax = 6; // 스탯과 폰트 크기 통일(기존 7)
            ApplyFont(titleTxt, Define.FontType.UI);
        }

        var placeTxt = GetText((int)Texts.MonsterDetailPopupPlaceText);
        if (placeTxt != null) placeTxt.text = ""; // 값은 타이틀 노드에 합침 → 빈 노드로
    }

    // 정수면 정수로, 아니면 소수 1자리
    static string Fmt(float v)
    {
        if (Mathf.Approximately(v, Mathf.Round(v)))
            return Mathf.RoundToInt(v).ToString();
        return v.ToString("0.#", CultureInfo.InvariantCulture);
    }
}
