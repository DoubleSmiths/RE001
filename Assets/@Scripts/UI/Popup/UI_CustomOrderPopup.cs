using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using Sequence = DG.Tweening.Sequence;

public class UI_CustomOrderPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        Img_CustomOrderPopupFrame_01,
        Alch_Img_mask,
        Alch_Img_mask_1,
        Img_Alch_Img_mask_01,
        GlowMask_2,
        Img_GlowMask_2_02,
        CustomOrderHPPotionImg,
        Mask_CustomOrderSPDPotionFram_01,
        Img_GlowMask_3_03,
        CustomOrderSPDPotionImg,
        lockbg_1,
        lockimg_1,
        Mask_CustomOrderLuckPotionFra_02,
        Img_GlowMask_3_04,
        CustomOrderLuckPotionImg,
        lockbg,
        lockimg,
        Img_CustomOrder_AutoPotionCo_02,
        Background,
        lockbg_AutoPotion,
        lockimg_AutoPotion,
        MasterIcon,
        Img_MasterIcon_02,
        LvlIcon,
        Img_LvlIcon_01,
        Img_CustomOrderPotionBtn_01,
        GameObject_2,
        Xbutton,
    }

    enum Texts
    {
        CustomOrderPopupTitle,
        engtext,
        kortext,
        kortext_1,
        CustomOrderSPDPotionNewTxt,
        AutoPotionNewTxt,
        Txt_MasterIcon_02,
        Txt_LvlIcon_01,
        Txt_CustomOrderPotionBtn_02,
        CustomOrderPotionNewTxt,
        Txt_CustomOrderSkillBtn_02,
        CustomOrderSkillNewTxt,
        Txt_Xbutton_01,
    }

    enum Buttons
    {
        CustomOrderPopupBackground,
        CustomOrderHPPotionFrame,
        CustomOrderSPDPotionFrame,
        CustomOrderLuckPotionFrame,
        WIcon,
        CustomOrderPotionBtn,
        CustomOrderSkillBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        CustomOrderPopup,
        CustomOrderPopupFrame,
        TalkText,
        Btm_PotionBtnGroup,
        CustomOrderHPPotionIcon,
        CustomOrderSPDPotionIcon,
        CustomOrderLuckPotionIcon,
        Btm_SkillBtnGroup,
        CustomOrder_AutoPotionCooltimeSkill,
    }

    #endregion


    DialogueAnimator _typing;
    TMP_Text _talkText;
    TMP_Text _talkEngText;

    // prefab asset 기준 고정 원위치 (런타임 Init 시점이 불안정해서 하드코딩)
    static readonly Vector2 FRAME_ORIGIN = new Vector2(0f, 0f);
    static readonly Vector2 ALCH_ORIGIN = new Vector2(70f, 1f);

    const float FRAME_FROM_Y = -120f;
    const float ALCH_SLIDE_OFFSET_X = -150f;

    // slide 35 확장
    static readonly Color FRAME_SELECTED_COLOR = new Color32(0xF5, 0xDD, 0x6C, 0xFF);
    static readonly Color FRAME_DEFAULT_COLOR = Color.white;
    const float BTN_Y_ORIGIN = 17f;
    const float BTN_Y_EXPANDED = -40f;
    static readonly Color LOCK_BG_COLOR   = new Color(0f, 0f, 0f, 180f / 255f);
    static readonly Color LOCK_ICON_COLOR = new Color(1f, 1f, 1f,  80f / 255f);

    int _currentGroup = -1; // 0 = Potion, 1 = Skill, -1 = none

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _typing = gameObject.GetOrAddComponent<DialogueAnimator>();

        // TalkText는 kortext/engtext 하위 구조. 한글/영어 둘 중 한쪽만 표시해야 하는데,
        // 현재 prefab은 둘 다 activeSelf=true에 텍스트가 들어있어 이중으로 보임.
        // Init에서 engtext를 비활성화하고, Korean 모드에선 kortext만 사용.
        var talkTextGO = GetObject((int)GameObjects.TalkText);
        var engTf = talkTextGO.transform.Find("engtext");
        if (engTf != null)
        {
            _talkEngText = engTf.GetComponent<TMP_Text>();
            engTf.gameObject.SetActive(false);
        }
        var korTf = talkTextGO.transform.Find("kortext");
        if (korTf != null)
        {
            _talkText = korTf.GetComponent<TMP_Text>();
            // kortext_1(중복 자식)도 존재하면 숨김 — parent kortext가 실제 표시 담당
            var korDup = korTf.Find("kortext_1");
            if (korDup != null) korDup.gameObject.SetActive(false);

            // [DOTweenAnimation 규약 예외 — 애니 교체 아님] kortext의 ContentSizeFitter + DOTweenAnimation이
            //  위치/크기를 조작해 텍스트가 밀린다. 이 DOTweenAnimation은 '쓰고 싶은 연출'이 아니라 레이아웃을
            //  망치는 잔존 컴포넌트라 제거(코드로 정적 배치). 대체 애니를 코드로 만드는 케이스가 아님.
            //  DestroyImmediate로 즉시 제거해야 이후 set이 덮어쓰여지지 않음.
            var csf = korTf.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (csf != null) DestroyImmediate(csf);
            var dta = korTf.GetComponent<DG.Tweening.DOTweenAnimation>();
            if (dta != null) DestroyImmediate(dta);
        }

        GetButton((int)Buttons.CustomOrderPopupBackground).gameObject.BindEvent(OnClickClose);
        GetImage((int)Images.Xbutton).gameObject.BindEvent(OnClickClose);
        GetButton((int)Buttons.CustomOrderPotionBtn).gameObject.BindEvent(OnClickPotion);
        GetButton((int)Buttons.CustomOrderSkillBtn).gameObject.BindEvent(OnClickSkill);

        return true;
    }

    public void SetInfo()
    {
        // 슬라이드 35 확장 그룹은 기본 상태에서 숨김 (물약/기술 버튼 클릭 시 슬라이드 35에서 표시)
        GetObject((int)GameObjects.Btm_PotionBtnGroup).SetActive(false);
        GetObject((int)GameObjects.Btm_SkillBtnGroup).SetActive(false);

        // 버튼 / 프레임 색 / 잠금 상태 초기화
        ResetBottomButtons();
        InitLockStates();
        RefreshPotionIcons();
        _currentGroup = -1;

        var frameRT = GetObject((int)GameObjects.CustomOrderPopupFrame).GetComponent<RectTransform>();
        var alchRT = GetImage((int)Images.Alch_Img_mask_1).rectTransform;

        // 기존 트윈 정리
        frameRT.DOKill();
        alchRT.DOKill();

        // 팝업 진입: From y = -120, Duration 0.5, OutBack
        frameRT.anchoredPosition = FRAME_ORIGIN + new Vector2(0f, FRAME_FROM_Y);
        frameRT.DOAnchorPos(FRAME_ORIGIN, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        // 캐릭터 슬라이드 인: mask 안쪽으로 왼쪽에서 오른쪽으로. OutBack 0.4s
        // Alch_Img_mask_1이 mask의 자식이므로 mask 바깥으로 나간 부분은 자동 클리핑됨.
        alchRT.anchoredPosition = ALCH_ORIGIN + new Vector2(ALCH_SLIDE_OFFSET_X, 0f);
        alchRT.DOAnchorPos(ALCH_ORIGIN, 0.4f).SetDelay(0.2f).SetEase(Ease.OutBack).SetUpdate(true);

        // TalkText 타이핑: 캐릭터가 거의 자리 잡은 뒤 시작
        if (_talkText != null)
        {
            _talkText.text = "";
            StartCoroutine(CoTalkTextPos());
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (_talkText == null) return;
                _typing.PlayTyping(_talkText, "\"무엇을 만들어드릴까요?\"", DialogueEmotion.Normal);
            }).SetTarget(this).SetLink(gameObject).SetUpdate(true);
        }

        // BlackBG fade (slide 8: Duration 0.3, OutQuad, target alpha 220/255)
        var bg = GetButton((int)Buttons.CustomOrderPopupBackground).GetComponent<Image>();
        if (bg != null)
        {
            bg.DOKill();
            Color c = bg.color; c.a = 0f; bg.color = c;
            bg.DOFade(220f / 255f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    System.Collections.IEnumerator CoTalkTextPos()
    {
        if (_talkText == null) yield break;
        var rt = _talkText.rectTransform;
        // prefab 잔여 오토 레이아웃(DOTween/TMP)이 pos/size를 덮어쓰므로 매 프레임 강제 설정.
        Vector2 targetPos = new Vector2(22f, -75f);
        Vector2 targetSize = new Vector2(65f, 40f);
        while (this != null && _talkText != null)
        {
            if (rt.anchoredPosition != targetPos) rt.anchoredPosition = targetPos;
            if (rt.sizeDelta != targetSize) rt.sizeDelta = targetSize;
            yield return null;
        }
    }

    void OnClickClose()
    {
        var frameRT = GetObject((int)GameObjects.CustomOrderPopupFrame).GetComponent<RectTransform>();
        var alchRT = GetImage((int)Images.Alch_Img_mask_1).rectTransform;

        frameRT.DOKill();
        alchRT.DOKill();

        // Rewind: 진입과 반대로 y -120으로 내려가며 사라짐
        Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject).SetUpdate(true);
        seq.Append(frameRT.DOAnchorPos(FRAME_ORIGIN + new Vector2(0f, FRAME_FROM_Y), 0.3f).SetEase(Ease.InBack));
        seq.Join(alchRT.DOAnchorPos(ALCH_ORIGIN + new Vector2(ALCH_SLIDE_OFFSET_X, 0f), 0.25f).SetEase(Ease.InBack));
        seq.OnComplete(() => Managers.UI.ClosePopupUI());
    }

    void OnClickPotion() => ExpandGroup(0);
    void OnClickSkill()  => ExpandGroup(1);

    /// <summary>
    /// 슬라이드 35: Potion/Skill 버튼 클릭 시 리스트 확장.
    /// - 선택 버튼 Frame 색 F5DD6C (다른 버튼은 흰색)
    /// - 두 버튼 모두 y=-40으로 이동 (Move OutBack 0.5s)
    /// - 대응 그룹 표시 + 각 아이템 Scale(from 1.8, OutBack 0.4s) 등장
    /// </summary>
    void ExpandGroup(int groupIndex)
    {
        // 재클릭이면 무시 (같은 그룹 이미 표시 중)
        if (_currentGroup == groupIndex) return;

        var potionBtnRT = GetButton((int)Buttons.CustomOrderPotionBtn).GetComponent<RectTransform>();
        var skillBtnRT  = GetButton((int)Buttons.CustomOrderSkillBtn).GetComponent<RectTransform>();
        var potionImg   = GetButton((int)Buttons.CustomOrderPotionBtn).GetComponent<Image>();
        var skillImg    = GetButton((int)Buttons.CustomOrderSkillBtn).GetComponent<Image>();

        // Frame 색: 선택 버튼만 F5DD6C
        potionImg.color = (groupIndex == 0) ? FRAME_SELECTED_COLOR : FRAME_DEFAULT_COLOR;
        skillImg.color  = (groupIndex == 1) ? FRAME_SELECTED_COLOR : FRAME_DEFAULT_COLOR;

        // 두 버튼 모두 y=-40으로 Move OutBack 0.5s
        potionBtnRT.DOKill();
        skillBtnRT.DOKill();
        potionBtnRT.DOAnchorPosY(BTN_Y_EXPANDED, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        skillBtnRT.DOAnchorPosY(BTN_Y_EXPANDED, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        // 그룹 전환
        var potionGrp = GetObject((int)GameObjects.Btm_PotionBtnGroup);
        var skillGrp  = GetObject((int)GameObjects.Btm_SkillBtnGroup);
        potionGrp.SetActive(groupIndex == 0);
        skillGrp.SetActive(groupIndex == 1);

        // 각 아이템 스케일 인 anim
        var activeGroup = (groupIndex == 0) ? potionGrp : skillGrp;
        PlayItemsScaleIn(activeGroup);

        _currentGroup = groupIndex;

        // 슬35 해금 연출: 컨텐츠로 해금됐는데 아직 모션을 안 본 항목이면 그룹 펼친 순간 1회 재생.
        if (groupIndex == 0)
        {
            // 물약 그룹: 신속(SPD) → 행운(Luck) 순. (SPD 트리거는 별도 과제로 컨텐츠 게이팅 예정)
            if (!Managers.Game.SpdPotionRevealed)
                PlaySpdPotionRevealAnim();
            else if (Managers.Game.LuckPotionUnlocked && !Managers.Game.LuckPotionRevealed)
                PlayLuckPotionRevealAnim();
        }
        else if (groupIndex == 1)
        {
            // 기술 그룹: 물약주입기 — 기술 습득(AutoPotionMemorized) 후 첫 조회 시 해금 모션.
            if (Managers.Game.AutoPotionMemorized && !Managers.Game.AutoPotionSkillRevealed)
                PlayAutoPotionSkillRevealAnim();
        }
    }

    /// <summary>
    /// 슬35 해금 연출 (공통). LockIcon에만 적용. 신속/행운 물약 + 물약주입기 기술 공용.
    /// 타임라인:
    ///   t=0.0~0.5  대기 (Delay)
    ///   t=0.5~1.2  Shake/Position (Duration 0.7, Strength (1,1,0), Vibrato 48, Randomness 59, OutQuad)
    ///   t=1.2~1.7  Scale 1→2 (Duration 0.5, OutBack)
    ///   t=1.7~2.1  Lock overlay Fade out → onRevealed (플래그 저장/아이콘 갱신) + MasterIcon/LvlIcon 노출
    /// </summary>
    void PlayRevealAnim(Transform iconRoot, Image lockBg, Image lockImg, System.Action onRevealed)
    {
        if (lockImg == null) { onRevealed?.Invoke(); return; }

        var lockImgTr = lockImg.rectTransform;
        Vector2 lockImgAnchorOrigin = lockImgTr.anchoredPosition;

        // 초기 상태 정리
        lockImgTr.DOKill();
        lockImgTr.localScale = Vector3.one;
        lockImgTr.anchoredPosition = lockImgAnchorOrigin;

        Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(gameObject).SetUpdate(true);
        seq.AppendInterval(0.5f);                                   // Delay
        seq.AppendCallback(() =>                                    // Shake/Position
            lockImgTr.DOShakeAnchorPos(0.7f, new Vector3(1f, 1f, 0f), vibrato: 48, randomness: 59f)
                     .SetEase(Ease.OutQuad).SetUpdate(true));
        seq.AppendInterval(0.7f);
        seq.AppendCallback(() =>                                    // Scale 1→2
            lockImgTr.DOScale(2f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true));
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() =>                                    // Lock overlay Fade out
        {
            if (lockBg != null) lockBg.DOFade(0f, 0.4f).SetUpdate(true);
            lockImg.DOFade(0f, 0.4f).SetUpdate(true);
        });
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() =>                                    // 플래그 저장 + 등급/레벨 아이콘 노출 + 원복
        {
            onRevealed?.Invoke();
            if (iconRoot != null)
            {
                foreach (Transform child in iconRoot)
                {
                    var n = child.name;
                    if (n == "MasterIcon" || n.StartsWith("Img_MasterIcon") ||
                        n == "LvlIcon"    || n.StartsWith("Img_LvlIcon"))
                        child.gameObject.SetActive(true);
                }
            }
            lockImgTr.anchoredPosition = lockImgAnchorOrigin;
            lockImgTr.localScale = Vector3.one;
        });
    }

    // 슬35: 신속물약 해금 연출
    void PlaySpdPotionRevealAnim() => PlayRevealAnim(
        GetObject((int)GameObjects.CustomOrderSPDPotionIcon)?.transform,
        GetImage((int)Images.lockbg_1), GetImage((int)Images.lockimg_1),
        () => { Managers.Game.SpdPotionRevealed = true; RefreshSpeedPotionIcon(); });

    // 슬35: 행운물약 해금 연출
    void PlayLuckPotionRevealAnim() => PlayRevealAnim(
        GetObject((int)GameObjects.CustomOrderLuckPotionIcon)?.transform,
        GetImage((int)Images.lockbg), GetImage((int)Images.lockimg),
        () => { Managers.Game.LuckPotionRevealed = true; });

    // 슬35: 물약주입기 기술 해금 연출 (기술 그룹)
    void PlayAutoPotionSkillRevealAnim() => PlayRevealAnim(
        GetObject((int)GameObjects.CustomOrder_AutoPotionCooltimeSkill)?.transform,
        GetImage((int)Images.lockbg_AutoPotion), GetImage((int)Images.lockimg_AutoPotion),
        () => { Managers.Game.AutoPotionSkillRevealed = true; });

    void PlayItemsScaleIn(GameObject groupGO)
    {
        // 직계 자식만 순회 (각 아이템 아이콘 루트)
        foreach (Transform child in groupGO.transform)
        {
            var rt = child as RectTransform;
            if (rt == null) continue;
            rt.DOKill();
            rt.localScale = Vector3.one * 1.8f;
            rt.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    void ResetBottomButtons()
    {
        var potionBtn = GetButton((int)Buttons.CustomOrderPotionBtn);
        var skillBtn  = GetButton((int)Buttons.CustomOrderSkillBtn);

        potionBtn.GetComponent<RectTransform>().DOKill();
        skillBtn.GetComponent<RectTransform>().DOKill();
        potionBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(14.5f,  BTN_Y_ORIGIN);
        skillBtn.GetComponent<RectTransform>().anchoredPosition  = new Vector2(-15.5f, BTN_Y_ORIGIN);
        potionBtn.GetComponent<Image>().color = FRAME_DEFAULT_COLOR;
        skillBtn.GetComponent<Image>().color  = FRAME_DEFAULT_COLOR;
    }

    /// <summary>
    /// 슬라이드 35 잠금 상태 스펙 적용.
    /// 회복물약(HP) 해금. 신속(SPD)는 SpdPotionRevealed 저장값에 따라 잠금/해금.
    /// 행운(Luck)/물약주입기(AutoPotion) 잠금.
    /// Sprite None (000000 alpha 180) + Chat_Icon_Lock (FFFFFF alpha 80)
    /// </summary>
    void InitLockStates()
    {
        // 슬35: 각 항목은 해금 모션을 본 뒤(*Revealed=true) 해제 상태로 표시. 그 전까지 잠금 자물쇠.
        //  (Revealed는 컨텐츠 해금 후 첫 조회 시 ExpandGroup의 해금 연출이 set — 신속/행운/물약주입기 공통)
        ApplyLockVisual(GetObject((int)GameObjects.CustomOrderSPDPotionIcon).transform,
                        GetImage((int)Images.lockbg_1), GetImage((int)Images.lockimg_1), locked: !Managers.Game.SpdPotionRevealed);
        ApplyLockVisual(GetObject((int)GameObjects.CustomOrderLuckPotionIcon).transform,
                        GetImage((int)Images.lockbg),   GetImage((int)Images.lockimg),   locked: !Managers.Game.LuckPotionRevealed);
        ApplyLockVisual(GetObject((int)GameObjects.CustomOrder_AutoPotionCooltimeSkill).transform,
                        GetImage((int)Images.lockbg_AutoPotion), GetImage((int)Images.lockimg_AutoPotion), locked: !Managers.Game.AutoPotionSkillRevealed);

        // 슬라이드 36: 해금 아이템 클릭 → 상세 팝업
        // CustomOrderHPPotionIcon 컨테이너엔 Graphic이 없어 raycast 불가 → Frame Button으로 바인딩
        GetButton((int)Buttons.CustomOrderHPPotionFrame).gameObject.BindEvent(
            () => OpenPotionDetail(Managers.Game.CurrentHPPotionDataId));
        GetButton((int)Buttons.CustomOrderSPDPotionFrame).gameObject.BindEvent(
            () =>
            {
                // SPD가 아직 공개되지 않은 상태에서 클릭 방지 (해금 애니 중에는 상호작용 차단)
                if (!Managers.Game.SpdPotionRevealed) return;
                OpenPotionDetail(Managers.Game.CurrentSpeedPotionDataId);
            });
    }

    void OpenPotionDetail(int dataId)
    {
        Managers.UI.ShowPopupUI<UI_PotionDetailPopup>().SetInfo(dataId, fromCustomOrder: true);
    }

    /// <summary>
    /// HP/SPD 물약 아이콘(Sprite)과 등급 Glow 색을 현재 물약 데이터 기준으로 갱신.
    /// (해금되지 않은 SPD/Luck은 이후 ApplyLockVisual에서 잠금 오버레이가 덮음)
    /// </summary>
    void RefreshPotionIcons()
    {
        RefreshHPPotionIcon();
        RefreshSpeedPotionIcon();
    }

    void RefreshHPPotionIcon()
    {
        int dataId = Managers.Game.CurrentHPPotionDataId;
        if (!Managers.Data.HPPotionDic.TryGetValue(dataId, out var data)) return;

        var img = GetImage((int)Images.CustomOrderHPPotionImg);
        if (img != null)
        {
            var sp = Managers.Resource.Load<Sprite>(data.SpriteName);
            if (sp != null) img.sprite = sp;
        }

        var glow = GetImage((int)Images.Img_GlowMask_2_02);
        if (glow != null) glow.color = Util.GetGradeColor(data.Grade);
    }

    void RefreshSpeedPotionIcon()
    {
        int dataId = Managers.Game.CurrentSpeedPotionDataId;
        if (!Managers.Data.SpeedPotionDic.TryGetValue(dataId, out var data)) return;

        var img = GetImage((int)Images.CustomOrderSPDPotionImg);
        if (img != null)
        {
            var sp = Managers.Resource.Load<Sprite>(data.SpriteName);
            if (sp != null) img.sprite = sp;
        }

        var glow = GetImage((int)Images.Img_GlowMask_3_03);
        if (glow != null) glow.color = Util.GetGradeColor(data.Grade);
    }

    void OpenSkillDetail(int skillId)
    {
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(skillId);
    }

    void ApplyLockVisual(Transform item, Image lockBg, Image lockImg, bool locked)
    {
        if (item == null) return;

        // prefab에 autoPlay=true로 걸린 "해금 연출" DOTweenAnimation은
        // 매 OnEnable마다 재생되어 팝업을 다시 열 때마다 해금 애니가 반복됨.
        // 잠긴 아이템/해금된 아이템 모두 이 auto-play 애님을 제거해서 최종 상태만 유지.
        foreach (var dta in item.GetComponentsInChildren<DG.Tweening.DOTweenAnimation>(true))
        {
            if      (lockBg  != null && dta.name == lockBg.name)  DestroyImmediate(dta);
            else if (lockImg != null && dta.name == lockImg.name) DestroyImmediate(dta);
        }

        if (lockBg != null)
        {
            lockBg.color = locked ? LOCK_BG_COLOR : new Color(0, 0, 0, 0);
            if (locked) lockBg.transform.SetAsLastSibling();
        }
        if (lockImg != null)
        {
            lockImg.color = locked ? LOCK_ICON_COLOR : new Color(1, 1, 1, 0);
            if (locked) lockImg.transform.SetAsLastSibling();
        }

        // NEW 뱃지 / 등급·레벨 아이콘은 잠긴 상태에서 숨김
        if (locked)
        {
            foreach (var tmp in item.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                if (tmp.name.EndsWith("NewTxt")) tmp.gameObject.SetActive(false);
            }
            foreach (Transform child in item)
            {
                var n = child.name;
                if (n == "MasterIcon" || n.StartsWith("Img_MasterIcon") ||
                    n == "LvlIcon"    || n.StartsWith("Img_LvlIcon"))
                    child.gameObject.SetActive(false);
            }
        }
    }
}
