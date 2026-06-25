using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// 장비 팝업 프리팹 3종을 어드레서블(주소=프리팹명)로 UI 그룹에 등록.
// Tools 메뉴에서 1회 실행. (exec는 Addressables 에디터 어셈블리 접근 불가라 MenuItem으로 처리)
public static class EquipPrefabAddressableRegistrar
{
    [MenuItem("Tools/Register Equipment Popup Addressables")]
    static void Register()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[EquipAddr] AddressableAssetSettings 없음");
            return;
        }

        var group = settings.FindGroup("UI") ?? settings.DefaultGroup;
        string[] names = { "UI_EquipmentPopup", "UI_EquipDetailPopup", "UI_ForgePopup" };

        foreach (var n in names)
        {
            string path = $"Assets/@Resources/Prefab/UI/Popup/{n}.prefab";
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"[EquipAddr] 프리팹 없음: {path}");
                continue;
            }

            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = n;
            entry.SetLabel("PreLoad", true, true);   // ResourceManager는 PreLoad로 미리 로드된 것만 Load 가능
            Debug.Log($"[EquipAddr] 등록: {n} (PreLoad) -> 그룹 {group.Name}");
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
        AssetDatabase.SaveAssets();
        Debug.Log("[EquipAddr] 장비 팝업 프리팹 3종 어드레서블 + PreLoad 라벨 등록 완료 — 카탈로그 재빌드 시작");

        // PackedMode(빌드 카탈로그 의존)라 PreLoad 라벨 반영 위해 콘텐츠 재빌드 필요.
        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log("[EquipAddr] Addressables 카탈로그 재빌드 완료");
    }
}
