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
}