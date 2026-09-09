using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [액티브 스킬바] 주인공이 직접 쓰는 능력을 최하단 탭 바로 위에 6칸으로 깐다.
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
        public Image cooldownMask;   // 위에서 덮는 어두운 막 (fillAmount = 남은 비율)
        public TMP_Text remainText;
        public int skillId = -1;
    }

    const int SLOT_SIZE = 26;
    const int SLOT_GAP = 2;
    const int BAR_Y = 40;        // 최하단 탭(y=4, 높이 34) 바로 위

    static readonly Color COLOR_READY = Color.white;
    static readonly Color COLOR_COOL = new Color(0.45f, 0.45f, 0.45f, 1f);
    static readonly Color COLOR_MASK = new Color(0f, 0f, 0f, 0.62f);

    readonly List<Slot> _slots = new();
    AbilityRunner _runner;

    public static UI_SkillBar Create(Transform parent, AbilityRunner runner)
    {
        var go = new GameObject("SkillBar", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, BAR_Y);
        rt.sizeDelta = new Vector2(ACTIVE_SKILL_SLOTS * SLOT_SIZE + (ACTIVE_SKILL_SLOTS - 1) * SLOT_GAP, SLOT_SIZE);

        var bar = go.AddComponent<UI_SkillBar>();
        bar.Build(runner);
        return bar;
    }

    void Build(AbilityRunner runner)
    {
        _runner = runner;

        float step = SLOT_SIZE + SLOT_GAP;
        float x0 = -(ACTIVE_SKILL_SLOTS - 1) * step * 0.5f;

        for (int i = 0; i < ACTIVE_SKILL_SLOTS; i++)
        {
            var slot = new Slot();

            slot.root = NewChild(transform, $"SkillSlot_{i}", SLOT_SIZE, SLOT_SIZE);
            slot.root.GetComponent<RectTransform>().anchoredPosition = new Vector2(x0 + i * step, 0f);

            // 바탕(눌리는 대상)
            var bg = slot.root.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.10f, 0.85f);
            slot.button = slot.root.AddComponent<Button>();

            slot.icon = NewImage(slot.root.transform, "Icon", SLOT_SIZE - 4, SLOT_SIZE - 4);
            slot.icon.raycastTarget = false;

            slot.cooldownMask = NewImage(slot.root.transform, "CooldownMask", SLOT_SIZE, SLOT_SIZE);
            slot.cooldownMask.color = COLOR_MASK;
            slot.cooldownMask.raycastTarget = false;
            slot.cooldownMask.type = Image.Type.Filled;
            slot.cooldownMask.fillMethod = Image.FillMethod.Vertical;
            slot.cooldownMask.fillOrigin = (int)Image.OriginVertical.Top;

            var txtGo = NewChild(slot.cooldownMask.transform, "RemainTxt", SLOT_SIZE, SLOT_SIZE);
            slot.remainText = txtGo.AddComponent<TextMeshProUGUI>();
            slot.remainText.alignment = TextAlignmentOptions.Center;
            slot.remainText.fontSize = 10;
            slot.remainText.color = Color.white;
            slot.remainText.raycastTarget = false;

            int idx = i;
            slot.button.onClick.AddListener(() => OnClickSlot(idx));
            _slots.Add(slot);
        }

        if (_runner != null) _runner.OnActiveSkillChanged += Refresh;
        if (Managers.Ability != null) Managers.Ability.OnAbilityChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (_runner != null) _runner.OnActiveSkillChanged -= Refresh;
        if (Managers.Ability != null) Managers.Ability.OnAbilityChanged -= Refresh;
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

    // ── 슬롯 구성 갱신 (능력 장착/해제, 쿨 완료) ──
    public void Refresh()
    {
        var ids = _runner != null ? _runner.ActiveSkillIds : new List<int>();

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            bool has = i < ids.Count;
            slot.skillId = has ? ids[i] : -1;
            slot.root.SetActive(has);
            if (!has) continue;

            // 첫 동행 한 칸도 화면 중앙에서 찾을 수 있게 현재 능력 수로 정렬한다.
            slot.root.GetComponent<RectTransform>().anchoredPosition =
                new Vector2((i - (ids.Count - 1) * 0.5f) * (SLOT_SIZE + SLOT_GAP), 0f);

            if (Managers.Data.SkillDic.TryGetValue(slot.skillId, out var d))
                slot.icon.sprite = Managers.Resource.Load<Sprite>(d.IconLabel);
            // 아이콘이 없으면 빈 칸이 아니라 흰 사각으로 남아 "뭔가 있다"는 건 보이게 둔다.
            slot.icon.enabled = slot.icon.sprite != null;
        }
    }

    void OnClickSlot(int index)
    {
        if (index >= _slots.Count) return;
        var slot = _slots[index];
        if (slot.skillId < 0 || _runner == null) return;
        _runner.UseSkill(slot.skillId);   // 쿨/사망/전투밖이면 내부에서 걸러진다
    }

    void Update()
    {
        if (_runner == null) return;

        foreach (var slot in _slots)
        {
            if (slot.skillId < 0 || !slot.root.activeSelf) continue;

            float remain = _runner.GetCooldownRemain(slot.skillId);
            bool ready = remain <= 0f && _runner.CanUse(slot.skillId);

            slot.cooldownMask.fillAmount = 1f - _runner.GetCooldownRatio(slot.skillId);
            slot.icon.color = ready ? COLOR_READY : COLOR_COOL;
            slot.remainText.text = remain > 0f ? Mathf.CeilToInt(remain).ToString() : "";
        }
    }
}
