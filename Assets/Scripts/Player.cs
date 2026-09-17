using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Weapon weapon;
    [SerializeField] private float mouseSensitivity = 50f;
    [SerializeField] private float minVerticalAngle = -70f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private UIDocument uI;
    [SerializeField] public List<Weapon> weaponsToEquip = new();
    [NonSerialized] public bool isAiming;
    [NonSerialized] public bool isReloading;
    [NonSerialized] public bool isSwappingWeapon;

    public static Player instance;
    private Rigidbody rb;
    private Animator animator;
    private InputSystem_Actions inputs;
    private Vector2 lookInput;
    private float cameraPitch;
    private Vector2 moveInput;
    private bool freeLook = false;
    private WeaponHandIK handIK;
    private Weapon otherWeapon;

    private void Awake()
    {
        inputs = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        handIK = GetComponent<WeaponHandIK>();

        instance = this;

        weaponsToEquip.Add(weapon);
    }

    void Start()
    {
        if (weapon)
            weapon.weaponSway.CalculateAimOffset();

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
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

        inputs.Player.Zoom.performed += OnAim;

        inputs.Player.CursorLock.started += LockCursor;
        inputs.Player.CursorLock.canceled += LockCursor;

        inputs.Player.Reload.performed += Reload;

        inputs.Player.Interact.performed += OnIntract;

        inputs.Player.Previous.performed += OnPrevious;
    }

    private void OnDisable()
    {
        inputs.Player.Look.performed -= RotateCameraAndCharacter;
        inputs.Player.Look.canceled -= RotateCameraAndCharacter;

        inputs.Player.Move.performed -= MoveCharacter;
        inputs.Player.Move.canceled -= MoveCharacter;

        inputs.Player.Attack.started -= OnAttack;
        inputs.Player.Attack.canceled -= OnAttack;

        inputs.Player.Zoom.performed -= OnAim;

        inputs.Player.CursorLock.started -= LockCursor;
        inputs.Player.CursorLock.canceled -= LockCursor;

        inputs.Player.Reload.performed -= Reload;

        inputs.Player.Interact.performed -= OnIntract;

        inputs.Player.Previous.performed -= OnPrevious;

        inputs.Disable();
    }

    private void RotateCameraAndCharacter(InputAction.CallbackContext context)
    {
        if (!freeLook)
            lookInput = context.ReadValue<Vector2>();
        else
            lookInput = Vector2.zero;
    }

    public void LockCursor(InputAction.CallbackContext context)
    {
        freeLook = !freeLook;

        if (freeLook)
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        else
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        ApplyCameraPitch(mouseY);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        weapon.weaponSway.ApplySway(lookInput, moveInput);

        float walkLayerTarget = isAiming && !isReloading ? weapon.onAimWalkWeight : 1f;

        float firingLayerTarget = isAiming && !isReloading ? weapon.onAimFiringWeight : 1f;

        float layerSmooth = 5f * Time.deltaTime;

        DampLayerWeight(1, walkLayerTarget, layerSmooth);
        DampLayerWeight(2, firingLayerTarget, layerSmooth);
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

    private void OnTriggerEnter(Collider other)
    {
        otherWeapon = other.GetComponent<Weapon>();
        if (otherWeapon)
            uI.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (otherWeapon == other.GetComponent<Weapon>())
        {
            otherWeapon = null;
            uI.enabled = false;
        }
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        if (weaponsToEquip.Count == 1 || isSwappingWeapon)
            return;

        int index = weaponsToEquip.IndexOf(weapon) - 1;
        if (index < 0)
            index = weaponsToEquip.Count - 1;

        otherWeapon = weaponsToEquip[index];

        StartSwapAnimation();
    }

    private void OnIntract(InputAction.CallbackContext context)
    {
        if (otherWeapon == null || isSwappingWeapon)
            return;

        otherWeapon.transform.parent = cameraTransform;
        otherWeapon.ApplyInHandPos();
        otherWeapon.weaponSway.enabled = true;
        otherWeapon.weaponSway.CalculateAimOffset();
        otherWeapon.GetComponent<SphereCollider>().enabled = false;
        otherWeapon.gameObject.SetActive(false);

        uI.enabled = false;
        weaponsToEquip.Add(otherWeapon);
        StartSwapAnimation();
    }

    private void StartSwapAnimation()
    {
        isAiming = false;
        weapon.StopFire();
        weapon.firingAudio.Stop();

        isSwappingWeapon = true;
        animator.CrossFade("Holster", 0.1f, 2, Time.deltaTime);
        weapon.reloadAudio.Stop();
    }

    public void SwapWeapon()
    {
        otherWeapon.crosshair.enabled = true;
        otherWeapon.enabled = true;
        otherWeapon.transform.gameObject.SetActive(true);

        handIK.leftHandTarget = otherWeapon.leftHandTarget;
        handIK.rightHandTarget = otherWeapon.rightHandTarget;

        animator.runtimeAnimatorController = otherWeapon.animatorOverride;

        weapon.crosshair.rootVisualElement.visible = true;
        weapon.enabled = false;
        weapon.crosshair.enabled = false;
        weapon.transform.gameObject.SetActive(false);

        weapon = otherWeapon;
        weapon.Start();

        isSwappingWeapon = false;

        if (weapon.bulletsInMag <= 0)
            Reload();
    }

    public void ApplyCameraPitch(float value)
    {
        cameraPitch -= value;
        cameraPitch = Mathf.Clamp(cameraPitch, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    public void Reload()
    {
        isReloading = true;

        animator.SetBool("isReloading", true);
        animator.SetBool("isFiring", false);

        weapon.StopFire();
        weapon.reloadAudio.Play();
        weapon.firingAudio.Stop();
    }

    private void Reload(InputAction.CallbackContext context)
    {
        Reload();
    }

    public void EndReload()
    {
        isReloading = false;
        animator.SetBool("isReloading", false);
        if (!isSwappingWeapon)
            weapon.SetBulletCount(weapon.magazineSize);
    }

    private void MoveCharacter(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        bool isAttackHeld = context.ReadValueAsButton();
        animator.SetBool("isFiring", isAttackHeld);

        if (isAttackHeld && !isReloading && !isSwappingWeapon)
        {
            weapon.StartFire();

            if (!weapon.firingAudio.isPlaying)
            {
                weapon.firingAudio.loop = true;
                weapon.firingAudio.Play();
            }
        }
        else
        {
            weapon.StopFire();
            weapon.firingAudio.Stop();
            animator.SetBool("isFiring", false);
        }
    }

    private void OnAim(InputAction.CallbackContext context)
    {
        if (isSwappingWeapon)
            return;

        isAiming = !isAiming;

        if (isAiming)
        {
            weapon.crosshair.rootVisualElement.visible = false;
        }
        else
        {
            weapon.crosshair.rootVisualElement.visible = true;
        }
    }

    private void DampLayerWeight(int layerIndex, float targetWeight, float dampTime)
    {
        float currentWeight = animator.GetLayerWeight(layerIndex);

        currentWeight = Mathf.MoveTowards(
            currentWeight,
            targetWeight,
            dampTime
        );

        animator.SetLayerWeight(layerIndex, currentWeight);
    }
}