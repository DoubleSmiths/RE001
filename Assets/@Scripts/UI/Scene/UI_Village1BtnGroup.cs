using static Define;

public class UI_Village1BtnGroup : UI_AreaBtnGroup
{
    enum Buttons
    {
        GuildButton,
        AlchemyButton,
        TravelButton,
        BackButton,
    }

    enum GameObjects
    {
        MainBtnGroup,
        GuildBtnGroup,
        AlchemyBtnGroup,
        TravelBtnGroup,
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.GuildButton).gameObject.BindEvent(OnClickGuild);
        GetButton((int)Buttons.AlchemyButton).gameObject.BindEvent(OnClickAlchemy);
        GetButton((int)Buttons.TravelButton).gameObject.BindEvent(OnClickTravel);
        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnClickBack);

        ShowGroup(GameObjects.MainBtnGroup);

        return true;
    }

    void ShowGroup(GameObjects group)
    {
        foreach (GameObjects g in System.Enum.GetValues(typeof(GameObjects)))
            GetObject((int)g).SetActive(g == group);
    }

    void OnClickGuild()   => ShowGroup(GameObjects.GuildBtnGroup);
    void OnClickAlchemy() => ShowGroup(GameObjects.AlchemyBtnGroup);
    void OnClickTravel()  => ShowGroup(GameObjects.TravelBtnGroup);
    void OnClickBack()    => ShowGroup(GameObjects.MainBtnGroup);
}
