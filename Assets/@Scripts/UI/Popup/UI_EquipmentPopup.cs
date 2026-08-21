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
        BindTierTabs();
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

    // T1/T2/T3 탭 — 프리팹에 버튼은 있는데 코드가 없어 눌러도 아무 일도 안 났다.
    //  부위마다 자기 TierFrame을 갖고 있고 이름 규칙이 두 갈래다:
    //    Btn_HelmetTierFrame_01 / _03 / _02   (숫자가 티어 순서와 무관)
    //    ArmorT1Btn / ArmorT2Btn / ArmorT3Btn (이름에 티어가 있음)
    //  이름에 T1/T2/T3가 있으면 그걸 쓰고, 없으면 x좌표 순으로 본다.
    //  (비활성 프레임은 레이아웃이 아직 계산되지 않아 x가 전부 0이라 좌표만으로는 불안정하다)
    static List<RectTransform> TierTabsOf(RectTransform frame)
    {
        var tabs = new List<RectTransform>();
        foreach (Transform c in frame)
            if (c.GetComponent<Button>() != null) tabs.Add(c as RectTransform);
        if (tabs.Count == 0) return tabs;

        // ⚠️ x좌표로는 정렬할 수 없다 — 이 프리팹의 탭은 **전부 anchoredPosition (0,0)**이고
        //  위치를 부모 레이아웃이 잡는다(실측 확인). 예전 코드는 좌표 정렬로 폴백해서
        //  순서가 프리팹 자식 순서 그대로였고, 그래서 무엇을 눌러도 T1만 선택돼 보였다.
        tabs.Sort((a, b) => TierOf(a).CompareTo(TierOf(b)));
        return tabs;
    }

    /// <summary>이 탭이 T1/T2/T3 중 무엇인가 (0/1/2). 못 찾으면 큰 값을 줘 뒤로 민다.
    /// 이름 규칙이 두 갈래다 — ArmorT2Btn처럼 자기 이름에 있는 경우와,
    /// Btn_WeaponTierFrame_01처럼 **자식 라벨**(Txt_WeaponT3Btn_19)에만 있는 경우.</summary>
    static int TierOf(RectTransform tab)
    {
        int t = TierFromName(tab.name);
        if (t >= 0) return t;

        foreach (Transform c in tab)
        {
            t = TierFromName(c.name);
            if (t >= 0) return t;
        }
        return int.MaxValue;
    }

    static int TierFromName(string n)
    {
        if (n.IndexOf("T1", System.StringComparison.Ordinal) >= 0) return 0;
        if (n.IndexOf("T2", System.StringComparison.Ordinal) >= 0) return 1;
        if (n.IndexOf("T3", System.StringComparison.Ordinal) >= 0) return 2;
        return -1;
    }

    void BindTierTabs()
    {
        foreach (var frame in GetComponentsInChildren<RectTransform>(true))
        {
            if (!frame.name.Contains("TierFrame")) continue;

            var tabs = TierTabsOf(frame);
            for (int i = 0; i < tabs.Count; i++)
            {
                int tier = i;
                tabs[i].gameObject.BindEvent(() => SelectTier(tier));
            }
        }
        RefreshTierTabs();
    }

    void SelectTier(int tier)
    {
        if (_tier == tier) return;
        _tier = tier;
        RefreshTierTabs();
        RefreshUI();
        PunchTier(tier);
    }

    /// <summary>누른 탭만 톡 튀게 — 목록이 바뀌었다는 것을 눈으로 알리는 신호.</summary>
    void PunchTier(int tier)
    {
        foreach (var frame in GetComponentsInChildren<RectTransform>(true))
        {
            if (!frame.name.Contains("TierFrame") || !frame.gameObject.activeInHierarchy) continue;

            var tabs = TierTabsOf(frame);
            if (tier < 0 || tier >= tabs.Count) continue;

            tabs[tier].DOKill();
            tabs[tier].localScale = Vector3.one * 1.08f;
            tabs[tier].DOPunchScale(new Vector3(0.12f, 0.12f, 0f), 0.22f, 8, 0.6f)
                      .SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    // 선택된 탭만 밝게 (프리팹에 선택 상태 스프라이트가 따로 없어 색으로 구분)
    void RefreshTierTabs()
    {
        foreach (var frame in GetComponentsInChildren<RectTransform>(true))
        {
            if (!frame.name.Contains("TierFrame")) continue;

            var tabs = TierTabsOf(frame);
            for (int i = 0; i < tabs.Count; i++)
            {
                bool on = i == _tier;

                // 프리팹에 **선택 상태 그림이 이미 저작돼 있다**(shop_tab_focus + UIMask).
                //  T1만 켜진 채로 저장돼 있었고 아무도 토글하지 않아, 무엇을 눌러도
                //  T1이 선택된 것처럼 보였다. 색을 덧칠하는 대신 그 그림을 켜고 끈다.
                SetTabFocus(tabs[i], on);

                tabs[i].DOKill();
                tabs[i].localScale = Vector3.one * (on ? 1.06f : 1f);

                // 라벨도 같이 — 그림만 바뀌면 글자가 뒤처져 보인다.
                foreach (var txt in tabs[i].GetComponentsInChildren<TMPro.TMP_Text>(true))
                    txt.color = on ? Color.white : new Color(0.62f, 0.62f, 0.62f, 1f);
            }
        }
    }

    /// <summary>탭의 선택 상태 그림(shop_tab_focus / UIMask)을 켜고 끈다.
    /// 이름이 프리팹마다 제각각(Img_WeaponT1Btn_11, Mask_WeaponT1Btn_11 …)이라
    /// **스프라이트 이름**으로 찾는다 — 이름 규칙에 기대면 부위 하나만 어긋나도 조용히 깨진다.</summary>
    static void SetTabFocus(RectTransform tab, bool on)
    {
        foreach (var img in tab.GetComponentsInChildren<Image>(true))
        {
            string sp = img.sprite != null ? img.sprite.name : "";
            if (sp == "shop_tab_focus" || sp == "UIMask")
                img.gameObject.SetActive(on);
        }
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

    // ── 목록 배치 규칙 (프리팹 저작 좌표에서 역산) ──
    //  첫 칸 (18, -22), 가로 40 / 세로 45 간격, 2열. 뷰포트 폭 83이라 2열이 한계.
    const float GRID_X0 = 18f, GRID_Y0 = -22f, GRID_DX = 40f, GRID_DY = 45f;
    const int   GRID_COLS = 2;

    // T1/T2/T3 = 등급 2개씩 묶음 (Basic·Abnormal / Rare·Exceptional / Legendary·Mystic)
    static int TierOf(EquipGrade g) => Mathf.Clamp((int)g / 2, 0, 2);
    int _tier = 0;

    // 이 팝업이 만들어낸 칸들 — 갱신할 때마다 지우고 다시 만든다(저작 노드는 건드리지 않는다).
    readonly List<GameObject> _spawnedSlots = new List<GameObject>();

    /// <summary>
    /// 보유 장비를 부위별 목록에 배치.
    ///
    /// 예전엔 프리팹에 저작된 칸(s_slotNodes)에만 꽂았다 — 무기 4칸, 나머지 부위는 **1칸**.
    /// 그래서 투구를 10개 갖고 있어도 목록엔 1개만 보였고, 장비창이 비어 보였다.
    /// 이제 보유 수만큼 칸을 **복제해서** 채우고, ScrollRect content 높이도 함께 늘린다.
    /// T1/T2/T3 탭은 아예 구현이 없어 눌러도 아무 일이 없었다 — 등급 대역으로 필터링한다.
    /// </summary>
    public override void RefreshUI()
    {
        if (Managers.Game == null || _slotViews.Count == 0) return;

        // 이전 복제분 정리
        foreach (var go in _spawnedSlots)
            if (go != null) Destroy(go);
        _spawnedSlots.Clear();

        // 부위별 목록 — **미보유 장비도 넣는다**.
        //  보유한 것만 보여주면 60종 중 6종만 존재하는 것처럼 보이고, 무엇을 만들 수 있는지도 알 수 없다.
        //  미보유는 회색 + 자물쇠로 표시하고, 눌러서 상세창의 [제작]으로 들어간다.
        var bySlot = new Dictionary<int, List<Equipment>>();
        var ownedIds = new HashSet<int>();
        foreach (var e in Managers.Game.OwnedEquipments) ownedIds.Add(e.DataId);

        foreach (var kv in Managers.Data.EquipmentDic)
        {
            var d = kv.Value;
            if (d == null || TierOf(d.Grade) != _tier) continue;

            // 보유분은 실제 인스턴스를(강화도/내구도가 있다), 미보유분은 표시용 임시 인스턴스를 쓴다.
            Equipment e = ownedIds.Contains(kv.Key)
                ? Managers.Game.OwnedEquipments.Find(x => x.DataId == kv.Key)
                : new Equipment(kv.Key);

            int key = (int)d.SlotType;
            if (bySlot.TryGetValue(key, out var list) == false)
                bySlot[key] = list = new List<Equipment>();
            list.Add(e);
        }
        foreach (var list in bySlot.Values)
            list.Sort((a, b) => a.DataId.CompareTo(b.DataId));

        // 부위별 **대표 칸**(저작 노드 중 첫 번째)을 템플릿으로 삼는다.
        var template = new Dictionary<int, SlotView>();
        foreach (var v in _slotViews)
            if (v.Slot >= 0 && !template.ContainsKey(v.Slot)) template[v.Slot] = v;

        // 템플릿이 아닌 저작 칸(W01/W02/W44 등)은 숨긴다 — 복제분이 자리를 대신한다.
        foreach (var v in _slotViews)
            if (v.Slot < 0 || template[v.Slot] != v) v.Btn.SetActive(false);

        foreach (var kv in template)
        {
            int slot = kv.Key;
            SlotView head = kv.Value;
            bySlot.TryGetValue(slot, out var list);
            int count = list?.Count ?? 0;

            head.Btn.SetActive(true);
            Place(head.Btn, 0);
            ApplySlot(head, count > 0 ? list[0] : null);

            for (int i = 1; i < count; i++)
            {
                GameObject clone = Instantiate(head.Btn, head.Btn.transform.parent);
                clone.name = $"{head.Btn.name}_gen{i}";
                clone.SetActive(true);
                Place(clone, i);
                _spawnedSlots.Add(clone);

                // 복제본은 이름이 같은 자식 구조를 그대로 갖고 있으므로 같은 규칙으로 다시 찾는다.
                var view = CloneView(head, clone);
                SlotView captured = view;
                clone.BindEvent(() => OnClickSlot(captured));
                ApplySlot(view, list[i]);
            }

            ResizeContent(head.Btn.transform.parent as RectTransform, count);
        }
    }

    static void Place(GameObject go, int index)
    {
        var rt = go.transform as RectTransform;
        if (rt == null) return;
        rt.anchoredPosition = new Vector2(
            GRID_X0 + (index % GRID_COLS) * GRID_DX,
            GRID_Y0 - (index / GRID_COLS) * GRID_DY);
    }

    // 칸이 늘어난 만큼 ScrollRect content를 키워야 아래쪽 칸에 스크롤이 닿는다.
    //
    //  ⚠️ content가 stretch 앵커 + pivot 0.5로 저작돼 있다. 이 상태에서 높이만 늘리면
    //     **위아래로 똑같이** 커져서 윗변이 마스크 밖으로 올라가고 첫 행이 잘린다
    //     (실제로 그래서 4칸 중 2행만 보였다 — 보이던 게 1행이 아니라 2행이었다).
    //     늘린 만큼 절반을 아래로 내려 **윗변을 제자리에 고정**한다.
    //  재갱신 때 누적되지 않도록 저작 원본값을 기억해 두고 매번 원본에서 계산한다.
    readonly Dictionary<RectTransform, (Vector2 size, Vector2 pos)> _contentBase = new();

    void ResizeContent(RectTransform content, int count)
    {
        if (content == null) return;
        if (!_contentBase.TryGetValue(content, out var basis))
            _contentBase[content] = basis = (content.sizeDelta, content.anchoredPosition);

        int rows = Mathf.Max(1, Mathf.CeilToInt(count / (float)GRID_COLS));
        float need = -GRID_Y0 + rows * GRID_DY;
        float grow = Mathf.Max(0f, need - basis.size.y);

        content.sizeDelta        = new Vector2(basis.size.x, basis.size.y + grow);
        content.anchoredPosition = new Vector2(basis.pos.x, basis.pos.y - grow * 0.5f);
    }

    SlotView CloneView(SlotView src, GameObject clone) => new SlotView
    {
        Slot    = src.Slot,
        Btn     = clone,
        Icon    = FindLike(src.Icon,    clone),
        LvTxt   = FindLike(src.LvTxt,   clone),
        LowDuri = FindLike(src.LowDuri, clone),
        Dim     = FindLike(src.Dim,     clone),
        Badge   = FindLike(src.Badge,   clone),
    };

    static T FindLike<T>(T src, GameObject clone) where T : Component
        => src == null ? null : Util.FindChild<T>(clone, src.gameObject.name, true);
    static GameObject FindLike(GameObject src, GameObject clone)
        => src == null ? null : Util.FindChild(clone, src.name, true);

    // 슬롯 하나 갱신: 아이콘/강화수치/LowDuri/E뱃지/빈 슬롯 dim.
    void ApplySlot(SlotView view, Equipment equip)
    {
        view.Equip = equip;
        var data = equip?.EquipmentData;
        bool has = equip != null && data != null;

        var btn = view.Btn.GetComponent<Button>();
        if (btn != null) btn.interactable = has;

        // 미보유는 회색으로 — 있다는 건 보여주되 가진 것과 구분한다.
        bool owned = has && Managers.Game.HasEquipment(equip.DataId);

        // 장비 아이콘 (EquipmentData.SpriteName)
        if (view.Icon != null)
        {
            view.Icon.gameObject.SetActive(has);
            if (has && string.IsNullOrEmpty(data.SpriteName) == false)
            {
                var sp = Managers.Resource.Load<Sprite>(data.SpriteName);
                if (sp != null)
                {
                    // 이 아이콘에도 EquipIcon Animator가 붙어 있어 클립이 sprite와 color를
                    //  **매 프레임 덮어쓴다**. 끄지 않으면 모든 칸이 같은 그림으로 보이고
                    //  미보유 회색 처리도 무효가 된다(상세 팝업에서 겪은 것과 같은 함정).
                    var animator = view.Icon.GetComponent<Animator>();
                    if (animator != null) animator.enabled = false;

                    view.Icon.sprite = sp;
                    view.Icon.preserveAspect = true;
                }
            }
            view.Icon.color = owned ? Color.white : new Color(0.32f, 0.32f, 0.32f, 1f);
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

        // 'E' 장착 뱃지 — 보유분만
        if (view.Badge != null) view.Badge.SetActive(owned && equip.IsEquipped);

        // 빈 슬롯 dim (미보유는 아이콘 회색으로 구분하므로 dim은 빈 칸에만)
        if (view.Dim != null) view.Dim.SetActive(has == false);

        // 내구도 경고는 보유분에만 의미가 있다
        if (view.LowDuri != null && !owned) view.LowDuri.SetActive(false);
        if (view.LvTxt != null && !owned) view.LvTxt.gameObject.SetActive(false);
    }

    // 슬롯 클릭 → 장비 상세 팝업
    void OnClickSlot(SlotView view)
    {
        if (view.Equip == null) return;
        Managers.UI.ShowPopupUI<UI_EquipDetailPopup>().SetInfo(view.Equip);
    }

    #endregion
}
