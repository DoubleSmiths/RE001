using System.Collections.Generic;
using Data;
using UnityEngine;
using static Define;

// 마을 상점 — [구매] / [되팔기].
//
//  왜 있는가: 골드 소비처가 강화(GameManager)와 주문제작 둘뿐이라, 사냥으로 골드는 계속
//  쌓이는데 쓸 데가 없었다. 여기서 재료와 강화 주문서를 사고, 남는 재료를 되판다.
//
//  **장비는 팔지 않는다** — 장비 획득은 대장간 제작으로 몰아야 재료가 쌓이기만 하는
//  문제가 풀리고, 두 건물이 서로 역할을 먹지 않는다.
//
//  프리팹은 UI_InventoryPopup을 복제해 만들었다(스크롤 + 슬롯 격자가 그대로 필요했다).
//  슬롯도 UI_InvSlot을 그대로 쓰되 개수 자리에 가격을 찍는다(UI_InvSlot.SetShopInfo).
public class UI_ShopPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        ShopPopupFrame,
        Xbutton,
    }

    enum Texts
    {
        title,
    }

    enum Buttons
    {
        ShopPopupBackground,
    }

    enum GameObjects
    {
        Content,
    }

    #endregion

    bool _sellMode;
    readonly List<UI_InvSlot> _slots = new List<UI_InvSlot>();

    void Awake() => Init();

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.ShopPopupBackground)?.gameObject.BindEvent(OnClickClose);
        GetImage((int)Images.Xbutton)?.gameObject.BindEvent(OnClickClose);

        // 골드가 바뀌면 살 수 있는지 표시가 달라진다
        if (Managers.Game != null) Managers.Game.OnResourceChanged += RefreshUI;

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnResourceChanged -= RefreshUI;
        base.OnDestroy();
    }

    public void SetInfo(bool sellMode)
    {
        _sellMode = sellMode;
        RefreshUI();
    }

    public override void RefreshUI()
    {
        var title = GetText((int)Texts.title);
        if (title != null)
        {
            title.text = Util.Localize(_sellMode ? LocalizeKey.ShopSellTitleText : LocalizeKey.ShopBuyTitleText);
            ApplyFont(title, FontType.UI);
        }

        var content = GetObject((int)GameObjects.Content);
        if (content == null) return;

        ClearChildren(content.transform);
        _slots.Clear();

        List<ShopItemData> stock = _sellMode
            ? Managers.Game.GetSellableStock()
            : Managers.Game.GetShopStock();

        if (stock.Count == 0)
        {
            // 되팔 게 하나도 없으면 빈 화면만 남아 고장으로 보인다 — 이유를 말해 준다.
            Managers.UI.ShowToast(Util.Localize(
                _sellMode ? LocalizeKey.ShopNothingToSellText : LocalizeKey.ForgeNoCraftableText));
            return;
        }

        foreach (ShopItemData s in stock)
        {
            if (!Managers.Data.ItemDic.TryGetValue(s.ItemId, out ItemData item)) continue;

            UI_InvSlot slot = Managers.UI.MakeSubItem<UI_InvSlot>(content.transform, pooling: false);
            ShopItemData captured = s;

            int price = _sellMode ? s.SellPrice : s.Price;
            bool enabled = _sellMode
                ? Managers.Game.GetItemCount(s.ItemId) > 0
                : string.IsNullOrEmpty(Managers.Game.GetBuyBlockReason(s));

            slot.SetShopInfo(item, price, enabled, () => OnClickSlot(captured));
            _slots.Add(slot);
        }
    }

    void OnClickSlot(ShopItemData shop)
    {
        if (_sellMode)
        {
            if (!Managers.Game.SellShopItem(shop))
            {
                Managers.UI.ShowToast(Util.Localize(LocalizeKey.ShopNothingToSellText));
                return;
            }
            Managers.Sound?.PlaySFX("SFX_Upgrade");
            Managers.UI.ShowToast(Util.Localize(LocalizeKey.ShopSoldText));
        }
        else
        {
            string reason = Managers.Game.GetBuyBlockReason(shop);
            if (!string.IsNullOrEmpty(reason))
            {
                Managers.UI.ShowToast(reason);
                return;
            }
            if (!Managers.Game.BuyShopItem(shop)) return;

            Managers.Sound?.PlaySFX("SFX_Upgrade");
            Managers.UI.ShowToast(Util.Localize(LocalizeKey.ShopPurchasedText));
        }

        RefreshUI();
    }

    void OnClickClose() => Managers.UI.ClosePopupUI(this);

    // 목록을 다시 그릴 때 이전 슬롯을 치운다. Object.Destroy가 아니라 ResourceManager 경유여야
    //  풀에 반환된다(UI_InventoryPopup과 같은 방식).
    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Managers.Resource.Destroy(parent.GetChild(i).gameObject);
    }
}
