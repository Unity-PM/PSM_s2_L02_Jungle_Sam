using UnityEngine;
using UnityEngine.EventSystems;
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

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        
        // Znalezienie broni w dzieciach (jeśli nie przypisana w Inspektorze)
        if (currentWeapon == null)
        {
            currentWeapon = GetComponentInChildren<WeaponBase>();
        }
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
        if (Mouse.current != null)
            _lookInput = Mouse.current.delta.ReadValue();

        if (Keyboard.current != null)
        {
            _moveInput = new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
            );
            _jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            _sprintPressed = Keyboard.current.leftShiftKey.isPressed;
        }
    }

    void HandleLook()
    {
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

        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _controller.Move(move * currentSpeed * Time.deltaTime);

        if (_jumpPressed && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    void UpdateWeaponAnimations()
    {
        if (currentWeapon == null)
            return;

        // Sprawdzenie, czy gracz się porusza
        bool isMoving = _moveInput.magnitude > 0.1f;
        
        // Sprawdzenie, czy gracz sprintuje
        bool isSprinting = isMoving && _sprintPressed;

        // Wysłanie danych animacji do broni
        currentWeapon.SetMovementAnimations(isMoving && !isSprinting, isSprinting);
    }
}