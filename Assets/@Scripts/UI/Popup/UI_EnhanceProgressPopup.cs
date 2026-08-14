using System.Collections;
using UnityEngine;
using static Define;

/// <summary>
/// 강화 결과 연출 팝업 — UI_EnhacePopup에서 [강화](영상 건너뛰기 미체크) 시 진입.
/// 망치 연출 후 성공(SuccessTxtGroup+잠재력 변화+능력치 변화) / 실패(FailTxtGroup+내구도 before→after) 표시.
/// ExitBtn(투명 전체화면)으로 닫기. 스탯 적용은 GameManager.EnhanceEquipment가 이미 처리.
/// </summary>
public class UI_EnhanceProgressPopup : UI_Popup
{
    #region Enum

    enum Images
    {
        WIcon,                   // 장비 아이콘 (forAnim 하위)
    }

    enum Texts
    {
        Txt_SuccessTxtGroup_01,  // "ENHACE SUCCEEDED" (prefab authored 오타 유지)
        Txt_SuccessTxtGroup_03,  // 장비명
        _4,                      // 장비명 펀치 연출용 복제 텍스트
        Txt_FailTxtGroup_02,     // "ENHACE FAILED"
        Txt_FailTxtGroup_01,     // "내구도" 라벨
        _2_B4_Variable,          // 내구도 before
        Txt_FailTxtGroup_04,     // "/max"
        _4_After_Variable,       // 내구도 after
        DurabilityText,          // EnhAttDetail "내구도" 라벨
        DurabilityRealText,      // EnhAttDetail "cur/max"
        AttText,                 // "능력치 변화" 라벨 (authored active)
        Text,                    // AttRealText: 스탯 이름 열
        Txt_AttRealText_02,      // AttRealText: before 열
        Text_2,                  // AttRealText: ▶ 열
        Text_3,                  // AttRealText: after 열
        MainPotenText,           // 잠재력 누적치
        IncPotenText,            // "+n" 증가치
        Txt_PotenGroup_01,       // "잠재력 변화" 라벨
    }

    enum Buttons
    {
        ExitBtn,
    }

    enum GameObjects
    {
        Icon,
        Hammer_IncludeAnim,
        SuccessFlash,
        Flash_USETHIS,
        SuccessTxtGroup,
        Img_SuccessTxtGroup_02,  // 성공 텍스트 상하 라인
        _2,
        FailTxtGroup,
        PotenGroup,
        AttRealText,
    }

    #endregion

    const float HAMMER_DURATION = 1.2f;   // 망치 연출 시간 — 결과 공개 전 대기

    GameManager.EnhanceResult _result;
    Equipment _equip;
    bool _revealed;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.ExitBtn).onClick.AddListener(OnClickExit);
        return true;
    }

    // ────────────────────────────────────────
    //  SetInfo
    // ────────────────────────────────────────
    public void SetInfo(GameManager.EnhanceResult result, Equipment equip)
    {
        _result = result;
        _equip = equip;
        if (_result == null || _equip == null || _equip.EquipmentData == null)
        {
            Managers.UI.ClosePopupUI();
            return;
        }

        // 결과 요소는 전부 숨긴 채 망치 연출부터
        SetGroupActive(GameObjects.SuccessTxtGroup, true);   // 그룹 자체는 켜두고 자식(INACTIVE)만 개별 공개
        SetGroupActive(GameObjects.FailTxtGroup, true);
        SetGroupActive(GameObjects.PotenGroup, true);
        SetGroupActive(GameObjects.AttRealText, false);

        // 장비 아이콘 — EquipIcon Animator 정지 후 세팅
        var wIcon = GetImage((int)Images.WIcon);
        Sprite sp = Managers.Resource.Load<Sprite>(_equip.EquipmentData.SpriteName);
        if (wIcon != null && sp != null)
        {
            var animator = wIcon.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            wIcon.sprite = sp;
        }

        StartCoroutine(CoPlay());
    }

    IEnumerator CoPlay()
    {
        yield return new WaitForSecondsRealtime(HAMMER_DURATION);   // 망치 연출(authored Animator 'Forge Smith')

        _revealed = true;
        var data = _equip.EquipmentData;
        string nm = Util.Localize(data.Name);
        string nmEnh = _equip.EnhanceLevel > 0 ? $"+{_equip.EnhanceLevel} {nm}" : nm;

        if (_result.Success)
        {
            Managers.Sound?.PlaySFX("SFX_Upgrade");   // 강화 성공음 (클립 미등록이면 경고만)

            // 플래시 + 성공 텍스트 (INACTIVE 자식들 활성 → authored autoPlay 연출 재생)
            Reveal(GameObjects.Flash_USETHIS);
            RevealText(Texts.Txt_SuccessTxtGroup_01);
            Reveal(GameObjects.Img_SuccessTxtGroup_02);
            Reveal(GameObjects._2);
            SetRevealText(Texts.Txt_SuccessTxtGroup_03, nmEnh);
            SetRevealText(Texts._4, nmEnh);

            // 잠재력 변화
            RevealText(Texts.Txt_PotenGroup_01);
            SetRevealText(Texts.MainPotenText, _equip.Potential.ToString());
            SetRevealText(Texts.IncPotenText, $"+{_result.PotenGain}");

            // 능력치 변화 — 현재 데이터는 공격력 단일(단계당 +5). 다중 스탯 데이터화 시 열 확장.
            SetGroupActive(GameObjects.AttRealText, true);
            int before = SumAtkBonus(_result.PrevLevel);
            int after  = SumAtkBonus(_equip.EnhanceLevel);
            SetRevealText(Texts.Text, Util.Localize(LocalizeKey.PhysicalAttackText));
            SetRevealText(Texts.Txt_AttRealText_02, $"+{before}");
            SetRevealText(Texts.Text_2, "▶");
            SetRevealText(Texts.Text_3, $"<color=#00FF00>+{after}</color>");
        }
        else
        {
            Managers.Sound?.PlaySFX("SFX_Fail");   // 강화 실패음 (클립 미등록이면 경고만)

            RevealText(Texts.Txt_FailTxtGroup_02);
            RevealText(Texts.Txt_FailTxtGroup_01);
            SetRevealText(Texts._2_B4_Variable, _result.PrevDurability.ToString());
            SetRevealText(Texts.Txt_FailTxtGroup_04, $"/{data.MaxDurability}");
            SetRevealText(Texts._4_After_Variable, $"<color=red>{_equip.Durability}</color>");

            SetGroupActive(GameObjects.PotenGroup, false);   // 실패 — 잠재력 변화 없음
        }

        // 공통: EnhAttDetail 내구도 현황
        var duriLabel = GetText((int)Texts.DurabilityText);
        if (duriLabel != null) duriLabel.gameObject.SetActive(true);
        SetRevealText(Texts.DurabilityRealText, $"{_equip.Durability}/{data.MaxDurability}");
    }

    // 단계별 증가분(EquipEnhanceData) 누적 합 — 표시용
    int SumAtkBonus(int enhanceLevel)
    {
        int sum = 0;
        if (Managers.Data?.EquipEnhanceDic == null) return 0;
        foreach (var enh in Managers.Data.EquipEnhanceDic.Values)
            if (enh.EquipDataId == _equip.DataId && enh.Level <= enhanceLevel)
                sum += enh.AtkBonus;
        return sum;
    }

    // ── 공개 헬퍼: INACTIVE 노드를 활성화(authored autoPlay 연출 재생)하고 텍스트 세팅 ──
    void Reveal(GameObjects go)
    {
        var o = GetObject((int)go);
        if (o == null) return;
        o.SetActive(false);
        o.SetActive(true);
    }

    void RevealText(Texts t)
    {
        var txt = GetText((int)t);
        if (txt == null) return;
        txt.gameObject.SetActive(false);
        txt.gameObject.SetActive(true);
    }

    void SetRevealText(Texts t, string value)
    {
        var txt = GetText((int)t);
        if (txt == null) return;
        txt.text = value;
        ApplyFont(txt, FontType.UI);
        txt.gameObject.SetActive(false);
        txt.gameObject.SetActive(true);
    }

    void SetGroupActive(GameObjects go, bool on)
    {
        var o = GetObject((int)go);
        if (o != null && o.activeSelf != on) o.SetActive(on);
    }

    void OnClickExit()
    {
        if (_isClosing) return;
        if (!_revealed) return;   // 망치 연출 중엔 닫기 무시(결과 확인 강제)
        _isClosing = true;
        Managers.UI.ClosePopupUI();
    }
}
