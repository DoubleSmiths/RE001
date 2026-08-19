using Data;
using DG.Tweening;
using UnityEngine;
using static Define;

public class UI_InvSlot : UI_Base
{
    #region Enum

    enum Images
    {
        GameObject,
        Frame,
        Glow,
        Img,
        BlockImg,
    }

    enum Texts
    {
        AmountTxt,
        InvSlotNewTxt,
    }

    enum Buttons
    {
        //UI_InvSlot,
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

    // enum Texts: AmountTxt(동적 숫자) / InvSlotNewTxt(NEW 라벨)
    void RefreshTexts()
    {
        var amount = GetText((int)Texts.AmountTxt);
        /* 성우
        if (amount != null) ApplyFont(amount, Define.FontType.UI);
        var newTxt = GetText((int)Texts.InvSlotNewTxt);
        if (newTxt != null)
        {
            ApplyFont(newTxt, Define.FontType.UI);
            newTxt.text = Util.Localize(Define.LocalizeKey.InvSlotNewTxt);
        } */
    }

    public void SetInfo(InventoryItem inventoryItem = null, ItemData itemData = null)
    {
        bool hasItem = inventoryItem != null && itemData != null;
        GetImage((int)Images.BlockImg).gameObject.SetActive(false);
        //성우 추가
        GetImage((int)Images.Frame).gameObject.SetActive(hasItem);
        
        // 아이템 아이콘
        Sprite sprite = hasItem ? Managers.Resource.Load<Sprite>(itemData.SpriteName) : null;
        GetImage((int)Images.Img).sprite = sprite;
        GetImage((int)Images.Img).gameObject.SetActive(sprite != null);

        // 개수 (2개 이상일 때만 표시)
        bool showCount = hasItem && inventoryItem.Count > 1;
        GetText((int)Texts.AmountTxt).text = showCount ? inventoryItem.Count.ToString() : "";
        GetText((int)Texts.AmountTxt).gameObject.SetActive(showCount);

        // 등급 글로우 (잠긴 슬롯 기본값 0F0F0F, SetOpenBlock에서 열린 슬롯 색상으로 덮어씀)
        var glowImage = GetImage((int)Images.Glow);
        glowImage.gameObject.SetActive(true);
        glowImage.color = hasItem ? Util.GetGradeColor(itemData.Grade) : new Color32(15, 15, 15, 255);

        // 새 아이템 표시
        GetText((int)Texts.InvSlotNewTxt).gameObject.SetActive(hasItem && inventoryItem.IsNew);

        if (hasItem)
            GetImage((int)Images.GameObject).gameObject.BindEvent(() => { OnClickGameObject(inventoryItem); });
    }

    /// <summary>상점용 표시 — 개수 자리에 **가격**을 쓴다.
    /// 슬롯 프리팹을 하나 더 만들지 않는 대신, 클릭 동작도 여기서 갈아끼운다
    /// (기본 SetInfo는 아이템 상세창을 열지만 상점에서는 사고파는 게 목적이다).</summary>
    public void SetShopInfo(ItemData itemData, int price, bool affordable, System.Action onClick)
    {
        GetImage((int)Images.BlockImg).gameObject.SetActive(false);
        GetImage((int)Images.Frame).gameObject.SetActive(true);

        Sprite sprite = itemData != null ? Managers.Resource.Load<Sprite>(itemData.SpriteName) : null;
        GetImage((int)Images.Img).sprite = sprite;
        GetImage((int)Images.Img).gameObject.SetActive(sprite != null);
        // 살 수 없으면 아이콘을 죽여 목록에서 바로 구분되게 한다
        GetImage((int)Images.Img).color = affordable ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);

        var amount = GetText((int)Texts.AmountTxt);
        amount.gameObject.SetActive(true);
        amount.text = ShortPrice(price);
        amount.color = affordable ? Color.white : Util.HexToColor("FF5555");

        var glowImage = GetImage((int)Images.Glow);
        glowImage.gameObject.SetActive(true);
        glowImage.color = itemData != null ? Util.GetGradeColor(itemData.Grade) : new Color32(15, 15, 15, 255);

        GetText((int)Texts.InvSlotNewTxt).gameObject.SetActive(false);

        GetImage((int)Images.GameObject).gameObject.BindEvent(() =>
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(new Vector3(-0.25f, 0.15f, 1f), 0.3f, 10, 1f)
                     .SetEase(Ease.OutQuad)
                     .SetUpdate(true);
            onClick?.Invoke();
        });
    }

    // 수량 칸은 원래 한두 자리용이라 30,000 같은 값이 "0,000"으로 잘렸다.
    //  1000 이상은 k로 줄인다 — 2400 -> 2.4k, 9000 -> 9k, 30000 -> 30k.
    static string ShortPrice(int price)
    {
        if (price < 1000) return price.ToString();
        float k = price / 1000f;
        return k < 10f && price % 1000 != 0 ? $"{k:0.#}k" : $"{Mathf.RoundToInt(k)}k";
    }

    public void SetOpenBlock()
    {
        //성우 추가
        GetImage((int)Images.Frame).gameObject.SetActive(false);

        GetImage((int)Images.BlockImg).gameObject.SetActive(false);
        // 열린 빈 슬롯: Glow FFFFFF alpha 40
        GetImage((int)Images.Glow).color = new Color32(255, 255, 255, 40);
    }

    public void SetBlock()
    {
        //성우 추가
        GetImage((int)Images.Frame).gameObject.SetActive(false);

        GetImage((int)Images.BlockImg).gameObject.SetActive(true);
        // 막힌 빈 슬롯: Glow 0F0F0F alpha 255
        GetImage((int)Images.Glow).color = Util.HexToColor("0F0F0F");
    }

    void OnClickGameObject(InventoryItem inventoryItem)
    {
        // anim
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(new Vector3(-0.25f, 0.15f, 1f), 0.3f, 10, 1f)
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true);

        Managers.Game.ClearItemNew(inventoryItem.DataId);
        GetText((int)Texts.InvSlotNewTxt).gameObject.SetActive(false);

        Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(inventoryItem.DataId);
    }
}
