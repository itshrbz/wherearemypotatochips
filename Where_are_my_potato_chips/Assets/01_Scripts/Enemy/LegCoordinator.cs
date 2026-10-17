using UnityEngine;

public class LegCoordinator : MonoBehaviour
{
    public Transform body;                       // raíz del jefe (si queda vacío usa transform.root)
    public ProceduralLeg[] groupA, groupB;       // 2 patas: A=izquierda, B=derecha. 4 patas: pares en diagonal
    Vector3 last, vel;

    void Start() { if (!body) body = transform.root; last = body.position; }

    void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 raw = Vector3.ClampMagnitude((body.position - last) / dt, 10f);  // evita picos al teletransportar
        vel = Vector3.Lerp(vel, raw, 10f * dt);
        last = body.position;

        if (AnyStepping(groupA) || AnyStepping(groupB)) return;  // nunca dos grupos a la vez
        float ea = Excess(groupA), eb = Excess(groupB);
        if (Mathf.Max(ea, eb) <= 0f) return;
        foreach (var l in (ea >= eb ? groupA : groupB))
            if (!l.freeHang) l.BeginStep(vel);
    }

    static bool AnyStepping(ProceduralLeg[] g) { foreach (var l in g) if (l.IsStepping) return true; return false; }
    static float Excess(ProceduralLeg[] g)
    {
        float m = -999f;
        foreach (var l in g) if (!l.freeHang) m = Mathf.Max(m, l.Error - l.stepDistance);
        return m;
    }
}