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

    // ── 지역→몬스터 스텁 매핑 ──────────────────────────────
    // 정식 BestiaryData JSON 도입 전까지 사용. region 표시명 + 그 지역 몬스터 DataId 목록.
    // DataId가 CreatureDic에 없거나 0이면 data=null로 "?" 잠금 슬롯 처리된다.
    static readonly List<(string region, int[] monsterIds)> REGION_STUB = new()
    {
        ("신들바람 숲",   new[] { 2001, 2002 }),
        //("시들어가는 사막", new[] { 0, 0, 0, 0 }), // 미구현 지역: 잠금 슬롯 프리뷰
    };

    const string CAPTURE_PREF_PREFIX = "BESTIARY_CAP_";

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

        ApplyFont(GetText((int)Texts.CompletionLevelText), Define.FontType.UI);

    }

    void OnClickMonster(int dataId)
    {
        // TODO: 몬스터 상세 정보 팝업(UI_BestiaryDetailPopup) 연결
        Debug.Log($"[Bestiary] 몬스터 클릭 dataId={dataId}");
    }

}
