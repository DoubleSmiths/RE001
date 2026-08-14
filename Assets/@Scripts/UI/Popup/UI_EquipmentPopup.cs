using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// 슬라이드 53-3: 장비 팝업 (RE001_Share VS → C# 포팅).
// 구조: 7슬롯(Weapon/Helmet/Cloak/Armor/Glove/Boots/Ring) — 각 슬롯마다
//   Open{Slot}ListBtn(슬롯 리스트 열기) + {Slot}List(슬롯 패널) + T1/T2/T3 리스트.
// 2차: OwnedEquipments를 부위별 슬롯 버튼(prefab 하드코딩)에 표시 —
//   아이콘/강화수치(+N)/E뱃지(장착중)/LowDuri(내구도 25% 이하)/빈 슬롯 dim.
//   슬롯 클릭 → UI_EquipDetailPopup.SetInfo(equipment). EquipInfoChanged 구독 갱신.
public class UI_EquipmentPopup : UI_Popup
{
    #region Enum

    enum Buttons
    {
        EquipmentPopupBackground,
        OpenWeaponListBtn,
        OpenHelemtListBtn,   // 원본 프리팹 오타(Helemt) 그대로 — 자동 바인드 호환 위해 유지
        OpenCloakListBtn,
        OpenArmorListBtn,
        OpenGloveListBtn,
        OpenBootsListBtn,
        OpenRingListBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        EquipmentPopup,
        WeaponList,
        HelmetList,
        CloakList,
        ArmorList,
        GloveList,
        BootsList,
        RingList,
    }

    #endregion

    // 슬롯 Open 버튼 → 표시할 리스트 컨테이너 매핑 (한 번에 하나만 노출).
    static readonly (Buttons btn, GameObjects list)[] s_slotMap =
    {
        (Buttons.OpenWeaponListBtn, GameObjects.WeaponList),
        (Buttons.OpenHelemtListBtn, GameObjects.HelmetList),
        (Buttons.OpenCloakListBtn,  GameObjects.CloakList),
        (Buttons.OpenArmorListBtn,  GameObjects.ArmorList),
        (Buttons.OpenGloveListBtn,  GameObjects.GloveList),
        (Buttons.OpenBootsListBtn,  GameObjects.BootsList),
        (Buttons.OpenRingListBtn,   GameObjects.RingList),
    };

    // ── 아이템 슬롯 (prefab 하드코딩 — tree_UI_EquipmentPopup.txt 노드명 그대로, 접미 숫자 포함) ──
    // slot: -1 = 대응 EquipSlotType 없음. Cloak(C00)은 데이터 enum(EquipSlotType 6종)에 부위가 없어 항상 빈 슬롯.
    static readonly (int slot, string btn, string icon, string lvTxt, string lowDuri, string dim, string badge)[] s_slotNodes =
    {
        ((int)EquipSlotType.Weapon,    "W00Btn", "Img_W00Btn_05", "Txt_W00Btn_04", "Img_W00Btn_03",  "Img_W00Btn_01", "Panel_W00Btn_10"),
        ((int)EquipSlotType.Weapon,    "W01Btn", "Img_W01Btn_08", "Txt_W01Btn_10", "Img_W01Btn_03",  "Img_W01Btn_10", "Panel_W01Btn_09"),
        ((int)EquipSlotType.Weapon,    "W02Btn", "Img_W02Btn_03", "Txt_W02Btn_05", "Img_W02Btn_04",  "Img_W02Btn_05", "Panel_W02Btn_03"),
        ((int)EquipSlotType.Weapon,    "W44Btn", "Img_W44Btn_10", "Txt_W44Btn_09", "Img_W44Btn_02",  "Img_W44Btn_05", "Panel_W44Btn_01"),
        ((int)EquipSlotType.Helmet,    "H00Btn", "Img_H00Btn_09", "Txt_H00Btn_06", "Img_H00Btn_01",  "Img_H00Btn_04", "Panel_H00Btn_02"),
        (-1,                           "C00Btn", "Img_C00Btn_02", "Txt_C00Btn_01", "Img_C00Btn_05",  "Img_C00Btn_04", "Panel_C00Btn_08"),
        ((int)EquipSlotType.Armor,     "A00Btn", "Img_A00Btn_04", "Txt_A00Btn_02", "Icon_LowDuri_2", "Img_A00Btn_12", "Panel_A00Btn_05"),
        ((int)EquipSlotType.Gloves,    "G00Btn", "Img_G00Btn_01", "Txt_G00Btn_07", "Icon_LowDuri_3", "Img_G00Btn_09", "Panel_G00Btn_07"),
        ((int)EquipSlotType.Legs,      "B00Btn", "Img_B00Btn_07", "Txt_B00Btn_03", "Icon_LowDuri_4", "Img_B00Btn_09", "Panel_B00Btn_04"),
        ((int)EquipSlotType.Accessory, "R00Btn", "Img_R00Btn_06", "Txt_R00Btn_08", "Icon_LowDuri_5", "Img_R00Btn_08", "Panel_R00Btn_06"),
    };

    const float LOW_DURI_RATE = 0.25f;   // 내구도 경고 기준(25% 이하)

    // 슬롯 하나의 노드 캐시 + 현재 표시 중인 장비
    class SlotView
    {
        public int        Slot;      // EquipSlotType (int). -1 = 미대응(Cloak)
        public GameObject Btn;
        public Image      Icon;
        public TMP_Text   LvTxt;
        public GameObject LowDuri;
        public GameObject Dim;
        public GameObject Badge;
        public Equipment  Equip;
    }

    readonly List<SlotView> _slotViews = new List<SlotView>();

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.EquipmentPopup).GetComponent<RectTransform>();

        // 배경 클릭 = 닫기
        GetButton((int)Buttons.EquipmentPopupBackground).gameObject.BindEvent(OnClickBG);

        // 슬롯 Open 버튼 → 해당 슬롯 리스트만 표시
        foreach (var (btn, list) in s_slotMap)
        {
            int listIdx = (int)list;
            GetButton((int)btn).gameObject.BindEvent(() => ShowSlotList(listIdx));
        }

        // 초기: 모든 슬롯 리스트 숨김 (무기부터 보이게 하려면 ShowSlotList((int)GameObjects.WeaponList))
        HideAllSlotLists();
        ShowSlotList((int)GameObjects.WeaponList);

        // 아이템 슬롯 구성 + 장비 정보 표시
        BuildSlotViews();
        if (Managers.Game != null)
            Managers.Game.EquipInfoChanged += RefreshUI;
        RefreshUI();

        OpenAnimation();
        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Managers.Game != null)
            Managers.Game.EquipInfoChanged -= RefreshUI;
    }

    // 지정 슬롯 리스트만 켜고 나머지는 끔.
    void ShowSlotList(int listIdx)
    {
        foreach (var (_, list) in s_slotMap)
        {
            var go = GetObject((int)list);
            if (go != null) go.SetActive((int)list == listIdx);
        }
    }

    void HideAllSlotLists()
    {
        foreach (var (_, list) in s_slotMap)
        {
            var go = GetObject((int)list);
            if (go != null) go.SetActive(false);
        }
    }

    #region 아이템 슬롯

    // s_slotNodes 테이블 기반으로 슬롯 노드 캐시 + 클릭 바인딩.
    void BuildSlotViews()
    {
        _slotViews.Clear();
        foreach (var n in s_slotNodes)
        {
            GameObject btnGo = Util.FindChild(gameObject, n.btn, true);
            if (btnGo == null)
            {
                Debug.Log($"[EquipmentPopup] 슬롯 노드 없음: {n.btn}");
                continue;
            }

            var view = new SlotView
            {
                Slot    = n.slot,
                Btn     = btnGo,
                Icon    = Util.FindChild<Image>(btnGo, n.icon, true),
                LvTxt   = Util.FindChild<TMP_Text>(btnGo, n.lvTxt, true),
                LowDuri = Util.FindChild(btnGo, n.lowDuri, true),
                Dim     = Util.FindChild(btnGo, n.dim, true),
                Badge   = Util.FindChild(btnGo, n.badge, true),
            };

            SlotView captured = view;
            btnGo.BindEvent(() => OnClickSlot(captured));
            _slotViews.Add(view);
        }
    }

    // 보유 장비를 부위별 슬롯에 배치 — 무기는 T1 리스트 4칸(W00/W01/W02/W44)에 보유 순서대로, 그 외 부위는 1칸.
    public override void RefreshUI()
    {
        if (Managers.Game == null || _slotViews.Count == 0) return;

        // 부위별 보유 리스트
        var bySlot = new Dictionary<int, List<Equipment>>();
        foreach (var e in Managers.Game.OwnedEquipments)
        {
            int key = (int)e.SlotType;
            if (bySlot.TryGetValue(key, out var list) == false)
                bySlot[key] = list = new List<Equipment>();
            list.Add(e);
        }

        var used = new Dictionary<int, int>();   // slotType → 이미 배치한 개수 (무기 4칸 순차 배치용)
        foreach (var view in _slotViews)
        {
            Equipment equip = null;
            if (view.Slot >= 0 && bySlot.TryGetValue(view.Slot, out var list))
            {
                used.TryGetValue(view.Slot, out int idx);
                if (idx < list.Count)
                {
                    equip = list[idx];
                    used[view.Slot] = idx + 1;
                }
            }
            ApplySlot(view, equip);
        }
    }

    // 슬롯 하나 갱신: 아이콘/강화수치/LowDuri/E뱃지/빈 슬롯 dim.
    void ApplySlot(SlotView view, Equipment equip)
    {
        view.Equip = equip;
        var data = equip?.EquipmentData;
        bool has = equip != null && data != null;

        var btn = view.Btn.GetComponent<Button>();
        if (btn != null) btn.interactable = has;

        // 장비 아이콘 (EquipmentData.SpriteName)
        if (view.Icon != null)
        {
            view.Icon.gameObject.SetActive(has);
            if (has && string.IsNullOrEmpty(data.SpriteName) == false)
            {
                var sp = Managers.Resource.Load<Sprite>(data.SpriteName);
                if (sp != null) { view.Icon.sprite = sp; view.Icon.preserveAspect = true; }
            }
        }

        // 강화수치 "+N" (0이면 숨김)
        if (view.LvTxt != null)
        {
            bool showLv = has && equip.EnhanceLevel > 0;
            view.LvTxt.gameObject.SetActive(showLv);
            if (showLv) view.LvTxt.text = $"+{equip.EnhanceLevel}";
        }

        // 내구도 25% 이하 경고 (모션은 prefab authored LowDuri Animator)
        bool lowDuri = has && equip.Durability <= data.MaxDurability * LOW_DURI_RATE;
        if (view.LowDuri != null) view.LowDuri.SetActive(lowDuri);

        // 'E' 장착 뱃지
        if (view.Badge != null) view.Badge.SetActive(has && equip.IsEquipped);

        // 빈 슬롯 dim
        if (view.Dim != null) view.Dim.SetActive(has == false);
    }

    // 슬롯 클릭 → 장비 상세 팝업
    void OnClickSlot(SlotView view)
    {
        if (view.Equip == null) return;
        Managers.UI.ShowPopupUI<UI_EquipDetailPopup>().SetInfo(view.Equip);
    }

    #endregion
}
