using Data;
using UnityEngine;

// 미니몬 기록(UI_MonsterDetailPopup)의 전리품 한 칸. 드롭 아이템 수만큼 동적 생성.
// 슬라이드 77: 드랍 아이콘 + 확률/개수. 슬라이드 78: 기록도 미해금 시 "?" + Glow 0F0F0F.
public class UI_MonsterItemDropListBox : UI_Base
{
    #region Enum

    enum Images
    {
        MonsterItemDropListBoxMask,
        MonsterItemDropListBoxFrame,
        MonsterItemDropListBoxGlow,
        MonsterItemDropListBoxItemIcon,
    }

    enum Texts
    {
        MonsterItemDropListBoxQuestionText, // 미해금 시 "?"
        MonsterItemDropListBoxProbText,     // 드랍 확률(+개수)
    }

    enum Buttons
    {
        UI_MonsterItemDropListBox,
    }

    enum GameObjects
    {
    }

    #endregion

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        return true;
    }

    // item: 드롭 아이템 데이터 / count: 드랍 개수 / chance: 0~1 확률 / unlocked: 기록도로 해금됐는지
    public void SetInfo(ItemData item, int count, float chance, bool unlocked)
    {
        var icon = GetImage((int)Images.MonsterItemDropListBoxItemIcon);
        var glow = GetImage((int)Images.MonsterItemDropListBoxGlow);
        var q    = GetText((int)Texts.MonsterItemDropListBoxQuestionText);
        var prob = GetText((int)Texts.MonsterItemDropListBoxProbText);

        bool show = unlocked && item != null;

        if (!show)
        {
            // 슬라이드 78: 기록도 미해금 — 아이콘을 "?"로 교체, Glow 0F0F0F, 확률/개수 표기 X
            if (icon != null) icon.gameObject.SetActive(false);
            if (glow != null) glow.color = Util.HexToColor("0F0F0F");
            if (q != null)
            {
                q.gameObject.SetActive(true);
                q.text = "?";
                ApplyFont(q, Define.FontType.UI);
            }
            if (prob != null) prob.text = "";
            return;
        }

        if (q != null) q.gameObject.SetActive(false);

        if (icon != null)
        {
            icon.gameObject.SetActive(true);
            Sprite sprite = string.IsNullOrEmpty(item.SpriteName)
                ? null : Managers.Resource.Load<Sprite>(item.SpriteName);
            if (sprite != null)
            {
                icon.sprite = sprite;
                icon.preserveAspect = true;
            }
        }

        if (glow != null) glow.color = Util.GetGradeColor(item.Grade);

        if (prob != null)
        {
            // 확률(+개수) 표기
            string c = count > 1 ? $"x{count} " : "";
            prob.text = $"{c}{chance * 100f:0.#}%";
            ApplyFont(prob, Define.FontType.UI);
        }
    }
}
