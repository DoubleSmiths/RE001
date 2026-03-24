using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;
using Sequence = DG.Tweening.Sequence;

public abstract class UI_Base : MonoBehaviour
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();
    protected bool _init = false;

    // 폰트 등록 목록 (text, fontType)
    readonly List<(TMP_Text text, FontType fontType)> _fontEntries = new();

    public virtual bool Init()
    {
        if (_init)
            return false;

        _init = true;
        return true;
    }

    private void Start()
    {
        Init();
    }

    public virtual void OnDestroy()
    {
        if (Managers.Game != null)
            Managers.Game.OnLanguageChanged -= RefreshAllFonts;
    }

    // ────────────────────────────────────────
    //  폰트 관리
    // ────────────────────────────────────────

    /// <summary>
    /// 텍스트에 FontType을 지정한다.
    /// 첫 호출 시 즉시 폰트를 적용하고, 이후 언어 변경 시 자동으로 갱신된다.
    /// </summary>
    protected void ApplyFont(TMP_Text text, FontType fontType)
    {
        if (text == null) return;

        // 첫 등록 시 이벤트 구독
        if (_fontEntries.Count == 0)
            Managers.Game.OnLanguageChanged += RefreshAllFonts;

        _fontEntries.Add((text, fontType));
        SetFont(text, fontType);
    }

    void RefreshAllFonts()
    {
        foreach (var (text, type) in _fontEntries)
            if (text != null) SetFont(text, type);
    }

    void SetFont(TMP_Text text, FontType type)
    {
        if (Managers.Font == null) return;
        var font = Managers.Font.GetFont(type);
        if (font != null) text.font = font;
    }

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
    }

    protected void BindObject(Type type) { Bind<GameObject>(type); }
    protected void BindImage(Type type) { Bind<Image>(type); }
    protected void BindText(Type type) { Bind<TMP_Text>(type); }
    protected void BindButton(Type type) { Bind<Button>(type); }
    protected void BindToggle(Type type) { Bind<Toggle>(type); }


    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }
    protected Toggle GetToggle(int idx) { return Get<Toggle>(idx); }


    public static void BindEvent(GameObject go, Action action = null, Action<BaseEventData> dragAction = null, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.UIEvent.Pressed:
                evt.OnPressedHandler -= action;
                evt.OnPressedHandler += action;
                break;
            case Define.UIEvent.PointerDown:
                evt.OnPointerDownHandler -= action;
                evt.OnPointerDownHandler += action;
                break;
            case Define.UIEvent.PointerUp:
                evt.OnPointerUpHandler -= action;
                evt.OnPointerUpHandler += action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= dragAction;
                evt.OnDragHandler += dragAction;
                break;
            case Define.UIEvent.BeginDrag:
                evt.OnBeginDragHandler -= dragAction;
                evt.OnBeginDragHandler += dragAction;
                break;
            case Define.UIEvent.EndDrag:
                evt.OnEndDragHandler -= dragAction;
                evt.OnEndDragHandler += dragAction;
                break;
        }
    }

    public void PopupOpenAnimation(GameObject contentObject) // 팝업 오픈 연출
    {
        contentObject.transform.localScale = new Vector3(0.8f,0.8f,1);
        contentObject.transform.DOScale(1f, 0.1f).SetEase(Ease.InOutBack).SetUpdate(true);
    }

    public void FadeIn(Image image)
    {
        image.DOFade(0f, 1f);
    }

    public void FadeOut(Image image)
    {
        image.DOFade(1f, 1f);
    }
    
    public void FadeInOut(Image image)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(image.DOFade(1f, 1f));
        seq.Append(image.DOFade(0f, 1f));
    }

}
