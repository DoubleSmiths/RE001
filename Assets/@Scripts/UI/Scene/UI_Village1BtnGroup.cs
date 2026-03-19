using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class UI_Village1BtnGroup : UI_AreaBtnGroup
{
    enum Buttons
    {
        GuildButton,
        AlchemyButton,
        TravelButton,
        Village1GuildExitButton,
        Village1AlchemyExitButton,
        Village1MonasteryExitButton,
    }

    enum Images
    {
        GuildButtonIcon,
        AlchemyButtonIcon,
        MonasteryButtonIcon,
        TravelButtonIcon,
    }

    enum Texts
    {
        Village1NameText,
        GuildButtonText,
        AlchemyButtonText,
        AlchemyButtonNewTxt,
        MonasteryButtonText,
        MonasteryButtonNewTxt,
        TravelButtonText,
        TravelButtonNewTxt,
    }

    enum GameObjects
    {
        Village1MainBtnGroup,
        Village1GuildBtnGroup,
        Village1AlchemyBtnGroup,
        Village1MonasteryBtnGroup,
        Village1GuildVFX,
        Village1AlchemyVFX_Sub1,
        MonasteryVFX_Sub1,
        Village1TravelQuestVFX,
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        GetImage((int)Images.GuildButtonIcon).gameObject.BindEvent(OnClickGuild);
        GetImage((int)Images.AlchemyButtonIcon).gameObject.BindEvent(OnClickAlchemy);
        GetImage((int)Images.TravelButtonIcon).gameObject.BindEvent(OnClickTravel);
        GetButton((int)Buttons.Village1GuildExitButton).gameObject.BindEvent(OnClickMain);
        GetButton((int)Buttons.Village1AlchemyExitButton).gameObject.BindEvent(OnClickMain);
        GetButton((int)Buttons.Village1MonasteryExitButton).gameObject.BindEvent(OnClickMain);

        SetInfo();

        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            UnlockButtonAnim(GetButton((int)Buttons.AlchemyButton).gameObject, GetImage((int)Images.AlchemyButtonIcon), GetText((int)Texts.AlchemyButtonText));
        }
    }

    void SetInfo()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1MainBtnGroup);

        //GetImage((int)Images.GuildButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.AlchemyButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.MonasteryButtonIcon).gameObject.SetActive(false);
        GetImage((int)Images.TravelButtonIcon).gameObject.SetActive(false);
        //GetText((int)Texts.GuildButtonText).gameObject.SetActive(false);
        GetText((int)Texts.AlchemyButtonText).gameObject.SetActive(false);
        GetText((int)Texts.AlchemyButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.MonasteryButtonText).gameObject.SetActive(false);
        GetText((int)Texts.MonasteryButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.TravelButtonText).gameObject.SetActive(false);
        GetText((int)Texts.TravelButtonNewTxt).gameObject.SetActive(false);

        GetObject((int)GameObjects.Village1GuildVFX).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1AlchemyVFX_Sub1).gameObject.SetActive(false);
        GetObject((int)GameObjects.MonasteryVFX_Sub1).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1TravelQuestVFX).gameObject.SetActive(false);

        GetText((int)Texts.AlchemyButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.MonasteryButtonNewTxt).gameObject.SetActive(false);
        GetText((int)Texts.TravelButtonNewTxt).gameObject.SetActive(false);

        // TODO : 해금된 버튼이 있으면 Image, Text 를 SetActive(true)로 변경.
        // TODO : 이벤트 ID 확인 후 VFX 켜기, NEW Text 켜기, 


    }

    void ShowGroup(GameObjects group)
    {
        foreach (GameObjects g in System.Enum.GetValues(typeof(GameObjects)))
            GetObject((int)g).SetActive(g == group);
    }

    void HideAllGroup()
    {
        GetObject((int)GameObjects.Village1MainBtnGroup).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1GuildBtnGroup).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1AlchemyBtnGroup).gameObject.SetActive(false);
        GetObject((int)GameObjects.Village1AlchemyBtnGroup).gameObject.SetActive(false);
    }

    void OnClickMain()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1MainBtnGroup);
    }

    void OnClickGuild()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1GuildBtnGroup);
    }

    void OnClickAlchemy()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1AlchemyBtnGroup);
    }

    void OnClickTravel()
    {
        HideAllGroup();

        ShowGroup(GameObjects.Village1MonasteryBtnGroup);
    }

    
}
