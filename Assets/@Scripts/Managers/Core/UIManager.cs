using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class UIManager
{
    int _order = 1000;        // 팝업 베이스 — 월드 전투 요소(데미지폰트 350 등) 위로
    int _toastOrder = 2000;    // 토스트는 팝업(1000+) 위

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    Stack<UI_Toast> _toastStack = new Stack<UI_Toast>();
    UI_Scene _sceneUI = null;
    public UI_Scene SceneUI { get { return _sceneUI; } }

    public event Action<int> OnTimeScaleChanged;

    // 전투를 읽고 조작하는 모달은 월드 시간만 멈춘다. 중첩 팝업은 최초 복원값을 공유한다.
    public bool IsBattlePaused { get; private set; }
    bool _hasTimeScaleOverride;
    bool _isClearing;
    float _resumeTimeScale = 1f;

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };
            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true, int sortOrder = 0, bool isToast = false)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        // 팝업 캔버스는 프리팹 설정(ScreenSpaceCamera + 수동 스케일) 유지 — Overlay로 강제하면 레이아웃이 깨짐.
        // 월드 전투 요소(데미지폰트 350 등)가 팝업 위로 렌더되던 문제는 아래 sortingOrder 베이스를 높여 해결.

        CanvasScaler cs = go.GetOrAddComponent<CanvasScaler>();
        if (cs != null)
        {
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(180, 440);
            // 세로 화면 게임이라 **높이 기준**으로 맞춘다. 기본값(0=너비 기준)이면
            // 기준 해상도보다 가로가 넓게 저작된 팝업(예: 장비 208x440)이 화면에 안 들어맞아
            // 일부만 보이거나 HUD에 가려진다.
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 1f;
        }

        go.GetOrAddComponent<GraphicRaycaster>();

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = sortOrder;
        }

        if (isToast)
        {
            _toastOrder++;
            canvas.sortingOrder = _toastOrder;
        }

    }

    public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"{name}");
        if (parent != null)
            go.transform.SetParent(parent);

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        return Util.GetOrAddComponent<T>(go);
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null, bool pooling = true) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"{name}", parent, pooling);
        go.transform.SetParent(parent, false);
        return Util.GetOrAddComponent<T>(go);
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

        go.transform.SetParent(Root.transform);

        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"{name}");
        T popup = Util.GetOrAddComponent<T>(go);
        ConfigurePopupAnimations(go);
        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform);

        RefreshTimeScale();

        return popup;
    }

    static void ConfigurePopupAnimations(GameObject root)
    {
        // Awake에서 이미 만든 Tween과, 비활성 자식이 나중에 생성할 Tween을 모두 처리한다.
        // autoGenerate는 보존한다. 도감 슬롯의 사용하지 않는 저작 Tween을 되살리지 않는다.
        foreach (DOTweenAnimation animation in root.GetComponentsInChildren<DOTweenAnimation>(true))
        {
            animation.isIndependentUpdate = true;
            if (animation.tween != null && animation.tween.IsActive())
                animation.tween.SetUpdate(true);
        }
        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }
        //Managers.Sound.PlayPopupClose();   // 팝업 닫기 사운드 — 이상하다는 피드백으로 일단 비활성
        ClosePopupUI();
    }

    public event System.Action<System.Type> OnPopupClosed;

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        System.Type closedType = popup.GetType();
        popup.StopAnimations();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
        RefreshTimeScale();
        OnPopupClosed?.Invoke(closedType);
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    // [미니몬 관심] Side UI 알림 — 팝업 스택/timeScale 미사용(게임 비정지). 자체 코루틴으로 자동 소멸.
    public UI_AttactedPopup ShowAttractedPopup(int dataId, bool storageFull)
    {
        GameObject go = Managers.Resource.Instantiate(nameof(UI_AttactedPopup), pooling: false);
        if (go == null) return null;
        var popup = Util.GetOrAddComponent<UI_AttactedPopup>(go);
        ConfigurePopupAnimations(go);
        go.transform.SetParent(Root.transform, false);   // worldPositionStays=false: 프리펩 anchoredPos 보존(슬24 우측 안착)
        popup.SetInfo(dataId, storageFull);
        return popup;
    }

    // [포획 창고 가득] 경고 — 팝업 스택/timeScale 미사용(게임 비정지). 자체 코루틴으로 자동 소멸.
    public UI_WarningPopup ShowWarningPopup(int dataId)
    {
        GameObject go = Managers.Resource.Instantiate(nameof(UI_WarningPopup), pooling: false);
        if (go == null) return null;
        var popup = Util.GetOrAddComponent<UI_WarningPopup>(go);
        ConfigurePopupAnimations(go);
        go.transform.SetParent(Root.transform, false);   // worldPositionStays=false: 프리펩 위치 보존
        popup.SetInfo(dataId);
        return popup;
    }

    public UI_Toast ShowToast(string msg)
    {
        string name = typeof(UI_Toast).Name;
        GameObject go = Managers.Resource.Instantiate($"{name}", pooling: false);
        UI_Toast popup = Util.GetOrAddComponent<UI_Toast>(go);
        ConfigurePopupAnimations(go);
        // Screen Space - Camera 인데 카메라가 비어 있으면 Overlay 처럼 화면 전체(레터박스 여백 포함)에 그려진다.
        var canvas = go.GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null) canvas.worldCamera = Camera.main;
        popup.SetInfo(msg);
        _toastStack.Push(popup);
        go.transform.SetParent(Root.transform);
        CoroutineManager.StartCoroutine(CoCloseToastUI());
        return popup;
    }

    IEnumerator CoCloseToastUI()
    {
       yield return new WaitForSecondsRealtime(3f);
       CloseToastUI();
    }

    public void CloseToastUI()
    {
        if (_toastStack.Count == 0)
            return;

        UI_Toast toast = _toastStack.Pop();
        Managers.Resource.Destroy(toast.gameObject);
        toast = null;
        _toastOrder--;
    }

    public int GetPopupCount()
    {
        return _popupStack.Count;
    }

    #region Back (안드로이드 뒤로가기 / PC ESC)
    // 순서: 맨 위 팝업 닫기 → 건물 나가기·전투 귀환(화면의 그 버튼을 누른 것과 동일) →
    //  최상위 화면에서는 안드로이드는 "한 번 더 누르면 종료", PC 는 설정창(종료 버튼 포함).
    const float BACK_QUIT_WINDOW = 2f;
    float _lastRootBackTime = -10f;

    public void OnBackPressed()
    {
        if (_popupStack.Count > 0)
        {
            _popupStack.Peek().OnBackPressed();
            return;
        }

        if (_sceneUI != null && ClickSceneBackButton())
            return;

        if (Application.platform == RuntimePlatform.Android)
        {
            if (Time.unscaledTime - _lastRootBackTime <= BACK_QUIT_WINDOW)
            {
                Managers.Game?.FlushSave();
                Application.Quit();
                return;
            }
            _lastRootBackTime = Time.unscaledTime;
            ShowToast(Util.Localize("BackAgainToQuitText"));
            return;
        }

        ShowPopupUI<UI_SettingPopup>();
    }

    bool ClickSceneBackButton()
    {
        UI_EventHandler exit = null, ret = null;
        foreach (var h in _sceneUI.GetComponentsInChildren<UI_EventHandler>(false))
        {
            if (h.OnClickHandler == null) continue;
            if (exit == null && h.name.EndsWith("ExitButton")) exit = h;       // 건물 안: 나가기
            else if (ret == null && h.name == "ReturnButton") ret = h;         // 전투: 귀환(2단계 확인 내장)
        }
        var target = exit != null ? exit : ret;
        if (target == null) return false;
        target.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        return true;
    }
    #endregion

    public void Clear()
    {
        _isClearing = true;
        try
        {
            CloseAllPopupUI();
        }
        finally
        {
            // 씬 종료는 이전 씬의 정지 사유와 복원값을 다음 씬으로 넘기지 않는다.
            _isClearing = false;
            IsBattlePaused = false;
            _isActiveSoulShop = false;
            _hasTimeScaleOverride = false;
            _resumeTimeScale = 1f;
            _sceneUI = null;
            ApplyTimeScale(1f);
        }
    }

    public void RefreshTimeScale()
    {
        // 이전 전체 팝업 정지 실험 코드는 참고용으로 보존한다. 현재 정책은 아래 스택 판정이 담당한다.
        //if (SceneManager.GetActiveScene().name != Define.Scene.GameScene.ToString())
        //{
        //    Time.timeScale = 1;
        //    return;
        //}
        //if (_popupStack.Count > 0 || IsActiveSoulShop == true)
        //    Time.timeScale = 0;
        //else
        //    Time.timeScale = 1;
        //DOTween.timeScale = 1;
        //OnTimeScaleChanged?.Invoke((int)Time.timeScale);

        if (_isClearing) return;
        var player = Managers.Game?.Player;
        bool hasBattleModal = false;
        foreach (var popup in _popupStack)
        {
            if (popup == null) continue;
            if (popup is UI_BestiaryPopup || popup is UI_MonsterDetailPopup
                || popup is UI_TamePopup || popup is UI_MinionInfoPopup)
            {
                hasBattleModal = true;
                break;
            }
        }
        IsBattlePaused = player != null && player.InBattle && (hasBattleModal || _isActiveSoulShop);
        bool shouldPause = IsBattlePaused || _isActiveSoulShop;
        if (shouldPause)
        {
            if (!_hasTimeScaleOverride)
            {
                _resumeTimeScale = Time.timeScale;
                _hasTimeScaleOverride = true;
            }
            ApplyTimeScale(0f);
        }
        else if (_hasTimeScaleOverride)
        {
            float resume = _resumeTimeScale;
            _hasTimeScaleOverride = false;
            _resumeTimeScale = 1f;
            ApplyTimeScale(resume);
        }
    }

    void ApplyTimeScale(float scale)
    {
        if (Time.timeScale == scale) return;
        Time.timeScale = scale;
        // 기존 이벤트 형식은 유지하되 실제 배율은 float 그대로 복원한다. DOTween 전역 배율은 소유하지 않는다.
        OnTimeScaleChanged?.Invoke((int)scale);
    }

    #region 임시
    bool _isActiveSoulShop = false;
    public bool IsActiveSoulShop 
    {
        get { return _isActiveSoulShop; }
        set
        {
            _isActiveSoulShop = value;
            RefreshTimeScale();
        }
    }

    #endregion
}
