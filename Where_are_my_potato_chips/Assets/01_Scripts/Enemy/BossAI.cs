using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA del jefe sobre NavMesh.
/// - Sospecha: al ver al jugador se llena un medidor; a medias va a mirar, lleno lo persigue.
/// - Oído: correr cerca lo alerta y lo hace investigar el ruido.
/// - Persecución: corta camino prediciendo hacia dónde corre el jugador y acelera al estar cerca.
/// - Búsqueda: sigue la última dirección del jugador y revisa escondites cercanos.
///   Si vio al jugador esconderse, va a ese escondite y lo saca.
/// Escondido (agachado en un HidingSpot) o en la tienda réplica, el jugador no es visible.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    public enum State { Patrol, Investigate, Chase, Search }

    [Header("Velocidad")]
    [SerializeField] float patrolSpeed = 2.6f;
    [SerializeField] float chaseSpeed = 4.4f;
    [SerializeField] float turnSpeed = 540f;
    [Tooltip("Multiplicador de velocidad cuando ya está cerca y viendo al jugador.")]
    [SerializeField] float burstMultiplier = 1.2f;
    [SerializeField] float burstRange = 6f;

    [Header("Sentidos")]
    [SerializeField] float sightRange = 18f;
    [SerializeField, Range(10f, 180f)] float fieldOfView = 110f;
    [SerializeField] float eyeHeight = 1.6f;
    [SerializeField] float hearingRange = 9f;
    [SerializeField] LayerMask sightMask = ~0;

    [Header("Sospecha")]
    [Tooltip("Más cerca que esto, lo detecta al instante.")]
    [SerializeField] float instantSightRange = 5f;
    [Tooltip("Sospecha ganada por segundo viendo al jugador a media distancia.")]
    [SerializeField] float awarenessGain = 1.5f;
    [SerializeField] float awarenessDecay = 0.3f;
    [SerializeField, Range(0f, 1f)] float investigateThreshold = 0.3f;

    [Header("Persecución")]
    [SerializeField] float catchDistance = 1.2f;
    [Tooltip("Pausa después de saltar o trepar antes de poder atrapar: evita que llegue y mate de golpe.")]
    [SerializeField] float landRecovery = 0.6f;
    [SerializeField] float memoryTime = 3f;
    [Tooltip("Segundos hacia adelante que predice el movimiento del jugador para cortarle el paso.")]
    [SerializeField] float predictionTime = 0.8f;
    [SerializeField] float searchTime = 6f;
    [SerializeField] float searchRadius = 6f;
    [Tooltip("Escondites cercanos que revisa al buscar.")]
    [SerializeField] int hidingSpotsToCheck = 2;

    [Header("Patrulla")]
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float wanderRadius = 15f;
    [SerializeField] float waitAtPoint = 0.6f;
    [Tooltip("Probabilidad de quedarse quieto escuchando y mirando alrededor al llegar a un punto.")]
    [SerializeField, Range(0f, 1f)] float listenChance = 0.35f;
    [SerializeField] float listenTime = 2.5f;

    [Header("Acecho")]
    [Tooltip("Probabilidad de que el siguiente destino de patrulla sea cerca del jugador.")]
    [SerializeField, Range(0f, 1f)] float huntChance = 0.6f;
    [SerializeField] float huntRadius = 10f;

    public State Current { get; private set; } = State.Patrol;
    public bool IsChasing => Current == State.Chase;
    public bool IsTraversing => jumping || (traversal != null && traversal.IsTraversing);
    /// <summary>0 = no sospecha nada, 1 = lo tiene de objetivo.</summary>
    public float Awareness01 => IsChasing ? 1f : awareness;
    public Vector3 EyePosition => transform.position + Vector3.up * eyeHeight;
    public event Action PlayerCaught;

    NavMeshAgent agent;
    ProceduralTraversal traversal;
    PlayerState player;
    PlayerController playerController;
    CharacterController playerBody;
    HidingSpot[] hidingSpots;
    readonly RaycastHit[] hits = new RaycastHit[16];
    readonly Queue<Vector3> searchPoints = new Queue<Vector3>();

    Vector3 lastKnownPosition;
    Vector3 lastKnownVelocity;
    float lastSeenTime = -100f;
    float awareness;
    float stateTimer;
    float pauseUntil;
    float lookYaw;
    int patrolIndex;
    bool caught;
    bool jumping;
    bool wasTraversing;
    bool wasHidden;
    bool sawPlayerHide;
    Vector3 hideSpot;
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

        hidingSpots = FindObjectsByType<HidingSpot>(FindObjectsSortMode.None);
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
        if (recovering)
        {
            agent.isStopped = true;
            return;
        }

        Sense();

        bool paused = Time.time < pauseUntil;
        agent.isStopped = paused;
        if (paused)
        {
            LookAround();
            return;
        }

        float multiplier = LevelContext.BossSpeedMultiplier;
        switch (Current)
        {
            case State.Patrol: UpdatePatrol(multiplier); break;
            case State.Investigate: UpdateInvestigate(multiplier); break;
            case State.Chase: UpdateChase(multiplier); break;
            case State.Search: UpdateSearch(multiplier); break;
        }
    }

    void Sense()
    {
        bool lineOfSight = HasLineOfSight(out float distance);
        bool visible = lineOfSight && !player.IsHidden && !player.IsSafe;

        // Si lo estaba viendo justo cuando se escondió, sabe dónde está
        if (player.IsHidden && !wasHidden && lineOfSight && Time.time - lastSeenTime < 0.5f)
        {
            sawPlayerHide = true;
            hideSpot = player.transform.position;
        }
        wasHidden = player.IsHidden;

        if (visible)
        {
            Remember();
            // Recién perdido (buscando) o muy cerca: lo reconoce al instante
            if (Current == State.Chase || Current == State.Search || distance <= instantSightRange) awareness = 1f;
            else
            {
                float closeness = 1f - Mathf.Clamp01(distance / sightRange);
                float stealth = player.IsCrouching ? 0.5f : 1f;
                awareness += awarenessGain * (0.35f + closeness) * stealth * Time.deltaTime;
            }
        }
        else if (Current != State.Chase)
        {
            awareness = Mathf.Max(0f, awareness - awarenessDecay * Time.deltaTime);
        }

        if (CanHearPlayer(out float noiseDistance))
        {
            Remember();
            awareness = Mathf.Max(awareness, noiseDistance < hearingRange * 0.5f ? 1f : 0.6f);
        }

        if (awareness >= 1f && Current != State.Chase) EnterChase();
        else if (awareness >= investigateThreshold && Time.time - lastSeenTime < 0.1f
                 && (Current == State.Patrol || Current == State.Search))
            EnterInvestigate();
    }

    void Remember()
    {
        lastKnownPosition = player.transform.position;
        if (playerBody != null)
        {
            Vector3 velocity = playerBody.velocity;
            velocity.y = 0f;
            lastKnownVelocity = velocity;
        }
        lastSeenTime = Time.time;
    }

    void UpdatePatrol(float multiplier)
    {
        agent.speed = patrolSpeed * multiplier;
        if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            GoToNextPatrolPoint();
            return;
        }
        if (!Arrived()) return;

        GoToNextPatrolPoint();
        if (UnityEngine.Random.value < listenChance) Pause(listenTime);
    }

    // Camina despacio hacia lo que vio u oyó; si al llegar no hay nada, busca por la zona
    void UpdateInvestigate(float multiplier)
    {
        agent.speed = patrolSpeed * 1.15f * multiplier;
        agent.SetDestination(lastKnownPosition);
        if (!Arrived()) return;

        Pause(1.2f);
        EnterSearch();
    }

    void UpdateChase(float multiplier)
    {
        bool seesPlayer = Time.time - lastSeenTime < 0.2f;
        float distance = FlatDistance(transform.position, player.transform.position);

        float speed = chaseSpeed;
        if (seesPlayer && distance < burstRange) speed *= burstMultiplier;
        agent.speed = speed * multiplier;

        if (player.IsSafe || player.IsHidden || Time.time - lastSeenTime > memoryTime)
        {
            EnterSearch();
            return;
        }

        agent.SetDestination(seesPlayer ? PredictedPosition(distance) : lastKnownPosition);

        // Solo atrapa a la misma altura: si el jugador subió a un bloque, primero tiene que trepar
        float height = Mathf.Abs(player.transform.position.y - transform.position.y);
        if (distance <= catchDistance && height < 1.3f)
            StartCoroutine(Catch());
    }

    void UpdateSearch(float multiplier)
    {
        agent.speed = patrolSpeed * 1.3f * multiplier;

        if (sawPlayerHide && FlatDistance(transform.position, hideSpot) < 1.8f)
        {
            sawPlayerHide = false;
            if (player.IsHidden && FlatDistance(player.transform.position, hideSpot) < 2f)
            {
                StartCoroutine(Catch());
                return;
            }
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            EnterPatrol();
            return;
        }

        if (!Arrived()) return;

        if (searchPoints.Count > 0)
        {
            Pause(0.8f);
            agent.SetDestination(searchPoints.Dequeue());
        }
        else if (RandomPointNear(lastKnownPosition, searchRadius, out var point))
            agent.SetDestination(point);
    }

    void EnterPatrol()
    {
        Current = State.Patrol;
        sawPlayerHide = false;
        GoToNextPatrolPoint();
    }

    void EnterInvestigate()
    {
        Current = State.Investigate;
        Sfx.Play("boss_alert", transform.position);
        agent.SetDestination(lastKnownPosition);
    }

    void EnterChase()
    {
        if (Current != State.Investigate) Sfx.Play("boss_alert", transform.position);
        Current = State.Chase;
        awareness = 1f;
        pauseUntil = 0f;
    }

    // Orden de búsqueda: el escondite donde lo vio meterse, hacia donde corría, y luego los escondites cercanos
    void EnterSearch()
    {
        Current = State.Search;
        stateTimer = searchTime + (sawPlayerHide ? 3f : 0f);
        awareness = Mathf.Min(awareness, 0.6f);
        searchPoints.Clear();

        if (sawPlayerHide)
        {
            agent.SetDestination(hideSpot);
            return;
        }

        Vector3 ahead = lastKnownPosition + lastKnownVelocity * 1.5f;
        if (NavMesh.SamplePosition(ahead, out var hit, 3f, NavMesh.AllAreas)) searchPoints.Enqueue(hit.position);
        foreach (var spot in NearestHidingSpots(lastKnownPosition)) searchPoints.Enqueue(spot);

        agent.SetDestination(lastKnownPosition);
    }

    // El jefe no sabe dónde está el jugador, pero "intuye" la zona: así siempre anda cerca.
    void GoToNextPatrolPoint()
    {
        Pause(waitAtPoint);

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

    Vector3 PredictedPosition(float distance)
    {
        Vector3 target = player.transform.position;
        if (distance < 3f || playerBody == null) return target;

        Vector3 velocity = playerBody.velocity;
        velocity.y = 0f;
        Vector3 ahead = target + velocity * Mathf.Min(predictionTime, distance / chaseSpeed);
        return NavMesh.SamplePosition(ahead, out var hit, 2f, NavMesh.AllAreas) ? hit.position : target;
    }

    List<Vector3> NearestHidingSpots(Vector3 center)
    {
        var result = new List<Vector3>();
        if (hidingSpots == null || hidingSpotsToCheck <= 0) return result;

        var candidates = new List<HidingSpot>();
        float maxDistance = searchRadius * 2f;
        foreach (var spot in hidingSpots)
            if (spot != null && spot.isActiveAndEnabled && Vector3.Distance(spot.transform.position, center) < maxDistance)
                candidates.Add(spot);

        candidates.Sort((a, b) => Vector3.Distance(a.transform.position, center)
            .CompareTo(Vector3.Distance(b.transform.position, center)));

        for (int i = 0; i < candidates.Count && result.Count < hidingSpotsToCheck; i++)
            if (NavMesh.SamplePosition(candidates[i].transform.position, out var hit, 2f, NavMesh.AllAreas))
                result.Add(hit.position);
        return result;
    }

    void Pause(float seconds)
    {
        pauseUntil = Time.time + seconds;
        lookYaw = transform.eulerAngles.y + UnityEngine.Random.Range(-100f, 100f);
    }

    // Quieto, gira la cabeza (el cuerpo entero) de un lado a otro como escuchando
    void LookAround()
    {
        Quaternion target = Quaternion.Euler(0f, lookYaw, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * 0.25f * Time.deltaTime);
    }

    bool Arrived() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f;

    bool HasLineOfSight(out float distance)
    {
        Vector3 eye = EyePosition;
        Vector3 target = player.transform.position + Vector3.up * (player.IsCrouching ? 0.5f : 1.2f);
        Vector3 toTarget = target - eye;
        distance = toTarget.magnitude;

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

    bool CanHearPlayer(out float distance)
    {
        distance = Vector3.Distance(transform.position, player.transform.position);
        if (player.IsHidden || player.IsSafe || playerController == null) return false;
        return playerController.IsSprinting && distance <= hearingRange;
    }

    IEnumerator Catch()
    {
        caught = true;
        agent.isStopped = true;
        player.InputLocked = true;
        PlayerCaught?.Invoke();

        var screamer = player.GetComponent<CameraEffects>();
        if (screamer != null)
        {
            StartCoroutine(Lunge());
            yield return screamer.Screamer(this);
            LevelFlowManager.RestartLevel(false);
            yield break;
        }

        yield return FacePlayer(0.25f);
        yield return new WaitForSeconds(0.6f);
        LevelFlowManager.RestartLevel();
    }

    IEnumerator FacePlayer(float duration)
    {
        Vector3 look = player.transform.position - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude < 0.01f) yield break;

        Quaternion start = transform.rotation;
        Quaternion target = Quaternion.LookRotation(look);
        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            transform.rotation = Quaternion.Slerp(start, target, t);
            yield return null;
        }
        transform.rotation = target;
    }

    // Se abalanza hasta quedar pegado a la cara del jugador
    IEnumerator Lunge()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
        StartCoroutine(FacePlayer(0.12f));

        Vector3 start = transform.position;
        Vector3 toPlayer = player.transform.position - start;
        toPlayer.y = 0f;
        float reach = Mathf.Max(0f, toPlayer.magnitude - 0.7f);
        Vector3 end = start + toPlayer.normalized * reach;

        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.18f)
        {
            transform.position = Vector3.Lerp(start, end, t * t);
            yield return null;
        }
        transform.position = end;
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
        if (player == null) return false;

        playerController = player.GetComponent<PlayerController>();
        playerBody = player.GetComponent<CharacterController>();
        return true;
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
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, instantSightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Gizmos.DrawRay(eye, Quaternion.Euler(0f, fieldOfView * 0.5f, 0f) * transform.forward * sightRange);
        Gizmos.DrawRay(eye, Quaternion.Euler(0f, -fieldOfView * 0.5f, 0f) * transform.forward * sightRange);
    }
}
