using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioDirector : MonoBehaviour
{
    class Channel
    {
        public AudioSource src;
        AudioClip pending; float pendingVol, target; bool hasPending;

        public void Set(AudioClip clip, float vol)
        {
            if (clip != null && src.clip == clip && src.isPlaying) { target = vol; hasPending = false; return; }
            if (clip == null && src.clip == null) { target = 0f; hasPending = false; return; }
            pending = clip; pendingVol = vol; hasPending = true; target = 0f;
        }

        public void Tick(float dt, float fade)
        {
            src.volume = Mathf.MoveTowards(src.volume, target, dt / Mathf.Max(0.05f, fade));
            if (hasPending && src.volume <= 0.001f)
            {
                hasPending = false;
                src.Stop(); src.clip = pending;
                if (pending != null) { src.loop = true; src.Play(); target = pendingVol; }
                else target = 0f;
            }
            else if (!hasPending && target <= 0f && src.volume <= 0.001f && src.isPlaying) src.Stop();
        }
    }

    static AudioDirector instance;

    AudioCatalog catalog;
    GameFlowSettings flow;
    Channel music, ambience;
    AudioSource hum;
    readonly List<Light> lamps = new List<Light>();
    Transform listener;
    Vector3 humTarget;
    float nextScan, nextNearest;
    bool humOn, hasLamp;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null) return;
        var go = new GameObject("AudioDirector");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AudioDirector>();
    }

    /// <summary>Créditos: suena la música del menú (u outro si hay una distinta).</summary>
    public static void PlayOutro()
    {
        if (instance == null || instance.catalog == null) return;
        var c = instance.catalog;
        instance.music.Set(c.outroMusic != null ? c.outroMusic : c.menuMusic, c.musicVolume);
        instance.ambience.Set(null, 0f);
        instance.humOn = false;
    }

    void Awake()
    {
        catalog = Resources.Load<AudioCatalog>("AudioCatalog");
        flow = GameFlowSettings.Load();
        music = MakeChannel("Music");
        ambience = MakeChannel("Ambience");

        var h = new GameObject("Hum");
        h.transform.SetParent(transform);
        hum = h.AddComponent<AudioSource>();
        hum.loop = true; hum.playOnAwake = false; hum.spatialBlend = 1f;
        hum.minDistance = 1.5f; hum.maxDistance = 14f;
        hum.rolloffMode = AudioRolloffMode.Linear; hum.dopplerLevel = 0f; hum.volume = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
        if (catalog == null) Debug.LogWarning("Falta Resources/AudioCatalog");
        Apply(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    Channel MakeChannel(string n)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform);
        var s = go.AddComponent<AudioSource>();
        s.playOnAwake = false; s.loop = true; s.spatialBlend = 0f; s.volume = 0f;
        return new Channel { src = s };
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m) => Apply(s);

    void Apply(Scene scene)
    {
        if (catalog == null) return;
        lamps.Clear(); hasLamp = false; humOn = false;

        if (scene.name == flow.menuScene)
        {
            music.Set(catalog.menuMusic, catalog.musicVolume);
            ambience.Set(null, 0f);
            return;
        }

        music.Set(null, 0f);    // en los niveles suena la música de tensión (ProximityMusic)
        AudioCatalog.SceneAudio entry = null;
        if (catalog.scenes != null)
            foreach (var e in catalog.scenes)
                if (e != null && e.sceneName == scene.name) { entry = e; break; }

        if (entry == null) { ambience.Set(null, 0f); return; }
        ambience.Set(entry.ambience, entry.ambienceVolume);

        humOn = entry.fluorescentHum && catalog.humClip != null;
        if (humOn)
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type != LightType.Directional) lamps.Add(l);
    }

    void Update()
    {
        if (catalog == null) return;
        float dt = Time.unscaledDeltaTime;
        music.Tick(dt, catalog.fadeTime);
        ambience.Tick(dt, catalog.fadeTime);
        UpdateHum(dt);

        if (Time.unscaledTime >= nextScan)
        {
            nextScan = Time.unscaledTime + 1f;
            var pc = FindAnyObjectByType<PlayerController>();
            listener = pc != null ? pc.transform : null;
            if (pc != null && !pc.TryGetComponent<PlayerFootsteps>(out _))
                pc.gameObject.AddComponent<PlayerFootsteps>();
        }
    }

    // Un solo emisor que sigue a la luz encendida más cercana al jugador
    void UpdateHum(float dt)
    {
        if (!humOn || listener == null)
        {
            hum.volume = Mathf.MoveTowards(hum.volume, 0f, dt);
            if (hum.volume <= 0.001f && hum.isPlaying) hum.Stop();
            return;
        }
        if (Time.unscaledTime >= nextNearest)
        {
            nextNearest = Time.unscaledTime + 0.3f;
            Light best = null; float bd = float.MaxValue;
            foreach (var l in lamps)
            {
                if (l == null || !l.isActiveAndEnabled || l.intensity < 0.05f) continue;   // un foco reventado no zumba
                float d = (l.transform.position - listener.position).sqrMagnitude;
                if (d < bd) { bd = d; best = l; }
            }
            if (best != null)
            {
                if (!hasLamp) hum.transform.position = best.transform.position;
                humTarget = best.transform.position;
            }
            hasLamp = best != null;
        }
        if (hasLamp && !hum.isPlaying) { hum.clip = catalog.humClip; hum.Play(); }
        hum.transform.position = Vector3.Lerp(hum.transform.position, humTarget, 1f - Mathf.Exp(-6f * dt));
        hum.volume = Mathf.MoveTowards(hum.volume, hasLamp ? catalog.humVolume : 0f, dt * 0.8f);
    }
}