#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Define;

// AdMob 설정(ADR0044) — 앱 ID 를 GoogleMobileAdsSettings 에 넣고 안드로이드 의존성(EDM4U)을 해석한다.
//  앱 ID·광고 단위 ID 는 Define.ADMOB_* 한곳. 지금은 구글 공식 테스트 ID — 계정이 생기면 Define 을 바꾸고 이 메뉴를 한 번 돌린다.
//  EDM4U 는 대화형 에디터에서 자동 해석·확인 창을 끈다(ProjectSettings/GvhProjectSettings.xml) — unity-cli 세션에서 모달이 뜨면
//  에디터가 멈춘다. 해석은 이 메뉴와 빌드 때(AutoResolveOnBuild) 한다. 두 패키지의 편집기 API 는 internal 이라 리플렉션으로 부른다.
//    배치: Unity -batchmode -quit -projectPath . -executeMethod AdsSetup.Batch -logFile <로그>
public static class AdsSetup
{
    [MenuItem("Tools/Ads/Set Up AdMob (앱 ID + 안드로이드 의존성)")]
    public static void Run()
    {
        bool ok = SetAppId() & Resolve();
        Debug.Log(ok ? "[AdsSetup] 완료" : "[AdsSetup] 실패 — 위 로그 참고");
    }

    public static void Batch()
    {
        bool ok = false;
        try { ok = SetAppId() & Resolve(); }
        catch (Exception e) { Debug.LogError("[AdsSetup] " + e); }
        Debug.Log(ok ? "[AdsSetup] BATCH OK" : "[AdsSetup] BATCH FAIL");
        EditorApplication.Exit(ok ? 0 : 1);
    }

    static Type FindType(string fullName)
        => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName, false)).FirstOrDefault(t => t != null);

    static bool SetAppId()
    {
        var type = FindType("GoogleMobileAds.Editor.GoogleMobileAdsSettings");
        if (type == null) { Debug.LogError("[AdsSetup] GoogleMobileAdsSettings 없음 — com.google.ads.mobile 패키지가 설치됐는가"); return false; }
        var settings = type.GetMethod("LoadInstance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)?.Invoke(null, null) as ScriptableObject;
        var prop = type.GetProperty("GoogleMobileAdsAndroidAppId");
        if (settings == null || prop == null) { Debug.LogError("[AdsSetup] 설정 자산을 열지 못했다"); return false; }
        if ((string)prop.GetValue(settings) != ADMOB_APP_ID_ANDROID)
        {
            prop.SetValue(settings, ADMOB_APP_ID_ANDROID);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
        Debug.Log($"[AdsSetup] 안드로이드 앱 ID = {ADMOB_APP_ID_ANDROID}" + (ADMOB_APP_ID_ANDROID.StartsWith(ADMOB_TEST_PUBLISHER) ? " (구글 테스트 ID)" : ""));
        return true;
    }

    static bool Resolve()
    {
        var resolver = FindType("GooglePlayServices.PlayServicesResolver");
        var sync = resolver?.GetMethod("ResolveSync", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(bool) }, null);
        if (sync == null) { Debug.LogError("[AdsSetup] EDM4U(PlayServicesResolver) 없음"); return false; }
        bool ok = (bool)sync.Invoke(null, new object[] { true });
        Debug.Log("[AdsSetup] 안드로이드 의존성 해석 " + (ok ? "성공" : "실패"));
        return ok;
    }
}
#endif
