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
    Color _frameOrigColor = Color.white; // 슬6: 동행 frame색(FF00CB) 적용/복귀용 원색
    bool _frameColorCached;
    GameObject _activeEf; // 슬6: 동행 중 frame에 활성화되는 Active Ef.prefab 인스턴스

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
            // 슬14: 별 등급별 색 — 1단계 흰색(아웃라인 FF00FF) / 2단계 FFFF00 / 3단계 FF00FF.
            //  (아웃라인 FF00FF는 Mat: Outline_Mat 머티리얼 영역 — fill 색을 등급별로 적용)
            starImg.color = star >= 3 ? Util.HexToColor("FF00FF")
                          : star == 2 ? Util.HexToColor("FFFF00")
                          : Color.white;

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
        if (_activeEf != null) { Managers.Resource.Destroy(_activeEf); _activeEf = null; }
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

        // 슬6: 동행 frame색 적용 전 원래 frame 색 캐싱(복귀용)
        var fr = GetImage((int)Images.BestiaryBoxFrame);
        if (fr != null) { _frameOrigColor = fr.color; _frameColorCached = true; }

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

        // 기록도 > 0 : 발견. 슬3: Creature ID = id에서 앞 2 뺀 3자리 (2001 → "001"), 슬8 NO. 표기와 동일.
        if (numTxt != null) numTxt.text = (_dataId % 1000).ToString("000");
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
            // 슬4: 기록도 10% 미만 — 몬스터는 어두운 실루엣, 배경 글로우는 "분석중" 초록(슬라이드 샘플 #0B7500).
            //  ⚠️ 순검정(0,0,0)으로 두면 **칸 배경도 검정이라 형태가 하나도 안 보인다** — 처음 잡은
            //     몬스터 칸이 그냥 빈 검은 사각형으로 보였다. 도감 카드가 칸을 꽉 채우게 바뀐 뒤로 더 심해졌다.
            //     "아직 분석 안 됨"은 유지하되 윤곽은 읽히도록 살짝 띄운다.
            icon.color = new Color32(58, 64, 58, 255);
            glow.color = Util.HexToColor("0B7500");
        }
        else
        {
            // 기록도 10% 이상 : 풀컬러 + Rarity 글로우
            icon.color = Color.white;
            glow.color = GetRarityColor(data.Rarity);
        }

        // 슬라이드 83: 별 1단계 이상이면 표기 + Punch/Rotation 항시 Anim
        ApplyStarVisual(star);

        // 슬4 #6 / 슬6: 동행 중인 미니몬이면 도감 ICON frame에 Active Ef.prefab 활성 + frame 색 FF00CB.
        //  (동행 해제 시 원색 복귀 + Active Ef 제거)
        RefreshCompanionVisual();
    }

    // 슬6: 이 박스의 미니몬이 현재 동행(소환) 중인지에 따라 frame 색(FF00CB/원색) + Active Ef.prefab 토글.
    public void RefreshCompanionVisual()
    {
        var frame = GetImage((int)Images.BestiaryBoxFrame);
        if (frame == null) return;
        if (!_frameColorCached) { _frameOrigColor = frame.color; _frameColorCached = true; }

        bool isCompanion = _dataId != 0
                        && Managers.Game != null
                        && Managers.Game.CompanionMinMonId == _dataId;

        frame.color = isCompanion ? Util.HexToColor("FF00CB") : _frameOrigColor;

        if (isCompanion && _activeEf == null)
        {
            // 기획서 명칭 "Active Ef.prefab". 미등록 시 Instantiate가 에러로그를 남기므로 Load로 먼저 가드.
            if (Managers.Resource.Load<GameObject>("Active Ef") != null)
            {
                _activeEf = Managers.Resource.Instantiate("Active Ef", frame.transform);
                if (_activeEf != null)
                {
                    _activeEf.transform.localPosition = Vector3.zero;
                    _activeEf.transform.localScale = Vector3.one;
                }
            }
        }
        else if (!isCompanion && _activeEf != null)
        {
            Managers.Resource.Destroy(_activeEf);
            _activeEf = null;
        }
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
