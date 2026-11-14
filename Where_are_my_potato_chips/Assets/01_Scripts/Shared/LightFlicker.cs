using UnityEngine;

/// <summary>
/// Tubo fluorescente viejo. Tres ritmos: un zumbido casi invisible, parpadeos cada tanto,
/// o un tubo que está fallando y pasa más tiempo apagado que prendido.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public enum Mode
    {
        /// <summary>Variación mínima y continua, como la corriente de un tubo sano.</summary>
        Hum,
        /// <summary>Ráfagas de 2 a 5 parpadeos separadas por varios segundos.</summary>
        Flicker,
        /// <summary>Casi siempre apagado, con encendidos cortos e inestables.</summary>
        Dying,
    }

    [SerializeField] Mode mode = Mode.Flicker;
    [Tooltip("Segundos mínimos entre ráfagas (Flicker) o entre encendidos (Dying).")]
    [SerializeField] float minWait = 2f;
    [Tooltip("Segundos máximos entre ráfagas (Flicker) o entre encendidos (Dying).")]
    [SerializeField] float maxWait = 7f;
    [Tooltip("Cuánto baja la intensidad en el zumbido (0.05 = 5 %).")]
    [SerializeField, Range(0f, 0.3f)] float humAmount = 0.06f;
    [Tooltip("Intensidad que queda al apagarse, como fracción de la normal.")]
    [SerializeField, Range(0f, 1f)] float offLevel = 0.1f;

    Light lamp;
    float baseIntensity;
    float nextChange;
    int flashesLeft;
    float noiseSeed;

    /// <summary>Configura el ritmo desde código (herramientas de editor o generadores).</summary>
    public void Configure(Mode newMode, float waitMin, float waitMax)
    {
        mode = newMode;
        minWait = waitMin;
        maxWait = waitMax;
    }

    void Awake()
    {
        lamp = GetComponent<Light>();
        baseIntensity = lamp.intensity;
        noiseSeed = Random.value * 100f;
        nextChange = Time.time + Random.Range(minWait, maxWait);
    }

    void Update()
    {
        switch (mode)
        {
            case Mode.Hum:
                // Ruido suave en vez de aleatorio por frame, para que no titile como estrobo
                float n = Mathf.PerlinNoise(noiseSeed, Time.time * 8f);
                lamp.intensity = baseIntensity * (1f - humAmount * n);
                break;
            case Mode.Flicker:
                UpdateBursts(Random.Range(2, 6), restOn: true);
                break;
            case Mode.Dying:
                UpdateBursts(Random.Range(3, 9), restOn: false);
                break;
        }
    }

    // Ráfaga de cambios rápidos; al terminar, la luz descansa prendida (Flicker) o apagada (Dying)
    void UpdateBursts(int burstLength, bool restOn)
    {
        if (Time.time < nextChange) return;

        if (flashesLeft == 0) flashesLeft = burstLength;
        flashesLeft--;

        bool off = flashesLeft % 2 == 1;
        lamp.intensity = off ? baseIntensity * offLevel : baseIntensity;

        if (flashesLeft > 0)
        {
            nextChange = Time.time + Random.Range(0.03f, restOn ? 0.12f : 0.25f);
            return;
        }

        lamp.intensity = restOn ? baseIntensity : baseIntensity * offLevel;
        nextChange = Time.time + Random.Range(minWait, maxWait);
    }
}
