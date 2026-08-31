using System;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] private float swayAmount = 2f;
    [SerializeField] private float maxSwayAngle = 5f;

    [Header("Move Offset")]
    [SerializeField] private Vector3 leftMoveRotationOffset = new Vector3(50f, 5f, 0f);
    [SerializeField] private Vector3 rightMoveRotationOffset = new Vector3(-30f, 5f, 0f);
    [SerializeField] private Vector3 backMoveRotationOffset = new Vector3(30f, -10f, -10f);
    [SerializeField] private float moveOffsetSmooth = 5f;

    [Header("Aim")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private float aimSmooth = 12f;
    [SerializeField] private Vector3 aimOffset;
    [SerializeField] private float maxAimAngle = 1f;
    [SerializeField] private Transform reticle;
    [SerializeField] private float horizontalLimit = 0.005f;
    [SerializeField] private float verticalLimit = 0.005f;

    [NonSerialized] public float distanceToAim;

    private Vector3 aimPos;
    private Vector3 startPosition;

    private Quaternion startRotation;
    private Vector3 reticleStartPosition;

    private void Awake()
    {
        startRotation = transform.localRotation;
        startPosition = transform.localPosition;
        if (reticle)
            reticleStartPosition = reticle.localPosition;
    }

    void Start()
    {
        Transform camera = Camera.main.transform;
        float distance = Vector3.Dot(aimPoint.position - camera.position, camera.forward);
        Vector3 cameraCenter = camera.position + camera.forward * distance;
        Vector3 localCameraCenter = transform.parent.InverseTransformPoint(cameraCenter);
        Vector3 localAimPoint = transform.parent.InverseTransformPoint(aimPoint.position);
        aimPos = localCameraCenter - localAimPoint;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = startPosition;

        if (Player.instance.isAiming)
        {
            targetPosition = aimPos + aimOffset;

            Vector3 rotation = transform.localRotation * aimPoint.localPosition;
            targetPosition.z -= rotation.z;
        }

        distanceToAim = Vector3.Distance(transform.localPosition, targetPosition);

        if (distanceToAim <= 0.001f)
        {
            transform.localPosition = targetPosition;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetPosition,
                Time.deltaTime * aimSmooth
            );
        }
    }

    void Update()
    {
        if (Player.instance.isAiming && reticle)
        {
            ReticleAtCenter();
        }
    }

    void ReticleAtCenter()
    {
        Transform parent = reticle.parent;
        Transform cam = Camera.main.transform;

        Vector3 localRayOrigin = parent.InverseTransformPoint(cam.position);

        Vector3 directionToAimPoint =
       (aimPoint.position - cam.position).normalized;
        Vector3 localRayDirection = parent.InverseTransformDirection(directionToAimPoint);

        Vector3 localPosition = reticle.localPosition;

        float fixedX = localPosition.x;

        if (Mathf.Abs(localRayDirection.x) > 0.0001f)
        {
            float distance = (fixedX - localRayOrigin.x) / localRayDirection.x;
            Vector3 localTarget = localRayOrigin + localRayDirection * distance;

            localPosition.y = Mathf.Clamp(
                localTarget.y,
                reticleStartPosition.y - verticalLimit,
                reticleStartPosition.y + verticalLimit
            );

            localPosition.z = Mathf.Clamp(
                localTarget.z,
                reticleStartPosition.z - horizontalLimit,
                reticleStartPosition.z + horizontalLimit
            );

            reticle.localPosition = localPosition;
        }
    }

    public void ApplySway(Vector2 lookInput, Vector2 moveInput)
    {
        if (Player.instance.isAiming)
            OnAimSway(lookInput, moveInput);
        else
            NormalSway(lookInput, moveInput);
    }

    private void NormalSway(Vector2 lookInput, Vector2 moveInput)
    {
        float swayX = Mathf.Clamp(-lookInput.y * swayAmount, -maxSwayAngle, maxSwayAngle);
        float swayY = Mathf.Clamp(lookInput.x * swayAmount, -maxSwayAngle, maxSwayAngle);

        Vector3 moveRotationOffset = Vector3.zero;

        if (moveInput.x < -0.1f)
        {
            moveRotationOffset += leftMoveRotationOffset * Mathf.Abs(moveInput.x);
        }
        else if (moveInput.x > 0.1f)
        {
            moveRotationOffset += rightMoveRotationOffset * Mathf.Abs(moveInput.x);
        }

        if (moveInput.y < -0.1f)
        {
            moveRotationOffset += backMoveRotationOffset * Mathf.Abs(moveInput.y);
        }

        Quaternion targetRotation =
            startRotation *
            Quaternion.Euler(moveRotationOffset) *
            Quaternion.Euler(swayX, swayY, 0f);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * moveOffsetSmooth
        );
    }

    private void OnAimSway(Vector2 lookInput, Vector2 moveInput)
    {
        float swayX = Mathf.Clamp(moveInput.x * swayAmount * 3f, -maxAimAngle, maxAimAngle);
        float swayY = Mathf.Clamp(lookInput.x * swayAmount, -0.5f, 0.5f);

        Quaternion swayRotation = Quaternion.Euler(-swayX, swayY, 0f);
        Quaternion targetRotation = startRotation * swayRotation;

        Quaternion newRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * 3f
        );
        transform.localRotation = newRotation;
    }

}