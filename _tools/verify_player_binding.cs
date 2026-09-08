// GameScene 진입 완료 후 실행. Android는 APK 실행 검증을 별도로 수행한다.
var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
if (player == null || Managers.Game.Player != player || player.AbilityRunner == null)
    throw new System.Exception("Player binding not ready");
var comboPlayer = typeof(ComboManager).GetField("_player", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(Managers.Combo);
if (!object.ReferenceEquals(comboPlayer, player)) throw new System.Exception("Combo player binding missing");
if (UnityEngine.Object.FindAnyObjectByType<UI_GameScene>() == null) throw new System.Exception("HUD missing");
return "PASS: player, ability runner, combo and HUD ready";
