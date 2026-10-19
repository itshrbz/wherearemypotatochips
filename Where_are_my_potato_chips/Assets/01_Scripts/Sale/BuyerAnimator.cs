using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anima al comprador (parámetros Speed / WalkMult) y lo mantiene PEGADO AL PISO.
///
/// Por qué hace falta lo del piso: el prefab del comprador tiene el modelo desplazado dentro de la raíz
/// (Model en y = -1.2 con escala 0.57), así que la raíz "espera" estar ~0,68 m sobre el piso.
/// La tienda coloca la raíz en y = 1,0 (flota) y la réplica en zone.min.y + 1 (se hunde ~0,2 m).
/// En vez de depender de esos números, cada frame se mide el punto MÁS BAJO del modelo ya animado
/// y se sube/baja el modelo hasta que ese punto toque el piso real (raycast hacia abajo).
/// </summary>
public class BuyerAnimator : MonoBehaviour
{
    public Animator animator;
    [Tooltip("Metros por segundo que 'avanza' el clip de caminar sin ajustar")]
    public float clipSpeed = 1.4f;
    public float smoothing = 12f;

    [Header("Apoyo en el piso")]
    public bool alignToFloor = true;
    [Tooltip("Objeto que contiene el modelo. Si queda vacío se usa el que tiene el Animator.")]
    public Transform visual;
    [Tooltip("El rayo hacia abajo sale de la raíz + esta altura.")]
    public float floorRayStartHeight = 0.6f;
    public float floorRayLength = 6f;
    public LayerMask floorMask = ~0;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int MultHash = Animator.StringToHash("WalkMult");
    static readonly RaycastHit[] Hits = new RaycastHit[16];

    bool hasSpeed, hasMult;
    Vector3 last;
    float speed;

    Renderer[] renderers;
    Mesh bakeMesh;
    readonly List<Vector3> vertices = new List<Vector3>(2048);

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (visual == null && animator != null && animator.transform != transform) visual = animator.transform;
        if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);

        if (animator == null) return;
        foreach (var p in animator.parameters)
        {
            if (p.nameHash == SpeedHash) hasSpeed = true;
            if (p.nameHash == MultHash) hasMult = true;
        }
        if (!hasSpeed) Debug.LogWarning("BuyerAnimator: el controlador no tiene el parámetro float 'Speed'.", this);
    }

    void OnEnable()
    {
        last = transform.position;      // evita un salto de velocidad al aparecer en la entrada
        speed = 0f;
    }

    void OnDestroy()
    {
        if (bakeMesh != null) Destroy(bakeMesh);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || animator == null) return;

        Vector3 d = transform.position - last;
        d.y = 0f;
        last = transform.position;

        float v = d.magnitude / dt;
        if (v > 8f) v = 0f;                         // un teletransporte no es caminar
        speed = Mathf.Lerp(speed, v, 1f - Mathf.Exp(-smoothing * dt));

        if (hasSpeed) animator.SetFloat(SpeedHash, speed);
        if (hasMult) animator.SetFloat(MultHash, Mathf.Clamp(speed / Mathf.Max(0.1f, clipSpeed), 0.6f, 1.6f));
    }

    // Después de que el Animator ya aplicó la pose de este frame.
    void LateUpdate()
    {
        if (!alignToFloor || visual == null) return;
        if (!TryGetFloorY(out float floorY)) return;

        float lowest = GetLowestPointY();
        if (float.IsInfinity(lowest)) return;

        float delta = floorY - lowest;
        if (Mathf.Abs(delta) > 0.0005f) visual.position += Vector3.up * delta;
    }

    // Superficie más alta que haya debajo del comprador (ignora triggers, al propio comprador y al jugador).
    bool TryGetFloorY(out float y)
    {
        y = 0f;
        Vector3 origin = transform.position + Vector3.up * floorRayStartHeight;
        int count = Physics.RaycastNonAlloc(origin, Vector3.down, Hits, floorRayStartHeight + floorRayLength,
                                            floorMask, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue;
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            RaycastHit h = Hits[i];
            if (h.distance <= 0f) continue;
            if (h.transform.IsChildOf(transform)) continue;
            if (h.collider.GetComponentInParent<PlayerState>() != null) continue;
            if (h.distance < best)
            {
                best = h.distance;
                y = h.point.y;
                found = true;
            }
        }
        return found;
    }

    // Y mundial del vértice más bajo del modelo, con la animación de este frame.
    float GetLowestPointY()
    {
        if (renderers == null) renderers = GetComponentsInChildren<Renderer>(true);

        float lowest = float.PositiveInfinity;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;

            float low = r.bounds.min.y;
            if (r is SkinnedMeshRenderer smr && smr.sharedMesh != null)
            {
                if (bakeMesh == null) bakeMesh = new Mesh();
                smr.BakeMesh(bakeMesh);
                bakeMesh.GetVertices(vertices);
                if (vertices.Count > 0)
                {
                    Matrix4x4 m = smr.transform.localToWorldMatrix;
                    float baked = float.PositiveInfinity;
                    for (int k = 0; k < vertices.Count; k++)
                    {
                        Vector3 p = vertices[k];
                        float wy = m.m10 * p.x + m.m11 * p.y + m.m12 * p.z + m.m13;
                        if (wy < baked) baked = wy;
                    }
                    // Si algo salió raro con la malla horneada, se confía en los bounds del renderer.
                    if (Mathf.Abs(baked - low) < 0.5f) low = baked;
                }
            }
            if (low < lowest) lowest = low;
        }
        return lowest;
    }
}
