using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
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
        Txt_SuccessTxtGroup_01,  // 성공 배너 (프리팹엔 "ENHACE SUCCEEDED" 오타 영문 → 로컬라이즈)
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
        Bg,                      // 전면 배경판 — 성공 직후 백색 잔광이 여기서 마저 걷힌다
        SuccessTxtGroup,
        Img_SuccessTxtGroup_02,  // 성공 텍스트 상하 라인
        _2,
        FailTxtGroup,
        PotenGroup,
        EnhAttDetail,            // 내구도 + "능력치 변화" 라벨 묶음
        AttRealText,
        God_ray_02,              // 빛줄기 (월드 ParticleSystem → UIParticle 필요)
    }

    #endregion

    // 원본(비주얼 스크립팅) 그래프의 'SuccessDone' 사슬을 그대로 옮긴 간격이다.
    //  Timer 노드 값: 0.2 → 1.7 → 0.25 → 1.85.
    //  이 창은 대장장이 클립이 끝나는 순간(SuccessDone) 열리므로 첫 대기가 곧 0.2다.
    const float STEP_TITLE  = 0.20f;   // 제목/이름 + 아이콘 하강 + 빛줄기
    const float STEP_DETAIL = 1.70f;   // 내구도 + "능력치 변화" 라벨
    const float STEP_TABLE  = 0.25f;   // 능력치 표가 오른쪽에서 밀려 들어온다

    // 능력치 표는 그 안에서 다시 다섯 걸음을 밟는다. 원본 영상을 열별 밝기로 재서 얻은 값이다
    //  (라벨 f253 → 이전값 f254 → 화살표 f255 → 초록 f256~258 → 채움 f265 → 정리 f267 → 라벨 f270).
    //  아래는 **라벨이 뜨는 순간을 0으로 둔 상대시각**이다.
    const float TBL_STAGGER   = 0.10f;   // 열 하나가 뜨고 다음 열이 뜨기까지
    const float TBL_ARROW_HIT = 1.20f;   // 화살표가 속을 채우며 한 번 번쩍
    const float TBL_SETTLE    = 1.40f;   // 화살표·이전값이 걷히고 초록이 왼쪽으로
    const float ARROW_FLASH   = 0.05f;   // 원본은 채운 ▶이 딱 한 프레임이다

    // 강화창의 백색 판은 1.5초에 걸쳐 걷히는데, 이 창은 그 중 1.2초쯤 지난 시점에 열린다.
    //  남은 밝기에서 이어받아 마저 검게 만든다(원본 223.1s 실측 46/255).
    const float WASH_LEFT = 0.18f;
    const float WASH_TAIL = 0.45f;

    // 화살표 색은 처음부터 끝까지 **하나**다(#A60000). 원본에서 바뀌는 건 색이 아니라
    //  글리프뿐이다 — 속 빈 ▷가 한 프레임 채워진 ▶이 된다(f264와 f265의 픽셀을 비교하면
    //  색 바이트는 그대로고 속만 0 → 166으로 찬다).
    const string ARROW_HEX = "A60000";

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

        // 결과 요소는 **꺼진 채로** 시작한다. 프리팹의 DOTweenAnimation이 대부분 autoPlay라
        //  그룹을 켜는 순간이 곧 그 연출의 시작이다 — 미리 켜 두면 아무 일도 안 일어난 채
        //  결과가 이미 떠 있는 화면이 된다(예전 증상).
        SetGroupActive(GameObjects.SuccessTxtGroup, false);
        SetGroupActive(GameObjects.FailTxtGroup, true);
        SetGroupActive(GameObjects.PotenGroup, true);
        SetGroupActive(GameObjects.AttRealText, false);
        SetGroupActive(GameObjects.EnhAttDetail, false);

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
        _revealed = true;
        var data = _equip.EquipmentData;
        string nm = Util.Localize(data.Name);
        string nmEnh = _equip.EnhanceLevel > 0 ? $"+{_equip.EnhanceLevel} {nm}" : nm;

        RefreshDurability(data);

        if (_result.Success)
            yield return CoRevealSuccess(data, nm, nmEnh);
        else
            yield return CoRevealFail(data);
    }

    /// <summary>성공 연출. 원본 그래프의 'SuccessDone' 사슬을 그대로 따른다 —
    /// 요소를 켜는 **순서와 간격**이 곧 연출이고, 움직임 자체는 프리팹에 저작돼 있다.</summary>
    IEnumerator CoRevealSuccess(Data.EquipmentData data, string nm, string nmEnh)
    {
        Managers.Sound?.PlaySFX("SFX_Upgrade");   // 강화 성공음 (클립 미등록이면 경고만)

        // ── 글자를 먼저 채운다. 그룹이 아직 꺼져 있어 연출은 시작되지 않는다 ──
        // 프리팹에 오타 섞인 영문("ENHACE SUCCEEDED")이 박혀 한글 화면에 그대로 떴다.
        SetRevealText(Texts.Txt_SuccessTxtGroup_01, Util.Localize("EnhanceSucceededText"));
        SetRevealText(Texts.Txt_SuccessTxtGroup_03, nm);   // 아직 강화 전 이름

        // 잠재력은 **표 안의 한 줄**로 넣는다.
        //  프리팹의 PotenGroup은 원본에서 *실패* 연출용 자리다(FailDone 사슬에서만 켜진다).
        //  성공 화면에 켜 두니 능력치 표 아래에 큰 글씨가 하나 더 붙어 원본과 달라 보였다.
        SetGroupActive(GameObjects.PotenGroup, false);

        FillStatTable(data);

        // ── ① +0.20s : 제목/이름 · 아이콘 하강 · 빛줄기 ──
        yield return Wait(STEP_TITLE);

        StartCoroutine(CoEmbers());               // 화로는 계속 달아 있다 — 불티가 화면 내내 흩날린다

        // 강화창에서 시작된 백색 잔광이 아직 남아 있는 시점에 이 창이 열린다.
        //  까만 배경으로 덮어 버리면 그 잔광이 한 프레임에 뚝 끊긴다 — 이어받아 마저 걷는다.
        var bg = GetObject((int)GameObjects.Bg)?.GetComponent<Image>();
        if (bg != null)
        {
            bg.DOKill();
            bg.color = new Color(WASH_LEFT, WASH_LEFT, WASH_LEFT, 1f);
            bg.DOColor(Color.black, WASH_TAIL).SetEase(Ease.OutQuad).SetUpdate(true);
        }
        // 원본의 아이콘은 담금질에 떠오르고(id 5: +50, 1.4배) 결과가 확정될 때 한 번 더
        //  튀어오른다(id 6: +80). 결과창은 **그 꼭대기에서** 열려 내려앉기(id 7)를 시작한다.
        SnapAuthored(GameObjects.Icon, "5");      // 위치 +50 · 크기 1.4배
        SnapAuthored(GameObjects.Icon, "6");      // 거기서 한 번 더 +80까지

        Reveal(GameObjects.Flash_USETHIS);
        Reveal(GameObjects.SuccessTxtGroup);      // 제목·이름의 autoPlay 연출이 여기서 시작된다

        // 제목 위아래 가로선은 프리팹에서 **꺼진 채**라 그룹을 켜도 따라 켜지지 않는다 — 따로 깨운다.
        Reveal(GameObjects.Img_SuccessTxtGroup_02);
        Reveal(GameObjects._2);

        PlayAuthored(GameObjects.Icon, "7");      // 모루 위로 떴던 아이콘이 내려앉으며 원래 크기로
        ShowGodRays();

        // ── ② +1.70s : 내구도 + "능력치 변화" 라벨 ──
        yield return Wait(STEP_DETAIL);
        Reveal(GameObjects.EnhAttDetail);

        // 이름이 "철검"에서 "+5 철검"으로 **바뀐다**. 프리팹은 이걸 두 개의 텍스트로 만들어 뒀다 —
        //  원래 이름은 아래로 밀리며 사라지고(id 1), 강화된 이름이 제자리에서 톡 튀며 들어온다.
        //  영상에서 이 교체는 "능력치 변화" 헤더가 뜨는 것과 같은 순간이다.
        PlayAuthoredOn(Texts.Txt_SuccessTxtGroup_03, "1");
        SetRevealText(Texts._4, nmEnh);

        // ── ③ +0.25s : 능력치 표가 오른쪽에서 밀려 들어온다 ──
        yield return Wait(STEP_TABLE);
        SetGroupActive(GameObjects.AttRealText, true);
        yield return null;                        // 켜진 프레임에 DOTweenAnimation이 트윈을 만든다

        // 표는 한 번에 뜨지 않는다. 라벨이 먼저 서고, 값이 따라 붙고, 화살표가 번쩍인 뒤
        //  이전값이 걷히며 초록만 남는다 — 원본의 이 다섯 걸음이 통째로 빠져 있었다.
        // 원본은 네 열이 **0.1초씩 어긋나** 들어온다(실측 등장 프레임 f253/254/255/256).
        //  id "1" 하나로 셋을 한꺼번에 누르면 이전값·화살표·초록이 뭉쳐 나온다 —
        //  DORestartById가 오브젝트 단위라, 열마다 따로 부르면 그대로 시차가 생긴다.
        PlayAuthoredNode("Text", "");                         // ① 스탯 이름

        yield return Wait(TBL_STAGGER);
        PlayAuthoredNode("Txt_AttRealText_02", "1");          // ② 이전값

        yield return Wait(TBL_STAGGER);
        PlayAuthoredNode("Text_2", "1", dropColor: true);     // ③ 화살표
        SetArrowColor(ARROW_HEX);

        yield return Wait(TBL_STAGGER);
        PlayAuthoredNode("Text_3", "1");                      // ④ 초록 이후값

        yield return Wait(TBL_ARROW_HIT - TBL_STAGGER * 3f);
        FlashArrows();                                        // ③ 화살표가 속을 채우며 번쩍

        yield return Wait(TBL_SETTLE - TBL_ARROW_HIT);
        PlayAuthored(GameObjects.AttRealText, "2");           // ④ 이전값·화살표 소멸 + 초록이 왼쪽으로

        // 원본은 여기서 라벨을 첫 줄만 남기고 지운다(f270, 한 프레임). 그대로 옮겼더니
        //  "강화도만 글자가 남는다"는 지적을 받았다 — 어느 수치가 무엇인지 알 수 없어진다.
        //  원본의 그 한 걸음은 따르지 않는다.
    }

    IEnumerator CoRevealFail(Data.EquipmentData data)
    {
        Managers.Sound?.PlaySFX("SFX_Fail");   // 강화 실패음 (클립 미등록이면 경고만)

        SetRevealText(Texts.Txt_FailTxtGroup_02, Util.Localize("EnhanceFailedText"));

        // before / "/max" / after 가 프리팹에서 거의 같은 x에 겹쳐 놓여 있어
        //  "내구도 /900" 처럼 숫자들이 서로를 덮어썼다 → 한 줄로 합쳐 라벨에 싣고 나머지는 끈다.
        SetRevealText(Texts.Txt_FailTxtGroup_01,
            $"{Util.Localize(LocalizeKey.ForgeDurabilityText)} {_result.PrevDurability} ▶ " +
            $"<color=red>{_equip.Durability}</color>/{data.MaxDurability}");
        HideText(Texts._2_B4_Variable);
        HideText(Texts.Txt_FailTxtGroup_04);
        HideText(Texts._4_After_Variable);

        SetGroupActive(GameObjects.PotenGroup, false);   // 실패 — 잠재력 변화 없음

        StartCoroutine(CoEmbers());                     // 화로는 실패해도 계속 달아 있다

        // 실패도 원본은 곧장 결과를 보여주지 않는다(FailDone → +0.3 → 아이콘 id 9 → …).
        yield return Wait(STEP_TITLE);
        PlayAuthored(GameObjects.Icon, "9");

        // ⚠️ 실패창의 내구도 줄은 프리팹에서 **isFrom 트윈**이라, 만들어지는 순간
        //    알파 0으로 눌린 채 멈춰 있다. id "1"을 눌러 주지 않으면 글자를 넣어도 보이지 않는다
        //    (실제로 "강화 실패"만 뜨고 내구도가 통째로 사라져 있었다).
        PlayAuthored(GameObjects.FailTxtGroup, "1");
    }

    /// <summary>능력치 표 채우기.
    ///  ⚠️ 예전엔 **강화 보너스 누적합만** 물공 한 줄로 찍었다(+0 ▶ +6).
    ///     영상은 강화도와 함께 **장비 총합**(기본치 포함)을 보여주고, 같은 프로젝트의
    ///     장비 상세창도 이미 총합으로 찍는다 — 두 창이 서로 다른 숫자를 보여주고 있었다.
    ///  프리팹 행이 1줄(라벨/이전/▶/이후 4열)뿐이라 각 열에 개행으로 몰아쓴다(프리팹 무수정).</summary>
    void FillStatTable(Data.EquipmentData data)
    {
        RevealText(Texts.AttText);   // "능력치 변화" 라벨도 색을 되살려야 보인다

        var b = Managers.Game.GetEnhanceBonus(_equip.DataId, _result.PrevLevel);
        var a = Managers.Game.GetEnhanceBonus(_equip.DataId, _equip.EnhanceLevel);

        var names = new System.Collections.Generic.List<string>();
        var befs  = new System.Collections.Generic.List<string>();
        var afts  = new System.Collections.Generic.List<string>();

        // 표는 **네 줄 고정**이다. 원본은 안 변한 항목도 이름과 현재값을 찍고,
        //  그 줄에만 화살표와 초록 이후값을 비워 둔다. 변한 줄만 골라 넣으면
        //  강화 때마다 표의 줄 수가 들쭉날쭉해진다(원본은 늘 같은 네 줄이다).
        //  잠재력은 이 표에 없다 — 그 수치는 강화창 상단 게이지가 맡는다.
        _arrowRows = new System.Collections.Generic.List<bool>();

        void AddRow(string nm, string bef, string aft, bool changed)
        {
            names.Add(nm);
            befs.Add(bef);
            afts.Add(changed ? aft : "");   // 안 변한 줄은 이후값 자리를 비운다
            _arrowRows.Add(changed);
        }

        AddRow(Util.Localize(LocalizeKey.EnhanceLevelText),
               $"+{_result.PrevLevel}", $"+{_equip.EnhanceLevel}", true);
        AddRow(Util.Localize(LocalizeKey.PhysicalAttackText),
               $"+{data.BaseAtk + b.atk}", $"+{data.BaseAtk + a.atk}", a.atk != b.atk);
        AddRow(Util.Localize(LocalizeKey.CriticalText),
               $"+{data.BaseCritRate + b.crit:0.#}%", $"+{data.BaseCritRate + a.crit:0.#}%",
               !Mathf.Approximately(a.crit, b.crit));
        AddRow(Util.Localize(LocalizeKey.MagicAttackText),
               $"+{data.BaseMagicAtk + b.magic}", $"+{data.BaseMagicAtk + a.magic}",
               a.magic != b.magic);

        _statNames = names;

        SetTableText(Texts.Text, string.Join("\n", names));
        SetTableText(Texts.Txt_AttRealText_02, string.Join("\n", befs));

        // 화살표는 처음엔 **속이 빈 윤곽(▷)** 이고, 결과가 확정되는 순간에만 채워진다(▶).
        //  예전엔 처음부터 채운 ▶라 "번쩍" 하는 순간이 아예 없었다.
        // ⚠️ 화살표를 여기서 보이게 두면 **라벨보다 먼저** 떠 있게 된다(실측: 라벨보다 0.1초 빨랐다).
        //    다른 열은 저작 색이 알파 0이라 저절로 숨어 있는데, 화살표만 색을 코드로 넣기 때문이다.
        //    글자만 채워 두고 색은 자기 차례(id "1" 재생 뒤)에 얹는다.
        SetTableText(Texts.Text_2, ArrowRow("▷"));
        SetArrowAlpha(0f);

        // ⚠️ <color> 태그를 붙이면 프리팹에 저작된 초록(#0DDB00)과 그 페이드인을 덮어쓴다.
        //    글자를 넣기만 하고 색은 저작 Color 트윈(id "1")에 맡긴다.
        SetTableText(Texts.Text_3, string.Join("\n", afts));
    }

    /// <summary>EnhAttDetail 내구도 현황.
    ///  라벨과 수치가 프리팹에서 거의 같은 자리에 놓여 "내구도110/110"처럼 겹쳐 보였다 →
    ///  라벨을 수치 안으로 합치고 원래 라벨은 끈다(대장간 팝업과 같은 처리).
    ///  실패 연출에는 FailTxtGroup이 before▶after 내구도를 따로 보여주므로 여기선 통째로 숨긴다
    ///  (둘 다 켜져서 "내구도"가 두 번 겹쳐 찍혔다).</summary>
    void RefreshDurability(Data.EquipmentData data)
    {
        var duriLabel = GetText((int)Texts.DurabilityText);
        if (duriLabel != null) duriLabel.gameObject.SetActive(false);

        // 성공 화면에는 내구도를 띄우지 않는다 — 원본의 결과 화면에는 제목·이름·능력치 표뿐이고,
        //  내구도는 강화창이 이미 보여 준다. 아이콘 바로 밑에 한 줄이 더 붙어 있어
        //  장비 아이콘의 자리가 어긋나 보였다.
        var duriValue = GetText((int)Texts.DurabilityRealText);
        if (duriValue != null) duriValue.gameObject.SetActive(false);
    }

    System.Collections.Generic.List<string> _statNames;

    System.Collections.Generic.List<bool> _arrowRows;

    /// <summary>화살표 열 — **변한 줄에만** 글리프를 놓고 나머지는 빈 줄로 둔다.
    /// 줄 수가 스탯 표와 같아야 칸이 맞는다.</summary>
    string ArrowRow(string glyph)
    {
        if (_arrowRows == null) return string.Empty;

        var lines = new string[_arrowRows.Count];
        for (int i = 0; i < _arrowRows.Count; i++) lines[i] = _arrowRows[i] ? glyph : "";
        return string.Join("\n", lines);
    }

    /// <summary>표 네 열 전용. MakeVisible을 태우지 않는다 —
    /// 저작 색은 알파 0에서 시작해야 저작 페이드인(0.1~0.3초)이 살아난다.</summary>
    void SetTableText(Texts t, string value)
    {
        var txt = GetText((int)t);
        if (txt == null) return;

        txt.text = value;
        ApplyFont(txt, FontType.UI);
    }

    void SetArrowColor(string hex)
    {
        var t = GetText((int)Texts.Text_2);
        if (t != null) t.color = Util.HexToColor(hex);
    }

    void SetArrowAlpha(float a)
    {
        var t = GetText((int)Texts.Text_2);
        if (t == null) return;
        var c = t.color; c.a = a; t.color = c;
    }

    /// <summary>화살표가 속을 채우며 한 번 번쩍인다(영상 f265~f266, 두 프레임).
    /// 곧이어 id "2"가 이 열을 걷어가지만, 건너뛰기 경로를 위해 원래 모습으로 되돌려 둔다.</summary>
    void FlashArrows()
    {
        var t = GetText((int)Texts.Text_2);
        if (t == null) return;

        t.text = ArrowRow("▶");
        SetArrowColor(ARROW_HEX);      // 번쩍이는 순간에도 색은 같다 — 채워지는 건 글리프뿐

        DOVirtual.DelayedCall(ARROW_FLASH, () =>
        {
            if (t != null) t.text = ArrowRow("▷");
        }).SetUpdate(true);
    }

    static WaitForSecondsRealtime Wait(float sec) => new WaitForSecondsRealtime(sec);

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

    #region 저작 연출 구동

    // 이 창의 움직임은 **코드가 아니라 프리팹에 들어 있다**. Icon 하나에만
    //  DOTweenAnimation이 12개 붙어 있고 전부 autoPlay=false — id("1"~"9")로 호출되기를
    //  기다리는 상태다. 원본(비주얼 스크립팅)이 애니메이션 이벤트마다 DORestartById로
    //  그 id를 눌러 연출을 지휘했다.
    //
    //  ⚠️ 예전 포트는 이걸 모르고 코드로 DOAnchorPos/DOScale을 새로 짜 넣었다.
    //     저작 트윈과 같은 RectTransform을 두고 싸웠고, DOKill()이 진행 중이던
    //     Scale 트윈을 중간에 죽여 **아이콘이 부푼 채 굳는** 버그까지 생겼다.
    //     이제는 저작된 것을 **호출만** 한다.
    //
    //  Icon의 id 지도 (프리팹에서 읽음):
    //    1 흔들림(타격) / 2 제자리로 / 3 잔진동 / 4 펀치 / 5 떠오르며 확대(담금질)
    //    6 위로 튐(성공 순간) / 7 내려앉으며 원래 크기 / 8 즉시 원위치(실패) / 9 살짝 상승

    /// <summary>node 하위에서 그 id를 가진 저작 트윈을 처음부터 재생한다.</summary>
    void PlayAuthored(GameObjects node, string id)
    {
        var root = GetObject((int)node);
        if (root == null) return;

        foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
        {
            if (anim.id != id) continue;

            // ⚠️ **빈 id는 DORestartById로 못 찾는다.** DOTweenAnimation은 id가 비어 있으면
            //    트윈에 SetId를 걸지 않는다(DOTweenAnimation.cs:508) — ById 조회의 열쇠가
            //    아예 없는 셈이다. 스탯 이름 열이 정확히 그 경우라, 이 호출이 오랫동안
            //    **아무 일도 하지 않고 있었다**(라벨이 슬며시 들어오는 연출이 통째로 빠졌다).
            if (string.IsNullOrEmpty(id)) anim.tween?.Restart();
            else anim.DORestartById(id);
        }
    }

    /// <summary>화살표 열의 색을 되돌려 놓는다.
    ///
    /// 화살표 열에는 **검정으로 끝나는** 저작 Color 트윈(id "1")이 걸려 있어,
    ///  까만 배경 위에서 화살표가 등장 0.1초 만에 사라진다. 원본은 처음부터 끝까지
    ///  붉은 ▷다.
    /// ⚠️ 그 컴포넌트만 골라 건너뛰는 방법은 통하지 않는다 — `DORestartById`는
    ///    **GameObject 단위**로 도는지라(DOTween.Restart(gameObject, id)), 같은 칸의
    ///    Move를 재생하는 순간 Color도 함께 되살아난다. 그래서 저작 트윈이 끝난 뒤
    ///    (0.1초짜리다) 색만 다시 얹는다.</summary>


    /// <summary>표의 **한 열만** 재생한다.
    ///
    /// `DORestartById`는 GameObject 단위라, 같은 id를 가진 네 열을 한 번에 부르면
    ///  전부 동시에 나온다. 열마다 따로 부르면 원본처럼 0.1초씩 어긋난다.
    ///
    /// `dropColor`는 화살표 열 전용이다 — 그 열의 저작 Color는 **검정으로 끝나서**
    ///  까만 배경 위에서 화살표를 지운다. 되살아나지 못하게 아예 끊고 색은 코드가 준다.
    ///  (같은 오브젝트의 Move를 재생하는 순간 Color도 함께 살아나므로, 끊기를 먼저 한다.)</summary>
    void PlayAuthoredNode(string childName, string id, bool dropColor = false)
    {
        var node = Util.FindChild(gameObject, childName, true);
        if (node == null) return;

        var anims = node.GetComponents<DOTweenAnimation>();

        if (dropColor)
            foreach (var a in anims)
                if (a.id == id && a.animationType == DOTweenAnimation.AnimationType.Color)
                    a.tween?.Kill();

        foreach (var a in anims)
        {
            if (a.id != id) continue;
            if (dropColor && a.animationType == DOTweenAnimation.AnimationType.Color) continue;

            // 빈 id는 트윈에 id가 붙지 않아 ById로 못 찾는다(DOTweenAnimation.cs:508).
            if (string.IsNullOrEmpty(id)) a.tween?.Restart();
            else a.DORestartById(id);
        }
    }

    /// <summary>저작 트윈 id의 **끝 상태로 즉시 옮겨 놓는다**(재생하지 않는다).
    ///
    /// 원본에서는 담금질(ChargeSuccess) 순간 아이콘이 모루 위로 떠오르며 커진다(id 5).
    ///  그런데 우리 포트는 그 순간 이 창이 아직 열려 있지 않다 — 망치질은 강화창이,
    ///  결과는 이 창이 맡기 때문이다. 그래서 창이 열릴 때 **이미 떠 있는 자세**로 시작해야
    ///  뒤이은 하강(id 7)이 실제로 내려오는 것처럼 보인다. 값은 프리팹에서 읽는다.</summary>
    void SnapAuthored(GameObjects node, string id)
    {
        var root = GetObject((int)node);
        if (root == null) return;

        foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
        {
            if (anim.id != id) continue;

            switch (anim.animationType)
            {
                case DOTweenAnimation.AnimationType.LocalMove:
                    anim.transform.localPosition = anim.endValueV3;
                    break;
                case DOTweenAnimation.AnimationType.Scale:
                    anim.transform.localScale = Vector3.one * anim.endValueFloat;
                    break;
            }
        }
    }

    void PlayAuthoredOn(Texts t, string id)
    {
        var txt = GetText((int)t);
        if (txt == null) return;

        foreach (var anim in txt.GetComponents<DOTweenAnimation>())
            if (anim.id == id) { anim.DORestartById(id); break; }
    }

    /// <summary>성공 순간의 빛줄기.
    ///
    /// 프리팹의 God_ray_02는 **월드 ParticleSystem**이다. Screen Space Camera 캔버스 위에는
    ///  그려지지 않아 켜 두어도 화면에 아무것도 안 나온다 — 원본 영상에서 가장 눈에 띄는
    ///  연출이 통째로 빠져 보였던 이유다.
    ///
    /// UIParticle로 감싸는 길도 시도했으나 두 가지가 걸렸다:
    ///  ① 자기 GameObject의 스케일을 20배로 고쳐 써 창 전체가 뒤틀렸고,
    ///  ② Mobile/Particles/Additive 재질이 캔버스 경로에서 아무것도 그리지 않았다.
    /// 원본 영상의 빛줄기는 사실 **거의 정지한 한 장의 그림**이다(±8px 흔들림뿐).
    ///  그래서 그 파티클이 쓰던 **텍스처를 그대로** 가져와 Image 한 장으로 깐다.
    ///
    /// 밝기는 숨을 쉰다 — 등장 시 최대의 45%, 0.9초 뒤 최소, 2초에 걸쳐 최대, 그 뒤 가라앉는다.
    void ShowGodRays()
    {
        var rays = GetObject((int)GameObjects.God_ray_02);
        if (rays == null) return;

        if (_rayImg == null)
        {
            var psr = rays.GetComponent<ParticleSystemRenderer>();
            var tex = psr != null && psr.sharedMaterial != null
                    ? psr.sharedMaterial.mainTexture as Texture2D : null;
            if (tex == null) return;

            var parent = rays.transform.parent;
            var go = new GameObject("GodRayImg", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            // 원본의 빛줄기는 화면 위에서 내려와 **중단쯤에서 스러진다**(y35~455 / 854).
            //  프레임 전체를 덮으면 어두운 대장간이 통째로 흐려져 안개처럼 보인다.
            rt.anchorMin = new Vector2(-0.25f, 0.30f);
            rt.anchorMax = new Vector2(1.25f, 1.15f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localRotation = Quaternion.Euler(0f, 0f, RAY_TILT);
            rt.SetSiblingIndex(rays.transform.GetSiblingIndex());   // 배경 위, 다리 난간 아래

            _rayImg = go.AddComponent<Image>();
            _rayImg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                           new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            _rayImg.raycastTarget = false;
            _rayImg.preserveAspect = false;
        }

        _rayImg.gameObject.SetActive(true);
        _rayImg.DOKill();
        _rayImg.color = new Color(1f, 1f, 1f, RAY_IN);        // 페이드 없이 곧바로 켜진다

        DOTween.Sequence().SetUpdate(true)
            .Append(_rayImg.DOFade(RAY_LOW,  0.9f))
            .Append(_rayImg.DOFade(RAY_HIGH, 2.0f).SetEase(Ease.InOutSine))
            .Append(_rayImg.DOFade(RAY_REST, 1.6f));
    }

    const float RAY_TILT = -12f;    // 원본의 좌경사 (y 160px당 좌로 33px)
    // 원본의 빛줄기 기여도는 255 중 24~55다 — 알파로 옮기면 0.09~0.22.
    //  이보다 올리면 대장간이 하얗게 떠 버린다(처음 그렇게 만들었다가 안개가 됐다).
    const float RAY_IN   = 0.10f;   // 등장 순간 = 최대의 45%
    const float RAY_LOW  = 0.075f;
    const float RAY_HIGH = 0.22f;
    const float RAY_REST = 0.12f;

    Image _rayImg;

    // ── 불티 ──────────────────────────────────────────────────────────
    //  원본은 결과 화면 **내내** 주황 불티가 흩날린다(프레임 001~060 전 구간).
    //  대장간이 아직 달아 있다는 신호라 이게 없으면 화면이 죽은 그림처럼 보인다.
    //  프리팹의 Dust_01은 월드 파티클이라 캔버스에 안 그려진다 — 빛줄기와 같은 이유로
    //  UI Image 몇 장을 주기적으로 띄워 대신한다.
    const float EMBER_INTERVAL = 0.16f;
    const int   EMBER_PER_TICK = 2;

    IEnumerator CoEmbers()
    {
        var host = GetObject((int)GameObjects.God_ray_02)?.transform.parent as RectTransform;
        if (host == null) yield break;

        var wait = new WaitForSecondsRealtime(EMBER_INTERVAL);
        while (true)
        {
            for (int i = 0; i < EMBER_PER_TICK; i++) UIFx.SpawnEmber(host);
            yield return wait;
        }
    }


    #endregion

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
