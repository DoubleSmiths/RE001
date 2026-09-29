#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

// @Resources/Sound 하위의 모든 AudioClip을 어드레서블로 등록(주소=파일명, 라벨 PreLoad).
//  → 게임 시작 시 LoadAllAsync<Object>("PreLoad")가 _resources에 적재하고,
//    SoundManager.Play(type, "클립명")이 Resource.Load<AudioClip>로 꺼내 재생한다.
//  (unity-cli exec는 Addressables Editor 어셈블리를 참조 못 해서, Editor 메뉴로 등록한다.)
public static class SoundAddressableRegistrar
{
    [MenuItem("Tools/Register Sound Addressables")]
    static void Register()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { Debug.LogError("[Sound] Addressable settings 없음"); return; }

        // ⚠️ 기본 그룹(DefaultGroup)은 UI 다 — 예전엔 여기에 넣어서 실행하면 사운드 65개가 전부 UI 그룹으로 옮겨졌다.
        var group = settings.FindGroup("Sound");
        if (group == null) { Debug.LogError("[Sound] 'Sound' 어드레서블 그룹 없음"); return; }

        // 지워진 파일의 항목을 치운다(형식을 바꿔 다시 구운 BGM 처럼 새 파일로 대체된 경우).
        foreach (var stale in new System.Collections.Generic.List<UnityEditor.AddressableAssets.Settings.AddressableAssetEntry>(group.entries))
            if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(stale.guid)) || AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(stale.guid)) == null)
                group.RemoveAssetEntry(stale);

        var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/@Resources/Sound" });
        int n = 0;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var entry = settings.CreateOrMoveEntry(g, group);
            entry.address = Path.GetFileNameWithoutExtension(path);   // 예: SFX_BtnSelect, Sound_Miss, BGM_BWF
            entry.SetLabel("PreLoad", true, true);
            n++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[Sound] AudioClip {n}개 어드레서블 등록 완료(PreLoad). 주소=파일명.");
    }
}
#endif
