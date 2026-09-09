// 마을의 격리 세션에서 unity-cli exec 인자로 실행한다. _path == null은 호출자 사전조건이다.
// 프레임을 양보하지 않고 복제 GameData만 바꾸며, 예외가 나도 원본 참조와 플레이어 HP를 복구한다.
var game = Managers.Game;
var player = game.Player;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var dataField = typeof(GameManager).GetField("_gameData", flags);
var pathField = typeof(GameManager).GetField("_path", flags);
var rebuild = typeof(GameManager).GetMethod("RebuildEquippedFromOwned", flags);
if (player == null || player.InBattle || player.AbilityRunner == null)
    throw new System.Exception("Novice crafting requires a ready player in town");
if (pathField.GetValue(game) != null)
    throw new System.Exception("Refusing verification: caller must set GameManager._path to null first");
if (((System.Collections.ICollection)typeof(AbilityRunner).GetField("_buffs", flags).GetValue(player.AbilityRunner)).Count != 0)
    throw new System.Exception("Wait for temporary ability buffs to expire before verification");
var original = (GameData)dataField.GetValue(game);
string originalJson = Newtonsoft.Json.JsonConvert.SerializeObject(original);
float originalHp = player.Hp;
int checks = 0;
void Check(bool pass, string message)
{
    if (!pass) throw new System.Exception("NoviceCrafting: " + message);
    checks++;
}
bool Same(float left, float right) => UnityEngine.Mathf.Approximately(left, right);
string Snapshot() => Newtonsoft.Json.JsonConvert.SerializeObject(dataField.GetValue(game));
var ids = new[] { 30061, 30062 };
var materialIds = new[] { new[] { 11001, 10001, 10002 }, new[] { 11001, 10002, 11004 } };
var materialCounts = new[] { 3, 3, 2 };
var attack = new[] { 18, 15 };
var critical = new[] { 0f, 10f };
try
{
    var isolated = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(originalJson);
    Check(isolated != null && !object.ReferenceEquals(isolated, original), "GameData must be an independent copy");
    isolated.OwnedEquipments.Clear();
    isolated.EquippedEquipments.Clear();
    isolated.Inventory.Clear();
    isolated.EquippedAbilities.Clear();
    isolated.Level = 5;
    dataField.SetValue(game, isolated);
    player.RefreshStats(false);
    float unarmedAtk = player.PhysicalAtk;
    float unarmedCrit = player.CritRate;
    // 시작 무기는 실제 장착 경로로 넣는다. 신규 무기로 교체할 때 해제되는지도 검사한다.
    var previousWeapon = game.AddEquipment(30001);
    game.EquipItem(previousWeapon);

    for (int index = 0; index < ids.Length; index++)
    {
        int id = ids[index];
        Check(Managers.Data.EquipmentDic.ContainsKey(id), $"Missing equipment {id}");
        var equipmentData = Managers.Data.EquipmentDic[id];
        Check(equipmentData.SlotType == Define.EquipSlotType.Weapon && equipmentData.Grade == Define.EquipGrade.Basic,
            $"{id} must be a Basic weapon");
        Check(equipmentData.BaseAtk == attack[index] && Same(equipmentData.BaseCritRate, critical[index]) && equipmentData.BaseMagicAtk == 0,
            $"{id} independent base stat contract");
        Check(!Managers.Data.SkillDic.Values.Any(s => s.Source == Define.AbilitySource.Equip && s.RequiredEquipId == id),
            $"{id} must not silently grant an extra equipment ability");
        Check(Managers.Data.ForgeRecipeDic.Values.Count(r => r.EquipDataId == id) == 1, $"{id} exactly one recipe");
        var recipe = game.GetRecipeFor(id);
        Check(recipe != null && recipe.DataId == 40060 + index && recipe.ReqLevel == 5 && recipe.Gold == 90,
            $"{id} recipe ID, level 5 and gold 90");
        var actualIds = new[] { recipe.Mat1Id, recipe.Mat2Id, recipe.Mat3Id };
        var actualCounts = new[] { recipe.Mat1Count, recipe.Mat2Count, recipe.Mat3Count };
        for (int m = 0; m < 3; m++)
        {
            Check(actualIds[m] == materialIds[index][m] && actualCounts[m] == materialCounts[m], $"{id} material contract #{m}");
            Check(Managers.Data.ItemDic.ContainsKey(actualIds[m]), $"{id} material {actualIds[m]} exists");
        }
        var enhanceRows = Managers.Data.EquipEnhanceDic.Values.Where(e => e.EquipDataId == id).ToArray();
        Check(enhanceRows.Length == 9, $"{id} has nine enhancement rows");
        for (int level = 1; level <= 9; level++)
        {
            Check(enhanceRows.Count(e => e.Level == level) == 1, $"{id} unique enhancement +{level}");
            var row = enhanceRows.Single(e => e.Level == level);
            Check(row.AtkBonus == (index == 0 ? 9 : 7) && Same(row.CritBonus, 0f) && row.MagicAtkBonus == 0,
                $"{id} enhancement +{level} stat contract");
            Check(row.RequiredMaterialId == 11001 && row.RequiredMaterialCount == 3 * level
                && Managers.Data.ItemDic.ContainsKey(row.RequiredMaterialId), $"{id} enhancement +{level} material contract");
            var nextMaterial = game.GetEnhanceMaterial(new Equipment(id) { EnhanceLevel = level - 1 });
            Check(nextMaterial.itemId == 11001 && nextMaterial.count == 3 * level, $"{id} runtime enhancement +{level} material lookup");
        }
        var maxBonus = game.GetEnhanceBonus(id, 9);
        Check(maxBonus.atk == (index == 0 ? 81 : 63) && Same(maxBonus.crit, 0f) && maxBonus.magic == 0,
            $"{id} runtime +9 cumulative bonus");

        // 충분한 재화로 레벨 경계만 분리한다. 각 실패는 전체 직렬화 값이 그대로여야 한다.
        isolated.Inventory.Clear();
        for (int m = 0; m < 3; m++)
            isolated.Inventory.Add(new InventoryItem { DataId = materialIds[index][m], Count = materialCounts[m] + 4 });
        isolated.Gold = 127;
        isolated.Level = 4;
        string before = Snapshot();
        Check(!game.CanCraft(id) && !game.CraftEquipment(id), $"{id} level 4 blocked");
        Check(Snapshot() == before, $"{id} failed level check leaves all data unchanged");
        isolated.Level = 5;
        Check(game.CanCraft(id), $"{id} level 5 allowed");
        isolated.Gold = 89;
        before = Snapshot();
        Check(!game.CanCraft(id) && !game.CraftEquipment(id), $"{id} insufficient gold blocked");
        Check(Snapshot() == before, $"{id} gold failure does not spend materials or change ownership");
        isolated.Gold = 127;
        for (int m = 0; m < 3; m++)
        {
            var item = isolated.Inventory.Single(i => i.DataId == materialIds[index][m]);
            int enough = item.Count;
            item.Count = materialCounts[m] - 1;
            before = Snapshot();
            Check(!game.CanCraft(id) && !game.CraftEquipment(id), $"{id} missing material #{m} blocked");
            Check(Snapshot() == before, $"{id} material #{m} failure is atomic");
            item.Count = enough;
        }
        // 요구량과 정확히 같은 재료도 허용하며, 0개가 된 인벤토리 행은 제거되어야 한다.
        isolated.Inventory.Single(i => i.DataId == materialIds[index][2]).Count = materialCounts[2];
        Check(game.CraftEquipment(id), $"{id} successful crafting");
        Check(game.Gold == 37, $"{id} spends exactly 90 gold");
        for (int m = 0; m < 3; m++)
            Check(game.GetItemCount(materialIds[index][m]) == (m == 2 ? 0 : 4), $"{id} exact material #{m} consumption");
        Check(!isolated.Inventory.Any(i => i.DataId == materialIds[index][2]), $"{id} consumed item row removed");
        Check(game.OwnedEquipments.Count(e => e.DataId == id) == 1, $"{id} exactly one owned result");
        var crafted = game.OwnedEquipments.Single(e => e.DataId == id);
        Check(!crafted.IsEquipped && previousWeapon.IsEquipped
            && object.ReferenceEquals(game.EquippedEquipments[Define.EquipSlotType.Weapon], previousWeapon),
            $"{id} crafting does not replace the occupied weapon slot");
        Check(crafted.EnhanceLevel == 0 && crafted.Durability == equipmentData.MaxDurability, $"{id} pristine crafted instance");
        // 중복 방지는 재화 부족 때문에 우연히 통과하지 않도록 재료와 골드를 다시 충분히 준다.
        isolated.Gold = 127;
        for (int m = 0; m < 3; m++) game.AddItem(materialIds[index][m], 10);
        before = Snapshot();
        Check(!game.CanCraft(id) && !game.CraftEquipment(id), $"{id} duplicate crafting blocked with sufficient resources");
        Check(Snapshot() == before, $"{id} duplicate attempt changes no persisted data");

        float previousAtk = player.PhysicalAtk;
        float previousCrit = player.CritRate;
        game.EquipItem(crafted);
        Check(!previousWeapon.IsEquipped && crafted.IsEquipped
            && object.ReferenceEquals(game.EquippedEquipments[Define.EquipSlotType.Weapon], crafted), $"{id} real equip swaps old instance");
        Check(Same(player.PhysicalAtk, unarmedAtk + attack[index]) && Same(player.CritRate, unarmedCrit + critical[index]),
            $"{id} actual player attack/crit equals independent unarmed plus weapon expectation");
        if (index == 1)
        {
            Check(Same(player.PhysicalAtk - previousAtk, -3f) && Same(player.CritRate - previousCrit, 10f),
                "Rapier swap trades exactly 3 physical attack for 10 critical percentage points");
            Check(Same(player.FinalPhysicalAtk, (unarmedAtk + 15f) * (1f + player.PhysicalAtkRate / 100f) * player.PhysicalAtkAmp),
                "Rapier final physical attack includes the expected base and current multipliers");
        }
        previousWeapon = crafted;
    }

    // 실제 로드가 사용하는 장착 참조 재구성 메서드를 호출한다. 디스크 LoadGame은 호출하지 않는다.
    string craftedJson = Snapshot();
    var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(craftedJson);
    Check(restored.OwnedEquipments.Count(e => ids.Contains(e.DataId)) == 2, "Both crafts survive serialization");
    Check(restored.OwnedEquipments.Single(e => e.DataId == 30062).IsEquipped
        && !restored.OwnedEquipments.Single(e => e.DataId == 30061).IsEquipped, "Equipped flags survive serialization");
    Check(restored.EquippedEquipments.Count == 0, "Derived equipment dictionary is excluded from save");
    dataField.SetValue(game, restored);
    rebuild.Invoke(game, null);
    player.RefreshStats(false);
    Check(object.ReferenceEquals(game.EquippedEquipments[Define.EquipSlotType.Weapon],
        restored.OwnedEquipments.Single(e => e.DataId == 30062)), "Restore rebuilds owned/equipped shared instance");
    Check(Same(player.PhysicalAtk, unarmedAtk + 15f) && Same(player.CritRate, unarmedCrit + 10f), "Restored weapon stats match before save");
    Check(!game.CanCraft(30061) && !game.CanCraft(30062), "Restored ownership still blocks duplicate crafting");
}
finally
{
    dataField.SetValue(game, original);
    player.RefreshStats(false);
    player.Hp = originalHp;
    typeof(AbilityManager).GetMethod("RaiseChanged", flags).Invoke(Managers.Ability, null);
}
Check(pathField.GetValue(game) == null, "Save path remains disabled");
Check(object.ReferenceEquals(dataField.GetValue(game), original) && Snapshot() == originalJson, "Original GameData reference and serialized contents restored");
return $"PASS: NoviceCrafting {checks} checks; level, atomic costs, duplicate protection, weapon tradeoff, enhancement data, save restore";
