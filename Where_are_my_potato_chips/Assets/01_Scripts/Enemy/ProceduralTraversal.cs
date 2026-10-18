using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Recorre un NavMeshLink con una curva: salto si el destino está a la misma altura,
/// trepada (manos al borde, piernas colgando) si está más arriba.
/// </summary>
public class ProceduralTraversal : MonoBehaviour
{
    public ProceduralLeg[] legs;
    public WallHand[] hands;               // [0] izquierda, [1] derecha
    [SerializeField] float climbThreshold = 0.5f;
    [SerializeField] float turnTime = 0.2f;

    public bool IsTraversing { get; private set; }

    public Coroutine Begin(NavMeshAgent agent)
    {
        if (IsTraversing || agent == null || !agent.isOnOffMeshLink) return null;
        return StartCoroutine(Run(agent));
    }

    IEnumerator Run(NavMeshAgent agent)
    {
        IsTraversing = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        Transform body = agent.transform;
        OffMeshLinkData link = agent.currentOffMeshLinkData;
        Vector3 offset = Vector3.up * agent.baseOffset;
        Vector3 start = body.position;
        Vector3 end = link.endPos + offset;
        Vector3 flat = Vector3.ProjectOnPlane(end - start, Vector3.up);
        Vector3 direction = flat.sqrMagnitude > 0.001f ? flat.normalized : body.forward;
        float rise = end.y - start.y;
        bool climb = rise > climbThreshold;

        Quaternion from = body.rotation;
        Quaternion look = Quaternion.LookRotation(direction);
        for (float t = 0f; t < 1f; t += Time.deltaTime / turnTime)
        {
            body.rotation = Quaternion.Slerp(from, look, t);
            yield return null;
        }
        body.rotation = look;

        SetLegsHanging(true);

        Vector3 ledge = end - offset - direction * agent.radius;
        Vector3 side = Vector3.Cross(Vector3.up, direction) * 0.35f;
        Vector3[] handStart = new Vector3[hands.Length];
        Vector3[] handGoal = new Vector3[hands.Length];
        for (int i = 0; i < hands.Length; i++)
        {
            if (!HasTarget(i)) continue;
            handStart[i] = hands[i].ikTarget.position;
            handGoal[i] = ledge + side * (i == 0 ? -1f : 1f);
            hands[i].overrideControl = climb;
        }

        Vector3 control = climb
            ? new Vector3(start.x, end.y + 0.4f, start.z) + direction * 0.3f
            : (start + end) * 0.5f + Vector3.up * 0.9f;
        float duration = climb
            ? Mathf.Clamp(0.7f + rise * 0.5f, 0.9f, 2f)
            : Mathf.Clamp(flat.magnitude * 0.35f, 0.5f, 1.2f);

        bool legsPlanted = false;
        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            float k = Mathf.SmoothStep(0f, 1f, t);
            body.position = (1 - k) * (1 - k) * start + 2 * (1 - k) * k * control + k * k * end;

            if (climb)
            {
                float reach = Mathf.Clamp01(t / 0.25f);
                for (int i = 0; i < hands.Length; i++)
                    if (HasTarget(i)) hands[i].ikTarget.position = Vector3.Lerp(handStart[i], handGoal[i], reach);
            }

            if (!legsPlanted && t > 0.8f)
            {
                SetLegsHanging(false);
                legsPlanted = true;
            }
            yield return null;
        }

        Finish(agent, end);
    }

    void Finish(NavMeshAgent agent, Vector3 end)
    {
        SetLegsHanging(false);
        foreach (var hand in hands)
        {
            if (hand == null) continue;
            hand.overrideControl = false;
            if (hand.ikTarget != null) hand.SyncFromTarget();
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.CompleteOffMeshLink();
            agent.Warp(end - Vector3.up * agent.baseOffset);
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
        IsTraversing = false;
    }

    void OnDisable()
    {
        if (!IsTraversing) return;
        StopAllCoroutines();
        SetLegsHanging(false);
        IsTraversing = false;
    }

    void SetLegsHanging(bool hanging)
    {
        if (legs == null) return;
        foreach (var leg in legs)
            if (leg != null) leg.freeHang = hanging;
    }

    bool HasTarget(int index) => hands[index] != null && hands[index].ikTarget != null;
}
