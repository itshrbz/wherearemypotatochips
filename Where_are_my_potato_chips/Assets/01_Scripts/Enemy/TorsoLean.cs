using UnityEngine;

public class TorsoLean : MonoBehaviour
{
    public Transform torso;
    public WallHand left, right;
    public float maxLean = 8f;
    Quaternion baseRot;
    void Start() { baseRot = torso.localRotation; }
    void LateUpdate()
    {
        float k = (right.IsPlanted ? 1f : 0f) - (left.IsPlanted ? 1f : 0f);
        Quaternion q = baseRot * Quaternion.Euler(0, 0, -k * maxLean);
        torso.localRotation = Quaternion.Slerp(torso.localRotation, q, 6f * Time.deltaTime);
    }
}