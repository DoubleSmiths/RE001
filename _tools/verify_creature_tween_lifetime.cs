// 실제 몬스터 프리팹의 피격 색상 연출을 제거/비활성/재사용 경계에서 검사한다.
// HP나 킬 보상을 건드리지 않고 PlayHitFeedback만 호출한다. 테스트 오브젝트만 생성·제거한다.
// python _tools/unity_exec.py _tools/verify_creature_tween_lifetime.cs --allow-async
if (!UnityEditor.EditorApplication.isPlaying || UnityEngine.Time.timeScale <= 0f)
    throw new System.Exception("Play mode at positive time scale required");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var playHit = typeof(CreatureController).GetMethod("PlayHitFeedback", flags);
if (playHit == null) throw new System.Exception("Creature hit-feedback entry missing");
MonsterController SpawnProbe()
{
    var data = Managers.Data.CreatureDic[2001];
    var go = Managers.Resource.Instantiate(data.PrefabLabel, pooling: false);
    // 같은 이름의 기존 풀로 들어가지 않게 고유 이름을 준다.
    go.name = "CreatureTweenLifetime_" + go.GetInstanceID();
    go.transform.position = new UnityEngine.Vector3(10000f, Define.GROUND_Y, 0f);
    var creature = go.GetComponent<MonsterController>() ?? go.AddComponent<MonsterController>();
    creature.SetInfo(2001);
    creature.GetComponent<UnityEngine.Rigidbody2D>().simulated = false;
    return creature;
}
System.Collections.IEnumerator Run()
{
    var failures = new System.Collections.Generic.List<string>();
    UnityEngine.Application.LogCallback onLog = (message, trace, type) =>
    {
        if ((type == UnityEngine.LogType.Warning || type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception)
            && ((message.Contains("DOTWEEN") && (message.Contains("SpriteRenderer") || message.Contains("destroyed") || message.Contains("NULL")))
                || trace.Contains("CoColorFlashOnHit"))) failures.Add(message);
    };
    var outside = new UnityEngine.GameObject("CreatureTweenLifetime_Outside", typeof(UnityEngine.SpriteRenderer));
    var outsideTween = DG.Tweening.DOTweenModuleSprite.DOColor(outside.GetComponent<UnityEngine.SpriteRenderer>(), UnityEngine.Color.blue, 60f);
    var probes = new System.Collections.Generic.List<MonsterController>();
    UnityEngine.Application.logMessageReceived += onLog;
    try
    {
        // Startup 전 제거, 붉어지는 중 제거, 흰색 복귀 Tween 생성 직후 제거를 각각 지난다.
        foreach (float delay in new[] { 0f, 0.15f, 0.36f })
        {
            var creature = SpawnProbe();
            probes.Add(creature);
            var sprite = creature.GetComponent<UnityEngine.SpriteRenderer>();
            playHit.Invoke(creature, null);
            if (delay > 0f) yield return new UnityEngine.WaitForSeconds(delay);
            Managers.Resource.Destroy(creature.gameObject);
            yield return new UnityEngine.WaitForSeconds(0.8f);
            if (creature != null) failures.Add("Probe not destroyed at hit phase " + delay);
            if (DG.Tweening.DOTween.IsTweening(sprite)) failures.Add("Destroyed renderer still owns a tween at hit phase " + delay);
        }

        // 풀 반환의 실제 경계인 비활성화: 색 복귀/트윈 종료 후 같은 오브젝트가 다시 피격 가능해야 한다.
        var reused = SpawnProbe();
        probes.Add(reused);
        var reusedSprite = reused.GetComponent<UnityEngine.SpriteRenderer>();
        playHit.Invoke(reused, null);
        yield return new UnityEngine.WaitForSeconds(0.15f);
        if (UnityEngine.Mathf.Approximately(reusedSprite.color.g, 1f)) failures.Add("Normal hit did not change sprite color");
        reused.gameObject.SetActive(false);
        if (DG.Tweening.DOTween.IsTweening(reusedSprite)) failures.Add("Disabled renderer still owns a hit tween");
        yield return new UnityEngine.WaitForSeconds(0.1f);
        reused.gameObject.SetActive(true);
        if (reusedSprite.color != UnityEngine.Color.white) failures.Add("Reactivated renderer kept stale hit tint");
        playHit.Invoke(reused, null);
        yield return new UnityEngine.WaitForSeconds(0.8f);
        if (reusedSprite.color != UnityEngine.Color.white || DG.Tweening.DOTween.IsTweening(reusedSprite))
            failures.Add("Normal hit did not finish white after reactivation");
        if (!DG.Tweening.TweenExtensions.IsActive(outsideTween)) failures.Add("Unrelated sprite tween was killed");
        if (failures.Count != 0) throw new System.Exception("Creature tween lifetime failed (" + failures.Count + "): " + string.Join(" | ", failures));
        UnityEngine.Debug.Log("[CreatureTweenLifetime] PASS: immediate/mid-hit/return-phase destruction without tween warnings, disable clears tint/tween, reactivation hit completes, unrelated tween alive");
    }
    finally
    {
        foreach (var creature in probes)
        {
            if (creature == null) continue;
            // 실패한 RED 검사에서도 자기 Tween만 정리하여 다음 검증에 경고를 남기지 않는다.
            creature.StopAllCoroutines();
            DG.Tweening.DOTween.Kill(creature.GetComponent<UnityEngine.SpriteRenderer>(), false);
            Managers.Resource.Destroy(creature.gameObject);
        }
        DG.Tweening.TweenExtensions.Kill(outsideTween, false);
        Managers.Resource.Destroy(outside);
        UnityEngine.Application.logMessageReceived -= onLog;
    }
}
CoroutineManager.StartCoroutine(Run());
return "Creature tween lifetime verification started";
