using UnityEngine;
using static Define;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    #region Contents
    GameManager     _game     = new GameManager();
    ObjectManager   _object   = new ObjectManager();
    MinMonManager   _minMon   = new MinMonManager();
    AbilityManager  _ability  = new AbilityManager();
    QuestManager    _quest    = new QuestManager();
    SubQuestManager _subQuest = new SubQuestManager();
    ComboManager    _combo    = new ComboManager();
    MissionManager  _mission  = new MissionManager();   // v2 미션·출석(ADR0030)
    ShopManager     _shop     = new ShopManager();      // v2 상점·핫타임·월정액(ADR0030)
    DungeonManager  _dungeon  = new DungeonManager();   // v2 요일 던전(ADR0034)
    RelicManager    _relic    = new RelicManager();     // v2 유물 소환(ADR0035)
    BondManager     _bond     = new BondManager();      // v2 마을 사람들(ADR0036)
    PromotionManager _promotion = new PromotionManager(); // v2 시련(승급 시험, ADR0037)
    AbyssManager    _abyss    = new AbyssManager();     // v2 심연(ADR0037)
    //TimeManager   _time;

    public static GameManager     Game     { get { return Instance?._game; } }
    public static ObjectManager   Object   { get { return Instance?._object; } }
    public static MinMonManager   MinMon   { get { return Instance?._minMon; } }
    public static AbilityManager  Ability  { get { return Instance?._ability; } }
    public static QuestManager    Quest    { get { return Instance?._quest; } }
    public static SubQuestManager SubQuest { get { return Instance?._subQuest; } }
    public static ComboManager    Combo    { get { return Instance?._combo; } }
    public static MissionManager  Mission  { get { return Instance?._mission; } }
    public static ShopManager     Shop     { get { return Instance?._shop; } }
    public static DungeonManager  Dungeon  { get { return Instance?._dungeon; } }
    public static RelicManager    Relic    { get { return Instance?._relic; } }
    public static BondManager     Bond     { get { return Instance?._bond; } }
    public static PromotionManager Promotion { get { return Instance?._promotion; } }
    public static AbyssManager    Abyss    { get { return Instance?._abyss; } }
    //public static TimeManager   Time   { get { return Instance?._time; } }
    #endregion

    // 씬에 배치된 @Managers가 DontDestroyOnLoad 싱글턴과 중복되면 이 중복을 제거.
    // (씬 재진입 시 새 @Managers가 로드되면 Update가 둘 다 돌아, 콤보 Tick이 게임이 쓰는
    //  s_instance._combo가 아닌 중복 인스턴스의 _combo로 가버림 → 콤보 게이지/숫자가 멈추는 버그)
    void Awake()
    {
        if (!Application.isPlaying) return;   // 에디트 모드에선 DontDestroyOnLoad가 예외 → 스킵

        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // 첫 인스턴스를 싱글턴으로 확정 (lazy Init보다 Awake가 먼저 와도 안전)
        s_instance = this;
        s_shuttingDown = false;   // 새 플레이 세션 — 종료 플래그 초기화(static은 도메인 리로드를 넘어 살아남는다)
        DontDestroyOnLoad(gameObject);
    }

    // 종료 중에는 lazy Init이 @Managers를 **되살리지 않도록** 막는 플래그.
    //  이걸 안 걸면 씬을 닫는 도중 누군가 Managers.X를 건드릴 때마다 @Managers가 새로 생기고,
    //  "Some objects were not cleaned up when closing the scene" 빨간 에러로 이어진다.
    static bool s_shuttingDown;

    // ⚠️ static은 플레이 세션을 넘어 살아남는다. Managers.Awake에서 초기화하면 늦다 —
    //    PlayerController.Awake가 먼저 돌면 Init이 막힌 채 Managers.Data가 null이 되어
    //    RefreshStats에서 NullReference가 난다(실제로 그렇게 터뜨렸다).
    //    어떤 Awake보다 먼저 도는 지점에서 푼다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetShutdownFlag()
    {
        s_shuttingDown = false;
        // 씬 Managers의 Awake가 lazy Init보다 먼저 오더라도 용량 설정은 반드시 수행한다.
        // 기존 전 구간 실측 용량을 유지하되 어떤 UI가 Tween을 만들기 전에 적용한다.
        DG.Tweening.DOTween.SetTweensCapacity(DOTWEEN_TWEENER_CAPACITY, DOTWEEN_SEQUENCE_CAPACITY);

        // 플레이어 빌드 공통 — 안드로이드 기본 30fps, 방치 전투를 보는 중 화면 꺼짐,
        //  릴리스 빌드의 Debug.Log 출력 비용(경고·에러는 남겨 둔다).
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (!Debug.isDebugBuild)
            Debug.unityLogger.filterLogType = LogType.Warning;

#if UNITY_STANDALONE && !UNITY_EDITOR
        // PC 첫 실행: 세로 창을 모니터 높이의 85% 로 연다(고정 기본값은 1080p 에서 작업표시줄에 걸리거나
        //  4K 에서 너무 작다). 이후엔 Unity 가 마지막 창 크기·전체화면 여부를 기억한다.
        if (!PlayerPrefs.HasKey(PC_WINDOW_INIT_KEY))
        {
            PlayerPrefs.SetInt(PC_WINDOW_INIT_KEY, 1);
            PlayerPrefs.Save();
            int h = Mathf.RoundToInt(Display.main.systemHeight * 0.85f);
            int w = Mathf.RoundToInt(h * FixedResolution.TARGET_ASPECT);
            Screen.SetResolution(w, h, FullScreenMode.Windowed);
        }
#endif
    }

    const string PC_WINDOW_INIT_KEY = "PC_WINDOW_INIT";

    void OnDestroy()
    {
        if (s_instance != this) return;
        s_instance = null;
        s_shuttingDown = true;
    }

    void Update()
    {
        // 콤보 타이머 감소 — Time.timeScale 영향 받음 (슬로우 모션 중엔 느려짐)
        _combo?.Tick(Time.deltaTime);

        // 방치 누적 — 전투 구역 밖에서도 사냥이 계속되게. 팝업으로 timeScale이 0이 되어도
        // 멈추면 안 되므로 unscaledDeltaTime을 쓴다.
        _game?.TickIdle(Time.unscaledDeltaTime);

        // 세터들이 표시만 해 둔 저장을 주기적으로 한 번에 기록
        _game?.TickSave(Time.unscaledDeltaTime);

        // v2 핫타임이 끝나는 순간(실제 시간) 공격력·버프 아이콘을 되돌린다
        _shop?.Tick();

        // 안드로이드 뒤로가기 = KeyCode.Escape. 데이터 준비(타이틀 프리로드) 전에는 받지 않는다.
        if (Input.GetKeyDown(KeyCode.Escape) && _data.LocalizationDic.Count > 0)
            _ui.OnBackPressed();
    }

    // 방치 보상 기준 시각 기록. 모바일은 종료 대신 일시정지로 빠지는 경우가 많아 둘 다 건다.
    //  복귀(paused=false)하면 자리를 비운 시간을 정산한다 — 프로세스가 살아 있으면 GameScene 이 다시 로드되지 않아
    //  예전엔 홈으로 나갔다 온 시간이 통째로 사라졌다(다음 일시정지의 MarkSeen 이 기준 시각을 덮어썼다).
    void OnApplicationPause(bool paused)
    {
        if (s_instance != this) return;
        if (paused) { _game?.MarkSeen(); return; }

        var scene = _ui?.SceneUI as UI_GameScene;
        if (_game == null || _game.Player == null || scene == null) return;   // Player 는 Unity == 로 파괴 여부까지 본다
        var reward = _game.ClaimOfflineReward();
        if (reward.Kills > 0) scene.ShowOfflineReward(reward);
    }

    // 종료 정리(OnDestroy)가 시작되기 전에 불린다(에디터에선 플레이 종료). 종료 플래그를 여기서 세워야
    //  전투 그룹 OnDestroy → 던전·시련·심연 Finish → 결과 토스트가 닫히는 씬에 새 오브젝트를 만들지 않는다.
    void OnApplicationQuit()
    {
        if (s_instance != this) return;
        s_shuttingDown = true;
        _game?.MarkSeen();
    }

    public static bool IsShuttingDown => s_shuttingDown;

    #region Core
    DataManager     _data     = new DataManager();
    FontManager     _font     = new FontManager();
    PoolManager     _pool     = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx  _scene    = new SceneManagerEx();
    SoundManager    _sound    = new SoundManager();
    UIManager       _ui       = new UIManager();

    public static DataManager     Data     { get { return Instance?._data; } }
    public static FontManager     Font     { get { return Instance?._font; } }
    public static PoolManager     Pool     { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SceneManagerEx  Scene    { get { return Instance?._scene; } }
    public static SoundManager    Sound    { get { return Instance?._sound; } }
    public static UIManager       UI       { get { return Instance?._ui; } }
    #endregion

    public static void Init()
    {
        if (s_instance != null)
            return;
        if (s_shuttingDown)     // 종료 중이면 되살리지 않는다 (씬 정리 에러 방지)
            return;

        GameObject go = GameObject.Find("@Managers");
        if (go == null)
        {
            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();
        }

        DontDestroyOnLoad(go);
        s_instance = go.GetComponent<Managers>();

        //s_instance._sound.Init();
        //s_instance._time = go.AddComponent<TimeManager>();
    }

    public static void Clear()
    {
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
        Object.Clear();
    }
}
