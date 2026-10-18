using System.Collections.Generic;
using UnityEngine;

public static class Sfx
{
    static Dictionary<string, SfxBank.Entry> map;
    static AudioSource[] pool;
    static int next;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { map = null; pool = null; }

    static void Init()
    {
        map = new Dictionary<string, SfxBank.Entry>();
        var bank = Resources.Load<SfxBank>("SfxBank");
        if (bank) foreach (var e in bank.entries) map[e.id] = e;
        else Debug.LogWarning("Falta Resources/SfxBank");
        var root = new GameObject("SfxPool");
        Object.DontDestroyOnLoad(root);
        pool = new AudioSource[16];
        for (int i = 0; i < pool.Length; i++)
        {
            var go = new GameObject("S" + i);
            go.transform.SetParent(root.transform);
            pool[i] = go.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
        }
    }

    // 3D en una posición
    public static void Play(string id, Vector3 pos) => Go(id, pos, 1f);
    // 2D (UI, caja, golpe de captura)
    public static void Play(string id) => Go(id, Vector3.zero, 0f);

    static void Go(string id, Vector3 pos, float spatial)
    {
        if (map == null || pool == null || pool[0] == null) Init();
        if (!map.TryGetValue(id, out var e) || e.clips == null || e.clips.Length == 0)
        { Debug.LogWarning("Sfx sin sonido: " + id); return; }
        var s = pool[next++ % pool.Length];
        s.transform.position = pos;
        s.spatialBlend = spatial;
        s.volume = e.volume;
        s.pitch = Random.Range(e.pitch.x, e.pitch.y);
        s.clip = e.clips[Random.Range(0, e.clips.Length)];
        s.Play();
    }
}