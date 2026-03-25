using UnityEngine;

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

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetImage((int)Images.TextBoxDialogueFrame).gameObject.BindEvent(OnClickTextBoxDialogueFrame);

        return true;
    }

    public void SetInfo()
    {
        GetText((int)Texts.DialogueName).text = "TestName";
        GetText((int)Texts.DialogueText).text = "TestDialogue";

        GetImage((int)Images.CharacterImage).sprite = Managers.Resource.Load<Sprite>("receptionist_sigh");
    }

    void OnClickTextBoxDialogueFrame()
    {
        // TODO : 글자가 다안나오면 한번에 다 나오기 -> DialogueNextImage 켜기
        // 글자가 다 나오면 다음 글자 시작

    }

    ///
    /// Addressable에서 Sprites그룹에 속한 것들을 보고 그 이름의 마지막에 .sprite가 붙어있지 않으면 붙게해주는 스크립트를 생성해줘.
    ///

}
