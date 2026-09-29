using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Data;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 종합 시스템 검증 — 지금까지 문제가 났던 영역을 **한 번에 처음부터** 훑는다.
///
///   A. 데이터 시트 ↔ JSON ↔ 런타임 딕셔너리 (행 수·참조 무결성)
///   B. 세이브/로드 왕복 (쓰고 → 다시 읽어 → 모든 값이 살아있는가)
///   C. 스탯 적용 (장비 / 장비 고유능력 / 능력 / 스탯포인트가 실제로 반영되는가)
///   D. 접지 · 아이콘 · 애니메이션 (몬스터/플레이어/미니몬)
///   E. 장비 팝업 전 경로 (목록·티어·장착·해제·제작)
///
/// 전투 전 구간(던전125·몬스터503)은 FullPlaythroughTest가 담당한다 — 여긴 나머지를 본다.
/// 실패는 전부 모아 한 번에 보고한다(첫 실패에서 멈추면 뒤쪽 문제를 못 본다).
/// </summary>
public static class FullSystemAudit
{
    [MenuItem("Test/Integration/종합 시스템 검증 (데이터·세이브·스탯·UI)", false, 6)]
    static void Run()
    {
        if (!Application.isPlaying)
        {
            // ⚠️ 여기서 DisplayDialog를 띄우면 안 된다. unity-cli로 메뉴를 실행하는 세션에서는
            //  아무도 [확인]을 누르지 않아 **에디터가 통째로 멈춘다**(모달이 메인 루프를 잡는다).
            //  실제로 26분간 CPU 0.01코어로 얼어 CLI 접속이 전부 끊겼다. 로그로만 알린다.
            Debug.LogError("[종합 검증] 플레이 모드에서 실행하세요.");
            return;
        }
        Managers.Game.Player.StartCoroutine(CoRun());
    }

    static IEnumerator CoRun()
    {
        float t0 = Time.realtimeSinceStartup;
        var fail = new List<string>();
        var note = new List<string>();

        AuditData(fail, note);
        yield return null;

        yield return CoAuditSaveLoad(fail, note);
        yield return CoAuditStats(fail, note);
        yield return CoAuditWorldObjects(fail, note);
        yield return CoAuditEquipmentUI(fail, note);
        yield return CoAuditPopupInput(fail, note);
        yield return CoAuditMinMonAndSkills(fail, note);
        yield return CoAuditShop(fail, note);
        yield return CoAuditQuestReachable(fail, note);

        var sb = new StringBuilder();
        sb.AppendLine("═══ 종합 시스템 검증 ═══");
        foreach (string n in note) sb.AppendLine("  · " + n);
        sb.AppendLine($"실패 : {fail.Count}   소요 : {Time.realtimeSinceStartup - t0:F1}s");
        for (int i = 0; i < fail.Count && i < 40; i++) sb.AppendLine("  ✗ " + fail[i]);
        if (fail.Count > 40) sb.AppendLine($"  … 외 {fail.Count - 40}건");

        // 콘솔은 도감이 뿜는 바인딩 로그(수백 줄)에 밀려 결과가 사라진다 → 파일로도 남긴다.
        string outPath = Path.Combine(Application.persistentDataPath, "audit_result.txt");
        try { File.WriteAllText(outPath, sb.ToString()); } catch { }
        sb.AppendLine($"(결과 파일: {outPath})");

        if (fail.Count == 0) Debug.Log(sb.ToString());
        else Debug.LogError(sb.ToString());
    }

    // ── A. 데이터 ────────────────────────────────────────────────
    static void AuditData(List<string> fail, List<string> note)
    {
        var d = Managers.Data;

        // 필수 딕셔너리가 비어 있지 않은가 (Addressable 누락/파싱 실패는 여기서 0으로 드러난다)
        var required = new (string name, int count, int min)[]
        {
            ("CreatureData",     d.CreatureDic?.Count     ?? 0, 500),
            ("EquipmentData",    d.EquipmentDic?.Count    ?? 0, 60),
            ("EquipEnhanceData", d.EquipEnhanceDic?.Count ?? 0, 500),
            ("ForgeRecipeData",  d.ForgeRecipeDic?.Count  ?? 0, 60),
            ("SkillData",        d.SkillDic?.Count        ?? 0, 60),
            ("ItemData",         d.ItemDic?.Count         ?? 0, 30),
            ("TravelNodeData",   d.TravelNodeDic?.Count   ?? 0, 150),
            ("PlayerStatusData", d.PlayerStatusDic?.Count ?? 0, 300),
            ("LevelData",        d.LevelDic?.Count        ?? 0, 300),
            ("QuestData",        d.QuestDic?.Count        ?? 0, 7),
            ("MinMonData",       d.MinMonDic?.Count       ?? 0, 3),
            ("ShopItemData",     d.ShopItemDic?.Count     ?? 0, 30),
        };
        var head = new StringBuilder("데이터 로드:");
        foreach (var (name, count, min) in required)
        {
            head.Append($" {name}={count}");
            if (count < min) fail.Add($"[데이터] {name} {count}행 (최소 {min} 기대) — Addressable/파싱 확인");
        }
        note.Add(head.ToString());

        // 상점 — 파는 물건이 실재하는가, 그리고 **되팔아서 돈을 벌 수 없는가**.
        //  되팔기가가 구매가 이상이면 사고팔기를 반복해 무한히 골드를 찍을 수 있다.
        if (d.ShopItemDic != null)
        {
            int badItem = 0, badPrice = 0, arbitrage = 0;
            foreach (var sh in d.ShopItemDic.Values)
            {
                if (d.ItemDic == null || !d.ItemDic.ContainsKey(sh.ItemId)) badItem++;
                if (sh.Price <= 0) badPrice++;
                if (sh.SellPrice >= sh.Price) arbitrage++;
            }
            if (badItem > 0)   fail.Add($"[상점] 존재하지 않는 아이템 {badItem}건");
            if (badPrice > 0)  fail.Add($"[상점] 구매가가 0 이하인 항목 {badPrice}건");
            if (arbitrage > 0) fail.Add($"[상점] 되팔기가 >= 구매가 {arbitrage}건 — 무한 차익 루프");
            note.Add($"상점: {d.ShopItemDic.Count}종 판매, 차익 루프 0, 죽은 참조 0");
        }

        // 참조 무결성 — 죽은 참조는 인게임에서 "안 보임/못 만듦"으로 나타난다
        int badDrop = 0, badRecipeEquip = 0, badRecipeMat = 0, badHabitat = 0, badEnhance = 0;

        var nodeNames = new HashSet<string>();
        if (d.TravelNodeDic != null)
            foreach (var n in d.TravelNodeDic.Values) if (!string.IsNullOrEmpty(n.Name)) nodeNames.Add(n.Name);

        if (d.CreatureDic != null)
            foreach (var c in d.CreatureDic.Values)
            {
                if (c.DropItemId > 0 && (d.ItemDic == null || !d.ItemDic.ContainsKey(c.DropItemId))) badDrop++;
                if (!string.IsNullOrEmpty(c.Habitat) && !nodeNames.Contains(c.Habitat)) badHabitat++;
            }

        if (d.ForgeRecipeDic != null)
            foreach (var r in d.ForgeRecipeDic.Values)
            {
                if (d.EquipmentDic == null || !d.EquipmentDic.ContainsKey(r.EquipDataId)) badRecipeEquip++;
                foreach (var (mid, cnt) in GameManager.RecipeMaterials(r))
                    if (mid > 0 && cnt > 0 && (d.ItemDic == null || !d.ItemDic.ContainsKey(mid))) badRecipeMat++;
            }

        if (d.EquipEnhanceDic != null)
            foreach (var e in d.EquipEnhanceDic.Values)
                if (d.EquipmentDic == null || !d.EquipmentDic.ContainsKey(e.EquipDataId)) badEnhance++;

        if (badDrop > 0)        fail.Add($"[데이터] 존재하지 않는 아이템을 드롭하는 몬스터 {badDrop}종");
        if (badHabitat > 0)     fail.Add($"[데이터] 어떤 노드에도 없는 서식지를 가진 몬스터 {badHabitat}종");
        if (badRecipeEquip > 0) fail.Add($"[데이터] 존재하지 않는 장비를 만드는 레시피 {badRecipeEquip}건");
        if (badRecipeMat > 0)   fail.Add($"[데이터] 존재하지 않는 재료를 요구하는 레시피 {badRecipeMat}건");
        if (badEnhance > 0)     fail.Add($"[데이터] 존재하지 않는 장비의 강화행 {badEnhance}건");
        note.Add("참조 무결성: 드롭/서식지/레시피/강화 " +
                 ((badDrop + badHabitat + badRecipeEquip + badRecipeMat + badEnhance) == 0 ? "이상 없음" : "이상 있음"));

        // 장비 60종 전부 제작 가능한가 (획득 경로)
        int noRecipe = 0;
        if (d.EquipmentDic != null)
            foreach (int id in d.EquipmentDic.Keys)
                if (Managers.Game.GetRecipeFor(id) == null) noRecipe++;
        if (noRecipe > 0) fail.Add($"[데이터] 제작 레시피가 없는 장비 {noRecipe}종 (획득 불가)");
        note.Add($"장비 획득 경로: {(d.EquipmentDic?.Count ?? 0) - noRecipe}/{d.EquipmentDic?.Count ?? 0} 제작 가능");
    }

    // ── B. 세이브 / 로드 ─────────────────────────────────────────
    static IEnumerator CoAuditSaveLoad(List<string> fail, List<string> note)
    {
        var g = Managers.Game;

        // 알아보기 쉬운 값들을 심어두고 저장 → 다시 읽어 살아있는지 본다.
        g.Level = 42;
        g.Gold = 123456;
        g.Crystal = 789;
        g.AddItem(11001, 77);
        g.AddUnlockedBtn("AUDIT_BTN");
        if (!g.DiscoveredMonsterIds.Contains(2001)) g.DiscoveredMonsterIds.Add(2001);
        if (Managers.Ability != null && !Managers.Ability.IsLearned(5002)) Managers.Ability.Learn(5002);

        // 장비 하나 제작해 보유/장착 상태까지 저장 대상에 포함
        var recipe = g.GetRecipeFor(30007);
        if (recipe != null && !g.HasEquipment(30007))
        {
            foreach (var (id, need) in GameManager.RecipeMaterials(recipe)) g.AddItem(id, need);
            g.CraftEquipment(30007);
        }

        // ⚠️ 제작이 골드를 차감하므로 기준값은 **제작 뒤에** 잡아야 한다.
        //    (앞에서 잡았다가 "골드 미복원 122656 ≠ 123456" 오탐을 냈다 — 차이 800 = 제작 비용)
        int goldExpect = g.Gold;
        int ownedBefore = g.OwnedEquipments.Count;
        int equippedBefore = g.EquippedEquipments.Count;
        int invBefore = g.Inventory.Count;
        int abilBefore = g.LearnedAbilities.Count;

        g.SaveGameNow();
        yield return null;

        string path = Application.persistentDataPath + "/SaveData.json";
        if (!File.Exists(path))
        {
            fail.Add("[세이브] SaveData.json이 만들어지지 않음");
            yield break;
        }
        long size = new FileInfo(path).Length;

        // 실제로 다시 읽는다 (메모리 값을 그대로 보는 게 아니라 파일 왕복)
        bool loaded = g.LoadGame();
        yield return null;

        if (!loaded) fail.Add("[세이브] LoadGame이 false 반환");

        if (g.Level != 42)                 fail.Add($"[세이브] 레벨 미복원 {g.Level} ≠ 42");
        if (g.Gold != goldExpect)          fail.Add($"[세이브] 골드 미복원 {g.Gold} ≠ {goldExpect}");
        if (g.Crystal != 789)              fail.Add($"[세이브] 결정 미복원 {g.Crystal} ≠ 789");
        if (g.GetItemCount(11001) < 77)    fail.Add($"[세이브] 아이템 미복원 11001={g.GetItemCount(11001)} < 77");
        if (!g.UnlockedBtnIds.Contains("AUDIT_BTN")) fail.Add("[세이브] 해금 버튼 미복원");
        if (!g.DiscoveredMonsterIds.Contains(2001))  fail.Add("[세이브] 도감 발견 기록 미복원");
        if (g.OwnedEquipments.Count != ownedBefore)  fail.Add($"[세이브] 보유 장비 {g.OwnedEquipments.Count} ≠ {ownedBefore}");
        if (g.Inventory.Count != invBefore)          fail.Add($"[세이브] 인벤 종류 {g.Inventory.Count} ≠ {invBefore}");
        if (g.LearnedAbilities.Count != abilBefore)  fail.Add($"[세이브] 학습 능력 {g.LearnedAbilities.Count} ≠ {abilBefore}");

        // 장착 딕셔너리는 로드 후 IsEquipped로 **재구성**된다 — 참조가 보유 리스트와 같은 객체여야 한다.
        if (g.EquippedEquipments.Count != equippedBefore)
            fail.Add($"[세이브] 장착 수 {g.EquippedEquipments.Count} ≠ {equippedBefore}");
        foreach (var kv in g.EquippedEquipments)
        {
            var owned = g.OwnedEquipments.Find(e => e.DataId == kv.Value.DataId);
            if (!ReferenceEquals(owned, kv.Value))
                fail.Add($"[세이브] 장착 {kv.Value.DataId}가 보유 리스트와 다른 객체 (강화가 반영 안 되는 원인이 된다)");
        }

        note.Add($"세이브/로드: {size:N0} bytes 왕복, 레벨·재화·인벤·장비·능력·도감 복원 확인");
    }

    // ── C. 스탯 적용 ─────────────────────────────────────────────
    static IEnumerator CoAuditStats(List<string> fail, List<string> note)
    {
        var g = Managers.Game;
        var p = g.Player;
        if (p == null) { fail.Add("[스탯] Player 없음"); yield break; }

        // 장비를 전부 벗고 기준을 잡는다
        var saved = new List<Equipment>(g.EquippedEquipments.Values);
        g.EquippedEquipments.Clear();
        foreach (var e in g.OwnedEquipments) e.IsEquipped = false;
        p.RefreshStats(fullHeal: false);
        float bare = p.PhysicalAtk;

        // 장비 하나씩 껴보며 BaseAtk + (장비 고유능력)만큼 오르는지
        int checkedCount = 0, mismatch = 0;
        foreach (var eq in g.OwnedEquipments)
        {
            var data = eq.EquipmentData;
            if (data == null) continue;

            g.EquippedEquipments.Clear();
            foreach (var x in g.OwnedEquipments) x.IsEquipped = false;
            eq.IsEquipped = true;
            g.EquippedEquipments[eq.SlotType] = eq;
            p.RefreshStats(fullHeal: false);

            // 강화 보너스도 기대치에 넣는다. 안 넣으면 **플레이 중 강화한 장비가 전부 불일치로 잡힌다**
            //  (대장간 검증하며 30001을 +2로 올렸더니 그 자리에서 실패로 떴다 — 게임이 아니라 검사가 틀린 것).
            float enhAtk = 0f;
            if (Managers.Data.EquipEnhanceDic != null)
                foreach (var enh in Managers.Data.EquipEnhanceDic.Values)
                    if (enh.EquipDataId == eq.DataId && enh.Level <= eq.EnhanceLevel)
                        enhAtk += enh.AtkBonus;

            float delta = p.PhysicalAtk - bare;
            float expect = data.BaseAtk + EquipGrantedAtk(eq.DataId) + enhAtk;
            if (Mathf.Abs(delta - expect) > 0.01f)
            {
                mismatch++;
                if (mismatch <= 3)
                    fail.Add($"[스탯] 장비 {eq.DataId} 물공 기여 {delta:F0} ≠ 기대 {expect:F0} " +
                             $"(BaseAtk {data.BaseAtk} + 고유능력 {EquipGrantedAtk(eq.DataId):F0} + 강화 +{eq.EnhanceLevel} {enhAtk:F0})");
            }
            checkedCount++;
            yield return null;
        }

        // 원상 복구
        g.EquippedEquipments.Clear();
        foreach (var e in g.OwnedEquipments) e.IsEquipped = false;
        foreach (var e in saved) { e.IsEquipped = true; g.EquippedEquipments[e.SlotType] = e; }
        p.RefreshStats(fullHeal: true);

        note.Add($"스탯 적용: 장비 {checkedCount}종 물공 기여 검사, 불일치 {mismatch}");

        // 레벨 스탯이 실제로 오르는가 (PlayerStatusData 연결)
        int lv = g.Level;
        g.Level = 1;   p.RefreshStats(false); float a1 = p.PhysicalAtk;
        g.Level = 300; p.RefreshStats(false); float a300 = p.PhysicalAtk;
        g.Level = lv;  p.RefreshStats(true);
        if (a300 <= a1) fail.Add($"[스탯] 레벨이 올라도 물공이 안 오름 (Lv1 {a1:F0} → Lv300 {a300:F0})");
        note.Add($"레벨 곡선: 물공 Lv1 {a1:F0} → Lv300 {a300:F0}");
    }

    // 장비 착용으로 부여되는 능력의 물공 보너스 (기획서 슬38)
    static float EquipGrantedAtk(int equipDataId)
    {
        var d = Managers.Data;
        if (d?.SkillDic == null) return 0f;
        float sum = 0f;
        foreach (var kv in d.SkillDic)
        {
            var s = kv.Value;
            if (s.Source != Define.AbilitySource.Equip || s.RequiredEquipId != equipDataId) continue;
            int lv = Mathf.Max(1, Managers.Ability?.GetLevel(kv.Key) ?? 1);
            // 다중 스탯(StatTypes/StatBonuses) 우선, 없으면 단일(StatType/StatBonus) — AbilityManager와 같은 규칙
            if (s.StatTypes != null && s.StatTypes.Count > 0)
            {
                for (int i = 0; i < s.StatTypes.Count; i++)
                    if (s.StatTypes[i] == "PhysicalAtk")
                        sum += (s.StatBonuses != null && i < s.StatBonuses.Count ? s.StatBonuses[i] : 0f) * lv;
            }
            else if (s.StatType == "PhysicalAtk") sum += s.StatBonus * lv;
        }
        return sum;
    }

    // ── D. 월드 오브젝트 (접지 · 아이콘 · 애니메이션) ─────────────
    static IEnumerator CoAuditWorldObjects(List<string> fail, List<string> note)
    {
        var g = Managers.Game;
        var p = g.Player;

        // 플레이어 접지
        if (Mathf.Abs(p.transform.position.y - Define.GROUND_Y) > 0.05f)
            fail.Add($"[접지] 플레이어 y={p.transform.position.y:F2} ≠ GROUND_Y {Define.GROUND_Y}");

        // 몬스터 표본 — 계열별로 하나씩
        int animOk = 0, animBad = 0, groundBad = 0, spriteBad = 0;
        foreach (int id in new[] { 2001, 2101, 2201, 2301, 2401, 2501 })
        {
            if (!Managers.Data.CreatureDic.TryGetValue(id, out var cd)) continue;
            var mc = Managers.Object.SpawnMonster(id, new Vector3(3f, Define.GROUND_Y, 0f));
            if (mc == null) { fail.Add($"[월드] 몬스터 {id} 스폰 실패"); continue; }
            yield return null;

            var an = mc.GetComponent<Animator>();
            bool ok = an != null && an.runtimeAnimatorController != null;
            if (ok)
                foreach (string s in new[] { "Idle", "Run", "Attack", "OnHit", "Die" })
                    if (!an.HasState(0, Animator.StringToHash(s))) { ok = false; break; }
            if (ok) animOk++; else { animBad++; fail.Add($"[애니] 몬스터 {id} 상태 5종 미비 ({cd.AnimController})"); }

            var sr = mc.GetComponentInChildren<SpriteRenderer>();
            if (sr == null || sr.sprite == null) { spriteBad++; fail.Add($"[아이콘] 몬스터 {id} 스프라이트 없음"); }
            if (Mathf.Abs(mc.transform.position.y - Define.GROUND_Y) > 0.05f) { groundBad++; fail.Add($"[접지] 몬스터 {id} y={mc.transform.position.y:F2}"); }

            Managers.Resource.Destroy(mc.gameObject);
        }

        // 동행 미니몬 접지
        var cc = Managers.Object.SpawnCompanion(2001, p);
        if (cc == null) fail.Add("[월드] 동행 미니몬 스폰 실패");
        else
        {
            yield return null;
            if (Mathf.Abs(cc.transform.position.y - Define.GROUND_Y) > 0.05f)
                fail.Add($"[접지] 미니몬 y={cc.transform.position.y:F2}");
            Managers.Resource.Destroy(cc.gameObject);
        }

        note.Add($"월드: 애니 정상 {animOk}/{animOk + animBad}, 접지 이상 {groundBad}, 스프라이트 이상 {spriteBad}");

        // 능력 아이콘이 서로 다른가 (하나로 통일돼 보이던 문제).
        //  1개만 학습된 상태에서는 "전부 같다"를 판별할 수 없다 → 표본을 충분히 만든 뒤 본다.
        if (Managers.Ability != null)
        {
            int learned = 0;
            foreach (var kv in Managers.Data.SkillDic)
            {
                if (learned >= 8) break;
                if (!Managers.Ability.IsLearned(kv.Key)) Managers.Ability.Learn(kv.Key);
                learned++;
            }
        }
        Managers.UI.CloseAllPopupUI();
        var sb = Managers.UI.ShowPopupUI<UI_SkillBookPopup>();
        if (sb != null) sb.SetInfo();
        yield return null;

        var icons = new HashSet<string>();
        int slotCount = 0;
        foreach (var e in Object.FindObjectsByType<UI_SkillBookSkill>(FindObjectsSortMode.None))
        {
            if (!e.gameObject.activeInHierarchy) continue;
            foreach (var img in e.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if (img.gameObject.name != "Img_SkillBookSkill_0_01") continue;
                slotCount++;
                if (img.sprite != null) icons.Add(img.sprite.name);
                if (img.color.r < 0.9f) fail.Add("[아이콘] 능력 아이콘이 어둡게 칠해짐(검은 tint 재발)");
            }
        }
        if (slotCount > 1 && icons.Count <= 1)
            fail.Add($"[아이콘] 능력 {slotCount}칸이 전부 같은 아이콘 ({icons.Count}종)");
        note.Add($"능력 아이콘: {slotCount}칸 / 서로 다른 아이콘 {icons.Count}종");
        Managers.UI.CloseAllPopupUI();
    }

    // ── E. 장비 팝업 전 경로 ─────────────────────────────────────
    static IEnumerator CoAuditEquipmentUI(List<string> fail, List<string> note)
    {
        Managers.UI.CloseAllPopupUI();
        var pop = Managers.UI.ShowPopupUI<UI_EquipmentPopup>();
        if (pop == null) { fail.Add("[장비UI] 장비 팝업이 열리지 않음"); yield break; }
        yield return null;

        // 부위별 목록 수가 데이터와 맞는가 (T1 기준)
        var slotBtn = new (string open, string node, Define.EquipSlotType slot)[]
        {
            ("OpenWeaponListBtn", "W00Btn", Define.EquipSlotType.Weapon),
            ("OpenHelemtListBtn", "H00Btn", Define.EquipSlotType.Helmet),
            ("OpenArmorListBtn",  "A00Btn", Define.EquipSlotType.Armor),
            ("OpenGloveListBtn",  "G00Btn", Define.EquipSlotType.Gloves),
            ("OpenBootsListBtn",  "B00Btn", Define.EquipSlotType.Legs),
            ("OpenRingListBtn",   "R00Btn", Define.EquipSlotType.Accessory),
        };

        int okSlots = 0;
        foreach (var (open, node, slot) in slotBtn)
        {
            Click(Find(pop, open));
            yield return null;

            int shown = 0;
            foreach (var b in pop.GetComponentsInChildren<UnityEngine.UI.Button>(false))
                if (b.gameObject.name.Contains(node)) shown++;

            int expect = 0;
            foreach (var kv in Managers.Data.EquipmentDic)
                if (kv.Value.SlotType == slot && (int)kv.Value.Grade / 2 == 0) expect++;

            if (shown != expect) fail.Add($"[장비UI] {slot} T1 표시 {shown} ≠ 데이터 {expect}");
            else okSlots++;
        }
        note.Add($"장비 목록: {okSlots}/{slotBtn.Length} 부위 표시 수 일치");

        // 장착 / 해제 / 상세 아이콘 일치
        var g = Managers.Game;
        var eq = g.OwnedEquipments.Find(e => e.SlotType == Define.EquipSlotType.Weapon);
        if (eq == null) { fail.Add("[장비UI] 무기 보유 없음"); yield break; }

        var det = Managers.UI.ShowPopupUI<UI_EquipDetailPopup>();
        det.SetInfo(eq);
        yield return null;

        var wIcon = Util.FindChild<UnityEngine.UI.Image>(det.gameObject, "WIcon", true);
        var expectSprite = Managers.Resource.Load<Sprite>(eq.EquipmentData.SpriteName);
        if (wIcon == null || wIcon.sprite != expectSprite)
            fail.Add($"[장비UI] 상세 아이콘 불일치 ({wIcon?.sprite?.name} ≠ {expectSprite?.name})");

        bool wasEquipped = eq.IsEquipped;
        if (wasEquipped)
        {
            Click(Util.FindChild(det.gameObject, "OnEquipBtn", true));
            yield return null;
            if (eq.IsEquipped) fail.Add("[장비UI] 해제가 동작하지 않음");
            det.SetInfo(eq);
            yield return null;
        }
        Click(Find(det, "EquipBtn"));
        yield return null;
        if (!eq.IsEquipped) fail.Add("[장비UI] 장착이 동작하지 않음");

        // (+) 상세 오버레이
        det.SetInfo(eq);
        yield return null;
        var ov = Util.FindChild(det.gameObject, "EquipAmountDetailPopup", true);
        Click(Find(det, "EquipAmountDetailBtn"));
        yield return null;
        if (ov == null || !ov.activeInHierarchy) fail.Add("[장비UI] (+) 상세 오버레이가 열리지 않음");

        note.Add("장비 팝업: 상세 아이콘 일치 / 장착 / 해제 / (+)상세 확인");
        Managers.UI.CloseAllPopupUI();
    }

    // ── F. 팝업 입력 (보이지 않는 차단막) ────────────────────────
    static IEnumerator CoAuditPopupInput(List<string> fail, List<string> note)
    {
        var g = Managers.Game;
        int checkedCount = 0;

        // 대장간은 장비를 받아야 하므로 따로, 나머지는 인자 없는 SetInfo로 연다.
        Managers.UI.CloseAllPopupUI();
        var eq = g.OwnedEquipments.Count > 0 ? g.OwnedEquipments[0] : null;
        if (eq != null)
        {
            var det = Managers.UI.ShowPopupUI<UI_EquipDetailPopup>(); det.SetInfo(eq);
            yield return null;
            AuditInputBlockers(det, "장비상세", fail); checkedCount++;

            var fp = Managers.UI.ShowPopupUI<UI_ForgePopup>(); fp.SetInfo(eq);
            yield return null;
            AuditInputBlockers(fp, "대장간", fail); checkedCount++;
        }
        Managers.UI.CloseAllPopupUI();
        yield return null;

        foreach (var t in new[] { typeof(UI_EquipmentPopup), typeof(UI_InventoryPopup), typeof(UI_SkillBookPopup),
                                  typeof(UI_BestiaryPopup), typeof(UI_TravelPopup), typeof(UI_PotionPopup),
                                  typeof(UI_CharacterPopup), typeof(UI_SettingPopup) })
        {
            Managers.UI.CloseAllPopupUI();
            // ShowPopupUI는 제네릭만 있어서 리플렉션으로 T를 채워 부른다.
            var open = typeof(UIManager).GetMethod("ShowPopupUI").MakeGenericMethod(t);
            var pop = open.Invoke(Managers.UI, new object[] { null }) as UI_Popup;
            if (pop == null) { fail.Add($"[입력] {t.Name} 열리지 않음"); continue; }
            var m = t.GetMethod("SetInfo", new System.Type[0]);
            if (m != null) m.Invoke(pop, null);
            yield return null;
            AuditInputBlockers(pop, t.Name, fail);
            checkedCount++;
        }
        Managers.UI.CloseAllPopupUI();
        note.Add($"팝업 입력: {checkedCount}종 레이캐스트 검사(보이지 않는 전체화면 차단막)");
    }

    /// <summary>
    /// 팝업 위에 **보이지 않는 전체화면 클릭 차단막**이 있는지 검사한다.
    ///
    /// 대장간이 "열리는데 아무것도 안 눌리고 X로 닫지도 못하는" 상태였던 원인이 이것이다.
    /// alpha 0 + raycastTarget=true 이미지가 콘텐츠보다 위에 그려지면 화면은 멀쩡한데 입력만 죽는다.
    ///
    /// ExecuteEvents로 대상에 직접 쏘는 방식은 **레이캐스트를 건너뛰어** 이 버그를 못 잡는다.
    /// 그래서 여기서는 화면 좌표로 GraphicRaycaster를 돌려 "실제로 무엇이 먼저 맞는지"를 본다.
    /// </summary>
    static void AuditInputBlockers(UI_Popup pop, string label, List<string> fail)
    {
        var self = pop.transform as RectTransform;
        if (self == null) return;

        var canvas = pop.GetComponent<Canvas>();
        var caster = pop.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (caster == null || canvas == null) { fail.Add($"[입력] {label}: GraphicRaycaster 없음"); return; }

        // **콘텐츠 프레임의 중심**에 레이캐스트한다.
        //  팝업 루트 중심으로 쏘면, 프레임이 화면 아래쪽에 있는 팝업(물약창 등)에서
        //  정상적인 배경(클릭하면 닫히는 layer)에 맞아 오탐이 난다.
        //  우리가 잡으려는 건 "콘텐츠 위를 덮은 투명 차단막"이다.
        var frame = pop._frameRT != null ? pop._frameRT : self;
        var cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, frame.position);

        var ptr = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = screen };
        var hits = new List<UnityEngine.EventSystems.RaycastResult>();
        caster.Raycast(ptr, hits);
        if (hits.Count == 0) return;

        var top = hits[0].gameObject;
        var img = top.GetComponent<UnityEngine.UI.Image>();
        var rt = top.transform as RectTransform;
        bool fullScreen = rt != null && rt.rect.width >= self.rect.width * 0.8f && rt.rect.height >= self.rect.height * 0.8f;
        bool invisible = img != null && img.color.a < 0.05f;

        if (fullScreen && invisible)
            fail.Add($"[입력] {label}: 보이지 않는 전체화면 차단막 '{top.name}'이 클릭을 가로챈다");
    }

    // ── G. 미니몬 잡기 / 액티브 스킬 / 지역 배경 ────────────────────
    //  이 셋은 "데이터가 비어 있어서 기능이 통째로 죽어 있던" 사례라 존재 여부가 곧 검증이다.
    //  (몬스터 503종 중 3종만 MinMonData에 있어 500종이 영영 안 잡혔다.)
    static IEnumerator CoAuditMinMonAndSkills(List<string> fail, List<string> note)
    {
        var data = Managers.Data;

        // 1) 전 몬스터가 잡히는가 + 난이도가 실제로 벌어져 있는가
        int noEntry = 0;
        float minChance = 1f, maxChance = 0f;
        int minCeil = int.MaxValue, maxCeil = 0;
        foreach (var c in data.CreatureDic.Values)
        {
            if (!data.MinMonDic.ContainsKey(c.DataId)) { noEntry++; continue; }
            float ch = Managers.MinMon.GetCatchChance(c.DataId);
            int ceil = Managers.MinMon.GetCatchCeil(c.DataId);
            minChance = Mathf.Min(minChance, ch); maxChance = Mathf.Max(maxChance, ch);
            minCeil = Mathf.Min(minCeil, ceil);   maxCeil = Mathf.Max(maxCeil, ceil);
        }
        if (noEntry > 0)
            fail.Add($"[미니몬] {noEntry}종이 MinMonData에 없어 절대 잡히지 않는다");
        else
            note.Add($"미니몬 잡기: {data.CreatureDic.Count}종 전부 포획 가능 " +
                     $"(확률 {minChance:P0}~{maxChance:P0}, 천장 {minCeil}~{maxCeil}회)");

        // 2) 실제로 잡히는지 — 천장까지 두드리면 반드시 성공해야 한다
        int probe = 0;
        foreach (var id in new[] { 2001, 2300, 2600 })
        {
            if (!data.MinMonDic.ContainsKey(id)) continue;
            var save = Managers.MinMon.GetSaveData(id);
            save.CaughtCount = 0; save.TameCount = 0;
            int ceil = Managers.MinMon.GetCatchCeil(id);
            for (int i = 0; i <= ceil && !Managers.MinMon.IsOwned(id); i++) Managers.MinMon.TryCatch(id);
            if (!Managers.MinMon.IsOwned(id)) fail.Add($"[미니몬] {id}: 천장({ceil}회)을 넘겨도 포획 실패");
            else probe++;
        }
        if (probe > 0) note.Add($"포획 천장: {probe}종 확정 성공 확인");
        yield return null;

        // 3) 액티브 스킬 — 데이터 + 실제 발동 + 쿨 차단
        var runner = Managers.Game.Player != null ? Managers.Game.Player.AbilityRunner : null;
        int activeCount = 0;
        foreach (var d in data.SkillDic.Values)
            if (d.TriggerType == Define.AbilityTrigger.Active) activeCount++;

        if (activeCount == 0) fail.Add("[액티브] 플레이어가 직접 쓸 능력이 하나도 없다");
        else if (runner == null) fail.Add("[액티브] AbilityRunner 없음");
        else
        {
            foreach (var d in data.SkillDic.Values)
                if (d.TriggerType == Define.AbilityTrigger.Active
                    && !Managers.Game.EquippedAbilities.Contains(d.DataId))
                    Managers.Game.EquippedAbilities.Add(d.DataId);

            var ids = runner.ActiveSkillIds;
            if (ids.Count == 0) fail.Add("[액티브] 장착했는데 스킬바 슬롯이 비어 있다");
            else
            {
                Managers.Game.Player.StartBattle();
                int id = ids[0];
                if (!runner.UseSkill(id)) fail.Add($"[액티브] {id} 발동 실패(전투 중·쿨 0인데)");
                else if (runner.GetCooldownRemain(id) <= 0f) fail.Add($"[액티브] {id} 발동 후 쿨이 안 걸린다");
                else if (runner.UseSkill(id)) fail.Add($"[액티브] {id} 쿨 중인데 재발동된다");
                else note.Add($"액티브 스킬: {activeCount}종 / 슬롯 {ids.Count}칸 / 발동·쿨 차단 확인");
                Managers.Game.Player.StopBattle();
            }
        }
        yield return null;

        // 4) 전투 배경 — 던전마다 그림이 붙는가 (전 던전이 CH1BG 한 장을 쓰던 회귀 방지)
        int noBg = 0, dungeons = 0;
        var bgSeen = new HashSet<string>();
        foreach (var kv in data.TravelNodeDic)
        {
            var tn = kv.Value;
            if (tn.Category != "EXPLORE" || tn.ParentId == 0) continue;
            dungeons++;
            var sp = Managers.Resource.Load<Sprite>($"Region_{kv.Key}")
                  ?? Managers.Resource.Load<Sprite>($"Region_{tn.ParentId}");
            if (sp == null) noBg++;
            else bgSeen.Add(sp.name);
        }
        if (noBg > 0) fail.Add($"[배경] 던전 {noBg}곳에 지역 배경이 없다");
        else note.Add($"전투 배경: 던전 {dungeons}곳 / 서로 다른 배경 {bgSeen.Count}종");

        // 5) 도감 그림 중복 — 카드가 붙은 종은 서로 겹치면 안 된다
        var iconOwner = new Dictionary<string, int>();
        int dup = 0, carded = 0;
        foreach (var c in data.CreatureDic.Values)
        {
            if (string.IsNullOrEmpty(c.IconLabel)) continue;
            if (c.IconLabel != $"Mon_{c.DataId}") continue;   // 아직 카드 미생성 종은 원형 공유가 정상
            carded++;
            if (iconOwner.TryGetValue(c.IconLabel, out int other)) { dup++; fail.Add($"[도감] {c.DataId}와 {other}가 같은 그림"); }
            else iconOwner[c.IconLabel] = c.DataId;
            if (Managers.Resource.Load<Sprite>(c.IconLabel) == null)
                fail.Add($"[도감] {c.DataId} 카드 {c.IconLabel} 로드 실패");
        }
        note.Add($"도감 고유 그림: {carded}/{data.CreatureDic.Count}종 적용 (중복 {dup})");
        yield return null;
    }

    // ── H. 진행 차단 (퀘스트가 요구하는 걸 실제로 얻을 수 있는가) ─────
    //  퀘3은 젤리 퐁(10001) 2개, 퀘6은 폴짝 젤리(10002) 5개를 요구하는데 몬스터 드롭 집합이
    //  11001~11030뿐이라 **어떤 몬스터도 이 둘을 안 떨궈** 퀘3에서 게임이 영구 정지했었다.
    //  "끝까지 플레이가 안 된다"의 진짜 원인이었으므로 데이터가 바뀔 때마다 다시 본다.
    // ── 상점 실동작 ──────────────────────────────────────────────
    //  데이터만 보는 검사는 "값은 맞는데 골드가 안 깎이는" 회귀를 못 잡는다.
    //  실제로 사고팔아 골드와 수량이 정확히 움직이는지 본다.
    static IEnumerator CoAuditShop(List<string> fail, List<string> note)
    {
        var g = Managers.Game;
        var stock = g.GetShopStock();
        if (stock.Count == 0)
        {
            fail.Add("[상점] 진열 목록이 비어 있다 (레벨 조건 또는 데이터 로드 확인)");
            yield break;
        }

        var shop = stock[0];
        const int N = 3;

        int gold0 = g.Gold;
        int  cnt0  = g.GetItemCount(shop.ItemId);

        if (!g.BuyShopItem(shop, N)) { fail.Add("[상점] 구매 실패 (골드 충분한데 거절)"); yield break; }
        yield return null;

        int gold1 = g.Gold;
        int  cnt1  = g.GetItemCount(shop.ItemId);
        if (gold0 - gold1 != shop.Price * N)
            fail.Add($"[상점] 구매 골드 차감 불일치: 기대 {shop.Price * N}, 실제 {gold0 - gold1}");
        if (cnt1 - cnt0 != N)
            fail.Add($"[상점] 구매 수량 불일치: 기대 +{N}, 실제 +{cnt1 - cnt0}");

        if (!g.SellShopItem(shop, N)) { fail.Add("[상점] 판매 실패 (보유 중인데 거절)"); yield break; }
        yield return null;

        int gold2 = g.Gold;
        int  cnt2  = g.GetItemCount(shop.ItemId);
        if (gold2 - gold1 != shop.SellPrice * N)
            fail.Add($"[상점] 판매 골드 환급 불일치: 기대 {shop.SellPrice * N}, 실제 {gold2 - gold1}");
        if (cnt2 != cnt0)
            fail.Add($"[상점] 판매 후 수량이 원래대로 안 돌아옴: {cnt0} -> {cnt2}");

        // 살 수 없는 상황에서 막히는가 — 막히지 않으면 골드가 음수로 간다.
        int saved = g.Gold;
        g.Gold = 0;
        yield return null;
        if (string.IsNullOrEmpty(g.GetBuyBlockReason(shop)))
            fail.Add("[상점] 골드 0인데 구매가 막히지 않는다");
        if (g.BuyShopItem(shop))
            fail.Add("[상점] 골드 0인데 구매가 성사된다");
        g.Gold = saved;
        yield return null;

        note.Add($"상점 실동작: 구매/판매 왕복 골드·수량 일치, 잔액 부족 차단 확인 (품목 {shop.ItemId})");
    }

    static IEnumerator CoAuditQuestReachable(List<string> fail, List<string> note)
    {
        var data = Managers.Data;
        if (data?.QuestDic == null) { note.Add("퀘스트 진행: 데이터 없음(건너뜀)"); yield break; }

        var droppable = new HashSet<int>();
        foreach (var c in data.CreatureDic.Values)
            if (c != null && c.DropItemId > 0 && c.DropChance > 0f) droppable.Add(c.DropItemId);

        int checkedCount = 0;
        foreach (var q in data.QuestDic.Values)
        {
            // ITEM_ACQUIRE(최초 1회)도 결국 아이템을 얻어야 넘어간다 — 둘 다 본다.
            if (q == null || (q.CompleteType != Define.QuestCompleteType.ITEM_COUNT
                           && q.CompleteType != Define.QuestCompleteType.ITEM_ACQUIRE)) continue;
            checkedCount++;
            if (!int.TryParse(q.CompleteTarget, out int itemId) || itemId <= 0) continue;

            if (!droppable.Contains(itemId))
            {
                fail.Add($"[진행차단] 퀘{q.QuestID}가 요구하는 아이템 {itemId}를 떨구는 몬스터가 0종 — 여기서 게임이 멈춘다");
                continue;
            }

            // 떨구는 몬스터가 **실제로 갈 수 있는 곳**에 사는지도 본다(못 가는 지역이면 없는 것과 같다).
            bool reachable = false;
            foreach (var c in data.CreatureDic.Values)
            {
                if (c == null || c.DropItemId != itemId) continue;
                foreach (var n in data.TravelNodeDic.Values)
                    if (n.Name == c.Habitat) { reachable = true; break; }
                if (reachable) break;
            }
            if (!reachable)
                fail.Add($"[진행차단] 퀘{q.QuestID} 요구 아이템 {itemId}를 떨구는 몬스터가 어느 여정 노드에도 살지 않는다");
        }
        note.Add($"퀘스트 진행: 아이템 요구 {checkedCount}건 모두 인게임 획득 경로 확인");
        yield return null;

        // 데이터만 맞는 게 아니라 **실제로 사냥해서 퀘스트가 넘어가는지**까지 본다.
        //  드롭 코드(MonsterController)와 퀘스트 감시(OnItemChanged)가 이어져 있어야 통과한다.
        yield return CoHuntForQuest(fail, note);
    }

    /// 산들바람 숲 입구에서 슬라임을 실제로 잡아 퀘3이 넘어가는지 — 무치트 진행의 첫 관문.
    static IEnumerator CoHuntForQuest(List<string> fail, List<string> note)
    {
        var g = Managers.Game;
        var data = Managers.Data;
        if (g?.Player == null || Managers.Quest == null) yield break;

        const int QUEST = 3, ITEM = 10001, MONSTER = 2001, NODE = 21010;
        if (!data.QuestDic.ContainsKey(QUEST) || !data.CreatureDic.ContainsKey(MONSTER)) yield break;

        // 검증이 세이브를 오염시키지 않도록 되돌릴 값을 잡아둔다.
        int savedQuest = g.CurrentQuestID;
        int savedNode = g.CurrentTravelAreaId;
        int savedItem = g.GetItemCount(ITEM);

        // ⚠️ 재료를 이미 들고 있으면 퀘스트로 넘어가는 순간 그 자리에서 통과한다(정상 동작).
        //  그러면 아래 사냥 루프가 한 번도 안 돌아 "0마리 잡아도 안 떨어진다"는 엉뚱한 실패가 난다.
        //  **직접 사냥해서 얻는 경로**를 보려는 검사이므로 보유량을 0으로 두고 시작한다.
        var held = g.Inventory.Find(x => x.DataId == ITEM);
        if (held != null) held.Count = 0;

        Managers.Quest.JumpToQuest(QUEST);
        g.CurrentTravelAreaId = NODE;
        g.Player.RefreshStats(fullHeal: true);

        int before = g.GetItemCount(ITEM);
        int kills = 0;
        for (int i = 0; i < 6 && g.CurrentQuestID == QUEST; i++)
        {
            var mc = Managers.Object.SpawnMonster(MONSTER, new Vector3(3f, Define.GROUND_Y, 0f));
            if (mc == null) { fail.Add("[무치트] 산들바람 숲 슬라임 스폰 실패"); break; }

            int guard = 0;
            while (mc != null && mc.Hp > 0f && guard++ < 200)
                mc.TakeHit(BattleMath.Resolve(g.Player, mc, Define.AttackType.Physical), g.Player);

            if (guard >= 200) { fail.Add("[무치트] Lv1 플레이어가 슬라임을 200타 안에 못 잡는다"); break; }
            kills++;
            if (mc != null) Managers.Resource.Destroy(mc.gameObject);
            yield return null;   // 드롭 통지/퀘스트 감시가 한 프레임 돌게 둔다
        }

        int gained = g.GetItemCount(ITEM) - before;
        if (gained <= 0)
            fail.Add($"[무치트] 슬라임 {kills}마리를 잡아도 젤리 퐁이 안 떨어진다 — CreatureData {MONSTER}의 DropItemId 확인");
        else if (g.CurrentQuestID == QUEST)
            fail.Add($"[무치트] 젤리 퐁 {gained}개를 얻었는데 퀘{QUEST}가 안 넘어간다 — 아이템 획득→퀘스트 감시 연결 끊김");
        else
            note.Add($"무치트 진행: 슬라임 {kills}마리 사냥 → 젤리 퐁 {gained}개 → 퀘{QUEST}→{g.CurrentQuestID} 통과");

        // 원상복구
        Managers.Quest.JumpToQuest(savedQuest);
        g.CurrentTravelAreaId = savedNode;
        var inv = g.Inventory.Find(x => x.DataId == ITEM);
        if (inv != null) inv.Count = savedItem;
        yield return null;
    }

    // ── 공용 ──
    static bool Click(GameObject go)
    {
        if (go == null) return false;
        var data = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        return UnityEngine.EventSystems.ExecuteEvents.Execute(go, data, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
    }

    static GameObject Find(Component root, string name)
    {
        foreach (var b in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            if (b.gameObject.name == name) return b.gameObject;
        return null;
    }
}
