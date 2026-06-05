using System;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [능력 목록](UI_SkillBookPopup) 안의 능력 아이콘 한 칸 (기획서 06.05 슬41).
//  - Content에 보유 능력 수만큼 동적 생성(MakeSubItem). SetInfo(abilityId, onClick)로 채움.
//  - 슬41: 미보유(잠금)는 [능력 목록]에 없음(미니몬 도감에서만) → 잠금 표시 off.
//  - 상태 표기: 재능/기억중 = 기억 오버레이("기억중")+Active_Ef / 그 외 = 레벨(레벨<최대=lime, =최대=magenta).
//  - 능력 ICON 클릭 시 Punch/Scale 후 콜백(→[능력 정보창]).
public class UI_SkillBookSkill : UI_Base
{
    #region Enum

    enum Images
    {
        Img_SkillBookSkill_0_09,
        Img_SkillDetailActiveEf_1_07,
        Img_SkillBookSkill_0_04,
        Img_SkillBookSkill_0_01,
        Img_LockedIcon_09,
        Img_SkillBookSkill_0_03,
        Img_MemorizedIcon_01,
        Img_SkillBookSkill_0_10,
        Img_LvlIcon_03,
    }

    enum Texts
    {
        Txt_MemorizedIcon_18,
        Txt_LvlIcon_13,
    }

    enum Buttons
    {
        Btn_SkillBookSkill_0_07,
    }

    enum GameObjects
    {
        Panel_Frame_05,
    }

    #endregion


    int _abilityId;
    Action<int> _onClick;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.Btn_SkillBookSkill_0_07).gameObject.BindEvent(OnClick);
        return true;
    }

    public void SetInfo(int abilityId, Action<int> onClick)
    {
        _abilityId = abilityId;
        _onClick = onClick;

        SkillData d = Managers.Data.SkillDic.TryGetValue(abilityId, out var sd) ? sd : null;
        bool talent    = Managers.Ability.IsTalent(abilityId);
        bool memorized = Managers.Ability.IsEquipped(abilityId);
        bool active    = talent || memorized;                          // 재능 or 기억중
        int lv    = Mathf.Max(1, Managers.Ability.GetLevel(abilityId));
        int maxLv = d != null ? Mathf.Max(1, d.MaxLevel) : lv;
        bool maxed = lv >= maxLv;

        // 슬41: 능력 목록엔 미보유(잠금) 없음
        Show(Images.Img_LockedIcon_09, false);
        // 재능/기억중일 때만 Active_Ef
        Show(Images.Img_SkillDetailActiveEf_1_07, active);

        // 능력 아이콘 sprite (IconLabel 있을 때만, 없으면 prefab 기본)
        var icon = GetImage((int)Images.Img_SkillBookSkill_0_01);
        if (icon != null && d != null && !string.IsNullOrEmpty(d.IconLabel))
        {
            var sp = Managers.Resource.Load<Sprite>(d.IconLabel);
            if (sp != null) { icon.sprite = sp; icon.preserveAspect = true; }
        }

        // 상태 오버레이: 기억중/재능("기억중") ↔ 레벨 (택1)
        Show(Images.Img_MemorizedIcon_01, active);
        ShowText(Texts.Txt_MemorizedIcon_18, active);
        Show(Images.Img_LvlIcon_03, !active);
        ShowText(Texts.Txt_LvlIcon_13, !active);

        if (active)
        {
            var memTxt = GetText((int)Texts.Txt_MemorizedIcon_18);
            if (memTxt != null) ApplyFont(memTxt, FontType.UI);        // prefab "기억중" 유지
        }
        else
        {
            // 슬41: 레벨<최대 = lime(프레임 기본) / =최대 = magenta(프레임 FF00F5)
            var frame = GetImage((int)Images.Img_LvlIcon_03);
            if (frame != null && maxed) frame.color = Util.HexToColor("FF00F5");

            var lvlTxt = GetText((int)Texts.Txt_LvlIcon_13);
            if (lvlTxt != null)
            {
                lvlTxt.text = $"LEVEL.<color={(maxed ? "magenta" : "lime")}>{lv}</color>";
                ApplyFont(lvlTxt, FontType.UI);
            }
        }
    }

    // 슬41: 능력 ICON 클릭 → Punch/Scale 후 [능력 정보창] 콜백
    void OnClick()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(new Vector3(-0.25f, 0.15f, 0f), 0.3f, 10, 1f)
            .SetEase(Ease.OutQuad).SetUpdate(true);

        _onClick?.Invoke(_abilityId);
    }

    void Show(Images id, bool on)
    {
        var img = GetImage((int)id);
        if (img != null) img.gameObject.SetActive(on);
    }

    void ShowText(Texts id, bool on)
    {
        var t = GetText((int)id);
        if (t != null) t.gameObject.SetActive(on);
    }
}
