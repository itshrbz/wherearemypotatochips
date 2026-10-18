using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    public string stepId = "step_player";
    [Header("Metros entre pasos")]
    public float walkStride = 1.25f, sprintStride = 1.9f, crouchStride = 0.85f;
    [Header("Volumen")]
    public float walkVolume = 0.7f, sprintVolume = 1f, crouchVolume = 0.35f, landVolume = 1f;

    CharacterController cc;
    PlayerState state;
    PlayerController pc;
    float distance;
    bool wasGrounded = true;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        state = GetComponent<PlayerState>();
        pc = GetComponent<PlayerController>();
    }

    void Update()
    {
        bool grounded = cc.isGrounded;
        if (state != null && state.InputLocked) { distance = 0f; wasGrounded = grounded; return; }

        if (grounded && !wasGrounded) Sfx.Play(stepId, landVolume);     // aterrizaje
        wasGrounded = grounded;

        Vector3 v = cc.velocity; v.y = 0f;
        if (!grounded || v.magnitude < 0.3f) return;                    // quieto o en el aire

        bool crouch = state != null && state.IsCrouching;
        bool sprint = pc != null && pc.IsSprinting;
        float stride = crouch ? crouchStride : sprint ? sprintStride : walkStride;

        distance += v.magnitude * Time.deltaTime;
        if (distance >= stride)
        {
            distance -= stride;
            Sfx.Play(stepId, crouch ? crouchVolume : sprint ? sprintVolume : walkVolume);
        }
    }
}