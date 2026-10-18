using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyStateMachine : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        ClimbHorizontal
    }

    [Header("Estado")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Velocidades")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 5f;
    public float climbSpeed = 2.5f;

    [Header("Patrulla (plano X-Z)")]
    public float patrolRadius = 6f;
    public float waitTimeAtTarget = 1.5f;
    public float arriveDistance = 0.4f;

    [Header("Persecución")]
    public float detectionRange = 7f;
    public float loseRange = 10f;
    [Tooltip("Distancia en el plano X-Z a la que se detiene junto al jugador")]
    public float stopDistance = 0.8f;

    [Header("Detección (se calcula desde el collider)")]
    public LayerMask groundLayer;
    [Tooltip("Paredes: bloquean de frente y son a las que se aferra por el costado")]
    public LayerMask wallLayer;
    [Tooltip("Qué tan adelante del borde del cuerpo se revisa el piso")]
    public float groundLookAhead = 0.6f;
    [Tooltip("Margen extra hacia abajo al revisar el piso")]
    public float groundCheckMargin = 0.5f;
    [Tooltip("Distancia de detección de paredes de frente")]
    public float wallCheckDistance = 0.5f;
    [Tooltip("Distancia, desde el borde del cuerpo, para detectar la pared del costado")]
    public float hangCheckDistance = 0.6f;

    [Header("Orientación")]
    [Tooltip("true: voltea la escala en X (sprites). false: rota hacia la dirección (modelos 3D)")]
    public bool flipSprite = true;
    public float rotationSpeed = 10f;

    [Header("Debug")]
    public bool debugLogs = false;

    private Rigidbody rb;
    private Collider col;
    private Transform player;

    // Movimiento (plano X-Z)
    private Vector3 moveDir = Vector3.right;
    private float currentSpeed = 0f;
    private Vector3 climbDir = Vector3.right;

    // Patrulla
    private Vector3 patrolCenter;
    private Vector3 targetPos;
    private bool isWaiting = false;
    private float waitTimer;
    private Vector3 blockedDir = Vector3.zero;

    // ------------------------------------------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Solo se bloquea la rotación: ahora puede moverse en X y Z
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (rb.isKinematic)
            Debug.LogWarning($"[{name}] El Rigidbody es Kinematic: la velocidad no tendrá efecto.", this);

        moveDir = transform.localScale.x >= 0f ? Vector3.right : Vector3.left;
        patrolCenter = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning($"[{name}] No se encontró ningún objeto con el tag 'Player'.", this);

        if (groundLayer.value == 0)
            Debug.LogError($"[{name}] groundLayer está en 'Nothing': el enemigo creerá que nunca hay piso y no avanzará.", this);
        if (wallLayer.value == 0)
            Debug.LogWarning($"[{name}] wallLayer está en 'Nothing': no detectará paredes.", this);

        PickNewTarget();
        ChangeState(EnemyState.Patrol);
    }

    // Decisiones en Update
    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: PatrolLogic(); break;
            case EnemyState.Chase: ChaseLogic(); break;
            case EnemyState.ClimbHorizontal: ClimbHorizontalLogic(); break;
        }
    }

    // Movimiento físico en FixedUpdate
    void FixedUpdate()
    {
        if (currentState == EnemyState.ClimbHorizontal)
        {
            Vector3 v = climbDir * climbSpeed;
            rb.linearVelocity = new Vector3(v.x, 0f, v.z);
        }
        else
        {
            Vector3 v = moveDir * currentSpeed;
            rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
        }
    }

    // ------------------------------------------------------------------
    #region Estados

    void PatrolLogic()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // Esperando
        if (isWaiting)
        {
            StopMoving();
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                PickNewTarget(blockedDir);
                blockedDir = Vector3.zero;
            }
            return;
        }

        Vector3 toTarget = Flat(targetPos - transform.position);

        // Llegó al objetivo
        if (toTarget.magnitude < arriveDistance)
        {
            StartWaiting(waitTimeAtTarget);
            return;
        }

        Vector3 dir = toTarget.normalized;

        if (!GroundAhead(dir))
        {
            if (CanHang(dir))
            {
                StartClimb(dir);
                return;
            }
            TurnBackAndWait(dir, 0.5f);
            return;
        }

        if (WallBlocking(dir))
        {
            TurnBackAndWait(dir, 0.5f);
            return;
        }

        SetMove(dir, moveSpeed);
    }

    void ChaseLogic()
    {
        if (player == null || Vector3.Distance(transform.position, player.position) > loseRange)
        {
            BackToPatrol();
            return;
        }

        Vector3 toPlayer = Flat(player.position - transform.position);

        if (toPlayer.magnitude < stopDistance)
        {
            StopMoving();
            return;
        }

        Vector3 dir = toPlayer.normalized;

        if (!GroundAhead(dir))
        {
            if (CanHang(dir))
            {
                StartClimb(dir);
                return;
            }
            StopMoving(); // no se tira al vacío
            UpdateFacing(dir);
            return;
        }

        if (WallBlocking(dir))
        {
            StopMoving();
            UpdateFacing(dir);
            return;
        }

        SetMove(dir, chaseSpeed);
    }

    void ClimbHorizontalLogic()
    {
        // Terminó el hueco: hay piso adelante Y debajo del cuerpo
        if (GroundAhead(climbDir) && GroundUnderBody())
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        // Se acabó la pared antes de cruzar
        if (!CanHang(climbDir))
        {
            if (GroundUnderBody())
            {
                // Todavía está sobre piso: se da la vuelta
                blockedDir = climbDir;
                PickNewTarget(blockedDir);
                blockedDir = Vector3.zero;
            }
            ChangeState(EnemyState.Patrol); // si no hay piso, simplemente cae
        }
    }

    #endregion

    // ------------------------------------------------------------------
    #region Detección (basada en el collider, en la dirección de movimiento)

    float BodyRadius()
    {
        Bounds b = col.bounds;
        return Mathf.Max(b.extents.x, b.extents.z);
    }

    bool GroundAhead(Vector3 dir)
    {
        Bounds b = col.bounds;
        Vector3 origin = b.center + dir * (BodyRadius() + groundLookAhead);
        return Physics.Raycast(origin, Vector3.down, b.extents.y + groundCheckMargin,
                               groundLayer, QueryTriggerInteraction.Ignore);
    }

    bool WallBlocking(Vector3 dir)
    {
        return Physics.Raycast(col.bounds.center, dir, BodyRadius() + wallCheckDistance,
                               wallLayer, QueryTriggerInteraction.Ignore);
    }

    // Pared "del costado" = a la izquierda o derecha respecto a la dirección de avance
    bool CanHang(Vector3 dir)
    {
        Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
        float dist = BodyRadius() + hangCheckDistance;
        Vector3 origin = col.bounds.center;
        return Physics.Raycast(origin, side, dist, wallLayer, QueryTriggerInteraction.Ignore)
            || Physics.Raycast(origin, -side, dist, wallLayer, QueryTriggerInteraction.Ignore);
    }

    bool GroundUnderBody()
    {
        Bounds b = col.bounds;
        return Physics.Raycast(b.center, Vector3.down, b.extents.y + 0.15f,
                               groundLayer, QueryTriggerInteraction.Ignore);
    }

    #endregion

    // ------------------------------------------------------------------
    #region Auxiliares

    static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    // Elige un punto aleatorio dentro de un círculo en el plano X-Z
    void PickNewTarget(Vector3 avoidDir = default)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 r = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = new Vector3(patrolCenter.x + r.x, transform.position.y, patrolCenter.z + r.y);
            Vector3 d = Flat(candidate - transform.position);

            if (d.magnitude < 1f) continue;
            if (avoidDir != Vector3.zero && Vector3.Dot(d.normalized, avoidDir) > 0.3f) continue;

            targetPos = candidate;
            return;
        }

        // Respaldo: alejarse de la dirección bloqueada
        Vector3 away = avoidDir != Vector3.zero ? -avoidDir : Vector3.right;
        targetPos = transform.position + away * 2f;
    }

    void SetMove(Vector3 dir, float speed)
    {
        moveDir = dir;
        currentSpeed = speed;
        UpdateFacing(dir);
    }

    void StopMoving()
    {
        currentSpeed = 0f;
    }

    void UpdateFacing(Vector3 dir)
    {
        if (flipSprite)
        {
            if (Mathf.Abs(dir.x) > 0.1f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (dir.x > 0f ? 1f : -1f);
                transform.localScale = scale;
            }
        }
        else if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
        }
    }

    void StartWaiting(float time)
    {
        isWaiting = true;
        waitTimer = time;
        StopMoving();
    }

    // Borde sin pared o pared bloqueando: esperar y elegir un destino del lado contrario
    void TurnBackAndWait(Vector3 blocked, float time)
    {
        if (debugLogs) Debug.Log($"[{name}] Obstáculo: elegirá otro destino.");
        blockedDir = blocked;
        StartWaiting(time);
    }

    void StartClimb(Vector3 dir)
    {
        climbDir = dir;
        UpdateFacing(dir);
        ChangeState(EnemyState.ClimbHorizontal);
    }

    void BackToPatrol()
    {
        patrolCenter = transform.position; // patrulla alrededor de donde quedó
        isWaiting = false;
        PickNewTarget();
        ChangeState(EnemyState.Patrol);
    }

    void ChangeState(EnemyState newState)
    {
        if (debugLogs && newState != currentState)
            Debug.Log($"[{name}] {currentState} -> {newState}");

        currentState = newState;
        currentSpeed = 0f;

        if (rb == null) return;

        if (newState == EnemyState.ClimbHorizontal)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.useGravity = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Collider c = col != null ? col : GetComponent<Collider>();
        if (c == null) return;

        Vector3 dir = Application.isPlaying ? moveDir : transform.right;
        Bounds b = c.bounds;
        float radius = Mathf.Max(b.extents.x, b.extents.z);

        // Piso adelante
        Gizmos.color = Color.green;
        Vector3 gOrigin = b.center + dir * (radius + groundLookAhead);
        Gizmos.DrawRay(gOrigin, Vector3.down * (b.extents.y + groundCheckMargin));

        // Pared de frente
        Gizmos.color = Color.red;
        Gizmos.DrawRay(b.center, dir * (radius + wallCheckDistance));

        // Paredes del costado
        Gizmos.color = Color.cyan;
        Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
        Gizmos.DrawRay(b.center, side * (radius + hangCheckDistance));
        Gizmos.DrawRay(b.center, -side * (radius + hangCheckDistance));

        // Destino de patrulla
        if (Application.isPlaying && currentState == EnemyState.Patrol)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.3f);
            Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
        }
    }

    #endregion
}