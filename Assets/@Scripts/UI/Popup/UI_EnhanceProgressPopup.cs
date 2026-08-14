using System.Collections;
using TMPro;
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

            // 잠재력 변화.
            //  "잠재력 변화" 라벨이 "능력치 변화" 라벨과 **같은 좌표**에 놓여 있어 두 글자가 겹쳐 찍혔다.
            //  라벨은 끄고 수치 쪽에 이름을 붙인다(내구도와 같은 처리). 라벨의 원래 자리는
            //  자기 수치(아래쪽)와도 멀어서 어차피 제 위치가 아니었다.
            var potenLabel = GetText((int)Texts.Txt_PotenGroup_01);
            if (potenLabel != null) potenLabel.gameObject.SetActive(false);
            SetRevealText(Texts.MainPotenText, $"잠재력 {_equip.Potential}");
            SetRevealText(Texts.IncPotenText, $"+{_result.PotenGain}");

            // 능력치 변화 — 현재 데이터는 공격력 단일(단계당 +5). 다중 스탯 데이터화 시 열 확장.
            SetGroupActive(GameObjects.AttRealText, true);
            RevealText(Texts.AttText);   // "능력치 변화" 라벨도 색을 되살려야 보인다
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

            // before / "/max" / after 가 프리팹에서 거의 같은 x에 겹쳐 놓여 있어
            //  "내구도 /900" 처럼 숫자들이 서로를 덮어썼다 → 한 줄로 합쳐 라벨에 싣고 나머지는 끈다.
            SetRevealText(Texts.Txt_FailTxtGroup_01,
                $"{Util.Localize(LocalizeKey.ForgeDurabilityText)} {_result.PrevDurability} ▶ " +
                $"<color=red>{_equip.Durability}</color>/{data.MaxDurability}");
            HideText(Texts._2_B4_Variable);
            HideText(Texts.Txt_FailTxtGroup_04);
            HideText(Texts._4_After_Variable);

            SetGroupActive(GameObjects.PotenGroup, false);   // 실패 — 잠재력 변화 없음
        }

        // EnhAttDetail 내구도 현황.
        //  라벨과 수치가 프리팹에서 거의 같은 자리에 놓여 "내구도110/110"처럼 겹쳐 보였다 →
        //  라벨을 수치 안으로 합치고 원래 라벨은 끈다(대장간 팝업과 같은 처리).
        //  실패 연출에는 FailTxtGroup이 before▶after 내구도를 따로 보여주므로 여기선 통째로 숨긴다
        //  (둘 다 켜져서 "내구도"가 두 번 겹쳐 찍혔다).
        var duriLabel = GetText((int)Texts.DurabilityText);
        if (duriLabel != null) duriLabel.gameObject.SetActive(false);

        var duriValue = GetText((int)Texts.DurabilityRealText);
        if (duriValue != null) duriValue.gameObject.SetActive(_result.Success);
        if (_result.Success)
            SetRevealText(Texts.DurabilityRealText,
                $"{Util.Localize(LocalizeKey.ForgeDurabilityText)} {_equip.Durability}/{data.MaxDurability}");
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
        MakeVisible(txt);
        txt.gameObject.SetActive(false);
        txt.gameObject.SetActive(true);
    }

    void SetRevealText(Texts t, string value)
    {
        var txt = GetText((int)t);
        if (txt == null) return;
        txt.text = value;
        ApplyFont(txt, FontType.UI);
        MakeVisible(txt);
        txt.gameObject.SetActive(false);
        txt.gameObject.SetActive(true);
    }

    void HideText(Texts t)
    {
        var txt = GetText((int)t);
        if (txt != null) txt.gameObject.SetActive(false);
    }

    // 프리팹에 저작된 색이 화면에서 안 보이는 값이라 [능력치 변화]/[잠재력 변화] 패널이
    //  통째로 비어 보였다 — 능력치 쪽은 **alpha 0**, 잠재력 쪽은 검은 배경 위 **검은 글자**.
    //  값은 정상적으로 들어가 있었으니 색만 되살린다. 의도된 색(초록 증가치 등)은 유지.
    static void MakeVisible(TMP_Text txt)
    {
        Color c = txt.color;
        if (c.r + c.g + c.b < 0.15f) c = Color.white;   // 사실상 검정 → 흰색
        c.a = 1f;
        txt.color = c;
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
