using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// 대장간 플로우 1→2: 장비창 슬롯 클릭 → 본 팝업 SetInfo(Equipment).
// 이름/부위/내구도/고유능력/능력치(기본+강화 합산) 표시 + 장착·해제 + 대장간(UI_ForgePopup) 진입.
// 등장 연출은 prefab authored DOTweenAnimation(autoPlay) 그대로 사용 — 코드 OpenAnimation 미호출.
// 노드명은 tree_UI_EquipDetailPopup.txt 그대로 (원본 프리팹 오타 EnhaceBtn 포함).
public class UI_EquipDetailPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        WIcon,                    // 장비 아이콘 (Animator=EquipIcon)
        BlockImg,                 // 잠금 dim — 보유 장비만 진입하므로 항상 꺼둠
        Img_EquipPopupIcon_02,    // 흰색 플래시(강화 연출용) — 상세에선 꺼둠
        Xbutton,                  // 우상단 X (Button 컴포넌트 없음 → GO에 직접 바인드)
    }

    enum Texts
    {
        Txt_TopFrame_01,          // 타이틀 "+N 장비명"
        ENumber,                  // 아이콘 위 "+N"
        EquipTypeTxt,             // 부위
        EquipDuriInfoTxt,         // 내구도 "cur/max"
        EquipSkillInfoTxt,        // 고유능력
        Content,                  // 능력치 목록 (ScrollView Content 자체가 TMP)
        StatNameTxt,              // (+)상세: 스탯명 행
        BaseTxt,                  // (+)상세: 기본 행
        EnTxt,                    // (+)상세: 강화 행
        DurTxt,                   // (+)상세: 내구 행
        Txt_EquipDuriTxtGroup_07,
        Txt_EquipSkillTxtGroup_05,
        Txt_EquipStatTxtGroup_02,
        Txt_EquipBtn_04,
        Txt_OnEquipBtn_02,
        Txt_DontHaveBtn_03,
        Txt_EnhaceBtn_05,
        Txt_X_1_08,
        Txt_X_1_02,
        Txt_X_1_04,
        __3,
        Btn_EquipAmountDetailPopup_03,
    }

    enum Buttons
    {
        EquipPopupBackground,
        EquipAmountDetailBtn,             // (+) 기본/강화/내구 상세 열기
        EquipBtn,                         // 장착
        EnhaceBtn,                        // 대장간 (원본 프리팹 오타 그대로)
        Btn_EquipAmountDetailPopup_01,    // 상세 오버레이 닫기(투명 배경)
        Btn_EquipAmountDetailPopup_03,    // 상세 오버레이 닫기(본문)
    }

    enum GameObjects
    {
        EquipDetailPopup,
        EquipedEf,                // 아이콘 'E' 장착 뱃지
        OnEquipBtn,               // 장착중 (Button 컴포넌트 없음 → GO 바인드, 클릭=해제)
        DontHaveBtn,              // 미보유 — 현재 플로우상 항상 보유 → 숨김
        EquipAmountDetailPopup,   // (+) 상세 오버레이
    }

    #endregion

    const float LOW_DURI_RATE = 0.25f;   // 내구도 경고 기준 (장비창과 동일)

    Equipment _equip;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // 가로 자동 확장이 버튼 마스크를 넘지 않도록 부모 버튼 안에서 줄바꿈한다.
        foreach (var id in new[] { Texts.Txt_EquipBtn_04, Texts.Txt_OnEquipBtn_02, Texts.Txt_DontHaveBtn_03, Texts.Txt_EnhaceBtn_05 })
        {
            var label = GetText((int)id);
            var fitter = label.GetComponent<ContentSizeFitter>();
            if (fitter != null) fitter.enabled = false;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(2f, 1f);
            label.rectTransform.offsetMax = new Vector2(-2f, -1f);
            label.enableAutoSizing = true;
            label.fontSizeMax = label.fontSize;
            label.fontSizeMin = label.fontSizeMax * 0.65f;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.alignment = TextAlignmentOptions.Center;
        }

        // 본문은 뷰포트 폭을 지키며 세로로 늘어나야 마지막 제작 재료까지 스크롤할 수 있다.
        var content = GetText((int)Texts.Content);
        var contentFitter = content.GetComponent<ContentSizeFitter>();
        if (contentFitter != null)
        {
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        content.textWrappingMode = TextWrappingModes.Normal;
        content.raycastTarget = true;
        content.gameObject.GetOrAddComponent<UI_ItemLinkHandler>();

        // 긴 영문 장비명도 아이콘 오른쪽부터 제목 프레임 끝까지의 폭 안에 맞춘다.
        var title = GetText((int)Texts.Txt_TopFrame_01);
        var titleFitter = title.GetComponent<ContentSizeFitter>();
        if (titleFitter != null) titleFitter.enabled = false;
        var titleRT = title.rectTransform;
        float titleWidth = ((RectTransform)titleRT.parent).rect.width
            - titleRT.anchoredPosition.x - EQUIP_TITLE_RIGHT_INSET;
        titleRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, titleWidth / titleRT.localScale.x);
        title.enableAutoSizing = true;
        title.fontSizeMax = title.fontSize;
        title.fontSizeMin = title.fontSizeMax * 0.65f;
        title.textWrappingMode = TextWrappingModes.Normal;

        // 영어 제목이 오른쪽 수치 열(x=33)을 덮지 않도록 기존 두 열 간격 안에 맞춘다.
        foreach (var id in new[] { Texts.Txt_EquipDuriTxtGroup_07, Texts.Txt_EquipSkillTxtGroup_05 })
        {
            var label = GetText((int)id);
            var fitter = label.GetComponent<ContentSizeFitter>();
            if (fitter != null) fitter.enabled = false;
            label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 26f);
            label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 8f);
            label.enableAutoSizing = true;
            label.fontSizeMax = label.fontSize;
            label.fontSizeMin = label.fontSizeMax * 0.65f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.alignment = TextAlignmentOptions.TopLeft;
        }

        _frameRT = GetObject((int)GameObjects.EquipDetailPopup)?.GetComponent<RectTransform>();

        // 닫기: BG / X버튼 (X는 Button 없음 → Image GO에 바인드)
        GetButton((int)Buttons.EquipPopupBackground).gameObject.BindEvent(OnClickBG);
        var xGo = GetImage((int)Images.Xbutton)?.gameObject;
        if (xGo != null) xGo.BindEvent(OnClickBG);

        // 장착 / 해제(장착중 클릭) / 대장간
        GetButton((int)Buttons.EquipBtn).gameObject.BindEvent(OnClickEquip);
        GetObject((int)GameObjects.OnEquipBtn)?.BindEvent(OnClickUnEquip);
        GetButton((int)Buttons.EnhaceBtn).gameObject.BindEvent(OnClickEnhance);

        // (+) 기본/강화/내구 상세 오버레이 — 열기/닫기 (기본 숨김)
        GetButton((int)Buttons.EquipAmountDetailBtn).gameObject.BindEvent(OpenAmountDetail);
        GetButton((int)Buttons.Btn_EquipAmountDetailPopup_01).gameObject.BindEvent(CloseAmountDetail);
        GetButton((int)Buttons.Btn_EquipAmountDetailPopup_03).gameObject.BindEvent(CloseAmountDetail);
        SetActive(GetObject((int)GameObjects.EquipAmountDetailPopup), false);

        // 강화 연출용 플래시 잔재는 상세에선 꺼둠
        SetActive(GetImage((int)Images.Img_EquipPopupIcon_02)?.gameObject, false);

        // 강화/장착 등 장비 상태 변화 시 자동 갱신 (대장간에서 강화 후 복귀 포함)
        if (Managers.Game != null)
        {
            Managers.Game.EquipInfoChanged += RefreshUI;
            Managers.Game.OnLanguageChanged += RefreshUI;
        }

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Managers.Game != null)
        {
            Managers.Game.EquipInfoChanged -= RefreshUI;
            Managers.Game.OnLanguageChanged -= RefreshUI;
        }
    }

    // 장비창에서 슬롯 클릭 시 호출 — 표시할 장비 지정.
    public void SetInfo(Equipment equip)
    {
        _equip = equip;
        RefreshUI();
    }

    public override void RefreshUI()
    {
        RefreshTexts();
        if (_equip == null) return;
        var data = _equip.EquipmentData;
        if (data == null)
        {
            Debug.Log($"[EquipDetail] EquipmentData 없음: {_equip.DataId}");
            return;
        }

        // ── 상단: 이름/아이콘/장착 이펙트 ──
        string name = Util.Localize(data.Name);   // EquipmentData.Name = 로컬라이즈 키 (예: Equip30001NameText)
        SetText(Texts.Txt_TopFrame_01, _equip.EnhanceLevel > 0 ? $"+{_equip.EnhanceLevel} {name}" : name);

        // 아이콘 위 "+N" — 강화 0이면 숨김
        SetActive(GetTextGo(Texts.ENumber), _equip.EnhanceLevel > 0);
        if (_equip.EnhanceLevel > 0)
            SetText(Texts.ENumber, $"+{_equip.EnhanceLevel}");

        var icon = GetImage((int)Images.WIcon);
        if (icon != null && string.IsNullOrEmpty(data.SpriteName) == false)
        {
            var sp = Managers.Resource.Load<Sprite>(data.SpriteName);
            if (sp != null)
            {
                // WIcon에는 EquipIcon Animator가 붙어 있고, 그 클립이 sprite를 **매 프레임 덮어쓴다**.
                //  그래서 여기서 올바른 스프라이트를 넣어도 다음 프레임에 클립 것으로 되돌아가,
                //  장비창에서 고른 장비와 상세 팝업의 그림이 서로 달랐다.
                //  (UI_ForgePopup은 이미 같은 이유로 애니메이터를 끄고 있다)
                var animator = icon.GetComponent<Animator>();
                if (animator != null) animator.enabled = false;
                icon.sprite = sp;
                icon.preserveAspect = true;
                // 애니메이터가 먹여 둔 회전/스케일도 되돌린다 — 안 그러면 아이콘이 누운 채로 남는다.
                icon.rectTransform.localRotation = Quaternion.identity;
                icon.rectTransform.localScale = Vector3.one;
            }
        }

        // 미보유 장비도 상세창에 들어온다(장비창이 60종 전부를 보여준다) → 잠금 dim + 제작 안내.
        bool owned = Managers.Game.HasEquipment(_equip.DataId);

        SetActive(GetObject((int)GameObjects.EquipedEf), owned && _equip.IsEquipped);   // 'E' 뱃지
        SetActive(GetImage((int)Images.BlockImg)?.gameObject, !owned);                  // 미보유면 잠금 dim

        // ── 중단: 부위/내구도/고유능력/능력치 ──
        SetText(Texts.EquipTypeTxt, SlotKor(data.SlotType));

        bool lowDuri = _equip.Durability <= data.MaxDurability * LOW_DURI_RATE;
        string duri = $"{_equip.Durability}/{data.MaxDurability}";
        SetText(Texts.EquipDuriInfoTxt, lowDuri ? $"<color=red>{duri}</color>" : duri);

        SetText(Texts.EquipSkillInfoTxt, GetEquipAbilityName());

        RefreshStatText(data);
        RefreshAmountDetail(data);

        // ── 하단 버튼 상호배타: 제작(미보유) / 장착 / 장착중(클릭=해제) ──
        SetActive(GetButton((int)Buttons.EquipBtn).gameObject, owned && _equip.IsEquipped == false);
        SetActive(GetObject((int)GameObjects.OnEquipBtn), owned && _equip.IsEquipped);
        SetActive(GetObject((int)GameObjects.DontHaveBtn), !owned);

        if (!owned) RefreshCraftButton();
    }

    // 미보유 장비 — DontHaveBtn을 [제작] 버튼으로 쓴다(프리팹 저작 의도 그대로).
    //  만들 수 있으면 "제작", 막혔으면 **왜 막혔는지**를 버튼에 그대로 쓴다.
    //  (대장간에서 "강화불가"만 뜨고 이유를 안 알려줘 고장으로 보였던 전례를 반복하지 않는다)
    void RefreshCraftButton()
    {
        var go = GetObject((int)GameObjects.DontHaveBtn);
        if (go == null) return;

        string reason = Managers.Game.GetCraftBlockReason(_equip.DataId);
        bool can = string.IsNullOrEmpty(reason);

        var label = go.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = can ? Util.Localize(LocalizeKey.CraftText) : reason;
            ApplyFont(label, FontType.UI);
        }

        if (!_craftBound)
        {
            _craftBound = true;
            go.BindEvent(OnClickCraft);
        }
    }

    bool _craftBound;

    void OnClickCraft()
    {
        if (_equip == null) return;

        string reason = Managers.Game.GetCraftBlockReason(_equip.DataId);
        if (!string.IsNullOrEmpty(reason))
        {
            Managers.UI.ShowToast(reason);
            return;
        }

        if (!Managers.Game.CraftEquipment(_equip.DataId)) return;

        Managers.Sound?.PlaySFX("SFX_Upgrade");
        // 방금 만든 실제 인스턴스로 갈아끼워 강화도·내구도가 상세창에 반영되게 한다
        var made = Managers.Game.OwnedEquipments.Find(e => e.DataId == _equip.DataId);
        SetInfo(made ?? _equip);
    }

    // ── 표시 빌더 ──

    // 부위명 — 기존 메서드명은 유지하고 현재 언어의 슬롯 이름을 표시한다.
    static string SlotKor(EquipSlotType slot)
    {
        switch (slot)
        {
            case EquipSlotType.Weapon:    return Util.Localize(LocalizeKey.EquipSlotWeaponText);
            case EquipSlotType.Helmet:    return Util.Localize(LocalizeKey.EquipSlotHelmetText);
            case EquipSlotType.Armor:     return Util.Localize(LocalizeKey.EquipSlotArmorText);
            case EquipSlotType.Gloves:    return Util.Localize(LocalizeKey.EquipSlotGlovesText);
            case EquipSlotType.Legs:      return Util.Localize(LocalizeKey.EquipSlotLegsText);
            case EquipSlotType.Accessory: return Util.Localize(LocalizeKey.EquipSlotAccessoryText);
            default:                      return slot.ToString();
        }
    }

    // 고유능력 = SkillData 중 Source=Equip && RequiredEquipId=이 장비인 능력 이름. 없으면 "-".
    string GetEquipAbilityName()
    {
        foreach (var kv in Managers.Data.SkillDic)
            if (kv.Value.Source == AbilitySource.Equip && kv.Value.RequiredEquipId == _equip.DataId)
                return Managers.Ability.GetName(kv.Key);
        return "-";
    }

    // 현재 강화 단계까지의 강화 보너스 합산 (EquipEnhanceData: EquipDataId 일치 && Level<=EnhanceLevel)
    void SumEnhanceBonus(out int atk, out float crit, out int magic)
    {
        atk = 0; crit = 0f; magic = 0;
        if (Managers.Data.EquipEnhanceDic == null) return;
        foreach (var kv in Managers.Data.EquipEnhanceDic)
        {
            var d = kv.Value;
            if (d.EquipDataId != _equip.DataId || d.Level > _equip.EnhanceLevel) continue;
            atk += d.AtkBonus;
            crit += d.CritBonus;
            magic += d.MagicAtkBonus;
        }
    }

    // 능력치 목록(ScrollView Content) — 기본+강화 합산, 강화분은 녹색 병기. 0인 스탯은 생략.
    void RefreshStatText(Data.EquipmentData data)
    {
        SumEnhanceBonus(out int enAtk, out float enCrit, out int enMagic);
        var sb = new StringBuilder();
        AppendStat(sb, Util.Localize(LocalizeKey.PhysicalAttackText), data.BaseAtk + enAtk, enAtk, false);
        AppendStat(sb, Util.Localize(LocalizeKey.CriticalText), data.BaseCritRate + enCrit, enCrit, true);
        AppendStat(sb, Util.Localize(LocalizeKey.MagicalAttackText), data.BaseMagicAtk + enMagic, enMagic, false);

        // 미보유면 능력치 아래에 **필요 재료**를 같이 보여준다.
        //  버튼에는 첫 번째로 막힌 사유만 뜨므로, 전체 목록은 여기서 확인한다.
        //  모자란 항목은 빨강, 충족은 초록 (기획서 p1-18 사용/필요 아이템 표기 규칙).
        if (!Managers.Game.HasEquipment(_equip.DataId))
        {
            var recipe = Managers.Game.GetRecipeFor(_equip.DataId);
            if (recipe != null)
            {
                // 한 줄에 몰아넣으면 폭이 좁아 뒷자리가 잘리고("골드 4,200/1"),
                //  기본 크기로는 5줄이 표시 영역을 넘어 마지막 재료가 잘린다. 줄을 나누고 조금 작게.
                if (sb.Length > 0) sb.Append("\n");
                sb.Append($"\n<size=85%><color=#FFD000>[{Util.Localize(LocalizeKey.ForgeReqMaterialText)}]</color>");
                sb.Append($"\n{Util.Localize(LocalizeKey.PlayerLevelLabelText)} {Colored(Managers.Game.Level, recipe.ReqLevel)}");
                sb.Append($"\n{Util.Localize(LocalizeKey.GoldLabelText)} {Colored(Managers.Game.Gold, recipe.Gold)}");

                foreach (var (id, need) in GameManager.RecipeMaterials(recipe))
                {
                    if (id <= 0 || need <= 0) continue;
                    string nm = Managers.Data.ItemDic != null && Managers.Data.ItemDic.TryGetValue(id, out var it)
                        ? Util.Localize(it.Name) : id.ToString();
                    sb.Append($"\n<link=\"{ITEM_LINK_PREFIX}{id}\"><u>{nm}</u></link> ")
                        .Append(Colored(Managers.Game.GetItemCount(id), need));
                }
                sb.Append($"\n<color=#FFD000>{Util.Localize(GROWTH_MATERIAL_HINT_TEXT)}</color>");
                sb.Append("</size>");
            }
        }

        SetText(Texts.Content, sb.Length > 0 ? sb.ToString() : "-");
    }

    // 보유/필요 — 충족 초록 / 부족 빨강 (기획서 p1-18)
    static string Colored(int have, int need)
        => have >= need
            ? $"<color=#00FF00>{have:N0}</color>/{need:N0}"
            : $"<color=#FF0000>{have:N0}</color>/{need:N0}";

    static void AppendStat(StringBuilder sb, string label, float total, float enhance, bool percent)
    {
        if (total == 0f && enhance == 0f) return;
        if (sb.Length > 0) sb.Append("\n");
        string unit = percent ? "%" : "";
        sb.Append($"{label} +{total:0.#}{unit}");
        if (enhance != 0f) sb.Append($" <color=#00FF00>(+{enhance:0.#}{unit})</color>");
    }

    // (+) 상세 오버레이: 기본/강화/내구 행 분해. 내구도의 스탯 기여 데이터는 없어 "-" 고정 (flags 참조).
    void RefreshAmountDetail(Data.EquipmentData data)
    {
        SumEnhanceBonus(out int enAtk, out float enCrit, out int enMagic);
        SetText(Texts.StatNameTxt, $"{Util.Localize(LocalizeKey.PhysicalAttackText)}   {Util.Localize(LocalizeKey.CriticalText)}   {Util.Localize(LocalizeKey.MagicalAttackText)}");
        SetText(Texts.Btn_EquipAmountDetailPopup_03, GetText((int)Texts.StatNameTxt).text);
        SetText(Texts.BaseTxt, $"+{data.BaseAtk}   +{data.BaseCritRate:0.#}%   +{data.BaseMagicAtk}");
        SetText(Texts.EnTxt, $"+{enAtk}   +{enCrit:0.#}%   +{enMagic}");
        SetText(Texts.DurTxt, "-   -   -");
    }

    // ── 클릭 핸들러 ──

    void OnClickEquip()
    {
        if (_equip == null || _equip.IsEquipped) return;
        Managers.Game.EquipItem(_equip);   // EquipInfoChanged → RefreshUI 자동 갱신
    }

    // 장착중 버튼 클릭 = 해제. prefab에 전용 '해제' 버튼이 없어 단순화 (flags 참조).
    void OnClickUnEquip()
    {
        if (_equip == null || _equip.IsEquipped == false) return;
        Managers.Game.UnEquipItem(_equip);
    }

    void OnClickEnhance()
    {
        if (_equip == null) return;
        Managers.UI.ShowPopupUI<UI_ForgePopup>().SetInfo(_equip);
    }

    void OpenAmountDetail() => SetActive(GetObject((int)GameObjects.EquipAmountDetailPopup), true);
    void CloseAmountDetail() => SetActive(GetObject((int)GameObjects.EquipAmountDetailPopup), false);

    // ── 헬퍼 (UI_SkillDetailPopup과 동일 패턴) ──

    void RefreshTexts()
    {
        SetText(Texts.Txt_EquipDuriTxtGroup_07, Util.Localize(LocalizeKey.ForgeDurabilityText));
        SetText(Texts.Txt_EquipSkillTxtGroup_05, Util.Localize(LocalizeKey.UniqueSkillText));
        SetText(Texts.Txt_EquipStatTxtGroup_02, Util.Localize("EquipmentStatsText"));
        SetText(Texts.Txt_EquipBtn_04, Util.Localize("EquipActionText"));
        SetText(Texts.Txt_OnEquipBtn_02, Util.Localize("EquippedText"));
        SetText(Texts.Txt_EnhaceBtn_05, Util.Localize(LocalizeKey.ForgeText));
        SetText(Texts.Txt_X_1_08, Util.Localize(LocalizeKey.BaseStatText));
        SetText(Texts.Txt_X_1_02, Util.Localize(LocalizeKey.ForgeEnhanceBtnText));
        SetText(Texts.Txt_X_1_04, Util.Localize(LocalizeKey.ForgeDurabilityText));
        SetText(Texts.__3, Util.Localize(LocalizeKey.DescriptionText));
    }

    static void SetActive(GameObject go, bool on) { if (go != null && go.activeSelf != on) go.SetActive(on); }
    GameObject GetTextGo(Texts id) { var t = GetText((int)id); return t != null ? t.gameObject : null; }

    void SetText(Texts id, string value)
    {
        var t = GetText((int)id);
        if (t == null) return;
        t.text = value;
        ApplyFont(t, FontType.UI);
    }
}
