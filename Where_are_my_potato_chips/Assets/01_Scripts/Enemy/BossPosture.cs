using UnityEngine;
using UnityEngine.AI;

public class BossPosture : MonoBehaviour
{
    [Header("Referencias")]
    public Transform body;                  // raíz del jefe (si queda vacío, transform.root)
    public Transform[] spine;               // Spine, Spine1, Spine2 (de abajo hacia arriba)
    public Transform neck, head;            // mixamorig:Neck, mixamorig:Head
    public BossAI ai;
    public NavMeshAgent agent;

    [Header("Torso")]
    public float maxSpeed = 4.4f;           // velocidad de persecución
    public float leanMax = 14f;             // inclinación hacia adelante a máxima velocidad
    public float swayRoll = 4f;             // balanceo lateral
    public float swayTwist = 3f;            // giro del torso
    public Vector2 swayFreq = new Vector2(1.4f, 3.6f);

    [Header("Cabeza")]
    public float lookMaxAngle = 75f;
    public float lookSpeed = 6f;
    public float headTilt = 7f;             // cabeza ladeada al mirarte
    public float lookRange = 12f;           // solo si no hay BossAI

    [Tooltip("Márcalo si el torso no se mueve (algo lo pisa en Update).")]
    public bool useLateUpdate;

    Quaternion[] baseSpine;
    Quaternion baseNeck, baseHead, headCal;
    float phase, lean, lookW;
    Transform cam;

    void Start()
    {
        if (body == null) body = transform.root;
        if (ai == null) ai = GetComponentInParent<BossAI>();
        if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
        baseSpine = new Quaternion[spine.Length];
        for (int i = 0; i < spine.Length; i++) baseSpine[i] = spine[i].localRotation;
        if (neck != null) baseNeck = neck.localRotation;
        if (head != null)
        {
            baseHead = head.localRotation;
            // calibración: en reposo el modelo mira a body.forward y está erguido
            headCal = Quaternion.LookRotation(head.InverseTransformDirection(body.forward),
                                              head.InverseTransformDirection(Vector3.up));
        }
    }

    void Update() { if (!useLateUpdate) Apply(); }
    void LateUpdate() { if (useLateUpdate) Apply(); }

    void Apply()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        float speed = agent != null && agent.enabled ? agent.velocity.magnitude : 0f;
        float s01 = Mathf.Clamp01(speed / Mathf.Max(0.1f, maxSpeed));
        phase += dt * Mathf.Lerp(swayFreq.x, swayFreq.y, s01);
        lean = Mathf.Lerp(lean, leanMax * s01, 1f - Mathf.Exp(-5f * dt));
        float amp = Mathf.Lerp(0.35f, 1f, s01);                      // parado casi no oscila

        float roll = Mathf.Sin(phase) * swayRoll * amp;
        float twist = Mathf.Sin(phase * 0.5f) * swayTwist * amp;
        Quaternion delta = Quaternion.AngleAxis(lean, body.right)
                         * Quaternion.AngleAxis(roll, body.forward)
                         * Quaternion.AngleAxis(twist, body.up);

        // el movimiento se reparte entre los huesos de la columna
        for (int i = 0; i < spine.Length; i++)
        {
            if (spine[i] == null) continue;
            Quaternion baseWorld = spine[i].parent.rotation * baseSpine[i];
            spine[i].rotation = Quaternion.Slerp(Quaternion.identity, delta, 1f / spine.Length) * baseWorld;
        }

        // cabeza: mira al jugador mientras lo persigue
        float want = 0f;
        if (ai != null) want = ai.IsChasing ? 1f : ai.Current == BossAI.State.Search ? 0.4f : 0f;
        else if (cam != null && Vector3.Distance(cam.position, body.position) < lookRange) want = 1f;
        lookW = Mathf.Lerp(lookW, want, 1f - Mathf.Exp(-lookSpeed * dt));

        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (head == null || cam == null) return;

        Vector3 to = cam.position - head.position;
        Vector3 dir = Vector3.RotateTowards(body.forward, to.normalized, lookMaxAngle * Mathf.Deg2Rad, 0f);
        Quaternion look = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Inverse(headCal);
        look = Quaternion.AngleAxis(Mathf.Sin(Time.time * 0.8f) * headTilt * lookW, dir) * look;

        if (neck != null)
        {
            Quaternion nb = neck.parent.rotation * baseNeck;
            neck.rotation = Quaternion.Slerp(nb, look, 0.4f * lookW);
        }
        Quaternion hb = head.parent.rotation * baseHead;
        head.rotation = Quaternion.Slerp(hb, look, lookW);
    }
}