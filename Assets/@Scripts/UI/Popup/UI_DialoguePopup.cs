using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using Data;

public class UI_DialoguePopup : UI_Popup
{
    #region Enum

    enum Images
    {
        Panel,
        CharacterImage,
        TextBoxDialogueFrame,
        DialogueNextImage,
    }

    enum Texts
    {
        DialogueText,
        DialogueName,
    }

    enum Buttons
    {
    }

    enum GameObjects
    {
        FIXED,
        In_Frame,
    }

    #endregion

    DialogueAnimator _animator;

    List<DialogueScriptLine> _lines;
    int _lineIndex;
    bool _isTypingDone;
    Action _onFinished;

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _animator = gameObject.GetOrAddComponent<DialogueAnimator>();

        GetImage((int)Images.DialogueNextImage).gameObject.SetActive(false);
        GetImage((int)Images.Panel).gameObject.BindEvent(OnClickTextBoxDialogueFrame);

        return true;
    }

    /// <summary>
    /// groupKey : DialogueScriptDic의 키 (예: "GuildQuest1Text")
    /// speakerName : 대화창 이름 표시
    /// portrait : 초상화 (없으면 null)
    /// onFinished : 마지막 대사 후 클릭 시 콜백
    /// </summary>
    public void SetInfo(string groupKey, string speakerName, Sprite portrait = null, Action onFinished = null)
    {
        if (!Managers.Data.DialogueScriptDic.TryGetValue(groupKey, out var group))
        {
            Debug.LogWarning($"[DialoguePopup] GroupKey 없음: {groupKey}");
            return;
        }

        _lines = group.Lines;
        _lineIndex = 0;
        _onFinished = onFinished;

        GetText((int)Texts.DialogueName).text = speakerName;
        GetImage((int)Images.DialogueNextImage).gameObject.SetActive(false);

        if (portrait != null)
        {
            var portraitImage = GetImage((int)Images.CharacterImage);
            portraitImage.sprite = portrait;
            _animator.EnterPortrait(portraitImage);
            _animator.SetPortraitSpeaking(portraitImage, true);
        }

        // For Test
        GetImage((int)Images.CharacterImage).gameObject.GetComponent<Animator>().Play("recep_normal");

        var inFrame = GetObject((int)GameObjects.In_Frame).GetComponent<RectTransform>();
        _animator.OpenBox(inFrame, () => ShowLine(_lineIndex));
    }

    void ShowLine(int index)
    {
        _isTypingDone = false;
        GetImage((int)Images.DialogueNextImage).gameObject.SetActive(false);

        string content = Managers.Game.Language == Language.Korean
            ? _lines[index].Kor
            : _lines[index].Eng;

        Debug.Log($"[Dialogue] ShowLine({index}/{_lines.Count - 1}): {content}");

        _animator.PlayTyping(
            GetText((int)Texts.DialogueText),
            content,
            DialogueEmotion.Normal,
            onComplete: () =>
            {
                _isTypingDone = true;
                bool isLast = _lineIndex >= _lines.Count - 1;
                GetImage((int)Images.DialogueNextImage).gameObject.SetActive(!isLast);
            }
        );
    }

    void OnClickTextBoxDialogueFrame()
    {
        Debug.Log($"[Dialogue] Click - isTypingDone:{_isTypingDone}, lineIndex:{_lineIndex}, total:{_lines?.Count}");

        if (!_isTypingDone)
        {
            _animator.SkipTyping(GetText((int)Texts.DialogueText));
            return;
        }

        _lineIndex++;

        if (_lineIndex < _lines.Count)
        {
            ShowLine(_lineIndex);
        }
        else
        {
            Managers.UI.ClosePopupUI();
            _onFinished?.Invoke();
        }
    }
}
