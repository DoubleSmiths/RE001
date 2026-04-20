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
        CustomOrderPopupBackground,
        Alch_Img_mask_1,
        Xbutton,
    }

    enum Texts
    {
        CustomOrderPopupTitle,
    }

    enum Buttons
    {
        CustomOrderPotionBtn,
        CustomOrderSkillBtn,
    }

    enum GameObjects
    {
        CustomOrderPopup,
        CustomOrderPopupFrame,
        TalkText,
        Btm_PotionBtnGroup,
        Btm_SkillBtnGroup,
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

            // prefab의 ContentSizeFitter + DOTweenAnimation이 위치/크기를 조작해서 위치가 밀림.
            // DestroyImmediate로 즉시 제거해야 이후 set이 덮어쓰여지지 않음.
            var csf = korTf.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (csf != null) DestroyImmediate(csf);
            var dta = korTf.GetComponent<DG.Tweening.DOTweenAnimation>();
            if (dta != null) DestroyImmediate(dta);
        }

        GetImage((int)Images.CustomOrderPopupBackground).gameObject.BindEvent(OnClickClose);
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
            }).SetUpdate(true);
        }

        // BlackBG fade (slide 8: Duration 0.3, OutQuad, target alpha 220/255)
        var bg = GetImage((int)Images.CustomOrderPopupBackground);
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
        Sequence seq = DOTween.Sequence().SetUpdate(true);
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
    }

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
    /// 회복물약(HP)만 해금, 신속(SPD)/행운(Luck)/물약주입기(AutoPotion) 잠금.
    /// Sprite None (000000 alpha 180) + Chat_Icon_Lock (FFFFFF alpha 80)
    /// </summary>
    void InitLockStates()
    {
        // 잠금
        ApplyLockVisual("CustomOrderSPDPotionIcon",              "lockbg_1",           "lockimg_1",           locked: true);
        ApplyLockVisual("CustomOrderLuckPotionIcon",             "lockbg",             "lockimg",             locked: true);
        ApplyLockVisual("CustomOrder_AutoPotionCooltimeSkill",   "lockbg_AutoPotion",  "lockimg_AutoPotion",  locked: true);

        // 슬라이드 36: 해금 아이템 클릭 → 상세 팝업
        BindItemClick("CustomOrderHPPotionIcon",             () => OpenPotionDetail(Managers.Game.CurrentHPPotionDataId), unlocked: true);
        BindItemClick("CustomOrderSPDPotionIcon",            () => OpenPotionDetail(-1),  unlocked: false);
        BindItemClick("CustomOrderLuckPotionIcon",           () => OpenPotionDetail(-1),  unlocked: false);
        BindItemClick("CustomOrder_AutoPotionCooltimeSkill", () => OpenSkillDetail(0),    unlocked: false);

        // ----- 그룹 트리 덤프 (디버그 전용) -----
#if UNITY_EDITOR
        var potionGrp = GetObject((int)GameObjects.Btm_PotionBtnGroup);
        Debug.Log($"[CustomOrder] Btm_PotionBtnGroup null? {potionGrp == null}");
        if (potionGrp != null) LogChildren(potionGrp.transform, 0);
#endif
    }

#if UNITY_EDITOR
    void LogChildren(Transform t, int depth)
    {
        string indent = new string('-', depth * 2);
        Debug.Log($"[CustomOrder]{indent} {t.name} (active={t.gameObject.activeSelf})");
        foreach (Transform child in t) LogChildren(child, depth + 1);
    }
#endif

    void BindItemClick(string itemName, Action onClick, bool unlocked)
    {
        if (!unlocked) return;

        var item = FindItemInGroups(itemName);
        if (item == null)
        {
            Debug.LogWarning($"[CustomOrderPopup] BindItemClick: '{itemName}' NOT FOUND");
            return;
        }

        Debug.Log($"[CustomOrderPopup] BindItemClick: '{itemName}' found → BindEvent 등록");
        item.gameObject.BindEvent(onClick);
    }

    void OpenPotionDetail(int dataId)
    {
        Debug.Log($"[CustomOrderPopup] OpenPotionDetail dataId={dataId}");
        Managers.UI.ShowPopupUI<UI_PotionDetailPopup>().SetInfo(dataId, fromCustomOrder: true);
    }

    void OpenSkillDetail(int skillId)
    {
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(skillId);
    }

    // 이미 바인딩된 그룹 오브젝트에서 itemName 자식을 재귀 탐색
    Transform FindItemInGroups(string itemName)
    {
        var potionGrp = GetObject((int)GameObjects.Btm_PotionBtnGroup);
        var skillGrp  = GetObject((int)GameObjects.Btm_SkillBtnGroup);

        Transform found = potionGrp != null ? FindDeep(potionGrp.transform, itemName) : null;
        if (found == null && skillGrp != null) found = FindDeep(skillGrp.transform, itemName);
        return found;
    }

    Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var result = FindDeep(child, name);
            if (result != null) return result;
        }
        return null;
    }

    void ApplyLockVisual(string itemName, string lockBgName, string lockImgName, bool locked)
    {
        var item = FindItemInGroups(itemName);
        if (item == null) return;

        // 잠금 유지: prefab에 autoPlay=true로 걸린 "해금 연출" DOTweenAnimation이
        // OnEnable에서 lock alpha를 0으로 만들어버리므로 잠긴 아이템은 이 애님을 제거.
        if (locked)
        {
            foreach (var dta in item.GetComponentsInChildren<DG.Tweening.DOTweenAnimation>(true))
            {
                if (dta.name == lockBgName || dta.name == lockImgName)
                    DestroyImmediate(dta);
            }
        }

        foreach (var img in item.GetComponentsInChildren<Image>(true))
        {
            if (img.name == lockBgName)
            {
                img.color = locked ? LOCK_BG_COLOR : new Color(0, 0, 0, 0);
                // 렌더 순서 최상위로 끌어올려 다른 아이콘/라벨이 잠금 비주얼을 덮지 않도록
                if (locked) img.transform.SetAsLastSibling();
            }
            if (img.name == lockImgName)
            {
                img.color = locked ? LOCK_ICON_COLOR : new Color(1, 1, 1, 0);
                if (locked) img.transform.SetAsLastSibling();
            }
        }

        // NEW 뱃지 / 등급·레벨 아이콘은 잠긴 상태에서 숨김 (잠긴 아이템은 Sprite None + Chat_Icon_Lock만 표시)
        if (locked)
        {
            foreach (var tmp in item.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                if (tmp.name.EndsWith("NewTxt")) tmp.gameObject.SetActive(false);
            }
            foreach (Transform child in item)
            {
                if (child.name == "MasterIcon" || child.name == "LvlIcon")
                    child.gameObject.SetActive(false);
            }
        }
    }
}
