using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// 상점 구매/판매 확인창.
//
//  슬롯을 누르면 곧바로 사고팔던 것을 막는다 — 한 번의 오클릭으로 골드가 나가거나
//  재료가 사라지면 되돌릴 방법이 없다. 몇 개를 사고팔지 슬라이더로 정한 뒤 [확인]을 눌러야
//  실제 거래가 일어난다.
//
//  최대 수량은 상황이 정한다:
//    구매 → 지금 골드로 살 수 있는 만큼 (가진 돈을 넘겨 담을 수 없다)
//    판매 → 지금 가진 수량
public class UI_ShopConfirmPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        ShopConfirmIcon,
    }

    enum Texts
    {
        ShopConfirmTitleText,
        ShopConfirmNameText,
        ShopConfirmCountText,
        ShopConfirmTotalText,
        ShopConfirmOkText,
        ShopConfirmCancelText,
    }

    enum Buttons
    {
        ShopConfirmOkButton,
        ShopConfirmCancelButton,
    }

    enum GameObjects
    {
        ShopConfirmSlider,
    }

    #endregion

    Data.ShopItemData _shop;
    bool _sellMode;
    int _max = 1;
    int _count = 1;
    Slider _slider;
    Action _onDone;

    void Awake() => Init();

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _slider = GetObject((int)GameObjects.ShopConfirmSlider)?.GetComponent<Slider>();
        if (_slider != null)
        {
            _slider.wholeNumbers = true;
            _slider.onValueChanged.RemoveAllListeners();
            _slider.onValueChanged.AddListener(OnSliderChanged);
        }

        GetButton((int)Buttons.ShopConfirmOkButton).gameObject.BindEvent(OnClickOk);
        GetButton((int)Buttons.ShopConfirmCancelButton).gameObject.BindEvent(OnClickCancel);

        return true;
    }

    /// <summary>onDone: 거래가 실제로 일어났을 때만 호출(목록 갱신용). 취소면 안 부른다.</summary>
    public void SetInfo(Data.ShopItemData shop, bool sellMode, Action onDone = null)
    {
        _shop = shop;
        _sellMode = sellMode;
        _onDone = onDone;

        _max = Mathf.Max(1, MaxAffordable());
        _count = 1;

        if (_slider != null)
        {
            _slider.minValue = 1;
            _slider.maxValue = _max;
            _slider.value = 1;
            // 살 수 있는 게 한 개뿐이면 슬라이더를 움직일 이유가 없다
            _slider.interactable = _max > 1;
        }

        RefreshUI();
    }

    /// <summary>지금 상황에서 최대 몇 개까지 가능한가.</summary>
    int MaxAffordable()
    {
        if (_shop == null) return 1;

        if (_sellMode)
            return Managers.Game.GetItemCount(_shop.ItemId);

        int price = Managers.Game.ShopPrice(_shop);   // 상점 주인 호감도 할인가(ADR0036)
        if (price <= 0) return 1;
        return Managers.Game.Gold / price;
    }

    void OnSliderChanged(float v)
    {
        _count = Mathf.Clamp(Mathf.RoundToInt(v), 1, _max);
        RefreshUI();
    }

    public override void RefreshUI()
    {
        if (_shop == null) return;

        Managers.Data.ItemDic.TryGetValue(_shop.ItemId, out Data.ItemData item);

        SetTextLocalized(Texts.ShopConfirmTitleText,
            _sellMode ? LocalizeKey.ShopSellBtnText : LocalizeKey.ShopBuyBtnText);

        var nameTxt = GetText((int)Texts.ShopConfirmNameText);
        if (nameTxt != null)
        {
            nameTxt.text = item != null ? Util.Localize(item.Name) : _shop.ItemId.ToString();
            ApplyFont(nameTxt, FontType.UI);
        }

        var icon = GetImage((int)Images.ShopConfirmIcon);
        if (icon != null && item != null)
        {
            Sprite sp = Managers.Resource.Load<Sprite>(item.SpriteName);
            icon.sprite = sp;
            icon.gameObject.SetActive(sp != null);
        }

        int unit = _sellMode ? _shop.SellPrice : Managers.Game.ShopPrice(_shop);

        var countTxt = GetText((int)Texts.ShopConfirmCountText);
        if (countTxt != null)
        {
            countTxt.text = $"{_count} / {_max}";
            ApplyFont(countTxt, FontType.UI);
        }

        var totalTxt = GetText((int)Texts.ShopConfirmTotalText);
        if (totalTxt != null)
        {
            long total = (long)unit * _count;
            // 파는 쪽은 돈이 들어오고 사는 쪽은 나간다 — 부호를 색으로 구분한다.
            string color = _sellMode ? "#2DFF00" : "#FFE300";
            totalTxt.text = $"<color={color}>{total:N0}</color> G";
            ApplyFont(totalTxt, FontType.UI);
        }

        SetTextLocalized(Texts.ShopConfirmOkText, LocalizeKey.ConfirmBtnText);
        SetTextLocalized(Texts.ShopConfirmCancelText, LocalizeKey.CancelBtnText);
    }

    void OnClickOk()
    {
        if (_shop == null) { OnClickCancel(); return; }

        bool ok = _sellMode
            ? Managers.Game.SellShopItem(_shop, _count)
            : Managers.Game.BuyShopItem(_shop, _count);

        if (!ok)
        {
            // 창을 띄운 사이 골드나 수량이 바뀌었을 수 있다 — 왜 막혔는지 말해 주고 닫지 않는다.
            string reason = _sellMode
                ? Util.Localize(LocalizeKey.ShopNothingToSellText)
                : Managers.Game.GetBuyBlockReason(_shop, _count);
            Managers.UI.ShowToast(string.IsNullOrEmpty(reason)
                ? Util.Localize(LocalizeKey.ShopNotEnoughGoldText) : reason);
            SetInfo(_shop, _sellMode, _onDone);   // 최대치를 다시 계산
            return;
        }

        Managers.Sound?.PlaySFX("SFX_Upgrade");
        Managers.UI.ShowToast(Util.Localize(
            _sellMode ? LocalizeKey.ShopSoldText : LocalizeKey.ShopPurchasedText));

        _onDone?.Invoke();
        Managers.UI.ClosePopupUI(this);
    }

    // 다른 팝업들과 같은 방식 — 로컬라이즈 키로 넣고 UI 폰트를 입힌다.
    void SetTextLocalized(Texts texts, string locKey)
    {
        var t = GetText((int)texts);
        if (t == null) return;
        t.text = Util.Localize(locKey);
        ApplyFont(t, FontType.UI);
    }

    void OnClickCancel() => Managers.UI.ClosePopupUI(this);
}
