using System.Collections;
using UnityEngine;

/// <summary>
/// Efectos de cámara del jugador: balanceo y algo de FOV al correr, y el susto cuando el jefe lo atrapa.
/// Corre antes que PlayerController para quitar el balanceo del frame anterior y lo vuelve a sumar en LateUpdate.
/// Con "Camera effects: Off" en la configuración no hay balanceo ni sacudidas. <see cref="Shake"/> sirve para golpes cortos.
/// </summary>
[DefaultExecutionOrder(-100)]
public class CameraEffects : MonoBehaviour
{
    [Header("Correr")]
    [Tooltip("Pasos completos (izquierdo + derecho) por segundo.")]
    [SerializeField] float bobFrequency = 1.5f;
    [SerializeField] float bobHeight = 0.05f;
    [SerializeField] float bobSway = 0.035f;
    [SerializeField] float bobRoll = 0.8f;
    [SerializeField] float shakeAngle = 0.35f;
    [Tooltip("Grados de FOV que se suman al correr. Negativo = zoom hacia adentro.")]
    [SerializeField] float sprintFovKick = 6f;
    [SerializeField] float blendSpeed = 4f;

    [Header("Captura")]
    [SerializeField] float screamerDuration = 1f;
    [SerializeField] float screamerFov = 35f;
    [SerializeField] float screamerShake = 2.5f;
    [SerializeField] float screamerPush = 0.3f;

    Camera view;
    Transform cam;
    PlayerController controller;
    CharacterController body;

    Vector3 appliedOffset;
    Quaternion appliedRotation = Quaternion.identity;
    float sprintBlend;
    float bobPhase;
    bool screaming;
    float shakeTime;
    float shakeDuration;
    float shakeStrength;

    static CameraEffects active;

    /// <summary>
    /// Sacudida corta de la cámara del jugador (un golpe de caja, un susto). Se apaga sola con
    /// "Camera effects: Off". No hace nada si no hay jugador en la escena.
    /// </summary>
    public static void Shake(float degrees, float seconds)
    {
        if (active == null || !GameSettings.CameraEffects || seconds <= 0f) return;
        // Una sacudida fuerte no la pisa una más suave que llega justo después
        if (active.shakeTime > 0f && degrees < active.shakeStrength * (active.shakeTime / active.shakeDuration)) return;
        active.shakeStrength = degrees;
        active.shakeTime = active.shakeDuration = seconds;
    }

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        body = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>();
        if (view != null) cam = view.transform;
    }

    void OnEnable() => active = this;

    void OnDisable()
    {
        if (active == this) active = null;
    }

    void Update()
    {
        if (cam == null || screaming) return;
        RemoveOffsets();
    }

    void LateUpdate()
    {
        if (cam == null || screaming) return;

        bool sprinting = controller != null && controller.IsSprinting && body != null && body.isGrounded;
        sprintBlend = Mathf.MoveTowards(sprintBlend, sprinting ? 1f : 0f, blendSpeed * Time.deltaTime);

        float intensity = GameSettings.CameraEffects ? sprintBlend : 0f;
        view.fieldOfView = GameSettings.FieldOfView + sprintFovKick * intensity;

        if (intensity > 0f)
        {
            bobPhase += Time.deltaTime * bobFrequency * Mathf.PI * 2f;
            float step = Mathf.Sin(bobPhase);
            float bounce = (Mathf.Cos(bobPhase * 2f) - 1f) * 0.5f;
            float noise = Time.time * 9f;

            appliedOffset = new Vector3(step * bobSway, bounce * bobHeight, 0f) * intensity;
            appliedRotation = Quaternion.Euler(
                (Mathf.PerlinNoise(noise, 0f) - 0.5f) * 2f * shakeAngle * intensity,
                (Mathf.PerlinNoise(0f, noise) - 0.5f) * 2f * shakeAngle * intensity,
                step * bobRoll * intensity);
        }

        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            // Decae rápido: el golpe se siente al principio y no marea
            float k = Mathf.Clamp01(shakeTime / shakeDuration);
            float amount = GameSettings.CameraEffects ? shakeStrength * k * k : 0f;
            float t = Time.time * 35f;
            appliedRotation *= Quaternion.Euler(
                (Mathf.PerlinNoise(t, 3.1f) - 0.5f) * 2f * amount,
                (Mathf.PerlinNoise(7.7f, t) - 0.5f) * 2f * amount,
                (Mathf.PerlinNoise(t, t * 0.5f) - 0.5f) * amount);
        }

        if (appliedOffset == Vector3.zero && appliedRotation == Quaternion.identity) return;
        cam.localPosition += appliedOffset;
        cam.localRotation *= appliedRotation;
    }

    void RemoveOffsets()
    {
        cam.localPosition -= appliedOffset;
        cam.localRotation *= Quaternion.Inverse(appliedRotation);
        appliedOffset = Vector3.zero;
        appliedRotation = Quaternion.identity;
    }

    /// <summary>La cámara gira hacia la cara del jefe, se le acerca de golpe y tiembla. Termina con un destello.</summary>
    public IEnumerator Screamer(BossAI boss)
    {
        if (cam == null)
        {
            yield return new WaitForSeconds(0.6f);
            yield break;
        }

        RemoveOffsets();
        screaming = true;

        var clip = LevelFlowManager.Settings != null ? LevelFlowManager.Settings.caughtClip : null;
        if (clip != null) AudioSource.PlayClipAtPoint(clip, cam.position);

        float startFov = view.fieldOfView;
        Quaternion startRotation = cam.rotation;
        Vector3 startPosition = cam.localPosition;
        float shake = GameSettings.CameraEffects ? screamerShake : 0f;

        for (float t = 0f; t < 1f; t += Time.deltaTime / screamerDuration)
        {
            Vector3 toFace = boss.EyePosition - cam.position;
            Quaternion look = toFace.sqrMagnitude > 0.01f ? Quaternion.LookRotation(toFace) : startRotation;

            float turn = 1f - Mathf.Pow(1f - Mathf.Clamp01(t * 5f), 3f);
            float zoom = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.45f, t));

            Vector3 jitter = Random.insideUnitSphere * shake * zoom;
            cam.rotation = Quaternion.Slerp(startRotation, look, turn) * Quaternion.Euler(jitter);
            cam.localPosition = startPosition + cam.parent.InverseTransformDirection(cam.forward) * screamerPush * zoom;
            view.fieldOfView = Mathf.Lerp(startFov, screamerFov, zoom);

            if (t > 0.75f) ScreenEffects.Flash(new Color(0.5f, 0f, 0f, 0.8f), 0.4f);
            yield return null;
        }
    }
}
