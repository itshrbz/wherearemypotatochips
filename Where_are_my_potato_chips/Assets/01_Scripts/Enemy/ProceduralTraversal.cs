using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ProceduralTraversal : MonoBehaviour
{
    public ProceduralLeg[] legs;
    public WallHand[] hands;               // [0] izquierda, [1] derecha
    public bool IsTraversing { get; private set; }

    public Coroutine Begin(NavMeshAgent agent) => StartCoroutine(Run(agent));

    IEnumerator Run(NavMeshAgent agent)
    {
        IsTraversing = true;
        OffMeshLinkData d = agent.currentOffMeshLinkData;
        Vector3 off = Vector3.up * agent.baseOffset;
        Vector3 a = agent.transform.position, b = d.endPos + off;
        Vector3 flat = Vector3.ProjectOnPlane(b - a, Vector3.up);
        Vector3 dir = flat.sqrMagnitude > 0.001f ? flat.normalized : agent.transform.forward;
        float rise = b.y - a.y;
        bool climb = rise > 0.5f;

        // 1) girar hacia el borde
        Quaternion look = Quaternion.LookRotation(dir);
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.2f)
        { agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, look, t); yield return null; }

        foreach (var l in legs) l.freeHang = true;

        Vector3 ledge = b - off - dir * agent.radius;                // borde superior
        Vector3 side = Vector3.Cross(Vector3.up, dir) * 0.35f;
        Vector3[] start = new Vector3[hands.Length];
        Vector3[] goal = new Vector3[hands.Length];
        for (int i = 0; i < hands.Length; i++)
        {
            start[i] = hands[i].ikTarget.position;
            goal[i] = ledge + side * (i == 0 ? -1f : 1f);
            if (climb) hands[i].overrideControl = true;
        }

        // 2) curva Bézier del cuerpo
        Vector3 c = climb ? new Vector3(a.x, b.y + 0.4f, a.z) + dir * 0.3f
                          : (a + b) / 2f + Vector3.up * 0.9f;
        float dur = climb ? Mathf.Clamp(0.7f + rise * 0.5f, 0.9f, 2f)
                          : Mathf.Clamp(flat.magnitude * 0.35f, 0.5f, 1.2f);
        bool released = false;

        for (float t = 0; t < 1f; t += Time.deltaTime / dur)
        {
            float k = Mathf.SmoothStep(0, 1, t);
            Vector3 p = (1 - k) * (1 - k) * a + 2 * (1 - k) * k * c + k * k * b;
            agent.transform.position = p;

            if (climb)
                for (int i = 0; i < hands.Length; i++)
                    hands[i].ikTarget.position = Vector3.Lerp(start[i], goal[i], Mathf.Clamp01(t / 0.25f));

            if (!released && t > 0.8f)       // las piernas suben y se plantan arriba
            { foreach (var l in legs) l.freeHang = false; released = true; }
            yield return null;
        }

        // 3) terminar
        agent.transform.position = b;
        foreach (var l in legs) l.freeHang = false;
        foreach (var hd in hands) { hd.overrideControl = false; hd.SyncFromTarget(); }
        agent.CompleteOffMeshLink();
        IsTraversing = false;
    }
}