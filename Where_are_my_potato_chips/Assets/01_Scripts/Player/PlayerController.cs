using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using static UnityEngine.Audio.GeneratorInstance;

public class PlayerController : MonoBehaviour
{
    public float speed = 2;
    public float jumpHeight = 0.5f;
    public float gravity = -9.8f;
    public float Sensibility = 0.5f;
    public float maxLimit = 80f;
    public float minLimit = -80f;
    private float vRotation;
    private bool groundedPlayer;
    public Transform camera;

    private PlayerInput inputAction;
    private CharacterController characterController;

    private Vector2 movement;
    private Vector2 look;
    private Vector3 velocity;
    private bool jumpPressed;
    private void Awake()
    {
        inputAction = new PlayerInput();
        characterController = GetComponent<CharacterController>();
    }
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        inputAction.Player.Enable();

        inputAction.Player.Move.performed += setMovement;
        inputAction.Player.Move.canceled += obj => movement = Vector2.zero;

        inputAction.Player.Look.performed += setLook;
        inputAction.Player.Look.canceled += obj => look = Vector2.zero;

        inputAction.Player.Jump.performed += setJump;
        inputAction.Player.Jump.canceled += obj => jumpPressed = false;
    }
    private void Update()
    {
        Movement();
        Look();
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
        Vector2 mouse = look * Sensibility;
        vRotation = Mathf.Clamp(vRotation - mouse.y, minLimit, maxLimit);
        camera.localRotation = Quaternion.Euler(vRotation, 0, 0);
        transform.Rotate(Vector3.up * mouse.x);
    }
    private void Movement()
    {
        Vector3 move = transform.right * movement.x + transform.forward * movement.y;
        characterController.Move(move * speed * Time.deltaTime);
        groundedPlayer = characterController.isGrounded;
        if (groundedPlayer && velocity.y < 0)
        {
            velocity.y = 0f;
        }
        if (jumpPressed && groundedPlayer)
        {
            velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
            jumpPressed = false;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
