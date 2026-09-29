#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

// v2 재설계(ADR0030)로 새로 생긴 JSON 표를 어드레서블 Data 그룹에 등록한다(주소 = 파일명, 라벨 PreLoad).
//  DataManager.LoadJson 은 PreLoad 된 것만 읽는다 — 새 표를 만들면 TABLES 에 이름만 더하고 메뉴를 한 번 돌린다.
public static class V2DataAddressableRegistrar
{
    static readonly string[] TABLES = { "BalanceConfig" };

    [MenuItem("Tools/Register V2 Data Addressables")]
    public static void Register()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { Debug.LogError("[V2Data] Addressable settings 없음"); return; }
        var group = settings.FindGroup("Data");
        if (group == null) { Debug.LogError("[V2Data] 'Data' 어드레서블 그룹 없음"); return; }

        int n = 0;
        foreach (string name in TABLES)
        {
            string path = $"Assets/@Resources/Data/JsonData/{name}.json";
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) { Debug.LogError($"[V2Data] 에셋 없음: {path} — Tools/ParseAllLocal 먼저"); continue; }
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = name;
            entry.SetLabel("PreLoad", true, true);
            n++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[V2Data] {n}/{TABLES.Length}개 등록 — 그룹 Data, 라벨 PreLoad");
    }
}
#endif
