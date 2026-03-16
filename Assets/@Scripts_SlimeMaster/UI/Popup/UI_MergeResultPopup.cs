using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Text.RegularExpressions;
using static Define;
using UnityEngine.UI;
using System;

public class UI_MergeResultPopup : UI_Popup
{
    Equipment _beforeEquipment;
    Equipment _afterEquipment;

    #region UI 기능 리스트
    // 정보 갱신
    // EquipmentGradeBackgroundImage : 합성 할 장비 등급의 테두리 (색상 변경)
    //      - 일반(Common) : #AC9B83
    //      - 고급(Uncommon)  : #73EC4E
    //      - 희귀(Rare) : #0F84FF
    //      - 유일(Epic) : #B740EA
    //      - 전설(Legendary) : #F19B02
    //      - 신화(Myth) : #FC2302
    // EquipmentEnforceBackgroundImage : 유일 +1 등급부터 활성화되고 등급에 따라 이미지 색깔 변경
    //      - 유일(Epic) : #9F37F2
    //      - 전설(Legendary) : #F67B09
    //      - 신화(Myth) : #F1331A
    // EquipmentNameValueText : 장비의 이름
    // EnforceValueText : 유일 +1 등의 등급 벨류
    // EquipmentImage : 장비의 아이콘
    // EquipmentLevelValueText : 장비의 현재 레벨

    // OptionResult (옵션 결과)
    //      BeforeLevelValueText : 합성 전 장비 최대 레벨
    //      AfterLevelValueText : 합성 후 장비 최대 레벨
    //      ImprovATKObject
    //      BeforeATKValueText : 합성 전 장비 공격력
    //      AfterATKValueText : 합성 후 장비 공격력


    // OptionResult (옵션 결과)
    //      BeforeLevelValueText : 합성 전 장비 최대 레벨
    //      AfterLevelValueText : 합성 후 장비 최대 레벨
    //      ImprovATKObject : 장비의 공격력 옵션이 있을때 활성화
    //      BeforeATKValueText : 합성 전 장비 공격력
    //      AfterATKValueText : 합성 후 장비 공격력
    //      ImprovHPObject : 장비의 체력 옵션이 있을때 활성화
    //      BeforeHPValueText : 합성 전 장비 체력
    //      AfterHPValueText : 합성 전 장비 체력
    //      ImprovOptionValueText : 합성 후 추가 옵션

    // 로컬라이징
    // BackgroundText : 탭하여 닫기
    // MergeResultCommentText : 합성 결과
    // ImprovLevelText : 최대 레벨
    // ImprovATKText : 공격력
    // ImprovHPText : 체력
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        ImprovATKObject,
        ImprovHPObject,

    }
    enum Buttons
    {
        BackgroundButton,
        //ImprovHPObject,
    }

    enum Texts
    {
        BackgroundText,
        MergeResultCommentText,
        EquipmentNameValueText,
        EquipmentLevelValueText,
        EnforceValueText,
        ImprovLevelText,
        BeforeLevelValueText,
        AfterLevelValueText,
        ImprovATKText,
        BeforeATKValueText,
        AfterATKValueText,
        ImprovHPText,
        BeforeHPValueText,
        AfterHPValueText,
        ImprovOptionValueText,
        EquipmentGradeValueText,
    }
    enum Images
    {
        EquipmentGradeBackgroundImage,
        EquipmentImage,
        EquipmentEnforceBackgroundImage,
        EquipmentTypeBackgroundImage,
        EquipmentTypeImage,
    }
    #endregion

    private void Awake()
    {
        Init();
    }
    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
        Managers.Sound.Play(Define.Sound.Effect, "Result_CommonMerge");

    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);

        // 테스트용
#if UNITY_EDITOR
        //TextBindTest();
#endif
        #endregion

        Refresh();
        return true;
    }

    Action _closeAction;
    public void SetInfo(Equipment beforeEquipment, Equipment afterEquipment, Action callback = null)
    {
        _beforeEquipment = beforeEquipment;
        _afterEquipment = afterEquipment;
        _closeAction = callback;
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;
        if (_beforeEquipment == null)
            return;
        if (_afterEquipment == null)
            return;

        #region 기본 정보
        // EquipmentImage : 장비의 아이콘
        Sprite spr = Managers.Resource.Load<Sprite>(_afterEquipment.EquipmentData.SpriteName);
        GetImage((int)Images.EquipmentTypeImage).sprite = Managers.Resource.Load<Sprite>($"{_afterEquipment.EquipmentData.EquipmentType}_Icon.sprite");

        GetImage((int)Images.EquipmentImage).sprite = spr;
        // 장비 이름
        GetText((int)Texts.EquipmentNameValueText).text = $"{_afterEquipment.EquipmentData.NameTextID}";
        // 장비 레벨
        GetText((int)Texts.EquipmentLevelValueText).text = $"Lv.{_beforeEquipment.Level}";
        #endregion

        #region 등급 대응 + 추가 옵션
        switch (_afterEquipment.EquipmentData.EquipmentGrade)
        {
            case EquipmentGrade.Uncommon:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Uncommon;
                GetText((int)Texts.EquipmentGradeValueText).text = "고급";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.UncommonNameColor;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.Uncommon;
                int uncommonGradeSkillId = _afterEquipment.EquipmentData.UncommonGradeSkill;
                GetText((int)Texts.ImprovOptionValueText).text = $"+ {Managers.Data.SupportSkillDic[uncommonGradeSkillId].Description}"; // 추가 옵션
                break;

            case EquipmentGrade.Rare:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Rare;
                GetText((int)Texts.EquipmentGradeValueText).text = "희귀";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.RareNameColor;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.Rare;

                int rareGradeSkillId = _afterEquipment.EquipmentData.RareGradeSkill;
                GetText((int)Texts.ImprovOptionValueText).text = $"+ {Managers.Data.SupportSkillDic[rareGradeSkillId].Description}"; // 추가 옵션
                break;

            case EquipmentGrade.Epic:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetText((int)Texts.EquipmentGradeValueText).text = "에픽";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.EpicNameColor;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.EpicBg;

                int epicGradeSkillId = _afterEquipment.EquipmentData.EpicGradeSkill;
                GetText((int)Texts.ImprovOptionValueText).text = $"+ {Managers.Data.SupportSkillDic[epicGradeSkillId].Description}"; // 추가 옵션
                break;

            case EquipmentGrade.Epic1:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.EpicBg;

                GetText((int)Texts.EquipmentGradeValueText).text = "에픽 1";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.EpicNameColor;
                break;

            case EquipmentGrade.Epic2:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Epic;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.EpicBg;
                GetText((int)Texts.EquipmentGradeValueText).text = "에픽 2";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.EpicNameColor;
                break;

            case EquipmentGrade.Legendary:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetText((int)Texts.EquipmentGradeValueText).text = "전설";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.LegendaryNameColor;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                int legendaryGradeSkillId = _afterEquipment.EquipmentData.LegendaryGradeSkill;
                GetText((int)Texts.ImprovOptionValueText).text = $"+ {Managers.Data.SupportSkillDic[legendaryGradeSkillId].Description}"; // 추가 옵션
                break;

            case EquipmentGrade.Legendary1:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetText((int)Texts.EquipmentGradeValueText).text = "전설 1";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.LegendaryNameColor;
                break;

            case EquipmentGrade.Legendary2:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetText((int)Texts.EquipmentGradeValueText).text = "전설 2";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.LegendaryNameColor;
                break;

            case EquipmentGrade.Legendary3:
                GetImage((int)Images.EquipmentGradeBackgroundImage).color = EquipmentUIColors.Legendary;
                GetImage((int)Images.EquipmentEnforceBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetImage((int)Images.EquipmentTypeBackgroundImage).color = EquipmentUIColors.LegendaryBg;
                GetText((int)Texts.EquipmentGradeValueText).text = "전설 3";
                GetText((int)Texts.EquipmentGradeValueText).color = EquipmentUIColors.LegendaryNameColor;
                break;

            default:
                break;
        }
        #endregion

        #region 유일 +1 등의 등급 벨류
        string gradeName = _afterEquipment.EquipmentData.EquipmentGrade.ToString();
        int num = 0;

        // Epic1 -> 1 리턴 Epic2 ->2 리턴 Common처럼 숫자가 없으면 0 리턴
        Match match = Regex.Match(gradeName, @"\d+$");
        if (match.Success)
            num = int.Parse(match.Value);

        if (num == 0)
        {
            GetText((int)Texts.EnforceValueText).text = "";
            GetImage((int)Images.EquipmentEnforceBackgroundImage).gameObject.SetActive(false);
        }
        else
        {
            GetText((int)Texts.EnforceValueText).text = num.ToString();
            GetImage((int)Images.EquipmentEnforceBackgroundImage).gameObject.SetActive(true);
        }
        #endregion

        #region 옵션
        // 최대 레벨
        GetText((int)Texts.BeforeLevelValueText).text = $"{Managers.Data.EquipDataDic[_beforeEquipment.EquipmentData.DataId].MaxLevel}"; // 합성 전 공격력
        GetText((int)Texts.AfterLevelValueText).text = $"{Managers.Data.EquipDataDic[_beforeEquipment.EquipmentData.DataId].MaxLevel}"; // 합성 전 공격력

        // 능력치 상승
        if (_beforeEquipment.EquipmentData.AtkDmgBonus != 0) // 공격력이 0이 아니면 무기
        {
            // 공격력 옵션
            GetObject((int)GameObjects.ImprovATKObject).gameObject.SetActive(true);
            GetObject((int)GameObjects.ImprovHPObject).gameObject.SetActive(false);

            GetText((int)Texts.BeforeATKValueText).text = $"{Managers.Data.EquipDataDic[_beforeEquipment.EquipmentData.DataId].MaxLevel}"; // 합성 전 공격력
            GetText((int)Texts.AfterATKValueText).text = $"{Managers.Data.EquipDataDic[_afterEquipment.EquipmentData.DataId].MaxLevel}"; // 합성 후 공격력
        }
        else
        {
            // 체력 옵션
            GetObject((int)GameObjects.ImprovATKObject).gameObject.SetActive(false);
            GetObject((int)GameObjects.ImprovHPObject).gameObject.SetActive(true);

            GetText((int)Texts.BeforeHPValueText).text = Managers.Data.EquipDataDic[_beforeEquipment.EquipmentData.DataId].MaxLevel.ToString(); // 합성 전 체력
            GetText((int)Texts.AfterHPValueText).text = Managers.Data.EquipDataDic[_afterEquipment.EquipmentData.DataId].MaxLevel.ToString(); // 합성 후 체력 
        }
        #endregion
    }

    // 빈 곳 눌러 닫기 버튼
    void OnClickBackgroundButton()
    {
        Managers.Sound.PlayPopupClose();
        _closeAction?.Invoke();
        gameObject.SetActive(false);
    }
}
