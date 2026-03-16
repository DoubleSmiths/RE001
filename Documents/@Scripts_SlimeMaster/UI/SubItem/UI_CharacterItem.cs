using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UI_CharacterItem : UI_Base
{
    #region UI 기능 리스트
    // 정보 갱신
    // SelectObject : 유저가 캐릭터를 눌렀을때 선택 상황을 알려주는 아웃라인
    // CharacterImage : 캐릭터 이미지
    // CharacterLevelValueText : 캐릭터 레벨
    // EquipedObject : 장착 캐릭터일때 활성화
    // LockObject : 소유하고 있다면 비활성화
    // EquipmentRedDotObject : 캐릭터가 강회(레벨업 or 업그레이드)가 가능하다면 활성화 (애니메이션 추가 필요) 

    // 로컬라이징
    // EquipedText
    #endregion

    #region Enums
    enum GameObjects
    {
        SelectObject,
        EquipedObject,
        LockObject,
        EquipmentRedDotObject,
    }

    enum Texts
    {
        CharacterLevelValueText,
        EquipedText,
    }

    enum Images
    {
        CharacterImage,
    }

    enum Buttons
    {
        UI_CharacterItem,
    }
    #endregion


    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetObject((int)GameObjects.SelectObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.LockObject).gameObject.SetActive(true);
        GetObject((int)GameObjects.EquipedObject).gameObject.SetActive(false); 
        GetObject((int)GameObjects.EquipmentRedDotObject).gameObject.SetActive(false); 

        GetButton((int)Buttons.UI_CharacterItem).gameObject.BindEvent(OnClickCharacterItem);

#if UNITY_EDITOR
        // 테스트용
        //TextBindTest();
#endif
        #endregion

        Refresh();
        return true;
    }

    public void SetInfo()
    {

        Refresh();
    }

    void Refresh()
    {


    }

    void OnClickCharacterItem() // 캐릭터 선택 버튼
    {
        Managers.Sound.PlayButtonClick();
        GetObject((int)GameObjects.SelectObject).gameObject.SetActive(true);
    }
}
