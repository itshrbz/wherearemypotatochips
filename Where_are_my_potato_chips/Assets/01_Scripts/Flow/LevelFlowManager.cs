using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Orden de la partida: tienda real -> 3 espacios en orden aleatorio -> tienda real (final).
/// Se crea sola al darle Play en cualquier escena.
/// </summary>
public class LevelFlowManager : MonoBehaviour
{
    static LevelFlowManager instance;

    GameFlowSettings settings;
    SceneLoader loader;
    readonly List<string> order = new List<string>();
    int levelIndex = -1;
    bool ending;

    public static LevelFlowManager Instance
    {
        get
        {
            if (instance == null) new GameObject("GameFlow").AddComponent<LevelFlowManager>();
            return instance;
        }
    }

    public static int CurrentLevel => Instance.levelIndex;
    public static bool IsEnding => Instance.ending;
    public static GameFlowSettings Settings => Instance.settings;
    public static bool IsLoading => Instance.loader.IsBusy;

    public static void StartNewGame() => Instance.LoadStore(false);
    public static void StartRun() => Instance.BeginRun();
    public static void GoToNextLevel() => Instance.Advance();
    public static void RestartLevel(bool playSting = true) => Instance.Restart(playSting);
    public static void BackToMenu() => Instance.LoadMenu();
    public static void FinishGame() => Instance.StartCoroutine(Instance.CreditsRoutine());

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateOnPlay() => _ = Instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        settings = GameFlowSettings.Load();
        GameSettings.ApplyAudio();
        loader = gameObject.AddComponent<SceneLoader>();
        loader.Setup(settings);
        ShuffleOrder();

        Scene active = SceneManager.GetActiveScene();
        if (IsLiminal(active.name))
        {
            levelIndex = Mathf.Clamp(settings.directPlayLevelIndex, 0, order.Count - 1);
            order.Remove(active.name);
            order.Insert(levelIndex, active.name);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        PrepareScene(active);
    }

    void OnDestroy()
    {
        if (instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PrepareScene(scene);

    void PrepareScene(Scene scene)
    {
        bool liminal = IsLiminal(scene.name);
        bool store = scene.name == settings.storeScene;
        if (!liminal && !store) return;

        var context = LevelContext.Current != null
            ? LevelContext.Current
            : new GameObject("LevelContext").AddComponent<LevelContext>();

        if (liminal) context.EnsureReplica(settings.replicaStorePrefab);
        context.EnsurePlayer(settings.playerPrefab);
        if (liminal) HidingSpotGenerator.EnsureSpots();
        if (liminal) context.SetupBoss(settings.bossPrefab);

        var player = FindAnyObjectByType<PlayerController>();
        if (player != null && player.GetComponent<CameraEffects>() == null) player.gameObject.AddComponent<CameraEffects>();
        if (liminal) new GameObject("ScreenEffects").AddComponent<ScreenEffects>();
        if (store) PlayerAreaLimit.ForStore();

        if (settings.hudPrefab != null && FindAnyObjectByType<GameHud>() == null)
            Instantiate(settings.hudPrefab).AddComponent<GameHud>();

        if (FindAnyObjectByType<PauseMenu>() == null)
            new GameObject("PauseMenu").AddComponent<PauseMenu>();

        if (liminal && settings.tensionMusic != null && FindAnyObjectByType<ProximityMusic>() == null)
            new GameObject("TensionMusic").AddComponent<ProximityMusic>().Play(settings.tensionMusic);

        if (store && ending)
        {
            var queue = FindAnyObjectByType<BuyerQueue>();
            if (queue != null) queue.enabled = false;
            context.gameObject.AddComponent<StoreEnding>();
        }
    }

    void LoadMenu()
    {
        levelIndex = -1;
        ending = false;
        loader.Load(settings.menuScene, SceneLoader.Reveal.Fade);
    }

    void LoadStore(bool asEnding)
    {
        levelIndex = -1;
        ending = asEnding;
        loader.Load(settings.storeScene, SceneLoader.Reveal.Fade);
    }

    void BeginRun()
    {
        // Solo desde la tienda: una puerta de almacén dentro de un nivel no reinicia la partida
        if (loader.IsBusy || levelIndex >= 0) return;
        ShuffleOrder();
        levelIndex = 0;
        ending = false;
        loader.Load(order[0], SceneLoader.Reveal.WakeUp);
    }

    void Advance()
    {
        if (loader.IsBusy || levelIndex < 0) return;

        string message = TicketMessage(levelIndex);
        levelIndex++;

        if (levelIndex >= order.Count)
        {
            levelIndex = -1;
            ending = true;
            loader.Load(settings.storeScene, SceneLoader.Reveal.WakeUp, message);
            return;
        }
        loader.Load(order[levelIndex], SceneLoader.Reveal.WakeUp, message);
    }

    void Restart(bool playSting)
    {
        if (loader.IsBusy) return;
        loader.Load(SceneManager.GetActiveScene().name, SceneLoader.Reveal.WakeUp, null, playSting ? settings.caughtClip : null);
    }

    IEnumerator CreditsRoutine()
    {
        AudioDirector.PlayOutro();
        yield return loader.ShowCredits(settings.credits, 7f, settings.registerClip);
        LoadMenu();
    }

    string TicketMessage(int finishedLevel)
    {
        var messages = settings.ticketMessages;
        if (messages == null || messages.Length == 0) return " ";
        return messages[Mathf.Min(finishedLevel, messages.Length - 1)];
    }

    void ShuffleOrder()
    {
        order.Clear();
        order.AddRange(settings.liminalScenes);
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
    }

    bool IsLiminal(string sceneName) => System.Array.IndexOf(settings.liminalScenes, sceneName) >= 0;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.f2Key.wasPressedThisFrame)
        {
            if (levelIndex < 0) BeginRun();
            else Advance();
        }
        if (keyboard.f3Key.wasPressedThisFrame) Restart(true);
    }
#endif
}
