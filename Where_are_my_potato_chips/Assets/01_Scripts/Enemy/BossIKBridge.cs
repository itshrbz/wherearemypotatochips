using UnityEngine;

public class BossIKBridge : MonoBehaviour
{
    public EnemyStateMachine ai;
    public ProceduralLeg[] legs;
    public WallHand[] hands;
    public float climbReach = 1.4f;     // alcance de las manos mientras cuelga

    float[] baseReach;

    void Awake()
    {
        if (!ai) ai = GetComponentInParent<EnemyStateMachine>();
        baseReach = new float[hands.Length];
        for (int i = 0; i < hands.Length; i++) baseReach[i] = hands[i].reach;
    }

    void Update()
    {
        if (!ai) return;
        bool hanging = ai.currentState == EnemyStateMachine.EnemyState.ClimbHorizontal;
        foreach (var l in legs) l.freeHang = hanging;
        for (int i = 0; i < hands.Length; i++)
            hands[i].reach = hanging ? climbReach : baseReach[i];
    }
}