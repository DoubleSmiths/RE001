using Data;
using DG.Tweening;
using UnityEngine;

public class UI_ImportantSlot : UI_Base
{
    #region Enum

    enum Images
    {
        GameObject,
        Frame,
        Img,
    }

    enum Texts
    {
        AmountTxt,
        ImportantSlotNewTxt,
    }

    enum Buttons
    {
        UI_ImportantSlot,
    }

    enum GameObjects
    {
    }

    #endregion

    private void Awake() { Init(); }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Bind
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        #endregion

        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        base.OnDestroy();
    }

    // enum Texts: AmountTxt(동적 숫자) / ImportantSlotNewTxt(NEW 라벨)
    void RefreshTexts()
    {
        var amount = GetText((int)Texts.AmountTxt);
        if (amount != null) ApplyFont(amount, Define.FontType.UI);
        var newTxt = GetText((int)Texts.ImportantSlotNewTxt);
        if (newTxt != null)
        {
            ApplyFont(newTxt, Define.FontType.UI);
            newTxt.text = Util.Localize(Define.LocalizeKey.ImportantSlotNewTxt);
        }
    }

    public void SetInfo(InventoryItem inventoryItem = null, ItemData itemData = null)
    {
        bool hasItem = inventoryItem != null && itemData != null;

        // 아이템 아이콘
        Sprite sprite = hasItem ? Managers.Resource.Load<Sprite>(itemData.SpriteName) : null;
        GetImage((int)Images.Img).sprite = sprite;
        GetImage((int)Images.Img).gameObject.SetActive(sprite != null);

        // 개수 (2개 이상일 때만 표시)
        bool showCount = hasItem && inventoryItem.Count > 1;
        GetText((int)Texts.AmountTxt).text = showCount ? inventoryItem.Count.ToString() : "";
        GetText((int)Texts.AmountTxt).gameObject.SetActive(showCount);

        // "New" 표시
        GetText((int)Texts.ImportantSlotNewTxt).gameObject.SetActive(hasItem && inventoryItem.IsNew);

        GetImage((int)Images.GameObject).gameObject.BindEvent(() => { OnClickGameObject(inventoryItem); });

        if (hasItem)
            GetImage((int)Images.Frame).color = Util.HexToColor("FF00C0"); // 프레임 색상 (예시: 마젠타색?)
    }

    // 슬롯 등장 애니메이션 (slide10: Scale FROM x0,y1,z0 / 0.15s / OutQuad)
    public void PlayOpenAnim(float delay = 0f)
    {
        transform.DOKill();
        transform.localScale = new Vector3(0f, 1f, 0f);
        transform.DOScale(Vector3.one, 0.15f)
                 .SetEase(Ease.OutQuad)
                 .SetDelay(delay)
                 .SetUpdate(true);
    }

    void OnClickGameObject(InventoryItem inventoryItem)
    {
        Managers.Game.ClearItemNew(inventoryItem.DataId);
        GetText((int)Texts.ImportantSlotNewTxt).gameObject.SetActive(false);

        Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(inventoryItem.DataId);
    }
}
