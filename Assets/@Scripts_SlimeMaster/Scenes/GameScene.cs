using Data;
using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;
using static GemInfo;

public class GameScene : BaseScene
{
    public GameObject Weapon;
    GameManager _game;

    private int _lastSecond = 30;
    bool isGameEnd = false;
    SpawningPool _spawningPool;
    PlayerController _player;

    #region Action
    public Action<int> OnWaveStart;
    public Action<int> OnSecondChange;
    public Action OnWaveEnd;
    #endregion
    UI_GameScene _ui;
    BossController _boss;

    private void Awake()
    {
        Init();
        SceneChangeAnimation_Out anim = Managers.Resource.Instantiate("SceneChangeAnimation_Out").GetOrAddComponent<SceneChangeAnimation_Out>();
        //anim.transform.SetParent(parents);
        anim.SetInfo(SceneType, () => { });
    }

    protected override void Init()
    {
        Debug.Log("@>> GameScene Init()");
        base.Init();
        SceneType = Define.Scene.GameScene;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        _game = Managers.Game;
        Managers.UI.ShowSceneUI<UI_Joystick>();

        if (_game.ContinueInfo.isContinue == true)
        {
            _player = Managers.Object.Spawn<PlayerController>(Vector3.zero, _game.ContinueInfo.PlayerDataId);
        }
        else
        {
            _game.ClearContinueData();
            _player = Managers.Object.Spawn<PlayerController>(Vector3.zero, 201000);

        }

        LoadStage();

        _player.OnPlayerDead = OnPlayerDead;

        Managers.Game.CameraController = FindObjectOfType<CameraController>();

        _ui = Managers.UI.ShowSceneUI<UI_GameScene>();
        //UI_GameScene만 따로 SortOrder을 줌 ( 영혼 획득 처리 떄문에 렌더모드를 ScreenSpace-Camera로 뒀기때문)
        _ui.GetComponent<Canvas>().sortingOrder = Define.UI_GAMESCENE_SORT_CLOSED;

        OnWaveStart = _ui.OnWaveStart;
        OnSecondChange = _ui.OnSecondChange;
        OnWaveEnd = _ui.OnWaveEnd;
        Managers.Sound.Play(Define.Sound.Bgm, "Bgm_Game");

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Managers.Game.Player.SoulCount += 100;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Managers.Game.Player.Exp += 5;
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Managers.Object.KillAllMonsters();
        }
        else if (Input.GetKeyDown(KeyCode.F10))
        {
            WaveEnd();
        }

        if (isGameEnd == true || _game.CurrentWaveData == null)
            return;

        if (_boss == null)
        {
            _game.TimeRemaining -= Time.deltaTime;
        }
        else 
        {
            _game.TimeRemaining = 0;

        }

        int currentMinute = Mathf.FloorToInt(_game.TimeRemaining / _game.CurrentWaveData.RemainsTime); // 웨이브 시간 이슈 수정 #Neo 
        int currentSecond =(int)_game.TimeRemaining;

        if (currentSecond != _lastSecond)
        {
            OnSecondChange?.Invoke(currentSecond);

            if (currentSecond == 30)
            {
                // 웨이브 보상 생성
                SpawnWaveReward();
            }
        }

        if (_game.TimeRemaining < 0)
        {
            //wave 종료
            WaveEnd();

        }

        _lastSecond = currentSecond;

    }

    public void LoadStage()
    {
        if (_spawningPool == null)
            _spawningPool = gameObject.AddComponent<SpawningPool>();

        Managers.Object.LoadMap(_game.CurrentStageData.MapName);

        // 웨이브 정보 적용
        StopAllCoroutines();
        StartCoroutine(StartWave(_game.CurrentStageData.WaveArray[_game.CurrentWaveIndex]));
    }

    IEnumerator StartWave(WaveData wave)
    {
        yield return new WaitForEndOfFrame();
        OnWaveStart?.Invoke(wave.WaveIndex);

        if (wave.WaveIndex == 1)
        {
            GenerateRandomExperience(30);
        }

        SpawnWaveReward();
        _game.TimeRemaining = _game.CurrentStageData.WaveArray[_game.CurrentWaveIndex].RemainsTime;
        _game.CurrentMap.ChangeMapSize(Define.MAPSIZE_REDUCTION_VALUE);

        Vector2 spawnPos = Util.GenerateMonsterSpawnPosition(_game.Player.PlayerCenterPos);

        _spawningPool.StartSpawn();

        _game.SaveGame();

        //엘리트 몬스터 소환
        EliteController elite;
        for (int i = 0; i < _game.CurrentWaveData.EleteId.Count; i++)
        {
            elite = Managers.Object.Spawn<EliteController>(spawnPos, _game.CurrentWaveData.EleteId[i]);
            elite.MonsterInfoUpdate -= _ui.MonsterInfoUpdate;
            elite.MonsterInfoUpdate += _ui.MonsterInfoUpdate;
        }

        //yield return new WaitForSeconds(Define.BOSS_GEN_TIME);


        yield break;
    }

    void WaveEnd()
    {
        OnWaveEnd?.Invoke();
        if (_game.CurrentWaveIndex < _game.CurrentStageData.WaveArray.Count - 1)
        {
            _game.CurrentWaveIndex++;

            StopAllCoroutines();
            StartCoroutine(StartWave(_game.CurrentStageData.WaveArray[_game.CurrentWaveIndex]));
        }
        else
        {
            Vector2 spawnPos = Util.GenerateMonsterSpawnPosition(_game.Player.PlayerCenterPos, 10, 15);

            //보스 출현
            for (int i = 0; i < _game.CurrentWaveData.BossId.Count; i++)
            {
                _boss = Managers.Object.Spawn<BossController>(spawnPos, _game.CurrentWaveData.BossId[i]);
                _boss.MonsterInfoUpdate -= _ui.MonsterInfoUpdate;
                _boss.MonsterInfoUpdate += _ui.MonsterInfoUpdate;
                _boss.OnBossDead -= OnBossDead;
                _boss.OnBossDead += OnBossDead;
            }

        }
    }

    void OnBossDead()
    {
        StartCoroutine(CoGameEnd());
    }

    IEnumerator CoGameEnd()
    {
        yield return new WaitForSeconds(1f);
        isGameEnd = true;
        if (Managers.Game.DicMission.TryGetValue(MissionTarget.StageClear, out MissionInfo mission))
            mission.Progress++;

        Managers.Game.IsGameEnd = true;
        UI_GameResultPopup cp = Managers.UI.ShowPopupUI<UI_GameResultPopup>();
        cp.SetInfo();
    }

    void OnPlayerDead()
    {
        if (Managers.Game.IsGameEnd == false)
        { 
            UI_ContinuePopup gp = Managers.UI.ShowPopupUI<UI_ContinuePopup>();
            gp.SetInfo();
        }
    }

    enum eDropType
    {
        Potion,
        Magnet,
        Bomb
    }
    void SpawnWaveReward()
    {
        eDropType spawnType = (eDropType)UnityEngine.Random.Range(0, 3);

        Vector3 spawnPos = Util.RandomPointInAnnulus(Managers.Game.Player.CenterPosition, 3, 6);
        Data.DropItemData dropItem;
        switch (spawnType)
        {
            case eDropType.Potion:
                if (Managers.Data.DropItemDataDic.TryGetValue(Define.ID_POTION, out dropItem) == true)
                    Managers.Object.Spawn<PotionController>(spawnPos).SetInfo(dropItem);
                break;
            case eDropType.Magnet:
                if (Managers.Data.DropItemDataDic.TryGetValue(Define.ID_MAGNET, out dropItem) == true)
                    Managers.Object.Spawn<MagnetController>(spawnPos).SetInfo(dropItem);
                break;
            case eDropType.Bomb:
                if (Managers.Data.DropItemDataDic.TryGetValue(Define.ID_BOMB, out dropItem) == true)
                    Managers.Object.Spawn<BombController>(spawnPos).SetInfo(dropItem);
                break;
        }
    }

    public void GenerateRandomExperience(int n)
    {
        int[] coins = new int[] { 1, 2, 5, 10 };
        List<GemType> combination = new List<GemType>();

        int remainingValue = n;

        while (remainingValue > 0)
        {
            int coinIndex = UnityEngine.Random.Range(0, coins.Length);
            int coinValue = coins[coinIndex];

            if (remainingValue >= coinValue)
            {
                GemType gemType = (GemType)coinIndex; 
                combination.Add(gemType);
                remainingValue -= coinValue;
            }
        }

        foreach (GemType type in combination) 
        {
            GemController gem = Managers.Object.Spawn<GemController>(Util.RandomPointInAnnulus(Managers.Game.Player.CenterPosition,6,10));
            gem.SetInfo(Managers.Game.GetGemInfo(type));
        }

    }

    public override void Clear()
    {

    }

}
