#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

// v2 재설계(ADR0030)로 새로 생긴 에셋을 어드레서블에 등록한다(주소 = 파일 이름, 라벨 PreLoad).
//  DataManager.LoadJson·ResourceManager.Load 는 PreLoad 된 것만 읽는다 — 새 에셋은 ENTRIES 에 한 줄 더하고 메뉴를 한 번 돌린다.
public static class V2DataAddressableRegistrar
{
    static readonly (string group, string path)[] ENTRIES =
    {
        ("Data",     "Assets/@Resources/Data/JsonData/BalanceConfig.json"),
        ("Data",     "Assets/@Resources/Data/JsonData/MissionData.json"),
        ("Data",     "Assets/@Resources/Data/JsonData/ProductData.json"),
        ("Data",     "Assets/@Resources/Data/JsonData/RelicData.json"),   // 유물 30종(ADR0035)
        ("Data",     "Assets/@Resources/Data/JsonData/BondNpcData.json"), // 좋아하는 선물(ADR0042)
        ("Data",     "Assets/@Resources/Data/JsonData/CostumeData.json"), // 의상 8벌(ADR0043)
        ("Material", BOSS_OUTLINE_MAT),   // 스테이지 보스 금빛 테두리(BossOutline)
        ("UI",       "Assets/@Resources/Prefab/UI/Popup/UI_MissionPopup.prefab"),   // 미션·출석 창
    };

    const string BOSS_OUTLINE_MAT    = "Assets/Material/BossOutline.mat";
    const string BOSS_OUTLINE_SHADER = "RE001/SpriteSilhouette";

    [MenuItem("Tools/Register V2 Data Addressables")]
    public static void Register()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { Debug.LogError("[V2Data] Addressable settings 없음"); return; }

        EnsureBossOutlineMaterial();

        int n = 0;
        foreach (var (groupName, path) in ENTRIES)
        {
            var group = settings.FindGroup(groupName);
            if (group == null) { Debug.LogError($"[V2Data] '{groupName}' 어드레서블 그룹 없음"); continue; }
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) { Debug.LogError($"[V2Data] 에셋 없음: {path} — 표는 Tools/ParseAllLocal 먼저"); continue; }
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = Path.GetFileNameWithoutExtension(path);
            entry.SetLabel("PreLoad", true, true);
            n++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[V2Data] {n}/{ENTRIES.Length}개 등록 — 라벨 PreLoad");
    }

    // 재질은 셰이더만 가리키면 되니 손으로 만들지 않고 여기서 만든다(다시 돌려도 그대로).
    static void EnsureBossOutlineMaterial()
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(BOSS_OUTLINE_MAT) != null) return;
        var shader = Shader.Find(BOSS_OUTLINE_SHADER);
        if (shader == null) { Debug.LogError($"[V2Data] 셰이더 {BOSS_OUTLINE_SHADER} 없음 — Assets/Material/Shader/SpriteSilhouette.shader"); return; }
        AssetDatabase.CreateAsset(new Material(shader), BOSS_OUTLINE_MAT);
        Debug.Log($"[V2Data] 재질 생성: {BOSS_OUTLINE_MAT}");
    }
}
#endif
