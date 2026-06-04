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

        // 슬86: 타이틀 Outline 784806 (0.5, 0.5)
        var title = GetText((int)Texts.MinionInfoPopupTitle);
        if (title != null)
        {
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

        // 슬85: 몬스터 IMG (IconLabel). idle Anim 반복은 Animator 연동 후속.
        var monImg = GetImage((int)Images.MinimonImage);
        if (monImg != null && data != null && !string.IsNullOrEmpty(data.IconLabel))
        {
            Sprite sprite = Managers.Resource.Load<Sprite>(data.IconLabel);
            if (sprite != null) { monImg.sprite = sprite; monImg.preserveAspect = true; }
        }

        // 슬17: 공명도 = "{LinkText}:{공명도}%" (콜론, 공백 없음). 100% 미만 FF820B / 100% 30FF00.
        //  (100% 시 MAT Text_ShineGreen 스펙 — 머티리얼 Resources 밖이라 색만 적용)
        int sync = Mathf.RoundToInt(Managers.MinMon.GetSyncRate(dataId) * 100f);
        var syncTxt = GetText((int)Texts.SYNCText);
        if (syncTxt != null)
        {
            syncTxt.text = $"{Util.Localize(Define.LocalizeKey.LinkText)}:{sync}%";
            syncTxt.color = sync >= 100 ? Util.HexToColor("30FF00") : Util.HexToColor("FF820B");
            ApplyFont(syncTxt, Define.FontType.UI);
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

        // 슬86/87: ATK/AS/CRIT 값 + 등급 outline 색
        RefreshStat(Texts.DamageNumberText, MMStat.Atk);
        RefreshStat(Texts.ASNumberText,    MMStat.AtkSpeed);
        RefreshStat(Texts.CritNumberText,  MMStat.Crit);

        // 슬86: FullGrowthInText "(완전 성장까지: <color=#FF0000>10</color>회)"
        //  — 현재 enum에 FullGrowthInText 노드 부재. prefab에 신설 후 enum 보강 필요.
        //  TODO: 잔여 횟수 = MaxKillCount - current. 포획 시스템 도입 전 임시 10 고정.

        // 슬88: 부르기/동행중 버튼 토글
        RefreshSummonButton();
    }

    // 능력치 값/등급을 MinMonManager(실 데이터)에서 조회. 등급 0~5 = E~S(슬18/20).
    void RefreshStat(Texts textId, MMStat stat)
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
    }

    void OnClickSummon()
    {
        if (!Managers.MinMon.IsOwned(_dataId)) return;
        // 슬88/19: 다른 미니몬과 동행 중이면 자동 해제 후 새로 지정 (MinMonManager.SetCompanion이 처리)
        Managers.MinMon.SetCompanion(_dataId);
        Debug.Log($"[MyMinimon] {_dataId} 동행 시작 (이전 동행 자동 해제)");
        RefreshSummonButton();
        // TODO 슬88: Flash_USETHIS 효과 + Active Ef, [미니몬 기록]/[미니몬 도감] UI 갱신
    }

    void OnClickUnsummon()
    {
        Managers.MinMon.RemoveCompanion();
        Debug.Log($"[MyMinimon] {_dataId} 동행 해제");
        RefreshSummonButton();
    }

    // 슬20~22: 포획 직후 호출되는 결과 연출.
    //  first    → 첫 포획: 이름 옆 NEW 표시 (GlowRays는 prefab 부재로 미적용)
    //  improved → 재포획 능력치 향상: 공격력→공속→치명타 순차 Scale Anim (슬21)
    //  그 외     → 향상 없음: NoImprovedText 표기 (슬22)
    public void PlayCatchResult(bool first, bool improved)
    {
        var newTxt = GetText((int)Texts.MinionNewText);
        if (newTxt != null) newTxt.gameObject.SetActive(first);

        if (!first && improved) StartCoroutine(CoGrowthSequence());
        else if (!first)
        {
            var noImp = GetText((int)Texts.NoImprovedText);
            if (noImp != null) { noImp.gameObject.SetActive(true); ApplyFont(noImp, Define.FontType.UI); }
        }
    }

    // 슬21: 1초 뒤부터 공격력→공속→치명타 순서로 숫자+등급 ICON Scale 펀치
    IEnumerator CoGrowthSequence()
    {
        yield return new WaitForSecondsRealtime(1f);
        yield return CoPunchStat(Texts.DamageNumberText, Images.DamageRankImage);
        yield return CoPunchStat(Texts.ASNumberText,     Images.ASRankImage);
        yield return CoPunchStat(Texts.CritNumberText,   Images.CritRankImage);
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
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(_dataId);
    }
}
