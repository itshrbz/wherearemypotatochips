using UnityEngine;

/// <summary>Música de tensión que sube y se abre (filtro) a medida que el jefe se acerca.</summary>
[RequireComponent(typeof(AudioSource))]
public class ProximityMusic : MonoBehaviour
{
    [SerializeField] float silentDistance = 25f;
    [SerializeField] float loudDistance = 4f;
    [SerializeField] float maxVolume = 0.8f;
    [SerializeField] float response = 2f;

    AudioSource source;
    AudioLowPassFilter lowPass;
    BossAI boss;
    Transform listener;
    float nextLookup;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = 600f;
    }

    public void Play(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }

    void Update()
    {
        if ((boss == null || listener == null) && Time.time >= nextLookup)
        {
            nextLookup = Time.time + 1f;
            boss = FindAnyObjectByType<BossAI>();
            var player = GameObject.FindWithTag("Player");
            listener = player != null ? player.transform : null;
        }

        float closeness = 0f;
        if (boss != null && listener != null && boss.isActiveAndEnabled)
        {
            float distance = Vector3.Distance(boss.transform.position, listener.position);
            closeness = 1f - Mathf.InverseLerp(loudDistance, silentDistance, distance);
            if (boss.IsChasing) closeness = Mathf.Max(closeness, 0.6f);
        }

        float blend = 1f - Mathf.Exp(-response * Time.deltaTime);
        source.volume = Mathf.Lerp(source.volume, closeness * maxVolume, blend);
        lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, Mathf.Lerp(600f, 22000f, closeness), blend);
    }
}
