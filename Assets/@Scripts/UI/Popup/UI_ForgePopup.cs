using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data;
using static Define;

/// <summary>
/// 대장간 팝업 — 장비 상세의 [대장간] 버튼으로 진입.
/// ForgePanel(장비 복구 + 장비 강화)만 이번 스코프. CraftingPanel/SOM/Poten/돌파(ForceUse)는 잠금·비활성.
/// 복구: 재료(10001×5) 우선, 없으면 거울(10015)로 내구도 +25.
/// 강화: 주문서 4종 중 선택 → [강화] → UI_EnhacePopup으로 진행.
/// </summary>
public class UI_ForgePopup : UI_Popup
{
    #region Enum

    enum Images
    {
        WIcon,                  // 장비 아이콘 (TopUIGroup)
        Img_ReqItemBox1_21,     // 복구 재료 아이콘 (IngredientBox 첫 슬롯)
        Img_BMORBox_01,         // 축복 거울 아이콘 — 스코프 제외(dim)
        Img_MORBox_02,          // 거울 아이콘
        Img_Scroll1Box_06,      // 주문서 아이콘 1~4
        Img_Scroll2Box_14,
        Img_Scroll3Box_11,
        Img_Scroll4Box_07,
        Img_SOM1Box_12,         // SOM 박스 — 스코프 제외(dim)
        Img_SOM2Box_09,
        Img_SOM3Box_13,
    }

    enum Texts
    {
        ForgePopupTitle,
        ENumber,                // "+N"
        ForgeEquipNameText,
        DuriText,               // "cur/max"
        Txt_ReqItemBox1_21,     // 복구 재료 수량 "n/5"
        Txt_BMORBox_01,
        Txt_MORBox_02,
        Txt_Scroll1Box_04,      // 주문서 보유 수량 1~4
        Txt_Scroll2Box_17,
        Txt_Scroll3Box_09,
        Txt_Scroll4Box_14,
        Txt_SOM1Box_10,
        Txt_SOM2Box_12,
        Txt_SOM3Box_06,
    }

    enum Buttons
    {
        Btn_BtnGroup_01,        // 복구
        Btn_BtnGroup_02,        // 강화
        Scroll1Box,
        Scroll2Box,
        Scroll3Box,
        Scroll4Box,
    }

    enum GameObjects
    {
        ForgePopupFrame,
        CraftingPanel,
        ForgePanel,
        Btn_Content_10,         // 복구 재료 첫 슬롯 (prefab INACTIVE → 활성화)
        Img_BtnGroup_01,        // CANT RECOV 오버레이
        Img_BtnGroup_02,        // CANT ENHANCE 오버레이
        ForceEnhTxtGroup,       // 강제 강화 뱃지 — 스코프 제외
        DuriOverRecovPopup,     // 초과 복구 경고 — 스코프 제외 (prefab ACTIVE → 꺼야 함)
        Xbutton,
    }

    #endregion

    static readonly Color DIM_COLOR    = new Color(1f, 1f, 1f, 0.3f);
    static readonly Color SELECT_COLOR = new Color(1f, 0.85f, 0.3f, 0.35f);   // 선택 주문서 하이라이트

    Equipment _equip;
    int _selectedScroll = -1;   // ENHANCE_SCROLL_IDS 인덱스 (-1 = 미선택)

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // 패널 상태 — 이번 스코프는 ForgePanel만
        GetObject((int)GameObjects.CraftingPanel)?.SetActive(false);
        GetObject((int)GameObjects.ForgePanel)?.SetActive(true);
        GetObject((int)GameObjects.DuriOverRecovPopup)?.SetActive(false);
        GetObject((int)GameObjects.ForceEnhTxtGroup)?.SetActive(false);
        GetObject((int)GameObjects.Btn_Content_10)?.SetActive(true);

        // ⚠️ 프리팹에 **화면 전체를 덮는 투명 차단막**이 켜진 채 저작돼 있다.
        //  ForgeBlockImg는 렌더 순서상 콘텐츠(ForgePopupFrame)보다 뒤라 위에 그려지고,
        //  alpha 0이라 보이지도 않으면서 raycastTarget=true로 **모든 클릭을 삼킨다**.
        //  → 대장간이 열리긴 하는데 아무것도 눌리지 않고 X로 닫지도 못했다("게임이 멈춤").
        //  진행 중 입력을 막을 용도로 만들어진 것 같은데 켜고 끄는 코드가 없다. 열 때 꺼둔다.
        BlockRaycast("ForgeBlockImg", false);
        BlockRaycast("ClickBlock", false);

        // 버튼
        GetButton((int)Buttons.Btn_BtnGroup_01).onClick.AddListener(OnClickRestore);
        GetButton((int)Buttons.Btn_BtnGroup_02).onClick.AddListener(OnClickEnhance);
        for (int i = 0; i < 4; i++)
        {
            int idx = i;   // 클로저 캡처
            GetButton((int)Buttons.Scroll1Box + i).onClick.AddListener(() => OnClickScroll(idx));
        }
        GetObject((int)GameObjects.Xbutton).BindEvent(OnClickClose);

        // 타이틀 로컬라이즈 (대장간)
        GetText((int)Texts.ForgePopupTitle).text = Util.Localize(LocalizeKey.SmithyText);

        // SOM 박스 — 스코프 제외: dim 처리
        for (int i = 0; i < 3; i++)
        {
            var icon = GetImage((int)Images.Img_SOM1Box_12 + i);
            if (icon != null) icon.color = DIM_COLOR;
            var txt = GetText((int)Texts.Txt_SOM1Box_10 + i);
            if (txt != null) txt.text = "";
        }

        if (Managers.Game != null)
            Managers.Game.EquipInfoChanged += OnEquipInfoChanged;

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Managers.Game != null)
            Managers.Game.EquipInfoChanged -= OnEquipInfoChanged;
    }

    void OnEquipInfoChanged()
    {
        if (_equip == null) return;
        EnsureScrollSelection();   // 자식 강화창에서 선택 주문서가 소진됐으면 보유 중인 다른 주문서로 재선택
        RefreshUI();
    }

    // ────────────────────────────────────────
    //  SetInfo
    // ────────────────────────────────────────
    public void SetInfo(Equipment equip)
    {
        _equip = equip;
        if (_equip == null) return;

        // 기본 선택: 보유 중인 첫 주문서
        _selectedScroll = -1;
        EnsureScrollSelection();

        RefreshUI();

        // 등장 연출 — prefab authored DOTweenAnimation(id=1 Move / id=2 PunchScale) 재생
        var frame = GetObject((int)GameObjects.ForgePopupFrame);
        if (frame != null)
            foreach (var anim in frame.GetComponents<DOTweenAnimation>())
                if (anim.id == "1") anim.DORestart();
    }

    // 선택 주문서가 없거나 소진됐으면 보유 중인 첫 주문서로 재선택
    void EnsureScrollSelection()
    {
        if (_selectedScroll >= 0 && Managers.Game.HasItem(ENHANCE_SCROLL_IDS[_selectedScroll]))
            return;
        _selectedScroll = -1;
        for (int i = 0; i < ENHANCE_SCROLL_IDS.Length; i++)
        {
            if (Managers.Game.HasItem(ENHANCE_SCROLL_IDS[i]))
            {
                _selectedScroll = i;
                break;
            }
        }
    }

    // ────────────────────────────────────────
    //  Refresh
    // ────────────────────────────────────────
    public override void RefreshUI()
    {
        if (_equip == null || _equip.EquipmentData == null) return;
        EquipmentData data = _equip.EquipmentData;

        // ── TopUIGroup: 장비 아이콘 / +N / 이름 ──
        var wIcon = GetImage((int)Images.WIcon);
        Sprite sp = Managers.Resource.Load<Sprite>(data.SpriteName);
        if (wIcon != null && sp != null)
        {
            // WIcon의 EquipIcon Animator가 sprite를 덮어쓸 수 있어 코드 세팅 시 정지
            var animator = wIcon.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            wIcon.sprite = sp;
        }
        GetText((int)Texts.ENumber).text = _equip.EnhanceLevel > 0 ? $"+{_equip.EnhanceLevel}" : "";
        GetText((int)Texts.ForgeEquipNameText).text = GetEquipName(_equip.EnhanceLevel);

        // ── RestorePart: 내구도 / 재료 / 거울 ──
        int maxDuri = data.MaxDurability;
        // 프리팹에 "내구도" 라벨과 수치가 **따로** 놓여 있는데 간격이 10px밖에 안 돼 서로 겹쳐 있었다
        //  ("내구도110/110"처럼 뭉개져 보임). 라벨을 수치 텍스트 안으로 합치고 원래 라벨은 끈다.
        //  원본 화면처럼 현재 수치만 초록으로 준다.
        var duriLabel = Util.FindChild<TMPro.TMP_Text>(gameObject, "Text_2", true);
        if (duriLabel != null) duriLabel.gameObject.SetActive(false);
        GetText((int)Texts.DuriText).text =
            $"{Util.Localize(Define.LocalizeKey.ForgeDurabilityText)} <color=#4CFF4C>{_equip.Durability}</color>/{maxDuri}";

        // 프리팹에 영문 placeholder("CANT RECOV"/"CANT ENHANCE")가 박혀 있어 한글 화면에 그대로 노출됐다.
        SetOverlayText("Txt_CantEnhaceCraft_13", Util.Localize(Define.LocalizeKey.ForgeCantRecoverText));
        SetOverlayText("Txt_CantEnhaceCraft_09", Util.Localize(Define.LocalizeKey.ForgeCantEnhanceText));

        int matCount = Managers.Game.GetItemCount(RESTORE_MATERIAL_ID);
        bool matOk   = matCount >= RESTORE_MATERIAL_COUNT;
        SetItemBox(GetImage((int)Images.Img_ReqItemBox1_21), GetText((int)Texts.Txt_ReqItemBox1_21),
                   RESTORE_MATERIAL_ID, $"{matCount}/{RESTORE_MATERIAL_COUNT}", matOk);

        int morCount = Managers.Game.GetItemCount(MOR_ITEM_ID);
        SetItemBox(GetImage((int)Images.Img_MORBox_02), GetText((int)Texts.Txt_MORBox_02),
                   MOR_ITEM_ID, morCount.ToString(), morCount > 0);

        // ponytail: BMOR(축복 거울)은 아이템 미정의 — 항상 dim, 아이템 추가 시 연결
        var bmorIcon = GetImage((int)Images.Img_BMORBox_01);
        if (bmorIcon != null) bmorIcon.color = DIM_COLOR;
        GetText((int)Texts.Txt_BMORBox_01).text = "0";

        bool canRestore = _equip.Durability < maxDuri && (matOk || morCount > 0);
        GetObject((int)GameObjects.Img_BtnGroup_01)?.SetActive(!canRestore);
        if (!canRestore)
        {
            // 강화쪽과 같은 이유 — "복구불가"만 떠 있으면 왜 안 되는지 알 수 없다.
            SetOverlayText("Txt_CantEnhaceCraft_13",
                _equip.Durability >= maxDuri ? "내구도 가득" : "재료 부족");
        }

        // ── EnhancePart: 주문서 4종 ──
        for (int i = 0; i < 4; i++)
        {
            int count = Managers.Game.GetItemCount(ENHANCE_SCROLL_IDS[i]);
            SetItemBox(GetImage((int)Images.Img_Scroll1Box_06 + i), GetText((int)Texts.Txt_Scroll1Box_04 + i),
                       ENHANCE_SCROLL_IDS[i], count.ToString(), count > 0);

            // 선택 하이라이트 — 박스 배경(투명 Image) 색으로 표시
            var boxImg = GetButton((int)Buttons.Scroll1Box + i).GetComponent<Image>();
            if (boxImg != null)
                boxImg.color = _selectedScroll == i ? SELECT_COLOR : new Color(0.2f, 0.2f, 0.2f, 0f);
        }

        bool canEnhance = _selectedScroll >= 0
            && Managers.Game.CanEnhance(_equip, ENHANCE_SCROLL_IDS[_selectedScroll]);
        GetObject((int)GameObjects.Img_BtnGroup_02)?.SetActive(!canEnhance);

        // 막혔으면 **왜** 막혔는지 버튼에 쓴다.
        //  갓 시작한 플레이어는 골드가 0인데 강화 비용이 120이라 항상 막히는데,
        //  화면엔 "강화불가"만 떠서 대장간이 통째로 고장 난 것처럼 보였다.
        if (!canEnhance)
            SetOverlayText("Txt_CantEnhaceCraft_09", BlockReason());
    }

    // 강화가 막힌 이유 (표시 우선순위 = 플레이어가 먼저 해결해야 하는 순서)
    string BlockReason()
    {
        if (_equip == null) return Util.Localize(Define.LocalizeKey.ForgeCantEnhanceText);
        if (_equip.EnhanceLevel >= ENHANCE_MAX_LEVEL) return "최대 강화";
        if (_equip.Durability <= 0) return "내구도 0";
        if (_selectedScroll < 0) return "주문서 없음";

        int cost = Managers.Game.GetEnhanceCost(_equip);
        if (Managers.Game.Gold < cost) return $"골드 {cost:N0} 필요";
        return Util.Localize(Define.LocalizeKey.ForgeCantEnhanceText);
    }

    // 아이템 박스 공용 세팅 — 아이콘(ItemDic 스프라이트, 없으면 authored 유지) + 수량 + dim
    // 전체 화면 차단막의 클릭 가로채기만 끈다(오브젝트는 남겨둔다 — 나중에 켤 수 있게).
    void BlockRaycast(string childName, bool on)
    {
        var img = Util.FindChild<Image>(gameObject, childName, true);
        if (img != null) img.raycastTarget = on;
    }

    // 프리팹 안 이름으로 찾아 문자열을 넣는다 (enum Texts에 없는 오버레이 라벨용).
    //  로컬라이즈가 필요한 값은 호출부에서 Util.Localize 해서 넘긴다 —
    //  여기서 무조건 Localize 하면 사유 문구처럼 조립된 문자열이 키로 취급돼 사라진다.
    void SetOverlayText(string childName, string text)
    {
        var t = Util.FindChild<TMP_Text>(gameObject, childName, true);
        if (t != null) t.text = text;
    }

    void SetItemBox(Image icon, TMP_Text countTxt, int itemId, string countStr, bool enough)
    {
        if (icon != null)
        {
            if (Managers.Data.ItemDic.TryGetValue(itemId, out var item))
            {
                Sprite sp = Managers.Resource.Load<Sprite>(item.SpriteName);
                if (sp != null) icon.sprite = sp;
            }
            icon.color = enough ? Color.white : DIM_COLOR;
        }
        if (countTxt != null) countTxt.text = countStr;
    }

    string GetEquipName(int level)
    {
        string nm = Util.Localize(_equip.EquipmentData.Name);
        return level > 0 ? $"+{level} {nm}" : nm;
    }

    // ────────────────────────────────────────
    //  버튼 핸들러
    // ────────────────────────────────────────
    void OnClickRestore()
    {
        if (_equip == null || _isClosing) return;

        // 재료 우선, 없으면 거울 — 성공 시 EquipInfoChanged로 자동 갱신
        if (Managers.Game.HasItem(RESTORE_MATERIAL_ID, RESTORE_MATERIAL_COUNT))
            Managers.Game.RestoreDurability(_equip, useMirror: false);
        else if (Managers.Game.HasItem(MOR_ITEM_ID))
            Managers.Game.RestoreDurability(_equip, useMirror: true);
    }

    void OnClickScroll(int idx)
    {
        if (_isClosing) return;
        if (!Managers.Game.HasItem(ENHANCE_SCROLL_IDS[idx])) return;   // 미보유 주문서 선택 불가
        _selectedScroll = idx;
        RefreshUI();
    }

    void OnClickEnhance()
    {
        if (_equip == null || _isClosing) return;   // 닫기 지연 창 중 새 팝업 push 차단

        // 주문서를 안 고른 채 누르면 예전엔 **아무 반응도 없었다** — 대장간이 고장난 것처럼 보이는
        //  전형적인 지점이다. 왜 안 되는지 말해 준다.
        if (_selectedScroll < 0)
        {
            Managers.UI.ShowToast("강화에 쓸 주문서를 먼저 선택하세요");
            return;
        }

        int scrollId = ENHANCE_SCROLL_IDS[_selectedScroll];

        // 골드 부족은 버튼만 막으면 이유를 알 수 없다 — 필요 금액을 알려준다.
        int cost = Managers.Game.GetEnhanceCost(_equip);
        if (Managers.Game.Gold < cost)
        {
            Managers.UI.ShowToast($"골드가 부족합니다\n강화 비용 {cost:N0} (보유 {Managers.Game.Gold:N0})");
            return;
        }
        if (!Managers.Game.CanEnhance(_equip, scrollId)) return;

        Managers.UI.ShowPopupUI<UI_EnhacePopup>().SetInfo(_equip, scrollId);
    }

    void OnClickClose()
    {
        if (_isClosing) return;
        _isClosing = true;

        // Xbutton authored PunchScale(id=xButtonClick) 재생 후 닫기
        var xbtn = GetObject((int)GameObjects.Xbutton);
        if (xbtn != null)
            foreach (var anim in xbtn.GetComponents<DOTweenAnimation>())
                if (anim.id == "xButtonClick") anim.DORestart();

        // 대상 지정 닫기 — 지연 중 다른 팝업이 push돼도 엉뚱한 top이 닫히지 않게 (UIManager는 top 불일치 시 no-op)
        DOVirtual.DelayedCall(0.2f, () => Managers.UI.ClosePopupUI(this), ignoreTimeScale: true);
    }
}
