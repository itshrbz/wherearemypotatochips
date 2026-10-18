using System;
using UnityEngine;

public class ProceduralLeg : MonoBehaviour
{
    [Header("Referencias")]
    public Transform ikTarget;      // Target del TwoBoneIK de esta pata
    public Transform homePoint;     // Hijo del cuerpo: donde el pie "quiere" estar
    [Header("Paso")]
    public float stepDistance = 0.9f;
    public float stepDuration = 0.25f;
    public float stepHeight = 0.4f;
    public float hangDrop = 0.3f;   // cuánto cuelga la pata en el aire
    public float overshoot = 0.45f;   // cuánto se adelanta el pie al dar el paso
    public LayerMask groundMask;    // pon aquí la capa Ground

    [HideInInspector] public bool freeHang;   // la pata cuelga (saltos/escalada)
    public bool IsStepping { get; private set; }
    public event Action<Vector3> Planted;     // para sonido de pasos

    Vector3 planted, from, to;
    float t, curDur;

    // Distancia horizontal entre el pie plantado y su punto natural
    public float Error
    {
        get { Vector3 d = homePoint.position - planted; d.y = 0; return d.magnitude; }
    }

    void Start()
    {
        planted = GroundPoint(homePoint.position);
        ikTarget.position = planted;
    }

    public void BeginStep(Vector3 bodyVelocity)
    {
        Vector3 h = new Vector3(bodyVelocity.x, 0, bodyVelocity.z);
        curDur = Mathf.Lerp(stepDuration, stepDuration * 0.5f, Mathf.InverseLerp(0, 8, h.magnitude));
        Vector3 ahead = h.sqrMagnitude > 0.01f ? h.normalized * overshoot : Vector3.zero;
        from = planted;
        to = GroundPoint(homePoint.position + ahead + h * curDur * 0.5f);   // cae por delante del Home
        t = 0; IsStepping = true;
    }

    void Update()
    {
        if (freeHang)
        {
            IsStepping = false;
            planted = Vector3.Lerp(planted, homePoint.position + Vector3.down * hangDrop, 8f * Time.deltaTime);
            ikTarget.position = planted;
            return;
        }
        if (IsStepping)
        {
            t += Time.deltaTime / curDur;
            float k = Mathf.Clamp01(t);
            Vector3 p = Vector3.Lerp(from, to, k * k * (3 - 2 * k));
            p.y += Mathf.Sin(k * Mathf.PI) * stepHeight;
            ikTarget.position = p;
            if (t >= 1f)
            {
                IsStepping = false; planted = to; ikTarget.position = planted;
                Planted?.Invoke(planted);
            }
        }
        else ikTarget.position = planted;   // pie fijo en el mundo mientras el cuerpo avanza
    }

    Vector3 GroundPoint(Vector3 p)
    {
        if (Physics.Raycast(p + Vector3.up * 1.5f, Vector3.down, out RaycastHit hit, 4f,
                            groundMask, QueryTriggerInteraction.Ignore))
            return hit.point;
        return p;
    }
}