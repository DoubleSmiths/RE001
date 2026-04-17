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
        GradientImage,
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

        // 초기 상태: 페이드 시퀀스가 시작되기 전까지 비표시
        SetAlpha(GetImage((int)Images.GradientImage), 0f);
        SetAlpha(GetImage((int)Images.Panel), 0f);
        SetAlpha(GetImage((int)Images.TextBoxDialogueFrame), 0f);
        GetImage((int)Images.CharacterImage).gameObject.SetActive(false);
        GetImage((int)Images.DialogueNextImage).gameObject.SetActive(false);
        GetText((int)Texts.DialogueName).gameObject.SetActive(false);

        GetImage((int)Images.Panel).gameObject.BindEvent(OnClickTextBoxDialogueFrame);

        return true;
    }

    static void SetAlpha(UnityEngine.UI.Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    /// <summary>
    /// groupKey : DialogueScriptDic의 키 (예: "GuildQuest1Text")
    /// speakerName : 대화창 이름 표시
    /// portrait : 초상화 (없으면 null)
    /// onFinished : 마지막 대사 후 클릭 시 콜백
    /// randomSingleLine : true면 그룹 내 한 줄만 랜덤 선택 (슬라이드 30: GuildNormalText 용)
    /// </summary>
    public void SetInfo(string groupKey, string speakerName, Sprite portrait = null, Action onFinished = null, bool randomSingleLine = false)
    {
        if (!Managers.Data.DialogueScriptDic.TryGetValue(groupKey, out var group))
        {
            Debug.LogWarning($"[DialoguePopup] GroupKey 없음: {groupKey}");
            return;
        }

        if (randomSingleLine && group.Lines != null && group.Lines.Count > 0)
        {
            var picked = group.Lines[UnityEngine.Random.Range(0, group.Lines.Count)];
            _lines = new List<DialogueScriptLine> { picked };
        }
        else
        {
            _lines = group.Lines;
        }
        _lineIndex = 0;
        _onFinished = onFinished;

        var gradientImage = GetImage((int)Images.GradientImage);
        var panelImage = GetImage((int)Images.Panel);
        var boxImage = GetImage((int)Images.TextBoxDialogueFrame);
        var portraitImage = GetImage((int)Images.CharacterImage);
        var arrowImage = GetImage((int)Images.DialogueNextImage);
        var dialogueName = GetText((int)Texts.DialogueName);
        var dialogueText = GetText((int)Texts.DialogueText);

        dialogueName.text = speakerName;
        dialogueText.text = "";

        if (portrait != null)
            portraitImage.sprite = portrait;

        // 초기 상태: Arrow 끔, 텍스트 공백으로 시작 (STEP 3에서 Text Show)
        arrowImage.gameObject.SetActive(false);
        _animator.StopArrowLoop(arrowImage);

        _animator.PlayOpenSequence(
            gradient: gradientImage,
            panel: panelImage,
            box: boxImage,
            portrait: portraitImage.gameObject,
            nameText: dialogueName.gameObject,
            onTextShowStart: () =>
            {
                // STEP 3: 캐릭터 애니메이터 재생 + Text Show
                var animator = portraitImage.GetComponent<Animator>();
                if (animator != null) animator.Play("recep_normal");
                ShowLine(_lineIndex);
            });
    }

    void ShowLine(int index)
    {
        _isTypingDone = false;
        var arrowImage = GetImage((int)Images.DialogueNextImage);
        arrowImage.gameObject.SetActive(false);
        _animator.StopArrowLoop(arrowImage);

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
                // STEP 4: Text Show 끝나면 Arrow_down 지속 페이드 루프 시작
                Debug.Log($"[Dialogue] Typing complete → PlayArrowLoop. arrow null? {arrowImage == null}");
                _animator.PlayArrowLoop(arrowImage);
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
