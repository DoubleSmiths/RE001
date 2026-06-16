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

    void Awake() => Init();

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

        GetButton((int)Buttons.MinionInfoPopupBackground).gameObject.BindEvent(OnClickBG);
        GetImage((int)Images.Xbutton).gameObject.BindEvent(OnClickBG);
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

        // 슬85: 몬스터 IMG (IconLabel) — 정적 폴백. idle Anim은 아래 SetupMinionIdleAnim에서 구동.
        _monImg = GetImage((int)Images.MinimonImage);
        if (_monImg != null && data != null && !string.IsNullOrEmpty(data.IconLabel))
        {
            Sprite sprite = Managers.Resource.Load<Sprite>(data.IconLabel);
            if (sprite != null) { _monImg.sprite = sprite; _monImg.preserveAspect = true; }
        }
        SetupMinionIdleAnim();   // 슬16: 몬스터 IMG Idle Anim 반복

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

        // 슬86: MinimonAbilityText (=SkillNameText) — UniqueSkillName 우선, 없으면 "-"
        var skillTxt = GetText((int)Texts.SkillNameText);
        if (skillTxt != null)
        {
            string name = data != null ? data.UniqueSkillName : "";
            skillTxt.text = string.IsNullOrEmpty(name) ? "-" : name;
            ApplyFont(skillTxt, Define.FontType.UI);
        }

        // 보유 여부. 미포획이면 BlockImg("미보유") 오버레이가 몬스터 박스를 덮음.
        bool owned = Managers.MinMon.IsOwned(dataId);
        GetImage((int)Images.BlockImg).gameObject.SetActive(!owned);

        var nameTxt = GetText((int)Texts.MinionInfoPopupNameText);
        if (nameTxt != null && data != null)
        {
            nameTxt.text = data.Name;
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
    void RefreshStat(Texts textId, Images rankImg, Images frameMask, MMStat stat)
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

        tmp.text = Mathf.RoundToInt(val).ToString();
        ApplyFont(tmp, Define.FontType.UI);
        ApplyGradeOutline(tmp, g);

        // 슬18: 5등급(SS)은 Text_ShineGold 머티리얼 스펙이나, 현재 Text_ShineGold.mat에 폰트 아틀라스가
        //  없어(=PF_Stardust SDF의 TMP 머티리얼 프리셋이 아님) fontSharedMaterial로 적용 시 글리프가 깨짐.
        //  → 머티리얼 미적용(등급5 outline 000000 0.7,0.7만 적용). 폰트 프리셋으로 재제작 시 연결 가능.

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
    void SetupMinionIdleAnim()
    {
        if (_monImg == null) return;
        var an = _monImg.GetComponent<Animator>();
        if (an == null || an.runtimeAnimatorController == null) { _idleSr = null; return; }
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
        if (!Managers.MinMon.IsOwned(_dataId)) return;
        // 슬19: 다른 미니몬과 동행 중이면 자동 해제 후 새로 지정 (MinMonManager.SetCompanion이 처리)
        Managers.MinMon.SetCompanion(_dataId);
        Debug.Log($"[MyMinimon] {_dataId} 동행 시작 (이전 동행 자동 해제)");
        RefreshSummonButton();

        // 슬19: [나의 미니몬] 창에 Flash_USETHIS 효과 + [미니몬 기록]/[미니몬 도감] UI 동시 갱신
        SpawnFlash();
        RefreshOtherCompanionUI();
    }

    void OnClickUnsummon()
    {
        Managers.MinMon.RemoveCompanion();
        Debug.Log($"[MyMinimon] {_dataId} 동행 해제");
        RefreshSummonButton();
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

        DOVirtual.DelayedCall(1f, () => { if (maskGo != null) Destroy(maskGo); }, true);
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

    // 슬21-f/22-c: FullGrowthInText를 delay 뒤 1.5초에 걸쳐 FadeIn.
    IEnumerator CoFadeInFullGrowth(float delay)
    {
        var fg = GetText((int)Texts.GuranteedText);
        if (fg == null) yield break;
        var c = fg.color; c.a = 0f; fg.color = c;
        yield return new WaitForSecondsRealtime(delay);
        fg.DOFade(1f, 1.5f).SetUpdate(true);
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

    // 슬21: 1초 뒤부터 공격력→공속→치명타 순서로 숫자+등급 ICON Scale 펀치.
    //  모든 Anim 재생 후 0.5초 뒤 FullGrowthInText FadeIn (슬21-f).
    IEnumerator CoGrowthSequence()
    {
        yield return new WaitForSecondsRealtime(1f);
        yield return CoPunchStat(Texts.DamageNumberText, Images.DamageRankImage);
        yield return CoPunchStat(Texts.ASNumberText,     Images.ASRankImage);
        yield return CoPunchStat(Texts.CritNumberText,   Images.CritRankImage);
        yield return CoFadeInFullGrowth(0.5f);
    }

    IEnumerator CoPunchStat(Texts numTxt, Images rankImg)
    {
        var t = GetText((int)numTxt);
        var img = GetImage((int)rankImg);
        if (t != null) t.rectTransform.DOPunchScale(new Vector3(0.4f, 0.4f, 0f), 0.4f, 8).SetUpdate(true);
        if (img != null) img.rectTransform.DOPunchScale(new Vector3(0.4f, 0.4f, 0f), 0.4f, 8).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.45f);
    }

    void OnClickSkill()
    {
        // 슬86: 기술 아이콘 클릭 시 (기술 상세). 슬76~79 게이팅과 별개로 나의 미니몬에선 항상 열림.
        Debug.Log($"[MyMinimon] 고유기술 상세 호출 dataId={_dataId}");
        // 몬스터 dataId → 미니몬 고유기술 능력 id (2001 → 6001)
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(Managers.Ability.MinionSkillId(_dataId));
    }
}
