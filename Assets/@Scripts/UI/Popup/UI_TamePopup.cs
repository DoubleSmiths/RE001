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

    int _dataId;
    bool _busy;                 // 연출/시도 진행 중 (입력 차단)
    MinMonData _mm;
    CreatureData _creature;
    string _cName;
    DialogueAnimator _dialogue;
    Transform _slimeTr;         // 0_Slime_Tame (숫자 시작이라 enum 불가 → 직접 탐색)
    Image _slimeImg;

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
        _cName = _creature != null && !string.IsNullOrEmpty(_creature.Name) ? _creature.Name : "미니몬";

        var title = GetText((int)Texts.Txt_Title_03);
        if (title != null) ApplyFont(title, FontType.UI);

        ApplyMonsterImage();
        ApplyLanguage();
        RefreshRateTexts();

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

        // 슬33 Loading Anim: Type1/Type2 무작위 (autoPlay Fade 시퀀스가 자동 재생)
        bool type1 = Random.value < 0.5f;
        SetActiveSafe(GameObjects.LoadingBox1, type1);
        SetActiveSafe(GameObjects.LoadingBox2, !type1);
        yield return new WaitForSecondsRealtime(1.2f);
        SetActiveSafe(GameObjects.LoadingBox1, false);
        SetActiveSafe(GameObjects.LoadingBox2, false);

        // 슬34 Setup Anim: prefab Move DOTweenAnimation 트리거 (화면 밖 → 지정위치 1.5s)
        PlayAnim(GetImage((int)Images.TamerImg)?.gameObject, "Tamer Move");
        PlayAnim(_slimeTr?.gameObject, "Monster Move");
        yield return new WaitForSecondsRealtime(1.5f);

        // 도착 후 Grayscale 해제 (None)
        SetMaterial(GetImage((int)Images.TamerImg), null);
        SetMaterial(_slimeImg, null);

        // 미니몬 머리 위 Emoj_Exclamation
        PlayEmoj(Images.MonsterEmoj, "Emoj_Exclamation");
        yield return new WaitForSecondsRealtime(0.3f);

        // 대사 표시 (슬32 순서1: CatchMinimonStartText1)
        yield return CoType($"야생의 {_cName}(이)가 나타났다!");

        // 버튼 등장 (슬34: 길들인다/가랏 표시 → 0.5s 후 나의미니몬). 라벨 그룹 활성 후 버튼 pop.
        SetActiveSafe(IsKorean ? GameObjects.KorBtnGroup : GameObjects.EngBtnGroup, true);
        ShowButton(Buttons.LureBtn);
        yield return new WaitForSecondsRealtime(0.5f);
        ShowButton(Buttons.UseMandrakeBtn);
        yield return new WaitForSecondsRealtime(0.5f);
        ShowButton(Buttons.MinimonInfoBtn);

        _busy = false; // 모든 버튼 등장 후 상호작용 가능
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

        // 순서2: TryLureText
        yield return CoType("길들이기를 시도한다!");

        // 플레이어 Lure Anim (슬35: Move Y 위로, 0.125s, Yoyo 4)
        var tamer = GetImage((int)Images.TamerImg);
        if (tamer != null)
        {
            var rt = tamer.rectTransform;
            rt.DOKill();
            rt.DOAnchorPosY(rt.anchoredPosition.y + 18f, 0.125f)
              .SetEase(Ease.OutQuad).SetLoops(4, LoopType.Yoyo).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // 0.5s 뒤 플레이어 머리 위 Emoj_Lure
        PlayEmoj(Images.TamerEmoj, "Emoj_Lure");
        yield return new WaitForSecondsRealtime(0.6f);

        // 화면 흔들림
        yield return CoShake();

        // 잡기 판정 (천장/확률은 MinMonManager가 처리)
        bool success = Managers.MinMon.TryCatch(_dataId);

        // 효과 有/無 분기 (슬35) — TODO: 종족별 면역(효과 없는 미니몬) 데이터 연동. 현재는 항상 효과 있음.
        // 순서3: LureEffective
        PlayEmoj(Images.MonsterEmoj, "Emoj_Excited");
        yield return CoType($"{_cName}(이)가 흥미를 보인다!");
        PlayEmoj(Images.MonsterEmoj, "Emoj_Dots");
        yield return new WaitForSecondsRealtime(0.6f);

        yield return CoResult(success);
    }

    // ─────────────────────────────────────────────
    // 슬37: [가랏! 만드라고라!]
    // ─────────────────────────────────────────────
    void OnClickMandrake()
    {
        if (_busy) return;
        _busy = true;
        StartCoroutine(CoMandrake());
    }

    IEnumerator CoMandrake()
    {
        SetButtonsActive(false);

        // TODO 슬31/37: 만드라고라 보유 개수 체크 + 소비 (인벤토리 연동)
        // 순서2
        yield return CoType("가랏! 만드라고라!");
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

        // 미니몬 Emoj_Exclamation + Jump Anim
        PlayEmoj(Images.MonsterEmoj, "Emoj_Exclamation");
        if (_slimeTr is RectTransform jrt)
        {
            jrt.DOKill();
            jrt.DOAnchorPosY(jrt.anchoredPosition.y + 20f, 0.15f)
               .SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.3f);
        }

        // 순서3: EatMandrakeText + Emoj_Shy
        PlayEmoj(Images.MonsterEmoj, "Emoj_Shy");
        yield return CoType($"{_cName}(이)가 만드라고라를 먹었다!");
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
            // 미니몬 Caught Anim (Y Yoyo 4)
            if (_slimeTr is RectTransform crt)
            {
                crt.DOKill();
                crt.DOAnchorPosY(crt.anchoredPosition.y + 14f, 0.18f)
                   .SetEase(Ease.OutQuad).SetLoops(4, LoopType.Yoyo).SetUpdate(true);
                yield return new WaitForSecondsRealtime(0.72f);
            }
            PlayEmoj(Images.MonsterEmoj, "Emoj_TwoHearts");
            yield return CoType($"{_cName}(을)를 길들이는 데 성공했다!");
            PlayEmoj(Images.TamerEmoj, "Emoj_Happy");
            yield return new WaitForSecondsRealtime(0.8f);

            // TODO 슬36 #4 / 슬20: GlowRays 적용된 [나의 미니몬] 창 (첫 포획 연출)
            ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(_dataId);
        }
        else
        {
            // 실패: Emoj_Sweat + 미니몬 좌우 반전 후 Runaway
            PlayEmoj(Images.MonsterEmoj, "Emoj_Sweat");
            PlayEmoj(Images.TamerEmoj, "Emoj_Sweat2");
            yield return CoType($"{_cName}(이)가 도망쳤다...");
            if (_slimeTr != null)
            {
                _slimeTr.DOScaleX(-Mathf.Abs(_slimeTr.localScale.x), 0.8f).SetUpdate(true);
                yield return new WaitForSecondsRealtime(1.1f);
            }
            // 슬: 미니몬 Runaway Anim (prefab DOTweenAnimation "Monster Runaway")
            PlayAnim(_slimeTr?.gameObject, "Monster Runaway");
            yield return new WaitForSecondsRealtime(0.5f);
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

    void RefreshRateTexts()
    {
        var amount = GetText((int)Texts.AmountTxt);
        if (amount == null) return;
        ApplyFont(amount, FontType.UI);
        if (_mm == null) { amount.text = ""; return; }

        int tried = Managers.MinMon.GetSaveData(_dataId).TameCount;
        int guaranteed = _mm.MaxTameCount;
        float rate = Mathf.Clamp01(_mm.CatchChance) * 100f;
        bool ceiling = tried >= guaranteed;
        amount.text = ceiling
            ? $"<color=#00FF00>100%</color>  ({tried}/{guaranteed})"
            : $"{rate:0}%  ({tried}/{guaranteed})";
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

    void ShowButton(Buttons id)
    {
        var btn = GetButton((int)id);
        if (btn == null) return;
        btn.gameObject.SetActive(true);
        btn.GetComponent<DOTweenAnimation>()?.DORestart();
    }

    // 버튼 + 언어별 라벨 그룹을 함께 토글 (라벨이 그룹 안에 있어 버튼만 숨기면 라벨이 남음)
    void SetButtonsActive(bool on)
    {
        SetActiveSafe(GameObjects.KorBtnGroup, on && IsKorean);
        SetActiveSafe(GameObjects.EngBtnGroup, on && !IsKorean);
        SetButtonActiveSafe(Buttons.LureBtn, on);
        SetButtonActiveSafe(Buttons.UseMandrakeBtn, on);
        SetButtonActiveSafe(Buttons.MinimonInfoBtn, on);
    }

    void SetButtonActiveSafe(Buttons id, bool on)
    {
        var btn = GetButton((int)id);
        if (btn != null) btn.gameObject.SetActive(on);
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
