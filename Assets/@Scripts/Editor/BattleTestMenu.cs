#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Phase A/B 검증용 임시 메뉴.
// PlayMode 중에 Test/Battle 메뉴에서 선택. Phase G 이후에는 실제 전투에서 자연스럽게 검증되니 삭제 가능.
public static class BattleTestMenu
{
    // ─────────────────────────────────────────────
    // Phase B — Damage Textpopup 5가지 모드 시각 검증
    // 카메라 중앙(0,0,0) 근처에 spawn한 뒤 0.8 위로 떠올라 사라지는지 본다.
    // ─────────────────────────────────────────────
    [MenuItem("Test/Battle/Damage Popup - Normal", false, 300)]
    static void SpawnNormal()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }
        Managers.Object.ShowDamageFont(SpawnPos(), Random.Range(50, 200), DamageFontMode.Normal);
    }

    [MenuItem("Test/Battle/Damage Popup - Critical", false, 301)]
    static void SpawnCritical()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }
        Managers.Object.ShowDamageFont(SpawnPos(), Random.Range(500, 1500), DamageFontMode.Critical);
    }

    [MenuItem("Test/Battle/Damage Popup - Miss", false, 302)]
    static void SpawnMiss()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }
        Managers.Object.ShowMissPopup(SpawnPos());
    }

    [MenuItem("Test/Battle/Critical Callout (\"치명타!\"/\"Critical!\")", false, 303)]
    static void SpawnCallout()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }
        Managers.Object.ShowCriticalCallout(SpawnPos());
    }

    [MenuItem("Test/Battle/Heal Popup", false, 304)]
    static void SpawnHeal()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }
        Managers.Object.ShowHealFont(SpawnPos(), Random.Range(20, 80));
    }

    [MenuItem("Test/Battle/All Modes (한번에 비교)", false, 320)]
    static void SpawnAll()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }
        Vector2 c = CameraCenter();
        Managers.Object.ShowDamageFont(c + new Vector2(-3f,  1f), 123, DamageFontMode.Normal);
        Managers.Object.ShowDamageFont(c + new Vector2(-1.5f, 1f), 999, DamageFontMode.Critical);
        Managers.Object.ShowMissPopup(c + new Vector2( 0f,  1f));
        Managers.Object.ShowCriticalCallout(c + new Vector2( 1.5f, 1f));
        Managers.Object.ShowHealFont(c + new Vector2( 3f,  1f), 42);
    }

    // ─────────────────────────────────────────────
    // Phase A — BattleMath 데미지 식 sanity check (로그 기반)
    // 가상의 attacker/defender로 100회 시뮬레이션해서 분포 확인.
    // ─────────────────────────────────────────────
    // ─────────────────────────────────────────────
    // Phase I — Level Up 검증
    // ─────────────────────────────────────────────
    [MenuItem("Test/Battle/Reset Player Lv.1", false, 350)]
    static void ResetPlayerLv1()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }
        var player = Managers.Game?.Player;
        if (player == null) { Debug.LogWarning("[BattleTest] Player 없음"); return; }
        player.DebugResetLevel(1);
        Debug.Log("[BattleTest] Player Lv.1 reset.");
    }

    [MenuItem("Test/Battle/Force Level Up", false, 351)]
    static void ForceLevelUp()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }
        var player = Managers.Game?.Player;
        if (player == null) { Debug.LogWarning("[BattleTest] Player 없음"); return; }
        player.DebugForceLevelUp();
        Debug.Log($"[BattleTest] Forced Level Up → Lv.{Managers.Game.Level}");
    }

    // ─────────────────────────────────────────────
    // 디버그 토글 — Player 공격을 모두 크리티컬로 (단축키 Ctrl+Shift+C)
    // BattleMath.Resolve에서 attacker.ObjectType == Player일 때 강제 isCrit=true.
    // ─────────────────────────────────────────────
    const string MENU_FORCE_CRIT = "Test/Battle/Toggle Force Player Critical %#c";

    [MenuItem(MENU_FORCE_CRIT, false, 360)]
    static void ToggleForcePlayerCritical()
    {
        BattleMath.DebugForcePlayerCritical = !BattleMath.DebugForcePlayerCritical;
        Menu.SetChecked(MENU_FORCE_CRIT, BattleMath.DebugForcePlayerCritical);
        Debug.Log($"[BattleTest] Force Player Critical = {BattleMath.DebugForcePlayerCritical}");
    }

    [MenuItem(MENU_FORCE_CRIT, true)]
    static bool ValidateForcePlayerCritical()
    {
        Menu.SetChecked(MENU_FORCE_CRIT, BattleMath.DebugForcePlayerCritical);
        return true;
    }

    [MenuItem("Test/Battle/Log: BattleMath 100회 시뮬레이션", false, 340)]
    static void SimulateBattleMath()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[BattleTest] PlayMode 진입 후 실행"); return; }

        var atk = new GameObject("__atk_temp").AddComponent<TestCreature>();
        var def = new GameObject("__def_temp").AddComponent<TestCreature>();
        atk.PhysicalAtk = 100; atk.Accuracy = 80; atk.CritRate = 30; atk.CritDamage = 200; atk.DmgAmp = 0;
        def.Evasion = 10; def.PhysicalResist = 20; def.Def = 10;

        int hits = 0, crits = 0, miss = 0;
        long total = 0; int min = int.MaxValue, max = 0;
        for (int i = 0; i < 100; i++)
        {
            var r = BattleMath.Resolve(atk, def, Define.AttackType.Physical);
            if (!r.Hit) { miss++; continue; }
            hits++;
            if (r.IsCrit) crits++;
            total += r.FinalDamage;
            if (r.FinalDamage < min) min = r.FinalDamage;
            if (r.FinalDamage > max) max = r.FinalDamage;
        }
        float avg = hits > 0 ? (float)total / hits : 0;
        Debug.Log($"[BattleMath] 100회 — Hit {hits} / Miss {miss} / Crit {crits} / Damage avg={avg:F1} min={min} max={max}\n" +
                  $"  기대치: Hit ~70% (Acc 80 - Eva 10), Crit ~30%, FINAL = (atk*[1 or 2]*0.8 - 저항20% - 방어10) * 0.8~1.2");

        Object.Destroy(atk.gameObject);
        Object.Destroy(def.gameObject);
    }

    // ─────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────
    static Vector2 SpawnPos()
    {
        return CameraCenter() + new Vector2(Random.Range(-1f, 1f), Random.Range(-0.3f, 0.3f));
    }

    static Vector2 CameraCenter()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector2.zero;
        return cam.transform.position;
    }

    // CreatureController는 abstract라 직접 인스턴스화 불가 → 간단 더미.
    class TestCreature : CreatureController
    {
        public override bool Init() => true;
    }
}
#endif
