using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lanza un raycast desde la cámara del jugador y, con E (o el botón Oeste del mando),
/// llama a Interact() del IInteractable que esté apuntando.
/// Va en el GameObject del Player.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] Camera viewCamera;
    [SerializeField] float range = 3f;
    [SerializeField] float aimRadius = 0.08f;
    [SerializeField] LayerMask mask = ~0;
    [Tooltip("Dibuja un punto de mira y el texto de ayuda con OnGUI mientras no exista la UI final.")]
    [SerializeField] bool drawFallbackUI = true;

    /// <summary>Se dispara cuando cambia el texto de ayuda. null = no hay nada que interactuar.</summary>
    public static event Action<string> PromptChanged;

    public string CurrentPrompt { get; private set; }

    public bool DrawFallbackUI
    {
        get => drawFallbackUI;
        set => drawFallbackUI = value;
    }

    IInteractable current;
    InputAction interactAction;
    PlayerState state;
    GUIStyle promptStyle;

    void Awake()
    {
        if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
        if (viewCamera == null) viewCamera = Camera.main;
        state = GetComponent<PlayerState>();

        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonWest");
    }

    void OnEnable() => interactAction.Enable();

    void OnDisable()
    {
        interactAction.Disable();
        SetPrompt(null);
    }

    void OnDestroy() => interactAction.Dispose();

    void Update()
    {
        if (state != null && state.InputLocked)
        {
            current = null;
            SetPrompt(null);
            return;
        }

        current = FindTarget();
        SetPrompt(current != null ? current.Prompt : null);

        if (current != null && interactAction.WasPressedThisFrame())
            current.Interact(gameObject);
    }

    IInteractable FindTarget()
    {
        if (viewCamera == null) return null;
        var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
        // Un rayo grueso para que los objetos chicos (producto, caja) no cuesten de apuntar
        if (!Physics.SphereCast(ray, aimRadius, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore)
            && !Physics.Raycast(ray, out hit, range, mask, QueryTriggerInteraction.Ignore))
            return null;

        var target = hit.collider.GetComponentInParent<IInteractable>();
        return target != null && target.CanInteract ? target : null;
    }

    void SetPrompt(string prompt)
    {
        if (prompt == CurrentPrompt) return;
        CurrentPrompt = prompt;
        PromptChanged?.Invoke(prompt);
    }

    void OnGUI()
    {
        if (!drawFallbackUI) return;
        if (promptStyle == null)
            promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        GUI.Label(new Rect(cx - 10, cy - 10, 20, 20), CurrentPrompt != null ? "+" : "·", promptStyle);
        if (CurrentPrompt != null)
            GUI.Label(new Rect(cx - 200, cy + 24, 400, 30), "[E] " + CurrentPrompt, promptStyle);
    }
}
