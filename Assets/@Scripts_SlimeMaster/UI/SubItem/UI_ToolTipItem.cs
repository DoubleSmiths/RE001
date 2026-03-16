using UnityEngine;
using static Define;


public class UI_ToolTipItem : UI_Base
{
    #region Enum

    enum Images
    {
        TargetImage,
        BackgroundImage,
    }

    enum Buttons
    {
        CloseButton
    }

    enum Texts
    {
        TargetNameText,
        TargetDescriptionText
    }
    #endregion

    RectTransform m_Canvas;
    Camera m_UiCamera;

    private void OnEnable()
    {
        GetText((int)Texts.TargetNameText).gameObject.SetActive(false); // 기본 비활성화 상태
        GetText((int)Texts.TargetNameText).gameObject.SetActive(false); // 기본 비활성화 상태
    }

    private void Awake()
    {
        Init();

    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);

        #endregion

        Refresh();
        return true;
    }

    // 서포트 스킬 툴팁
    public void SetInfo(Data.SupportSkillData skillData, RectTransform targetPos, RectTransform parentsCanvas)
    {
        GetImage((int)Images.TargetImage).sprite = Managers.Resource.Load<Sprite>(skillData.IconLabel);
        GetText((int)Texts.TargetNameText).gameObject.SetActive(true);
        GetText((int)Texts.TargetNameText).text = skillData.Name;
        GetText((int)Texts.TargetDescriptionText).gameObject.SetActive(true);
        GetText((int)Texts.TargetDescriptionText).text = skillData.Description;
        // 등급에 따라 배경 색상 변경
        switch (skillData.SupportSkillGrade)
        {
            case SupportSkillGrade.Common:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Common;
                break;
            case SupportSkillGrade.Uncommon:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Uncommon;
                break;
            case SupportSkillGrade.Rare:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Rare;
                break;
            case SupportSkillGrade.Epic:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Epic;
                break;
            case SupportSkillGrade.Legend:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Legendary;
                break;
            default:
                break;
        }

        ToolTipPosSet(targetPos, parentsCanvas); // 위치 설정

        Refresh();
    }

    // 재료 툴팁
    public void SetInfo(Data.MaterialData materialData, RectTransform targetPos, RectTransform parentsCanvas)
    {
        GetImage((int)Images.TargetImage).sprite = Managers.Resource.Load<Sprite>(materialData.SpriteName);
        GetText((int)Texts.TargetNameText).gameObject.SetActive(true); 
        GetText((int)Texts.TargetNameText).text = materialData.NameTextID;
        GetText((int)Texts.TargetDescriptionText).gameObject.SetActive(true); 
        GetText((int)Texts.TargetDescriptionText).text = materialData.DescriptionTextID;
        // 등급에 따라 배경 색상 변경
        switch (materialData.MaterialGrade)
        {
            case MaterialGrade.Common:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Common;
                break;
            case MaterialGrade.Uncommon:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Uncommon;
                break;
            case MaterialGrade.Rare:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Rare;
                break;
            case MaterialGrade.Epic:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Epic;
                break;
            case MaterialGrade.Legendary:
                GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Legendary;
                break;
            default:
                break;
        }

        ToolTipPosSet(targetPos, parentsCanvas); // 위치 설정
        Refresh();
    }

    // 몬스터 툴팁
    public void SetInfo(Data.CreatureData creatureData, RectTransform targetPos, RectTransform parentsCanvas)
    {
        GetImage((int)Images.TargetImage).sprite = Managers.Resource.Load<Sprite>(creatureData.IconLabel);
        GetText((int)Texts.TargetDescriptionText).gameObject.SetActive(true);
        GetText((int)Texts.TargetDescriptionText).text = creatureData.DescriptionTextID;
        GetImage((int)Images.BackgroundImage).color = EquipmentUIColors.Common;

        ToolTipPosSet(targetPos, parentsCanvas); // 위치 설정
        Refresh();
    }

    void Refresh()
    {

    }

    void OnClickCloseButton()
    {
        Managers.Sound.PlayButtonClick();
        //자신 끄기
        Managers.Resource.Destroy(gameObject);
    }

    // 툴팁 위치 설정
    void ToolTipPosSet(RectTransform targetPos, RectTransform parentsCanvas)
    {
        // 기본 정렬
        gameObject.transform.position = targetPos.transform.position;

        // 세로 높이 설정
        float sizeY = targetPos.sizeDelta.y / 2;
        transform.localPosition += new Vector3(0f, sizeY);

        // 가로 높이 설정
        if (targetPos.transform.localPosition.x > 0) // 오른쪽
        {
            float canvasMaxX = parentsCanvas.sizeDelta.x / 2;
            float targetPosMaxX = transform.localPosition.x + transform.GetComponent<RectTransform>().sizeDelta.x / 2;
            if (canvasMaxX < targetPosMaxX)
            {
                float deltaX = targetPosMaxX - canvasMaxX;
                transform.localPosition = -new Vector3(deltaX+20, 0f) + transform.localPosition;
            }

        }
        else // 왼쪽
        {
            float canvasMinX = -parentsCanvas.sizeDelta.x / 2;
            float targetPosMinX = transform.localPosition.x - transform.GetComponent<RectTransform>().sizeDelta.x / 2;
            if (canvasMinX > targetPosMinX)
            {
                float deltaX = canvasMinX - targetPosMinX;
                transform.localPosition = new Vector3(deltaX+20, 0f) + transform.localPosition;
            }

        }
        
    }
}
