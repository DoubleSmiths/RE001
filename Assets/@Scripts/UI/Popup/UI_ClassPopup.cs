using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [직업(전직) 능력] 팝업 — 수도원 [축성] 버튼으로 진입. UI_SkillBookPopup과 동일 프레임 구조.
//  - SkillDic에서 출처(Source)=Class인 능력만 슬롯(SkillBookSkill_0~6 템플릿)에 표시.
//  - MemoryAmountTxt_1 = 기억력 게이지 대신 [직업 숙련도] "prof/100" (만충=#00FF00 / 미만=yellow).
//  - 슬롯 클릭: 보유 → [능력 정보창] / 미보유&숙련도 만충 → 축성(TryAnoint)+authored PunchScale /
//    미보유&숙련도 부족 → 토스트(Lack_Proficiency_Text).
//  - 필터 버튼(SkillPopup_BtnGroup)은 출처가 Class 고정이라 미사용(비활성).
public class UI_ClassPopup : UI_Popup
{
    #region Enum

    enum Texts
    {
        title,
        SubtitleText,
        MemoryAmountTxt_1,   // 직업 숙련도 게이지 "prof/100"
    }

    enum Buttons
    {
        ClassPopupBackground,
    }

    enum Images
    {
        ClassPopupBlockImg,
        Xbutton,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        ClassPopup,
        Content,             // SkillBookSkill_0~6 슬롯 템플릿 부모
        SkillPopup_BtnGroup, // 필터 버튼 그룹 — 직업창에선 불필요(비활성)
    }

    #endregion


    const int SLOT_COUNT = 7;   // prefab 슬롯 템플릿 SkillBookSkill_0~6

    // 슬롯 템플릿 내부 노드(접미사 난수)는 prefix 탐색으로 캐싱
    class Slot
    {
        public Transform Root;          // SkillBookSkill_N (authored PunchScale 보유)
        public GameObject IconBtn;      // Btn_SkillBookSkill_N_* (아이콘 겸 클릭)
        public GameObject Locked;       // 잠금 오버레이 컨테이너 (Img_LockedIcon_* 부모)
        public GameObject ActiveEf;     // Panel_Frame_* (기억중/재능 빛 이펙트)
        public GameObject MemBadge;     // 기억중 배지 컨테이너 (Img_MemorizedIcon_* 부모)
        public TMP_Text MemTxt;         // Txt_MemorizedIcon_*
        public GameObject LvlGroup;     // 레벨 표기 컨테이너 (Img_LvlIcon_* 부모)
        public Image LvlBg;             // 레벨 컨테이너 배경
        public Image LvlFrame;          // Img_LvlIcon_* (HP_MP_little_frame)
        public TMP_Text LvlTxt;         // Txt_LvlIcon_*
    }

    readonly Slot[] _slots = new Slot[SLOT_COUNT];
    readonly int[] _slotIds = new int[SLOT_COUNT];   // 슬롯별 능력 id (-1=빈 슬롯)

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.ClassPopup).GetComponent<RectTransform>();

        // ⚠️ 이 팝업은 스킬북 프리팹을 복제해 만들었고, 그때 **autoPlay Move 저작 애니 9개**가
        //  딸려 왔다(라벨·NEW 뱃지 등). 창이 아래에서 올라오는 0.5초 동안 그것들이 제각각
        //  0.5초씩 따로 움직여, 내용물이 창을 쫓아오다 미끄러지는 것처럼 보였다.
        //  창과 내용이 **한 덩어리로** 올라오게 만든 뒤, 내용은 부드럽게 페이드로만 들인다.
        StopStrayIntroAnimations();
        OpenAnimation();
        FadeInContents();

        GetButton((int)Buttons.ClassPopupBackground).gameObject.BindEvent(OnClickBG);
        // 우상단 X 버튼 — Button 미부착 노드라 Image에 직접 바인드 (UI_SkillDetailPopup과 동일 패턴)
        var xGo = GetImage((int)Images.Xbutton)?.gameObject;
        if (xGo != null) xGo.BindEvent(OnClickBG);

        // 투명 BlockImg가 슬롯/X 클릭을 가로채지 않게 raycast 차단 (UI_MinionInfoPopup Block 처리와 동일)
        var block = GetImage((int)Images.ClassPopupBlockImg);
        if (block != null) block.raycastTarget = false;

        // 필터 버튼 그룹 — 직업창은 출처 고정(Class)이라 비활성
        SetActiveSafe(GetObject((int)GameObjects.SkillPopup_BtnGroup), false);

        CacheSlots();

        // 숙련도/능력 보유 변동 시 갱신 (OnDestroy 해제)
        if (Managers.Game != null) Managers.Game.OnClassProficiencyChanged += Refresh;
        if (Managers.Ability != null) Managers.Ability.OnAbilityChanged += Refresh;

        return true;
    }

    // ── 등장 연출 ────────────────────────────────────────────────
    const float CONTENT_FADE = 0.25f;

    /// <summary>복제로 딸려온 autoPlay Move 애니를 멈추고 최종 위치로 확정한다.
    /// Fade는 남겨도 무해하지만 Move는 창 이동과 겹쳐 흔들림이 된다.</summary>
    void StopStrayIntroAnimations()
    {
        foreach (var anim in GetComponentsInChildren<DOTweenAnimation>(true))
        {
            if (!anim.autoPlay) continue;
            if (anim.animationType != DOTweenAnimation.AnimationType.Move) continue;

            anim.DOComplete();      // 목적지에 바로 놓는다
            anim.DOKill();
            anim.autoPlay = false;  // 다시 열 때 또 튀지 않게
        }
    }

    /// <summary>창이 올라오는 동안 내용은 살짝 늦게 스며들게 한다.
    /// 위치를 건드리지 않아 창 이동과 싸우지 않는다.</summary>
    void FadeInContents()
    {
        if (_frameRT == null) return;

        // ⚠️ `??`는 여기서 못 쓴다 — 유니티의 파괴된/없는 컴포넌트는 진짜 null이 아니라
        //  "fake null"이라 null 병합 연산자가 왼쪽을 살아 있다고 보고 넘어간다.
        //  실제로 CanvasGroup이 끝내 안 붙어 페이드가 통째로 건너뛰어졌다.
        var cg = _frameRT.GetComponent<CanvasGroup>();
        if (cg == null) cg = _frameRT.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.alpha = 0f;
        cg.DOFade(1f, CONTENT_FADE).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Managers.Game != null) Managers.Game.OnClassProficiencyChanged -= Refresh;
        if (Managers.Ability != null) Managers.Ability.OnAbilityChanged -= Refresh;
    }

    public void SetInfo()
    {
        // 타이틀 "직업"은 prefab authored 텍스트 유지(전용 로컬라이즈 키 없음 — 새 키 남발 금지), 폰트만 적용
        var title = GetText((int)Texts.title);
        if (title != null) ApplyFont(title, FontType.UI);

        // 상단 서브타이틀 = "직업 기술" (기존 키 Class_Ability_Text 재사용)
        var sub = GetText((int)Texts.SubtitleText);
        if (sub != null) { sub.text = Util.Localize(LocalizeKey.Class_Ability_Text); ApplyFont(sub, FontType.UI); }

        Refresh();
    }

    // 외부(능력 정보창 축성/기억 등)에서 갱신
    public override void RefreshUI() => Refresh();

    void Refresh()
    {
        RefreshProficiency();

        // 출처=Class 능력만 수집 (DataId 정렬)
        var ids = new List<int>();
        if (Managers.Data?.SkillDic != null)
            foreach (int id in Managers.Data.SkillDic.Keys)
                if (Managers.Ability.GetSource(id) == AbilitySource.Class) ids.Add(id);
        ids.Sort();

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            var s = _slots[i];
            if (s == null || s.Root == null) { _slotIds[i] = -1; continue; }
            bool used = i < ids.Count;
            _slotIds[i] = used ? ids[i] : -1;
            SetActiveSafe(s.Root.gameObject, used);
            if (used) RefreshSlot(s, ids[i]);
        }
        if (ids.Count > SLOT_COUNT)
            Debug.Log($"[ClassPopup] 직업 능력 {ids.Count}개 — 슬롯({SLOT_COUNT}) 초과분 미표시");
    }

    // 직업 숙련도 게이지 — 만충 숫자만 #00FF00('lime' TMP 미지원), 미만 yellow. '/최대'는 흰색 유지.
    void RefreshProficiency()
    {
        var txt = GetText((int)Texts.MemoryAmountTxt_1);
        if (txt == null) return;
        int prof = Managers.Game != null ? Managers.Game.ClassProficiency : 0;
        string col = prof >= CLASS_PROFICIENCY_MAX ? "#00FF00" : "yellow";
        txt.color = Color.white;
        txt.text = $"<color={col}>{prof}</color>/{CLASS_PROFICIENCY_MAX}";
        ApplyFont(txt, FontType.UI);
    }

    // 슬롯 표시 — 시각 규칙은 UI_SkillBookSkill과 동일. 단 직업창은 미보유(축성 대상)도 노출 → 잠금 오버레이 사용.
    void RefreshSlot(Slot s, int id)
    {
        var d = Managers.Data.SkillDic.TryGetValue(id, out var sd) ? sd : null;
        bool owned    = Managers.Ability.IsLearned(id);
        bool talent   = Managers.Ability.IsTalent(id);
        bool equipped = Managers.Ability.IsEquipped(id);
        bool active   = owned && (talent || equipped);   // 재능 or 기억중
        int lv    = Mathf.Max(1, Managers.Ability.GetLevel(id));
        int maxLv = d != null ? Mathf.Max(1, d.MaxLevel) : lv;
        bool maxed = owned && lv >= maxLv;

        // 능력 아이콘 (IconLabel 있을 때만, 없으면 prefab 기본)
        if (s.IconBtn != null && d != null && !string.IsNullOrEmpty(d.IconLabel))
        {
            var img = s.IconBtn.GetComponent<Image>();
            var sp = Managers.Resource.Load<Sprite>(d.IconLabel);
            if (img != null && sp != null) { img.sprite = sp; img.preserveAspect = true; }
        }

        // 미보유 = 잠금 오버레이 (능력 목록과 달리 축성 대상이므로 목록에 노출)
        SetActiveSafe(s.Locked, !owned);
        SetActiveSafe(s.ActiveEf, active);

        // 기억중 배지 (재능/기억중)
        SetActiveSafe(s.MemBadge, active);
        if (active && s.MemTxt != null)
        {
            s.MemTxt.text = Util.Localize(LocalizeKey.Memorized_Text_Short);
            ApplyFont(s.MemTxt, FontType.UI);
        }

        // 레벨(단계) — 보유 시에만. 색 규칙은 UI_SkillBookSkill과 동일(<최대=흰/노랑, =최대=magenta 계열).
        SetActiveSafe(s.LvlGroup, owned);
        if (owned)
        {
            if (s.LvlFrame != null) s.LvlFrame.color = maxed ? Util.HexToColor("FF00F5") : Color.white;
            if (s.LvlBg != null)    s.LvlBg.color    = maxed ? Util.HexToColor("3C143A") : Util.HexToColor("000000");
            if (s.LvlTxt != null)
            {
                string col = maxed ? "#FF00FF" : "#B9B900";
                s.LvlTxt.color = Color.white;
                s.LvlTxt.text = $"<color={col}>{lv}</color>{Util.Localize(LocalizeKey.LevelText)}";
                ApplyFont(s.LvlTxt, FontType.UI);
            }
        }
    }

    // 슬롯 클릭 — 보유:정보창 / 미보유&만충:축성 / 미보유&부족:토스트
    void OnClickSlot(int idx)
    {
        int id = _slotIds[idx];
        if (id < 0) return;

        if (Managers.Ability.IsLearned(id))
        {
            Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(id);
            return;
        }

        if (Managers.Ability.CanAnoint(id))
        {
            // 축성(습득) — 숙련도 100 소진. 표시 갱신은 OnAbilityChanged/OnClassProficiencyChanged 구독이 처리.
            if (Managers.Ability.TryAnoint(id))
            {
                var anim = _slots[idx].Root.GetComponent<DOTweenAnimation>();
                if (anim != null) anim.DORestart();   // 슬롯 authored PunchScale
                Managers.Game?.Player?.RefreshStats(fullHeal: false);   // 재능(점유0)은 습득 즉시 적용
            }
            return;
        }

        // 숙련도 부족 (키는 LocalizationData 시트 소관 — Localize는 키 부재 시 키 문자열 반환)
        Managers.UI.ShowToast(Util.Localize("Lack_Proficiency_Text"));
    }

    // 슬롯 템플릿 7개의 내부 노드 캐싱 + 클릭 바인딩 (Refresh마다 재바인딩하면 람다가 중복 누적되므로 1회만)
    void CacheSlots()
    {
        var content = GetObject((int)GameObjects.Content)?.transform;
        if (content == null) return;

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            _slotIds[i] = -1;
            var root = content.Find($"SkillBookSkill_{i}");
            if (root == null) { Debug.Log($"[ClassPopup] 슬롯 템플릿 누락: SkillBookSkill_{i}"); continue; }

            var s = new Slot { Root = root };
            var btnT   = FindByPrefix(root, "Btn_");
            var lockT  = FindByPrefix(root, "Img_LockedIcon_");
            var memI   = FindByPrefix(root, "Img_MemorizedIcon_");
            var memT   = FindByPrefix(root, "Txt_MemorizedIcon_");
            var lvlI   = FindByPrefix(root, "Img_LvlIcon_");
            var lvlT   = FindByPrefix(root, "Txt_LvlIcon_");
            var panelT = FindByPrefix(root, "Panel_Frame_");

            s.IconBtn  = btnT != null ? btnT.gameObject : null;
            s.Locked   = lockT != null ? lockT.parent.gameObject : null;
            s.ActiveEf = panelT != null ? panelT.gameObject : null;
            s.MemBadge = memI != null ? memI.parent.gameObject : null;
            s.MemTxt   = memT != null ? memT.GetComponent<TMP_Text>() : null;
            if (lvlI != null)
            {
                s.LvlGroup = lvlI.parent.gameObject;
                s.LvlFrame = lvlI.GetComponent<Image>();
                s.LvlBg    = lvlI.parent.GetComponent<Image>();
            }
            s.LvlTxt = lvlT != null ? lvlT.GetComponent<TMP_Text>() : null;
            _slots[i] = s;

            if (s.IconBtn != null)
            {
                int idx = i;
                s.IconBtn.BindEvent(() => OnClickSlot(idx));
            }
        }
    }

    // 접미사 난수가 붙은 템플릿 내부 노드를 prefix로 탐색 (슬롯마다 노드명이 달라 enum 바인딩 불가)
    static Transform FindByPrefix(Transform root, string prefix)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t != root && t.name.StartsWith(prefix)) return t;
        return null;
    }

    static void SetActiveSafe(GameObject go, bool on)
    {
        if (go != null && go.activeSelf != on) go.SetActive(on);
    }
}
