using UnityEngine;
using UnityEngine.InputSystem; // Nowa biblioteka

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 12f;
    public float jumpHeight = 2f;
    public float gravity = -19.62f;

    [Header("Look Settings")]
    public float mouseSensitivity = 0.1f; // Nowy system używa innych wartości dla myszy
    public Transform playerCamera;

    [Header("Weapon Reference")]
    public WeaponBase currentWeapon;

    private CharacterController _controller;
    private Vector3 _velocity;
    private float _xRotation = 0f;
    private bool _isGrounded;

    // Referencje do nowych akcji
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _jumpPressed;
    private bool _sprintPressed;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Znalezienie broni w dzieciach (jeśli nie przypisana w Inspektorze)
        if (currentWeapon == null)
        {
            currentWeapon = GetComponentInChildren<WeaponBase>();
        }
    }

    public void ResetLook(Quaternion worldRotation)
    {
        Vector3 euler = worldRotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
        _xRotation = 0f;
        _lookInput = Vector2.zero;
        _velocity = Vector3.zero;

        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.identity;
    }

    void Update()
    {
        ReadInput();
        HandleLook();
        HandleMovement();
        UpdateWeaponAnimations();
    }

    void ReadInput()
    {
        // Odczyt myszy i klawiatury w nowym systemie
        _lookInput = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        if (Keyboard.current != null)
        {
            Vector2 moveInput = new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
            );

            _moveInput = Vector2.ClampMagnitude(moveInput, 1f);
            _jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            _sprintPressed = Keyboard.current.leftShiftKey.isPressed;
        }
        else
        {
            _moveInput = Vector2.zero;
            _jumpPressed = false;
            _sprintPressed = false;
        }
    }

    void HandleLook()
    {
        if (playerCamera == null)
            return;

        float mouseX = _lookInput.x * mouseSensitivity;
        float mouseY = _lookInput.y * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;

        float currentSpeed = _sprintPressed ? sprintSpeed : walkSpeed;

        Vector3 horizontalMove = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        if (_jumpPressed && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        _velocity.y += gravity * Time.deltaTime;

        Vector3 frameMove = horizontalMove * currentSpeed + _velocity;
        _controller.Move(frameMove * Time.deltaTime);
    }

    void UpdateWeaponAnimations()
    {
        if (currentWeapon == null)
            return;

        // Sprawdzenie, czy gracz się porusza
        bool isMoving = _moveInput.sqrMagnitude > 0.01f;

        // Sprawdzenie, czy gracz sprintuje
        bool isSprinting = isMoving && _sprintPressed;

        // Wysłanie danych animacji do broni
        currentWeapon.SetMovementAnimations(isMoving && !isSprinting, isSprinting);
    }
}
