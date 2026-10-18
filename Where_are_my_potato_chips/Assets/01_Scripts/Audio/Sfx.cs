using System.Collections.Generic;
using UnityEngine;

public static class Sfx
{
    static Dictionary<string, SfxBank.Entry> map;
    static Dictionary<string, int> last;
    static HashSet<string> warned;
    static AudioSource[] pool;
    static int next;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { map = null; pool = null; last = null; warned = null; }

    static void Ensure()
    {
        if (map != null && pool != null && pool[0] != null) return;
        map = new Dictionary<string, SfxBank.Entry>();
        last = new Dictionary<string, int>();
        warned = new HashSet<string>();
        var bank = Resources.Load<SfxBank>("SfxBank");
        if (bank != null && bank.entries != null)
        {
            foreach (var e in bank.entries)
                if (!string.IsNullOrEmpty(e.id)) map[e.id] = e;
        }
        else Debug.LogWarning("Falta Resources/SfxBank");

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

    public static void Play(string id) => Go(id, Vector3.zero, 0f, 1f);                       // 2D
    public static void Play(string id, float volumeScale) => Go(id, Vector3.zero, 0f, volumeScale);
    public static void Play(string id, Vector3 pos) => Go(id, pos, 1f, 1f);                   // 3D
    public static void Play(string id, Vector3 pos, float volumeScale) => Go(id, pos, 1f, volumeScale);

    static void Go(string id, Vector3 pos, float spatial, float scale)
    {
        Ensure();
        if (!map.TryGetValue(id, out var e) || e.clips == null || e.clips.Length == 0)
        {
            if (warned.Add(id)) Debug.LogWarning("Sfx sin sonido: " + id);
            return;
        }
        int len = e.clips.Length;
        int i = Random.Range(0, len);
        if (len > 1 && last.TryGetValue(id, out int prev))
            i = (prev + 1 + Random.Range(0, len - 1)) % len;     // cualquiera menos el anterior
        last[id] = i;

        var s = pool[next++ % pool.Length];
        s.Stop();
        s.transform.position = pos;
        s.spatialBlend = spatial;
        s.minDistance = 2f;
        s.maxDistance = Mathf.Max(3f, e.maxDistance);
        s.volume = Mathf.Clamp01(e.volume * scale);
        s.pitch = Random.Range(e.pitch.x, e.pitch.y);
        s.clip = e.clips[i];
        s.Play();
    }
}