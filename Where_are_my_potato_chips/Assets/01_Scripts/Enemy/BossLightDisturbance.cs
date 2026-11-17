using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hace fallar las luces cercanas al jefe, para que el jugador note que se acerca antes de verlo.
/// Apaga y prende la luz (Light.enabled) en vez de tocar su intensidad, así no pelea con LightFlicker.
/// BossAI lo agrega solo si el prefab no lo tiene.
/// </summary>
public class BossLightDisturbance : MonoBehaviour
{
    [Tooltip("Distancia a la que el jefe empieza a hacer fallar las luces.")]
    [SerializeField] float radius = 9f;
    [Tooltip("Probabilidad de que una luz quede apagada en cada cambio, con el jefe justo debajo.")]
    [SerializeField, Range(0f, 1f)] float maxOffChance = 0.6f;
    [Tooltip("Segundos entre cambios. Muy bajo se vuelve un estroboscopio.")]
    [SerializeField] float minInterval = 0.08f;
    [SerializeField] float maxInterval = 0.3f;

    readonly List<Light> lights = new List<Light>();
    readonly HashSet<Light> disturbed = new HashSet<Light>();
    float next;

    void Start()
    {
        // Solo luces del nivel que ya estaban prendidas: ni el sol ni las del jugador
        foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (light.enabled && light.type != LightType.Directional && light.GetComponentInParent<PlayerState>() == null)
                lights.Add(light);
    }

    void Update()
    {
        if (Time.time < next) return;
        next = Time.time + Random.Range(minInterval, maxInterval);

        Vector3 position = transform.position;
        foreach (var light in lights)
        {
            if (light == null) continue;
            float distance = Vector3.Distance(position, light.transform.position);
            if (distance > radius)
            {
                if (disturbed.Remove(light)) light.enabled = true;
                continue;
            }
            disturbed.Add(light);
            light.enabled = Random.value > maxOffChance * (1f - distance / radius);
        }
    }

    void OnDisable()
    {
        foreach (var light in disturbed)
            if (light != null) light.enabled = true;
        disturbed.Clear();
    }
}
