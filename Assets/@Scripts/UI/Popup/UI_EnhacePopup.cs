using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

/// <summary>
/// 강화 실행 팝업(모루/망치 씬) — UI_ForgePopup에서 주문서 선택 후 [강화]로 진입.
/// 성공확률/실패 리스크(내구도 감소량)/잠재력 게이지 표시 + [강화] 실행.
///  · [영상 건너뛰기] 체크: 결과 플래시(Flash_Success/Fail)만 재생하고 팝업 유지(연속 강화).
///  · 미체크: UI_EnhanceProgressPopup(결과 연출)로 진행.
///  · [주문서 슬롯]/[잠재 슬롯] 두 박스를 눌러 각각 선택 팝업을 연다(원본 영상과 동일).
/// SOM(돌파)만 스코프 제외.
/// </summary>
public class UI_EnhacePopup : UI_Popup
{
    #region Enum

    enum Images
    {
        WIcon,                  // 장비 아이콘 (CraftEquipIcon 하위)
    }

    enum Texts
    {
        ENumber,                // "+N"
        PotenForceBarBarText,   // 잠재력 수치
        ChanceTxt,              // 성공확률 "90%"
        RiskNum,                // 실패 시 내구도 감소량
        EnhEquipNameText,       // "+N 장비명"
        DurabilityText,         // "내구도" 라벨
        DurabilityRealText,     // "cur/max"
        AmountText,             // 주문서 보유 수량 (prefab INACTIVE → 코드가 켠다)
    }

    enum Buttons
    {
        Enhance_Btn,
        EnhancePopupCloseBtn,   // 투명 전체화면 닫기 (prefab interactable=False → 코드 활성)
    }

    enum GameObjects
    {
        EnhacePopup,            // 프레임 루트
        FlameImg,               // 잠재 박스의 불꽃 (prefab INACTIVE → 코드가 켠다)
        PlusText,               // 주문서 미선택 시 "+"
        EnhAttDetail,           // 이름/내구도/능력치 미리보기 묶음 — 연출 중 숨긴다
        CraftEquipIcon,         // 모루 위 장비 아이콘 — 담금질에 떠오른다
        PotenForceBarGroup,     // 잠재력 슬라이더
        Holder,                 // 슬라이더 Fill 홀더 (prefab INACTIVE → 활성)
        PotenSelectPopup,       // 잠재 슬롯 선택 — 스코프 제외(비활성)
        EnhScrollSelectPopup,   // 주문서 재선택 — 스코프 제외(ForgePopup에서 선택 완료)
        PotentialBox,           // 잠재 "+" 박스 — 스코프 제외
        Btn_EnhacePopup_01,     // 주문서 "+" 박스 — 스코프 제외
        RiskGroup,
        text,                   // RiskGroup 라벨 "필요/실패 시 내구도 감소량" (prefab INACTIVE)
        isSkipBtn,              // 영상 건너뛰기 Toggle
        Flash_Success,
        Flash_Fail,
        HammerNormalAnim,
    }

    #endregion

    Equipment _equip;
    int _scrollId;
    Toggle _skipToggle;
    bool _resolving;   // 강화 연출 중 중복 클릭 방지

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // 선택 팝업은 닫힌 채로 시작한다(버튼을 눌러야 열린다).
        GetObject((int)GameObjects.PotenSelectPopup)?.SetActive(false);
        GetObject((int)GameObjects.EnhScrollSelectPopup)?.SetActive(false);
        GetObject((int)GameObjects.Holder)?.SetActive(true);
        GetObject((int)GameObjects.text)?.SetActive(true);

        // ⚠️ 예전엔 이 두 박스를 "스코프 제외"라며 통째로 꺼 뒀다. 원본 영상에서는
        //  [영상 건너뛰기] 위에 **두 개의 박스**가 있고, 거기에 강화 주문서와 잠재력을 넣는다.
        //  꺼 두니 강화창에서 주문서를 바꿀 방법이 아예 없었다.
        GetObject((int)GameObjects.PotentialBox)?.SetActive(true);
        GetObject((int)GameObjects.Btn_EnhacePopup_01)?.SetActive(true);

        // 불꽃은 Init이 아니라 PlayIntro가 붙인다 — 상자가 자리에 앉은 다음이다.

        BindSlotButtons();

        // ⚠️ 망치가 대기 화면에서 **보이지 않았다.** Image 알파가 0이고, 그걸 1로 올리는 것은
        //    Normal.anim의 커브뿐인데 애니메이터의 기본 상태가 빈 상태(New State)라
        //    그 클립이 한 번도 돌지 않았다 — [강화]를 눌러야 비로소 망치가 생겨났다.
        //    창을 열 때 대기 자세를 직접 재생한다(연출 소리는 _ceremony 게이트가 막는다).
        var smithGo = GetObject((int)GameObjects.HammerNormalAnim);
        var smithAnim = smithGo != null ? smithGo.GetComponent<Animator>() : null;
        if (smithAnim != null) smithAnim.Play("Normal", 0, 0f);

        GetButton((int)Buttons.Enhance_Btn).onClick.AddListener(OnClickEnhance);

        var closeBtn = GetButton((int)Buttons.EnhancePopupCloseBtn);
        closeBtn.interactable = true;
        closeBtn.onClick.AddListener(OnClickClose);

        _skipToggle = GetObject((int)GameObjects.isSkipBtn)?.GetComponent<Toggle>();

        if (Managers.Game != null)
            Managers.Game.EquipInfoChanged += OnEquipInfoChanged;

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Managers.Game != null)
            Managers.Game.EquipInfoChanged -= OnEquipInfoChanged;
    }

    #region 연출 중 화면 정리 / 순차 등장

    // 원본 영상에서 [강화]를 누르면 화면이 **비워진다** — 이름·내구도·확률·슬롯·버튼이
    //  전부 사라지고 모루 위의 망치와 장비만 남는다. 그래야 타격에 눈이 간다.
    //  우리 창은 강화 중에도 표가 그대로 떠 있어 어수선했다.
    static readonly GameObjects[] CEREMONY_HIDE =
    {
        GameObjects.EnhAttDetail,      // 이름 / 내구도 / 능력치 미리보기
        GameObjects.PotenForceBarGroup,// 상단 잠재력 게이지
        GameObjects.PotentialBox,      // 잠재 슬롯
        GameObjects.Btn_EnhacePopup_01,// 주문서 슬롯
        GameObjects.RiskGroup,
        GameObjects.isSkipBtn,         // 영상 건너뛰기
    };

    readonly System.Collections.Generic.List<GameObject> _hidden = new System.Collections.Generic.List<GameObject>();

    /// <summary>연출 시작 — 부수적인 UI를 접는다. 껐다는 사실을 기억해 나중에 정확히 되돌린다.</summary>
    void EnterCeremonyView()
    {
        Blackout(STAGE_CEREMONY, 0.35f);
        LandCraftIcon();                        // 장비가 모루 위로 내려앉는다
        _embers = StartCoroutine(CoEmbers());   // 까만 무대 위로 불티가 흩날린다(원본 전 구간)
        _hidden.Clear();

        // (표는 EnhAttDetail 아래에 있어 CEREMONY_HIDE[0]이 부모째 접는다)

        foreach (var g in CEREMONY_HIDE)
        {
            var go = GetObject((int)g);
            if (go == null || !go.activeSelf) continue;
            go.SetActive(false);
            _hidden.Add(go);
        }

        // 확률/버튼은 오브젝트가 아니라 텍스트라 따로 접는다.
        FadeText(Texts.ChanceTxt, 0f);
        var btn = GetButton((int)Buttons.Enhance_Btn);
        if (btn != null) btn.gameObject.SetActive(false);

        // 선택 팝업이 열려 있었다면 닫는다.
        GetObject((int)GameObjects.EnhScrollSelectPopup)?.SetActive(false);
        GetObject((int)GameObjects.PotenSelectPopup)?.SetActive(false);
    }

    /// <summary>연출 끝 — 접었던 것만 되돌린다(원래 꺼져 있던 것은 꺼진 채로).</summary>
    void ExitCeremonyView()
    {
        Blackout(STAGE_IDLE, 0.30f);
        GlowWash(0f, 0.30f, Ease.OutQuad);        // 백열이 남아 있으면 대기 화면이 하얗게 뜬다
        RestoreCraftIcon(0f);                     // 떠 있던 아이콘을 제자리로
        if (_embers != null) { StopCoroutine(_embers); _embers = null; }
        foreach (var go in _hidden)
            if (go != null) go.SetActive(true);
        _hidden.Clear();

        FadeText(Texts.ChanceTxt, 1f);
        var btn = GetButton((int)Buttons.Enhance_Btn);
        if (btn != null) btn.gameObject.SetActive(true);
    }

    /// <summary>연출 동안 화면 전체를 검게 덮는다.
    ///
    /// 원본 영상에서 [강화]를 누르면 **마을 배경도, 상단 HUD도, 하단 탭도 전부 사라진다** —
    ///  까만 무대에 모루와 망치만 남는다. 우리 창은 팝업 안쪽만 정리해서, 뒤로 캐릭터 초상화와
    ///  체력바가 그대로 비쳤다. 뒤에 있는 것들은 다른 캔버스라 이 창의 SetActive로는 못 끈다 —
    ///  이 팝업 캔버스 맨 아래에 검은 판을 깔면 한 번에 덮인다.
    ///
    /// 판은 흔들리는 프레임이 아니라 **팝업 루트**에 붙인다(같이 흔들리면 가장자리가 새어 나온다).
    void Blackout(Color target, float duration)
    {
        if (_blackout == null)
        {
            var go = new GameObject("CeremonyBlackout", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-BLACKOUT_OVERSCAN, -BLACKOUT_OVERSCAN);
            rt.offsetMax = new Vector2(BLACKOUT_OVERSCAN, BLACKOUT_OVERSCAN);
            rt.SetAsFirstSibling();          // 창 내용보다 뒤, 게임 화면보다 앞

            _blackout = go.AddComponent<Image>();
            _blackout.color = new Color(0f, 0f, 0f, 0f);
            _blackout.raycastTarget = true;  // 연출 중 뒤쪽 클릭을 막는다
        }

        _blackout.DOKill();
        _blackout.gameObject.SetActive(true);

        if (duration <= 0f) { _blackout.color = target; return; }
        _blackout.DOColor(target, duration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    // 불티는 프레임(흔들리는 노드)에 붙여야 모루와 함께 움직인다.
    Coroutine _embers;
    const float EMBER_INTERVAL = 0.16f;
    const int   EMBER_PER_TICK = 2;

    IEnumerator CoEmbers()
    {
        var host = _frameRT != null ? _frameRT
                 : GetObject((int)GameObjects.EnhacePopup)?.GetComponent<RectTransform>();
        if (host == null) yield break;

        var wait = new WaitForSecondsRealtime(EMBER_INTERVAL);
        while (true)
        {
            for (int i = 0; i < EMBER_PER_TICK; i++) UIFx.SpawnEmber(host);
            yield return wait;
        }
    }

    Image _blackout;
    const float BLACKOUT_OVERSCAN = 600f;   // 캔버스보다 넉넉히 — 가장자리가 새지 않게

    // 무대 색은 원본 프레임에서 재서 옮겼다.
    //  대기 중에는 **완전한 검정이 아니라 어두운 적색**이다(RGB 24,0,0) — 화로의 열기다.
    //  망치질에 들어가면 그마저 꺼지고 순수한 검정이 된다(측정 밝기 10).
    static readonly Color STAGE_IDLE     = new Color(24f / 255f, 1f / 255f, 1f / 255f, 1f);
    static readonly Color STAGE_CEREMONY = new Color(0f, 0f, 0f, 1f);

    // 성공하면 화면이 통째로 하얗게 타올랐다가 1.5초에 걸쳐 걷힌다(원본 221.9~223.4s).
    //  예전엔 0.5초짜리 섬광 한 번뿐이라 "번쩍"하고 마는 정도였다.
    const float SUCCESS_WASH = 1.5f;

    void FadeText(Texts t, float alpha)
    {
        var txt = GetText((int)t);
        if (txt == null) return;
        var c = txt.color; c.a = alpha; txt.color = c;
    }

    // ── 순차 등장 ──────────────────────────────────────────────────
    //  원본은 창이 통째로 뜨지 않는다. 망치가 위에서 떨어지고, 모루·장비·내구도·게이지·
    //  슬롯·불꽃·능력치표가 차례로 붙는다. 순서와 간격은 영상에서 읽은 값이다.
    static readonly (GameObjects go, float delay)[] INTRO_STEPS =
    {
        (GameObjects.HammerNormalAnim,   0.00f),   // 망치가 먼저 떨어진다
        (GameObjects.EnhAttDetail,       0.35f),   // 장비 이름 / 내구도
        (GameObjects.PotenForceBarGroup, 0.60f),   // 상단 게이지가 좌우로 펼쳐진다
        (GameObjects.PotentialBox,       0.80f),   // 잠재 슬롯 + 불꽃
        (GameObjects.Btn_EnhacePopup_01, 0.95f),   // 주문서 슬롯
        (GameObjects.isSkipBtn,          1.10f),   // 영상 건너뛰기
        (GameObjects.RiskGroup,          1.20f),
    };

    bool _introPlayed;

    /// <summary>처음 열릴 때 한 번만. 다시 강화할 때마다 재생하면 지겹다(원본도 1회다).</summary>
    void PlayIntro()
    {
        if (_introPlayed) return;
        _introPlayed = true;
        IgniteFlame();

        foreach (var (g, delay) in INTRO_STEPS)
        {
            var go = GetObject((int)g);
            if (go == null) continue;

            var rt = go.transform as RectTransform;
            if (rt == null) continue;

            Vector2 home = rt.anchoredPosition;
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            rt.anchoredPosition = home + new Vector2(0f, INTRO_DROP);

            cg.DOKill();
            rt.DOKill();
            cg.DOFade(1f, INTRO_FADE).SetDelay(delay).SetUpdate(true);
            rt.DOAnchorPos(home, INTRO_FADE).SetDelay(delay)
              .SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    const float INTRO_DROP = 24f;    // 위에서 떨어지는 거리
    // 잠재 슬롯 등장(0.80)이 끝나는 지점 = 원작의 상자 착지 직후 점화 시점
    const float FLAME_IGNITE = 0.80f + INTRO_FADE;

    /// <summary>잠재 슬롯이 내려앉은 뒤 불이 붙는다. Flame_Start(0.583초)가
    /// 작은 불씨에서 긴 불길로 자라는 대목이라, 이게 보여야 "아래에서 위로 올라온다".</summary>
    void IgniteFlame()
    {
        var flameGo = GetObject((int)GameObjects.FlameImg);
        if (flameGo == null) return;

        flameGo.SetActive(false);
        DOVirtual.DelayedCall(FLAME_IGNITE, () =>
        {
            if (flameGo == null) return;
            flameGo.SetActive(true);
            flameGo.GetComponent<Animator>()?.Play("Flame_Start", 0, 0f);   // 처음부터 확실히
        }).SetUpdate(true);
    }
    const float INTRO_FADE = 0.28f;

    #endregion

    #region 주문서 / 잠재 슬롯

    // 선택 팝업 안의 칸 이름. 프리팹이 정한 순서라 코드가 맞춘다
    //  (Btn_Layout_02가 1번 칸이다 — 이름만 다르고 역할은 같다).
    static readonly string[] SCROLL_BOX_NODES =
    {
        "Btn_Layout_02", "EnhScrollBox2", "EnhScrollBox3", "EnhScrollBox4",
    };

    void BindSlotButtons()
    {
        // 주문서 박스 — 누르면 주문서 선택 팝업
        var scrollBox = GetObject((int)GameObjects.Btn_EnhacePopup_01);
        if (scrollBox != null) scrollBox.BindEvent(ToggleScrollSelect);

        // 잠재 박스 — 누르면 잠재 선택 팝업
        var potenBox = GetObject((int)GameObjects.PotentialBox);
        if (potenBox != null) potenBox.BindEvent(TogglePotenSelect);

        // 선택 팝업의 각 칸
        var sel = GetObject((int)GameObjects.EnhScrollSelectPopup);
        if (sel != null)
        {
            for (int i = 0; i < SCROLL_BOX_NODES.Length && i < ENHANCE_SCROLL_IDS.Length; i++)
            {
                var node = Util.FindChild(sel, SCROLL_BOX_NODES[i], true);
                if (node == null) { Debug.Log($"[EnhacePopup] 주문서 칸 없음: {SCROLL_BOX_NODES[i]}"); continue; }
                int idx = i;
                node.BindEvent(() => OnPickScroll(idx));
            }
        }
    }

    void ToggleScrollSelect()
    {
        if (_resolving) return;
        var sel = GetObject((int)GameObjects.EnhScrollSelectPopup);
        if (sel == null) return;

        bool on = !sel.activeSelf;
        GetObject((int)GameObjects.PotenSelectPopup)?.SetActive(false);
        sel.SetActive(on);
        if (on) { sel.transform.SetAsLastSibling(); RefreshScrollSelect(); PopIn(sel); }
        Managers.Sound?.PlaySFX("SFX_BtnSelect");
    }

    void TogglePotenSelect()
    {
        if (_resolving) return;
        var sel = GetObject((int)GameObjects.PotenSelectPopup);
        if (sel == null) return;

        bool on = !sel.activeSelf;
        GetObject((int)GameObjects.EnhScrollSelectPopup)?.SetActive(false);
        sel.SetActive(on);
        // 프리팹 순서상 장비 아이콘보다 뒤라 아이콘이 팝업을 뚫고 나왔다 — 열 때 맨 앞으로.
        if (on) { sel.transform.SetAsLastSibling(); PopIn(sel); }
        Managers.Sound?.PlaySFX("SFX_BtnSelect");
    }

    /// <summary>선택 팝업이 톡 튀어나오게. 원본도 즉시 나타나지 않고 살짝 커지며 뜬다.</summary>
    static void PopIn(GameObject go)
    {
        var rt = go.transform as RectTransform;
        if (rt == null) return;
        rt.DOKill();
        rt.localScale = Vector3.one * 0.8f;
        rt.DOScale(1f, 0.18f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    /// <summary>선택 팝업의 네 칸에 아이콘과 보유 수량을 채운다. 없는 주문서는 흐리게.</summary>
    void RefreshScrollSelect()
    {
        var sel = GetObject((int)GameObjects.EnhScrollSelectPopup);
        if (sel == null) return;

        for (int i = 0; i < SCROLL_BOX_NODES.Length && i < ENHANCE_SCROLL_IDS.Length; i++)
        {
            var node = Util.FindChild(sel, SCROLL_BOX_NODES[i], true);
            if (node == null) continue;

            int id = ENHANCE_SCROLL_IDS[i];
            int have = Managers.Game.GetItemCount(id);

            // ⚠️ 칸 그림은 **건드리지 않는다.**
            //  프리팹의 네 칸에는 이미 등급별 배경(초록/파랑/보라/빨강)과 그에 맞는
            //  주문서 그림이 저작돼 있다. 예전엔 첫 번째 "Img_"에 ScrollSprite를 덮어써서
            //  네 칸이 전부 같은 회색 그림이 됐다 — 원본은 등급 색으로 구분한다.
            //  보유하지 않은 주문서만 흐리게 눌러 준다.
            PaintScrollCell(node, i, have > 0);
            foreach (var t in node.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                if (!t.name.StartsWith("Txt_")) continue;
                t.text = ShortCount(have);
                t.color = have > 0 ? Color.white : Util.HexToColor("FF5555");
                ApplyFont(t, FontType.UI);
                SeatAmountBadge(t, CELL_BADGE_SIZE);
                break;
            }
        }
    }

    /// <summary>수량 라벨을 칸 **안쪽 아래**에 앉힌다.
    ///
    /// 프리팹의 수량 텍스트는 `anchor/pivot = (0.5, 0)`에 `sizeDelta.y = 1`(슬롯 쪽은 0),
    ///  `anchoredPosition.y = -1`로 저작돼 있다. 글자 높이가 들어갈 자리가 없는 데다
    ///  기준점이 칸 **바닥**이라, 숫자가 칸 밑으로 흘러내려 테두리 밖에 걸린다
    ///  (수량이 한 자리인 원본에서도 아슬아슬했고, 네 자리가 되자 확실히 새어 나왔다).
    ///  높이를 주고 안쪽으로 끌어올려 배지처럼 앉힌다.
    static void SeatAmountBadge(TMPro.TMP_Text txt, float fontSize)
    {
        if (txt == null) return;

        var rt = txt.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 18f), fontSize + 2f);
        rt.anchoredPosition = new Vector2(0f, BADGE_INSET);

        txt.fontSize = fontSize;
        txt.alignment = TMPro.TextAlignmentOptions.Bottom;
        txt.enableWordWrapping = false;
        txt.overflowMode = TMPro.TextOverflowModes.Overflow;
    }

    const float BADGE_INSET = 1.5f;   // 칸 바닥에서 살짝 올려 테두리를 밟지 않게
    const float CELL_BADGE_SIZE = 6f;  // 선택 팝업 칸 (프리팹 저작값)
    const float SLOT_BADGE_SIZE = 6f;  // 슬롯 겉면 — 저작값 10은 칸보다 커서 밖으로 삐져나왔다

    /// <summary>네 자리 수량은 칸 폭을 넘는다 — 원본은 한두 자리였다.</summary>
    static string ShortCount(int n) => n > 999 ? "999+" : n.ToString();

    // 원본은 칸마다 **등급 색**을 깔아 네 주문서를 색으로 구분한다(초록/파랑/보라/빨강).
    //  우리 프리팹의 네 칸은 전부 같은 그림에 검은 70% 덮개라, 넷이 똑같은 회색으로 보였다.
    //  덮개의 색을 등급 색으로 바꾸고 그림만 갈아 끼운다(구조는 그대로 둔다).
    static readonly Color[] SCROLL_GRADE_TINT =
    {
        new Color(0.16f, 0.50f, 0.18f, 1f),      // 일반
        new Color(0.13f, 0.32f, 0.72f, 1f),      // 상급
        new Color(0.46f, 0.16f, 0.70f, 1f),      // 고대
        new Color(0.66f, 0.13f, 0.15f, 1f),      // 이그누스의 인정
    };

    // 칸의 자식 순서 — 프리팹이 정한 구조다(네 칸 모두 같다).
    //  0 테두리 / 1 마스크+광택 / 2 주문서 그림 / 3 덮개
    const int CELL_FRAME   = 0;
    const int CELL_PICTURE = 2;
    const int CELL_VEIL    = 3;

    /// <summary>칸 하나를 등급 색으로 칠하고 그 주문서 그림을 넣는다.
    /// ⚠️ 칸 안의 Image를 이름으로 뭉뚱그려 칠하면 안 된다 — 알파 0으로 숨겨 둔 흰 판이 섞여 있어
    ///    한꺼번에 흰색을 먹이면 칸이 통째로 하얘진다(실제로 그렇게 만들었다가 되돌렸다).</summary>
    void PaintScrollCell(GameObject node, int idx, bool owned)
    {
        if (node == null || idx < 0 || idx >= SCROLL_GRADE_TINT.Length) return;
        if (node.transform.childCount <= CELL_VEIL) return;

        // 색은 **그림 뒤쪽 테두리**에 넣는다. 맨 위 덮개에 칠하면 주문서 그림까지 물들어
        //  파스텔처럼 흐려진다(원본은 그림이 또렷하고 배경만 색이다).
        var frame = node.transform.GetChild(CELL_FRAME).GetComponent<Image>();
        if (frame != null) frame.color = owned ? SCROLL_GRADE_TINT[idx] : new Color(0.22f, 0.22f, 0.22f, 1f);

        var veil = node.transform.GetChild(CELL_VEIL).GetComponent<Image>();
        if (veil != null)
        {
            // 덮개는 옅게만 — 테두리 색만으로는 칸 안쪽이 허옇게 뜬다. 같은 색을 얇게 얹어
            //  바탕까지 물들이되, 그림이 파스텔로 흐려지지 않을 만큼만(알파 0.35).
            Color c = SCROLL_GRADE_TINT[idx];
            c.a = 0.35f;
            veil.color = owned ? c : new Color(0f, 0f, 0f, 0.70f);
        }

        var pic = node.transform.GetChild(CELL_PICTURE).GetComponent<Image>();
        Sprite sp = ScrollSprite(ENHANCE_SCROLL_IDS[idx]);
        if (pic != null && sp != null)
        {
            pic.sprite = sp;
            pic.color = owned ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
        }
    }

    static Sprite ScrollSprite(int itemId)
    {
        if (Managers.Data.ItemDic == null) return null;
        if (!Managers.Data.ItemDic.TryGetValue(itemId, out var it)) return null;
        return string.IsNullOrEmpty(it.SpriteName) ? null : Managers.Resource.Load<Sprite>(it.SpriteName);
    }

    void OnPickScroll(int idx)
    {
        if (_resolving) return;
        if (idx < 0 || idx >= ENHANCE_SCROLL_IDS.Length) return;

        int id = ENHANCE_SCROLL_IDS[idx];
        if (Managers.Game.GetItemCount(id) <= 0)
        {
            // 없는 주문서를 고르면 왜 안 되는지 말해 준다 — 조용히 무시하면 고장으로 보인다.
            string nm = Managers.Data.ItemDic != null && Managers.Data.ItemDic.TryGetValue(id, out var it)
                ? Util.Localize(it.Name) : id.ToString();
            Managers.UI.ShowToast($"{nm} 없음");
            return;
        }

        _scrollId = id;
        GetObject((int)GameObjects.EnhScrollSelectPopup)?.SetActive(false);
        Managers.Sound?.PlaySFX("SFX_BtnSelect");
        RefreshUI();
    }

    /// <summary>주문서 박스 겉면 — 고른 주문서의 아이콘과 남은 수량.</summary>
    void RefreshScrollBox()
    {
        var box = GetObject((int)GameObjects.Btn_EnhacePopup_01);
        if (box == null) return;

        int have = Managers.Game.GetItemCount(_scrollId);
        Sprite sp = ScrollSprite(_scrollId);

        // 주문서를 고른 상태면 아이콘을, 아니면 "+"를 보인다.
        var plus = GetObject((int)GameObjects.PlusText);
        if (plus != null) plus.SetActive(sp == null);

        foreach (var img in box.GetComponentsInChildren<Image>(true))
        {
            if (img.name != "Img_EnhScrollBox_02") continue;
            img.gameObject.SetActive(sp != null);
            if (sp != null) { img.sprite = sp; img.color = Color.white; }
            break;
        }

        var amount = GetText((int)Texts.AmountText);
        if (amount != null)
        {
            amount.gameObject.SetActive(sp != null);
            amount.text = ShortCount(have);
            amount.color = have > 0 ? Color.white : Util.HexToColor("FF5555");
            ApplyFont(amount, FontType.UI);
            SeatAmountBadge(amount, SLOT_BADGE_SIZE);
        }
    }

    #endregion

    void OnEquipInfoChanged()
    {
        if (_equip != null) RefreshUI();
    }

    // ────────────────────────────────────────
    //  SetInfo
    // ────────────────────────────────────────
    public void SetInfo(Equipment equip, int scrollId)
    {
        _equip = equip;
        _scrollId = scrollId;
        if (_equip == null) return;

        // 원본에서 강화창은 **어두운 무대 위**에 선다 — 마을 배경도 상단 재화 HUD도 하단 탭도
        //  거의 보이지 않고, 앞 창(대장간)만 희미하게 남는다. 우리 창은 뒤가 전부 밝게 비쳐
        //  모루와 망치가 마을 그림 위에 떠 있는 것처럼 보였다.
        Blackout(STAGE_IDLE, 0.25f);

        RefreshUI();
        PlayIntro();
        StartCoroutine(CoRevealPreview());   // 표는 다른 것이 다 자리 잡은 뒤 차례로 선다
    }

    public override void RefreshUI()
    {
        if (_equip == null || _equip.EquipmentData == null) return;
        var data = _equip.EquipmentData;

        // 장비 아이콘 — EquipIcon Animator가 sprite를 덮어쓰므로 코드 세팅 시 정지 (ForgePopup과 동일 처리)
        var wIcon = GetImage((int)Images.WIcon);
        Sprite sp = Managers.Resource.Load<Sprite>(data.SpriteName);
        if (wIcon != null && sp != null)
        {
            var animator = wIcon.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            wIcon.sprite = sp;
            // 애니메이터가 먹여 둔 회전/스케일이 그대로 굳는다(대장간에서 아이콘이 z=90으로 누워 있었다).
            wIcon.rectTransform.localRotation = Quaternion.identity;
            wIcon.rectTransform.localScale = Vector3.one;
        }

        GetText((int)Texts.ENumber).text = _equip.EnhanceLevel > 0 ? $"+{_equip.EnhanceLevel}" : "";

        string nm = Util.Localize(data.Name);
        var nameTxt = GetText((int)Texts.EnhEquipNameText);
        nameTxt.gameObject.SetActive(true);
        nameTxt.text = _equip.EnhanceLevel > 0 ? $"+{_equip.EnhanceLevel} {nm}" : nm;
        ApplyFont(nameTxt, FontType.UI);

        // 내구도
        GetText((int)Texts.DurabilityText).gameObject.SetActive(true);
        var duriTxt = GetText((int)Texts.DurabilityRealText);
        duriTxt.gameObject.SetActive(true);
        duriTxt.text = $"{_equip.Durability}/{data.MaxDurability}";

        // 성공확률 / 리스크
        bool maxed = _equip.EnhanceLevel >= ENHANCE_MAX_LEVEL;
        // 원본은 정수 퍼센트다("32%" → "88%" → "90%"). 소수 셋째 자리까지 찍으니
        //  숫자가 길어져 자리를 넘치고, 한눈에 읽히지도 않았다.
        GetText((int)Texts.ChanceTxt).text = maxed ? "MAX" : $"{Managers.Game.GetEnhanceChance(_equip):F0}%";

        int idx = System.Array.IndexOf(ENHANCE_SCROLL_IDS, _scrollId);
        var riskTxt = GetText((int)Texts.RiskNum);
        riskTxt.gameObject.SetActive(true);
        riskTxt.text = (idx >= 0 ? ENHANCE_SCROLL_DURI_LOSS[idx] : ENHANCE_SCROLL_DURI_LOSS[0]).ToString();

        // 잠재력 게이지 — 만충(ENHANCE_POTEN_FULL) 기준 비율
        var barGroup = GetObject((int)GameObjects.PotenForceBarGroup);
        var slider = barGroup != null ? barGroup.GetComponent<Slider>() : null;
        if (slider != null) slider.value = Mathf.Clamp01(_equip.Potential / ENHANCE_POTEN_FULL);
        GetText((int)Texts.PotenForceBarBarText).text = _equip.Potential.ToString();

        RefreshScrollBox();
        RefreshPreview(data);

        // 강화 가능 여부
        GetButton((int)Buttons.Enhance_Btn).interactable =
            !_resolving && Managers.Game.CanEnhance(_equip, _scrollId);
    }

    // ────────────────────────────────────────
    //  강화 성공 시 능력치 변화 (미리보기)
    // ────────────────────────────────────────
    //  영상에는 강화를 누르기 **전에** "강화 성공 시 / 능력치 변화" 블록이 떠서, 무엇이 얼마나
    //  오르는지 보고 결정할 수 있다. 우리 강화창엔 이 화면이 통째로 없었다 —
    //  확률과 내구도 손실만 보이고 정작 얻는 것이 안 보였다.
    //  영상처럼 **변하는 스탯에만** 화살표를 붙이고, 안 변하는 것은 현재 값만 적는다.
    // 표는 **프리팹에 통째로 저작돼 있다** — EnhAttDetail 아래 AttText(제목 2줄)와
    //  AttRealText(네 열: 이름 / 이전값 / 화살표 / 이후값). 네 열 각각에
    //  "오른쪽에서 밀려 들어오는" Move(isFrom)와 Color 트윈이 붙어 있고, TMP 알파는 0이다.
    //
    // ⚠️ 예전엔 이걸 모르고 확률 텍스트를 복제해 **한 장짜리 TMP**에 네 행을 몰아썼다.
    //    오브젝트가 하나라 열을 따로 움직일 수 없었고, 그래서 창이 열리는 프레임에
    //    표가 통째로 튀어나왔다 — "순차적으로 나오는 애니메이션이 없다"의 정체다.
    //    이제 저작된 표를 켜고 열을 차례로 부른다.
    GameObject _tblTitle, _tblBody;
    TMPro.TMP_Text _colName, _colBefore, _colArrow, _colAfter;
    bool _previewRevealed;
    readonly System.Collections.Generic.List<bool> _arrowRows = new System.Collections.Generic.List<bool>();

    const float PREVIEW_DELAY   = 1.65f;   // 창이 열리고 제목이 내려앉기까지
    const float PREVIEW_TITLE   = 0.35f;   // 제목 → 첫 열
    const float PREVIEW_STAGGER = 0.10f;   // 열과 열 사이
    const float PREVIEW_FLASH   = 0.05f;   // 화살표가 속을 채운 채 나타나는 한 순간
    const string PREVIEW_ARROW_HEX = "A60000";

    void CachePreviewNodes()
    {
        if (_tblBody != null) return;

        var detail = GetObject((int)GameObjects.EnhAttDetail);
        if (detail == null) return;

        _tblTitle = Util.FindChild(detail, "AttText", true);
        _tblBody  = Util.FindChild(detail, "AttRealText", true);
        if (_tblBody == null) return;

        _colName   = Util.FindChild(_tblBody, "Txt_AttRealText_03", true)?.GetComponent<TMPro.TMP_Text>();
        _colBefore = Util.FindChild(_tblBody, "Text_1", true)?.GetComponent<TMPro.TMP_Text>();
        _colArrow  = Util.FindChild(_tblBody, "Text_2", true)?.GetComponent<TMPro.TMP_Text>();
        _colAfter  = Util.FindChild(_tblBody, "Text_3", true)?.GetComponent<TMPro.TMP_Text>();

        foreach (var t in new[] { _colName, _colBefore, _colArrow, _colAfter })
            if (t != null) ApplyFont(t, FontType.UI);

        ScaleTable(_tblTitle);
        ScaleTable(_tblBody);
    }

    // 표 글자가 원본의 3분의 2 크기였다 — 행 간격 실측 13.7px 대 원본 19~20px.
    //  네 열의 폰트 크기를 각각 만지면 열마다 어긋나므로, 표 묶음의 스케일만 올린다.
    //  (열 사이 간격과 밀려 들어오는 거리까지 같은 비율로 커져 저작 레이아웃이 유지된다.)
    const float TABLE_SCALE = 1.42f;

    static void ScaleTable(GameObject node)
    {
        var rt = node != null ? node.transform as RectTransform : null;
        if (rt != null) rt.localScale = Vector3.one * TABLE_SCALE;   // 절대값 — 여러 번 불려도 안전
    }


    void RefreshPreview(Data.EquipmentData data)
    {
        CachePreviewNodes();
        if (_colName == null) return;

        if (_equip.EnhanceLevel >= ENHANCE_MAX_LEVEL) { _tblBody.SetActive(false); return; }

        var cur  = Managers.Game.GetEnhanceBonus(_equip.DataId, _equip.EnhanceLevel);
        var next = Managers.Game.GetEnhanceBonus(_equip.DataId, _equip.EnhanceLevel + 1);

        var names = new System.Collections.Generic.List<string>();
        var befs  = new System.Collections.Generic.List<string>();
        var afts  = new System.Collections.Generic.List<string>();
        _arrowRows.Clear();

        // 네 줄 고정. 안 변하는 줄도 이름과 현재값은 찍고, 화살표와 이후값만 비운다.
        void AddRow(string nm, string bef, string aft, bool changed)
        {
            names.Add(nm);
            befs.Add(bef);
            afts.Add(changed ? aft : "");
            _arrowRows.Add(changed);
        }

        AddRow(Util.Localize(LocalizeKey.EnhanceLevelText),
               $"+{_equip.EnhanceLevel}", $"+{_equip.EnhanceLevel + 1}", true);
        AddRow(Util.Localize(LocalizeKey.PhysicalAttackText),
               $"+{data.BaseAtk + cur.atk}", $"+{data.BaseAtk + next.atk}", next.atk != cur.atk);
        AddRow(Util.Localize(LocalizeKey.CriticalText),
               $"+{data.BaseCritRate + cur.crit:0.#}%", $"+{data.BaseCritRate + next.crit:0.#}%",
               !Mathf.Approximately(next.crit, cur.crit));
        AddRow(Util.Localize(LocalizeKey.MagicAttackText),
               $"+{data.BaseMagicAtk + cur.magic}", $"+{data.BaseMagicAtk + next.magic}",
               next.magic != cur.magic);

        // ⚠️ <color> 태그를 붙이면 저작 Color 트윈(흰 / 흰 / 검정 / 초록)을 덮어쓴다.
        _colName.text   = string.Join("\n", names);
        _colBefore.text = string.Join("\n", befs);
        _colAfter.text  = string.Join("\n", afts);
        _colArrow.text  = PreviewArrowRow("▷");

        // 다른 열은 저작 알파가 0이라 저절로 숨는데, 화살표만 색을 코드가 준다 —
        //  제 차례가 오기 전에는 직접 감춘다.
        if (!_previewRevealed) SetPreviewArrowAlpha(0f);
    }

    string PreviewArrowRow(string glyph)
    {
        var lines = new string[_arrowRows.Count];
        for (int i = 0; i < _arrowRows.Count; i++) lines[i] = _arrowRows[i] ? glyph : "";
        return string.Join("\n", lines);
    }

    void SetPreviewArrowAlpha(float a)
    {
        if (_colArrow == null) return;
        var c = _colArrow.color; c.a = a; _colArrow.color = c;
    }

    /// <summary>제목이 내려앉고, 네 열이 0.1초씩 어긋나 오른쪽에서 밀려 들어온다.
    /// 창을 여는 프레임에 표가 통째로 뜨지 않게, 다른 요소가 다 자리 잡은 뒤에 시작한다.</summary>
    IEnumerator CoRevealPreview()
    {
        CachePreviewNodes();
        if (_previewRevealed || _tblBody == null) yield break;
        _previewRevealed = true;

        yield return new WaitForSecondsRealtime(PREVIEW_DELAY);

        if (_tblTitle != null)
        {
            _tblTitle.SetActive(true);            // autoPlay 저작 LocalMove가 스스로 내려앉는다
            yield return null;
            PlayAuthoredNode("AttText", "");      // 두 번째부터는 직접 되감아 준다
        }

        yield return new WaitForSecondsRealtime(PREVIEW_TITLE);

        _tblBody.SetActive(true);
        yield return null;                        // 켜진 프레임에 DOTweenAnimation이 트윈을 만든다

        PlayAuthoredNode("Txt_AttRealText_03", "");        // ① 이름
        yield return new WaitForSecondsRealtime(PREVIEW_STAGGER);

        PlayAuthoredNode("Text_1", "");                    // ② 이전값
        yield return new WaitForSecondsRealtime(PREVIEW_STAGGER);

        PlayAuthoredNode("Text_2", "", dropColor: true);   // ③ 화살표 (저작 Color는 검정으로 끝난다)
        SetPreviewArrowAlpha(1f);
        _colArrow.color = Util.HexToColor(PREVIEW_ARROW_HEX);
        _colArrow.text  = PreviewArrowRow("▶");            // 나타나는 한 순간만 속이 찬다
        DOVirtual.DelayedCall(PREVIEW_FLASH,
            () => { if (_colArrow != null) _colArrow.text = PreviewArrowRow("▷"); }).SetUpdate(true);
        yield return new WaitForSecondsRealtime(PREVIEW_STAGGER);

        PlayAuthoredNode("Text_3", "");                    // ④ 이후값
    }

    /// <summary>표의 **한 열만** 재생한다.
    ///
    /// 이 표의 저작 트윈은 id가 전부 빈 문자열이라 `DORestartById`로는 잡히지 않는다 —
    ///  빈 id에는 SetId가 걸리지 않는다(DOTweenAnimation.cs:508). 컴포넌트가 들고 있는
    ///  트윈을 직접 되감는다.
    /// `dropColor`는 화살표 열 전용 — 그 열의 저작 Color는 **검정으로 끝나서**
    ///  까만 배경 위에서 화살표를 지운다.</summary>
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

            if (string.IsNullOrEmpty(id)) a.tween?.Restart();
            else a.DORestartById(id);
        }
    }

    // ────────────────────────────────────────
    //  강화 실행
    // ────────────────────────────────────────
    void OnClickEnhance()
    {
        if (_resolving || _equip == null) return;

        // 골드 부족은 버튼만 막으면 이유를 알 수 없다 — 필요 금액을 알려준다.
        int cost = Managers.Game.GetEnhanceCost(_equip);
        if (Managers.Game.Gold < cost)
        {
            Managers.UI.ShowToast($"골드가 부족합니다\n강화 비용 {cost:N0} (보유 {Managers.Game.Gold:N0})");
            return;
        }
        if (!Managers.Game.CanEnhance(_equip, _scrollId)) return;

        var result = Managers.Game.EnhanceEquipment(_equip, _scrollId);
        if (result == null) return;

        // 망치 소리는 여기서 한 번 울리지 않는다 — 클립(Forge Smith/Success.anim)이
        //  t=0.08 / 1.08 / 2.08 에 SmashSound 이벤트를 갖고 있어 **타격마다** 울린다.
        //  (원본 영상도 1.0초 간격 3연타다. 예전엔 여기 한 번뿐이라 첫 타만 소리가 났다)

        bool skip = _skipToggle != null && _skipToggle.isOn;
        if (skip)
        {
            // 결과 플래시만 — 팝업 유지, 연속 강화 가능
            StartCoroutine(CoSkipFlash(result.Success));
        }
        else
        {
            // 원작대로 **결과창이 연출 전체를 맡는다.** [강화]를 누른 쪽은 창을 띄우기만 한다
            //  (원작 그래프: 확률 굴림 → SetActive(EnhanceProgress Popup) → CustomEvent).
            //  예전엔 여기서 망치질을 하고 끝나면 결과창으로 갈아탔는데, 그 이음매에서
            //  백열 잔광이 끊기고 프리팹에 저작된 아이콘 궤적을 쓸 수 없었다.
            _resolving = true;
            Managers.UI.ShowPopupUI<UI_EnhanceProgressPopup>().SetInfo(result, _equip);
            _resolving = false;
        }
        RefreshUI();   // 주문서 소진/확률/내구도/잠재력 즉시 갱신
    }

    // 클립의 첫 Shake가 t=0.33이라, 암전 +1.10초에 첫 타를 놓으려면 앞에 여백을 둔다.
    //  0.57로 넣었더니 실측 +1.27이 나와 0.2초를 덜어냈다(암전 판정 자체에 지연이 있다).
    const float SMITH_LEAD_IN = 0.37f;

    /// 대장장이 모션 → 결과 창. 모션 도중에는 중복 클릭을 막는다.
    IEnumerator CoSmithMotion(GameManager.EnhanceResult result)
    {
        _resolving = true;

        HookSmithEvents();
        EnterCeremonyView();

        // ⚠️ 무대가 검어지자마자 망치를 휘두르면 안 된다.
        //    원작 영상에서 무대 휘도 곡선을 재 보면 **암전 +1.10초**에 첫 타가 온다
        //    (그다음 +2.10 / +3.10 — 1초 간격). 우리는 +0.53초였다.
        //    그 0.57초는 빈 시간이 아니라 "장비가 모루에 내려앉고 대장장이가 망치를 드는" 사이다.
        yield return new WaitForSecondsRealtime(SMITH_LEAD_IN);

        var smith = GetObject((int)GameObjects.HammerNormalAnim);
        var anim = smith != null ? smith.GetComponent<Animator>() : null;
        float wait = 0.4f;

        if (anim != null)
        {
            string state = result.Success ? "Success" : "Fail";
            _ceremony = true;
            anim.Play(state, 0, 0f);
            foreach (var c in anim.runtimeAnimatorController.animationClips)
                if (c.name == state) { wait = c.length; break; }
        }

        // 결과 섬광은 클립의 Success/Fail 이벤트가 정확한 시점에 터뜨린다(OnSmithAnimEvent).
        //  여기서는 모션이 끝날 때까지만 기다린다.
        yield return new WaitForSecondsRealtime(wait);

        _ceremony = false;                             // 대기 자세의 망치 소리는 듣지 않는다
        if (anim != null) anim.Play("Normal", 0, 0f);   // 대기 자세로 복귀
        ExitCeremonyView();
        _resolving = false;

        Managers.UI.ShowPopupUI<UI_EnhanceProgressPopup>().SetInfo(result, _equip);
        RefreshUI();
    }

    #region 강화 연출 대본 (클립 AnimationEvent)

    // 대장간 강화 클립은 stringParameter로 **연출 대본**을 들고 있다.
    //  원본(비주얼 스크립팅)의 그래프를 그대로 옮긴 것 — 이름/시각은 클립이 정한다.
    //
    //    Success.anim(5.02s)  SmashSound 0.08 / Curious 0.25 / Shake 0.33
    //                         SmashSound 1.08 / Shake 1.33
    //                         SmashSound 2.08 / ChargeSuccess 2.33
    //                         Serious 3.75 / Success 3.83 / SuccessDone 5.00
    //    Fail.anim(5.02s)     … ChargeFail 2.33 / Serious 3.75 / Fail 3.85
    //                         FailSound 3.90 / FailDone 5.00
    //
    //  소리 이름은 원본 그래프의 PlaySFX 인자를 우리 클립으로 옮긴 것이다.
    static readonly (string ev, string sfx)[] EVENT_SFX =
    {
        ("SmashSound",  "SFX_EnhanceHammer"),     // 망치 타격 (3연타)
        ("Curious",     "SFX_CuriousCaution2"),   // 첫 타 직후 — 대장장이가 살피는 소리
        ("Serious",     "SFX_Serious"),           // 마지막 담금질 직전 긴장
        ("Success",     "SFX_Success(Base)"),     // 성공 폭발 (영상에서 가장 큰 소리)
        ("FailSound",   "SFX_Fail"),
    };

    // 실패 마무리의 불꽃 소리는 이벤트와 **동시에** 나지 않는다.
    //  원본 그래프에서 FailDone -> Timer(0.3) -> ... -> Timer(1.1) -> Timer(0.2) -> PlaySFX(FlameOn)
    //  이라 합계 1.6초 뒤다. 담금질한 쇠를 물에 넣는 타이밍이라 이 지연이 곧 연출이다.
    const float FLAME_DELAY = 1.6f;

    AnimEventSink _smithSink;

    // 대기 자세(Normal.anim)에도 SmashSound가 3개 박혀 있다 — 대장장이가 늘 망치질하는
    //  루프라서다. 그대로 두면 강화창을 열어 둔 내내 망치 소리가 계속 난다(원본 영상은 조용하다).
    //  **강화 연출(Success/Fail) 중에만** 대본을 듣는다.
    bool _ceremony;

    /// <summary>망치 애니메이터의 이벤트를 구독한다. 중복 구독은 하지 않는다.</summary>
    void HookSmithEvents()
    {
        var smith = GetObject((int)GameObjects.HammerNormalAnim);
        if (smith == null) return;

        var sink = smith.GetComponent<AnimEventSink>();
        if (sink == null) sink = smith.AddComponent<AnimEventSink>();
        if (ReferenceEquals(sink, _smithSink)) return;   // 이미 이 sink를 듣고 있다

        if (_smithSink != null) _smithSink.OnAnimEvent -= OnSmithAnimEvent;
        _smithSink = sink;
        _smithSink.OnAnimEvent += OnSmithAnimEvent;
    }

    void OnSmithAnimEvent(string id)
    {
        if (!_ceremony || string.IsNullOrEmpty(id)) return;

        foreach (var (ev, sfx) in EVENT_SFX)
            if (ev == id) { Managers.Sound?.PlaySFX(sfx); break; }

        switch (id)
        {
            case "FailDone":
                DOVirtual.DelayedCall(FLAME_DELAY, () => Managers.Sound?.PlaySFX("SFX_FlameOn"))
                         .SetUpdate(true);
                break;

            // 타격 — 흔들림 + 모루 위 섬광/불티. 원본도 Shake에서 FX 오브젝트를 켠다.
            case "Shake":
                ShakeFrame(0.22f, 2f);
                PopImpactFx();
                break;

            case "ShakeLast":
                ShakeFrame(0.30f, 2.5f);
                PopImpactFx();
                break;

            case "ChargeSuccess":                                    // 담금질 — 아이콘이 떠오른다
            case "ChargeFail":
                ChargeGlow();                                        // 화면 전체가 서서히 달아오른다
                // 원작은 담금질 순간에도 모루를 한 번 치고 들어간다(Floor PunchPosition 0.3s,
                //  Hammer ShakePosition 0.25s). 3타의 타격감이 여기서 나온다.
                PopImpactFx();
                // 흔들지 않는다. 아이콘이 1.4초에 걸쳐 떠오르는 구간이라, 프레임을 흔들면
                //  그 궤적이 통째로 휘청인다 — 원본은 이 구간 내내 창이 정지해 있다.
                RaiseCraftIcon();
                break;

            // 결과 섬광은 **이 순간**이다(클립 t=3.83). 예전엔 코루틴이 "클립길이-0.5"로
            //  어림잡아 터뜨려 0.7초쯤 늦었다 — 소리(Success)와 빛이 어긋나 보였다.
            case "Success":
                // 여기도 흔들지 않는다. 결과가 확정되는 0.45초는 아이콘이 꼭대기로 튀어오르고
                //  결과창이 그 꼭대기를 이어받는 순간이라, 프레임이 떨면 인계가 어긋나 보인다.
                PopCraftIcon();
                PopResultFlash(true);
                break;

            case "Fail":
                ShakeFrame(0.35f, 2.5f);
                DropToAnvil();                     // 실패는 **모루 위로** 툭 떨어진다(원작 id 8)
                PopResultFlash(false);
                break;
        }
    }

    /// <summary>타격 순간의 이펙트. 프리팹에 저작된 섬광을 되살리고, 그 위에
    /// 모루 지점에서 흰 구체 + 연기 + 불티를 터뜨린다(원본 영상의 타격 구성).</summary>
    Coffee.UIExtensions.UIParticle _impactFx;

    void PopImpactFx()
    {
        // ⚠️ 타격 파티클은 프리팹에 **이미 이식돼 있다** — GlowDot_01_1 과 그 자식
        //    VFX_GlowDot_01_1_03(원작 Dust_01, 연기) / Ring_01(퍼지는 링).
        //    그런데 월드 ParticleSystem이라 Screen Space Camera 캔버스 위에 렌더되지 않아
        //    **한 번도 화면에 나온 적이 없다.** 켰다 껐다만 하고 있었으니 아무 일도 없었고,
        //    그래서 원작의 "검은 연기 구름 + 흰 반짝임"이 우리에겐 흐릿한 흰 얼룩이었다.
        //    UIParticle로 감싸면 그 자리에서 그대로 그려진다(빛줄기와 같은 처리).
        // ⚠️ 프리팹의 GlowDot_01_1(원작 타격 파티클)은 UIParticle로 감싸도 **아무것도 그리지 않는다**
        //    (재질이 Mobile/Particles/Additive라 캔버스 경로에서 죽는다 — 빛줄기와 달랐다).
        //    실측으로 재현한 그림(UIFx.SpawnForgeSmash)이 지금은 유일하게 보이는 길이다.

        // 저작 섬광(Flash)은 Image라 그대로 껐다 켠다
        var flashGo = Util.FindChild(gameObject, "Flash", true);
        if (flashGo != null) { flashGo.SetActive(false); flashGo.SetActive(true); }

        // 타격점은 장비 아이콘 자리다(망치가 내려찍는 곳). 그 좌표에 버스트를 얹는다.
        var host = _frameRT != null ? _frameRT
                 : GetObject((int)GameObjects.EnhacePopup)?.GetComponent<RectTransform>();
        if (host == null) return;

        Vector2 at = Vector2.zero;
        var icon = GetImage((int)Images.WIcon);
        if (icon != null)
        {
            // 아이콘의 위치를 프레임 좌표계로 옮긴다 — 부모가 달라 anchoredPosition을 그대로 쓰면 어긋난다.
            Vector3 world = icon.rectTransform.position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                host, RectTransformUtility.WorldToScreenPoint(GetComponent<Canvas>()?.worldCamera, world),
                GetComponent<Canvas>()?.worldCamera, out at);
        }

        // 진짜 파티클이 살아났으면 흉내는 필요 없다 — 겹치면 원작에 없는 흰 구름이 된다.
        UIFx.PlayBurst(host, at, UIFx.FxType.ForgeSmash);

        // 장비 아이콘도 얻어맞는다. 원작 id"1"은 **ShakePosition (4,4,0) 0.25초 OutQuad**다 —
        //  크기를 눌렀다 펴면(예전 방식) 판이 찌그러져 보이고, 모루에 박혀 있다는 느낌이 안 난다.
        var iconRt = CraftIcon();
        if (iconRt != null)
        {
            Vector2 home = iconRt.anchoredPosition;
            iconRt.DOComplete();
            iconRt.DOShakeAnchorPos(0.25f, new Vector2(4f, 4f), 50, 90f, false, true)
                  .SetUpdate(true)
                  .OnComplete(() => { if (iconRt != null) iconRt.anchoredPosition = home; });
        }
    }

    #region 장비 아이콘 궤적

    // 원본은 이 궤적을 **결과창 아이콘의 DOTweenAnimation id**로 적어 뒀다:
    //   5 담금질에 떠오르며 커진다 (+50, 1.4배, 1.4초)
    //   6 결과가 확정될 때 한 번 더 튀어오른다 (+80, 0.3초 OutBack)
    //   7 결과 화면에서 내려앉으며 원래 크기로 (+20, 1.2초)
    //   8 실패는 제자리로 (0, 0.1초)
    // 그런데 원본은 망치질과 결과가 **한 창**이고, 우리 포트는 둘로 나뉘어 있다.
    //  그래서 5와 6은 이 창(강화창)이 맡아야 한다 — 이게 없으면 아이콘이 모루에 붙박인 채
    //  결과창에서 난데없이 위에서 내려온다(사용자가 "아이콘 위치가 정확하지 않다"고 한 지점).
    // 좌표는 **절대값**이다(피벗이 윗변이라 anchoredPosition.y가 곧 판 윗변 위치).
    //  원본 프레임에서 그대로 읽었다 — 캔버스 세로 중앙을 0으로 놓고:
    //    대기/정착  +20   아이콘이 모루에서 떠 있고 그 사이에 내구도 글자가 들어간다
    //    연출 바닥    0   [강화]를 누르면 모루 위로 내려앉는다  ← 이게 빠져 있었다
    //    담금질     +50   (저작 id 5)
    //    결과 확정  +80   (저작 id 6) — 결과창이 이 높이에서 이어받는다
    //
    // ⚠️ 예전엔 "두 창의 1유닛 길이가 다르다"고 보고 0.731을 곱했다. 그건 착시였다 —
    //    강화창은 홈(+20)에 상대로 더하고 결과창은 절대로 스냅해서, 같은 80이 다른 데로 갔던 것뿐이다.
    //    두 창 모두 절대값을 쓰면 인계 지점이 정확히 겹친다(실측 20u = 양쪽 다 native 1215/1217px).
    const float ICON_HOME_Y   = 20f;
    const float ICON_ANVIL_Y  = 0f;
    const float ICON_LAND_DUR   = 0.30f;   // 원작 id"2"
    // 원작은 아이콘이 **망치가 돌기 시작하는 바로 그 순간**에 모루에 얹힌다(그래프 T+0.40→0.70).
    const float ICON_LAND_DELAY = 0.07f;   // = SMITH_LEAD_IN(0.37) − 0.30
    const float ICON_RISE_Y   = 50f;
    const float ICON_RISE_SC  = 1.4f;
    const float ICON_RISE_DUR = 1.4f;
    const float ICON_POP_Y    = 80f;
    const float ICON_POP_DUR  = 0.3f;
    const float ICON_DROP_DUR = 0.1f;

    RectTransform CraftIcon()
    {
        var go = GetObject((int)GameObjects.CraftEquipIcon);
        return go != null ? go.transform as RectTransform : null;
    }

    /// <summary>[강화]를 누르면 아이콘이 **모루 위로 내려앉는다.**
    ///
    /// 원본에서 무대가 검어지고 잠시 뒤 장비가 20유닛 아래로 내려와 모루에 닿는다
    ///  (프레임 실측: 판 아랫변 443 → 482, 모루 윗면 486). 우리는 이 하강이 없어
    ///  장비가 모루에서 한 뼘 뜬 채로 망치를 맞고 있었다.</summary>
    void LandCraftIcon()
    {
        var rt = CraftIcon();
        if (rt == null) return;

        rt.DOKill();
        // 원작 저작값은 id"2" LocalMove (0,0,0) / 0.3초 / **InBack** 이다.
        //  InBack은 먼저 위로 살짝 물렸다가 가속해 "쿵" 얹힌다 — OutQuad로 두면 스르르 멈춘다.
        //  스케일 트윈은 원작에 없다(내려앉는 동안 크기는 1 그대로).
        rt.DOAnchorPos(new Vector2(0f, ICON_ANVIL_Y), ICON_LAND_DUR)
          .SetEase(Ease.InBack).SetDelay(ICON_LAND_DELAY).SetUpdate(true);
    }

    /// <summary>담금질 — 모루에서 천천히 떠오르며 커진다.</summary>
    void RaiseCraftIcon()
    {
        var rt = CraftIcon();
        if (rt == null) return;

        rt.DOKill();
        rt.DOAnchorPos(new Vector2(0f, ICON_RISE_Y), ICON_RISE_DUR)
          .SetEase(Ease.Linear).SetUpdate(true);
        rt.DOScale(ICON_RISE_SC, ICON_RISE_DUR).SetEase(Ease.Linear).SetUpdate(true);

        Whiten(1f, ICON_RISE_DUR);   // 담금질 — 쇠가 달아오르듯 아이콘이 하얗게 탄다
    }

    /// <summary>아이콘 위의 흰 덮개(프리팹 CraftEquipIcon/Flash).
    ///
    /// 원본에서 담금질에 들어가면 장비 칸이 서서히 하얗게 타올랐다가, 결과 섬광에서
    ///  완전히 날아가고, 0.8초쯤 뒤 평상 밝기로 돌아온다. 우리는 타격 순간에만
    ///  깜빡이고 말아, 떠오르는 아이콘이 그냥 빨간 네모로 보였다.
    /// 저작 Fade(0.25초 autoPlay)가 켜질 때마다 끼어들므로 끊고 직접 몬다.</summary>
    void Whiten(float alpha, float duration)
    {
        var go = Util.FindChild(gameObject, "Flash", true);
        var img = go != null ? go.GetComponent<Image>() : null;
        if (img == null) return;

        go.SetActive(true);
        img.DOKill();

        var c = img.color;
        if (alpha > 0f && c.a <= 0f) { c.a = 0f; img.color = c; }

        img.DOFade(alpha, duration).SetEase(Ease.Linear).SetUpdate(true)
           .OnComplete(() => { if (alpha <= 0f && go != null) go.SetActive(false); });
    }

    /// <summary>결과가 확정되는 순간 — 한 번 더 튀어오른다. 결과창은 이 꼭대기에서 이어받는다.</summary>
    void PopCraftIcon()
    {
        var rt = CraftIcon();
        if (rt == null) return;

        rt.DOKill();
        rt.DOAnchorPos(new Vector2(0f, ICON_POP_Y), ICON_POP_DUR)
          .SetEase(Ease.OutBack).SetUpdate(true);

        // 섬광에서 완전히 날아갔다가 0.8초에 걸쳐 평상으로 — 원본의 해제 시점이다.
        Whiten(1f, 0f);
        DOVirtual.DelayedCall(ICON_WHITE_HOLD, () => Whiten(0f, ICON_WHITE_OUT)).SetUpdate(true);
    }

    const float ICON_WHITE_HOLD = 0.25f;
    const float ICON_WHITE_OUT  = 0.55f;

    /// <summary>실패 — 떠 있던 아이콘이 **모루 위로** 툭 떨어진다.
    ///
    /// ⚠️ 예전엔 RestoreCraftIcon을 불러 대기 자리(+20)로 돌려보냈다. 그러면 실패 결과를
    ///    보는 내내 장비가 모루에서 20유닛 뜬 채로 떠 있다 — 원작 id"8"은 (0,0)이다.
    /// 크기는 따로 1.0초에 걸쳐 돌아온다(원작도 이동 0.1초 / 크기 1.0초로 따로 간다).</summary>
    void DropToAnvil()
    {
        var rt = CraftIcon();
        if (rt == null) return;

        rt.DOKill();
        rt.DOAnchorPos(new Vector2(0f, ICON_ANVIL_Y), ICON_DROP_DUR)
          .SetEase(Ease.OutBack).SetUpdate(true);
        rt.DOScale(1f, 1.0f).SetEase(Ease.OutQuad).SetUpdate(true);
        Whiten(0f, 0.3f);
    }

    /// <summary>제자리로. 연출이 끝나면 반드시 불러야 다음 강화가 어긋난 자리에서 시작하지 않는다.</summary>
    void RestoreCraftIcon(float duration)
    {
        var rt = CraftIcon();
        if (rt == null) return;

        rt.DOKill();
        if (duration <= 0f)
        {
            rt.anchoredPosition = new Vector2(0f, ICON_HOME_Y);
            rt.localScale = Vector3.one;
            Whiten(0f, 0f);
            return;
        }
        rt.DOAnchorPos(new Vector2(0f, ICON_HOME_Y), duration).SetEase(Ease.OutQuad).SetUpdate(true);
        rt.DOScale(1f, duration).SetEase(Ease.OutQuad).SetUpdate(true);
        Whiten(0f, duration);
    }

    #endregion

    /// <summary>성공/실패 전체화면 섬광(Animator "Flash").</summary>
    void PopResultFlash(bool success)
    {
        // ⚠️ 여기서 화면을 하얗게 "번쩍"하면 안 된다.
        //    원작 영상의 전체 휘도 곡선을 재 보면 결과 순간은 **갑작스러운 섬광이 아니라
        //    담금질부터 1.7초 동안 달아오른 백열의 정점**이다(8.6 → 137.9, 가속 곡선).
        //    예전엔 한 프레임만 255로 튀었다가 꺼져서 "번쩍"하고 마는 정도였다.
        //    그 오름은 ChargeGlow가 이미 몰고 있으니, 여기서는 식히기만 한다.
        if (success)
        {
            // 계단 한 칸 올라갔다가(0.15초) 잠깐 머문 뒤 1.8초에 걸쳐 검정으로.
            GlowWash(CHARGE_PEAK, PEAK_RISE, Ease.InSine);
            DOVirtual.DelayedCall(PEAK_RISE + PEAK_HOLD,
                () => GlowWash(0f, WASH_OUT, Ease.InOutQuad)).SetUpdate(true);
        }
        else
        {
            GlowWash(0f, 0.30f, Ease.InOutCubic);   // 실패는 원작 id"54" 0.3초 InOutCubic
        }
        Blackout(STAGE_CEREMONY, 0.35f);

        // ⚠️ Flash_Success / Flash_Fail 은 **건너뛰기 전용**이다.
        //    원작에서 그 둘은 "Skip Flash Group" 아래에 묶여 있다 — 연출을 건너뛴 사람에게
        //    결과만 한 번 번쩍 보여 주는 장치다. 연출을 다 보는 경로에서 같이 터뜨렸더니
        //    담금질 백열 위에 전체화면 흰 판이 겹쳐 정점이 185까지 치솟았다(원작 137.9).
        //    건너뛰기 경로(CoSkipFlash)는 지금도 이 둘을 쓴다.
    }

    const float FLASH_HOLD = 0.5f;

    #region 담금질 백열

    // 원작 영상(3:20~3:50)의 **화면 전체 평균 휘도**를 프레임마다 재서 옮긴 값이다.
    //  막타(암전 +3.10초)부터 결과(+4.70초)까지 1.6초 동안 8.6 → 137.9로 오른다.
    //  흰 판을 위에 덮었다고 보고 알파를 역산하면
    //    +3.1  0.045 / +3.5  0.133 / +4.3  0.343 / +4.7  0.525
    //  — 가속 곡선(InQuad)에 정점 0.525다.
    //
    // ⚠️ 이 판은 **내용보다 위**에 깔아야 한다. 뒤에 깔면 배경만 밝아지고
    //    모루·망치·장비는 검은 채로 남아 원작과 전혀 다른 그림이 된다
    //    (CeremonyBlackout은 SetAsFirstSibling — 그건 무대를 지우는 용도라 그대로 둔다).
    // 0.525는 "흰 판을 sRGB로 덧칠했다"고 본 역산값인데, 우리 렌더가 리니어라
    //  같은 알파가 훨씬 밝게 합성된다(실측 정점 185 대 원작 137.9). 실측으로 맞춘 값이다.
    // 원작 그래프의 실제 값(EnhanceProgress Popup / Bg):
    //    담금질  id"51"  →회색 0.5882   1.4초 InSine
    //    결과    id"52"  →회색 0.8627   0.15초 InSine   (계단 한 칸)
    //    0.2초 뒤 id"53" →검정          1.8초 InOutQuad
    //  우리 렌더는 리니어 합성이라 같은 알파가 더 밝게 나온다. **비율과 시간·이징은 원작 그대로**,
    //  절대 알파만 실측으로 눌렀다(0.38로 재 보니 정점 165, 원작 137.9 → 0.827배).
    const float CHARGE_PLATEAU = 0.184f;   // 0.5882 자리
    const float CHARGE_RAMP    = 1.40f;
    const float CHARGE_PEAK    = 0.270f;   // 0.8627 자리
    const float PEAK_RISE      = 0.15f;
    const float PEAK_HOLD      = 0.05f;    // 원작 Timer 0.2 − 0.15
    const float WASH_OUT       = 1.80f;

    Image _glow;

    void GlowWash(float alpha, float duration, Ease ease)
    {
        if (_glow == null)
        {
            var go = new GameObject("CeremonyGlow", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-BLACKOUT_OVERSCAN, -BLACKOUT_OVERSCAN);
            rt.offsetMax = new Vector2(BLACKOUT_OVERSCAN, BLACKOUT_OVERSCAN);
            rt.SetAsLastSibling();                 // 창 내용 **위**

            _glow = go.AddComponent<Image>();
            _glow.color = new Color(1f, 1f, 1f, 0f);
            _glow.raycastTarget = false;
        }

        _glow.transform.SetAsLastSibling();
        _glow.gameObject.SetActive(true);
        _glow.DOKill();

        if (duration <= 0f) { var c = _glow.color; c.a = alpha; _glow.color = c; return; }
        _glow.DOFade(alpha, duration).SetEase(ease).SetUpdate(true);
    }

    /// <summary>담금질 — 쇠가 달아오르듯 화면 전체가 하얗게 탄다.</summary>
    void ChargeGlow() => GlowWash(CHARGE_PLATEAU, CHARGE_RAMP, Ease.InSine);

    #endregion

    /// <summary>강화창 프레임을 흔든다. 원본의 HammerSmash / HammerSmashLong 자리.
    /// 위치를 되돌려 놓고 끝내야 다음 연출이 어긋난 자리에서 시작하지 않는다.</summary>
    Vector2? _frameHome;

    void ShakeFrame(float duration, float strength)
    {
        var rt = _frameRT != null ? _frameRT : GetObject((int)GameObjects.EnhacePopup)?.GetComponent<RectTransform>();
        if (rt == null) return;

        // ⚠️ 홈을 매번 새로 읽으면 안 된다 — 흔들리는 도중에 또 불리면 흔들린 자리를
        //    홈으로 착각해 창이 조금씩 밀려난다.
        if (_frameHome == null) _frameHome = rt.anchoredPosition;

        rt.DOKill();
        rt.anchoredPosition = _frameHome.Value;
        // fadeOut을 켠다. 껐더니 0.22초 내내 최대 진폭으로 떨다가 끝에서 툭 제자리로 돌아왔다.
        //  vibrato 18은 0.22초에 80Hz — 프레임마다 방향이 바뀌어 떨림이 아니라 지직거림이 된다.
        rt.DOShakeAnchorPos(duration, strength, 10, 90f, false, true)
          .SetUpdate(true)
          .OnComplete(() => { if (rt != null) rt.anchoredPosition = _frameHome.Value; });
    }

    #endregion

    IEnumerator CoSkipFlash(bool success)
    {
        _resolving = true;
        // 건너뛰기는 화면을 비우지 않는다 — 연속 강화가 목적이라 표가 계속 보여야 한다.
        var flash = GetObject(success ? (int)GameObjects.Flash_Success : (int)GameObjects.Flash_Fail);
        if (flash != null)
        {
            flash.SetActive(false);
            flash.SetActive(true);   // Animator(Flash) 재생
        }
        yield return new WaitForSecondsRealtime(0.6f);
        if (flash != null) flash.SetActive(false);
        _resolving = false;
        RefreshUI();
    }

    void OnClickClose()
    {
        if (_isClosing || _resolving) return;
        _isClosing = true;
        Managers.UI.ClosePopupUI();
    }
}
