using System.Collections;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [미니몬 잡기!] 팝업 (기획서 v3 05.28 슬라이드 28~37 = 미니몬 잡기 #1~#10).
// [미니몬 도감]의 [미니몬 잡기!] 버튼으로 호출되어 포획 미니게임을 진행한다.
// 백엔드(확률/천장)는 MinMonManager.TryCatch가 담당. 본 팝업은 UI/연출 + 결과 호출.
//
//  - 슬33: Popup Anim(UI_Popup.OpenAnimation) → Loading Anim(LoadingBox1/2 랜덤, autoPlay Fade)
//  - 슬34: Setup Anim(TamerImg/0_Slime_Tame prefab Move DOTweenAnimation 트리거 → Grayscale 머티리얼 해제)
//          + Emoj_Exclamation + 대사 타이핑(DialogueAnimator) + 버튼 0.5s 간격 등장
//  - 슬35: [길들인다] Lure Anim(Y Yoyo) + Emoj_Lure + 화면 흔들림 + 효과有/無 분기
//  - 슬36: 결과 — 성공(Emoj_TwoHearts/Happy → GlowRays 나의미니몬) / 실패(Emoj_Sweat + Runaway Anim)
//  - 슬37: [가랏!만드라고라] 포물선 Happy Mandrake + Jump/Eat Anim 후 성공 처리
//
// 주의: 팝업 오픈 시 Time.timeScale=0 이므로 모든 대기는 WaitForSecondsRealtime / SetUpdate(true) 사용.
// 대사 텍스트 로컬라이즈 키(CatchMinimonStartText1 등)는 LocalizationData에 없어 한글 리터럴로 처리(TODO: 데이터화).
public class UI_TamePopup : UI_Popup
{
    #region Enum

    enum Images
    {
        TamePopupBackground,
        TamePopupPanelFrame,
        TamerImg,            // 플레이어 IMG (슬34 Setup, Grayscale 머티리얼)
        TamerEmoj,           // 플레이어 머리 위 Emoj (Animator: Emoj.controller)
        MonsterEmoj,         // 미니몬 머리 위 Emoj (Animator: Emoj.controller)
        BlockImg,            // 입력 차단 / Opening 깜빡임 오버레이
        UseHappyMandrakeImg, // 만드라고라 투척 IMG (슬37)
        Title,
        TamePopup_TalkBox,
        TamePopup_TalkBoxBackground,
    }

    enum Texts
    {
        Txt_Title_03,                       // 타이틀(MyMinimonText)
        showTextBox,                        // 대사 타이핑 박스(슬32)
        KorLureBtn,                         // "길들인다!" (KR)
        EngLureBtn,                         // (EN)
        KorMandrakeBtn,                     // "가랏! 만드라고라!" (KR)
        EngMandrakeBtn,                     // (EN)
        AmountTxt,                          // 성공확률 / 시도횟수
        Txt_MinimonInfoBtn_01,              // "나의 미니몬"
        Txt_KOR_TameConfirmPopup_02,
        Txt_KOR_TameConfirmPopup_06,
        Txt_ENG_TameConfirmPopup_04,
        Txt_ENG_TameConfirmPopup_06,
        Txt_TameConfirmPopup_UseMand_05,
    }

    enum Buttons
    {
        LureBtn,                            // 길들인다!
        UseMandrakeBtn,                     // 가랏! 만드라고라!
        HappyMandrakeBox,
        MinimonInfoBtn,                     // 나의 미니몬
        ExitBtn,                            // 나가기
        TameConfirmPopupBackground,
        TameConfirmPopup_UseMandrakeBtn,
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
        TamePopup,
        TamePopupPanel,
        Imgs_off,                           // 플레이어/미니몬/이모지 컨테이너
        TamePopupMonster_1,
        LoadingBox1,                        // 로딩 Type1
        LoadingBox2,                        // 로딩 Type2
        KorBtnGroup,
        EngBtnGroup,
        TameConfirmPopups,
        KOR_TameConfirmPopup,
        ENG_TameConfirmPopup,
    }

    #endregion

    // 테스트 전용: 다음 길들이기 결과 강제 (0=정상 확률, 1=성공, 2=실패). 사용 후 자동 리셋.
    public static int DebugForceCatch = 0;

    const float EMOJ_LEN = 0.9f;   // Emoj 클립 길이 (non-loop) — 다음 단계 전 끝까지 재생 보장용

    int _dataId;
    bool _busy;                 // 연출/시도 진행 중 (입력 차단)
    MinMonData _mm;
    CreatureData _creature;
    string _cName;
    DialogueAnimator _dialogue;
    Transform _slimeTr;         // 0_Slime_Tame (숫자 시작이라 enum 불가 → 직접 탐색)
    Image _slimeImg;
    Vector2 _tamerRest, _monRest; // 슬34: 플레이어/몬스터 설계 위치(Init=Awake에서 캡처, 슬라이드인 복귀 지점)

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // 잡기 시작 — 잡기 전용 BGM. 닫을 때 OnDestroy에서 원래 BGM 복귀.
        //  (예전 BGM_Minion_battle·SFX_MinimonCatch 는 포켓몬스터 원곡 추출본이라 출시 불가 — 자작 칩튠으로 교체,
        //   _tools/gen_chiptune.py)
        Managers.Sound?.PushBgm("BGM_MinimonCapture");

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.TamePopup).GetComponent<RectTransform>();
        _dialogue = gameObject.GetOrAddComponent<DialogueAnimator>();

        var slime = Util.FindChild<Image>(gameObject, "0_Slime_Tame", true);
        if (slime != null) { _slimeImg = slime; _slimeTr = slime.transform; }

        // [DOTweenAnimation 규약 예외 — 의도적 코드 대체] 슬34 등장 슬라이드인.
        //  prefab "Tamer Move"는 4-loop 바운스(Move loops4 dur0.125), "Monster Move"는 짧은 0.5s 수평이라
        //  설계 의도(1.5s 깔끔한 수평 슬라이드인)와 안 맞고, isFrom anim이 Start에서 FROM 위치로 스냅시켜
        //  rest 캡처를 오염시킨다. → 두 prefab Move를 비활성화하고 아래 CoEnter에서 코드 수평 슬라이드로 구동.
        //  (컴포넌트로 되돌리려면 prefab Move를 1.5s 수평으로 재저작해야 함 — 그 전엔 코드가 정답.)
        DisablePrefabAnim(GetImage((int)Images.TamerImg)?.gameObject, "Tamer Move");
        DisablePrefabAnim(_slimeTr?.gameObject, "Monster Move");

        // 설계 rest 위치 (prefab serialized: TamerImg=(-14,-66), 0_Slime_Tame=(35,130)).
        //  런타임 캡처는 isFrom anim 스냅에 오염될 수 있어 prefab 값으로 고정. (디자이너가 옮기면 갱신)
        _tamerRest = new Vector2(-14f, -66f);
        _monRest   = new Vector2(35f, 130f);

        // 슬33 Popup Anim: From y -120, OutBack 0.5s (UI_Popup 기본값)
        OpenAnimation();

        GetButton((int)Buttons.ExitBtn).gameObject.BindEvent(OnClickExit);
        GetButton((int)Buttons.LureBtn).gameObject.BindEvent(OnClickLure);
        GetButton((int)Buttons.UseMandrakeBtn).gameObject.BindEvent(OnClickMandrake);
        GetButton((int)Buttons.MinimonInfoBtn).gameObject.BindEvent(OnClickMinimonInfo);

        return true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // 잡기 팝업 닫힘 — 원래 배경음악으로 복귀.
        Managers.Sound?.PopBgm();
    }

    public void SetInfo(int dataId)
    {
        _dataId = dataId;
        _busy = true;

        _mm = null;
        Managers.Data.MinMonDic.TryGetValue(dataId, out _mm);
        _creature = null;
        Managers.Data.CreatureDic.TryGetValue(dataId, out _creature);
        _cName = _creature != null && !string.IsNullOrEmpty(_creature.Name) ? Util.Localize(_creature.Name) : Util.Localize(LocalizeKey.MinimonDefaultNameText);

        // 슬31: 타이틀 = CatchMinimonTitleText + "!" ("미니몬 잡기!"). prefab 영어 하드코딩 교체 → 한/영 전환.
        var title = GetText((int)Texts.Txt_Title_03);
        if (title != null)
        {
            title.text = Util.Localize(LocalizeKey.CatchMinimonTitleText) + "!";
            ApplyFont(title, FontType.UI);
            title.color = Util.HexToColor("FDFFB6");   // 슬31: Title Text Color FDFFB6 (Outline 003F82은 프리펩 머티리얼)
        }

        // [나의 미니몬] 버튼 라벨 — 한/영. ⚠️ "MyMinimon" 키는 미존재 → MyMinimonText("나의 미니몬"/"MY MINIMON") 사용.
        var infoBtnTxt = GetText((int)Texts.Txt_MinimonInfoBtn_01);
        if (infoBtnTxt != null)
        {
            infoBtnTxt.text = Util.Localize(LocalizeKey.MyMinimonText);
            ApplyFont(infoBtnTxt, FontType.UI);
        }

        ApplyMonsterImage();
        ApplyLanguage();
        RefreshMandrakeButton();   // 슬31: 가랏 라벨 lime→hex + 만드라고라 이미지 밑 소지 수(AmountTxt)
        RefreshLureButton();       // 슬31: 길들인다! 라벨 = 성공확률% + 시도횟수/확정횟수(천장). baked 정적값 교체

        // 초기 상태: 버튼/확인팝업/로딩 숨김 (연출 코루틴이 단계적으로 노출)
        SetButtonsActive(false);
        SetActiveSafe(GameObjects.TameConfirmPopups, false);
        SetActiveSafe(GameObjects.LoadingBox1, false);
        SetActiveSafe(GameObjects.LoadingBox2, false);
        SetActiveSafe(Images.UseHappyMandrakeImg, false);

        StopAllCoroutines();
        StartCoroutine(CoEnter());
    }

    // ─────────────────────────────────────────────
    // 슬33~34: 진입 시퀀스 (로딩 → Setup → 대사 → 버튼)
    // ─────────────────────────────────────────────
    IEnumerator CoEnter()
    {
        _busy = true;

        // 미니몬/플레이어 컨테이너 활성. Grayscale 머티리얼은 prefab 기본 적용 상태(슬34) → 도착 후 해제만.
        SetActiveSafe(GameObjects.Imgs_off, true);
        SetActiveSafe(GameObjects.TamePopupMonster_1, true);
        SetEmojActive(Images.MonsterEmoj, false);
        SetEmojActive(Images.TamerEmoj, false);

        // 슬33 Opening Anim: Popup Anim 끝난 직후 회색 Sprite(None) 오버레이가 창을 덮고 깜빡임
        //  (Color DBDBDA ↔ 7B7B7B, OutQuad, Yoyo 6 loops × 0.25s = 1.5s)
        yield return CoOpeningFlicker();

        // 슬33 Loading Anim: Type1/Type2 무작위 (autoPlay Fade 시퀀스가 자동 재생)
        bool type1 = Random.value < 0.5f;
        SetActiveSafe(GameObjects.LoadingBox1, type1);
        SetActiveSafe(GameObjects.LoadingBox2, !type1);
        yield return new WaitForSecondsRealtime(1.2f);
        SetActiveSafe(GameObjects.LoadingBox1, false);
        SetActiveSafe(GameObjects.LoadingBox2, false);

        // 슬34 Setup Anim: 몬스터=화면 밖 왼쪽→오른쪽, 플레이어=화면 밖 오른쪽→왼쪽으로 1.5s 슬라이드인.
        //  이동 중 Grayscale(prefab 머티리얼) 유지, 도착 후 컬러. (prefab "Tamer/Monster Move"가 대각선으로
        //  떨어지는 이상한 움직임이라 코드 수평 슬라이드인으로 대체)
        var tamerImg = GetImage((int)Images.TamerImg);
        var tamerRT  = tamerImg != null ? tamerImg.rectTransform : null;
        var monRT    = _slimeImg != null ? _slimeImg.rectTransform : null;
        const float OFF = 320f;
        if (tamerImg != null) tamerImg.gameObject.SetActive(true);
        if (_slimeImg != null) _slimeImg.gameObject.SetActive(true);
        if (tamerRT != null)
        {
            tamerRT.DOKill();
            tamerRT.anchoredPosition = new Vector2(_tamerRest.x + OFF, _tamerRest.y); // 오른쪽 화면 밖
            tamerRT.DOAnchorPos(_tamerRest, 1.5f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
        if (monRT != null)
        {
            monRT.DOKill();
            monRT.anchoredPosition = new Vector2(_monRest.x - OFF, _monRest.y);       // 왼쪽 화면 밖
            monRT.DOAnchorPos(_monRest, 1.5f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
        yield return new WaitForSecondsRealtime(1.5f);

        // 도착 후 Grayscale 해제 (컬러)
        SetMaterial(tamerImg, null);
        SetMaterial(_slimeImg, null);

        // 슬34: 미니몬 머리 위 Emoj_Exclamation 재생 → 애니가 끝나면 이모지 사라짐
        Managers.Sound?.PlaySFX("SFX_Minimon_Appear");   // 미니몬 등장
        PlayEmoj(Images.MonsterEmoj, "Emoj_Exclamation");
        yield return new WaitForSecondsRealtime(EMOJ_LEN);
        SetEmojActive(Images.MonsterEmoj, false);

        // 슬32 순서1: CatchMinimonStartText1 + CreatureName + CatchMinimonStartText2 (로컬라이즈, 한/영)
        yield return CoType(Util.ApplyJosa(
            Util.Localize(LocalizeKey.CatchMinimonStartText1) + _cName + Util.Localize(LocalizeKey.CatchMinimonStartText2)));
        ShowButton(Buttons.LureBtn);
        RefreshLureButton();   // 슬31: 등장 직전 최신 시도횟수/천장 반영 (언어/카운트)
        yield return new WaitForSecondsRealtime(0.5f);
        ShowButton(Buttons.UseMandrakeBtn);
        RefreshMandrakeButton();   // 슬31/37: 만드라고라 보유 수에 따라 게이팅
        yield return new WaitForSecondsRealtime(0.5f);
        ShowButton(Buttons.MinimonInfoBtn);   // 슬34-4: 나의 미니몬은 가랏 표시 0.5초 뒤 마지막 등장

        // 슬34-5: 모든 버튼 등장 후 상호작용 가능 (interactable ON, 만드라고라는 보유 수 게이팅)
        SetButtonInteractable(Buttons.LureBtn, true);
        SetButtonInteractable(Buttons.MinimonInfoBtn, true);
        RefreshMandrakeButton();
        _busy = false;
    }

    string _mandLabelKr, _mandLabelEn;   // 가랏 라벨 baked 원문 캐시(lime→hex 치환본)

    // 슬31/37: 만드라고라 보유 수 0이면 [가랏! 만드라고라!] 비활성.
    //  라벨엔 성공확률 100% + "필요: 1개의 [만드라고라 이미지]"(prefab baked) — lime→hex만 치환.
    //  소지 수는 만드라고라 이미지 밑 AmountTxt에 lime(#00FF00)/red 작게 표기.
    void RefreshMandrakeButton()
    {
        int owned = Managers.Game.MandrakeCount;
        var btn = GetButton((int)Buttons.UseMandrakeBtn);
        if (btn != null) btn.interactable = owned > 0;

        // 가랏 라벨: ⚠️ TMP 'lime' 미지원 → baked <color=lime>를 hex로 치환(캐시 1회). 소지 수는 append 안 함.
        var krTxt = GetText((int)Texts.KorMandrakeBtn);
        if (krTxt != null) { if (_mandLabelKr == null) _mandLabelKr = krTxt.text.Replace("=lime>", "=#00FF00>"); krTxt.text = _mandLabelKr; }
        var enTxt = GetText((int)Texts.EngMandrakeBtn);
        if (enTxt != null) { if (_mandLabelEn == null) _mandLabelEn = enTxt.text.Replace("=lime>", "=#00FF00>"); enTxt.text = _mandLabelEn; }

        // 슬31: 만드라고라 이미지 밑 소지 수 (AmountTxt) — >0=lime(#00FF00) / 0=red
        var amount = GetText((int)Texts.AmountTxt);
        if (amount != null)
        {
            amount.text = $"<color={(owned > 0 ? "#00FF00" : "red")}>{owned}</color>";
            ApplyFont(amount, FontType.UI);
        }
    }

    // 슬31: [길들인다!] 라벨 동적 갱신 — prefab baked("성공확률:10%", "1/10번째에 확정")는 정적값이라
    //  시도해도 카운터가 안 변하고(차감 안 됨) 천장 도달도 표기 안 됐다. 실데이터(MinMonManager)로 매번 갱신.
    //   · 성공확률 = GetCatchChance(능력 보너스 포함). 천장 도달 시 100%(lime).
    //   · 시도/확정 = GetTameCount / GetCatchCeil. 천장 도달 시 GuaranteedCatchText("확정 길들이기!")로 교체.
    void RefreshLureButton()
    {
        int lbl = LabelOf(Buttons.LureBtn);
        var txt = lbl >= 0 ? GetText(lbl) : null;
        if (txt == null) return;

        int tame = Managers.MinMon.GetTameCount(_dataId);
        int ceil = Managers.MinMon.GetCatchCeil(_dataId);
        bool guaranteed = Managers.MinMon.IsCatchGuaranteed(_dataId);
        int pct = guaranteed ? 100 : Mathf.Clamp(Mathf.RoundToInt(Managers.MinMon.GetCatchChance(_dataId) * 100f), 0, 100);

        // 머리글(▶길들인다!) + 성공확률 줄(천장=lime / 그 외=red. ⚠️ TMP 'lime' 미지원 → hex)
        string head = Util.Localize(LocalizeKey.LureBtnText);
        string rateLine = $"<size=7>{Util.Localize(LocalizeKey.CatchSuccessRateText)}<color={(guaranteed ? "#00FF00" : "red")}>{pct}%</color></size>";

        // 카운트 줄: 천장 도달 시 "확정 길들이기!"(lime→hex) / 그 외 "시도횟수/확정횟수 + 번째에 확정"(슬31/잡기#4).
        //  확정 길들이기 횟수 = 천장(ceil) — tame이 ceil에 도달하면 확정. orange로 강조.
        string countLine = guaranteed
            ? $"<size=7>{Util.Localize(LocalizeKey.GuaranteedCatchText).Replace("=lime>", "=#00FF00>")}</size>"
            : $"<size=7>{tame}/<color=orange>{ceil}</color>{Util.Localize(LocalizeKey.CatchSuccessAtText)}</size>";

        txt.text = $"{head}\n   {rateLine}\n   {countLine}";
        ApplyFont(txt, FontType.UI);
    }

    // 슬33 Opening Anim: 회색 Sprite(None) 오버레이(BlockImg)가 본문을 덮고 깜빡임.
    IEnumerator CoOpeningFlicker()
    {
        var block = GetImage((int)Images.BlockImg);
        if (block == null) yield break;
        block.gameObject.SetActive(true);
        block.color = Util.HexToColor("DBDBDA");
        block.DOKill();
        block.DOColor(Util.HexToColor("7B7B7B"), 0.25f)
             .SetEase(Ease.OutQuad).SetLoops(6, LoopType.Yoyo).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.25f * 6f);
        block.DOKill();
        block.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // 슬35~36: [길들인다!] Lure
    // ─────────────────────────────────────────────
    void OnClickLure()
    {
        if (_busy) return;
        _busy = true;
        StartCoroutine(CoLure());
    }

    IEnumerator CoLure()
    {
        SetButtonsActive(false);

        // 슬32 순서2: TryLureText
        yield return CoType(Util.Localize(LocalizeKey.TryLureText));

        // 슬35: 플레이어 Lure Anim — prefab DOTweenAnimation "Tamer Lure"(Move Y Yoyo 0.125s ×4)
        PlayAnim(GetImage((int)Images.TamerImg)?.gameObject, "Tamer Lure");
        yield return new WaitForSecondsRealtime(0.5f);

        // 슬35: 0.5s 뒤 플레이어 머리 위 Emoj_Lure — 재생이 끝난 뒤 화면 흔들림
        Managers.Sound?.PlaySFX("SFX_Charm");   // 길들이기 시도
        PlayEmoj(Images.TamerEmoj, "Emoj_Lure");
        yield return new WaitForSecondsRealtime(EMOJ_LEN);

        // 화면 흔들림
        yield return CoShake();

        // 잡기 판정 (천장/확률은 MinMonManager가 처리). 테스트 강제 결과 우선.
        bool success;
        if (DebugForceCatch == 1) { Managers.MinMon.TryCatch(_dataId, forceSuccess: true); success = true; }
        else if (DebugForceCatch == 2) { Managers.MinMon.TryCatch(_dataId, forceFail: true); success = false; }   // 강제 실패도 시도횟수 차감
        else success = Managers.MinMon.TryCatch(_dataId);
        DebugForceCatch = 0;

        // 시도 직후 버튼 라벨(시도횟수/확정횟수)을 즉시 갱신한다.
        //  TryCatch는 실패 시 TameCount를 올리지만, 여기서 다시 그리지 않으면 화면 숫자가 그대로라
        //  **기회가 차감되지 않는 것처럼 보인다.** 예전에 같은 증상을 한 번 고쳤는데(303행 주석)
        //  갱신 지점이 Init/등장 시점뿐이라 시도 후에는 다시 낡은 값이 남아 있었다.
        // 성공 직후에는 이번 시도의 확률을 유지한다. 첫 포획 100%가 재포획 확률로 바뀌지 않게 한다.
        if (!success) RefreshLureButton();

        // ⚠️ 갱신만으로는 부족했다. 시도 중에는 버튼이 숨겨져 있고 **실패하면 미니몬이 도망가며
        //    팝업이 곧장 닫힌다** — 바뀐 숫자를 볼 틈이 아예 없어서 여전히 "차감이 안 된다"로 보였다.
        //    갱신된 라벨을 다시 띄워 두고(누르지는 못하게) 결과 연출 내내 보이게 한다.
        ShowButton(Buttons.LureBtn);
        SetButtonInteractable(Buttons.LureBtn, false);

        // "..." Emoj_Dots(생각하는 텀)를 먼저 충분히(약 3초) 보여준 뒤 결과/효과 텍스트를 낸다.
        Managers.Sound?.PlaySFX("SFX_MinimonThink");   // 생각하는 텀
        PlayEmoj(Images.MonsterEmoj, "Emoj_Dots");
        yield return new WaitForSecondsRealtime(3f);
        SetEmojActive(Images.MonsterEmoj, false);

        // Dots 이후 효과 텍스트 — 효과有=성공(Excited+LureEffective) / 효과無=실패(Name+LureNotEffective).
        //  (효과 없음 문구는 실패 시에만 — 긍정 결과에선 안 나온다)
        if (success)
        {
            Managers.Sound?.PlaySFX("SFX_LureHit");   // 효과 有(흥미)
            PlayEmoj(Images.MonsterEmoj, "Emoj_Excited");
            yield return CoType(Util.Localize(LocalizeKey.LureEffectiveText));   // 순서3-1
            yield return new WaitForSecondsRealtime(EMOJ_LEN);
        }
        else
        {
            Managers.Sound?.PlaySFX("SFX_LureFail");   // 효과 無
            // 순서3-2 (KR=CreatureName+LureNotEffectiveText / EN=LureNotEffectiveText만)
            yield return CoType((IsKorean ? _cName : "") + Util.Localize(LocalizeKey.LureNotEffectiveText));
        }

        yield return CoResult(success);
    }

    // ─────────────────────────────────────────────
    // 슬37: [가랏! 만드라고라!]
    // ─────────────────────────────────────────────
    void OnClickMandrake()
    {
        if (_busy) return;
        // 슬31/37: 만드라고라 보유 수 체크 — 없으면 무시(버튼도 비활성이지만 이중 가드)
        if (Managers.Game.MandrakeCount <= 0)
        {
            Debug.Log("[TamePopup] 만드라고라 부족 — 확정 포획 불가");
            return;
        }
        _busy = true;
        StartCoroutine(CoMandrake());
    }

    IEnumerator CoMandrake()
    {
        SetButtonsActive(false);

        // 슬31/37: 만드라고라 1개 소비 (보유 수는 OnClickMandrake에서 이미 검증)
        Managers.Game.MandrakeCount--;
        // 슬32 순서2(가랏): GoMandrakeText
        yield return CoType(Util.Localize(LocalizeKey.GoMandrakeText));
        yield return new WaitForSecondsRealtime(1f);

        // 포물선 Happy Mandrake IMG (플레이어 중앙 → 미니몬 중앙, 0.67s)
        var mandrake = GetImage((int)Images.UseHappyMandrakeImg);
        if (mandrake != null && _slimeTr != null)
        {
            mandrake.gameObject.SetActive(true);
            var rt = mandrake.rectTransform;
            Vector2 start = rt.anchoredPosition;
            Vector2 end = _slimeTr is RectTransform srt ? srt.anchoredPosition : start;
            rt.DOKill();
            // 포물선: DOAnchorPos + DOAnchorPosY는 같은 anchoredPosition.Y를 동시에 건드려 충돌 →
            //  호(arc) 전용 DOJumpAnchorPos 단일 트윈으로 처리(jumpPower 80, 1회, 0.67s).
            rt.DOJumpAnchorPos(end, 80f, 1, 0.67f).SetEase(Ease.OutQuad).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.67f);
            // 도착 후 scale 1.5 / alpha 0 (0.33s Linear)
            rt.DOScale(1.5f, 0.33f).SetEase(Ease.Linear).SetUpdate(true);
            mandrake.DOFade(0f, 0.33f).SetEase(Ease.Linear).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.33f);
            mandrake.gameObject.SetActive(false);
        }

        // 슬37-4: 미니몬 Emoj_Exclamation + Jump Anim — prefab "Monster Mandrake"(Move Y Yoyo 0.15s ×2 = 0.3s)
        PlayEmoj(Images.MonsterEmoj, "Emoj_Exclamation");
        Managers.Sound?.PlaySFX("SFX_UseHappyMandrake");   // 만드라고라 투척
        PlayAnim(_slimeTr?.gameObject, "Monster Mandrake");
        yield return new WaitForSecondsRealtime(0.3f);

        // 슬37-5: Jump 종료 후 Eat Mandrake Anim — Scale Yoyo 0.05s ×24 TO 1.1 (= 1.2s)
        if (_slimeTr != null)
        {
            _slimeTr.DOKill();
            _slimeTr.DOScale(1.1f, 0.05f).SetEase(Ease.OutQuad).SetLoops(24, LoopType.Yoyo).SetUpdate(true);
        }
        yield return new WaitForSecondsRealtime(0.05f * 24f);

        // 슬37-6/슬32 순서3: Eat 종료 후 미니몬 Emoj_Shy + CreatureName + EatMandrakeText
        PlayEmoj(Images.MonsterEmoj, "Emoj_Shy");
        Managers.Sound?.PlaySFX("SFX_MinimonEat");   // 만드라고라 먹기
        yield return CoType(_cName + Util.Localize(LocalizeKey.EatMandrakeText));
        yield return new WaitForSecondsRealtime(1.2f);

        // 만드라고라는 확정 성공
        Managers.MinMon.TryCatch(_dataId, forceSuccess: true);
        yield return CoResult(true);
    }

    // ─────────────────────────────────────────────
    // 슬36: 결과 처리
    // ─────────────────────────────────────────────
    IEnumerator CoResult(bool success)
    {
        if (success)
        {
            // 슬36-1: 미니몬 Caught Anim — prefab "Monster Caught"(Move Y Yoyo 0.18s ×4 = 0.72s)
            Managers.Sound?.PlaySFX("SFX_MinimonCaught");   // 포획 성공
            PlayAnim(_slimeTr?.gameObject, "Monster Caught");
            yield return new WaitForSecondsRealtime(0.72f);

            // 슬36-2: Anim 끝난 뒤 순서4-1 Text + 미니몬 머리 위 Emoj_TwoHearts (재생 끝까지 본다)
            PlayEmoj(Images.MonsterEmoj, "Emoj_TwoHearts");
            yield return CoType(Util.Localize(LocalizeKey.LureSuccessText1) + _cName + Util.Localize(LocalizeKey.LureSuccessText2));
            yield return new WaitForSecondsRealtime(EMOJ_LEN);   // TwoHearts 재생 완료까지

            // 슬36-3: TwoHearts 끝나면 플레이어 머리 위 Emoj_Happy (재생 끝까지)
            PlayEmoj(Images.TamerEmoj, "Emoj_Happy");
            yield return new WaitForSecondsRealtime(EMOJ_LEN);

            // 슬36-4: Happy 끝나면 GlowRays [나의 미니몬] Popup (첫포획 NEW / 재포획 향상 연출)
            bool first = Managers.MinMon.LastCatchWasFirst;
            bool improved = Managers.MinMon.LastCatchImproved;
            ClosePopupUI();
            var info = Managers.UI.ShowPopupUI<UI_MinionInfoPopup>();
            info.SetInfo(_dataId);
            info.PlayCatchResult(first, improved);
        }
        else
        {
            // 슬36 실패: Emoj_Sweat(미니몬) + 플레이어 Emoj_Sweat2 + 순서4-2 Text
            Managers.Sound?.PlaySFX("SFX_MinimonRunaway");   // 도망
            PlayEmoj(Images.MonsterEmoj, "Emoj_Sweat");
            PlayEmoj(Images.TamerEmoj, "Emoj_Sweat2");
            yield return CoType(_cName + Util.Localize(LocalizeKey.LureFailText));
            yield return new WaitForSecondsRealtime(0.3f);

            // 도망 시작 직전 머리 위 Emoj 숨김 (Emoj는 슬라임과 별개 노드라 안 숨기면 제자리에 남음)
            SetEmojActive(Images.MonsterEmoj, false);
            SetEmojActive(Images.TamerEmoj, false);

            // 슬36 Runaway: 기존 그대로 Move InBack 0.5s(X+360) — InBack 예비동작으로 왼쪽 갔다가 오른쪽 화면 밖.
            //  + 플립: 왼쪽 보기로 시작 → 오른쪽으로 가기 시작할 때(예비동작 후) 플립해 오른쪽 보기.
            //  슬라임 기본 scale.x(+)=왼쪽 보기 / (-)=오른쪽 보기.
            var srt = _slimeImg != null ? _slimeImg.rectTransform : null;
            if (srt != null && _slimeTr != null)
            {
                srt.DOKill();
                float baseX = Mathf.Abs(_slimeTr.localScale.x);
                _slimeTr.localScale = new Vector3(baseX, _slimeTr.localScale.y, _slimeTr.localScale.z);   // 왼쪽 보기
                srt.DOAnchorPos(new Vector2(srt.anchoredPosition.x + 360f, srt.anchoredPosition.y), 0.5f)
                   .SetEase(Ease.InBack).SetUpdate(true);
                yield return new WaitForSecondsRealtime(0.22f);   // InBack 왼쪽 예비동작(최저점 ~0.21s)까지 왼쪽 보기 유지
                _slimeTr.localScale = new Vector3(-baseX, _slimeTr.localScale.y, _slimeTr.localScale.z);   // 오른쪽으로 가기 시작할 때 플립
                yield return new WaitForSecondsRealtime(0.28f);
            }
            OnClickBG();
        }
    }

    void OnClickMinimonInfo()
    {
        Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(_dataId);
    }

    void OnClickExit() => OnClickBG();

    // ─────────────────────────────────────────────
    // 표기
    // ─────────────────────────────────────────────
    void ApplyMonsterImage()
    {
        if (_slimeImg == null) return;

        // MinMonData 아이콘 → 없거나 로드 실패면 CreatureData 아이콘.
        //  폴백이 없으면 프리팹에 저작된 **초록 슬라임이 그대로 남아** 광부망령을 잡는데
        //  슬라임이 나오는 이름-그림 불일치가 된다. (한쪽만 보고 return 하던 게 원인)
        Sprite sprite = null;
        foreach (string label in new[] { _mm?.IconLabel, _creature?.IconLabel })
        {
            if (string.IsNullOrEmpty(label)) continue;
            sprite = Managers.Resource.Load<Sprite>(label);
            if (sprite != null) break;
        }
        if (sprite == null) return;

        // 이 프로젝트의 단골 함정 — Animator가 매 프레임 sprite를 덮어써 방금 넣은 그림을 지운다.
        var anim = _slimeImg.GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        _slimeImg.sprite = sprite;
        _slimeImg.preserveAspect = true;
    }

    // 버튼 그룹(라벨)의 표시는 SetButtonsActive가 담당하므로 여기선 확인팝업만 언어 토글.
    void ApplyLanguage()
    {
        bool kr = Managers.Game.Language == Language.Korean;
        SetActiveSafe(GameObjects.KOR_TameConfirmPopup, kr);
        SetActiveSafe(GameObjects.ENG_TameConfirmPopup, !kr);
    }

    bool IsKorean => Managers.Game.Language == Language.Korean;

    // ─────────────────────────────────────────────
    // 연출 유틸
    // ─────────────────────────────────────────────
    IEnumerator CoType(string text)
    {
        var box = GetText((int)Texts.showTextBox);
        if (box == null) yield break;
        ApplyFont(box, FontType.UI);
        bool done = false;
        _dialogue.PlayTyping(box, text, DialogueEmotion.Normal, () => done = true);
        while (!done) yield return null;
    }

    IEnumerator CoShake()
    {
        if (_frameRT != null)
        {
            _frameRT.DOShakeAnchorPos(0.4f, new Vector3(8f, 0f, 0f), 20, 90f, false, true)
                    .SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }

    void PlayAnim(GameObject go, string id)
    {
        if (go == null) return;
        foreach (var a in go.GetComponents<DOTweenAnimation>())
        {
            if (a.id != id) continue;
            // 컴포넌트의 DORestart는 같은 오브젝트의 다른 모션까지 재시작한다.
            // 필요한 저작 Tween만 준비하고 재생해 입장/유혹/성공 모션이 서로 간섭하지 않게 한다.
            if (a.tween == null || !a.tween.IsActive()) a.CreateTween(false, false);
            a.tween?.Restart();
            return;
        }
    }

    // 특정 id의 prefab DOTweenAnimation을 비활성화(스냅/자동재생 방지). 코드로 대체할 때 사용.
    static void DisablePrefabAnim(GameObject go, string id)
    {
        if (go == null) return;
        foreach (var a in go.GetComponents<DOTweenAnimation>())
        {
            if (a.id != id) continue;
            // DOKill은 같은 GameObject의 모든 Tween을 죽이므로 교체한 입장 모션만 직접 종료한다.
            a.tween?.Kill();
            a.tween = null;
            a.autoGenerate = false;
            a.enabled = false;
        }
    }

    void PlayEmoj(Images id, string state)
    {
        var img = GetImage((int)id);
        if (img == null) return;
        img.gameObject.SetActive(true);
        var anim = img.GetComponent<Animator>();
        if (anim != null) { anim.enabled = true; anim.Play(state, 0, 0f); }
    }

    void SetEmojActive(Images id, bool on)
    {
        var img = GetImage((int)id);
        if (img != null) img.gameObject.SetActive(on);
    }

    static void SetMaterial(Image img, Material mat)
    {
        if (img != null) img.material = mat;
    }

    // 버튼 → 라벨 텍스트 (라벨이 버튼과 다른 컨테이너에 있어 함께 토글해야 순차 등장이 됨)
    int LabelOf(Buttons id)
    {
        switch (id)
        {
            case Buttons.LureBtn:        return IsKorean ? (int)Texts.KorLureBtn     : (int)Texts.EngLureBtn;
            case Buttons.UseMandrakeBtn: return IsKorean ? (int)Texts.KorMandrakeBtn : (int)Texts.EngMandrakeBtn;
            case Buttons.MinimonInfoBtn: return (int)Texts.Txt_MinimonInfoBtn_01;
            default: return -1;
        }
    }

    // 슬34: 버튼 1개를 라벨과 함께 등장 + pop anim. (가랏 버튼은 만드라고라 카운트 박스도 함께)
    void ShowButton(Buttons id)
    {
        var btn = GetButton((int)id);
        if (btn != null)
        {
            btn.gameObject.SetActive(true);
            btn.GetComponent<DOTweenAnimation>()?.DORestart();
        }
        int lbl = LabelOf(id);
        if (lbl >= 0) { var t = GetText(lbl); if (t != null) t.gameObject.SetActive(true); }

        // 가랏! 버튼과 함께 만드라고라 필요개수 박스 표시
        if (id == Buttons.UseMandrakeBtn) SetButtonActiveSafe(Buttons.HappyMandrakeBox, true);
    }

    // 버튼 + 라벨 + 만드라고라 박스를 모두 토글. on=false면 라벨 컨테이너만 켜고 내부는 전부 숨김(순차 등장 준비).
    void SetButtonsActive(bool on)
    {
        // 라벨 컨테이너는 켜두되(라벨을 개별 토글할 수 있게), 내부 라벨/버튼은 숨김
        SetActiveSafe(GameObjects.KorBtnGroup, IsKorean);
        SetActiveSafe(GameObjects.EngBtnGroup, !IsKorean);

        foreach (Buttons b in new[] { Buttons.LureBtn, Buttons.UseMandrakeBtn, Buttons.MinimonInfoBtn })
        {
            SetButtonActiveSafe(b, on);
            int lbl = LabelOf(b);
            if (lbl >= 0) { var t = GetText(lbl); if (t != null) t.gameObject.SetActive(on); }
        }
        // 슬3 만드라고라 카운트 박스 — 포획 연출 중엔 숨김(이게 안 숨겨져 포획 애니 동안 만드라고라가 보였음)
        SetButtonActiveSafe(Buttons.HappyMandrakeBox, on);
        SetActiveSafe(Images.UseHappyMandrakeImg, false);
    }

    void SetButtonActiveSafe(Buttons id, bool on)
    {
        var btn = GetButton((int)id);
        if (btn != null) btn.gameObject.SetActive(on);
    }

    void SetButtonInteractable(Buttons id, bool on)
    {
        var btn = GetButton((int)id);
        if (btn != null) btn.interactable = on;
    }

    void SetActiveSafe(GameObjects id, bool on)
    {
        var go = GetObject((int)id);
        if (go != null) go.SetActive(on);
    }

    void SetActiveSafe(Images id, bool on)
    {
        var img = GetImage((int)id);
        if (img != null) img.gameObject.SetActive(on);
    }
}
