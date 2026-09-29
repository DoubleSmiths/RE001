using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// 재현 가능한 플레이어 빌드 진입점.
//
//  이 프로젝트의 기본 조종 경로는 unity-cli 지만, **플레이어 빌드에는 쓸 수 없다** —
//  IL2CPP/Gradle 단계가 에디터 메인 스레드를 수십 분 잡고 있으면 unity-cli 커넥터의
//  하트비트가 끊기고, exec 컨텍스트가 죽으면서 빌드까지 함께 중단된다(실측: 안드로이드
//  APK 빌드가 매번 이 지점에서 산출물 없이 죽었다).
//
//  그래서 빌드만은 batchmode 로 돌린다. 에디터를 닫아야 하지만(Library 락) 빌드는
//  원래 오래 걸리는 작업이라 그 편이 맞다.
//
//  사용:
//    Unity.exe -batchmode -quit -projectPath . -logFile - \
//              -executeMethod BuildScript.Android -buildOut <경로>
//
//  -buildOut 을 주지 않으면 프로젝트 옆 Builds/ 아래에 떨어진다.
//  어드레서블 콘텐츠는 **플랫폼별**이라 플레이어 빌드 직전에 항상 다시 굽는다
//  (안 그러면 다른 플랫폼의 카탈로그가 남아 런타임에 로드가 통째로 실패한다).
//
//  구글 플레이 제출용은 AndroidBundle(.aab). APK 는 기기에 직접 깔아 보는 용도.
//  서명: 환경변수 RE001_KEYSTORE(경로) / RE001_KEYSTORE_PASS / RE001_KEY_ALIAS / RE001_KEY_PASS 가
//  있으면 그 업로드 키로 서명한다. 없으면 디버그 키 — 플레이 콘솔은 디버그 서명 번들을 받지 않는다.
//  키스토어는 저장소에 두지 않는다(_tools/make_upload_keystore.ps1 로 만들어 따로 보관).
//  -versionCode N 을 주면 안드로이드 버전 코드를 그 값으로(업로드마다 올려야 한다).
public static class BuildScript
{
    const string APK = "MinimonQuest.apk";
    const string AAB = "MinimonQuest.aab";
    const string EXE = "MinimonQuest.exe";

    [MenuItem("Tools/Build/안드로이드 APK (기기 테스트)")]
    public static void AndroidMenu() => Run(BuildTarget.Android, APK);

    [MenuItem("Tools/Build/안드로이드 AAB (구글 플레이)")]
    public static void AndroidBundleMenu() => Run(BuildTarget.Android, AAB);

    [MenuItem("Tools/Build/윈도우 (스팀)")]
    public static void WindowsMenu() => Run(BuildTarget.StandaloneWindows64, EXE);

    public static void Android() => Run(BuildTarget.Android, APK);

    public static void AndroidBundle() => Run(BuildTarget.Android, AAB);

    public static void Windows() => Run(BuildTarget.StandaloneWindows64, EXE);

    static void Run(BuildTarget target, string fileName)
    {
        string outDir = Arg("-buildOut") ?? Path.Combine(Directory.GetCurrentDirectory(), "Builds", target.ToString());
        Directory.CreateDirectory(outDir);

        var group = BuildPipeline.GetBuildTargetGroup(target);
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            Log($"플랫폼 전환 {EditorUserBuildSettings.activeBuildTarget} → {target} (에셋 재임포트가 걸린다)");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(group, target))
                Fail($"플랫폼 전환 실패: {target}");
        }

        if (target == BuildTarget.Android)
            ConfigureAndroid(fileName.EndsWith(".aab"));

        // 어드레서블 먼저 — 카탈로그가 플랫폼별이라 플레이어보다 앞서야 한다.
        Log("어드레서블 콘텐츠 빌드");
        AddressableAssetSettings.BuildPlayerContent(out var aa);
        if (aa == null || !string.IsNullOrEmpty(aa.Error))
            Fail("어드레서블 빌드 실패: " + (aa == null ? "결과 없음" : aa.Error));
        Log($"어드레서블 완료 {aa.Duration:0.0}s → {aa.OutputPath}");

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) Fail("빌드에 포함된 씬이 없다");
        Log("씬: " + string.Join(", ", scenes.Select(Path.GetFileNameWithoutExtension)));

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(outDir, fileName),
            target = target,
            targetGroup = group,
            options = BuildOptions.None,
        });

        // 키스토어 경로·별칭은 ProjectSettings 에 직렬화된다 — 기계마다 다른 값이 저장소에 새지 않게 되돌린다.
        if (target == BuildTarget.Android && PlayerSettings.Android.useCustomKeystore)
        {
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.Android.keystoreName = "";
            PlayerSettings.Android.keyaliasName = "";
        }

        var s = report.summary;
        Log($"빌드 결과={s.result} 시간={s.totalTime} 크기={s.totalSize / 1048576}MB " +
            $"에러={s.totalErrors} 경고={s.totalWarnings} 산출물={s.outputPath}");

        if (s.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Fail("플레이어 빌드 실패: " + s.result);

        Log("BUILD OK");
    }

    static void ConfigureAndroid(bool appBundle)
    {
        EditorUserBuildSettings.buildAppBundle = appBundle;
        // 플레이 콘솔 비정상 종료 보고서에 네이티브 스택을 보이게 — symbols.zip 을 함께 올린다.
        EditorUserBuildSettings.androidCreateSymbols = appBundle ? AndroidCreateSymbols.Public : AndroidCreateSymbols.Disabled;

        string versionCode = Arg("-versionCode");
        if (versionCode != null)
            PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);

        string keystore = Environment.GetEnvironmentVariable("RE001_KEYSTORE");
        bool signed = !string.IsNullOrEmpty(keystore) && File.Exists(keystore);
        PlayerSettings.Android.useCustomKeystore = signed;
        if (signed)
        {
            PlayerSettings.Android.keystoreName = keystore;
            PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("RE001_KEYSTORE_PASS");
            PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("RE001_KEY_ALIAS");
            PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("RE001_KEY_PASS");
        }
        Log($"안드로이드 {(appBundle ? "AAB" : "APK")} v{PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode}) " +
            $"targetSdk={PlayerSettings.Android.targetSdkVersion} 서명={(signed ? "업로드 키" : "디버그 키(플레이 콘솔 제출 불가)")}");
    }

    // batchmode 로그에서 눈에 띄게 — 로그가 수만 줄이라 접두사가 없으면 찾을 수 없다.
    static void Log(string m) => Debug.Log("[BuildScript] " + m);

    static void Fail(string m)
    {
        Debug.LogError("[BuildScript] " + m);
        // -quit 만으로는 종료 코드가 0 이라 CI/스크립트가 실패를 놓친다.
        EditorApplication.Exit(1);
        throw new Exception(m);
    }

    static string Arg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == name) return args[i + 1];
        return null;
    }
}
