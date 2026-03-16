using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using static Define;

public class UI_GachaRateItem : UI_Base
{
    enum Texts
    {
        EquipmentNameValueText,
        EquipmentReteValueText,
    }
    
    enum Images
    {
        BackgroundImage,
    }

    GachaRateData _gachaRateData;
    private void OnEnable()
    {
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindImage(typeof(Images));

        Refresh();
        return true;
    }

    public void SetInfo(GachaRateData gachaRateData)
    {
        
        _gachaRateData = gachaRateData;

        Refresh();
        transform.localScale = Vector3.one;

    }


    void Refresh()
    {
        if (_init == false)
            return;

        //string spriteName = Managers.Data.EquipDataDic[_gachaRateData.EquipmentID].SpriteName;
        //GetImage((int)Images.EquipmentImage).sprite = Managers.Resource.Load<Sprite>(spriteName);

        string weaponName = Managers.Data.EquipDataDic[_gachaRateData.EquipmentID].NameTextID;
        GetText((int)Texts.EquipmentNameValueText).text = weaponName;
        GetText((int)Texts.EquipmentReteValueText).text = _gachaRateData.GachaRate.ToString("P2");
        switch (_gachaRateData.EquipGrade)
        {
            case EquipmentGrade.Common:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Common;
                break;
            case EquipmentGrade.Uncommon:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Uncommon;
                break;
            case EquipmentGrade.Rare:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Rare;
                break;
            case EquipmentGrade.Epic:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Epic;
                break;
            default:
                break;
        }
    }
}
