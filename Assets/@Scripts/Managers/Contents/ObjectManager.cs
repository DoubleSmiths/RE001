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
        SpawnDamageFont(pos, Util.Localize(Define.LocalizeKey.CritTextPopupText), DamageFontMode.CriticalCallout, parent);
    }

    public void ShowHealFont(Vector2 pos, int amount, Transform parent = null)
    {
        SpawnDamageFont(pos, amount.ToString(), DamageFontMode.Heal, parent);
    }

    void SpawnDamageFont(Vector2 pos, string display, DamageFontMode mode, Transform parent)
    {
        if (mode == DamageFontMode.Critical)
        {
            GameObject go = Managers.Resource.Instantiate("CriticalDamageFont", pooling: true);
            go.transform.SetParent(TextFontTransform, worldPositionStays: false);
            DamageFont df = go.GetOrAddComponent<DamageFont>();
            df.SetInfo(pos, display, mode, parent);
        }
        else
        {
            GameObject go = Managers.Resource.Instantiate("DamageFont", pooling: true);
            go.transform.SetParent(TextFontTransform, worldPositionStays: false);
            DamageFont df = go.GetOrAddComponent<DamageFont>();
            df.SetInfo(pos, display, mode, parent);
        }
    }

    // 슬라이드 62 — 타격 성공 시 피격자 위치에 Hit Effect (Player 공격 시만 호출)
    const float FX_LIFETIME = 2f;

    public void ShowHitEffect(Vector3 pos)
    {
        SpawnFxAt("FX_hit_10", pos);
    }

    // 슬라이드 62 — 치명타 시 피격자 위치에 Crit Effect (Player 공격 시만 호출)
    public void ShowCritEffect(Vector3 pos)
    {
        // 기존 프리펩에는 y가 3.44임. 이유는 모르겠는데 0으로 바꿈.
        SpawnFxAt("Basic Crit", pos);
    }

    void SpawnFxAt(string prefabKey, Vector3 pos)
    {
        GameObject go = Managers.Resource.Instantiate(prefabKey);
        if (go == null)
        {
            Debug.LogWarning($"[ObjectManager] FX prefab not found: {prefabKey}. Addressable 등록 + PreLoad 라벨 필요.");
            return;
        }
        go.transform.position = pos;
        Object.Destroy(go, FX_LIFETIME);
    }
}
