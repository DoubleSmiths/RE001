using Data;
using DG.Tweening;
using Unity.VisualScripting;
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
        if (amount != null) ApplyFont(amount, Define.FontType.UI);
        /* 성우
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
