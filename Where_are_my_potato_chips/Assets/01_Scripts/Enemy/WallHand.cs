using System;
using UnityEngine;

public class WallHand : MonoBehaviour
{
    [Header("Referencias")]
    public Transform ikTarget;      // Target del TwoBoneIK del brazo
    public Transform shoulder;
    public Transform body;          // raíz del jefe (usa su right y forward)
    public WallHand other;          // la otra mano (se turnan)
    public float side = 1f;         // -1 izquierda, +1 derecha

    [Header("Palma sobre la pared")]
    public Transform handBone;      // mixamorig:LeftHand / RightHand
    public Transform fingerBone;    // mixamorig:LeftHandMiddle1 / RightHandMiddle1
    public float palmOffset = 0.05f;
    public float wristBack = 0.08f;
    public bool flipPalm;
    public float rotateSpeed = 14f;

    [Header("Alcance y pasos")]
    public float reach = 1.2f, lead = 0.5f, replantDistance = 0.8f;
    public float stepDuration = 0.2f, stepHeight = 0.15f;
    public LayerMask wallMask = 1;  // Default (paredes). Excluye Player y Enemy

    [Header("Reposo (sin pared)")]
    public ProceduralLeg oppositeLeg;   // pierna del lado CONTRARIO: sincroniza el balanceo
    public float hangDrop = 0.8f;       // distancia hombro-muñeca hacia abajo (~85-90% del brazo)
    public float armOut = 0.12f;        // separación lateral del cuerpo
    public float restForward = 0.05f;   // el brazo cuelga un poco hacia adelante
    public float swingAmp = 0.35f;      // balanceo adelante/atrás al caminar (metros)
    public float swingLift = 0.05f;     // la mano sube un poco al adelantarse
    public float swingBias = 0.2f;      // centra el balanceo (las piernas pisan más atrás)
    public float fullSwingSpeed = 2.5f; // velocidad a la que el balanceo es completo
    public float idleSway = 0.02f;      // vaivén mínimo parado
    public float restFingerForward = 0.15f;   // inclinación de los dedos hacia adelante
    public float followSpeed = 12f;     // qué tan rápido sigue la mano a su posición de reposo

    [HideInInspector] public bool overrideControl;   // lo usa ProceduralTraversal
    public bool IsPlanted { get; private set; }
    public bool IsStepping { get; private set; }
    public event Action<Vector3> Planted;

    Vector3 planted, from, to, lastBodyPos;
    Quaternion plantedRot, toRot, curRot;
    Vector3 palmLocal, fingerLocal;
    bool calibrated;
    float t, speed;

    void Start()
    {
        planted = ikTarget.position;
        curRot = plantedRot = ikTarget.rotation;
        lastBodyPos = body.position;

        // Calibración: en T-pose las palmas miran hacia abajo
        if (handBone != null)
        {
            palmLocal = handBone.InverseTransformDirection(flipPalm ? Vector3.up : Vector3.down).normalized;
            fingerLocal = fingerBone != null
                ? handBone.InverseTransformDirection(fingerBone.position - handBone.position)
                : handBone.InverseTransformDirection(body.right * side);
            fingerLocal = Vector3.ProjectOnPlane(fingerLocal, palmLocal).normalized;
            calibrated = fingerLocal.sqrMagnitude > 0.5f && palmLocal.sqrMagnitude > 0.5f;
        }
        if (!calibrated)
            Debug.LogWarning("WallHand: asigna Hand Bone para que la mano gire correctamente.", this);
    }

    // Úsalo cuando otro script suelta el control de la mano
    public void SyncFromTarget()
    {
        planted = ikTarget.position;
        curRot = plantedRot = ikTarget.rotation;
        IsStepping = false;
        IsPlanted = false;
    }

    void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 vel = (body.position - lastBodyPos) / dt;
        lastBodyPos = body.position;
        vel.y = 0f;
        speed = Mathf.Lerp(speed, Mathf.Min(vel.magnitude, 10f), 1f - Mathf.Exp(-6f * dt));

        if (overrideControl) { IsPlanted = true; return; }

        Vector3 dir = body.right * side;
        Vector3 fwd = Vector3.ProjectOnPlane(body.forward, Vector3.up);
        fwd = fwd.sqrMagnitude > 0.001f ? fwd.normalized : Vector3.forward;
        bool wall = Physics.Raycast(shoulder.position, dir, out RaycastHit hit, reach,
                                    wallMask, QueryTriggerInteraction.Ignore);

        if (IsStepping)
        {
            t += dt / stepDuration;
            float k = Mathf.Clamp01(t);
            Vector3 p = Vector3.Lerp(from, to, k);
            p.y += Mathf.Sin(k * Mathf.PI) * stepHeight;
            ikTarget.position = p;
            SetRot(toRot, dt);
            if (t >= 1f)
            {
                IsStepping = false; IsPlanted = true; planted = to; plantedRot = toRot;
                Planted?.Invoke(planted);
            }
            return;
        }

        float swing = SwingValue(fwd);

        if (wall)
        {
            Pose(hit, fwd, out Vector3 wantPos, out Quaternion wantRot);
            bool need = !IsPlanted || Vector3.Distance(planted, wantPos) > replantDistance;
            if (need && (other == null || !other.IsStepping))
            {
                from = ikTarget.position; to = wantPos; toRot = wantRot;
                t = 0; IsStepping = true; IsPlanted = false;
            }
            else
            {
                ikTarget.position = planted;
                SetRot(IsPlanted ? plantedRot : RestRot(dir, fwd, swing), dt);
            }
        }
        else
        {
            IsPlanted = false;
            Vector3 flatDir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
            float breathe = Mathf.Sin(Time.time * 1.3f + (side > 0 ? 0f : 1.7f)) * idleSway;
            Vector3 hang = shoulder.position
                         + Vector3.down * hangDrop
                         + flatDir * armOut
                         + fwd * (swing * swingAmp + restForward + breathe)
                         + Vector3.up * (Mathf.Max(0f, swing) * swingLift);
            planted = Vector3.Lerp(planted, hang, 1f - Mathf.Exp(-followSpeed * dt));
            ikTarget.position = planted;
            SetRot(RestRot(dir, fwd, swing), dt);
        }
    }

    // -1..1: el brazo se adelanta cuando la pierna contraria se adelanta
    float SwingValue(Vector3 fwd)
    {
        if (oppositeLeg == null || oppositeLeg.ikTarget == null || oppositeLeg.homePoint == null) return 0f;
        Vector3 off = oppositeLeg.ikTarget.position - oppositeLeg.homePoint.position;
        float f = Vector3.Dot(off, fwd) / Mathf.Max(0.1f, oppositeLeg.stepDistance);
        float s = Mathf.Clamp(f + swingBias, -1f, 1f);
        return s * Mathf.Clamp01(speed / fullSwingSpeed);     // parado: sin balanceo
    }

    // Posición de la muñeca y rotación para que la palma quede plana en la pared
    void Pose(RaycastHit hit, Vector3 fwd, out Vector3 pos, out Quaternion rot)
    {
        Vector3 n = hit.normal;
        Vector3 fDir = Vector3.ProjectOnPlane(Vector3.up, n);
        if (fDir.sqrMagnitude < 0.01f) fDir = Vector3.ProjectOnPlane(fwd, n);
        fDir.Normalize();

        pos = hit.point + n * palmOffset - fDir * wristBack
            + Vector3.ProjectOnPlane(fwd, n) * lead;

        if (!calibrated) { rot = curRot; return; }
        Quaternion world = Quaternion.LookRotation(fDir, -n);          // palma hacia la pared
        Quaternion local = Quaternion.LookRotation(fingerLocal, palmLocal);
        rot = world * Quaternion.Inverse(local);
    }

    // Mano relajada: dedos hacia abajo, palma hacia el muslo
    Quaternion RestRot(Vector3 dir, Vector3 fwd, float swing)
    {
        if (!calibrated) return curRot;
        Vector3 fingers = (Vector3.down + fwd * (restFingerForward + swing * 0.25f)).normalized;
        Quaternion world = Quaternion.LookRotation(fingers, -Vector3.ProjectOnPlane(dir, Vector3.up).normalized);
        Quaternion local = Quaternion.LookRotation(fingerLocal, palmLocal);
        return world * Quaternion.Inverse(local);
    }

    void SetRot(Quaternion goal, float dt)
    {
        if (!calibrated) return;
        curRot = Quaternion.Slerp(curRot, goal, 1f - Mathf.Exp(-rotateSpeed * dt));
        ikTarget.rotation = curRot;
    }
}