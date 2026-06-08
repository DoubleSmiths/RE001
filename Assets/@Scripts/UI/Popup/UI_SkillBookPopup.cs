using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [능력 목록] 팝업 (기획서 06.05 슬40~42 + 능력 시스템 슬38~39).
// 보유 능력 목록 + 기억력(used/max) 표시. 능력 ICON 클릭 → [능력 정보창](기억/망각은 거기서).
//  - 능력 데이터/회계는 AbilityManager. 슬롯은 Content에 UI_SkillBookSkill 동적 생성.
//  - 슬42: 필터 = 능력 종류(전체/기술/마법/수비/지원). 잠금(미보유)은 목록에 없음(슬41).
public class UI_SkillBookPopup : UI_Popup
{
    #region Enum

    enum Texts
    {
        title,
        SubtitleText,
        MemoryAmountTxt_1,   // 기억력 게이지 "used/max"
    }

    enum Buttons
    {
        SkillPopupBackground,
        SkillPopup_ShowAll_Btn,
        SkillPopup_ShowTalent_Btn,
        SkillPopup_ShowOnMemory_Btn,
        SkillPopup_ShowNotOnMemory_Btn,
        SkillPopup_ShowLocked_Btn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        SkillBookPopup,
        Content,             // 능력 아이콘(UI_SkillBookSkill) 동적 생성 부모
    }

    #endregion

    // 슬42: 능력 종류 필터 (전체/기술/마법/수비/지원)
    enum Filter { All, Skill, Spell, Def, Util }
    Filter _filter = Filter.All;

    readonly List<int> _shown = new List<int>(); // 현재 표시 중인 abilityId 목록

    // 슬42: 필터 버튼 선택/기본 sprite (2_options_on/off). prefab 버튼에서 런타임 캐싱.
    Sprite _onSprite, _offSprite;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.SkillBookPopup).GetComponent<RectTransform>();
        OpenAnimation();

        GetButton((int)Buttons.SkillPopupBackground).gameObject.BindEvent(OnClickBG);

        // 슬42: 하단 5버튼 = 능력 종류 (좌→우: 전체/기술/마법/수비/지원). prefab 버튼 이름은 구버전이라 위치로 매핑.
        BindFilter(Buttons.SkillPopup_ShowAll_Btn,         Filter.All);
        BindFilter(Buttons.SkillPopup_ShowTalent_Btn,      Filter.Skill);
        BindFilter(Buttons.SkillPopup_ShowOnMemory_Btn,    Filter.Spell);
        BindFilter(Buttons.SkillPopup_ShowNotOnMemory_Btn, Filter.Def);
        BindFilter(Buttons.SkillPopup_ShowLocked_Btn,      Filter.Util);

        CacheOptionSprites();
        return true;
    }

    void BindFilter(Buttons btn, Filter f)
    {
        var b = GetButton((int)btn);
        if (b != null) b.gameObject.BindEvent(() => OnClickFilter(f));
    }

    // 슬42: 필터 클릭 → 선택 상태 갱신 + Scale 연출
    void OnClickFilter(Filter f)
    {
        _filter = f;

        var btn = GetButton(FilterButton(f));
        if (btn != null)
        {
            btn.transform.DOKill();
            btn.transform.localScale = Vector3.one;
            // 슬42: 상태전환 Scale Yoyo TO 0.85
            btn.transform.DOPunchScale(new Vector3(-0.15f, -0.15f, 0f), 0.25f, 8, 1f)
                .SetEase(Ease.OutQuad).SetUpdate(true);
        }

        Refresh();
    }

    public void SetInfo()
    {
        // 슬40: 제목 = AbilityCodex_TitleText("능력 목록"). prefab 영어("SKILL LIST") 하드코딩 교체.
        var title = GetText((int)Texts.title);
        if (title != null)
        {
            title.text = Util.Localize(Define.LocalizeKey.AbilityCodex_TitleText);
            ApplyFont(title, FontType.UI);
        }
        // 슬42: 하단 필터 버튼 라벨 한/영 (전체/기술/마법/수비/지원).
        foreach (Filter f in System.Enum.GetValues(typeof(Filter))) SetFilterLabel(f);

        // 슬42: 능력 목록 호출 시 항상 [전체] 선택
        _filter = Filter.All;
        Refresh();
    }

    // 슬42: 활성 필터에 대응하는 하단 버튼 enum (위치 매핑: 전체/기술/마법/수비/지원)
    int FilterButton(Filter f)
    {
        switch (f)
        {
            case Filter.Skill: return (int)Buttons.SkillPopup_ShowTalent_Btn;
            case Filter.Spell: return (int)Buttons.SkillPopup_ShowOnMemory_Btn;
            case Filter.Def:   return (int)Buttons.SkillPopup_ShowNotOnMemory_Btn;
            case Filter.Util:  return (int)Buttons.SkillPopup_ShowLocked_Btn;
            default:           return (int)Buttons.SkillPopup_ShowAll_Btn;
        }
    }

    // 슬42: 필터별 라벨 로컬라이즈 키
    string FilterLocKey(Filter f)
    {
        switch (f)
        {
            case Filter.Skill: return Define.LocalizeKey.AbilityCodex_BtmText_Skill;
            case Filter.Spell: return Define.LocalizeKey.AbilityCodex_BtmText_Spell;
            case Filter.Def:   return Define.LocalizeKey.AbilityCodex_BtmText_Def;
            case Filter.Util:  return Define.LocalizeKey.AbilityCodex_BtmText_Util;
            default:           return Define.LocalizeKey.AbilityCodex_BtmText_All;
        }
    }

    // 하단 필터 버튼의 라벨 텍스트를 로컬라이즈 값으로 설정
    void SetFilterLabel(Filter f)
    {
        var lbl = FilterLabel(f);
        if (lbl != null) { lbl.text = Util.Localize(FilterLocKey(f)); ApplyFont(lbl, FontType.UI); }
    }

    TMPro.TMP_Text FilterLabel(Filter f)
    {
        var btn = GetButton(FilterButton(f));
        return btn != null ? btn.GetComponentInChildren<TMPro.TMP_Text>(true) : null;
    }

    // 외부(능력 정보창 기억/망각)에서 필터 유지한 채 갱신
    public void RefreshUI() => Refresh();

    void Refresh()
    {
        RefreshMemoryGauge();
        RefreshFilterButtons();

        // 슬40: 상단 TopText(SubtitleText) = 현재 눌린 하단 필터 버튼의 라벨과 동일하게 표기
        var sub = GetText((int)Texts.SubtitleText);
        var curLbl = FilterLabel(_filter);
        if (sub != null && curLbl != null) { sub.text = curLbl.text; ApplyFont(sub, FontType.UI); }

        // 필터에 맞는 능력 목록 수집 (DataId 정렬)
        _shown.Clear();
        var ids = new List<int>(Managers.Data.SkillDic.Keys);
        ids.Sort();
        foreach (int id in ids)
            if (PassFilter(id)) _shown.Add(id);

        // Content 비우고 능력 아이콘 동적 생성
        var content = GetObject((int)GameObjects.Content).transform;
        for (int i = content.childCount - 1; i >= 0; i--)
            Managers.Resource.Destroy(content.GetChild(i).gameObject);

        foreach (int id in _shown)
        {
            var item = Managers.UI.MakeSubItem<UI_SkillBookSkill>(content, pooling: false);
            item.SetInfo(id, OnClickSkill);
        }
    }

    // 슬41: 능력 ICON 클릭 → [능력 정보창] 호출 (기억/망각은 정보창에서)
    void OnClickSkill(int abilityId)
    {
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(abilityId);
    }

    // 슬41: 잠김(미보유) 능력은 [능력 목록]에 없음(미니몬 도감에서만) → 보유 능력만.
    // 슬42: 필터 = 능력 종류(All/기술/마법/수비/지원).
    bool PassFilter(int id)
    {
        if (!Managers.Ability.IsLearned(id)) return false;
        if (_filter == Filter.All) return true;
        var role = Managers.Ability.GetRole(id);
        switch (_filter)
        {
            case Filter.Skill: return role == AbilityManager.Role.Skill;
            case Filter.Spell: return role == AbilityManager.Role.Spell;
            case Filter.Def:   return role == AbilityManager.Role.Def;
            case Filter.Util:  return role == AbilityManager.Role.Util;
            default:           return true;
        }
    }

    void RefreshMemoryGauge()
    {
        var txt = GetText((int)Texts.MemoryAmountTxt_1);
        if (txt == null) return;
        int used = Managers.Ability.UsedMemory;
        int max  = Managers.Ability.MaxMemory;
        txt.text = $"{used}/{max}";
        // 슬40: 현재 기억력 색 — 0=red / <max=yellow / =max=lime
        txt.color = used == 0      ? Util.HexToColor("FF0000")
                  : used >= max    ? Util.HexToColor("00FF00")
                  :                  Util.HexToColor("FFFF00");
        ApplyFont(txt, FontType.UI);
    }

    #region 슬42: 필터 버튼 선택/기본 2상태

    // prefab 버튼들의 현재 sprite에서 2_options_on/off를 캐싱 (IMPORTS라 Resources.Load 불가)
    void CacheOptionSprites()
    {
        foreach (Filter f in System.Enum.GetValues(typeof(Filter)))
        {
            var img = OptionImage(GetButton(FilterButton(f)));
            if (img == null || img.sprite == null) continue;
            if (img.sprite.name.Contains("_on"))  _onSprite  = img.sprite;
            if (img.sprite.name.Contains("_off")) _offSprite = img.sprite;
        }
    }

    // 버튼(+자식) 중 2_options sprite를 쓰는 배경 Image
    Image OptionImage(Button btn)
    {
        if (btn == null) return null;
        foreach (var img in btn.GetComponentsInChildren<Image>(true))
            if (img.sprite != null && img.sprite.name.Contains("2_options")) return img;
        return btn.image;   // 폴백: 버튼 자신
    }

    // 슬42: 선택=2_options_on+EEB568+Outline 594A2D / 기본=2_options_off+흰색+Outline 000000
    void RefreshFilterButtons()
    {
        foreach (Filter f in System.Enum.GetValues(typeof(Filter)))
        {
            var btn = GetButton(FilterButton(f));
            if (btn == null) continue;
            bool sel = (f == _filter);

            // 슬42: 2_options 스프라이트 스왑 — 옵션 탭(자식 이미지)에만 적용
            var opt = OptionImage(btn);
            if (opt != null && opt != btn.image)
            {
                if (sel  && _onSprite  != null) opt.sprite = _onSprite;
                if (!sel && _offSprite != null) opt.sprite = _offSprite;
            }

            // 슬42: 프레임(Frame_bg_big) 색 — 선택=EEB568 / 기본=흰색.
            // 색은 옵션 탭이 아니라 버튼 본체(targetGraphic) 프레임에 적용해야 한다.
            if (btn.image != null)
                btn.image.color = sel ? Util.HexToColor("EEB568") : Color.white;

            // 슬42: Text Outline — 선택=594A2D / 기본=000000
            var lbl = FilterLabel(f);
            if (lbl != null)
            {
                lbl.outlineColor = sel ? Util.HexToColor("594A2D") : Util.HexToColor("000000");
                lbl.outlineWidth = 0.15f;
            }
        }
    }

    #endregion
}
