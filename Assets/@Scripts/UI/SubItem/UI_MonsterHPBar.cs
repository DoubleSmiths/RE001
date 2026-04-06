using UnityEngine;

public class UI_MonsterHPBar : UI_Base
{
    #region Enum

    enum Images
    {
        Frame,
        MonsterHPBar,
        MonsterHPBarGuage,
    }

    enum Texts
    {
        MonsterNameDisplay,
    }

    enum Buttons
    {
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

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        return true;
    }

    public void SetInfo(MonsterController monster)
    {
        if (Managers.Data.CreatureDic.TryGetValue(monster.DataId, out var data))
            GetText((int)Texts.MonsterNameDisplay).text = data.Name;

        UpdateHp(monster.Hp, monster.MaxHp);

        monster.OnHpChanged += (cur, max) =>
        {
            UpdateHp(cur, max);
            if (cur <= 0)
                Managers.Resource.Destroy(gameObject);
        };
    }

    void UpdateHp(float cur, float max)
    {
        GetImage((int)Images.MonsterHPBarGuage).fillAmount = max > 0 ? cur / max : 0f;
    }

}
