using UnityEngine;

/// <summary>Parpadeo irregular de un tubo fluorescente viejo.</summary>
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [SerializeField] float minWait = 2f;
    [SerializeField] float maxWait = 7f;

    Light lamp;
    float baseIntensity;
    float nextFlicker;
    int flashesLeft;

    void Awake()
    {
        lamp = GetComponent<Light>();
        baseIntensity = lamp.intensity;
        nextFlicker = Time.time + Random.Range(minWait, maxWait);
    }

    void Update()
    {
        if (Time.time < nextFlicker) return;

        if (flashesLeft == 0) flashesLeft = Random.Range(2, 6);
        flashesLeft--;

        bool off = flashesLeft % 2 == 1;
        lamp.intensity = off ? baseIntensity * 0.1f : baseIntensity;
        nextFlicker = flashesLeft > 0
            ? Time.time + Random.Range(0.03f, 0.12f)
            : Time.time + Random.Range(minWait, maxWait);
        if (flashesLeft == 0) lamp.intensity = baseIntensity;
    }
}
