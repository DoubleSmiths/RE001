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

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _frameRT = GetObject((int)GameObjects.TamePopup).GetComponent<RectTransform>();
        _dialogue = gameObject.GetOrAddComponent<DialogueAnimator>();

        var slime = Util.FindChild<Image>(gameObject, "0_Slime_Tame", true);
        if (slime != null) { _slimeImg = slime; _slimeTr = slime.transform; }

        // 슬34: prefab의 "Tamer Move"/"Monster Move"는 코드 수평 슬라이드인으로 대체 → 비활성화.
        //  (isFrom anim이 Start에서 오브젝트를 FROM 위치로 스냅시켜 rest 캡처를 망치는 것 방지)
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

    public void SetInfo(int dataId)
    {
        _dataId = dataId;
        _busy = true;

        _mm = null;
        Managers.Data.MinMonDic.TryGetValue(dataId, out _mm);
        _creature = null;
        Managers.Data.CreatureDic.TryGetValue(dataId, out _creature);
        _cName = _creature != null && !string.IsNullOrEmpty(_creature.Name) ? Util.Localize(_creature.Name) : "미니몬";

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
        PlayEmoj(Images.MonsterEmoj, "Emoj_Exclamation");
        yield return new WaitForSecondsRealtime(EMOJ_LEN);
        SetEmojActive(Images.MonsterEmoj, false);

        // 슬32 순서1: CatchMinimonStartText1 + CreatureName + CatchMinimonStartText2 (로컬라이즈, 한/영)
        yield return CoType(Util.Localize(LocalizeKey.CatchMinimonStartText1) + _cName + Util.Localize(LocalizeKey.CatchMinimonStartText2));
        ShowButton(Buttons.LureBtn);
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
        PlayEmoj(Images.TamerEmoj, "Emoj_Lure");
        yield return new WaitForSecondsRealtime(EMOJ_LEN);

        // 화면 흔들림
        yield return CoShake();

        // 잡기 판정 (천장/확률은 MinMonManager가 처리). 테스트 강제 결과 우선.
        bool success;
        if (DebugForceCatch == 1) { Managers.MinMon.TryCatch(_dataId, forceSuccess: true); success = true; }
        else if (DebugForceCatch == 2) { success = false; }
        else success = Managers.MinMon.TryCatch(_dataId);
        DebugForceCatch = 0;

        // "..." Emoj_Dots(생각하는 텀)를 먼저 충분히(약 3초) 보여준 뒤 결과/효과 텍스트를 낸다.
        PlayEmoj(Images.MonsterEmoj, "Emoj_Dots");
        yield return new WaitForSecondsRealtime(3f);
        SetEmojActive(Images.MonsterEmoj, false);

        // Dots 이후 효과 텍스트 — 효과有=성공(Excited+LureEffective) / 효과無=실패(Name+LureNotEffective).
        //  (효과 없음 문구는 실패 시에만 — 긍정 결과에선 안 나온다)
        if (success)
        {
            PlayEmoj(Images.MonsterEmoj, "Emoj_Excited");
            yield return CoType(Util.Localize(LocalizeKey.LureEffectiveText));   // 순서3-1
            yield return new WaitForSecondsRealtime(EMOJ_LEN);
        }
        else
        {
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
            rt.DOAnchorPos(end, 0.67f).SetEase(Ease.OutQuad).SetUpdate(true);
            rt.DOAnchorPosY(end.y + 80f, 0.335f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.67f);
            // 도착 후 scale 1.5 / alpha 0 (0.33s Linear)
            rt.DOScale(1.5f, 0.33f).SetEase(Ease.Linear).SetUpdate(true);
            mandrake.DOFade(0f, 0.33f).SetEase(Ease.Linear).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.33f);
            mandrake.gameObject.SetActive(false);
        }

        // 슬37-4: 미니몬 Emoj_Exclamation + Jump Anim — prefab "Monster Mandrake"(Move Y Yoyo 0.15s ×2 = 0.3s)
        PlayEmoj(Images.MonsterEmoj, "Emoj_Exclamation");
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
        string iconLabel = _mm != null && !string.IsNullOrEmpty(_mm.IconLabel) ? _mm.IconLabel
                          : _creature != null ? _creature.IconLabel : null;
        if (string.IsNullOrEmpty(iconLabel) || _slimeImg == null) return;

        Sprite sprite = Managers.Resource.Load<Sprite>(iconLabel);
        if (sprite != null) { _slimeImg.sprite = sprite; _slimeImg.preserveAspect = true; }
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
            if (a.id == id) { a.DORestart(); return; }
        }
        go.GetComponent<DOTweenAnimation>()?.DORestart();
    }

    // 특정 id의 prefab DOTweenAnimation을 비활성화(스냅/자동재생 방지). 코드로 대체할 때 사용.
    static void DisablePrefabAnim(GameObject go, string id)
    {
        if (go == null) return;
        foreach (var a in go.GetComponents<DOTweenAnimation>())
            if (a.id == id) { a.DOKill(); a.enabled = false; }
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
