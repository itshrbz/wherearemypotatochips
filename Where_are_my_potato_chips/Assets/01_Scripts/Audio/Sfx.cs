using System.Collections.Generic;
using UnityEngine;

public static class Sfx
{
    static Dictionary<string, SfxBank.Entry> map;
    static Dictionary<string, int> last;
    static HashSet<string> warned;
    static AudioSource[] pool;
    static int next;
    static bool checkedListener;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { map = null; pool = null; last = null; warned = null; checkedListener = false; }

    static void Ensure()
    {
        if (map != null && pool != null && pool[0] != null) return;
        map = new Dictionary<string, SfxBank.Entry>();
        last = new Dictionary<string, int>();
        warned = new HashSet<string>();

        var bank = Resources.Load<SfxBank>("SfxBank");
        if (bank == null)
            Debug.LogError("Sfx: no encuentro 'SfxBank'. Debe llamarse exactamente SfxBank y estar dentro de una carpeta llamada Resources (ej. Assets/Resources/SfxBank.asset).");
        else if (bank.entries == null || bank.entries.Length == 0)
            Debug.LogError("Sfx: el SfxBank está vacío (Entries sin elementos).");
        else
        {
            foreach (var e in bank.entries)
                if (e != null && !string.IsNullOrWhiteSpace(e.id)) map[e.id.Trim()] = e;
            Debug.Log("Sfx: SfxBank cargado con " + map.Count + " ids: " + string.Join(", ", map.Keys));
        }

        var root = new GameObject("SfxPool");
        Object.DontDestroyOnLoad(root);
        pool = new AudioSource[24];
        for (int i = 0; i < pool.Length; i++)
        {
            var go = new GameObject("S" + i);
            go.transform.SetParent(root.transform);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.dopplerLevel = 0f;
            s.rolloffMode = AudioRolloffMode.Linear;
            pool[i] = s;
        }
    }

    public static bool Has(string id)
    {
        Ensure();
        return map.TryGetValue(id, out var e) && e.clips != null && e.clips.Length > 0;
    }

    public static void Play(string id) => Go(id, Vector3.zero, 0f, 1f);                         // 2D
    public static void Play(string id, float volumeScale) => Go(id, Vector3.zero, 0f, volumeScale);
    public static void Play(string id, Vector3 pos) => Go(id, pos, 1f, 1f);                     // 3D
    public static void Play(string id, Vector3 pos, float volumeScale) => Go(id, pos, 1f, volumeScale);

    static void Go(string id, Vector3 pos, float spatial, float scale)
    {
        Ensure();
        if (!checkedListener)
        {
            checkedListener = true;
            if (Object.FindAnyObjectByType<AudioListener>() == null)
                Debug.LogError("Sfx: no hay ningún AudioListener en la escena (normalmente va en la cámara).");
            if (AudioListener.volume <= 0f)
                Debug.LogWarning("Sfx: AudioListener.volume está en 0. Revisa el volumen en Settings del juego.");
        }

        if (!map.TryGetValue(id, out var e))
        {
            if (warned.Add(id)) Debug.LogWarning("Sfx: el id '" + id + "' no existe en el SfxBank (revisa mayúsculas y espacios).");
            return;
        }
        if (e.clips == null || e.clips.Length == 0)
        {
            if (warned.Add(id)) Debug.LogWarning("Sfx: el id '" + id + "' existe pero no tiene clips.");
            return;
        }

        int len = e.clips.Length;
        int i = Random.Range(0, len);
        if (len > 1 && last.TryGetValue(id, out int prev))
            i = (prev + 1 + Random.Range(0, len - 1)) % len;
        last[id] = i;

        AudioClip clip = e.clips[i];
        if (clip == null)
        {
            if (warned.Add(id + "#" + i)) Debug.LogWarning("Sfx: el id '" + id + "' tiene un clip vacío en la posición " + i + ".");
            return;
        }

        float vol = e.volume > 0f ? e.volume : 1f;
        float p0 = e.pitch.x > 0f ? e.pitch.x : 0.95f;
        float p1 = e.pitch.y > 0f ? e.pitch.y : 1.05f;
        float maxD = e.maxDistance > 0f ? e.maxDistance : 30f;

        var s = pool[next++ % pool.Length];
        s.Stop();
        s.transform.position = pos;
        s.spatialBlend = spatial;
        s.minDistance = 2f;
        s.maxDistance = Mathf.Max(3f, maxD);
        s.volume = Mathf.Clamp01(vol * scale);
        s.pitch = Random.Range(p0, p1);
        s.clip = clip;
        s.Play();
    }
}