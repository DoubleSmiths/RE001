using System.Collections.Generic;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class UI_BestiaryPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        BestiaryPopupFrame,
        BestiaryCompletionBox,
        AllSStarImage,
        HybridKinshipSkillFrame,
        HybridKinshipSkillBackground,
        HybridKinshipSkillLock,
        HybridKinshipSkillLockIcon,
        HybridKinshipSkillLevel,
        HybridKinshipSkillLevelFrame,
        MasterHandlerSkillFrame,
        MasterHandlerSkillBackground,
        MasterHandlerSkillLock,
        MasterHandlerSkillLockIcon,
        MasterHandlerSkillLevel,
        MasterHandlerSkillLevelFrame,
        RetrievionSkillFrame,
        RetrievionSkillBackground,
        RetrievionSkillLock,
        RetrievionSkillLockIcon,
        RetrievionSkillLevel,
        RetrievionSkillLevelFrame,
        MemoryExpansionSkillFrame,
        MemoryExpansionSkillBackground,
        MemoryExpansionSkillLock,
        MemoryExpansionSkillLockIcon,
        MemoryExpansionSkillLevel,
        MemoryExpansionSkillLevelFrame,
        BestiaryCompletionTitleFrame,
        Viewport,
        ScrollbarVertical,
        Handle,
        MinimonButton,
        MinimonTitleFrame,
        MinimonMask,
        MinimonExistImg,
        MinimonEmoj,
        MinimonNoExistImg,
        MinimonPlayerImg,
        Xbutton,
    }

    enum Texts
    {
        BestiaryPopupTitleText, // 마물 도감
        CompletionLevelText, // 보라 별 아래 숫자. => 동적으로 변함.
        HybridKinshipSkillNewText,
        HybridKinshipSkillLevelText,
        MasterHandlerSkillNewText,
        MasterHandlerSkillLevelText,
        RetrievionSkillNewText,
        RetrievionSkillLevelText,
        MemoryExpansionSkillNewText,
        MemoryExpansionSkillLevelText,
        BestiaryCompletionTitleText, // -> 완성. 4 개 스킬에 있는 타이틀이름.
        MinimonTitleText, 
        MinimonAmountTxt, // 미니몬 포획 가능 횟수 -> 동적으로 변함.
        Txt_Xbutton_01,
    }

    enum Buttons
    {
        BestiaryPopupBackground,
        HybridKinshipSkillIcon,
        MasterHandlerSkillIcon,
        RetrievionSkillIcon,
        MemoryExpansionSkillIcon,
        MinimonBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        BestiaryPopup,
        HybridKinshipSkill,
        MasterHandlerSkill,
        RetrievionSkill,
        MemoryExpansionSkill,
        BestiaryInbox,
        Content,
        SlidingArea,
    }

    #endregion

    // 어떤것이 데이터가 필요한지, 엑셀화 해야됨, 근데 기존에 있는 데이터랑 잘 엮어야하는 지 아니면 새로운 데이터 테이블을 만들어야하는지 고민 필요.
    // 지금도 REGGION_STUB도 데이터에서 가져와야함.


    // ── 지역→몬스터 스텁 매핑 ──────────────────────────────
    // 정식 BestiaryData JSON 도입 전까지 사용. region 표시명 + 그 지역 몬스터 DataId 목록.
    // DataId가 CreatureDic에 없거나 0이면 data=null로 "?" 잠금 슬롯 처리된다.
    static readonly List<(string region, int[] monsterIds)> REGION_STUB = new()
    {
        ("신들바람 숲",   new[] { 2001, 2002 }),
        //("시들어가는 사막", new[] { 0, 0, 0, 0 }), // 미구현 지역: 잠금 슬롯 프리뷰
    };

    // ── 도감 표시 데이터 (MinMonManager 이관) ──
    // 기록도(분석률): 0=미발견("?"/Interactive X), 0~10미만=실루엣, 10이상=풀컬러.
    // 기록도/공명도/별/학습/AllSS/등급은 MinMonManager(실 데이터)로 이관됨.
    //   기록도 = KillCount/MaxKillCount, 공명도 = CaughtCount×25%, 등급 = MinMonSaveData.
    //   아래 static 메서드는 호출부 호환을 위해 시그니처만 유지하고 MinMonManager로 위임.
    //   Set* 는 테스트(BestiaryTestMenu) 시드용 — 실 데이터(KillCount/CaughtCount/등급)를 직접 세팅.
    public static int  GetRecordRate(int dataId) => Managers.MinMon != null ? Managers.MinMon.GetRecordRate(dataId) : 0;
    public static void SetRecordRate(int dataId, int pct)
    {
        if (Managers.MinMon == null) return;
        int max = Managers.Data.CreatureDic.TryGetValue(dataId, out var cd) ? Mathf.Max(1, cd.MaxKillCount) : 100;
        Managers.MinMon.DebugSetKillCount(dataId, Mathf.RoundToInt(Mathf.Clamp(pct, 0, 100) / 100f * max));
    }

    public static int  GetSync(int dataId) => Managers.MinMon != null ? Mathf.RoundToInt(Managers.MinMon.GetSyncRate(dataId) * 100f) : 0;
    public static void SetSync(int dataId, int pct)
    {
        if (Managers.MinMon == null) return;
        // 공명도 = CaughtCount × 25% → 역산해 CaughtCount 시드
        Managers.MinMon.DebugSetCaughtCount(dataId, Mathf.RoundToInt(Mathf.Clamp(pct, 0, 100) / 25f));
    }

    // 슬라이드 79: 공명도 100%(=4회 포획) 시 고유기술 영구 학습 — MinMonManager 위임(파생).
    public static bool IsUniqueSkillLearned(int dataId)
        => Managers.MinMon != null && Managers.MinMon.IsUniqueSkillLearned(dataId);
    public static void SetUniqueSkillLearned(int dataId, bool learned)
    {
        if (Managers.MinMon == null) return;
        // 학습=공명도100% 파생 → 테스트 시 CaughtCount 시드(4회=100%)
        if (learned) Managers.MinMon.DebugSetCaughtCount(dataId, Mathf.Max(4, (int)(Managers.MinMon.GetSyncRate(dataId) / MinMonManager.SYNC_PER_CATCH)));
        else         Managers.MinMon.DebugSetCaughtCount(dataId, 0);
    }

    // 고유기술 이름 — CreatureData.UniqueSkillName 우선, 비어있으면 폴백.
    public static string GetUniqueSkillName(int dataId)
    {
        if (Managers.Data != null
            && Managers.Data.CreatureDic.TryGetValue(dataId, out var cd)
            && !string.IsNullOrEmpty(cd.UniqueSkillName))
            return cd.UniqueSkillName;
        return $"고유기술 #{dataId % 1000:000}";
    }

    // ── 슬라이드 86/87: 보유 미니몬 능력치 (MinMonManager 이관) ──
    // 능력치 등급 0~5 (슬87, 6종류 = E~S). 등급/값은 MinMonManager가 보관.
    public enum MMStat { Atk, AtkSpeed, Crit }

    // 등급/값은 MinMonManager 위임. (Set* 는 테스트 시드용 — 실 등급 직접 세팅, 값은 파생이라 무시)
    public static int  GetMMGrade(int dataId, MMStat s)
    {
        if (Managers.MinMon == null) return 0;
        return s == MMStat.Atk ? Managers.MinMon.GetAtkGrade(dataId)
             : s == MMStat.AtkSpeed ? Managers.MinMon.GetAtkSpeedGrade(dataId)
             : Managers.MinMon.GetCritGrade(dataId);
    }
    public static void SetMMGrade(int dataId, MMStat s, int grade)
    {
        if (Managers.MinMon != null) Managers.MinMon.DebugSetGrade(dataId, (int)s, grade);
    }

    public static int  GetMMValue(int dataId, MMStat s)
    {
        if (Managers.MinMon == null) return 0;
        float v = s == MMStat.Atk ? Managers.MinMon.GetAtk(dataId)
                : s == MMStat.AtkSpeed ? Managers.MinMon.GetAtkSpeed(dataId)
                : Managers.MinMon.GetCritRate(dataId);
        return Mathf.RoundToInt(v);
    }
    public static void SetMMValue(int dataId, MMStat s, int v) { /* 값은 Base×(1+공명도) 파생 — 직접 세팅 불가(무시) */ }

    // 슬라이드 88: 동행 — GameData.CompanionMinMonId 위임 (없음: 스텁 0 / 실데이터 -1).
    public static int  GetFollowingId()
    {
        int id = Managers.Game != null ? Managers.Game.CompanionMinMonId : -1;
        return id < 0 ? 0 : id;
    }
    public static void SetFollowing(int dataId)
    {
        if (Managers.MinMon == null) return;
        if (dataId == 0) Managers.MinMon.RemoveCompanion();
        else             Managers.MinMon.SetCompanion(dataId);
    }

    // 슬라이드 80/83: AllSS / 별 단계 — MinMonManager 위임(파생). Set* 는 테스트 시드용.
    public static bool IsAllSS(int dataId) => Managers.MinMon != null && Managers.MinMon.IsAllSS(dataId);
    public static void SetAllSS(int dataId, bool v)
    {
        if (Managers.MinMon == null) return;
        // AllSS = 3등급 모두 S 파생 → 테스트 시 등급 직접 세팅
        int g = v ? MinMonManager.GRADE_MAX : 0;
        Managers.MinMon.DebugSetGrade(dataId, 0, g);
        Managers.MinMon.DebugSetGrade(dataId, 1, g);
        Managers.MinMon.DebugSetGrade(dataId, 2, g);
    }

    // 별 = rec100 + sync100 + AllSS 파생. RecomputeStar는 더 이상 저장 안 함(no-op).
    public static void RecomputeStar(int dataId) { /* 파생값 — 별도 계산/저장 불필요 */ }

    public static int  GetStarGrade(int dataId) => Managers.MinMon != null ? Managers.MinMon.GetStarGrade(dataId) : 0;
    // 별은 파생값이라 직접 세팅 불가 → 테스트 시 별 n단계가 되도록 하위 조건(기록도/공명도/등급) 시드
    public static void SetStarGrade(int dataId, int grade)
    {
        if (Managers.MinMon == null) return;
        grade = Mathf.Clamp(grade, 0, 3);
        SetRecordRate(dataId, grade >= 1 ? 100 : 0);   // 1단계: 기록도
        SetSync(dataId,       grade >= 2 ? 100 : 0);   // 2단계: 공명도
        SetAllSS(dataId,      grade >= 3);             // 3단계: 모든 등급 S
    }

    // ── 슬라이드 71: 완성도란 4개 스킬 (좌→우) ──
    // 0:종을 초월한 교감 1:조련사의 기술 2:오다 주웠습니다 주인님! 3:기억 확장
    // 시작 시 가장 오른쪽 "기억 확장"(idx3)만 해금, 나머지는 마물 도감으로만 해금.
    const int SKILL_COUNT = 4;
    static readonly string[] SKILL_NAMES =
        { "종을 초월한 교감", "조련사의 기술", "오다 주웠습니다 주인님!", "기억 확장" };
    const string SKILL_UNLOCK_PREFIX = "BESTIARY_SKILL_UNLOCK_";
    const string SKILL_GRADE_PREFIX  = "BESTIARY_SKILL_GRADE_";

    // 기본 해금: idx3(기억 확장)만 true
    public static bool GetSkillUnlocked(int idx)
        => PlayerPrefs.GetInt(SKILL_UNLOCK_PREFIX + idx, idx == SKILL_COUNT - 1 ? 1 : 0) == 1;
    public static void SetSkillUnlocked(int idx, bool unlocked)
    {
        PlayerPrefs.SetInt(SKILL_UNLOCK_PREFIX + idx, unlocked ? 1 : 0);
        PlayerPrefs.Save();
    }
    public static int  GetSkillGrade(int idx) => PlayerPrefs.GetInt(SKILL_GRADE_PREFIX + idx, 0);
    public static void SetSkillGrade(int idx, int grade)
    {
        PlayerPrefs.SetInt(SKILL_GRADE_PREFIX + idx, Mathf.Max(0, grade));
        PlayerPrefs.Save();
    }

    // ── 슬라이드 74: 등급 상승 연출 관련 상태 ──
    // NEW 뱃지: 해금 시 켜고, 기술 상세창을 한 번 보면 끈다.
    const string SKILL_NEW_PREFIX = "BESTIARY_SKILL_NEW_";
    public static bool GetSkillNew(int idx) => PlayerPrefs.GetInt(SKILL_NEW_PREFIX + idx, 0) == 1;
    public static void SetSkillNew(int idx, bool isNew)
    {
        PlayerPrefs.SetInt(SKILL_NEW_PREFIX + idx, isNew ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 좌→우 순환 포인터: 다음에 등급 오를 스킬 idx (슬라이드 71 #4)
    const string SKILL_ROT_PTR = "BESTIARY_SKILL_ROTPTR";
    // 완성도 숫자: 보라(3단계) 달성 누적 수
    const string COMPLETION_KEY = "BESTIARY_COMPLETION";
    public static int  GetCompletion() => PlayerPrefs.GetInt(COMPLETION_KEY, 0);
    public static void SetCompletion(int v)
    {
        PlayerPrefs.SetInt(COMPLETION_KEY, Mathf.Max(0, v));
        PlayerPrefs.Save();
    }

    // 슬라이드 71 #4: 보라 달성 1건당 좌→우 순환으로 다음 스킬 +1등급. 잠금이면 해금(Lv0→Lv1)+NEW.
    static int AdvanceSkillGrade(out bool newlyUnlocked)
    {
        int ptr = PlayerPrefs.GetInt(SKILL_ROT_PTR, 0) % SKILL_COUNT;
        newlyUnlocked = !GetSkillUnlocked(ptr);
        if (newlyUnlocked)
        {
            SetSkillUnlocked(ptr, true);
            SetSkillNew(ptr, true);
        }
        SetSkillGrade(ptr, GetSkillGrade(ptr) + 1);
        PlayerPrefs.SetInt(SKILL_ROT_PTR, (ptr + 1) % SKILL_COUNT);
        PlayerPrefs.Save();
        return ptr;
    }

    public static void ResetSkillRotation()
    {
        PlayerPrefs.SetInt(SKILL_ROT_PTR, 0);
        PlayerPrefs.SetInt(COMPLETION_KEY, 0);
        for (int i = 0; i < SKILL_COUNT; i++) SetSkillNew(i, false);
        PlayerPrefs.Save();
    }

    // 스킬 idx별 prefab 노드 enum 매핑 (좌→우 순서)
    static readonly int[] SKILL_ICON_BTN =
    {
        (int)Buttons.HybridKinshipSkillIcon, (int)Buttons.MasterHandlerSkillIcon,
        (int)Buttons.RetrievionSkillIcon,    (int)Buttons.MemoryExpansionSkillIcon,
    };
    static readonly int[] SKILL_LOCK =
    {
        (int)Images.HybridKinshipSkillLock, (int)Images.MasterHandlerSkillLock,
        (int)Images.RetrievionSkillLock,    (int)Images.MemoryExpansionSkillLock,
    };
    static readonly int[] SKILL_LOCK_ICON =
    {
        (int)Images.HybridKinshipSkillLockIcon, (int)Images.MasterHandlerSkillLockIcon,
        (int)Images.RetrievionSkillLockIcon,    (int)Images.MemoryExpansionSkillLockIcon,
    };
    static readonly int[] SKILL_LEVEL_TEXT =
    {
        (int)Texts.HybridKinshipSkillLevelText, (int)Texts.MasterHandlerSkillLevelText,
        (int)Texts.RetrievionSkillLevelText,    (int)Texts.MemoryExpansionSkillLevelText,
    };
    static readonly int[] SKILL_NEW_TEXT =
    {
        (int)Texts.HybridKinshipSkillNewText, (int)Texts.MasterHandlerSkillNewText,
        (int)Texts.RetrievionSkillNewText,    (int)Texts.MemoryExpansionSkillNewText,
    };

    readonly Tween[] _skillIdleTweens = new Tween[SKILL_COUNT];

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        _frameRT = GetObject((int)GameObjects.BestiaryPopup).GetComponent<RectTransform>();
        GetButton((int)Buttons.BestiaryPopupBackground).gameObject.BindEvent(OnClickBG);
        GetImage((int)Images.Xbutton).gameObject.BindEvent(OnClickBG);

        for (int i = 0; i < SKILL_COUNT; i++)
        {
            int idx = i; // 클로저 캡처
            GetButton(SKILL_ICON_BTN[i]).gameObject.BindEvent(() => OnClickSkill(idx));
        }

        // 슬28~29: [미니몬 잡기!] 버튼 — 포획 가능 창고의 미니몬으로 잡기 팝업 진입
        GetButton((int)Buttons.MinimonBtn).gameObject.BindEvent(OnClickMinimon);
        if (Managers.MinMon != null)
            Managers.MinMon.OnInterestChanged += RefreshMinimonButton;

        return true;
    }

    public void SetInfo()
    {
        var title = GetText((int)Texts.BestiaryPopupTitleText);
        if (title != null)  
            ApplyFont(title, Define.FontType.UI);

        Transform content = GetObject((int)GameObjects.Content).transform;

        // prefab에 박혀있는 템플릿 UI_Bestiary 포함 기존 자식 제거
        for (int i = content.childCount - 1; i >= 0; i--)
            Managers.Resource.Destroy(content.GetChild(i).gameObject);
        //ClearChildren(content);
        
        foreach (var (region, monsterIds) in REGION_STUB)
        {
            var monsters = new List<CreatureData>();

            for (int i = 0; i < monsterIds.Length; i++)
            {
                int id = monsterIds[i];
                CreatureData data = null;
                if (id != 0)
                    Managers.Data.CreatureDic.TryGetValue(id, out data);

                monsters.Add(data);
            }

            UI_Bestiary bestiary = Managers.UI.MakeSubItem<UI_Bestiary>(content, pooling: false);
            bestiary.SetInfo(region, monsters, OnClickMonster);
        }

        RefreshCompletionNumber();
        RefreshSkills();
        RefreshMinimonButton();
    }

    // 슬28~29: [미니몬 잡기!] 버튼 상태. 포획 가능 창고(InterestedMinMons) 유무에 따라 변화.
    //  창고 1마리 이상: 타이틀 "!" + 슬라임 표시 + Emoj_Exclamation + interactable
    //  창고 비어있음  : 타이틀 "?" + 슬라임 숨김 + Emoj_Question   + non-interactable
    void RefreshMinimonButton()
    {
        int count = Managers.MinMon != null ? Managers.MinMon.InterestedMinMons.Count : 0;
        bool has = count > 0;

        var title = GetText((int)Texts.MinimonTitleText);
        if (title != null)
        {
            title.text = "미니몬 잡기" + (has ? "!" : "?");
            ApplyFont(title, Define.FontType.UI);
        }

        var amount = GetText((int)Texts.MinimonAmountTxt);
        if (amount != null)
        {
            amount.text = $"x{count}";
            ApplyFont(amount, Define.FontType.UI);
        }

        // 슬29: 슬라임(Slime_Idle) 표시 여부
        var slime = GetImage((int)Images.MinimonExistImg);
        if (slime != null) slime.gameObject.SetActive(has);
        var noSlime = GetImage((int)Images.MinimonNoExistImg);
        if (noSlime != null) noSlime.gameObject.SetActive(!has);

        // 슬29: Emoj_Exclamation(창고 있음) / Emoj_Question(없음)
        PlayMinimonEmoj(has ? "Emoj_Exclamation" : "Emoj_Question");

        // 슬29: Button interactive
        var btn = GetButton((int)Buttons.MinimonBtn);
        if (btn != null) btn.interactable = has;
    }

    void PlayMinimonEmoj(string state)
    {
        var emoj = GetImage((int)Images.MinimonEmoj);
        if (emoj == null) return;
        emoj.gameObject.SetActive(true);
        var anim = emoj.GetComponent<Animator>();
        if (anim != null) { anim.enabled = true; anim.Play(state, 0, 0f); }
    }

    // 슬28: [미니몬 잡기!] 클릭 → 창고 첫 미니몬으로 잡기 미니게임(UI_TamePopup) 진입
    void OnClickMinimon()
    {
        if (Managers.MinMon == null || Managers.MinMon.InterestedMinMons.Count == 0)
        {
            Debug.Log("[Bestiary] 포획 가능 창고 비어있음 — 잡기 불가");
            return;
        }
        int dataId = Managers.MinMon.InterestedMinMons[0];
        Managers.UI.ShowPopupUI<UI_TamePopup>().SetInfo(dataId);
    }

    // 완성도 보라별 밑 숫자 = 보라(3단계) 달성 누적 수
    void RefreshCompletionNumber()
    {
        var t = GetText((int)Texts.CompletionLevelText);
        if (t == null) return;
        t.text = GetCompletion().ToString();
        ApplyFont(t, Define.FontType.UI);

        // 슬라이드 70: "0" Outline FF00C4 / Alpha 132 / Distance (0.2, -0.2)
        var outline = t.GetComponent<Outline>();
        if (outline != null)
        {
            outline.effectColor = new Color32(0xFF, 0x00, 0xC4, 132);
            outline.effectDistance = new Vector2(0.2f, -0.2f);
        }
    }

    // 슬라이드 71: 4개 스킬 아이콘 잠금/해금 + "+n 등급" + idle Anim
    void RefreshSkills()
    {
        for (int i = 0; i < SKILL_COUNT; i++)
        {
            bool unlocked = GetSkillUnlocked(i);
            int  grade    = GetSkillGrade(i);

            // "+n 등급" — "+n"은 노란색(FFFF00), 항상 표기 (잠금 시엔 자물쇠가 위를 덮음)
            var lvlTxt = GetText(SKILL_LEVEL_TEXT[i]);
            if (lvlTxt != null)
            {
                lvlTxt.text = $"<color=yellow>+{grade}</color>등급";
                ApplyFont(lvlTxt, Define.FontType.UI);
            }

            // 잠금 오버레이(자물쇠 + 검은 Sprite)
            GetImage(SKILL_LOCK[i]).gameObject.SetActive(!unlocked);
            GetImage(SKILL_LOCK_ICON[i]).gameObject.SetActive(!unlocked);

            // NEW 뱃지: 해금된 스킬을 아직 안 봤을 때만 표시 (슬라이드 74)
            var newTxt = GetText(SKILL_NEW_TEXT[i]);
            if (newTxt != null)
            {
                bool showNew = unlocked && GetSkillNew(i);
                newTxt.gameObject.SetActive(showNew);
                if (showNew) ApplyFont(newTxt, Define.FontType.UI);
            }

            // 잠금 아이콘은 Interactive 불가
            var iconBtn = GetButton(SKILL_ICON_BTN[i]);
            if (iconBtn != null) iconBtn.interactable = unlocked;

            // 해금 아이콘 idle Anim (DOTween Punch/Rotation, Dur 1, OutQuad, Loops -1, TO z20, Vibrato 2) 이건 별 아이콘에 적용돼야함.
            //Transform iconTr = iconBtn != null ? iconBtn.transform : null;
            //_skillIdleTweens[i]?.Kill();
            //_skillIdleTweens[i] = null;
            //if (iconTr != null)
            //{
            //    iconTr.localRotation = Quaternion.identity;
            //    if (unlocked)
            //    {
            //        _skillIdleTweens[i] = iconTr
            //            .DOPunchRotation(new Vector3(0f, 0f, 20f), 1f, 2, 0f)
            //            .SetEase(Ease.OutQuad)
            //            .SetLoops(-1, LoopType.Restart)
            //            .SetUpdate(true);
            //    }
            //}
        }
    }

    void OnClickSkill(int idx)
    {
        if (!GetSkillUnlocked(idx))
        {
            // 잠금 상태: Interactive 불가 (슬라이드 71 #6)
            Debug.Log($"[Bestiary] 스킬 '{SKILL_NAMES[idx]}' 잠금 상태 — 클릭 무시");
            return;
        }

        // 본 스킬은 NEW 해제
        if (GetSkillNew(idx))
        {
            SetSkillNew(idx, false);
            var nt = GetText(SKILL_NEW_TEXT[idx]);
            if (nt != null) nt.gameObject.SetActive(false);
        }

        // 해금 상태: 해당 [기술 상세정보창] 호출 (슬라이드 71 #7)
        Debug.Log($"[Bestiary] 스킬 '{SKILL_NAMES[idx]}' 상세정보창 호출 (idx={idx})");
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(idx);
    }

    // ── 슬라이드 74: 별 등급 상승 연출 ──
    // 도감이 열려있는 상태에서 어떤 몬스터의 별 등급이 올랐을 때 호출.
    // newStar < 3 : 해당 미니몬 아이콘만 Punch/Scale
    // newStar == 3: 기술→미니몬→완성도별→숫자 0.1s 간격 순차 + 기술/미니몬에 Flash_USETHIS
    public void PlayGradeUp(int dataId, int newStar)
    {
        SetStarGrade(dataId, newStar);

        UI_BestiaryBox box = FindBox(dataId);
        box?.RefreshStarGrade();

        if (newStar < 3)
        {
            // 3단계 미만: 별 등급 오른 미니몬 아이콘만 Anim
            if (box != null) PunchScale(box.MonsterIconTr);
            return;
        }

        // 3단계(보라) 달성: 좌→우 순환 등급상승 + 완성도 누적
        int skillIdx = AdvanceSkillGrade(out bool _);
        SetCompletion(GetCompletion() + 1);

        // 비주얼 상태 먼저 갱신 (자물쇠 off / +n등급 / NEW / 숫자)
        RefreshSkills();
        RefreshCompletionNumber();

        Transform skillTr = GetButton(SKILL_ICON_BTN[skillIdx])?.transform;
        Transform starTr  = GetImage((int)Images.AllSStarImage)?.transform;
        Transform numTr   = GetText((int)Texts.CompletionLevelText)?.transform;
        Transform monTr   = box != null ? box.MonsterIconTr : null;

        // 0.1초 간격 순차: a.기술 b.미니몬 c.완성도 보라별 d.그 밑 숫자
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.AppendCallback(() => { PunchScale(skillTr); SpawnFlash(skillTr); });
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => { PunchScale(monTr); SpawnFlash(monTr); });
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => PunchScale(starTr));
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => PunchScale(numTr));
    }

    // 슬라이드 74 #3: DOTween Punch/Scale Dur 0.3 OutQuad TO(0.2,0.2,0) Vibrato 10 Elasticity 1
    void PunchScale(Transform tr)
    {
        if (tr == null) return;
        tr.DOKill();
        tr.localScale = Vector3.one;
        tr.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 10, 1f)
          .SetEase(Ease.OutQuad)
          .SetUpdate(true);
    }

    // 기술/미니몬 아이콘에 Flash_USETHIS.prefab 적용 (슬라이드 74)
    void SpawnFlash(Transform parent)
    {
        if (parent == null) return;
        GameObject fx = Managers.Resource.Instantiate("Flash_USETHIS", parent);
        if (fx == null) return;
        fx.transform.localPosition = Vector3.zero;
        fx.transform.localScale = Vector3.one;
        // 팝업은 timeScale 0이라 unscaled로 정리
        DOVirtual.DelayedCall(1f, () => { if (fx != null) Managers.Resource.Destroy(fx); }, true);
    }

    // Content 안 인스턴스화된 박스 중 dataId 일치하는 것 탐색
    UI_BestiaryBox FindBox(int dataId)
    {
        Transform content = GetObject((int)GameObjects.Content).transform;
        foreach (var b in content.GetComponentsInChildren<UI_BestiaryBox>(true))
            if (b.DataId == dataId) return b;
        return null;
    }

    void OnClickMonster(int dataId)
    {
        // 슬라이드 73 #: 몬스터 ICON 클릭 → [미니몬 기록] 창 호출 (슬라이드 76 = UI_MonsterDetailPopup)
        Debug.Log($"[Bestiary] 몬스터 클릭 dataId={dataId} → 미니몬 기록");
        Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(dataId); // 팝업이 중간에 안나오고 밑에 나오는 버그가 있음.
    }

    public override void OnDestroy()
    {
        if (Managers.MinMon != null)
            Managers.MinMon.OnInterestChanged -= RefreshMinimonButton;
        //for (int i = 0; i < _skillIdleTweens.Length; i++)
        //{
        //    _skillIdleTweens[i]?.Kill();
        //    _skillIdleTweens[i] = null;
        //}
        base.OnDestroy();
    }

}
