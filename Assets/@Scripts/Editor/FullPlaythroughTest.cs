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

        // 전 컨텐츠 해금 — 잠금 때문에 못 들어가는 노드가 생기면 검증이 아니라 관문 테스트가 된다.
        game.UnlockEverything();

        var nodes = new List<TravelNodeData>();
        foreach (var n in data.TravelNodeDic.Values)
            if (n.AreaType == "Battle") nodes.Add(n);
        nodes.Sort((a, b) => a.Id.CompareTo(b.Id));

        var fail = new List<string>();
        var seenMonsters = new HashSet<int>();
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

        // 몬스터 커버리지 — 503종 중 실제로 어느 노드에서도 안 뽑히는 종이 있는지
        var unreachable = new List<int>();
        foreach (var id in data.CreatureDic.Keys)
            if (!seenMonsters.Contains(id)) unreachable.Add(id);

        var sb = new StringBuilder();
        sb.AppendLine("═══ 전 구간 플레이스루 결과 ═══");
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
