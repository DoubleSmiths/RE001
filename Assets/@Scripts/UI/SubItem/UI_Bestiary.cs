using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.UI;

// 마물 도감의 지역 한 덩어리. 지역명 + 그 지역 몬스터 박스(UI_BestiaryBox) 그리드.
// UI_BestiaryPopup의 Content 안에서 지역 수만큼 동적 생성된다.
public class UI_Bestiary : UI_Base
{
    #region Enum

    enum Images
    {
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
    }

    enum GameObjects
    {
        BestiaryAreaTextGroup,
        BestiayBoxGroup,
        BestiaryBoxSelect,
    }

    #endregion

    RectTransform _boxGroup;
    GridLayoutGroup _grid;
    LayoutElement _reservedLayout;
    List<CreatureData> _monsters;
    Action<int> _onClickMonster;
    readonly List<UI_BestiaryBox> _boxes = new();
    bool _boxesVisible;

    public float RowStride => _grid.cellSize.y + _grid.spacing.y;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _boxGroup = GetObject((int)GameObjects.BestiayBoxGroup).GetComponent<RectTransform>();
        _grid = _boxGroup.GetComponent<GridLayoutGroup>();
        _reservedLayout = _boxGroup.gameObject.GetOrAddComponent<LayoutElement>();
        _reservedLayout.flexibleHeight = 0f;
        // 표시 여부와 관계없이 전체 행 높이를 부모 레이아웃에 제공한다.
        var fitter = _boxGroup.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;

        ApplyFont(GetText((int)Texts.BestiaryAreaText), Define.FontType.UI);
        ApplyFont(GetText((int)Texts.BestiaryAreaTextBar), Define.FontType.UI);
        return true;
    }

    // regionName: 지역 표시명 / monsters: (데이터, 도감번호, 포획여부) 목록 / onClickMonster: 박스 클릭 콜백
    public void SetInfo(string regionName, List<CreatureData> monsters, Action<int> onClickMonster = null, bool deferBoxes = false)
    {
        RefreshRegionName(regionName);
        _monsters = monsters;
        _onClickMonster = onClickMonster;

        // prefab에 박혀있는 템플릿 박스 포함 기존 자식 제거
        ClearBoxes();
        RefreshReservedHeight();
        if (!deferBoxes) SetBoxesVisible(true);
    }

    public void RefreshRegionName(string regionName)
    {
        var areaText = GetText((int)Texts.BestiaryAreaText);
        if (areaText != null)
        {
            areaText.text = regionName;
            // 슬라이드 72: PlaceName Outline 83BC00 / Alpha 180 / Distance (0.5, -0.5)
            SetOutline(areaText, 180);
        }

        var bar = GetText((int)Texts.BestiaryAreaTextBar);
        if (bar != null)
        {
            // 슬라이드 72: Décor("|") Outline 동일 색 / Alpha 100
            SetOutline(bar, 100);
        }
    }

    public bool RefreshReservedHeight()
    {
        int count = _monsters != null ? _monsters.Count : 0;
        int rows;
        // UGUI GridLayoutGroup.CalculateLayoutInputVertical과 같은 열/행 계산이다.
        if (_grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            rows = Mathf.CeilToInt(count / (float)_grid.constraintCount - Define.BESTIARY_LAYOUT_EPSILON);
        else if (_grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
            rows = _grid.constraintCount;
        else
        {
            int columns = Mathf.Max(1, Mathf.FloorToInt((_boxGroup.rect.width - _grid.padding.horizontal
                + _grid.spacing.x + Define.BESTIARY_LAYOUT_EPSILON) / (_grid.cellSize.x + _grid.spacing.x)));
            rows = Mathf.CeilToInt(count / (float)columns);
        }
        float height = _grid.padding.vertical + RowStride * rows - _grid.spacing.y;
        if (Mathf.Approximately(_reservedLayout.preferredHeight, height)) return false;
        _reservedLayout.minHeight = height;
        _reservedLayout.preferredHeight = height;
        return true;
    }

    public void SetBoxesVisible(bool visible)
    {
        if (_boxesVisible == visible) return;
        if (!visible)
        {
            ClearBoxes();
            return;
        }

        _boxesVisible = true;
        _grid.enabled = true;
        if (_monsters == null) return;
        foreach (var monster in _monsters)
        {
            UI_BestiaryBox box = Managers.UI.MakeSubItem<UI_BestiaryBox>(_boxGroup, pooling: false);
            // 재진입 때 기록도·별·동행을 현재 저장 데이터에서 다시 읽는다.
            box.SetInfo(monster, _onClickMonster);
            _boxes.Add(box);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_boxGroup);
    }

    void ClearBoxes()
    {
        _boxesVisible = false;
        _grid.enabled = false;
        _boxes.Clear();
        for (int i = _boxGroup.childCount - 1; i >= 0; i--)
        {
            var child = _boxGroup.GetChild(i).gameObject;
            child.GetComponent<UI_Base>()?.StopAnimations();
            // Destroy는 프레임 끝에 처리되므로 먼저 숨겨 레이아웃/입력에서 즉시 제외한다.
            child.SetActive(false);
            Managers.Resource.Destroy(child);
        }
    }

    public UI_BestiaryBox FindBox(int dataId)
    {
        foreach (var box in _boxes)
            if (box != null && box.DataId == dataId) return box;
        return null;
    }

    public void RefreshCompanionVisuals()
    {
        foreach (var box in _boxes)
            if (box != null) box.RefreshCompanionVisual();
    }

    // 슬라이드 72: 지역명/장식 Outline — 색 83BC00 고정, alpha만 가변, Distance (0.5, -0.5)
    void SetOutline(TMPro.TMP_Text txt, byte alpha)
    {
        var outline = txt.GetComponent<Outline>();
        if (outline == null) return;
        outline.effectColor = new Color32(0x83, 0xBC, 0x00, alpha);
        outline.effectDistance = new Vector2(0.5f, -0.5f);
    }

}
