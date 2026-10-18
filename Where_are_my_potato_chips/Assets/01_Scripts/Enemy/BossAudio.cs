using UnityEngine;

public class BossAudio : MonoBehaviour
{
    public ProceduralLeg[] legs;
    public WallHand[] hands;
    void OnEnable()
    {
        foreach (var l in legs) l.Planted += OnStep;
        foreach (var h in hands) h.Planted += OnHand;
    }
    void OnDisable()
    {
        foreach (var l in legs) l.Planted -= OnStep;
        foreach (var h in hands) h.Planted -= OnHand;
    }
    void OnStep(Vector3 p) => Sfx.Play("boss_step", p);
    void OnHand(Vector3 p) => Sfx.Play("boss_hand", p);
}