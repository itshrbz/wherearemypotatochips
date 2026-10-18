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
    }
    public Entry[] entries;
}