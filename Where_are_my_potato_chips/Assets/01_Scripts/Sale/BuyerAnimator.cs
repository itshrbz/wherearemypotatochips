using UnityEngine;

public class BuyerAnimator : MonoBehaviour
{
    public Animator animator;
    [Tooltip("Metros por segundo que 'avanza' el clip de caminar sin ajustar")]
    public float clipSpeed = 1.4f;
    public float smoothing = 12f;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int MultHash = Animator.StringToHash("WalkMult");

    bool hasSpeed, hasMult;
    Vector3 last;
    float speed;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
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
}