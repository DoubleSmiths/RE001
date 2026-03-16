using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_BattleScene : UI_Scene
{
    #region Enum
    enum Images
    {
        PlayerHPBarGuage,
        PlayerEXPBarGuage,
        BossHPBarGuage,
    }

    enum Texts
    {
        MainGoldText,
        MainCrystalText,
        PlayerHPBarText,
        MonsterKillCountText,
        AreaNameText,
        BossHPBarText,
    }

    enum Buttons
    {
        PauseBtn,
        ReturnHubBtn,
    }

    enum GameObjects
    {
        BossHPBar,
        PlayerBar,
    }
    #endregion

    int _killCount;

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.PauseBtn).gameObject.BindEvent(() => OnClickPause());
        GetButton((int)Buttons.ReturnHubBtn).gameObject.BindEvent(() => OnClickReturnHub());

        // 보스 HP 바는 기본적으로 숨김
        GetObject((int)GameObjects.BossHPBar).SetActive(false);

        RefreshCurrency();
        Managers.Game.OnResourceChanged += RefreshCurrency;

        return true;
    }

    void OnDestroy()
    {
        if (Managers.Game != null)
            Managers.Game.OnResourceChanged -= RefreshCurrency;
    }

    // ── 화폐 ──
    void RefreshCurrency()
    {
        GetText((int)Texts.MainGoldText).text    = Managers.Game.Gold.ToString("N0");
        GetText((int)Texts.MainCrystalText).text = Managers.Game.Crystal.ToString("N0");
    }

    // ── PlayerController 이벤트 연결 ──
    public void BindPlayer(PlayerController player)
    {
        player.OnHpChanged += (cur, max) =>
        {
            GetImage((int)Images.PlayerHPBarGuage).fillAmount = cur / max;
            GetText((int)Texts.PlayerHPBarText).text = $"{(int)cur} / {(int)max}";
        };

        player.OnExpChanged += (_, _) => { };  // 배틀씬에서는 EXP 표시 생략 가능
    }

    // ── 몬스터 킬 카운트 ──
    public void AddKillCount(int amount = 1)
    {
        _killCount += amount;
        GetText((int)Texts.MonsterKillCountText).text = $"처치: {_killCount}";
    }

    // ── 구역 이름 ──
    public void SetAreaName(string name)
    {
        GetText((int)Texts.AreaNameText).text = name;
    }

    // ── 보스 HP 바 ──
    public void ShowBossHP(string bossName, float current, float max)
    {
        GetObject((int)GameObjects.BossHPBar).SetActive(true);
        GetImage((int)Images.BossHPBarGuage).fillAmount = current / max;
        GetText((int)Texts.BossHPBarText).text = $"{bossName}  {(int)current} / {(int)max}";
    }

    public void UpdateBossHP(float current, float max)
    {
        GetImage((int)Images.BossHPBarGuage).fillAmount = current / max;
        GetText((int)Texts.BossHPBarText).text = $"{(int)current} / {(int)max}";
        if (current <= 0)
            GetObject((int)GameObjects.BossHPBar).SetActive(false);
    }

    // ── 버튼 핸들러 ──
    void OnClickPause()
    {
        Time.timeScale = Time.timeScale > 0 ? 0 : 1;
    }

    void OnClickReturnHub()
    {
        Time.timeScale = 1;
        Managers.Scene.LoadScene(Scene.HubScene, transform);
    }
}
