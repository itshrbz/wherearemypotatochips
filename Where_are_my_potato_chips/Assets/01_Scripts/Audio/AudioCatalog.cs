using UnityEngine;

[CreateAssetMenu(menuName = "WAMC/Audio Catalog", fileName = "AudioCatalog")]
public class AudioCatalog : ScriptableObject
{
    [System.Serializable]
    public class SceneAudio
    {
        public string sceneName;                       // 02_Store, 10_Warehouse, 11_Pools, 12_Playground
        public AudioClip ambience;
        [Range(0, 1)] public float ambienceVolume = 0.5f;
        public bool fluorescentHum;
    }

    [Header("Música")]
    public AudioClip menuMusic;
    [Tooltip("Si queda vacío, los créditos usan la música del menú.")]
    public AudioClip outroMusic;
    [Range(0, 1)] public float musicVolume = 0.7f;

    [Header("Zumbido fluorescente (un emisor que sigue a la luz más cercana)")]
    public AudioClip humClip;
    [Range(0, 1)] public float humVolume = 0.35f;

    [Header("Ambiente por escena")]
    public SceneAudio[] scenes;

    public float fadeTime = 1.5f;
}