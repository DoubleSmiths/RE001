using UnityEngine;

// M1: 기본 구조만 포함. M2에서 컨트롤러 참조 추가 예정.
public class ObjectManager
{
    public Transform SkillTransform
    {
        get
        {
            GameObject root = GameObject.Find("@Skill");
            if (root == null)
                root = new GameObject { name = "@Skill" };
            return root.transform;
        }
    }

    public Transform MonsterTransform
    {
        get
        {
            GameObject root = GameObject.Find("@Monster");
            if (root == null)
                root = new GameObject { name = "@Monster" };
            return root.transform;
        }
    }

    public Transform TextFontTransform
    {
        get
        {
            GameObject root = GameObject.Find("@TextFont");
            if (root == null)
                root = new GameObject { name = "@TextFont" };
            return root.transform;
        }
    }

    public ObjectManager() { }

    public void Clear() { }

    public MonsterController SpawnMonster(int dataId, Vector3 pos)
    {
        if (!Managers.Data.CreatureDic.TryGetValue(dataId, out var data))
        {
            Debug.LogWarning($"[ObjectManager] CreatureData not found: {dataId}");
            return null;
        }

        GameObject go = Managers.Resource.Instantiate(data.PrefabLabel, MonsterTransform);
        go.transform.position = pos;

        MonsterController mc = go.GetOrAddComponent<MonsterController>();
        mc.SetInfo(dataId);
        return mc;
    }

    // 슬라이드 63 — Damage Textpopup. 모드는 DamageFontMode 참조.
    public void ShowDamageFont(Vector2 pos, int damage, DamageFontMode mode = DamageFontMode.Normal, Transform parent = null)
    {
        string display = (mode == DamageFontMode.Miss) ? Util.Localize(Define.LocalizeKey.BattleMissText) : damage.ToString();
        SpawnDamageFont(pos, display, mode, parent);
    }

    public void ShowMissPopup(Vector2 pos, Transform parent = null)
    {
        SpawnDamageFont(pos, Util.Localize(Define.LocalizeKey.BattleMissText), DamageFontMode.Miss, parent);
    }

    // 슬라이드 62/63 — 치명타 시 공격자 위치에 callout
    public void ShowCriticalCallout(Vector2 pos, Transform parent = null)
    {
        SpawnDamageFont(pos, Util.Localize(Define.LocalizeKey.BattleCriticalText), DamageFontMode.CriticalCallout, parent);
    }

    public void ShowHealFont(Vector2 pos, int amount, Transform parent = null)
    {
        SpawnDamageFont(pos, amount.ToString(), DamageFontMode.Heal, parent);
    }

    void SpawnDamageFont(Vector2 pos, string display, DamageFontMode mode, Transform parent)
    {
        GameObject go = Managers.Resource.Instantiate("DamageFont", pooling: true);
        go.transform.SetParent(TextFontTransform, worldPositionStays: false);
        DamageFont df = go.GetOrAddComponent<DamageFont>();
        df.SetInfo(pos, display, mode, parent);
    }
}
