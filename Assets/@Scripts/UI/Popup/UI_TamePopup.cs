using Data;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// [미니몬 잡기!] 팝업 (기획서 v3 05.28 슬라이드 28~37 = 미니몬 잡기 #1~#10).
// [미니몬 도감]의 [미니몬 잡기!] 버튼으로 호출되어 포획 미니게임을 진행한다.
// 백엔드(확률/천장)는 MinMonManager.TryCatch가 담당. 본 팝업은 UI/연출 + 결과 호출.
//
// === Phase 1 (현재) ===
//  - enum 바인딩 / 진입·종료 Anim(슬33 Popup Anim From y-120 OutBack 0.5s = UI_Popup.OpenAnimation)
//  - KOR/ENG 그룹 토글, 몬스터 IMG / 성공확률 / 시도횟수 / 만드라고라 개수 표기
//  - [길들인다]/[가랏!만드라고라]/[나의 미니몬]/[나가기] 버튼 → MinMonManager.TryCatch 연결
//  - 성공 시 [나의 미니몬] 창 호출(슬36 #4)
//
// === TODO (슬33~37 연출, Phase 2) ===
//  - 슬33: Opening Anim(Sprite 깜빡임) → Loading Anim Type1/2 랜덤(LoadingBox1/2)
//  - 슬34: Setup Anim(Player/미니몬 IMG 화면 밖→지정위치 1.5s, Grayscale Material → None 교체), Emoj_Exclamation
//  - 슬32/35: TextBox 타이핑(DialogueAnimator), 길들이기 Lure Anim + Emoj_Lure + 화면 흔들림 + 효과有/無 분기
//  - 슬36: 결과 — 성공(Caught Anim + Emoj_TwoHearts/Happy + GlowRays 나의미니몬) / 실패(Emoj_Sweat + Runaway Anim)
//  - 슬37: 만드라고라 — 포물선 Happy Mandrake IMG + Jump/Eat Anim 후 [성공]과 동일 진행
public class UI_TamePopup : UI_Popup
{
    #region Enum

    enum Images
    {
        TamePopupBackground,
        TamePopupPanelFrame,
        TamerImg,            // 플레이어 IMG (슬34 Setup)
        TamerEmoj,           // 플레이어 머리 위 Emoj
        MonsterEmoj,         // 미니몬 머리 위 Emoj
        BlockImg,            // 입력 차단용 오버레이
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
        TamePopupMonster_1,
        LoadingBox1,
        LoadingBox2,
        KorBtnGroup,
        EngBtnGroup,
        TameConfirmPopups,
        KOR_TameConfirmPopup,
        ENG_TameConfirmPopup,
    }

    #endregion

    int _dataId;
    bool _busy;          // 잡기 시도 진행 중 (중복 입력 방지)

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
        // 슬33 Popup Anim: From y -120, OutBack 0.5s (UI_Popup 기본값과 동일)
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
        _busy = false;

        MinMonData mm = null;
        Managers.Data.MinMonDic.TryGetValue(dataId, out mm);

        CreatureData creature = null;
        Managers.Data.CreatureDic.TryGetValue(dataId, out creature);

        // 타이틀 폰트
        var title = GetText((int)Texts.Txt_Title_03);
        if (title != null)
            ApplyFont(title, FontType.UI);

        // 잡기 대상 미니몬 IMG (슬30: MASK 처리된 미니몬 IMG). 0_Slime_Tame은 숫자 시작이라 직접 탐색.
        ApplyMonsterImage(mm, creature);

        // KOR/ENG 그룹 토글 + 버튼 라벨
        ApplyLanguage();

        // 성공확률 / 시도횟수 표기 (슬31)
        RefreshRateTexts(mm);

        // 초기 상태: 확인 팝업/로딩 박스 숨김 (연출은 Phase 2에서 단계적으로 노출)
        SetActiveSafe(GameObjects.TameConfirmPopups, false);
        SetActiveSafe(GameObjects.LoadingBox1, false);
        SetActiveSafe(GameObjects.LoadingBox2, false);
        SetActiveSafe(Images.BlockImg, false);
    }

    // ─────────────────────────────────────────────
    // 표기 갱신
    // ─────────────────────────────────────────────
    void ApplyMonsterImage(MinMonData mm, CreatureData creature)
    {
        string iconLabel = mm != null && !string.IsNullOrEmpty(mm.IconLabel) ? mm.IconLabel
                          : creature != null ? creature.IconLabel : null;
        if (string.IsNullOrEmpty(iconLabel)) return;

        Sprite sprite = Managers.Resource.Load<Sprite>(iconLabel);
        if (sprite == null) return;

        var slime = Util.FindChild<Image>(gameObject, "0_Slime_Tame", true);
        if (slime != null) { slime.sprite = sprite; slime.preserveAspect = true; }
    }

    // 슬31: 성공확률 / "시도횟수 / 확정횟수"
    void RefreshRateTexts(MinMonData mm)
    {
        var amount = GetText((int)Texts.AmountTxt);
        if (amount == null) return;

        ApplyFont(amount, FontType.UI);
        if (mm == null) { amount.text = ""; return; }

        int tried = Managers.MinMon.GetSaveData(_dataId).TameCount;
        int guaranteed = mm.MaxTameCount;
        float rate = Mathf.Clamp01(mm.CatchChance) * 100f;
        // 시도횟수가 확정횟수에 도달하면 100% 확정 (천장)
        bool ceiling = tried >= guaranteed;
        amount.text = ceiling
            ? $"<color=#00FF00>100%</color>  ({tried}/{guaranteed})"
            : $"{rate:0}%  ({tried}/{guaranteed})";
    }

    // 슬: KOR/ENG 그룹 토글
    void ApplyLanguage()
    {
        bool kr = Managers.Game.Language == Language.Korean;
        SetActiveSafe(GameObjects.KorBtnGroup, kr);
        SetActiveSafe(GameObjects.EngBtnGroup, !kr);
        SetActiveSafe(GameObjects.KOR_TameConfirmPopup, kr);
        SetActiveSafe(GameObjects.ENG_TameConfirmPopup, !kr);
    }

    // ─────────────────────────────────────────────
    // 버튼 핸들러
    // ─────────────────────────────────────────────
    // 슬35/36: [길들인다!] — 확률 기반 포획 시도
    void OnClickLure()
    {
        if (_busy) return;
        _busy = true;

        // TODO 슬35: Lure Anim(Y축 4회 Yoyo) + Emoj_Lure + 화면 흔들림 + 효과有/無 분기 연출
        bool success = Managers.MinMon.TryCatch(_dataId);
        Debug.Log($"[Tame] 길들이기 시도 dataId={_dataId} → {(success ? "성공" : "실패")}");
        HandleResult(success);
    }

    // 슬37: [가랏! 만드라고라!] — 확정 포획 (만드라고라 소비)
    void OnClickMandrake()
    {
        if (_busy) return;
        _busy = true;

        // TODO 슬31/37: 만드라고라 보유 개수 체크 + 소비, 포물선 Happy Mandrake IMG / Jump·Eat Anim
        bool success = Managers.MinMon.TryCatch(_dataId, forceSuccess: true);
        Debug.Log($"[Tame] 만드라고라 사용 dataId={_dataId} → {(success ? "성공" : "실패")}");
        HandleResult(success);
    }

    // 슬36: 결과 처리. 성공 시 GlowRays 적용된 [나의 미니몬] 창 호출 (현재는 즉시 호출, 연출은 Phase 2).
    void HandleResult(bool success)
    {
        if (success)
        {
            // TODO 슬36 #1~4: Caught Anim → Emoj_TwoHearts → Emoj_Happy → GlowRay 후 [나의 미니몬] 호출
            ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(_dataId);
        }
        else
        {
            // TODO 슬36: Emoj_Sweat + 미니몬 좌우 반전 + Runaway Anim 후 Popup Close
            _busy = false;
            RefreshRateTexts(Managers.Data.MinMonDic.TryGetValue(_dataId, out var mm) ? mm : null);
        }
    }

    void OnClickMinimonInfo()
    {
        // 슬: 하단 [나의 미니몬] 버튼 → 나의 미니몬 창
        Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(_dataId);
    }

    void OnClickExit() => OnClickBG();

    // ─────────────────────────────────────────────
    // 유틸
    // ─────────────────────────────────────────────
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
