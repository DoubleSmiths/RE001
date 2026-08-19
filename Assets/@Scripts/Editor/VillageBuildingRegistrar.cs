using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// 마을 건물(수도원·대장간·상점)이 쓰는 에셋을 어드레서블로 등록한다.
//
//  수도원 배경 프리팹은 파일은 있는데 등록이 빠져 있어 `Failed to load prefab : Village1_Monastery_BG`가
//  났다 — 그래서 예전 코드는 수도원에서도 마을 배경(Village1_Main_BG)을 쓰고 있었다.
//  대장간·상점 배경/아이콘/팝업도 같은 함정을 밟지 않도록 여기 한 곳에 모은다.
//
//  ResourceManager는 **PreLoad 라벨이 붙어 미리 로드된 것만** Load 할 수 있고,
//  PackedMode라 라벨 반영에 카탈로그 재빌드가 필요하다.
public static class VillageBuildingRegistrar
{
    // (에셋 경로, 주소, 그룹)
    static readonly (string path, string address, string group)[] TARGETS =
    {
        ("Assets/@Resources/Prefab/Map/Village1_Monastery_BG.prefab", "Village1_Monastery_BG", "Prefabs"),
        ("Assets/@Resources/Prefab/Map/Village1_Forge_BG.prefab",     "Village1_Forge_BG",     "Prefabs"),
        ("Assets/@Resources/Prefab/Map/Village1_Shop_BG.prefab",      "Village1_Shop_BG",      "Prefabs"),
        ("Assets/@Resources/Prefab/UI/Popup/UI_ShopPopup.prefab",     "UI_ShopPopup",          "UI"),
        // 신규 JSON은 PreLoad 등록이 없으면 `[DataManager] JSON load 실패`로 조용히 빈 사전이 된다.
        ("Assets/@Resources/Data/JsonData/ShopItemData.json",         "ShopItemData",          "Data"),
    };

    // 스프라이트는 주소 끝에 .sprite가 붙는 규약을 따른다 (서브스프라이트 등록과 동일).
    static readonly (string path, string address)[] SPRITES =
    {
        ("Assets/@Resources/Sprite/Icon/Menu Icon/Forge_Icon.png", "Forge_Icon"),
        ("Assets/@Resources/Sprite/Icon/Menu Icon/Shop_Icon.png",  "Shop_Icon"),
    };

    [MenuItem("Tools/Register Village Building Addressables")]
    static void Register()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[VillageAddr] AddressableAssetSettings 없음");
            return;
        }

        var done = new List<string>();
        var missing = new List<string>();

        foreach (var (path, address, groupName) in TARGETS)
        {
            if (!File.Exists(path)) { missing.Add(path); continue; }
            if (Add(settings, path, address, groupName)) done.Add(address);
        }

        foreach (var (path, address) in SPRITES)
        {
            if (!File.Exists(path)) { missing.Add(path); continue; }
            if (Add(settings, path, address, "Sprites")) done.Add(address);
        }

        if (done.Count == 0)
        {
            Debug.LogWarning($"[VillageAddr] 등록할 에셋이 없습니다. 누락: {string.Join(", ", missing)}");
            return;
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
        AssetDatabase.SaveAssets();
        Debug.Log($"[VillageAddr] 등록 {done.Count}건: {string.Join(", ", done)}" +
                  (missing.Count > 0 ? $"\n  (아직 없는 에셋: {string.Join(", ", missing)})" : ""));

        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log("[VillageAddr] Addressables 카탈로그 재빌드 완료");
    }

    static bool Add(AddressableAssetSettings settings, string path, string address, string groupName)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogWarning($"[VillageAddr] GUID 없음: {path}");
            return false;
        }

        var group = settings.FindGroup(groupName) ?? settings.DefaultGroup;
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = address;
        entry.SetLabel("PreLoad", true, true);
        return true;
    }
}
