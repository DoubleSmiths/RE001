using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class AdsManager
{
    public enum AdsStateType
    {
        None,
        Failed,
        Success
    }

    Action _rewardedCallback;

    RewardedAd _rewardedAd;

    public void Init()
    {
        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

        // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
        deviceIds.Add("96e23e80653bb28980d3f40beb58915c");
#elif UNITY_ANDROID
        deviceIds.Add("75EF8D155528C04DACBBA6F36F433035");
        deviceIds.Add("27A2FD423D2B1C06FF3A253C63E03940");//네오님 테스트폰
#endif
        Debug.Log("@>> Init Admanager");
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            Debug.Log("@>> MobileAds.Initialize()");
            RequestAndLoadRewardedAd();
        });
    }

    #region REWARDED ADS

    public void RequestAndLoadRewardedAd()
    {
        Debug.Log("@>> Requesting Rewarded ad.");
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/5224354917";//테스트용
        //string adUnitId = "ca-app-pub-2191637184387147/3994520404";//실제 배포
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        string adUnitId = "unexpected_platform";
#endif

        // create new rewarded ad instance
        RewardedAd.Load(adUnitId, CreateAdRequest(),
            (RewardedAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    Debug.Log("@>> Rewarded ad failed to load with error: " +
                                loadError.GetMessage());
                    return;
                }
                else if (ad == null)
                {
                    Debug.Log("@>> Rewarded ad failed to load.");
                    return;
                }

                Debug.Log("@>> Rewarded ad loaded.");
                _rewardedAd = ad;

                ad.OnAdFullScreenContentOpened += () =>
                {
                    Debug.Log("@>> Rewarded ad opening.");
                };
                ad.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("@>> Rewarded ad closed.");
                    RequestAndLoadRewardedAd();
                };
                ad.OnAdImpressionRecorded += () =>
                {
                    Debug.Log("@>> Rewarded ad recorded an impression.");
                };
                ad.OnAdClicked += () =>
                {
                    Debug.Log("@>> Rewarded ad recorded a click.");
                };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.Log("Rewarded ad failed to show with error: " +
                               error.GetMessage());
                };
                ad.OnAdPaid += (AdValue adValue) =>
                {
                    string msg = string.Format("{0} (currency: {1}, value: {2}",
                                               "Rewarded ad received a paid event.",
                                               adValue.CurrencyCode,
                                               adValue.Value);
                    Debug.Log(msg);
                };
            });
    }
    private Coroutine _coroutine;

    public void ShowRewardedAd(Action callback)
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Show((Reward reward) =>
            {
                CoroutineManager.StartCoroutine(CoRewardEnd(callback));              
                Debug.Log("Rewarded ad granted a reward: " + reward.Amount);
            });
        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet.");
        }
    }

    private IEnumerator CoRewardEnd(Action callback)
    {
        //yield return new WaitForSeconds(0.2f);
        yield return new WaitForEndOfFrame();
        if (Managers.Game.DicMission.TryGetValue(MissionTarget.ADWatchIng, out MissionInfo mission))
            mission.Progress++;
        callback?.Invoke();
    }
    #endregion

    private AdRequest CreateAdRequest()
    {
        AdRequest adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        return adRequest;
    }
}
