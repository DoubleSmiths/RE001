using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_ContinuePopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // CountdownValueText : 10초 부터 카운트 다운하고, 0초가 되면 UI_ContinuePopup을 닫고  UI_GameResultPopup 호출 (닷트윈으로 스케일 애니메이션 연출 생각중)

    // 버튼
    // ContinueButton : 열쇠(아직 없음)를 사용하여 캐릭터가 부활하며 팝업이 닫힘
    // ContinueCostValueText : 부활에 필요한 열쇠표시 ( 필요 / 보유)
    // ADContinueButton : 광고 후 부활하며 팝업이 닫힘

    // 로컬라이징 텍스트
    // ContinuePopupTitleText : CONTINUE
    // ContinueButtonText : 계속하기
    // ADContinueText : 계속하기

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject
    }
    enum Texts
    {
        ContinuePopupTitleText,
        CountdownValueText,
        ContinueButtonText,
        ContinueCostValueText,
        ADContinueText,
    }

    enum Buttons
    {
        CloseButton,
        ContinueButton,
        ADContinueButton,
    }
    #endregion

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.CloseButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnClickContinueButton);
        GetButton((int)Buttons.ContinueButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ADContinueButton).gameObject.BindEvent(OnClickADContinueButton);
        GetButton((int)Buttons.ADContinueButton).GetOrAddComponent<UI_ButtonAnimation>();
        Refresh();
         return true;
    }


    private void Start()
    {
        StartCoroutine(CountdownCoroutine());
    }

    public void SetInfo()
    {
        Refresh();
    }

    void Refresh()
    {
        if (Managers.Game.ItemDictionary.TryGetValue(Define.ID_BRONZE_KEY, out int keyCount) == true)
        {
            GetText((int)Texts.ContinueCostValueText).text = $"1/{keyCount}";
        }
        else
        {
            GetText((int)Texts.ContinueCostValueText).text = $"<color=red>0</color>";
        }

        // 리프레시 버그 대응
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetButton((int)Buttons.ADContinueButton).gameObject.GetComponent<RectTransform>());
    }

    void OnClickCloseButton() // 닫기 버튼
    {
        Managers.UI.ClosePopupUI(this);
        Managers.Game.GameOver();

    }

    IEnumerator CountdownCoroutine()
    {
        int count = 10;

        while (count>0)
        {
            yield return new WaitForSecondsRealtime(1f);
            count--;
            GetText((int)Texts.CountdownValueText).text = count.ToString(); 
            if (count == 0)
                break;
        }
        yield return new WaitForSecondsRealtime(1f);

        Managers.UI.ClosePopupUI(this);
        Managers.Game.GameOver();

    }

    void OnClickContinueButton() // 계속하기 버튼
    {
        Managers.Sound.PlayButtonClick();

        if (Managers.Game.ItemDictionary.TryGetValue(Define.ID_BRONZE_KEY, out int keyCount) == true)
        {
            Managers.Game.RemovMaterialItem(Define.ID_BRONZE_KEY, 1);
            Managers.Game.Player.Resurrection(1);
            Managers.UI.ClosePopupUI(this);
        }
    }
    void OnClickADContinueButton() // 광고 보고 계속하기 버튼
    {
        Managers.Sound.PlayButtonClick();

        Managers.Ads.ShowRewardedAd(() =>
        {
            Managers.Game.Player.Resurrection(1);
            Managers.UI.ClosePopupUI(this);
        });

    }


}
