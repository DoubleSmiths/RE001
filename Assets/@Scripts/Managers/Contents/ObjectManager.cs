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

    /// <summary>
    /// 이미 있을 때만 돌려준다 (없으면 만들지 않고 null).
    ///
    /// 정리 경로(OnDestroy 등)에서 위의 lazy 게터를 쓰면 **파괴 중에 @Monster를 새로 만들어**
    /// 씬을 닫을 때 "Some objects were not cleaned up when closing the scene" 빨간 에러가 난다.
    /// 치울 대상이 없으면 애초에 만들 필요도 없다.
    /// </summary>
    public Transform MonsterRootIfExists => GameObject.Find("@Monster")?.transform;

    // 동행 미니몬 루트 — @Monster와 분리(몬스터로 오인되지 않도록).
    public Transform CompanionTransform
    {
        get
        {
            GameObject root = GameObject.Find("@Companion");
            if (root == null)
                root = new GameObject { name = "@Companion" };
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

        // Y는 **항상 지면(GROUND_Y)**. 몬스터마다 다른 SpawnY를 쓰던 걸 폐기했다 —
        //  아트별 바닥 여백은 pivot이 흡수하므로(Tools/Ground Pivot 정렬) 500종이 같은 y를 써도
        //  전부 같은 바닥에 선다. 데이터로 높이를 흔들면 누구는 뜨고 누구는 파묻힌다.
        pos.y = Define.GROUND_Y;
        go.transform.position = pos;

        MonsterController mc = go.GetOrAddComponent<MonsterController>();
        mc.SetInfo(dataId);
        return mc;
    }

    // 데이터 드리븐 스폰: 현재 전투 지역(CurrentTravelAreaId)의 Habitat과 일치하는 몬스터 풀에서
    //  EncounterRate를 가중치로 랜덤 선택. 지역을 모르거나 풀이 비면 fallback(슬라임 2001).
    //  → CreatureData의 Habitat/EncounterRate만 고치면 등장 몬스터·확률을 데이터로 조정 가능.
    public int PickFieldMonsterId(int fallback = 2001)
    {
        int nodeId = Managers.Game != null ? Managers.Game.CurrentTravelAreaId : 0;
        var pool = FieldPool(nodeId);

        float total = 0f;
        foreach (var c in pool) total += c.EncounterRate;
        if (pool.Count == 0 || total <= 0f) return fallback;

        float r = Random.value * total;
        foreach (var c in pool)
        {
            r -= c.EncounterRate;
            if (r <= 0f) return c.DataId;
        }
        return pool[pool.Count - 1].DataId;
    }

    // 그 노드에 나오는 몬스터 — 서식지가 노드 이름이나 부모 이름(입구/지역 둘 다)과 같고 출현 확률이 0보다 큰 것.
    System.Collections.Generic.List<Data.CreatureData> FieldPool(int nodeId)
    {
        var pool = new System.Collections.Generic.List<Data.CreatureData>();
        var data = Managers.Data;
        if (data?.CreatureDic == null || data.TravelNodeDic == null) return pool;
        if (!data.TravelNodeDic.TryGetValue(nodeId, out var node)) return pool;

        var keys = new System.Collections.Generic.HashSet<string>();
        if (!string.IsNullOrEmpty(node.Name)) keys.Add(node.Name);
        if (data.TravelNodeDic.TryGetValue(node.ParentId, out var parent) && !string.IsNullOrEmpty(parent.Name))
            keys.Add(parent.Name);

        foreach (var c in data.CreatureDic.Values)
        {
            if (c == null || string.IsNullOrEmpty(c.Habitat)) continue;
            if (!keys.Contains(c.Habitat) || c.EncounterRate <= 0f) continue;
            pool.Add(c);
        }
        return pool;
    }

    // v2 스테이지 보스(ADR0030). 모습은 그 노드에서 체력이 가장 높은 몬스터, 능력치는 그 노드 몬스터의
    //  출현 가중 평균에 보스 배율을 곱한다. 가장 센 개체의 능력치에 곱하면 우두머리(체력 5~7배)가 사는
    //  노드만 보스가 터무니없이 세진다 — 평균을 쓰면 어느 스테이지든 "일반 처치 8번 분량"으로 난이도가 고르다.
    public MonsterController SpawnStageBoss(int nodeId, Vector3 pos)
    {
        var pool = FieldPool(nodeId);
        if (pool.Count == 0) return null;

        Data.CreatureData look = pool[0];
        float w = 0f, hp = 0f, atk = 0f, matk = 0f, def = 0f;
        foreach (var c in pool)
        {
            if (c.MaxHp > look.MaxHp) look = c;
            w    += c.EncounterRate;
            hp   += c.MaxHp       * c.EncounterRate;
            atk  += c.PhysicalAtk * c.EncounterRate;
            matk += c.MagicAtk    * c.EncounterRate;
            def  += c.Def         * c.EncounterRate;
        }

        MonsterController boss = SpawnMonster(look.DataId, pos);
        if (boss == null) return null;

        var d = Managers.Data;
        float atkMul = d.Balance(Define.BalanceKey.BOSS_ATK_MULT, 1.5f);
        float kills  = d.Balance(Define.BalanceKey.BOSS_REWARD_KILLS, 10f);
        Managers.Game.TryGetAreaAverage(nodeId, out float avgExp, out float avgGold);
        boss.MakeStageBoss(
            maxHp: hp / w * d.Balance(Define.BalanceKey.BOSS_HP_MULT, 8f),
            atk:   atk / w * atkMul,
            matk:  matk / w * atkMul,
            def:   def / w * d.Balance(Define.BalanceKey.BOSS_DEF_MULT, 1.2f),
            scale: d.Balance(Define.BalanceKey.BOSS_SCALE, 1.35f),
            rewardExp:  avgExp * kills,
            rewardGold: Mathf.RoundToInt(avgGold * kills));
        return boss;
    }

    // [동행 미니몬] 스폰 — 적과 동일한 CreatureData.PrefabLabel(슬라임 등) 재사용.
    //  CompanionController가 추종/자동공격을 담당. (MonsterController는 붙이지 않음)
    public CompanionController SpawnCompanion(int minionId, PlayerController player)
    {
        if (player == null) return null;
        if (!Managers.Data.CreatureDic.TryGetValue(minionId, out var data))
        {
            Debug.LogWarning($"[ObjectManager] Companion CreatureData not found: {minionId}");
            return null;
        }

        GameObject go = Managers.Resource.Instantiate(data.PrefabLabel, CompanionTransform);
        if (go == null) return null;

        CompanionController cc = go.GetOrAddComponent<CompanionController>();
        cc.Setup(minionId, player);
        return cc;
    }

    // 슬라이드 63 — Damage Textpopup. 모드는 DamageFontMode 참조.
    public void ShowDamageFont(Vector2 pos, int damage, DamageFontMode mode = DamageFontMode.Normal, Transform parent = null)
    {
        string display = (mode == DamageFontMode.Miss) ? Util.Localize(Define.LocalizeKey.BattleMissText) : damage.ToString();
        SpawnDamageFont(pos, display, mode, parent);
    }

    public void ShowMissPopup(Vector2 pos, Transform parent = null)
    {
        Managers.Sound?.PlaySFX("Sound_Miss");   // 빗나감 (VS MissText &Sound 이식)
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
        // random하게 위치 보정값
        Vector3 randPos = new Vector3(Random.Range(0, 0.3f), Random.Range(0, 0.3f), 0);
        SpawnFxAt("FX_hit_04", pos + randPos); // 나중에는 무기에 따라 달라져야 함. 또는 치명타인지 아닌지에 따라 다름. 치명타면 FX_hit_01, 일반은 FX_hit_04
    }

    public void ShowCriHitEffect(Vector3 pos)
    {
        // random하게 위치 보정값
        Vector3 randPos = new Vector3(Random.Range(0, 0.3f), Random.Range(0, 0.3f), 0);
        SpawnFxAt("FX_hit_01", pos + randPos);
    }

    // 슬라이드 62 — 치명타 시 피격자 위치에 Crit Effect (Player 공격 시만 호출)
    public void ShowCritEffect(Vector3 pos)
    {
        // 기존 프리펩에는 y가 3.44임. 이유는 모르겠는데 0으로 바꿈.
        // test vec
        //Vector3 randPos = new Vector3(Random.Range(0, 0.3f), Random.Range(0, 0.3f), 0);
        //SpawnFxAt("FX_hit_01", pos + randPos);

        // todo 나중에 사용하는거.
        //SpawnFxAt("Basic Crit", new Vector3(0, 0, 0)); // 나중에는 무기에 따라 달라져야 함.
    }

    // 슬42: HP 물약 사용 시 HP Potion Ef를 부모(플레이어) 아래 생성. 좌표는 프리팹 저작 로컬값 유지(코드 보정 안 함).
    public void ShowHpPotionEffectOn(Transform parent)
    {
        GameObject go = Managers.Resource.Instantiate("HP Potion Ef");
        if (go == null)
        {
            Debug.LogWarning("[ObjectManager] FX prefab not found: HP Potion Ef. Addressable PreLoad 필요.");
            return;
        }
        go.transform.SetParent(parent, worldPositionStays: false);   // 프리팹 로컬 위치 유지
        Object.Destroy(go, FX_LIFETIME);
    }

    // 기획 수정안(2026-07-05): 랭크업 연출 — Player Rank Up Ef를 부모(플레이어) 아래 생성.
    // 재생 속도/타이밍은 프리팹 저작값 그대로 사용 (코드 보정 안 함).
    // 위치는 캐릭터 발밑 기준: 프리팹 로컬 오프셋을 쓰면 머리쪽에 떠서, 스프라이트 바운즈 하단(y)에 정렬한다.
    const float RANKUP_FX_LIFETIME = 4f;
    public void ShowRankUpEffectOn(Transform parent)
    {
        GameObject go = Managers.Resource.Instantiate("Player Rank Up Ef");
        if (go == null)
        {
            Debug.LogWarning("[ObjectManager] FX prefab not found: Player Rank Up Ef. Addressable PreLoad 필요.");
            return;
        }
        go.transform.SetParent(parent, worldPositionStays: false);

        var sr = parent.GetComponentInChildren<SpriteRenderer>();
        float footY = sr != null ? sr.bounds.min.y : parent.position.y;
        go.transform.position = new Vector3(parent.position.x, footY, parent.position.z);

        Object.Destroy(go, RANKUP_FX_LIFETIME);
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
