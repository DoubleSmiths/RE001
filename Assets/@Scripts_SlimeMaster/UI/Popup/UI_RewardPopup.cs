
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//UI Popup을 왜 굳이 atctive true false 하는지?물어보기
public class UI_RewardPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // RewardItemScrollContentObject : 리워드 아이템 들어갈 부모개체

    // 호출되는 곳
    // 미션 팝업 : 미션 완료 보상
    // 오프라인 보상 팝업 : 보상 수령 시
    // 빠른 보상 팝업 : 보상 수령 시
    // 스테이지 보상 팝업 : 클리어 보상 수령 시
    // 상점 페이지 : 상품 구매
    // 장비 초기화 팝업 : 초기화 결과 수령 시

    // 호출 예정
    // 배틀 패스 팝업 : 패스 보상 
    // 월정액 팝업 : 즉시 보상
    // 다이아 패스 팝업 : 즉시 보상
    // 첫 결제 보상 팝업 : 즉시 보상
    // 일일 특가 팝업 : 상품 구매


    // 로컬라이징 텍스트
    // RewardPopupTitleText : 보상
    // BackgroundText : 탭하여 닫기

    #endregion


    #region Enum
    enum GameObjects
    {
        RewardItemScrollContentObject, 
    }

    enum Buttons
    {
        BackgroundButton,
    }

    enum Texts
    {
        RewardPopupTitleText,
        BackgroundText
    }


    #endregion

    public Action OnClosed;
    string[] _spriteName;
    int[] _count;

    public void OnEnable()
    {
        //TODO 팝업 스택이 꼬이는 경우가 발생 임시로 sortingOrder조절 
        //Canvas canvas = GetComponent<Canvas>();
        //canvas.sortingOrder = 300;
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);
        #endregion
        Refresh();
        Managers.Sound.Play(Define.Sound.Effect, "PopupOpen_Reward");

        return true;
    }

    public void SetInfo(string[] spriteName, int[] count, Action callback = null)
    {
        _spriteName = spriteName;
        _count = count;
        OnClosed = callback;
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;

        GetObject((int)GameObjects.RewardItemScrollContentObject).DestroyChilds();
        for (int i = 0; i < _spriteName.Length; i++)
        {
            Debug.Log(_spriteName[i]);
            UI_MaterialItem item = Managers.UI.MakeSubItem<UI_MaterialItem>(GetObject((int)GameObjects.RewardItemScrollContentObject).transform);
            item.SetInfo(_spriteName[i], _count[i]);
        }
    }

    void OnClickBackgroundButton() // 화면 터치하여 닫기
    {
        Managers.Sound.PlayPopupClose();
        gameObject.SetActive(false);
        OnClosed?.Invoke();
    }

}
