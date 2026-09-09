// 씬 Clear가 정지 소유권과 복원값을 다음 씬으로 넘기지 않는지 검사한다.
var ui = Managers.UI;
var player = Managers.Game.Player;
if (!UnityEngine.Application.isPlaying || player == null || player.InBattle || ui.GetPopupCount() != 0 || ui.IsActiveSoulShop)
    throw new System.Exception("BattlePauseClear requires idle town without popups");
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var battle = typeof(PlayerController).GetField("_inBattle", flags);
var fields = typeof(UIManager).GetFields(flags).Where(f => !f.IsInitOnly).ToDictionary(f => f, f => f.GetValue(ui));
float originalScale = UnityEngine.Time.timeScale;
float originalTweenScale = DG.Tweening.DOTween.timeScale;
var failures = new System.Collections.Generic.List<string>();
void Check(bool pass, string message) { if (!pass) failures.Add(message); }
try
{
    foreach (var field in fields.Keys.Where(f => typeof(System.Delegate).IsAssignableFrom(f.FieldType))) field.SetValue(ui, null);
    battle.SetValue(player, true);
    foreach (float scale in new[] { 0f, 0.25f, 1f, 10f })
    {
        UnityEngine.Time.timeScale = scale;
        ui.ShowPopupUI<UI_BestiaryPopup>();
        Check(ui.IsBattlePaused && UnityEngine.Time.timeScale == 0f, "Pause failed at " + scale);
        ui.CloseAllPopupUI();
        Check(!ui.IsBattlePaused && UnityEngine.Time.timeScale == scale, "Exact restore failed at " + scale);
    }
    UnityEngine.Time.timeScale = 0.5f;
    ui.ShowPopupUI<UI_BestiaryPopup>();
    ui.IsActiveSoulShop = true;
    ui.Clear();
    Check(ui.GetPopupCount() == 0 && !ui.IsBattlePaused && !ui.IsActiveSoulShop && ui.SceneUI == null && UnityEngine.Time.timeScale == 1f, "Clear retained old pause ownership");
    UnityEngine.Time.timeScale = 0.25f;
    ui.ShowPopupUI<UI_BestiaryPopup>();
    ui.CloseAllPopupUI();
    Check(UnityEngine.Time.timeScale == 0.25f && !ui.IsBattlePaused, "New popup restored stale pre-Clear scale");
    Check(DG.Tweening.DOTween.timeScale == originalTweenScale, "Global tween scale was overwritten");
}
finally
{
    ui.CloseAllPopupUI();
    battle.SetValue(player, false);
    foreach (var pair in fields) pair.Key.SetValue(ui, pair.Value);
    UnityEngine.Time.timeScale = originalScale;
}
if (failures.Count != 0) throw new System.Exception("BattlePauseClear FAIL: " + string.Join(" | ", failures));
return "BattlePauseClear PASS: 0/0.25/1/10 exact restore; Clear clears ownership; new pause independent; global tween scale preserved";
