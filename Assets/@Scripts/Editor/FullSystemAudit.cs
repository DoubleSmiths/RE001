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
            EditorUtility.DisplayDialog("종합 검증", "플레이 모드에서 실행하세요.", "확인");
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

        var sb = new StringBuilder();
        sb.AppendLine("═══ 종합 시스템 검증 ═══");
        foreach (string n in note) sb.AppendLine("  · " + n);
        sb.AppendLine($"실패 : {fail.Count}   소요 : {Time.realtimeSinceStartup - t0:F1}s");
        for (int i = 0; i < fail.Count && i < 40; i++) sb.AppendLine("  ✗ " + fail[i]);
        if (fail.Count > 40) sb.AppendLine($"  … 외 {fail.Count - 40}건");

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
        };
        var head = new StringBuilder("데이터 로드:");
        foreach (var (name, count, min) in required)
        {
            head.Append($" {name}={count}");
            if (count < min) fail.Add($"[데이터] {name} {count}행 (최소 {min} 기대) — Addressable/파싱 확인");
        }
        note.Add(head.ToString());

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

        g.SaveGame();
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

            float delta = p.PhysicalAtk - bare;
            float expect = data.BaseAtk + EquipGrantedAtk(eq.DataId);
            if (Mathf.Abs(delta - expect) > 0.01f)
            {
                mismatch++;
                if (mismatch <= 3)
                    fail.Add($"[스탯] 장비 {eq.DataId} 물공 기여 {delta:F0} ≠ 기대 {expect:F0} " +
                             $"(BaseAtk {data.BaseAtk} + 고유능력 {EquipGrantedAtk(eq.DataId):F0})");
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
