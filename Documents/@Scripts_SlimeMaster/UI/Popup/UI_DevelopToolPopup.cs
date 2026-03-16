using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static Define;

public class UI_DevelopToolPopup : UI_Popup
{

    enum GameObjects
    {
        Background,
    }
    enum Buttons
    {
        OpenButton,
    }
    enum Toggles
    {
        PlayerCharacterToggle,
        MonsterACharacterToggle,
        MonsterBCharacterToggle,
        BossCharacterToggle,
        FixedTypeToggle,
        FlexibleTypeToggle,
        GrassToggle,
        LavaToggle,
        SnowToggle,
        MonsterAToggle,
        MonsterBToggle,
        MonsterCToggle,
        MonsterDToggle,
        BossAToggle,
        BossBToggle,
    }

    public delegate void OnChangeSettingEventHandler(bool isFixed, int index);
    public static event OnChangeSettingEventHandler OnSettingChanged;
    int _polymophIdx;
    bool _isFixedJoystick;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindToggle(typeof(Toggles));

        GetButton((int)Buttons.OpenButton).gameObject.BindEvent(OnClickButton);
        GetToggle((int)Toggles.PlayerCharacterToggle).gameObject.BindEvent(()=> _polymophIdx = (int)Polymorph.BlueSlime);
        GetToggle((int)Toggles.MonsterACharacterToggle).gameObject.BindEvent(() => _polymophIdx = (int)Polymorph.Goblin);
        GetToggle((int)Toggles.MonsterBCharacterToggle).gameObject.BindEvent(() => _polymophIdx = (int)Polymorph.Snake);
        GetToggle((int)Toggles.BossCharacterToggle).gameObject.BindEvent(() => _polymophIdx = (int)Polymorph.GoblinLoad);
        GetToggle((int)Toggles.FixedTypeToggle).gameObject.BindEvent(() => OnClickJoystickType(true));
        GetToggle((int)Toggles.FlexibleTypeToggle).gameObject.BindEvent(() => OnClickJoystickType(false));

        _polymophIdx = 0;
        _isFixedJoystick = true;
        return true;
    }

    public void OnClickButton()
    {
        Managers.Sound.PlayButtonClick();

        if (OnSettingChanged != null)
            OnSettingChanged(_isFixedJoystick, _polymophIdx);

        Managers.UI.ClosePopupUI(this);
    }

    void OnClickJoystickType(bool isFixed)
    {
        _isFixedJoystick = isFixed;
    }
}
