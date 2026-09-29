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
            if (sp != null)
            {
                icon.sprite = sp;
                icon.preserveAspect = true;
                // 프리팹에 저작된 tint가 검정(alpha 0.94)이라 넣은 아이콘이 새까맣게 칠해졌다.
                //  아래 깔린 버튼의 placeholder 그림만 비쳐서 "능력 아이콘이 전부 똑같다"로 보였다.
                icon.color = Color.white;
            }
        }

        // 클릭 판정용 버튼에 placeholder 아이콘(AutoPotion Cooltime Skill Icon)이 저작돼 있다.
        //  아이콘의 투명한 부분으로 그게 비쳐 능력마다 같은 그림이 겹쳐 보인다 → 그림만 지운다.
        //  (Image 컴포넌트는 남겨야 클릭이 먹는다. sprite=null + alpha 0 이면 레이캐스트는 유지된다)
        var btnImg = GetButton((int)Buttons.Btn_SkillBookSkill_0_07)?.GetComponent<Image>();
        if (btnImg != null)
        {
            btnImg.sprite = null;
            btnImg.color = new Color(0f, 0f, 0f, 0f);
        }

        // 슬41: 레벨(단계)은 항상 표시 — 기억중/재능이어도 함께 보인다(image105 목업: 기억중 아이콘도 "5등급" 표시).
        Show(Images.Img_LvlIcon_03, true);
        ShowText(Texts.Txt_LvlIcon_13, true);
        // 슬41: 프레임(HP_MP_little_frame=Img_LvlIcon_03) = 흰색(<최대) / FF00F5(=최대).
        var frame = GetImage((int)Images.Img_LvlIcon_03);
        if (frame != null) frame.color = maxed ? Util.HexToColor("FF00F5") : Color.white;
        // 슬41: 배경(Sprite(None)=Img_SkillBookSkill_0_10) = 000000(1~4등급) / 3C143A(5등급=최대).
        var lvlBg = GetImage((int)Images.Img_SkillBookSkill_0_10);
        if (lvlBg != null) lvlBg.color = maxed ? Util.HexToColor("3C143A") : Util.HexToColor("000000");
        var lvlTxt = GetText((int)Texts.Txt_LvlIcon_13);
        if (lvlTxt != null)
        {
            // 슬41: 스킬레벨(숫자)만 색 — 단계<최대=#B9B900(진한 노랑, 이미지 기준) / =최대=#FF00FF(magenta).
            //       "등급"(LevelText)은 흰색이어야 하므로 base color=흰색 + 숫자만 rich-text 색.
            string col = maxed ? "#FF00FF" : "#B9B900";
            lvlTxt.color = Color.white;
            lvlTxt.text = $"<color={col}>{lv}</color>{Util.Localize(Define.LocalizeKey.LevelText)}";
            ApplyFont(lvlTxt, FontType.UI);
        }

        // 슬41: 재능/기억중이면 상단에 "기억중" 배지 표시 / 아니면 끔. 배지 배경 컨테이너(Img_SkillBookSkill_0_03)째 토글.
        //  (기억하기 시 active=true로 켜지고, 망각 시 RefreshUI→SetInfo 재실행으로 꺼짐)
        bool showMemoryBadge = active && (!Managers.Ability.IsCompanionGranted(abilityId)
            || Managers.Game.EquippedAbilities.Contains(abilityId));
        Show(Images.Img_SkillBookSkill_0_03, showMemoryBadge);
        Show(Images.Img_MemorizedIcon_01, showMemoryBadge);
        ShowText(Texts.Txt_MemorizedIcon_18, showMemoryBadge);
        if (showMemoryBadge)
        {
            var memTxt = GetText((int)Texts.Txt_MemorizedIcon_18);
            if (memTxt != null)
            {
                memTxt.text = Util.Localize(Define.LocalizeKey.Memorized_Text_Short);
                ApplyFont(memTxt, FontType.UI);
            }
        }
    }

    // 슬41: 능력 ICON 클릭 → authored Punch/Scale(루트 DOTweenAnimation, 0.3/OutQuad/-0.25,0.15/vib10/el1) 재생 후 [능력 정보창] 콜백
    void OnClick()
    {
        var anim = GetComponent<DG.Tweening.DOTweenAnimation>();
        if (anim != null) anim.DORestart();
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
