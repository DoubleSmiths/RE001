using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// 슬라이드 53-3: 장비 팝업 (RE001_Share VS → C# 포팅).
// 구조: 7슬롯(Weapon/Helmet/Cloak/Armor/Glove/Boots/Ring) — 각 슬롯마다
//   Open{Slot}ListBtn(슬롯 리스트 열기) + {Slot}List(슬롯 패널) + T1/T2/T3 리스트.
// 본 1차 구현 = 팝업 열림/닫힘 + 슬롯 리스트 네비게이션(한 번에 한 슬롯 표시).
// 아이템 채우기/장착(EquipItem)/장착이펙트/상세·강화 팝업 연동은 후속.
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

        OpenAnimation();
        return true;
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
}
