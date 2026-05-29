using System;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 마물 도감의 몬스터 한 칸. UI_Bestiary(지역) 안에서 몬스터 수만큼 동적 생성된다.
public class UI_BestiaryBox : UI_Base
{
    #region Enum

    enum Images
    {
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
        BestiaryBoxUnlockQustion,
        BestiaryBoxNumber,
    }

    enum Buttons
    {
        UI_BestiaryBox,
    }

    enum GameObjects
    {
        BestiaryBoxSelect,
    }

    #endregion


    int _dataId;
    bool _interactable;
    Action<int> _onClick;
    Tween _starIdleTween;

    public int DataId => _dataId;
    // 슬라이드 74 연출용 — 몬스터 아이콘 Transform
    public Transform MonsterIconTr => GetImage((int)Images.BeastiaryBoxMonsterExist).transform;

    // 별 등급만 다시 읽어 Star 표기 갱신 (전체 SetInfo 재호출 없이)
    public void RefreshStarGrade()
    {
        int star = _dataId != 0 ? UI_BestiaryPopup.GetStarGrade(_dataId) : 0;
        ApplyStarVisual(star);
    }

    // 슬라이드 83: 별 항시 Anim 재생상태 — Punch/Rotation Dur 1, OutQuad, Loops -1, z20, Vibrato 2.
    void ApplyStarVisual(int star)
    {
        var starImg = GetImage((int)Images.BestiaryBoxStar);
        bool show = star >= 1;
        starImg.gameObject.SetActive(show);

        _starIdleTween?.Kill();
        _starIdleTween = null;
        if (show)
        {
            Transform tr = starImg.transform;
            tr.localRotation = Quaternion.identity;
            _starIdleTween = tr.DOPunchRotation(new Vector3(0f, 0f, 20f), 1f, 2, 0f)
                               .SetEase(Ease.OutQuad)
                               .SetLoops(-1, LoopType.Restart)
                               .SetUpdate(true);
        }
    }

    public override void OnDestroy()
    {
        _starIdleTween?.Kill();
        _starIdleTween = null;
        base.OnDestroy();
    }

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        gameObject.BindEvent(OnClickBox);

        return true;
    }

    // 슬라이드 73: 기록도(분석률)에 따라 3분류
    //  - 기록도 0%      : "?" 잠금, 번호 "-", Interactive X
    //  - 기록도 0~10미만 : 몬스터 검정 실루엣, 번호 표기, Interactive O
    //  - 기록도 10이상   : 풀컬러 + Rarity 글로우, 번호 표기, Interactive O
    //  - 별등급 >= 1     : Star 표기
    public void SetInfo(CreatureData data, Action<int> onClick = null)
    {
        _dataId = data != null ? data.DataId : 0;
        _onClick = onClick;

        int rec  = data != null ? UI_BestiaryPopup.GetRecordRate(_dataId) : 0;
        int star = data != null ? UI_BestiaryPopup.GetStarGrade(_dataId) : 0;
        _interactable = data != null && rec > 0;

        var btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = _interactable;

        // 선택 하이라이트는 기본 숨김
        GetImage((int)Images.BestiaryBoxSelectActive).gameObject.SetActive(false);

        var numTxt = GetText((int)Texts.BestiaryBoxNumber);
        if (numTxt != null)
            ApplyFont(numTxt, Define.FontType.UI);

        var icon = GetImage((int)Images.BeastiaryBoxMonsterExist);
        var glow = GetImage((int)Images.BestiaryBoxGlow);
        var unlock = GetImage((int)Images.BestiaryBoxUnlock);
        var question = GetText((int)Texts.BestiaryBoxUnlockQustion);

        if (data == null || rec <= 0)
        {
            // 기록도 0% : 미발견
            if (numTxt != null) numTxt.text = "-";
            icon.gameObject.SetActive(false);
            ApplyStarVisual(0);
            glow.color = new Color32(15, 15, 15, 255);

            unlock.gameObject.SetActive(true);
            question.gameObject.SetActive(true);
            ApplyFont(question, Define.FontType.UI);
            return;
        }

        // 기록도 > 0 : 발견
        if (numTxt != null) numTxt.text = _dataId.ToString("000");
        unlock.gameObject.SetActive(false);
        question.gameObject.SetActive(false);

        icon.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(data.IconLabel))
        {
            Sprite sprite = Managers.Resource.Load<Sprite>(data.IconLabel);
            if (sprite != null)
            {
                icon.sprite = sprite;
                icon.preserveAspect = true;
            }
            // 스프라이트를 못 찾으면 prefab 기본 스프라이트 유지
        }

        glow.gameObject.SetActive(true);

        if (rec < 10)
        {
            // 기록도 10% 미만 : 검정 실루엣
            icon.color = new Color32(0, 0, 0, 255);
            glow.color = new Color32(15, 15, 15, 255);
        }
        else
        {
            // 기록도 10% 이상 : 풀컬러 + Rarity 글로우
            icon.color = Color.white;
            glow.color = GetRarityColor(data.Rarity);
        }

        // 슬라이드 83: 별 1단계 이상이면 표기 + Punch/Rotation 항시 Anim
        ApplyStarVisual(star);
    }

    // Rarity 문자열 → 글로우 색 (CreatureData.Rarity: "Common"/"Rare"/"Boss" 등)
    Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "Rare":   return Util.HexToColor("0064C8");
            case "Elite":  return Util.HexToColor("9600FF");
            case "Boss":   return Util.HexToColor("960E00");
            default:       return new Color32(255, 255, 255, 200); // Common
        }
    }

    void OnClickBox()
    {
        // 기록도 0%(미발견)면 Interactive X — BindEvent는 Button.interactable과 무관하므로 직접 가드
        if (!_interactable)
            return;

        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(new Vector3(-0.2f, 0.15f, 1f), 0.3f, 10, 1f)
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true);

        _onClick?.Invoke(_dataId);
    }
}
