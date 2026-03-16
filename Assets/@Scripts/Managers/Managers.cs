using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    #region Contents
    GameManager  _game   = new GameManager();
    ObjectManager _object = new ObjectManager();
    QuestManager  _quest  = new QuestManager();
    MinMonManager _minMon = new MinMonManager();
    TimeManager   _time;

    public static GameManager   Game   { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    public static QuestManager  Quest  { get { return Instance?._quest; } }
    public static MinMonManager MinMon { get { return Instance?._minMon; } }
    public static TimeManager   Time   { get { return Instance?._time; } }
    #endregion

    #region Core
    DataManager    _data     = new DataManager();
    PoolManager    _pool     = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx  _scene    = new SceneManagerEx();
    SoundManager    _sound    = new SoundManager();
    UIManager       _ui       = new UIManager();

    public static DataManager    Data     { get { return Instance?._data; } }
    public static PoolManager    Pool     { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SceneManagerEx  Scene    { get { return Instance?._scene; } }
    public static SoundManager    Sound    { get { return Instance?._sound; } }
    public static UIManager       UI       { get { return Instance?._ui; } }
    #endregion

    public static void Init()
    {
        if (s_instance != null)
            return;

        GameObject go = GameObject.Find("@Managers");
        if (go == null)
        {
            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();
        }

        DontDestroyOnLoad(go);
        s_instance = go.GetComponent<Managers>();
        s_instance._sound.Init();
        s_instance._time = go.AddComponent<TimeManager>();
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
