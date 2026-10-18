using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Datos de la escena actual. Si nadie lo pone en la escena, lo crea LevelFlowManager
/// y busca objetos vacíos llamados "PlayerSpawn", "BossSpawn" y "PatrolPoints".
/// </summary>
public class LevelContext : MonoBehaviour
{
    [SerializeField] Transform playerSpawn;
    [SerializeField] Transform bossSpawn;
    [SerializeField] List<Transform> patrolPoints = new List<Transform>();

    public static LevelContext Current { get; private set; }

    public static bool BossEnabled => LevelFlowManager.CurrentLevel >= 1;
    public static float BossSpeedMultiplier => LevelFlowManager.CurrentLevel >= 2 ? 1.25f : 1f;

    public IReadOnlyList<Transform> PatrolPoints => patrolPoints;

    void Awake()
    {
        Current = this;
        if (playerSpawn == null) playerSpawn = FindByName("PlayerSpawn");
        if (bossSpawn == null) bossSpawn = FindByName("BossSpawn");

        if (patrolPoints.Count == 0)
        {
            Transform group = FindByName("PatrolPoints");
            if (group != null)
                foreach (Transform child in group) patrolPoints.Add(child);
        }
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    public void EnsurePlayer(GameObject playerPrefab)
    {
        var player = FindAnyObjectByType<PlayerController>();

        if (player == null && playerPrefab != null)
        {
            Vector3 position = playerSpawn != null ? playerSpawn.position : Vector3.up;
            float yaw = playerSpawn != null ? playerSpawn.eulerAngles.y : 0f;
            player = Instantiate(playerPrefab, position, Quaternion.Euler(0f, yaw, 0f)).GetComponent<PlayerController>();
            if (playerSpawn == null) Debug.LogWarning("Falta un objeto 'PlayerSpawn' en la escena; el jugador aparece en el origen.", this);
        }
        else if (player != null && playerSpawn != null)
        {
            player.Teleport(playerSpawn.position, playerSpawn.eulerAngles.y);
        }

        if (player != null) DisableOtherCameras(player.transform);
    }

    public void SetupBoss(GameObject bossPrefab)
    {
        var boss = FindAnyObjectByType<BossAI>();

        if (!BossEnabled)
        {
            if (boss != null) boss.gameObject.SetActive(false);
            return;
        }

        if (boss != null) return;

        // Enemigo viejo puesto a mano en la escena: se le pone el cerebro nuevo
        var legacy = FindAnyObjectByType<EnemyStateMachine>();
        if (legacy != null)
        {
            legacy.gameObject.AddComponent<BossAI>();
            return;
        }

        if (!TryGetBossSpawn(out Vector3 position, out Quaternion rotation)) return;
        if (bossPrefab != null) Instantiate(bossPrefab, position, rotation);
        else SpawnPlaceholderBoss(position, rotation);
    }

    bool TryGetBossSpawn(out Vector3 position, out Quaternion rotation)
    {
        rotation = Quaternion.identity;
        if (bossSpawn != null)
        {
            position = bossSpawn.position;
            rotation = bossSpawn.rotation;
            return true;
        }

        // Sin BossSpawn: un punto del NavMesh lejos del jugador
        Vector3 origin = playerSpawn != null ? playerSpawn.position : Vector3.zero;
        for (int i = 0; i < 20; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized * 18f;
            Vector3 candidate = origin + new Vector3(direction.x, 0f, direction.y);
            if (UnityEngine.AI.NavMesh.SamplePosition(candidate, out var hit, 6f, UnityEngine.AI.NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }
        }

        position = Vector3.zero;
        Debug.LogWarning("LevelContext: no hay BossSpawn ni NavMesh horneado; el jefe no aparece.");
        return false;
    }

    void SpawnPlaceholderBoss(Vector3 position, Quaternion rotation)
    {
        var boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        boss.name = "Boss (placeholder)";
        boss.transform.SetPositionAndRotation(position + Vector3.up * 1.2f, rotation);
        boss.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
        Destroy(boss.GetComponent<Collider>());
        boss.GetComponent<Renderer>().material.color = new Color(0.15f, 0.05f, 0.05f);
        boss.AddComponent<BossAI>().SetEyeHeight(0.9f);
        boss.GetComponent<UnityEngine.AI.NavMeshAgent>().baseOffset = 1.2f;
    }

    static void DisableOtherCameras(Transform player)
    {
        foreach (var cam in Camera.allCameras)
            if (!cam.transform.IsChildOf(player)) cam.gameObject.SetActive(false);
    }

    static Transform FindByName(string objectName)
    {
        var go = GameObject.Find(objectName);
        return go != null ? go.transform : null;
    }
}
