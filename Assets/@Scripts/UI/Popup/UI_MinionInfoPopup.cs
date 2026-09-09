using System.Collections;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI_BestiaryPopup;

// [나의 미니몬] 팝업 (기획서 슬라이드 84~91).
// UI_MonsterDetailPopup의 [내 미니몬] 버튼 클릭 시 호출. 잡은 미니몬의 능력치/공명도/동행 상태 표시.
// 슬85(기본 UI) / 슬86(텍스트) / 슬87(능력치 등급 outline) / 슬88(부르기·동행중 토글) 구현.
// 슬89~91(첫 포획/재포획 anim flows)는 포획 시스템 의존 → 후속.
public class UI_MinionInfoPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        MinionInfoPopupBlock,
        shineeffectImage1,
        shineeffectImage2,
        MinionInfoPopupFrame,
        EFrame,
        MinionInfoPopupMonsterBox,
        MinionInfoPopupMonsterBoxMask,
        MinimonImage,
        BlockImg,
        AllSStar,
        MinionInfoPopupSkillBox,
        MinionUsingSkillFrame,
        MinionUsingSkillBackground,
        MinionInfoPopupInfoBox,
        MinionInfoPopupNameFrame,
        MinionInfoPopupNameFrameMask,
        MinionInfoPopupNameFrameImage,
        MinionInfoPopupDmgFrame,
        MinionInfoPopupDmgFrameMask,
        DamageRankImage,
        MinionInfoPopupASFrame,
        MinionInfoPopupASFrameMask,
        ASRankImage,
        MinionInfoPopupCritFrame,
        MinionInfoPopupCritFrameMask,
        CritRankImage,
        MinionSummonButtonImage,
        MinionSummonButtonMask,
        Img_Item_03,
        Img_Item_06,
        MinionSummonedButtonImage,
        MinionSummonedButtonMask,
        Img_Item_04,
        Img_Item_01,
        MinionCantSummonButton,
        MinionCantSummonButtonImage,
        MinionCantSummonButtonMask,
        Img_Item_02,
        Img_Item_05,
        Xbutton,
    }

    enum Texts
    {
        MinionInfoPopupTitle,
        BlockImgText,
        MinionNumberText,
        SYNCText,
        SkillNameText,
        MinionNotUseSkillText,
        MinionInfoPopupNameText,
        DamageStringText,
        DamageNumberText,
        ASStringText,
        ASNumberText,
        CritStringText,
        CritNumberText,
        MinionSummonButtonText,
        MinionSummonedButtonText,
        MinionCantSummonButtonText,
        MinionNewText,
        MinionATKNewText,
        MinionASNewText,
        MinionCritNewText,
        NoImprovedText,
        GuranteedText,
        Txt_Xbutton_01,
    }

    enum Buttons
    {
        MinionInfoPopupBackground,
        MinionUsingSkillIcon,
        MinionSummonButton,
        MinionSummonedButton,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        MinionInfoPopup,
        MinionInfoPopupPanel,
        shineeffect,
        ActiveEf,
        Star_GlowDot,
        VFX_Star_GlowDot_02,
        VFX_Star_GlowDot_04,
        SYNC_GlowDot,
        VFX_SYNC_GlowDot_01,
        VFX_SYNC_GlowDot_03,
        MinionUseSkill,
        MinionUsingSkill,
        MinionNotUseSkill,
        MinionInfoPopupDmgUpgradeSFX,
        VFX_MinionInfoPopupDmgUpgrad_05,
        VFX_MinionInfoPopupDmgUpgrad_04,
        VFX_MinionInfoPopupDmgUpgrad_02,
        VFX_MinionInfoPopupDmgUpgrad_01,
        VFX_GlowDot_02_End_01,
        MinionInfoPopupASUpgradeSFX,
        VFX_MinionInfoPopupASUpgrade_04,
        VFX_MinionInfoPopupASUpgrade_03,
        VFX_MinionInfoPopupASUpgrade_01,
        VFX_MinionInfoPopupASUpgrade_02,
        VFX_GlowDot_02_End_03,
        MinionInfoPopupCritUpgradeSFX,
        VFX_MinionInfoPopupCritUpgra_07,
        VFX_MinionInfoPopupCritUpgra_06,
        VFX_MinionInfoPopupCritUpgra_03,
        VFX_MinionInfoPopupCritUpgra_05,
        VFX_GlowDot_02_End_02,
        MinionInfoPopupButtonGroup,
    }

    #endregion

    int _dataId;
    bool _resultAnimPlaying;   // 슬20~22-d/g: 포획 결과 Anim 재생 중 닫기/상호작용 차단

    void Awake() => Init();

    // 슬20~22-d/g: 결과 Anim이 모두 끝난 뒤에만 background/X 클릭으로 닫을 수 있다.
    void OnClickBGGuarded()
    {
        if (_resultAnimPlaying) return;
        OnClickBG();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.MinionInfoPopup).GetComponent<RectTransform>();
        OpenAnimation();

        // MinionInfoPopupBlock: 투명(alpha=0)이지만 RaycastTarget=1로 prefab에 남아있어
        // Background/Xbutton 클릭을 가로채는 문제 → raycast 차단.
        var block = GetImage((int)Images.MinionInfoPopupBlock);
        if (block != null) block.raycastTarget = false;

        GetButton((int)Buttons.MinionInfoPopupBackground).gameObject.BindEvent(OnClickBGGuarded);
        GetImage((int)Images.Xbutton).gameObject.BindEvent(OnClickBGGuarded);
        GetButton((int)Buttons.MinionUsingSkillIcon).gameObject.BindEvent(OnClickSkill);
        GetButton((int)Buttons.MinionSummonButton).gameObject.BindEvent(OnClickSummon);
        GetButton((int)Buttons.MinionSummonedButton).gameObject.BindEvent(OnClickUnsummon);

        return true;
    }

    public void SetInfo(int dataId)
    {
        _dataId = dataId;

        CreatureData data = null;
        Managers.Data.CreatureDic.TryGetValue(dataId, out data);

        // 슬20: NEW 뱃지(이름 옆 + 능력치별) + 성장실패 텍스트는 기본 숨김.
        //  포획 호출(PlayCatchResult) 시에만 NEW/실패문구를 켠다. (미니몬 기록 창엔 미등장)
        foreach (var nt in new[] { Texts.MinionNewText, Texts.MinionATKNewText, Texts.MinionASNewText, Texts.MinionCritNewText, Texts.NoImprovedText })
        {
            var t = GetText((int)nt);
            if (t != null) t.gameObject.SetActive(false);
        }
        // 슬21: 능력치 업그레이드 VFX도 기본 OFF (향상된 스탯에서만 CoImprovedStat이 켠다)
        foreach (var sfx in new[] { GameObjects.MinionInfoPopupDmgUpgradeSFX, GameObjects.MinionInfoPopupASUpgradeSFX, GameObjects.MinionInfoPopupCritUpgradeSFX })
        {
            var go = GetObject((int)sfx);
            if (go != null) go.SetActive(false);
        }

        // 슬86: 타이틀 "나의 미니몬"(한/영) + Outline 784806 (0.5, 0.5)
        var title = GetText((int)Texts.MinionInfoPopupTitle);
        if (title != null)
        {
            title.text = Util.Localize(Define.LocalizeKey.MyMinimonText);
            ApplyFont(title, Define.FontType.UI);
            SetOutline(title, new Color32(0x78, 0x48, 0x06, 255), new Vector2(0.5f, 0.5f));
        }

        // 슬86: "NO." + (id-2000) 3자리
        var numTxt = GetText((int)Texts.MinionNumberText);
        if (numTxt != null)
        {
            numTxt.text = $"NO.{(dataId % 1000):000}";
            ApplyFont(numTxt, Define.FontType.UI);
        }

        // 슬85: 몬스터 IMG. idle Anim은 아래 SetupMinionIdleAnim에서 PetAnimLabel로 구동.
        _monImg = GetImage((int)Images.MinimonImage);
        // 슬16: 몬스터 IMG Idle Anim 반복. prefab baked 컨트롤러가 슬라임이라 모든 미니몬이 슬라임으로 보이던
        //  문제 → MinMonData.PetAnimLabel(잡은 몬스터별 펫 컨트롤러)로 교체. (동행 미니몬과 동일 소스)
        string petAnimLabel = Managers.Data.MinMonDic != null && Managers.Data.MinMonDic.TryGetValue(dataId, out var mmAnim) ? mmAnim.PetAnimLabel : null;
        // 정적 IconLabel 스프라이트는 펫 애니가 없을 때만 폴백으로 사용.
        //  (2001 등은 IconLabel="slime_monster_0"(파란 시트)지만 PetAnimLabel="Pet_Slime(Green)"(초록)으로 구동 →
        //   파란 정적 프레임을 먼저 깔면 애니 복사 전/실패 시 파란색이 노출되던 문제 방지)
        if (_monImg != null && string.IsNullOrEmpty(petAnimLabel) && data != null && !string.IsNullOrEmpty(data.IconLabel))
        {
            Sprite sprite = Managers.Resource.Load<Sprite>(data.IconLabel);
            if (sprite != null) { _monImg.sprite = sprite; _monImg.preserveAspect = true; }
        }
        SetupMinionIdleAnim(petAnimLabel);

        // 슬17: 공명도 = "{LinkText}:{공명도}%" (콜론, 공백 없음). 100% 미만 FF820B / 100% 30FF00.
        //  (100% 시 MAT Text_ShineGreen 스펙 — 머티리얼 Resources 밖이라 색만 적용)
        int sync = Mathf.RoundToInt(Managers.MinMon.GetSyncRate(dataId) * 100f);
        var syncTxt = GetText((int)Texts.SYNCText);
        if (syncTxt != null)
        {
            syncTxt.text = $"{Util.Localize(Define.LocalizeKey.LinkText)}:{sync}%";
            syncTxt.color = sync >= 100 ? Util.HexToColor("30FF00") : Util.HexToColor("FF820B");
            ApplyFont(syncTxt, Define.FontType.UI);
            // 슬17: 공명도 100% Text_ShineGreen 스펙 — Text_ShineGold와 동일하게 폰트 아틀라스 없는 머티리얼이라
            //  적용 시 글리프 깨짐 → 색(30FF00)만 적용. 폰트 프리셋 재제작 시 연결.
        }

        // 능력 목록과 같은 아이콘으로 포획 보상을 식별한다.
        int skillId = Managers.Ability.MinionSkillId(_dataId);
        if (Managers.Data.SkillDic.TryGetValue(skillId, out var skill))
        {
            var icon = GetButton((int)Buttons.MinionUsingSkillIcon)?.image;
            if (icon != null)
            {
                icon.sprite = Managers.Resource.Load<Sprite>(skill.IconLabel);
                icon.preserveAspect = true;
            }
        }

        // 포획 보상과 능력 목록에서 같은 이름을 표시한다. 매핑이 없으면 기존 이름을 사용한다.
        var skillTxt = GetText((int)Texts.SkillNameText);
        if (skillTxt != null)
        {
            // UniqueSkillName 은 로컬라이즈 **키**다 — 그대로 찍으면 키가 노출된다
            //  (503종 전부 "0" 이던 시절엔 실제로 "0" 이 표시됐다).
            string name = skill != null ? skill.Name : data != null ? data.UniqueSkillName : "";
            skillTxt.text = string.IsNullOrEmpty(name) ? "-" : Util.Localize(name);
            ApplyFont(skillTxt, Define.FontType.UI);
        }

        // 보유 여부. 미포획이면 BlockImg("미보유") 오버레이가 몬스터 박스를 덮음.
        bool owned = Managers.MinMon.IsOwned(dataId);
        GetImage((int)Images.BlockImg).gameObject.SetActive(!owned);

        var nameTxt = GetText((int)Texts.MinionInfoPopupNameText);
        if (nameTxt != null && data != null)
        {
            nameTxt.text = Util.Localize(data.Name);
            ApplyFont(nameTxt, Define.FontType.UI);
        }

        // 슬17: 스탯 라벨 — 공격력/공격속도/치명타 (로컬라이즈)
        SetLabel(Texts.DamageStringText, Define.LocalizeKey.AttackPowerText);
        SetLabel(Texts.ASStringText,     Define.LocalizeKey.AttackSpeedText);
        SetLabel(Texts.CritStringText,   Define.LocalizeKey.CriticalText);

        // 슬18: ATK/AS/CRIT 값 + 등급 letter 스프라이트(E/D/C/B/A/SS) + outline 색 + 5등급 골드 Layer2(FrameMask)
        RefreshStat(Texts.DamageNumberText, Images.DamageRankImage, Images.MinionInfoPopupDmgFrameMask,  MMStat.Atk);
        RefreshStat(Texts.ASNumberText,     Images.ASRankImage,     Images.MinionInfoPopupASFrameMask,   MMStat.AtkSpeed);
        RefreshStat(Texts.CritNumberText,   Images.CritRankImage,   Images.MinionInfoPopupCritFrameMask, MMStat.Crit);
        RefreshWeaponIcon();   // 슬16: 공격력에 무기 아이콘(물리=Sword01 / 마법=Staff01)
        RefreshLearnedLabel(); // D5: 고유기술 부분/완벽획득 라벨 (미니몬 기록과 동일 표기)

        // 슬17/86: FullGrowthInText(=GuranteedText) "(완전 성장까지: <color=#FF0000>N</color>회)"
        //  N = 완전 성장(능력치 최대치)까지 남은 포획 횟수 = MaxTameCount - CaughtCount(0 이상).
        //  Color C8C8C8, Outline B48C08 (0.1,0.1). "회" 접미사(KR). (EN 접미사 없음)
        var fullGrowth = GetText((int)Texts.GuranteedText);
        if (fullGrowth != null)
        {
            int maxTame = Managers.Data.MinMonDic != null && Managers.Data.MinMonDic.TryGetValue(dataId, out var mm) ? mm.MaxTameCount : 0;
            int remain = Mathf.Max(0, maxTame - Managers.MinMon.GetCaughtCount(dataId));
            bool kr = Managers.Game == null || Managers.Game.Language == Define.Language.Korean;
            fullGrowth.text = kr ? $"(완전 성장까지: <color=#FF0000>{remain}</color>회)"
                                 : $"(Full growth in: <color=#FF0000>{remain}</color>)";
            fullGrowth.color = Util.HexToColor("C8C8C8");
            SetOutline(fullGrowth, new Color32(0xB4, 0x8C, 0x08, 255), new Vector2(0.1f, 0.1f));
            ApplyFont(fullGrowth, Define.FontType.UI);
        }

        // 슬88: 부르기/동행중 버튼 토글
        RefreshSummonButton();
    }

    // 슬17: 스탯 라벨 등 로컬라이즈 텍스트 세팅 헬퍼
    void SetLabel(Texts id, string locKey)
    {
        var t = GetText((int)id);
        if (t == null) return;
        t.text = Util.Localize(locKey);
        ApplyFont(t, Define.FontType.UI);
    }

    // 슬18: 0~5등급 → Screen FX 32 스프라이트 (E=252,D=253,C=254,B=255,A=256, SS=258 / 257 미사용)
    static readonly int[] RANK_SPRITE_IDX = { 252, 253, 254, 255, 256, 258 };

    // 능력치 값/등급을 MinMonManager(실 데이터)에서 조회. 등급 0~5 = E/D/C/B/A/SS(슬18).
    //  gradeOverride/numberOverride 지정 시(슬21 포획 전 값 연출) 그 값으로 표기.
    void RefreshStat(Texts textId, Images rankImg, Images frameMask, MMStat stat, int gradeOverride = -1, float numberOverride = -1f)
    {
        var tmp = GetText((int)textId);
        if (tmp == null) return;

        float val;
        int g;
        switch (stat)
        {
            case MMStat.Atk:      val = Managers.MinMon.GetAtk(_dataId);      g = Managers.MinMon.GetAtkGrade(_dataId);      break;
            case MMStat.AtkSpeed: val = Managers.MinMon.GetAtkSpeed(_dataId); g = Managers.MinMon.GetAtkSpeedGrade(_dataId); break;
            default:              val = Managers.MinMon.GetCritRate(_dataId); g = Managers.MinMon.GetCritGrade(_dataId);     break;
        }
        if (gradeOverride  >= 0)   g   = gradeOverride;
        if (numberOverride >= 0f)  val = numberOverride;

        tmp.text = Mathf.RoundToInt(val).ToString();
        ApplyFont(tmp, Define.FontType.UI);
        ApplyGradeOutline(tmp, g);

        // 슬18: 5등급(SS) 골드 표기 — PF_Stardust_Gold(TMP SDF 프리셋)로 적용.
        //  (원기획 Text_ShineGold는 SSU 셰이더라 폰트 아틀라스가 없어 글리프가 깨짐 → SDF 골드 프리셋으로 대체.
        //   등급은 하락하지 않으므로 미만 등급 복원은 불필요)
        if (g >= 5)
        {
            var gold = Managers.Resource.Load<Material>("PF_Stardust_Gold");
            if (gold != null) tmp.fontSharedMaterial = gold;
        }

        // 슬18: 등급 letter 스프라이트(E/D/C/B/A/SS)를 RankImage에 표기
        var img = GetImage((int)rankImg);
        if (img != null)
        {
            int idx = RANK_SPRITE_IDX[Mathf.Clamp(g, 0, 5)];
            var sp = Managers.Resource.Load<Sprite>($"Screen FX 32 ({idx})");
            if (sp != null) { img.sprite = sp; img.preserveAspect = true; }
        }

        // 슬18: 5등급(SS)일 때만 골드 Layer2(FrameMask) ON, 0~4등급은 OFF
        var mask = GetImage((int)frameMask);
        if (mask != null) mask.gameObject.SetActive(g >= 5);
    }

    // 슬16: 몬스터 IMG Idle Anim 반복.
    //  MinimonImage의 Animator(Slime(Green) 등)는 클립이 SpriteRenderer.m_Sprite를 구동하므로
    //  같은 GO에 SpriteRenderer를 (렌더 off로) 두어 프레임 소스로 쓰고, 매 프레임 그 sprite를 UI Image에 복사.
    //  (UI 캔버스 마스킹/스케일은 Image 그대로 유지)
    Image _monImg;
    SpriteRenderer _idleSr;
    void SetupMinionIdleAnim(string petAnimLabel = null)
    {
        if (_monImg == null) return;
        var an = _monImg.GetComponent<Animator>();
        if (an == null) { _idleSr = null; return; }

        // 잡은 몬스터별 펫 컨트롤러로 교체 (baked 슬라임 컨트롤러 덮어쓰기 방지). 로드 실패 시 정적 IconLabel 스프라이트 유지.
        if (!string.IsNullOrEmpty(petAnimLabel))
        {
            var ctrl = Managers.Resource.Load<RuntimeAnimatorController>(petAnimLabel);
            if (ctrl != null) an.runtimeAnimatorController = ctrl;
        }
        if (an.runtimeAnimatorController == null) { _idleSr = null; return; }

        an.cullingMode = AnimatorCullingMode.AlwaysAnimate;   // UI(Renderer 없음)에서도 항상 진행
        _idleSr = _monImg.GetComponent<SpriteRenderer>();
        if (_idleSr == null) _idleSr = _monImg.gameObject.AddComponent<SpriteRenderer>();
        _idleSr.enabled = false;   // 월드 렌더 방지 — Animator 프레임 추출 용도
        an.Rebind();
    }

    void Update()
    {
        // idle 프레임(Animator가 SpriteRenderer에 기록)을 UI Image로 복사 → 몬스터 IMG가 idle 반복.
        if (_monImg != null && _idleSr != null && _idleSr.sprite != null)
            _monImg.sprite = _idleSr.sprite;
    }

    // 슬16: 공격력 스탯 박스 상단에 무기 아이콘 — 물리공격력=Sword01 / 마법공격력=Staff01. Outline 000000 x1 y1.
    //  prefab에 전용 노드가 없어 런타임 생성.
    Image _weaponIcon;
    void RefreshWeaponIcon()
    {
        var dmgFrame = GetImage((int)Images.MinionInfoPopupDmgFrame)?.transform;
        if (dmgFrame == null) return;

        bool magic = Managers.Data.CreatureDic.TryGetValue(_dataId, out var cd) && cd.MagicAtk > cd.PhysicalAtk;
        string key = magic ? "Staff01" : "Sword01";
        var sp = Managers.Resource.Load<Sprite>(key);
        if (sp == null) return;

        if (_weaponIcon == null)
        {
            var go = new GameObject("WeaponIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            var rt = (RectTransform)go.transform;
            rt.SetParent(dmgFrame, false);
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);   // 공격력 박스 우측 상단
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-1f, -1f);          // 코너에서 살짝 안쪽
            rt.sizeDelta = new Vector2(12f, 12f);
            _weaponIcon = go.GetComponent<Image>();
            _weaponIcon.preserveAspect = true;
            _weaponIcon.raycastTarget = false;
            var ol = go.GetComponent<Outline>();
            ol.effectColor = Color.black;
            ol.effectDistance = new Vector2(1f, 1f);
        }
        _weaponIcon.sprite = sp;
        _weaponIcon.gameObject.SetActive(true);
    }

    // D5: 고유기술 부분/완벽획득 라벨 — 고유기술명(SkillNameText) 바로 아래 소형 표기.
    //  판정/문구/색은 UI_MonsterDetailPopup.RefreshLearned와 동일:
    //  learned=IsUniqueSkillLearned / perfect=IsAllSS, 부분획득(ParticalGetText)=36FF00 / 완벽획득(CompleteGetText)=FF0000.
    //  prefab에 전용 노드가 없어 런타임 생성 (RefreshWeaponIcon 패턴).
    TMP_Text _learnedLabel;
    void RefreshLearnedLabel()
    {
        bool learned = IsUniqueSkillLearned(_dataId);
        bool perfect = learned && IsAllSS(_dataId);

        if (_learnedLabel == null)
        {
            var skillName = GetText((int)Texts.SkillNameText);
            if (skillName == null) return;
            var go = new GameObject("LearnedLabel", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = (RectTransform)go.transform;
            rt.SetParent(skillName.rectTransform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);   // 기술명 rect 바로 아래 중앙
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -0.5f);
            rt.sizeDelta = new Vector2(40f, 6f);
            _learnedLabel = go.AddComponent<TextMeshProUGUI>();
            _learnedLabel.fontSize = 5f;
            _learnedLabel.alignment = TextAlignmentOptions.Center;
            _learnedLabel.raycastTarget = false;
            _learnedLabel.enableWordWrapping = false;
        }

        bool owned = Managers.MinMon.IsOwned(_dataId);
        _learnedLabel.gameObject.SetActive(learned || owned);
        if (!learned && !owned) return;
        int skillId = Managers.Ability.MinionSkillId(_dataId);
        string key = learned ? (perfect ? Define.LocalizeKey.CompleteGetText : Define.LocalizeKey.ParticalGetText)
            : Managers.Ability.IsCompanionGranted(skillId) ? Define.COMPANION_ABILITY_BORROWED_TEXT
            : Define.COMPANION_ABILITY_AVAILABLE_TEXT;
        _learnedLabel.text = Util.Localize(key);
        _learnedLabel.enableAutoSizing = true;
        _learnedLabel.fontSizeMin = 3f;
        _learnedLabel.fontSizeMax = 5f;
        _learnedLabel.color = perfect ? Util.HexToColor("FF0000") : Util.HexToColor("36FF00");
        ApplyFont(_learnedLabel, Define.FontType.UI);
    }

    // 슬라이드 87: 능력치 등급별 outline 색/거리 (0~5등급)
    static void ApplyGradeOutline(TMP_Text tmp, int grade)
    {
        var outline = tmp.GetComponent<Outline>();
        if (outline == null) return;
        switch (grade)
        {
            case 0: outline.effectColor = new Color32(0xB9, 0x00, 0xFF, 255); outline.effectDistance = new Vector2(0.05f, 0.05f); break;
            case 1: outline.effectColor = new Color32(0x00, 0x00, 0xFF, 255); outline.effectDistance = new Vector2(0.05f, 0.05f); break;
            case 2: outline.effectColor = new Color32(0x3A, 0xE7, 0x95, 255); outline.effectDistance = new Vector2(0.05f, 0.05f); break;
            case 3: outline.effectColor = new Color32(0xFF, 0xC8, 0x24, 255); outline.effectDistance = new Vector2(0.05f, 0.05f); break;
            case 4: outline.effectColor = new Color32(0xFF, 0x32, 0x24, 255); outline.effectDistance = new Vector2(0.05f, 0.05f); break;
            case 5: outline.effectColor = new Color32(0x00, 0x00, 0x00, 255); outline.effectDistance = new Vector2(0.7f, 0.7f); break;
            // mat Text_ShineGold (등급5)는 머티리얼 에셋 미연동 — 후속
        }
    }

    static void SetOutline(TMP_Text tmp, Color color, Vector2 distance)
    {
        var outline = tmp.GetComponent<Outline>();
        if (outline == null) return;
        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    // 슬라이드 88: 부르기 / 동행중 버튼 토글. 동행 상태는 MinMonManager(GameData.CompanionMinMonId) 기준.
    // 미포획이면 부르기 불가(CantSummon 표시).
    void RefreshSummonButton()
    {
        bool owned = Managers.MinMon.IsOwned(_dataId);
        bool isFollowingThis = Managers.Game.CompanionMinMonId == _dataId && _dataId != 0;

        GetButton((int)Buttons.MinionSummonButton).gameObject.SetActive(owned && !isFollowingThis);
        GetButton((int)Buttons.MinionSummonedButton).gameObject.SetActive(owned && isFollowingThis);
        GetImage((int)Images.MinionCantSummonButton).gameObject.SetActive(!owned);

        // 슬19: 이 창의 미니몬이 현재 동행 미니몬이면 [나의 미니몬] 창에 Active Ef 효과
        var activeEf = GetObject((int)GameObjects.ActiveEf);
        if (activeEf != null) activeEf.SetActive(isFollowingThis);

        // 슬17/19: 버튼 텍스트 한/영 로컬라이즈 + 부르기/동행중 Outline F5A200 (alpha 146, x0.1 y-0.1)
        var f5a200 = new Color32(0xF5, 0xA2, 0x00, 146);
        var sumTxt = GetText((int)Texts.MinionSummonButtonText);
        if (sumTxt != null) { sumTxt.text = Util.Localize(Define.LocalizeKey.SummonText); ApplyFont(sumTxt, Define.FontType.UI); SetOutline(sumTxt, f5a200, new Vector2(0.1f, -0.1f)); }
        var sumdTxt = GetText((int)Texts.MinionSummonedButtonText);
        if (sumdTxt != null) { sumdTxt.text = Util.Localize(Define.LocalizeKey.FollowingText); ApplyFont(sumdTxt, Define.FontType.UI); SetOutline(sumdTxt, f5a200, new Vector2(0.1f, -0.1f)); }
        var cantTxt = GetText((int)Texts.MinionCantSummonButtonText);
        if (cantTxt != null) { cantTxt.text = Util.Localize(Define.LocalizeKey.RequireMinimonText); ApplyFont(cantTxt, Define.FontType.UI); }
    }

    void OnClickSummon()
    {
        if (_resultAnimPlaying) return;
        if (!Managers.MinMon.IsOwned(_dataId)) return;
        // 슬19: 다른 미니몬과 동행 중이면 자동 해제 후 새로 지정 (MinMonManager.SetCompanion이 처리)
        Managers.MinMon.SetCompanion(_dataId);
        Debug.Log($"[MyMinimon] {_dataId} 동행 시작 (이전 동행 자동 해제)");
        RefreshSummonButton();
        RefreshLearnedLabel();

        // 슬19: [나의 미니몬] 창에 Flash_USETHIS 효과 + [미니몬 기록]/[미니몬 도감] UI 동시 갱신
        SpawnFlash();
        RefreshOtherCompanionUI();
    }

    void OnClickUnsummon()
    {
        if (_resultAnimPlaying) return;
        Managers.MinMon.RemoveCompanion();
        Debug.Log($"[MyMinimon] {_dataId} 동행 해제");
        RefreshSummonButton();
        RefreshLearnedLabel();
        RefreshOtherCompanionUI();
    }

    // 슬19: [나의 미니몬] 창에 Flash_USETHIS.prefab 잠깐 표시 — MinionInfoPopupFrame 영역에만.
    //  Flash는 Animator가 Image를 스케일업해 "뱅"을 만들어 프레임 밖으로 퍼지므로,
    //  프레임을 꽉 채운 RectMask2D 컨테이너 안에 넣어 프레임 영역으로 클리핑한다.
    void SpawnFlash()
    {
        if (Managers.Resource.Load<GameObject>("Flash_USETHIS") == null) return;
        var frame = GetImage((int)Images.MinionInfoPopupFrame)?.rectTransform;
        if (frame == null) return;

        // 프레임을 채우는 마스크 컨테이너 (Flash만 클리핑, 프레임 본문엔 영향 없음)
        var maskGo = new GameObject("FlashMask", typeof(RectTransform), typeof(UnityEngine.UI.RectMask2D));
        var maskRT = (RectTransform)maskGo.transform;
        maskRT.SetParent(frame, false);
        maskRT.anchorMin = Vector2.zero; maskRT.anchorMax = Vector2.one;
        maskRT.offsetMin = Vector2.zero; maskRT.offsetMax = Vector2.zero;
        maskRT.SetAsLastSibling();   // 프레임 본문 위에 표시

        var fx = Managers.Resource.Instantiate("Flash_USETHIS", maskRT);
        if (fx == null) { Destroy(maskGo); return; }
        fx.transform.localPosition = Vector3.zero;
        fx.transform.localScale = Vector3.one;

        DOVirtual.DelayedCall(1f, () => { if (maskGo != null) Destroy(maskGo); }, true).SetTarget(this).SetLink(gameObject);
    }

    // 슬19: 부르기/동행 변경 시 열려있는 [미니몬 기록]/[미니몬 도감]의 동행 UI도 함께 갱신
    static void RefreshOtherCompanionUI()
    {
        var detail = Object.FindObjectOfType<UI_MonsterDetailPopup>();
        if (detail != null) detail.RefreshCompanionState();
        var bestiary = Object.FindObjectOfType<UI_BestiaryPopup>();
        if (bestiary != null) bestiary.RefreshCompanionVisuals();
    }

    // 슬20~22: 포획 직후 호출되는 결과 연출.
    //  first    → 첫 포획: 이름 옆 NEW 표시
    //  improved → 재포획 능력치 향상: 공격력→공속→치명타 순차 Scale Anim (슬21)
    //  그 외     → 향상 없음: NoImprovedText 표기 (슬22)
    //  공통(포획 호출): GlowRays(창 뒤) + 공명도 lerp. (미니몬 기록 통해 연 창엔 호출 안 됨)
    public void PlayCatchResult(bool first, bool improved)
    {
        _resultAnimPlaying = true;   // 슬20~22-d/g: 결과 Anim 끝날 때까지 닫기 차단

        // 결과 연출 중에는 버튼을 숨기고, 연출이 끝나면 곧바로 동행할 수 있게 한다.
        var btnGroup = GetObject((int)GameObjects.MinionInfoPopupButtonGroup);
        if (btnGroup != null) btnGroup.SetActive(false);

        var newTxt = GetText((int)Texts.MinionNewText);
        if (newTxt != null) newTxt.gameObject.SetActive(first);

        // 슬21-f/22-c: FullGrowthInText는 포획 연출에서 FadeIn으로 등장 → 먼저 alpha 0으로 숨김.
        //  (SetInfo가 텍스트/색은 이미 세팅. 미니몬 기록 창에선 SetInfo의 풀 alpha 그대로 유지)
        var fg = GetText((int)Texts.GuranteedText);
        if (fg != null) { var c = fg.color; c.a = 0f; fg.color = c; }

        SpawnGlowRays();                       // 슬20: 창 뒤 GlowRays (포획 시 항상)
        StartCoroutine(CoSyncLerp());          // 슬20: 공명도 오른 만큼 lerp

        if (!first && improved) StartCoroutine(CoGrowthSequence());
        else if (!first) StartCoroutine(CoGrowthFail());
        else StartCoroutine(CoFadeInFullGrowth(1.5f));   // 첫 포획: 공명도 lerp(1s) 후 FadeIn
    }

    // 슬21-f/22-c: FullGrowthInText를 delay 뒤 1.5초에 걸쳐 FadeIn. (포획 연출의 마지막 Anim)
    //  FadeIn 완료 시점에 _resultAnimPlaying 해제 → 이후 background 클릭으로 닫기 허용(슬20~22-d/g).
    IEnumerator CoFadeInFullGrowth(float delay)
    {
        var fg = GetText((int)Texts.GuranteedText);
        if (fg != null)
        {
            var c = fg.color; c.a = 0f; fg.color = c;
            yield return new WaitForSecondsRealtime(delay);
            fg.DOFade(1f, 1.5f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(1.5f);
        }
        _resultAnimPlaying = false;
        // 포획의 보상을 바로 써볼 수 있도록 결과 연출이 끝나면 기존 동행 버튼을 연다.
        var btnGroup = GetObject((int)GameObjects.MinionInfoPopupButtonGroup);
        if (btnGroup != null) btnGroup.SetActive(true);
        RefreshSummonButton();
    }

    // 슬20: [나의 미니몬] 창 뒤에 GlowRays 표시 (포획으로 호출됐을 때만). 느린 회전 + 페이드 인.
    void SpawnGlowRays()
    {
        var panel = GetObject((int)GameObjects.MinionInfoPopupPanel)?.transform;
        var sp = Managers.Resource.Load<Sprite>("GlowRays");
        if (panel == null || sp == null) return;

        var go = new GameObject("GlowRays", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(panel, false);
        rt.SetAsFirstSibling();                // 프레임 뒤로
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(360f, 360f);
        var img = go.GetComponent<Image>();
        img.sprite = sp; img.raycastTarget = false; img.preserveAspect = true;
        var c = img.color; c.a = 0f; img.color = c;
        img.DOFade(0.9f, 0.5f).SetUpdate(true);
        rt.DORotate(new Vector3(0f, 0f, 360f), 12f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1).SetUpdate(true);
    }

    // 슬20: 공명도 수치를 (포획 전→후)만큼 lerp. 0~99% 주황, 100% 달성 시 주황→초록 서서히.
    IEnumerator CoSyncLerp()
    {
        var syncTxt = GetText((int)Texts.SYNCText);
        if (syncTxt == null) yield break;

        int newPct  = Mathf.RoundToInt(Managers.MinMon.GetSyncRate(_dataId) * 100f);
        int prevPct = Mathf.Max(0, newPct - Mathf.RoundToInt(MinMonManager.SYNC_PER_CATCH * 100f));
        string link = Util.Localize(Define.LocalizeKey.LinkText);
        Color orange = Util.HexToColor("FF820B"), green = Util.HexToColor("30FF00");

        syncTxt.color = orange;
        float e = 0f, dur = 1f;
        while (e < dur)
        {
            e += Time.unscaledDeltaTime;
            int shown = Mathf.RoundToInt(Mathf.Lerp(prevPct, newPct, e / dur));
            syncTxt.text = $"{link}:{shown}%";
            yield return null;
        }
        syncTxt.text = $"{link}:{newPct}%";
        // 100% 달성 시 주황→초록 서서히
        if (newPct >= 100) syncTxt.DOColor(green, 0.5f).SetUpdate(true);
    }

    // 슬22: 재포획 성장 실패 — 호출 1.5초 뒤 MinimonGrowthFailText를 1.5초에 걸쳐 FadeIn.
    //  실패 텍스트 FadeIn 시작 0.5초 후 FullGrowthInText도 FadeIn (슬22-c).
    IEnumerator CoGrowthFail()
    {
        var t = GetText((int)Texts.NoImprovedText);
        if (t == null) yield break;
        t.text = Util.Localize(Define.LocalizeKey.MinimonGrowthFailText);
        ApplyFont(t, Define.FontType.UI);
        t.gameObject.SetActive(true);
        var c = t.color; c.a = 0f; t.color = c;            // alpha 0에서 시작
        yield return new WaitForSecondsRealtime(1.5f);      // 호출 1.5초 뒤
        t.DOFade(1f, 1.5f).SetUpdate(true);                 // 1.5초에 걸쳐 FadeIn
        StartCoroutine(CoFadeInFullGrowth(0.5f));           // 0.5초 후 FullGrowthInText FadeIn
    }

    // 슬21: 1초 뒤부터 공격력→공속→치명타 순서로, 실제로 오른 능력치만 Anim 재생(슬21-c,e).
    //  모든 Anim 재생 후 0.5초 뒤 FullGrowthInText FadeIn (슬21-f).
    IEnumerator CoGrowthSequence()
    {
        // 슬21-d: Anim 전엔 "포획 전 값(이전 등급/이전 공명도 기준 숫자)"을 보여준다.
        float newSync  = Managers.MinMon.GetSyncRate(_dataId);
        float prevSync = Mathf.Max(0f, newSync - MinMonManager.SYNC_PER_CATCH);
        float ratio = newSync > 0f ? (1f + prevSync) / (1f + newSync) : 1f;   // 새 숫자 → 이전 숫자 환산
        var mm = Managers.MinMon;
        if (mm.LastAtkImproved)  RefreshStat(Texts.DamageNumberText, Images.DamageRankImage, Images.MinionInfoPopupDmgFrameMask,  MMStat.Atk,      mm.LastAtkGradePrev,  Managers.MinMon.GetAtk(_dataId)      * ratio);
        if (mm.LastASImproved)   RefreshStat(Texts.ASNumberText,     Images.ASRankImage,     Images.MinionInfoPopupASFrameMask,   MMStat.AtkSpeed, mm.LastASGradePrev,   Managers.MinMon.GetAtkSpeed(_dataId) * ratio);
        if (mm.LastCritImproved) RefreshStat(Texts.CritNumberText,   Images.CritRankImage,   Images.MinionInfoPopupCritFrameMask, MMStat.Crit,     mm.LastCritGradePrev, Managers.MinMon.GetCritRate(_dataId) * ratio);

        yield return new WaitForSecondsRealtime(1f);   // 슬21-c: 1초 뒤부터
        if (mm.LastAtkImproved)  yield return CoImprovedStat(MMStat.Atk);
        if (mm.LastASImproved)   yield return CoImprovedStat(MMStat.AtkSpeed);
        if (mm.LastCritImproved) yield return CoImprovedStat(MMStat.Crit);
        yield return CoFadeInFullGrowth(0.5f);
    }

    // 슬21-b,d: 향상된 한 능력치의 연출 — 프레임 Flip(Rotate y180 ×4 Incremental) + Punch,
    //  플립 중간에 숫자/등급 ICON을 새 값으로 갱신, 업그레이드 VFX 표시, NEW 뱃지 Scale(FROM 3, OutBack).
    IEnumerator CoImprovedStat(MMStat stat)
    {
        Texts numTxt, newTxt; Images rankImg, frameImg, frameMask; GameObjects sfx;
        switch (stat)
        {
            case MMStat.Atk:
                numTxt = Texts.DamageNumberText; rankImg = Images.DamageRankImage; frameImg = Images.MinionInfoPopupDmgFrame; frameMask = Images.MinionInfoPopupDmgFrameMask; newTxt = Texts.MinionATKNewText; sfx = GameObjects.MinionInfoPopupDmgUpgradeSFX; break;
            case MMStat.AtkSpeed:
                numTxt = Texts.ASNumberText; rankImg = Images.ASRankImage; frameImg = Images.MinionInfoPopupASFrame; frameMask = Images.MinionInfoPopupASFrameMask; newTxt = Texts.MinionASNewText; sfx = GameObjects.MinionInfoPopupASUpgradeSFX; break;
            default:
                numTxt = Texts.CritNumberText; rankImg = Images.CritRankImage; frameImg = Images.MinionInfoPopupCritFrame; frameMask = Images.MinionInfoPopupCritFrameMask; newTxt = Texts.MinionCritNewText; sfx = GameObjects.MinionInfoPopupCritUpgradeSFX; break;
        }

        var frame = GetImage((int)frameImg)?.rectTransform;
        // 업그레이드 VFX ON (활성 시 재생)
        var sfxGo = GetObject((int)sfx);
        if (sfxGo != null) sfxGo.SetActive(true);

        // 프레임 Flip(Rotate y180, 0.2s ×4 Incremental = 총 720°) + 0.9s 뒤 Punch (vibrato 8)
        if (frame != null)
        {
            frame.DORotate(new Vector3(0f, 180f, 0f), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(4, LoopType.Incremental).SetUpdate(true);
            frame.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.4f, 8).SetDelay(0.9f).SetUpdate(true);
        }

        // 플립 중간(앞면이 안 보이는 시점)에 숫자/등급 ICON을 새 값으로 교체
        yield return new WaitForSecondsRealtime(0.4f);
        RefreshStat(numTxt, rankImg, frameMask, stat);

        // NEW 뱃지: Scale FROM 3 → 1 (OutBack 0.3s) 후 원래 Move Anim 재생
        var nt = GetText((int)newTxt);
        if (nt != null)
        {
            nt.gameObject.SetActive(true);
            nt.rectTransform.localScale = Vector3.one * 3f;
            nt.rectTransform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            var moveAnim = nt.GetComponent<DG.Tweening.DOTweenAnimation>();
            if (moveAnim != null) StartCoroutine(CoDelayed(0.3f, () => moveAnim.DORestart()));
        }

        yield return new WaitForSecondsRealtime(0.9f);   // 프레임 Punch(0.9 delay)까지 마무리
    }

    IEnumerator CoDelayed(float delay, System.Action act)
    {
        yield return new WaitForSecondsRealtime(delay);
        act?.Invoke();
    }

    void OnClickSkill()
    {
        // 슬86: 기술 아이콘 클릭 시 (기술 상세). 슬76~79 게이팅과 별개로 나의 미니몬에선 항상 열림.
        Debug.Log($"[MyMinimon] 고유기술 상세 호출 dataId={_dataId}");
        // 몬스터 dataId → 미니몬 고유기술 능력 id (2001 → 6001)
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(Managers.Ability.MinionSkillId(_dataId));
    }
}
