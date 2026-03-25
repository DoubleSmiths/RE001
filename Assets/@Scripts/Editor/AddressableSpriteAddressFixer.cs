using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class AddressableSpriteAddressFixer
{
    const string GROUP_NAME = "Sprites";
    const string SUFFIX = ".sprite";

    [MenuItem("Tools/Addressables/Fix Sprite Addresses")]
    public static void FixSpriteAddresses()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            EditorUtility.DisplayDialog("Error", "Addressable Settings를 찾을 수 없습니다.", "OK");
            return;
        }

        var group = settings.FindGroup(GROUP_NAME);
        if (group == null)
        {
            EditorUtility.DisplayDialog("Error", $"'{GROUP_NAME}' 그룹을 찾을 수 없습니다.", "OK");
            return;
        }

        int fixedCount = 0;

        foreach (AddressableAssetEntry entry in group.entries)
        {
            if (!entry.address.EndsWith(SUFFIX))
            {
                entry.address += SUFFIX;
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
            AssetDatabase.SaveAssets();
        }

        EditorUtility.DisplayDialog("완료", $"{fixedCount}개 항목에 '{SUFFIX}'를 추가했습니다.", "OK");
    }
}
