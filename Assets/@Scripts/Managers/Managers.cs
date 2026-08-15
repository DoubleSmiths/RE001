using UnityEngine;

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
    //TimeManager   _time;

    public static GameManager     Game     { get { return Instance?._game; } }
    public static ObjectManager   Object   { get { return Instance?._object; } }
    public static MinMonManager   MinMon   { get { return Instance?._minMon; } }
    public static AbilityManager  Ability  { get { return Instance?._ability; } }
    public static QuestManager    Quest    { get { return Instance?._quest; } }
    public static SubQuestManager SubQuest { get { return Instance?._subQuest; } }
    public static ComboManager    Combo    { get { return Instance?._combo; } }
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
    static void ResetShutdownFlag() => s_shuttingDown = false;

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
    }

    // 방치 보상 기준 시각 기록. 모바일은 종료 대신 일시정지로 빠지는 경우가 많아 둘 다 건다.
    void OnApplicationPause(bool paused)
    {
        if (paused && s_instance == this) _game?.MarkSeen();
    }

    void OnApplicationQuit()
    {
        if (s_instance == this) _game?.MarkSeen();
    }

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
