using UnityEngine;

public class BossAudio : MonoBehaviour
{
    public BossAI ai;
    public ProceduralLeg[] legs;
    public WallHand[] hands;
    [Header("Respiración (loop)")]
    public AudioClip breathLoop;
    [Range(0, 1)] public float breathPatrol = 0.25f, breathChase = 0.7f;

    AudioSource breath;

    void Awake()
    {
        if (ai == null) ai = GetComponentInParent<BossAI>();
        if (breathLoop == null) return;
        breath = gameObject.AddComponent<AudioSource>();
        breath.clip = breathLoop; breath.loop = true; breath.spatialBlend = 1f;
        breath.minDistance = 2f; breath.maxDistance = 20f;
        breath.rolloffMode = AudioRolloffMode.Linear; breath.dopplerLevel = 0f;
        breath.volume = breathPatrol;
        breath.Play();
    }

    void OnEnable()
    {
        if (legs != null) foreach (var l in legs) if (l != null) l.Planted += OnStep;
        if (hands != null) foreach (var h in hands) if (h != null) h.Planted += OnHand;
        if (ai != null) ai.PlayerCaught += OnCaught;
    }

    void OnDisable()
    {
        if (legs != null) foreach (var l in legs) if (l != null) l.Planted -= OnStep;
        if (hands != null) foreach (var h in hands) if (h != null) h.Planted -= OnHand;
        if (ai != null) ai.PlayerCaught -= OnCaught;
    }

    void Update()
    {
        if (breath != null && ai != null)
            breath.volume = Mathf.Lerp(breath.volume, ai.IsChasing ? breathChase : breathPatrol, 3f * Time.deltaTime);
    }

    float Loudness => ai != null && ai.IsChasing ? 1f : 0.7f;
    void OnStep(Vector3 p) => Sfx.Play("boss_step", p, Loudness);
    void OnHand(Vector3 p) => Sfx.Play("boss_hand", p, Loudness);
    void OnCaught() => Sfx.Play("boss_catch");
}