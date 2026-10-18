using UnityEngine;

[CreateAssetMenu(menuName = "WAMC/Sfx Bank")]
public class SfxBank : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string id;
        public AudioClip[] clips;
        [Range(0, 1)] public float volume = 1f;
        public Vector2 pitch = new Vector2(0.95f, 1.05f);
        [Tooltip("Distancia a la que el sonido 3D deja de oírse")]
        public float maxDistance = 30f;
    }
    public Entry[] entries;

    void OnValidate()
    {
        if (entries == null) return;
        foreach (var e in entries)
        {
            if (e == null) continue;
            if (e.volume <= 0f) e.volume = 1f;
            if (e.pitch.x <= 0f || e.pitch.y <= 0f) e.pitch = new Vector2(0.95f, 1.05f);
            if (e.maxDistance <= 0f) e.maxDistance = 30f;
            if (e.id != null) e.id = e.id.Trim();
        }
    }
}