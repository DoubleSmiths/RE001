using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [액티브 스킬바] 전투 기록 아래 빈 공간에 직접 사용하는 능력 6칸을 표시한다.
//  - 슬롯 구성 = AbilityRunner.ActiveSkillIds (장착된 능력 중 TriggerType=Active)
//  - 누르면 AbilityRunner.UseSkill → 효과/VFX는 확률·시간 발동과 **같은 경로**(ApplyEffect)를 탄다.
//  - 쿨타임은 아이콘 위 어두운 막이 아래로 걷히는 방식 + 남은 초 표기.
//
//  ponytail: 프리팹 대신 코드로 짓는다. 6칸 아이콘+게이지가 전부라 프리팹 저작/바인딩을
//            새로 들일 값어치가 없고, 이 프로젝트에서 "안 되는" 원인 대부분이 저작값이었다.
public class UI_SkillBar : MonoBehaviour
{
    class Slot
    {
        public GameObject root;
        public Button button;
        public Image icon;
        public Image frame;
        public Image cooldownMask;   // 남은 비율만큼 위에서 덮는다.
        public TMP_Text remainText;
        public int skillId = -1;
        public int shownRemain = -1;  // 남은 초가 바뀔 때만 다시 쓴다(매 프레임 ToString 가비지 방지)
    }

    static readonly Color COLOR_READY = Color.white;
    static readonly Color COLOR_COOL = new Color(0.7f, 0.7f, 0.7f, 1f);
    static readonly Color COLOR_MASK = new Color(0f, 0f, 0f, 0.62f);

    readonly List<Slot> _slots = new();
    AbilityRunner _runner;
    CanvasGroup _canvasGroup;
    TMP_Text _statusText;
    int _selectedSkillId = -1;
    bool _hasSkills;
    string _statusKey;

    public static UI_SkillBar Create(Transform parent, AbilityRunner runner)
    {
        var go = new GameObject("SkillBar", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, ACTIVE_SKILL_BAR_Y);
        rt.sizeDelta = new Vector2(ACTIVE_SKILL_SLOTS * ACTIVE_SKILL_SLOT_SIZE + (ACTIVE_SKILL_SLOTS - 1) * ACTIVE_SKILL_SLOT_GAP, ACTIVE_SKILL_SLOT_SIZE);

        var bar = go.AddComponent<UI_SkillBar>();
        bar.Build(runner);
        return bar;
    }

    void Build(AbilityRunner runner)
    {
        _runner = runner;
        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;

        float step = ACTIVE_SKILL_SLOT_SIZE + ACTIVE_SKILL_SLOT_GAP;
        float x0 = -(ACTIVE_SKILL_SLOTS - 1) * step * 0.5f;
        float barWidth = GetComponent<RectTransform>().rect.width;
        var backdrop = NewImage(transform, "Backdrop", barWidth, ACTIVE_SKILL_SLOT_SIZE + ACTIVE_SKILL_STATUS_HEIGHT);
        backdrop.rectTransform.anchoredPosition = new Vector2(0f, ACTIVE_SKILL_STATUS_HEIGHT * 0.5f);
        backdrop.color = new Color(0.13f, 0.10f, 0.08f, 1f);
        backdrop.raycastTarget = false;
        _statusText = NewText(transform, "AbilityStatusText", barWidth, ACTIVE_SKILL_STATUS_HEIGHT, ACTIVE_SKILL_STATUS_FONT_SIZE);
        _statusText.rectTransform.anchoredPosition = new Vector2(0f, (ACTIVE_SKILL_SLOT_SIZE + ACTIVE_SKILL_STATUS_HEIGHT) * 0.5f);
        _statusText.enableAutoSizing = true;
        _statusText.fontSizeMin = ACTIVE_SKILL_STATUS_FONT_SIZE - 1f;
        _statusText.fontSizeMax = ACTIVE_SKILL_STATUS_FONT_SIZE;
        _statusText.textWrappingMode = TextWrappingModes.NoWrap;
        _statusText.overflowMode = TextOverflowModes.Ellipsis;
        var frameSprite = Managers.Resource.Load<Sprite>(ACTIVE_SKILL_FRAME_SPRITE);

        for (int i = 0; i < ACTIVE_SKILL_SLOTS; i++)
        {
            var slot = new Slot();

            slot.root = NewChild(transform, $"SkillSlot_{i}", ACTIVE_SKILL_SLOT_SIZE, ACTIVE_SKILL_SLOT_SIZE);
            slot.root.GetComponent<RectTransform>().anchoredPosition = new Vector2(x0 + i * step, 0f);

            // 바탕(눌리는 대상)
            var bg = slot.root.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.12f, 0.08f, 1f);
            slot.button = slot.root.AddComponent<Button>();
            slot.button.targetGraphic = bg;

            slot.icon = NewImage(slot.root.transform, "Icon", ACTIVE_SKILL_SLOT_SIZE - ACTIVE_SKILL_ICON_INSET, ACTIVE_SKILL_SLOT_SIZE - ACTIVE_SKILL_ICON_INSET);
            slot.icon.raycastTarget = false;
            slot.icon.preserveAspect = true;

            slot.cooldownMask = NewImage(slot.root.transform, "CooldownMask", ACTIVE_SKILL_SLOT_SIZE, ACTIVE_SKILL_SLOT_SIZE);
            slot.cooldownMask.color = COLOR_MASK;
            slot.cooldownMask.raycastTarget = false;
            // 스프라이트 없는 Filled Image는 fillAmount를 무시한다. 실제 높이를 줄여 준비 상태에서 완전히 걷는다.
            slot.cooldownMask.rectTransform.anchorMin = slot.cooldownMask.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            slot.cooldownMask.rectTransform.pivot = new Vector2(0.5f, 1f);

            slot.frame = NewImage(slot.root.transform, "Frame", ACTIVE_SKILL_SLOT_SIZE, ACTIVE_SKILL_SLOT_SIZE);
            slot.frame.sprite = frameSprite;
            slot.frame.raycastTarget = false;
            slot.frame.transform.SetAsFirstSibling();
            // 마스크의 높이가 변해도 숫자는 버튼 중앙에 고정한다.
            slot.remainText = NewText(slot.root.transform, "RemainTxt", ACTIVE_SKILL_SLOT_SIZE, ACTIVE_SKILL_SLOT_SIZE, ACTIVE_SKILL_COOLDOWN_FONT_SIZE);

            int idx = i;
            slot.root.BindEvent(() => OnClickSlot(idx));
            _slots.Add(slot);
        }

        if (_runner != null) _runner.OnActiveSkillChanged += Refresh;
        if (Managers.Ability != null) Managers.Ability.OnAbilityChanged += Refresh;
        Managers.Game.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (_runner != null) _runner.OnActiveSkillChanged -= Refresh;
        if (Managers.Ability != null) Managers.Ability.OnAbilityChanged -= Refresh;
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= Refresh;
    }

    static GameObject NewChild(Transform parent, string name, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
        return go;
    }

    static Image NewImage(Transform parent, string name, float w, float h)
        => NewChild(parent, name, w, h).AddComponent<Image>();

    static TMP_Text NewText(Transform parent, string name, float width, float height, float fontSize)
    {
        var text = NewChild(parent, name, width, height).AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.raycastTarget = false;
        text.font = Managers.Font.GetFont(FontType.UI);
        return text;
    }

    // ── 슬롯 구성 갱신 (능력 장착/해제, 쿨 완료) ──
    public void Refresh()
    {
        var ids = _runner != null ? _runner.ActiveSkillIds : new List<int>();
        _hasSkills = ids.Count > 0;
        if (!ids.Contains(_selectedSkillId)) _selectedSkillId = _hasSkills ? ids[0] : -1;
        _statusKey = null;
        _statusText.font = Managers.Font.GetFont(FontType.UI);

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            bool has = i < ids.Count;
            slot.skillId = has ? ids[i] : -1;
            slot.root.SetActive(has);
            if (!has) continue;

            // 첫 동행 한 칸도 화면 중앙에서 찾을 수 있게 현재 능력 수로 정렬한다.
            slot.root.GetComponent<RectTransform>().anchoredPosition =
                new Vector2((i - (ids.Count - 1) * 0.5f) * (ACTIVE_SKILL_SLOT_SIZE + ACTIVE_SKILL_SLOT_GAP), 0f);

            if (Managers.Data.SkillDic.TryGetValue(slot.skillId, out var d))
                slot.icon.sprite = Managers.Resource.Load<Sprite>(d.IconLabel);
            // 아이콘이 없으면 빈 칸이 아니라 흰 사각으로 남아 "뭔가 있다"는 건 보이게 둔다.
            slot.icon.enabled = slot.icon.sprite != null;
            slot.remainText.font = Managers.Font.GetFont(FontType.UI);
        }
        Update();
    }

    void OnClickSlot(int index)
    {
        if (index >= _slots.Count) return;
        var slot = _slots[index];
        if (slot.skillId < 0 || _runner == null || !_canvasGroup.blocksRaycasts) return;
        _selectedSkillId = slot.skillId;
        _statusKey = null;
        _runner.UseSkill(slot.skillId);   // 쿨/사망/전투밖이면 내부에서 걸러진다
        Update();
    }

    void Update()
    {
        if (_runner == null) return;
        var player = Managers.Game.Player;
        bool visible = _hasSkills && player != null && player.InBattle;
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.blocksRaycasts = visible && Managers.UI.GetPopupCount() == 0;
        _canvasGroup.interactable = _canvasGroup.blocksRaycasts;
        if (!visible) return;

        foreach (var slot in _slots)
        {
            if (slot.skillId < 0 || !slot.root.activeSelf) continue;

            float remain = _runner.GetCooldownRemain(slot.skillId);
            bool ready = remain <= 0f && _runner.CanUse(slot.skillId);

            float ratio = 1f - _runner.GetCooldownRatio(slot.skillId);
            slot.cooldownMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ACTIVE_SKILL_SLOT_SIZE * ratio);
            slot.cooldownMask.enabled = remain > 0f;
            slot.icon.color = ready ? COLOR_READY : COLOR_COOL;
            slot.frame.color = slot.skillId == _selectedSkillId ? COLOR_READY : COLOR_COOL;
            int secs = remain > 0f ? Mathf.CeilToInt(remain) : 0;
            if (secs != slot.shownRemain)
            {
                slot.shownRemain = secs;
                if (secs > 0) slot.remainText.SetText("{0}", secs);
                else slot.remainText.text = "";
            }
        }
        RefreshStatus(player);
    }

    void RefreshStatus(PlayerController player)
    {
        if (!Managers.Data.SkillDic.TryGetValue(_selectedSkillId, out var skill)) return;
        string key = COMBAT_ABILITY_UNAVAILABLE_TEXT;
        if (_runner.GetCooldownRemain(_selectedSkillId) > 0f)
            key = COMBAT_ABILITY_COOLDOWN_TEXT;
        else if (_runner.CanUse(_selectedSkillId))
            key = COMBAT_ABILITY_READY_TEXT;
        else if (player.State != State.Dead && skill.Effect == AbilityEffect.Heal && player.Hp >= player.MaxHp)
            key = COMBAT_ABILITY_FULL_HP_TEXT;
        else if (player.State != State.Dead && skill.Effect == AbilityEffect.BonusDamage && (player.CurrentTarget == null || player.CurrentTarget.State == State.Dead))
            key = COMBAT_ABILITY_NO_TARGET_TEXT;
        if (_statusKey == key) return;
        _statusKey = key;
        _statusText.text = $"{Util.Localize(skill.Name)} · {Util.Localize(key)}";
    }
}
