using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Datos de la escena actual. Si nadie lo pone en la escena, lo crea LevelFlowManager
/// y busca objetos vacíos llamados "PlayerSpawn", "BossSpawn" y "PatrolPoints".
/// Si falta algo, intenta resolverlo con el NavMesh para que el nivel se pueda jugar igual.
/// </summary>
public class LevelContext : MonoBehaviour
{
    const float PlayerHalfHeight = 1.05f;

    [SerializeField] Transform playerSpawn;
    [SerializeField] Transform bossSpawn;
    [SerializeField] List<Transform> patrolPoints = new List<Transform>();

    public static LevelContext Current { get; private set; }

    public static bool BossEnabled => LevelFlowManager.CurrentLevel >= 1;
    public static float BossSpeedMultiplier => LevelFlowManager.CurrentLevel >= 2 ? 1.25f : 1f;

    public IReadOnlyList<Transform> PatrolPoints => patrolPoints;

    Vector3? replicaPosition;

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

    public void EnsureReplica(GameObject replicaPrefab)
    {
        var checkout = FindAnyObjectByType<ReplicaCheckout>();
        if (checkout != null)
        {
            replicaPosition = checkout.transform.position;
            return;
        }

        // Una copia desempaquetada del prefab: solo le falta el componente
        Transform copy = FindByName("ReplicaStore");
        if (copy != null)
        {
            copy.gameObject.AddComponent<ReplicaCheckout>();
            replicaPosition = copy.position;
            return;
        }

        Transform anchor = FindByName("ReplicaStore_Anchor");
        if (anchor != null && replicaPrefab != null)
        {
            Instantiate(replicaPrefab, anchor.position, anchor.rotation);
            replicaPosition = anchor.position;
            return;
        }

        Debug.LogWarning("LevelContext: este nivel no tiene ReplicaStore ni ReplicaStore_Anchor; usa F2 para avanzar.", this);
    }

    public void EnsurePlayer(GameObject playerPrefab)
    {
        Vector3 position;
        float yaw = 0f;

        if (playerSpawn != null)
        {
            position = OnGround(playerSpawn.position);
            yaw = playerSpawn.eulerAngles.y;
        }
        else if (TryFarthestNavMeshPoint(replicaPosition, out Vector3 point))
        {
            position = point + Vector3.up * PlayerHalfHeight;
            Debug.LogWarning("LevelContext: falta 'PlayerSpawn'; el jugador aparece en el punto del NavMesh más lejano a la réplica.", this);
        }
        else
        {
            position = OnGround(Vector3.zero);
            Debug.LogWarning("LevelContext: falta 'PlayerSpawn' y no hay NavMesh; el jugador aparece en el origen.", this);
        }

        var player = FindAnyObjectByType<PlayerController>();
        if (player == null && playerPrefab != null)
            player = Instantiate(playerPrefab, position, Quaternion.Euler(0f, yaw, 0f)).GetComponent<PlayerController>();
        else if (player != null)
            player.Teleport(position, yaw);

        if (player != null) DisableOtherCameras(player.transform);
    }

    public void SetupBoss(GameObject bossPrefab)
    {
        var boss = FindAnyObjectByType<BossAI>();

        if (!BossEnabled)
        {
            if (boss != null) boss.gameObject.SetActive(false);
            var oldEnemy = FindAnyObjectByType<EnemyStateMachine>();
            if (oldEnemy != null) oldEnemy.gameObject.SetActive(false);
            return;
        }

        if (boss != null) return;

        if (!TryGetBossSpawn(out Vector3 position, out Quaternion rotation)) return;

        // Enemigo viejo puesto a mano en la escena: se le pone el cerebro nuevo
        var legacy = FindAnyObjectByType<EnemyStateMachine>();
        if (legacy != null)
        {
            if (NavMesh.SamplePosition(legacy.transform.position, out var hit, 10f, NavMesh.AllAreas))
                position = hit.position;
            legacy.transform.position = position;
            legacy.gameObject.AddComponent<BossAI>();
            return;
        }

        if (bossPrefab != null) Instantiate(bossPrefab, position, rotation);
        else SpawnPlaceholderBoss(position, rotation);
    }

    bool TryGetBossSpawn(out Vector3 position, out Quaternion rotation)
    {
        rotation = Quaternion.identity;
        position = Vector3.zero;

        if (bossSpawn != null)
        {
            rotation = bossSpawn.rotation;
            if (NavMesh.SamplePosition(bossSpawn.position, out var hit, 10f, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }
            Debug.LogWarning("LevelContext: 'BossSpawn' está lejos del NavMesh; busco otro punto.", bossSpawn);
        }

        Vector3 from = playerSpawn != null ? playerSpawn.position : Vector3.zero;
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null) from = player.transform.position;

        if (TryFarthestNavMeshPoint(from, out position)) return true;

        Debug.LogWarning("LevelContext: el nivel no tiene NavMesh horneado; el jefe no aparece.", this);
        return false;
    }

    static bool TryFarthestNavMeshPoint(Vector3? from, out Vector3 point)
    {
        point = Vector3.zero;
        var mesh = NavMesh.CalculateTriangulation();
        if (mesh.vertices == null || mesh.vertices.Length == 0) return false;

        Vector3 origin = from ?? mesh.vertices[0];
        float best = -1f;
        foreach (var vertex in mesh.vertices)
        {
            float distance = (vertex - origin).sqrMagnitude;
            if (distance > best)
            {
                best = distance;
                point = vertex;
            }
        }

        // El vértice puede quedar en un borde; se acerca un poco al centro del NavMesh
        if (NavMesh.SamplePosition(Vector3.MoveTowards(point, origin, 1f), out var hit, 2f, NavMesh.AllAreas))
            point = hit.position;
        return true;
    }

    static Vector3 OnGround(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out var hit, 10f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point + Vector3.up * PlayerHalfHeight;
        return position + Vector3.up * PlayerHalfHeight;
    }

    static void SpawnPlaceholderBoss(Vector3 position, Quaternion rotation)
    {
        var boss = new GameObject("Boss (placeholder)");
        boss.SetActive(false);
        boss.transform.SetPositionAndRotation(position, rotation);

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Destroy(body.GetComponent<Collider>());
        body.transform.SetParent(boss.transform, false);
        body.transform.localPosition = Vector3.up * 1.2f;
        body.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
        body.GetComponent<Renderer>().material.color = new Color(0.15f, 0.05f, 0.05f);

        boss.AddComponent<BossAI>();
        BossAI.UseSceneAgentType(boss.GetComponent<NavMeshAgent>());
        boss.SetActive(true);
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
