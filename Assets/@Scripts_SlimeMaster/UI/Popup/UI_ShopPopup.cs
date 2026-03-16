using Data;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;
using static UnityEngine.ParticleSystem;

public class UI_ShopPopup : UI_Popup
{
    #region UI 기능 리스트
    // 정보 갱신
    // Package
    // StagePackage (패키지 내 이전, 다음 버튼을 누를때 마다 컨탠츠들이 변경되고, 패키지 버튼을 누르면 해당 컨텐츠에 해당하는 상품을 구매하게되야함.
    //      - StagePackageItemTitleText : 패키지 이름 ( "n 스테이지 패키지" )
    //      - StagePackageContentObject : 패키지 구성을 보여줄 UI_StagePackageItem이 들어갈 부모계체 
    //      - PackageFirstProductItemCountValueText : 첫번째 상품 개수
    //      - PackageSecondProductItemCountValueText : 두번째 상품 개수
    //      - PackageThirdProductItemCountValueText : 세번째 상품 개수
    //      - PackageFourthProductItemCountValueText : 네번째 상품 개수
    //      - StagePackageCostValueText : 패키지 가격

    // DalyShop
    // RefreshTimerValueText : 데일리 성점의 리스트 갱신까지 남은 시간
    // 갱신되는 컨텐츠들을 어떻게 구성해야하는지 잘 모르겠음. (바인딩 못함)

    // Gacha
    //  PickupGachaGroup
    //      - PickupGuaranteedCountSliderObject : 천장 도달 상황을 슬라이더로 표시
    //      - BuyLimitValueText : 픽업 남은시간 "hh : mm : ss" 으로 표기
    //      - PickupGachaGuaranteedCountValueText : 천장 도달까지 남은 횟수
    //  CommonGacha
    //      - CommonGachaCostValueText : 일반 가챠 키 표시 ( 필요 / 보유 ) 
    //  AdvancedGacha
    //      - AdvancedGuaranteedCountSliderObject : 천장 도달 상황을 슬라이더로 표시
    //      - AdvancedGachaGuaranteedCountValueText :천장 도달까지 남은 횟수
    //      - AdvancedGachaCostValueText : 고급 가챠 키 표시 ( 필요 / 보유 ) 

    // DiaShop
    //  FreeDia
    //      - FreeDiaRequestTimeValueText : 무료 보상까지 남은 시간 표시 ( hh : mm : ss )
    //      - FreeDiaRedDotObject : 무료 보상을 받을 수 있을때 활성화 ( FreeDiaSoldOutObject은 비활성화)
    //  First
    //      - FirstDiaCostValueText : 첫번째 다이아 구매 가격 
    //      - FirstDiaProductBonusObject : 첫구매 이후 비활성화
    //  Second
    //      - SecondDiaCostValueText : 두번째 다이아 구매 가격 
    //      - SecondDiaProductBonusObject : 첫구매 이후 비활성화
    //  Third
    //      - ThirdDiaCostValueText : 세번째 다이아 구매 가격 
    //      - ThirdDiaProductBonusObject : 첫구매 이후 비활성화
    //  Fourth
    //      - FourthDiaCostValueText : 네번째 다이아 구매 가격 
    //      - FourthDiaProductBonusObject : 첫구매 이후 비활성화
    //  Fifth
    //      - FifthDiaCostValueText : 다섯번째 다이아 구매 가격 
    //      - FifthDiaProductBonusObject : 첫구매 이후 비활성화

    // KeyShop
    //      - AdKeyRequestTimeValueText : 광고 보상까지 남은 시간 표시 ( hh : mm : ss )
    //      - AdKeyRedDotObject : 광고 보상을 받을 수 있을때 활성화 ( FreeDiaSoldOutObject은 비활성화)

    // GoldShop
    //      - FreeGoldRequestTimeValueText : 무료 보상까지 남은 시간 표시 ( hh : mm : ss )
    //      - FreeGoldRedDotObject : 무료 보상을 받을 수 있을때 활성화 ( FreeDiaSoldOutObject은 비활성화)



    // 로컬라이징
    // PackageTitleText : 패키지 상점
    // DailyShopTitleText : 일일 상점
    // RefreshTimerText : 리셋 남은 시간
    // GachaTitleText : 가챠 상점
    // DiaShopTitleText : 다이아 상점
    // GoldShopTitleText : 골드 상점
    // DiaShopCommentText : 첫 구매 시 2배, 1회 한정

    // StagePackageTitleText : 챕터 패키지
    // BeginnerPackageTitleText : 비기너 패키지

    // BeginnerPackageBuyLimitText : 1회 한정
    // BeginnerPackageDiscountText : 가치
    // PickupGachaCommentText : 픽업가챠 코멘트
    // PickupGachaTitleText : 픽업 가챠 타이틀 (출시 상품마다 이름이 다름 주의)
    // CommonGachaTitleText : 일반 가챠
    // AdvancedGachaTitleText : 고급 가챠

    // BuyLimitText : 제한 시간
    // OpenOneButtonText : 1회 열기
    // OpenTenButtonText : 10회 열기
    // PickupGachaGuaranteedCountText : 픽업 보장 : 
    // AdvancedGachaGuaranteedCountText : 보장 :
    // CommonGachaOpenButtonText : 열기
    // AdvancedGachaOpenButtonText : 열기

    // KeyShopTitleText : 열쇠 상점

    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        ShopScrollContent,

        //StagePackageContentObject,

        //PickupGuaranteedCountSliderObject,
        //AdvancedGuaranteedCountSliderObject,

        //FreeDiaSoldOutObject,
        //FreeDiaRedDotObject,

        FreeGoldSoldOutObject,
        FreeGoldRedDotObject,

        //FirstDiaProductBonusObject,
        //SecondDiaProductBonusObject,
        //ThirdDiaProductBonusObject,
        //FourthDiaProductBonusObject,
        //FifthDiaProductBonusObject,

        //OpenTenCostObject,
        //OpenOneCostObject,
        CommonGachaCostObject,
        AdvancedGachaCostObject,
        //StagePackageCostObject,

        AdKeySoldOutObject,
        AdKeyRedDotObject,

        // 출시모델 제외
        //DailyShopTitle,
        //DailyShopGroup,
        //PickupGachaGroup,

    }

    enum Buttons
    {
        //StagePackageButton,
        //StagePackagePrevButton,
        //StagePackageNextButton,
        //BeginnerPackageButton,

        //PickupGachaInfoButton,
        //PickupGachaListButton,
        //OpenOneButton,
        //OpenTenButton,
        CommonGachaOpenButton,
        ADCommonGachaOpenButton,
        ADAdvancedGachaOpenButton,
        CommonGachaListButton,
        AdvancedGachaOpenButton,
        AdvancedGachaOpenTenButton,
        AdvancedGachaListButton,
        //FreeDiaButton,
        //FirstDiaProductButton,
        //SecondDiaProductButton,
        //ThirdDiaProductButton,
        //FourthDiaProductButton,
        //FifthDiaProductButton,

        AdKeyButton,
        SilverKeyProductButton,
        GoldKeyProductButton,

        FreeGoldButton,
        FirstGoldProductButton,
        SecondGoldProductButton,
    }

    enum Texts
    {
        //PackageTitleText,
        //DailyShopTitleText,
        //RefreshTimerText,
        //RefreshTimerValueText,
        GachaTitleText,
        //DiaShopTitleText,
        //DiaShopCommentText,
        KeyShopTitleText,
        GoldShopTitleText,

        //StagePackageTitleText,
        //StagePackageItemTitleText,
        //PackageFirstProductItemCountValueText,
        //PackageSecondProductItemCountValueText,
        //PackageThirdProductItemCountValueText,
        //PackageFourthProductItemCountValueText,
        //StagePackageCostValueText,

        //BeginnerPackageTitleText,
        //BeginnerPackageBuyLimitText,
        //BeginnerPackageDiscountText,

        //PickupGachaTitleText,
        //PickupGachaCommentText,
        //PickupGachaGuaranteedCountText,
        //PickupGachaGuaranteedCountValueText,

        CommonGachaTitleText,
        CommonGachaOpenButtonText,
        CommonGachaCostValueText,

        AdvancedGachaTitleText,
        //AdvancedGachaGuaranteedCountText,
        //AdvancedGachaGuaranteedCountValueText,
        AdvancedGachaOpenButtonText,
        AdvancedGachaCostValueText,
        AdvancedGachaTenCostValueText,
        //BuyLimitText,
        //BuyLimitValueText,
        //OpenOneButtonText,
        //OpenTenButtonText,

        //FreeDiaRequestTimeValueText,
        //FirstDiaCostValueText,
        //SecondDiaCostValueText, 
        //ThirdDiaCostValueText, 
        //FourthDiaCostValueText, 
        //FifthDiaCostValueText,

        FreeGoldRequestTimeValueText,
        AdKeyRequestTimeValueText,

        // 골드 상점
        FirstGoldProductTitleText,
        FreeGoldTitleText,
        SecondGoldProductTitleText
    }

    enum Images
    {
        //BeginnerPackageFirstlItemImage,
    }
    #endregion

    ScrollRect _scrollRect;

    UI_GachaListPopup _gachaListPopupUI;
    public UI_GachaListPopup GachaListPopupUI
    {
        get
        {
            if(_gachaListPopupUI == null)
            {
                _gachaListPopupUI = Managers.UI.ShowPopupUI<UI_GachaListPopup>();
            }
            return _gachaListPopupUI;
        }
    }
    

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
        Refresh();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Managers.Game.Dia += 300;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD_KEY], 10);
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_BRONZE_KEY], 10);
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_SILVER_KEY], 10);
            Managers.Game.AddMaterialItem(Define.ID_WEAPON_SCROLL, 10);
            Managers.Game.AddMaterialItem(Define.ID_GLOVES_SCROLL, 10);
            Managers.Game.AddMaterialItem(Define.ID_RING_SCROLL, 10);
            Managers.Game.AddMaterialItem(Define.ID_BELT_SCROLL, 10);
            Managers.Game.AddMaterialItem(Define.ID_ARMOR_SCROLL, 10);
            Managers.Game.AddMaterialItem(Define.ID_BOOTS_SCROLL, 10);

            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_STAMINA], 10);
            Refresh();
        }
    }
#endif
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        // 버튼 스크롤 대응 (드래그 안되는 버튼에 하나씩 추가 필요)
        gameObject.BindEvent(null, OnDrag, Define.UIEvent.Drag);
        gameObject.BindEvent(null, OnBeginDrag, Define.UIEvent.BeginDrag);
        gameObject.BindEvent(null, OnEndDrag, Define.UIEvent.EndDrag);


        // 패키지 상점
        //GetButton((int)Buttons.StagePackageButton).gameObject.BindEvent(OnClickStagePackageButton);
        //GetButton((int)Buttons.StagePackageButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.StagePackagePrevButton).gameObject.BindEvent(OnClickStagePackagePrevButton);
        //GetButton((int)Buttons.StagePackagePrevButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.StagePackageNextButton).gameObject.BindEvent(OnClickStagePackageNextButton);
        //GetButton((int)Buttons.StagePackageNextButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.BeginnerPackageButton).gameObject.BindEvent(OnClickBeginnerPackageButton);
        //GetButton((int)Buttons.BeginnerPackageButton).GetOrAddComponent<UI_ButtonAnimation>();

        // 가챠 상점
        //GetButton((int)Buttons.PickupGachaInfoButton).gameObject.BindEvent(OnClickPickupGachaInfoButton);
        //GetButton((int)Buttons.PickupGachaInfoButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.PickupGachaListButton).gameObject.BindEvent(OnClickGachaListButton);
        //GetButton((int)Buttons.PickupGachaListButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.OpenOneButton).gameObject.BindEvent(OnClickOpenOneButton);
        //GetButton((int)Buttons.OpenOneButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.OpenTenButton).gameObject.BindEvent(OnClickOpenTenButton);
        //GetButton((int)Buttons.OpenTenButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.CommonGachaOpenButton).gameObject.BindEvent(OnClickCommonGachaOpenButton);
        GetButton((int)Buttons.CommonGachaOpenButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ADCommonGachaOpenButton).gameObject.BindEvent(OnClickADCommonGachaOpenButton);
        GetButton((int)Buttons.ADCommonGachaOpenButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ADAdvancedGachaOpenButton).gameObject.BindEvent(OnClickADAdvancedGachaOpenButton);
        GetButton((int)Buttons.ADAdvancedGachaOpenButton).GetOrAddComponent<UI_ButtonAnimation>();

        GetButton((int)Buttons.CommonGachaListButton).gameObject.BindEvent(OnClickCommonGachaListButton);
        GetButton((int)Buttons.CommonGachaListButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.AdvancedGachaOpenButton).gameObject.BindEvent(OnClickAdvancedGachaOpenButton);
        GetButton((int)Buttons.AdvancedGachaOpenButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.AdvancedGachaOpenTenButton).gameObject.BindEvent(OnClickAdvancedGachaOpenTenButton);
        GetButton((int)Buttons.AdvancedGachaOpenTenButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.AdvancedGachaListButton).gameObject.BindEvent(OnClickAdvancedGachaListButton);
        GetButton((int)Buttons.AdvancedGachaListButton).GetOrAddComponent<UI_ButtonAnimation>();

        // 출시모델 제외
        
        //GetObject((int)GameObjects.DailyShopTitle).gameObject.SetActive(false);
        //GetObject((int)GameObjects.DailyShopGroup).gameObject.SetActive(false);
        //GetObject((int)GameObjects.PickupGachaGroup).gameObject.SetActive(false);
        //GetObject((int)GameObjects.AdvancedGuaranteedCountSliderObject).gameObject.SetActive(false);


        // 다이아 상점
        //GetObject((int)GameObjects.FreeDiaSoldOutObject).gameObject.SetActive(false);
        //GetObject((int)GameObjects.FreeDiaRedDotObject).gameObject.SetActive(false);
        //GetButton((int)Buttons.FreeDiaButton).gameObject.BindEvent(OnClickFreeDiaButton);
        //GetButton((int)Buttons.FreeDiaButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.FirstDiaProductButton).gameObject.BindEvent(OnClickFirstDiaProductButton);
        //GetButton((int)Buttons.FirstDiaProductButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.SecondDiaProductButton).gameObject.BindEvent(OnClickSecondDiaProductButton);
        //GetButton((int)Buttons.SecondDiaProductButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.ThirdDiaProductButton).gameObject.BindEvent(OnClickThirdDiaProductButton);
        //GetButton((int)Buttons.ThirdDiaProductButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.FourthDiaProductButton).gameObject.BindEvent(OnClickFourthDiaProductButton);
        //GetButton((int)Buttons.FourthDiaProductButton).GetOrAddComponent<UI_ButtonAnimation>();
        //GetButton((int)Buttons.FifthDiaProductButton).gameObject.BindEvent(OnClickFifthDiaProductButton);
        //GetButton((int)Buttons.FifthDiaProductButton).GetOrAddComponent<UI_ButtonAnimation>();

        // 첫구매 보너스 처리
        //GetObject((int)GameObjects.FirstDiaProductBonusObject).gameObject.SetActive(false); // 첫구매 이력이 있다면 비활성화
        //GetObject((int)GameObjects.SecondDiaProductBonusObject).gameObject.SetActive(false); // 첫구매 이력이 있다면 비활성화
        //GetObject((int)GameObjects.ThirdDiaProductBonusObject).gameObject.SetActive(false); // 첫구매 이력이 있다면 비활성화
        //GetObject((int)GameObjects.FourthDiaProductBonusObject).gameObject.SetActive(false); // 첫구매 이력이 있다면 비활성화
        //GetObject((int)GameObjects.FifthDiaProductBonusObject).gameObject.SetActive(false); // 첫구매 이력이 있다면 비활성화

        // 열쇠 상점
        GetObject((int)GameObjects.AdKeySoldOutObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.AdKeyRedDotObject).gameObject.SetActive(false);
        GetButton((int)Buttons.AdKeyButton).gameObject.BindEvent(OnClickAdKeyButton);
        GetButton((int)Buttons.AdKeyButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SilverKeyProductButton).gameObject.BindEvent(OnClickSilverKeyProductButton);
        GetButton((int)Buttons.SilverKeyProductButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.GoldKeyProductButton).gameObject.BindEvent(OnClickGoldKeyProductButton);
        GetButton((int)Buttons.GoldKeyProductButton).GetOrAddComponent<UI_ButtonAnimation>();

        // 골드 상점
        GetObject((int)GameObjects.FreeGoldSoldOutObject).gameObject.SetActive(false);
        GetObject((int)GameObjects.FreeGoldRedDotObject).gameObject.SetActive(false);
        GetButton((int)Buttons.FreeGoldButton).gameObject.BindEvent(OnClickFreeGoldButton);
        GetButton((int)Buttons.FreeGoldButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.FirstGoldProductButton).gameObject.BindEvent(OnClickFirstGoldProductButton);
        GetButton((int)Buttons.FirstGoldProductButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SecondGoldProductButton).gameObject.BindEvent(OnClickSecondGoldProductButton);
        GetButton((int)Buttons.SecondGoldProductButton).GetOrAddComponent<UI_ButtonAnimation>();

        // 테스트용
#if UNITY_EDITOR

        //TextBindTest();
#endif
        #endregion

        Refresh();
        return true;
    }

    public void SetInfo(ScrollRect scrollRect)
    {
        _scrollRect = scrollRect;
        Refresh();
    }

    void Refresh()
    {

        Managers.Game.ItemDictionary.TryGetValue(Define.ID_GOLD_KEY, out int goldKeyCount);
        Managers.Game.ItemDictionary.TryGetValue(Define.ID_SILVER_KEY, out int silverKeyCount);
        Managers.Game.ItemDictionary.TryGetValue(Define.ID_BRONZE_KEY, out int bronzeKeyCount);
        
        GetText((int)Texts.CommonGachaCostValueText).text = $"{silverKeyCount}/1";
        GetText((int)Texts.AdvancedGachaCostValueText).text = $"{goldKeyCount}/1";
        GetText((int)Texts.AdvancedGachaTenCostValueText).text = $"{goldKeyCount}/10";
        GetButton((int)Buttons.ADAdvancedGachaOpenButton).gameObject.SetActive(Managers.Game.GachaCountAdsAnvanced > 0);
        GetButton((int)Buttons.ADCommonGachaOpenButton).gameObject.SetActive(Managers.Game.GachaCountAdsCommon > 0);
        GetObject((int)GameObjects.FreeGoldSoldOutObject).SetActive(Managers.Game.GoldCountAds == 0); // 솔드아웃 표시
        GetObject((int)GameObjects.AdKeySoldOutObject).SetActive(Managers.Game.BronzeKeyCountAds == 0);

        int goldAmount = 0;
        if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Game.GetMaxStageIndex(), out OfflineRewardData offlineReward))
        {
            goldAmount = offlineReward.Reward_Gold;
        }

        GetText((int)Texts.FreeGoldTitleText).text = $"{goldAmount}";
        GetText((int)Texts.FirstGoldProductTitleText).text = $"{goldAmount * 3}";
        GetText((int)Texts.SecondGoldProductTitleText).text = $"{goldAmount * 5}";

        #region 리프레스 버그 대응
        // 리프레시 버그 대응
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.StagePackageCostObject).GetComponent<RectTransform>());
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.OpenTenCostObject).GetComponent<RectTransform>());
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.OpenOneCostObject).GetComponent<RectTransform>());
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetButton((int)Buttons.OpenTenButton).gameObject.GetComponent<RectTransform>());
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetButton((int)Buttons.OpenOneButton).gameObject.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.CommonGachaCostObject).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetObject((int)GameObjects.AdvancedGachaCostObject).GetComponent<RectTransform>());
        #endregion
    }


    #region Buttons

    #region 패키지 버튼 (보류)
    //void OnClickStagePackageButton() // 챕터 패키지
    //{
    //    // 상품 구매 모듈 호출
    //}
    //void OnClickStagePackagePrevButton() // 이전 챕터 버튼
    //{
    //    // 패키지의 구성을 이전 패키지 구성으로 변경
    //}
    //void OnClickStagePackageNextButton() // 다음 챕터 버튼
    //{
    //    // 패키지의 구성을 다음 패키지 구성으로 변경
    //}
    //void OnClickBeginnerPackageButton() // 비기너 패키지
    //{
    //    // 상품 구매 모듈 호출
    //}
    #endregion

    #region 가챠 상점 버튼 보류
    //void OnClickGachaListButton() // 픽업 가챠 확률표
    //{
    //    GachaListPopupUI.gameObject.SetActive(true);
    //    GachaListPopupUI.SetInfo(Define.GachaType.PickupGacha);
    //}

    //void OnClickPickupGachaInfoButton() // 픽업 가챠 정보 버튼
    //{
    //    Managers.UI.ShowPopupUI<UI_PickupGachaInfoPopup>();
    //}

    //void OnClickOpenOneButton() // 픽업 가챠 1회 열기
    //{
    //    Managers.Game.Dia -= 300;
    //    DoGacha(Define.GachaType.PickupGacha, 1);
    //}

    //void OnClickOpenTenButton() // 픽업 가챠 10회 열기
    //{
    //    Managers.Game.Dia -= 2750;
    //    DoGacha(Define.GachaType.PickupGacha, 10);
    //}


    void OnClickCommonGachaOpenButton() // 일반 가챠 오픈 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.ItemDictionary.TryGetValue(Define.ID_SILVER_KEY, out int keyCount))
        {
            if (keyCount > 0) 
            { 
                Managers.Game.RemovMaterialItem(Define.ID_SILVER_KEY, 1);
                DoGacha(Define.GachaType.CommonGacha, 1);
                Refresh();
            }
            else
            {
                 Managers.UI.ShowToast("열쇠가 부족합니다.");
            }
        }
    }

    void OnClickADCommonGachaOpenButton() // 일반 가챠 광고 오픈 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.GachaCountAdsCommon > 0)
        { 
            Managers.Ads.ShowRewardedAd(() =>
            {
                Managers.Game.GachaCountAdsCommon--;
                DoGacha(Define.GachaType.CommonGacha, 1);
                Refresh();
            });
        }
    }

    void OnClickADAdvancedGachaOpenButton() // 고급 가챠 광고 오픈 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.GachaCountAdsAnvanced > 0)
        {
            Managers.Ads.ShowRewardedAd(() =>
            {
                Managers.Game.GachaCountAdsAnvanced--;
                DoGacha(Define.GachaType.CommonGacha, 1);
                Refresh();
            });
        }
    }
    void OnClickCommonGachaListButton() // 일반 가챠 리스트 버튼
    {
        Managers.Sound.PlayButtonClick();
        // 일반 가챠의 리스트를 가지고옴
        GachaListPopupUI.SetInfo(Define.GachaType.CommonGacha);

    }
   
    void OnClickAdvancedGachaOpenButton() // 고급 가챠 오픈 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.ItemDictionary.TryGetValue(Define.ID_GOLD_KEY, out int keyCount))
        {
            if (keyCount > 0)
            {
                Managers.Game.RemovMaterialItem(Define.ID_GOLD_KEY, 1);
                DoGacha(Define.GachaType.AdvancedGacha, 1);
                Refresh();
            }
            else
            {
                Managers.UI.ShowToast("열쇠가 부족합니다.");
            }
        }

    }
   
    void OnClickAdvancedGachaOpenTenButton() // 고급 가챠 10회 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.ItemDictionary.TryGetValue(Define.ID_GOLD_KEY, out int keyCount))
        {
            if (keyCount >= 10)
            {
                Managers.Game.RemovMaterialItem(Define.ID_GOLD_KEY, 10);
                DoGacha(Define.GachaType.AdvancedGacha, 10);
                Refresh();
            }
            else
            {
                Managers.UI.ShowToast("열쇠가 부족합니다.");
            }
        }
    }

    void OnClickAdvancedGachaListButton() // 고급 가챠 리스트 버튼
    {
        Managers.Sound.PlayButtonClick();
        // 고급 가챠의 리스트를 가지고옴
        GachaListPopupUI.SetInfo(Define.GachaType.AdvancedGacha);
    }

    void DoGacha(Define.GachaType gachaType, int count = 1)
    {
        List<Equipment> res = new List<Equipment>();
        res = Managers.Game.DoGacha(gachaType, count).ToList();
        if (Managers.Game.DicMission.TryGetValue(MissionTarget.GachaOpen, out MissionInfo mission))
            mission.Progress++; 
        Managers.UI.ShowPopupUI<UI_GachaResultsPopup>().SetInfo(res);
    }
    #endregion

    #region 다이아 상점 버튼 (보류)
    //void OnClickFreeDiaButton() // 무료 다이아 버튼
    //{
    //    // 보상을 얻을 수 있을 때 터치 시 무료 다이아 지급 (지급 애니나 효과 추가 필요)
    //    GetObject((int)GameObjects.FreeDiaSoldOutObject).gameObject.SetActive(true); // 솔드아웃 표시

    //}
    //void OnClickFirstDiaProductButton() // 첫번째 다이아 구매 버튼
    //{
    //    // 구매 모듈 호출
    //}
    //void OnClickSecondDiaProductButton() // 두번째 다이아 구매 버튼
    //{
    //    // 구매 모듈 호출
    //}
    //void OnClickThirdDiaProductButton() // 세번째 다이아 구매 버튼
    //{
    //    // 구매 모듈 호출
    //}
    //void OnClickFourthDiaProductButton() // 네번째 다이아 구매 버튼
    //{
    //    // 구매 모듈 호출
    //}
    //void OnClickFifthDiaProductButton() // 다섯번째 다이아 구매 버튼
    //{
    //    // 구매 모듈 호출
    //}
    #endregion

    #region 열쇠 상점 버튼
    void OnClickAdKeyButton() // 광고 열쇠 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.BronzeKeyCountAds > 0)
        {
            Managers.Ads.ShowRewardedAd(() =>
            {
                string[] spriteName = new string[1];
                int[] count = new int[1];

                spriteName[0] = Managers.Data.MaterialDic[Define.ID_BRONZE_KEY].SpriteName;
                count[0] = 1;

                UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
                rewardPopup.gameObject.SetActive(true);
                Managers.Game.BronzeKeyCountAds--;
                Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_BRONZE_KEY], 1);
                Refresh();
                rewardPopup.SetInfo(spriteName, count);
            });
        }
    }
    void OnClickSilverKeyProductButton() // 실버 열쇠 구매 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.Dia >= 150)
        {
            string[] spriteName = new string[1];
            int[] count = new int[1];

            spriteName[0] = Managers.Data.MaterialDic[Define.ID_SILVER_KEY].SpriteName;
            count[0] = 1;

            UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
            rewardPopup.gameObject.SetActive(true);
            Managers.Game.Dia -= 150;
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_SILVER_KEY], 1);
            Refresh();
            rewardPopup.SetInfo(spriteName, count);
        }
        else
        {
            Managers.UI.ShowToast("다이아가 부족합니다.");
        }
    }

    void OnClickGoldKeyProductButton() // 골드 열쇠 구매 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.Dia >= 300)
        {
            string[] spriteName = new string[1];
            int[] count = new int[1];

            spriteName[0] = Managers.Data.MaterialDic[Define.ID_GOLD_KEY].SpriteName;
            count[0] = 1;

            UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
            rewardPopup.gameObject.SetActive(true);
            Managers.Game.Dia -= 300;
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD_KEY], 1);
            Refresh();
            rewardPopup.SetInfo(spriteName, count);
        }
        else
        {
            Managers.UI.ShowToast("다이아가 부족합니다.");
        }
    }
    #endregion

    #region 골드 상점 버튼
    void OnClickFreeGoldButton() // 무료 골드 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.GoldCountAds > 0)
        {
            Managers.Ads.ShowRewardedAd(() =>
            {
                int goldAmount = 0;
                if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Game.GetMaxStageIndex(), out OfflineRewardData offlineReward))
                {
                    goldAmount = offlineReward.Reward_Gold;
                }

                string[] spriteName = new string[1];
                int[] count = new int[1];

                spriteName[0] =Define.GOLD_SPRITE_NAME;
                count[0] = goldAmount;

                UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
                rewardPopup.gameObject.SetActive(true);
                
                Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD], goldAmount);
                Managers.Game.GoldCountAds--;
                Refresh();
                rewardPopup.SetInfo(spriteName, count);

            });
        }
    }
    void OnClickFirstGoldProductButton() // 첫번째 골드 구매 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.Dia >= 300)
        {
            int goldAmount = 0;
            if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Game.GetMaxStageIndex(), out OfflineRewardData offlineReward))
            {
                goldAmount = offlineReward.Reward_Gold * 3;
            }

            string[] spriteName = new string[1];
            int[] count = new int[1];

            spriteName[0] = Define.GOLD_SPRITE_NAME;
            count[0] = goldAmount;

            UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
            rewardPopup.gameObject.SetActive(true);
            Managers.Game.Dia -= 300;
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD], goldAmount);
            rewardPopup.SetInfo(spriteName, count);
        }
        else
        {
            Managers.UI.ShowToast("다이아가 부족합니다.");
        }

    }
    void OnClickSecondGoldProductButton() // 두번째 골드 구매 버튼
    {
        Managers.Sound.PlayButtonClick();
        if (Managers.Game.Dia >= 500)
        {
            int goldAmount = 0;
            if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Game.GetMaxStageIndex(), out OfflineRewardData offlineReward))
            {
                goldAmount = offlineReward.Reward_Gold * 5;
            }

            string[] spriteName = new string[1];
            int[] count = new int[1];

            spriteName[0] = Define.GOLD_SPRITE_NAME;
            count[0] = goldAmount;

            UI_RewardPopup rewardPopup = (Managers.UI.SceneUI as UI_LobbyScene).RewardPopupUI;
            rewardPopup.gameObject.SetActive(true);
            Managers.Game.Dia -= 500;
            Managers.Game.ExchangeMaterial(Managers.Data.MaterialDic[Define.ID_GOLD], goldAmount);
            rewardPopup.SetInfo(spriteName, count);
        }
        else
        {
            Managers.UI.ShowToast("다이아가 부족합니다.");
        }
    }
    #endregion

    #endregion

    #region 버튼 스크롤 대응
    public void OnDrag(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnDrag(pointerEventData);
    }

    public void OnBeginDrag(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnBeginDrag(pointerEventData);
    }

    public void OnEndDrag(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnEndDrag(pointerEventData);
    }
    #endregion
}
