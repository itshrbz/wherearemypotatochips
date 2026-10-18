using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA del jefe sobre NavMesh: patrulla, persigue si ve u oye al jugador y busca donde lo perdió.
/// Si el jugador está escondido (agachado en un HidingSpot) o en la tienda réplica, no lo detecta.
/// Los NavMeshLink los recorre ProceduralTraversal cuando está presente.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search }

    [Header("Velocidad")]
    [SerializeField] float patrolSpeed = 2.6f;
    [SerializeField] float chaseSpeed = 4.4f;
    [SerializeField] float turnSpeed = 540f;

    [Header("Sentidos")]
    [SerializeField] float sightRange = 18f;
    [SerializeField, Range(10f, 180f)] float fieldOfView = 110f;
    [SerializeField] float eyeHeight = 1.6f;
    [SerializeField] float hearingRange = 9f;
    [SerializeField] LayerMask sightMask = ~0;

    [Header("Persecución")]
    [SerializeField] float catchDistance = 1.2f;
    [Tooltip("Pausa después de saltar o trepar antes de poder atrapar: evita que llegue y mate de golpe.")]
    [SerializeField] float landRecovery = 0.6f;
    [SerializeField] float memoryTime = 3f;
    [SerializeField] float searchTime = 6f;
    [SerializeField] float searchRadius = 6f;

    [Header("Patrulla")]
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float wanderRadius = 15f;
    [SerializeField] float waitAtPoint = 0.6f;

    [Header("Acecho")]
    [Tooltip("Probabilidad de que el siguiente destino de patrulla sea cerca del jugador.")]
    [SerializeField, Range(0f, 1f)] float huntChance = 0.6f;
    [SerializeField] float huntRadius = 10f;

    public State Current { get; private set; } = State.Patrol;
    public bool IsChasing => Current == State.Chase;
    public bool IsTraversing => jumping || (traversal != null && traversal.IsTraversing);
    public event Action PlayerCaught;

    NavMeshAgent agent;
    ProceduralTraversal traversal;
    PlayerState player;
    PlayerController playerController;
    readonly RaycastHit[] hits = new RaycastHit[16];

    Vector3 lastKnownPosition;
    float lastSeenTime = -100f;
    float stateTimer;
    int patrolIndex;
    bool caught;
    bool jumping;
    bool wasTraversing;
    float recoverUntil;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        traversal = GetComponent<ProceduralTraversal>();
        UseSceneAgentType(agent);

        agent.updateRotation = true;
        agent.angularSpeed = turnSpeed;
        agent.autoBraking = true;
        agent.autoTraverseOffMeshLink = false;

        // El prototipo anterior movía al jefe con física; con NavMesh eso lo hace patinar de lado.
        var oldBrain = GetComponent<EnemyStateMachine>();
        if (oldBrain != null) oldBrain.enabled = false;
        var body = GetComponent<Rigidbody>();
        if (body != null) body.isKinematic = true;
    }

    void Start()
    {
        if (!agent.isOnNavMesh && NavMesh.SamplePosition(transform.position, out var hit, 10f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("BossAI: el jefe no está sobre un NavMesh; queda desactivado en este nivel.", this);
            enabled = false;
            return;
        }

        if ((patrolPoints == null || patrolPoints.Length == 0) && LevelContext.Current != null)
        {
            var points = LevelContext.Current.PatrolPoints;
            if (points != null && points.Count > 0)
            {
                patrolPoints = new Transform[points.Count];
                for (int i = 0; i < points.Count; i++) patrolPoints[i] = points[i];
            }
        }

        EnterPatrol();
    }

    void Update()
    {
        if (caught || !agent.isOnNavMesh) return;
        if (!FindPlayer()) return;

        if (agent.isOnOffMeshLink && !IsTraversing)
        {
            if (traversal != null) traversal.Begin(agent);
            else StartCoroutine(JumpLink());
        }

        if (IsTraversing)
        {
            wasTraversing = true;
            return;
        }
        if (wasTraversing)
        {
            wasTraversing = false;
            recoverUntil = Time.time + landRecovery;
        }

        bool recovering = Time.time < recoverUntil;
        agent.isStopped = recovering;
        if (recovering) return;

        float multiplier = LevelContext.BossSpeedMultiplier;
        bool detected = CanSeePlayer() || CanHearPlayer();
        if (detected)
        {
            lastKnownPosition = player.transform.position;
            lastSeenTime = Time.time;
            if (Current != State.Chase) Current = State.Chase;
        }

        switch (Current)
        {
            case State.Patrol: UpdatePatrol(multiplier); break;
            case State.Chase: UpdateChase(multiplier); break;
            case State.Search: UpdateSearch(multiplier); break;
        }
    }

    void UpdatePatrol(float multiplier)
    {
        agent.speed = patrolSpeed * multiplier;
        if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            GoToNextPatrolPoint();
            return;
        }
        if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.2f) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) GoToNextPatrolPoint();
    }

    void UpdateChase(float multiplier)
    {
        agent.speed = chaseSpeed * multiplier;

        if (player.IsSafe || player.IsHidden || Time.time - lastSeenTime > memoryTime)
        {
            EnterSearch();
            return;
        }

        agent.SetDestination(lastKnownPosition);

        // Solo atrapa a la misma altura: si el jugador subió a un bloque, primero tiene que trepar
        float height = Mathf.Abs(player.transform.position.y - transform.position.y);
        if (FlatDistance(transform.position, player.transform.position) <= catchDistance && height < 1.3f)
            StartCoroutine(Catch());
    }

    void UpdateSearch(float multiplier)
    {
        agent.speed = patrolSpeed * 1.3f * multiplier;
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            EnterPatrol();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f
            && RandomPointNear(lastKnownPosition, searchRadius, out var point))
            agent.SetDestination(point);
    }

    void EnterPatrol()
    {
        Current = State.Patrol;
        stateTimer = 0f;
        GoToNextPatrolPoint();
    }

    void EnterSearch()
    {
        Current = State.Search;
        stateTimer = searchTime;
        agent.SetDestination(lastKnownPosition);
    }

    // El jefe no sabe dónde está el jugador, pero "intuye" la zona: así siempre anda cerca.
    void GoToNextPatrolPoint()
    {
        stateTimer = waitAtPoint;

        if (player != null && UnityEngine.Random.value < huntChance
            && RandomPointNear(player.transform.position, huntRadius, out var near))
        {
            agent.SetDestination(near);
            return;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Transform target = patrolPoints[patrolIndex % patrolPoints.Length];
            patrolIndex++;
            if (target != null) agent.SetDestination(target.position);
            return;
        }

        if (RandomPointNear(transform.position, wanderRadius, out var point))
            agent.SetDestination(point);
    }

    bool CanSeePlayer()
    {
        if (player.IsHidden || player.IsSafe) return false;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.transform.position + Vector3.up * (player.IsCrouching ? 0.5f : 1.2f);
        Vector3 toTarget = target - eye;
        float distance = toTarget.magnitude;

        if (distance < 0.05f) return true;
        if (distance > sightRange) return false;
        if (Vector3.Angle(transform.forward, toTarget) > fieldOfView * 0.5f && distance > catchDistance * 2f) return false;

        int count = Physics.RaycastNonAlloc(eye, toTarget / distance, hits, distance, sightMask, QueryTriggerInteraction.Ignore);
        float nearest = float.MaxValue;
        Transform blocker = null;
        for (int i = 0; i < count; i++)
        {
            Transform hitTransform = hits[i].transform;
            if (hitTransform.IsChildOf(transform)) continue;
            if (hits[i].distance < nearest)
            {
                nearest = hits[i].distance;
                blocker = hitTransform;
            }
        }
        return blocker == null || blocker.IsChildOf(player.transform);
    }

    bool CanHearPlayer()
    {
        if (player.IsHidden || player.IsSafe || playerController == null) return false;
        if (!playerController.IsSprinting) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= hearingRange;
    }

    IEnumerator Catch()
    {
        caught = true;
        agent.isStopped = true;
        player.InputLocked = true;
        PlayerCaught?.Invoke();

        Vector3 look = player.transform.position - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(look);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.6f);
        LevelFlowManager.RestartLevel();
    }

    // Salto simple por un NavMeshLink cuando el jefe no tiene ProceduralTraversal (por ejemplo, el placeholder)
    IEnumerator JumpLink()
    {
        jumping = true;
        agent.updatePosition = false;

        OffMeshLinkData link = agent.currentOffMeshLinkData;
        Vector3 offset = Vector3.up * agent.baseOffset;
        Vector3 start = transform.position;
        Vector3 end = link.endPos + offset;

        Vector3 flat = end - start;
        flat.y = 0f;
        if (flat.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(flat);

        float arc = Mathf.Max(0.6f, end.y - start.y + 0.5f);
        float duration = Mathf.Clamp(Vector3.Distance(start, end) / 3f, 0.5f, 1.3f);
        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            Vector3 position = Vector3.Lerp(start, end, t);
            position.y += Mathf.Sin(t * Mathf.PI) * arc;
            transform.position = position;
            yield return null;
        }

        agent.CompleteOffMeshLink();
        agent.Warp(end - offset);
        agent.updatePosition = true;
        jumping = false;
    }

    bool FindPlayer()
    {
        if (player != null) return true;
        player = FindAnyObjectByType<PlayerState>();
        if (player != null) playerController = player.GetComponent<PlayerController>();
        return player != null;
    }

    /// <summary>Cada nivel puede hornear su NavMesh con otro tipo de agente; el jefe usa el de la escena.</summary>
    public static void UseSceneAgentType(NavMeshAgent navAgent)
    {
        var surface = FindAnyObjectByType<NavMeshSurface>();
        if (surface == null || surface.agentTypeID == navAgent.agentTypeID) return;

        navAgent.agentTypeID = surface.agentTypeID;
        if (navAgent.isActiveAndEnabled)
        {
            navAgent.enabled = false;
            navAgent.enabled = true;
        }
    }

    static bool RandomPointNear(Vector3 center, float radius, out Vector3 point)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;
            if (NavMesh.SamplePosition(center + new Vector3(offset.x, 0f, offset.y), out var hit, 2f, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }
        point = center;
        return false;
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Gizmos.DrawRay(eye, Quaternion.Euler(0f, fieldOfView * 0.5f, 0f) * transform.forward * sightRange);
        Gizmos.DrawRay(eye, Quaternion.Euler(0f, -fieldOfView * 0.5f, 0f) * transform.forward * sightRange);
    }
}
