using UnityEngine;

/// <summary>Amplía el alcance de las manos mientras el jefe trepa un NavMeshLink.</summary>
public class BossIKBridge : MonoBehaviour
{
    public BossAI ai;
    public WallHand[] hands;
    public float climbReach = 1.4f;

    float[] baseReach;

    void Awake()
    {
        if (ai == null) ai = GetComponentInParent<BossAI>();
        if (hands == null) hands = new WallHand[0];

        baseReach = new float[hands.Length];
        for (int i = 0; i < hands.Length; i++)
            if (hands[i] != null) baseReach[i] = hands[i].reach;
    }

    void Update()
    {
        if (ai == null) return;

        bool climbing = ai.IsTraversing;
        for (int i = 0; i < hands.Length; i++)
            if (hands[i] != null) hands[i].reach = climbing ? climbReach : baseReach[i];
    }
}
