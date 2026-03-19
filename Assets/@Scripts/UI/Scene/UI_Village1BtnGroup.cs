using static Define;

public class UI_Village1BtnGroup : UI_AreaBtnGroup
{
    enum Buttons
    {
        GuildButton,
        AlchemyButton,
        TravelButton,
        //BackButton,
    }

    enum GameObjects
    {
        Village1MainBtnGroup,
        Village1GuildBtnGroup,
        Village1AlchemyBtnGroup,
        Village1MonasteryBtnGroup,
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.GuildButton).gameObject.BindEvent(OnClickGuild);
        GetButton((int)Buttons.AlchemyButton).gameObject.BindEvent(OnClickAlchemy);
        GetButton((int)Buttons.TravelButton).gameObject.BindEvent(OnClickTravel);
        //GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnClickBack);

        HideAllGroup();

        ShowGroup(GameObjects.Village1MainBtnGroup);

        return true;
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

    void OnClickGuild()   => ShowGroup(GameObjects.Village1GuildBtnGroup);
    void OnClickAlchemy() => ShowGroup(GameObjects.Village1AlchemyBtnGroup);
    void OnClickTravel()  => ShowGroup(GameObjects.Village1MonasteryBtnGroup);
    void OnClickBack()    => ShowGroup(GameObjects.Village1MainBtnGroup);
}
