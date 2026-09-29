using Data;

public static class ItemSourceGuide
{
    public static string GetFoundLocation(int itemId, string fallbackKey)
    {
        if (TryGetFirstSource(itemId, out var area, out var creature))
            return $"{Util.Localize(area.Name)}\n{Util.Localize(creature.Name)}";

        return Util.Localize(fallbackKey);
    }

    public static bool TryGetFirstSource(int itemId, out TravelNodeData area, out CreatureData creature)
    {
        area = null;
        creature = null;
        if (Managers.Data?.TravelNodeDic == null || Managers.Data.CreatureDic == null) return false;

        // 여정 순서상 첫 출현 지역 한 곳만 고른다. 동일 지역에 여러 마물이 있어도 안내를 늘리지 않는다.
        foreach (var node in Managers.Data.TravelNodeDic.Values)
        {
            if (node.AreaType != nameof(Define.AreaType.Battle)) continue;
            Managers.Data.TravelNodeDic.TryGetValue(node.ParentId, out var parent);

            foreach (var candidate in Managers.Data.CreatureDic.Values)
            {
                if (candidate.DropItemId != itemId || candidate.DropCount <= 0 || candidate.DropChance <= 0f
                    || candidate.EncounterRate <= 0f || string.IsNullOrEmpty(candidate.PrefabLabel)
                    || string.IsNullOrEmpty(candidate.Habitat)) continue;

                // ObjectManager.PickFieldMonsterId와 같이 행선지 또는 상위 지역 키로 출현한다.
                if (candidate.Habitat != node.Name && candidate.Habitat != parent?.Name) continue;
                if (area != null && (node.Id > area.Id || (node.Id == area.Id && candidate.DataId >= creature.DataId))) continue;

                area = node;
                creature = candidate;
            }
        }
        return area != null;
    }
}
