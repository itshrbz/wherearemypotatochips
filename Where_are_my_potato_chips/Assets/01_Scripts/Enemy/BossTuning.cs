using UnityEngine;

/// <summary>
/// Carácter del jefe en un espacio: velocidad, sentidos y búsqueda.
/// Un asset por espacio (almacén, piscinas, parque); BossAI lo elige por la escena. Sin asset usa los valores del prefab.
/// </summary>
[CreateAssetMenu(menuName = "Potato Chips/Boss Tuning", fileName = "BossTuning")]
public class BossTuning : ScriptableObject
{
    [Header("Velocidad (el jugador camina a 2.5 y corre a 4.5)")]
    [Tooltip("Investigar = ×1.15 y buscar = ×1.3 de este valor.")]
    public float patrolSpeed = 2.6f;
    [Tooltip("Persecución × arranque debería quedar por debajo de 4.5 para que correr sirva.")]
    public float chaseSpeed = 3.9f;
    public float burstMultiplier = 1.1f;

    [Header("Sentidos")]
    public float sightRange = 18f;
    [Tooltip("Más cerca que esto, lo detecta al instante.")]
    public float instantSightRange = 4.5f;
    [Tooltip("Distancia a la que oye correr al jugador.")]
    public float hearingRange = 8f;
    [Tooltip("Sospecha ganada por segundo viendo al jugador a media distancia.")]
    public float awarenessGain = 1.3f;

    [Header("Búsqueda y acecho")]
    public float searchTime = 6f;
    [Tooltip("Probabilidad de que el siguiente destino de patrulla sea cerca del jugador.")]
    [Range(0f, 1f)] public float huntChance = 0.45f;
    [Tooltip("Segundos después de perderlo en los que no puede volver a perseguirlo (solo investigar).")]
    public float chaseCooldown = 4f;
}
