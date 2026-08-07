using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private WeaponSway weaponSway;
    [SerializeField] private AudioSource weaponAudioSource;
    [SerializeField] private float mouseSensitivity = 50f;
    [SerializeField] private float minVerticalAngle = -70f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private float moveSpeed = 3f;
    private Rigidbody rb;
    private Animator animator;

    private InputSystem_Actions inputs;
    private Vector2 lookInput;
    private float cameraPitch;
    private Vector2 moveInput;

    private void Awake()
    {
        inputs = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        inputs.Enable();

        inputs.Player.Look.performed += RotateCameraAndCharacter;
        inputs.Player.Look.canceled += RotateCameraAndCharacter;

        inputs.Player.Move.performed += MoveCharacter;
        inputs.Player.Move.canceled += MoveCharacter;

        inputs.Player.Attack.started += OnAttack;
        inputs.Player.Attack.canceled += OnAttack;
    }

    private void OnDisable()
    {
        inputs.Player.Look.performed -= RotateCameraAndCharacter;
        inputs.Player.Look.canceled -= RotateCameraAndCharacter;

        inputs.Player.Move.performed -= MoveCharacter;
        inputs.Player.Move.canceled -= MoveCharacter;

        inputs.Player.Attack.started -= OnAttack;
        inputs.Player.Attack.canceled -= OnAttack;

        inputs.Disable();
    }

    private void RotateCameraAndCharacter(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minVerticalAngle, maxVerticalAngle);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        weaponSway.ApplySway(lookInput, moveInput);
    }

    private void FixedUpdate()
    {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize();

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        if (moveDirection.x != 0 || moveDirection.y != 0)
            animator.SetBool("isMoving", true);
        else
            animator.SetBool("isMoving", false);
    }

    private void MoveCharacter(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        bool isAttackHeld = context.ReadValueAsButton();
        animator.SetBool("isFiring", isAttackHeld);

        if (isAttackHeld)
        {
            if (!weaponAudioSource.isPlaying)
            {
                weaponAudioSource.loop = true;
                weaponAudioSource.Play();
            }
        }
        else
        {
            weaponAudioSource.Stop();
        }
    }
}