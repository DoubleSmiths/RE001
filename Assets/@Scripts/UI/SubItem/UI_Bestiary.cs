using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

// 마물 도감의 지역 한 덩어리. 지역명 + 그 지역 몬스터 박스(UI_BestiaryBox) 그리드.
// UI_BestiaryPopup의 Content 안에서 지역 수만큼 동적 생성된다.
public class UI_Bestiary : UI_Base
{
    #region Enum

    enum Images
    {
        UI_Bestiary,
        BestiaryAreaTextLine,
        BestiaryBoxSelectActive,
        BestiaryBoxFrame,
        BestiaryBoxMask,
        BestiaryBoxGlow,
        BeastiaryBoxMonsterExist,
        BestiaryBoxUnlock,
        BestiaryBoxUnlockGlow,
        BestiaryBoxNumberFrame,
        BestiaryBoxStar,
    }

    enum Texts
    {
        BestiaryAreaText,
        BestiaryAreaTextBar,
        BestiaryBoxUnlockQustion,
        BestiaryBoxNumber,
    }

    enum Buttons
    {
        UI_BestiaryBox,
    }

    enum GameObjects
    {
        BestiaryAreaTextGroup,
        BestiayBoxGroup,
        BestiaryBoxSelect,
    }

    #endregion


    void Awake() => Init();

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

    // regionName: 지역 표시명 / monsters: (데이터, 도감번호, 포획여부) 목록 / onClickMonster: 박스 클릭 콜백
    public void SetInfo(string regionName, List<CreatureData> monsters, Action<int> onClickMonster = null)
    {
        var areaText = GetText((int)Texts.BestiaryAreaText);
        if (areaText != null)
        {
            areaText.text = regionName;
            ApplyFont(areaText, Define.FontType.UI);
        }

        var bar = GetText((int)Texts.BestiaryAreaTextBar);
        if (bar != null)
            ApplyFont(bar, Define.FontType.UI);

        Transform boxGroup = GetObject((int)GameObjects.BestiayBoxGroup).transform;

        // prefab에 박혀있는 템플릿 박스 포함 기존 자식 제거
        for (int i = boxGroup.childCount - 1; i >= 0; i--)
            Managers.Resource.Destroy(boxGroup.GetChild(i).gameObject);

        if (monsters == null)
            return;

        foreach (var m in monsters)
        {
            UI_BestiaryBox box = Managers.UI.MakeSubItem<UI_BestiaryBox>(boxGroup, pooling: false);
            box.SetInfo(m, onClickMonster);
        }
    }

}
