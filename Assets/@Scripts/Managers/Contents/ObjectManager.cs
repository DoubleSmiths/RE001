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

    public void ShowDamageFont(Vector2 pos, float damage, Transform parent, bool isCritical = false)
    {
        string prefabName = isCritical ? "CriticalDamageFont" : "DamageFont";
        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        DamageFont damageText = go.GetOrAddComponent<DamageFont>();
        damageText.SetInfo(pos, damage, 0, parent, isCritical);
    }
}
