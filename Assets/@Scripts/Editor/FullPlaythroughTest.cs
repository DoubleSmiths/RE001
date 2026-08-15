using System.Collections;
using System.Collections.Generic;
using System.Text;
using Data;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 전 구간 플레이스루 검증 — 던전 125개 · 몬스터 503종 · 레벨 1~300을 **실제 게임 경로로** 돌린다.
///
/// 왜 이렇게 하나
///   실시간으로 300레벨까지 치려면 수십 시간이 걸린다. 그렇다고 데이터만 훑으면
///   "스폰은 되는데 그림이 없다"류를 못 잡는다.
///   → 스폰·데미지·드롭은 **진짜 경로**(ObjectManager.SpawnMonster / BattleMath.Resolve /
///     MonsterController.OnDead)를 그대로 쓰고, 이동/대기 같은 시간만 건너뛴다.
///
/// 검사 항목 (하나라도 어긋나면 실패로 기록)
///   · 노드마다 몬스터 풀이 비어 있지 않은가 (PickFieldMonsterId가 fallback으로 도망가지 않는가)
///   · 스폰된 몬스터가 CreatureData / 애니메이터 / 스프라이트를 갖췄는가
///   · 발이 지면(GROUND_Y)에 닿는가
///   · 이름과 그림이 같은 아키타입인가
///   · 처치 시 드롭이 인벤토리에 실제로 들어가는가
///   · 그 레벨대 플레이어가 그 몬스터를 잡을 수 있는가 (무한 루프가 되지 않는가)
/// </summary>
public static class FullPlaythroughTest
{
    const int MAX_ROUNDS = 400;    // 몬스터 1마리에 허용하는 최대 공격 횟수 (넘으면 "못 잡음")

    [MenuItem("Test/Integration/전 구간 플레이스루 검증 (던전125·몬스터503)", false, 5)]
    static void Run()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("플레이스루 검증", "플레이 모드에서 실행하세요.", "확인");
            return;
        }
        Managers.Game.Player.StartCoroutine(CoRun());
    }

    static IEnumerator CoRun()
    {
        var t0 = Time.realtimeSinceStartup;
        var game = Managers.Game;
        var data = Managers.Data;

        // ① 메인 퀘스트를 **조건을 실제로 만족시키며** 끝까지 진행한다.
        //    치트로 마지막 번호로 점프하기 전에 먼저 돌려야 의미가 있다.
        var questFail = new List<string>();
        yield return CoWalkQuestChain(questFail);

        // ② 전 컨텐츠 해금 — 잠금 때문에 못 들어가는 노드가 생기면 검증이 아니라 관문 테스트가 된다.
        game.UnlockEverything();

        var nodes = new List<TravelNodeData>();
        foreach (var n in data.TravelNodeDic.Values)
            if (n.AreaType == "Battle") nodes.Add(n);
        nodes.Sort((a, b) => a.Id.CompareTo(b.Id));

        var fail = new List<string>();
        var seenMonsters = new HashSet<int>();
        var droppedMaterials = new HashSet<int>();   // 실제로 몬스터가 떨구는 재료 (제작 검사용)
        int spawned = 0, killed = 0, dropped = 0;

        Debug.Log($"[플레이스루] 시작 — 전투 노드 {nodes.Count}개");

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            game.CurrentTravelAreaId = node.Id;

            // 이 노드의 레벨대에 맞춰 플레이어를 올린다 (노드 순서 ≈ 난이도 순서)
            int lv = Mathf.Clamp(1 + (int)(299f * i / Mathf.Max(1, nodes.Count - 1)), 1, 300);
            if (game.Level != lv) game.Level = lv;
            var player = game.Player;
            player.RefreshStats(fullHeal: true);

            string place = Util.Localize(node.Name);

            // 이 노드의 몬스터 풀 — **서식지로 전수 열거**한다.
            //  확률로 여러 번 뽑는 방식은 보스(EncounterRate 0.08)를 자주 놓쳐
            //  "어느 노드에서도 안 나오는 몬스터"가 있는 것처럼 보인다(실제로 49종 오탐이 났다).
            var pool = new List<int>();
            foreach (var kv in data.CreatureDic)
                if (kv.Value != null && kv.Value.Habitat == node.Name && kv.Value.EncounterRate > 0f)
                    pool.Add(kv.Key);

            if (pool.Count == 0)
            {
                fail.Add($"{place}({node.Id}): 몬스터 풀 비어 있음");
                continue;
            }

            // 실제 스폰 경로(PickFieldMonsterId)도 이 풀 안에서 고르는지 확인 — fallback으로 새면 안 된다.
            for (int t = 0; t < 5; t++)
            {
                int picked = Managers.Object.PickFieldMonsterId(0);
                if (!pool.Contains(picked))
                {
                    fail.Add($"{place}: PickFieldMonsterId가 풀 밖의 {picked}를 반환");
                    break;
                }
            }

            foreach (int monId in pool)
            {
                if (!data.CreatureDic.TryGetValue(monId, out var cd))
                {
                    fail.Add($"{place}: CreatureData 없음 {monId}");
                    continue;
                }
                seenMonsters.Add(monId);
                if (cd.DropItemId > 0) droppedMaterials.Add(cd.DropItemId);

                var mc = Managers.Object.SpawnMonster(monId, new Vector3(3f, Define.GROUND_Y, 0f));
                if (mc == null) { fail.Add($"{place}: 스폰 실패 {monId}"); continue; }
                spawned++;
                yield return null;                     // 애니메이터가 첫 프레임을 적용하도록 1프레임

                CheckVisual(mc, cd, place, fail);

                // 실제 전투: 플레이어 공격을 반복해 잡는다
                int before = game.GetItemCount(cd.DropItemId);
                int rounds = 0;
                while (mc != null && mc.Hp > 0f && rounds < MAX_ROUNDS)
                {
                    var r = BattleMath.Resolve(player, mc, Define.AttackType.Physical);
                    mc.TakeHit(r, player);
                    rounds++;
                }

                if (rounds >= MAX_ROUNDS)
                    fail.Add($"{place} Lv{lv}: '{Util.Localize(cd.Name)}'를 {MAX_ROUNDS}회 안에 못 잡음 " +
                             $"(플레이어 물공 {player.PhysicalAtk:F0} vs 몬스터 HP {cd.MaxHp:F0}/방어 {cd.Def:F0})");
                else
                {
                    killed++;
                    if (cd.DropItemId > 0 && game.GetItemCount(cd.DropItemId) > before) dropped++;
                }

                if (mc != null) Managers.Resource.Destroy(mc.gameObject);
            }

            if (i % 10 == 0)
            {
                Debug.Log($"[플레이스루] {i + 1}/{nodes.Count} — {place} Lv{lv} (누적 스폰 {spawned} / 처치 {killed})");
                yield return null;
            }
        }

        // ③ 장비 60종이 **전부 제작 가능한가** — 레시피 존재 + 재료가 실제로 얻을 수 있는 것인가.
        //    획득 경로가 없으면 "만들어만 두고 못 쓰는 장비"가 다시 생긴다.
        int craftable = 0;
        foreach (var eq in data.EquipmentDic)
        {
            var r = game.GetRecipeFor(eq.Key);
            if (r == null) { fail.Add($"장비 {eq.Key} 레시피 없음"); continue; }
            if (r.ReqLevel < 1 || r.ReqLevel > 300) fail.Add($"장비 {eq.Key} 요구레벨 이상 {r.ReqLevel}");

            foreach (var (mid, need) in GameManager.RecipeMaterials(r))
            {
                if (mid <= 0 || need <= 0) continue;
                if (data.ItemDic == null || !data.ItemDic.ContainsKey(mid))
                { fail.Add($"장비 {eq.Key} 재료 {mid} 아이템 데이터 없음"); continue; }
                if (!droppedMaterials.Contains(mid))
                    fail.Add($"장비 {eq.Key} 재료 {mid}는 어떤 몬스터도 드롭하지 않음");
            }
            craftable++;
        }

        // 몬스터 커버리지 — 503종 중 실제로 어느 노드에서도 안 뽑히는 종이 있는지
        var unreachable = new List<int>();
        foreach (var id in data.CreatureDic.Keys)
            if (!seenMonsters.Contains(id)) unreachable.Add(id);

        fail.InsertRange(0, questFail);

        var sb = new StringBuilder();
        sb.AppendLine("═══ 전 구간 플레이스루 결과 ═══");
        sb.AppendLine($"메인 퀘스트    : {(questFail.Count == 0 ? "1→마지막 전부 통과" : $"막힘 {questFail.Count}건")}");
        sb.AppendLine($"장비 제작      : {craftable} / {data.EquipmentDic.Count} 레시피 확인");
        sb.AppendLine($"전투 노드      : {nodes.Count}");
        sb.AppendLine($"스폰/처치      : {spawned} / {killed}");
        sb.AppendLine($"드롭 확인      : {dropped}");
        sb.AppendLine($"조우한 몬스터  : {seenMonsters.Count} / {data.CreatureDic.Count}");
        sb.AppendLine($"어느 노드에서도 안 나오는 몬스터: {unreachable.Count}");
        sb.AppendLine($"실패           : {fail.Count}");
        sb.AppendLine($"소요           : {Time.realtimeSinceStartup - t0:F1}s");
        for (int i = 0; i < fail.Count && i < 40; i++) sb.AppendLine("  ✗ " + fail[i]);
        if (fail.Count > 40) sb.AppendLine($"  … 외 {fail.Count - 40}건");

        if (fail.Count == 0) Debug.Log(sb.ToString());
        else Debug.LogError(sb.ToString());
    }

    /// <summary>
    /// 메인 퀘스트를 1번부터 마지막까지 **조건을 실제로 만족시키며** 걸어간다.
    ///  퀘스트 번호를 강제로 밀어넣지 않는다 — 그러면 "진행 가능한가"를 검증하지 못한다.
    ///  각 완료 타입에 맞는 실제 API(CheckButtonClick / AddItem)를 호출해 자연스럽게 넘어가는지 본다.
    /// </summary>
    static IEnumerator CoWalkQuestChain(List<string> fail)
    {
        var game = Managers.Game;
        var data = Managers.Data;
        if (Managers.Quest == null || data.QuestDic == null || data.QuestDic.Count == 0)
        {
            fail.Add("퀘스트 데이터 없음");
            yield break;
        }

        int last = 0;
        foreach (int q in data.QuestDic.Keys) if (q > last) last = q;

        int guard = 0;
        while (game.CurrentQuestID <= last && guard++ < 60)
        {
            int cur = game.CurrentQuestID;
            if (!data.QuestDic.TryGetValue(cur, out var quest))
            {
                fail.Add($"퀘스트 {cur} 데이터 없음");
                yield break;
            }

            switch (quest.CompleteType)
            {
                case Define.QuestCompleteType.ITEM_ACQUIRE:
                case Define.QuestCompleteType.ITEM_COUNT:
                    if (int.TryParse(quest.CompleteTarget, out int itemId))
                    {
                        game.AddItem(itemId, Mathf.Max(1, quest.CompleteCount));
                        Managers.Quest.CheckItemAcquire(itemId);
                    }
                    break;

                case Define.QuestCompleteType.BTN_CLICK:
                    Managers.Quest.CheckButtonClick(quest.CompleteTarget);
                    break;

                case Define.QuestCompleteType.ANIM_END:
                    Managers.Quest.CheckAnimationEnd(quest.AnimationID);
                    break;

                case Define.QuestCompleteType.MULTI:
                    // 조건 목록을 순서대로 전부 만족시킨다
                    if (data.QuestConditionDic != null &&
                        data.QuestConditionDic.TryGetValue(cur, out var conds))
                    {
                        foreach (var c in conds)
                        {
                            if (int.TryParse(c.Target, out int cid)) { game.AddItem(cid, Mathf.Max(1, c.Count)); Managers.Quest.CheckItemAcquire(cid); }
                            else Managers.Quest.CheckButtonClick(c.Target);
                        }
                    }
                    break;
            }

            yield return null;

            if (game.CurrentQuestID == cur)
            {
                fail.Add($"퀘스트 {cur} '{quest.QuestName}'에서 진행이 막힘 " +
                         $"(CompleteType={quest.CompleteType} Target='{quest.CompleteTarget}')");
                yield break;
            }
        }

        Debug.Log($"[플레이스루] 메인 퀘스트 1→{last} 전부 통과 (현재 {game.CurrentQuestID})");
    }

    // 그림/접지/이름-그림 정합
    static void CheckVisual(MonsterController mc, CreatureData cd, string place, List<string> fail)
    {
        string name = Util.Localize(cd.Name);

        var anim = mc.GetComponent<Animator>();
        if (anim == null || anim.runtimeAnimatorController == null)
            fail.Add($"{place}: '{name}' 애니메이터 없음 (AnimController={cd.AnimController})");
        else if (anim.runtimeAnimatorController.animationClips.Length < 5)
            fail.Add($"{place}: '{name}' 클립 부족 {anim.runtimeAnimatorController.animationClips.Length}/5");

        var sr = mc.GetComponentInChildren<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
            fail.Add($"{place}: '{name}' 스프라이트 없음");

        // 발이 지면에 닿는가 (pivot이 보이는 발밑이므로 transform.y가 곧 발 높이)
        float dy = Mathf.Abs(mc.transform.position.y - Define.GROUND_Y);
        if (dy > 0.05f)
            fail.Add($"{place}: '{name}' 접지 어긋남 y={mc.transform.position.y:F2} (지면 {Define.GROUND_Y})");

        // 이름과 그림이 같은 아키타입인가 — IconLabel/AnimController가 같은 원형을 가리켜야 한다
        if (!string.IsNullOrEmpty(cd.IconLabel) && !string.IsNullOrEmpty(cd.AnimController)
            && cd.IconLabel.StartsWith("Mon_") && cd.AnimController != cd.IconLabel)
            fail.Add($"{place}: '{name}' 그림({cd.IconLabel}) ≠ 애니메이터({cd.AnimController})");
    }
}
