using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// v2 유물 소환 연출(ADR0045) — 1회·10회 결과를 카드로 하나씩 보인다(빌더: Tools/Remake V2/Build Relic Summon Popup — 미션 창 틀을 복제).
//  카드: 등급 색 빛 · 아이콘 · 이름 · NEW/UP/MAX. 영웅부터 빛이 번지고, 전설은 빛줄기가 돌고 카드가 튀며 소리가 크다.
//  아래 줄: 전설 확정까지 남은 횟수(천장) · 최대 레벨 환급. [한 번 더]는 같은 소환을 다시(광고 소환은 없음).
//  배경을 누르면 남은 연출을 건너뛰고, 다 보인 뒤 누르면 닫는다.
public class UI_RelicSummonPopup : UI_Popup
{
    #region Enum
    enum Buttons
    {
        SummonPopupBackground,
        Btn_SummonAgain,
    }

    enum Texts
    {
        title,
        Txt_SummonAgain,
        Txt_SummonInfo,
    }

    enum GameObjects
    {
        SummonGrid,
        SummonCardTemplate,
    }
    #endregion

    const float CARD_STEP   = 0.12f;   // 카드 사이 간격(초)
    const float CARD_POP    = 0.28f;
    const float RAYS_TURN   = 4f;      // 전설 빛줄기 한 바퀴(초)
    const float SINGLE_SCALE = 1.7f;   // 1회 소환은 카드 하나를 크게

    Action _again;
    bool _revealing;
    Vector2 _cellSize;
    readonly List<GameObject> _cards = new List<GameObject>();
    readonly List<Tween> _tweens = new List<Tween>();

    void Awake() => Init();

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.SummonPopupBackground).gameObject.BindEvent(OnClickBackground);
        GetButton((int)Buttons.Btn_SummonAgain).gameObject.BindEvent(OnClickAgain);
        foreach (Texts t in Enum.GetValues(typeof(Texts))) ApplyFont(GetText((int)t), FontType.UI);
        GetObject((int)GameObjects.SummonCardTemplate).SetActive(false);
        _cellSize = GetObject((int)GameObjects.SummonGrid).GetComponent<GridLayoutGroup>().cellSize;
        return true;
    }

    public void SetInfo(List<RelicManager.Pull> pulls, Action again)
    {
        _again = again;
        foreach (var t in _tweens) t?.Kill();
        _tweens.Clear();
        foreach (var c in _cards) Destroy(c);
        _cards.Clear();

        var rm = Managers.Relic;
        GetText((int)Texts.title).text = Util.Localize("RelicSummonTitleText");
        var grid = GetObject((int)GameObjects.SummonGrid).GetComponent<GridLayoutGroup>();
        grid.cellSize = pulls.Count == 1 ? _cellSize * SINGLE_SCALE : _cellSize;

        var template = GetObject((int)GameObjects.SummonCardTemplate);
        int refund = 0, best = 0;
        for (int i = 0; i < pulls.Count; i++)
        {
            var pull = pulls[i];
            refund += pull.Refund;
            best = Mathf.Max(best, pull.Grade);
            var card = Instantiate(template, grid.transform);
            card.name = "SummonCard" + i;
            card.SetActive(true);
            foreach (var t in card.GetComponentsInChildren<TMP_Text>(true)) ApplyFont(t, FontType.UI);
            Fill(card, pull, rm.Level(pull.Id));
            _cards.Add(card);

            // 차례로 튀어나온다 — 영웅부터 소리, 전설은 빛줄기·흔들림
            float at = i * CARD_STEP;
            card.transform.localScale = Vector3.zero;
            _tweens.Add(card.transform.DOScale(1f, CARD_POP).SetEase(Ease.OutBack).SetDelay(at).SetUpdate(true).SetLink(card));
            if (pull.Grade >= RelicManager.LEGENDARY)
            {
                var rays = card.transform.Find("Img_SummonCardRays");
                rays.gameObject.SetActive(true);
                _tweens.Add(rays.DORotate(new Vector3(0f, 0f, -360f), RAYS_TURN, RotateMode.FastBeyond360)
                                .SetEase(Ease.Linear).SetLoops(-1).SetUpdate(true).SetLink(card));
                _tweens.Add(card.transform.DOPunchScale(Vector3.one * 0.25f, 0.4f, 6, 0.5f).SetDelay(at + CARD_POP).SetUpdate(true).SetLink(card));
            }
            if (pull.Grade >= 2 || i == 0)
            {
                string sfx = pull.Grade >= RelicManager.LEGENDARY ? "SFX_Rankup" : pull.Grade >= 2 ? "SFX_Upgrade" : "SFX_ItemPopup";
                _tweens.Add(DOVirtual.DelayedCall(at, () => Managers.Sound?.PlaySFX(sfx)).SetUpdate(true).SetLink(card));
            }
        }
        _revealing = pulls.Count > 1;
        _tweens.Add(DOVirtual.DelayedCall(pulls.Count * CARD_STEP + CARD_POP, () => _revealing = false).SetUpdate(true).SetLink(gameObject));

        // 천장 · 환급
        string info = string.Format(Util.Localize("RelicSummonPityFormat"), Mathf.Max(0, rm.PityLimit - rm.Pity));
        if (refund > 0) info += "\n" + string.Format(Util.Localize("RelicSummonRefundFormat"), refund);
        GetText((int)Texts.Txt_SummonInfo).text = info;

        // 한 번 더 — 광고 소환은 하루 한 번이라 없다
        var againBtn = GetButton((int)Buttons.Btn_SummonAgain).gameObject;
        againBtn.SetActive(again != null);
        if (again != null)
        {
            int cost = pulls.Count >= 10 ? rm.CostTen : rm.CostOne;
            var txt = GetText((int)Texts.Txt_SummonAgain);
            txt.text = string.Format(Util.Localize("RelicSummonAgainFormat"), pulls.Count, cost);
            txt.color = Managers.Game.Crystal >= cost ? Color.white : new Color32(0x8A, 0x8A, 0x8A, 0xFF);
        }
    }

    void Fill(GameObject card, RelicManager.Pull pull, int level)
    {
        Managers.Data.RelicDic.TryGetValue(pull.Id, out var relic);
        Color grade = UI_MissionPopup.GRADE_COLORS[Mathf.Clamp(pull.Grade, 0, RelicManager.GRADE_COUNT - 1)];
        var icon = card.transform.Find("Img_SummonCardBg/Img_SummonCardIcon").GetComponent<Image>();
        icon.sprite = relic != null ? Managers.Resource.Load<Sprite>(relic.IconLabel) : null;
        var glow = card.transform.Find("Img_SummonCardGlow").GetComponent<Image>();
        glow.gameObject.SetActive(pull.Grade >= 1);
        glow.color = new Color(grade.r, grade.g, grade.b, pull.Grade >= 2 ? 1f : 0.6f);
        var name = card.transform.Find("Txt_SummonCardName").GetComponent<TMP_Text>();
        name.text = relic != null ? Util.Localize(relic.NameKey) : "";
        name.color = grade;
        var badge = card.transform.Find("Txt_SummonCardBadge").GetComponent<TMP_Text>();
        badge.text = pull.IsNew ? "<color=#FFE06A>NEW</color>" : pull.Refund > 0 ? "<color=#8A8A8A>MAX</color>" : $"<color=#7FE0FF>Lv{level}</color>";
    }

    // 연출 중이면 건너뛰기(모든 카드를 제자리로), 끝났으면 닫기
    void OnClickBackground()
    {
        if (_revealing)
        {
            _revealing = false;
            foreach (var t in _tweens)
                if (t != null && t.IsActive() && !(t.Loops() == -1)) t.Complete();
            return;
        }
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickAgain()
    {
        var again = _again;
        Managers.UI.ClosePopupUI(this);
        again?.Invoke();
    }
}
