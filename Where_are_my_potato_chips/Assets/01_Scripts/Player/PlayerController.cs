using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Controlador en primera persona (base de Grover).
/// Caminar, mirar y saltar con las acciones de PlayerInput.inputactions.
/// Correr (Shift / L3) con energía y agacharse (Ctrl o C / botón Este) con acciones en código.
/// Escribe PlayerState.IsCrouching y respeta PlayerState.InputLocked.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerInteractor))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 2;
    public float jumpHeight = 0.5f;
    public float gravity = -9.8f;

    [Header("Cámara")]
    public float Sensibility = 0.5f;
    public float maxLimit = 80f;
    public float minLimit = -80f;
    [FormerlySerializedAs("camera")] public Transform cameraTransform;

    [Header("Correr")]
    public float sprintMultiplier = 1.8f;
    [Tooltip("Segundos que se puede correr con la energía llena.")]
    public float maxStamina = 5f;
    [Tooltip("Energía recuperada por segundo.")]
    public float staminaRegen = 1f;
    [Tooltip("Segundos sin correr antes de empezar a recuperar energía.")]
    public float regenDelay = 1f;
    [Tooltip("Si la energía se agota, no se puede volver a correr hasta recuperar esta fracción.")]
    [Range(0f, 1f)] public float minStaminaToSprint = 0.25f;

    [Header("Agacharse")]
    public float crouchHeight = 1f;
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchCameraDrop = 0.6f;
    public float crouchTransitionSpeed = 8f;
    public LayerMask ceilingMask = ~0;

    [Header("UI provisional")]
    public bool drawStaminaBar = true;

    /// <summary>Energía entre 0 y 1, para la barra de la UI.</summary>
    public float Stamina01 => maxStamina > 0f ? stamina / maxStamina : 0f;
    public bool IsSprinting { get; private set; }

    private PlayerInput inputAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private CharacterController characterController;
    private PlayerState state;

    private Vector2 movement;
    private Vector2 look;
    private Vector3 velocity;
    private bool jumpPressed;
    private bool groundedPlayer;
    private float vRotation;

    private float stamina;
    private float lastSprintTime = -999f;
    private bool exhausted;

    private float standHeight;
    private Vector3 standCenter;
    private float standCameraY;
    private float currentHeight;

    private void Awake()
    {
        inputAction = new PlayerInput();
        characterController = GetComponent<CharacterController>();

        state = GetComponent<PlayerState>();
        if (state == null) state = gameObject.AddComponent<PlayerState>();
        if (GetComponent<PlayerInteractor>() == null) gameObject.AddComponent<PlayerInteractor>();

        // El CharacterController ya es el colisionador del jugador; un CapsuleCollider extra estorba al agacharse.
        if (TryGetComponent(out CapsuleCollider extraCapsule)) extraCapsule.enabled = false;
        if (gameObject.CompareTag("Untagged")) gameObject.tag = "Player";

        if (cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }

        sprintAction = new InputAction("Sprint", InputActionType.Button);
        sprintAction.AddBinding("<Keyboard>/leftShift");
        sprintAction.AddBinding("<Gamepad>/leftStickPress");

        crouchAction = new InputAction("Crouch", InputActionType.Button);
        crouchAction.AddBinding("<Keyboard>/leftCtrl");
        crouchAction.AddBinding("<Keyboard>/c");
        crouchAction.AddBinding("<Gamepad>/buttonEast");

        stamina = maxStamina;
        standHeight = characterController.height;
        standCenter = characterController.center;
        currentHeight = standHeight;
        if (cameraTransform != null) standCameraY = cameraTransform.localPosition.y;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        inputAction.Player.Move.performed += setMovement;
        inputAction.Player.Move.canceled += obj => movement = Vector2.zero;

        inputAction.Player.Look.performed += setLook;
        inputAction.Player.Look.canceled += obj => look = Vector2.zero;

        inputAction.Player.Jump.performed += setJump;
        inputAction.Player.Jump.canceled += obj => jumpPressed = false;
    }

    private void OnEnable()
    {
        inputAction.Player.Enable();
        sprintAction.Enable();
        crouchAction.Enable();
    }

    private void OnDisable()
    {
        inputAction.Player.Disable();
        sprintAction.Disable();
        crouchAction.Disable();
    }

    private void OnDestroy()
    {
        inputAction.Dispose();
        sprintAction.Dispose();
        crouchAction.Dispose();
    }

    private void Update()
    {
        bool locked = state.InputLocked;
        Vector2 move = locked ? Vector2.zero : movement;
        if (locked) jumpPressed = false;

        Crouch(locked);
        Sprint(move, locked);
        Movement(move);
        if (!locked) Look();
    }

    /// <summary>Mueve al jugador a un punto (por ejemplo, el punto de aparición de un nivel).</summary>
    public void Teleport(Vector3 position, float yaw)
    {
        characterController.enabled = false;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        velocity = Vector3.zero;
        characterController.enabled = true;
    }

    private void setJump(InputAction.CallbackContext obj)
    {
        jumpPressed = true;
    }

    private void setLook(InputAction.CallbackContext obj)
    {
        look = obj.ReadValue<Vector2>();
    }

    private void setMovement(InputAction.CallbackContext obj)
    {
        movement = obj.ReadValue<Vector2>();
    }

    private void Look()
    {
        if (cameraTransform == null) return;
        Vector2 mouse = look * Sensibility;
        vRotation = Mathf.Clamp(vRotation - mouse.y, minLimit, maxLimit);
        cameraTransform.localRotation = Quaternion.Euler(vRotation, 0, 0);
        transform.Rotate(Vector3.up * mouse.x);
    }

    private void Crouch(bool locked)
    {
        bool wantsCrouch = locked ? state.IsCrouching : crouchAction.IsPressed();
        // Si hay techo encima, no se puede levantar.
        if (!wantsCrouch && state.IsCrouching && !CanStandUp()) wantsCrouch = true;
        state.IsCrouching = wantsCrouch;

        float target = wantsCrouch ? crouchHeight : standHeight;
        currentHeight = Mathf.MoveTowards(currentHeight, target, crouchTransitionSpeed * Time.deltaTime);

        characterController.height = currentHeight;
        characterController.center = standCenter - Vector3.up * ((standHeight - currentHeight) * 0.5f);

        if (cameraTransform != null)
        {
            float range = Mathf.Max(0.0001f, standHeight - crouchHeight);
            float t = Mathf.Clamp01((standHeight - currentHeight) / range);
            Vector3 p = cameraTransform.localPosition;
            p.y = standCameraY - crouchCameraDrop * t;
            cameraTransform.localPosition = p;
        }
    }

    private bool CanStandUp()
    {
        float radius = characterController.radius * 0.95f;
        Vector3 center = transform.TransformPoint(characterController.center);
        Vector3 top = center + Vector3.up * (currentHeight * 0.5f - characterController.radius);
        float distance = standHeight - currentHeight + 0.05f;
        return !Physics.SphereCast(top, radius, Vector3.up, out _, distance, ceilingMask, QueryTriggerInteraction.Ignore);
    }

    private void Sprint(Vector2 move, bool locked)
    {
        bool movingForward = move.y > 0.1f;
        bool wantsSprint = !locked && sprintAction.IsPressed() && movingForward && !state.IsCrouching
                           && (characterController.isGrounded || IsSprinting);

        if (exhausted && stamina >= maxStamina * minStaminaToSprint) exhausted = false;
        IsSprinting = wantsSprint && !exhausted && stamina > 0f;

        if (IsSprinting)
        {
            stamina -= Time.deltaTime;
            lastSprintTime = Time.time;
            if (stamina <= 0f)
            {
                stamina = 0f;
                exhausted = true;
            }
        }
        else if (Time.time - lastSprintTime >= regenDelay)
        {
            stamina = Mathf.Min(maxStamina, stamina + staminaRegen * Time.deltaTime);
        }
    }

    private void Movement(Vector2 move)
    {
        float multiplier = state.IsCrouching ? crouchSpeedMultiplier : (IsSprinting ? sprintMultiplier : 1f);
        Vector3 direction = transform.right * move.x + transform.forward * move.y;
        if (direction.sqrMagnitude > 1f) direction.Normalize();
        characterController.Move(direction * speed * multiplier * Time.deltaTime);

        groundedPlayer = characterController.isGrounded;
        if (groundedPlayer && velocity.y < 0)
        {
            velocity.y = -2f; // mantiene al CharacterController pegado al piso
        }
        if (jumpPressed && groundedPlayer && !state.IsCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
            jumpPressed = false;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void OnGUI()
    {
        if (!drawStaminaBar || stamina >= maxStamina) return;
        const float w = 200f, h = 8f;
        var back = new Rect(20, Screen.height - 30, w, h);
        GUI.Box(back, GUIContent.none);
        GUI.Box(new Rect(back.x, back.y, w * Stamina01, h), GUIContent.none);
    }
}
