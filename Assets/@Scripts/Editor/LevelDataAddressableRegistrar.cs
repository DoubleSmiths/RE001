#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

// @Resources/Data/JsonData/LevelData.json을 어드레서블로 등록(그룹 Data, 주소 LevelData, 라벨 PreLoad).
//  → DataManager.Init()의 LoadJson("LevelData")가 읽을 수 있게 한다. (BuffData.json과 동일 방식)
//  배치 실행: Unity.exe -batchmode -quit -executeMethod LevelDataAddressableRegistrar.RegisterBatch
public static class LevelDataAddressableRegistrar
{
    const string ASSET_PATH = "Assets/@Resources/Data/JsonData/LevelData.json";

    [MenuItem("Tools/Register LevelData Addressable")]
    public static void RegisterBatch()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { Debug.LogError("[LevelData] Addressable settings 없음"); return; }

        string guid = AssetDatabase.AssetPathToGUID(ASSET_PATH);
        if (string.IsNullOrEmpty(guid)) { Debug.LogError($"[LevelData] 에셋 없음: {ASSET_PATH}"); return; }

        var group = settings.FindGroup("Data") ?? settings.DefaultGroup;
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = "LevelData";
        entry.SetLabel("PreLoad", true, true);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LevelData] 어드레서블 등록 완료 — 그룹 {group.Name}, 주소 LevelData, 라벨 PreLoad");
    }
}
#endif
