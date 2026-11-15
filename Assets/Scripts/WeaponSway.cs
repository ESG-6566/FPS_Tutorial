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
    [SerializeField] public float aimSmooth = 12f;
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
    private float aimProgress;


    public void CalculateAimOffset()
    {
        startRotation = transform.localRotation;
        startPosition = transform.localPosition;
        if (reticle)
            reticleStartPosition = reticle.localPosition;

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

        if (Player.instance.isAiming && !Player.instance.isReloading)
        {
            Vector3 baseAimPosition = aimPos + aimOffset;

            float totalAimDistance = Vector3.Distance(startPosition, baseAimPosition);
            float remainingDistance = Vector3.Distance(transform.localPosition, baseAimPosition);

            float distanceClamp = 1f - Mathf.Clamp01(
                remainingDistance / Mathf.Max(totalAimDistance, 0.0001f)
            );

            aimProgress = Mathf.SmoothStep(0f, 1f, distanceClamp);
            aimProgress = Mathf.Pow(aimProgress, 10f);

            Vector3 currentAimOffset = transform.localRotation * aimPoint.localPosition;

            Vector3 startAimOffset = startRotation * aimPoint.localPosition;

            Vector3 rotationOffset = currentAimOffset - startAimOffset;

            targetPosition = baseAimPosition;
            targetPosition.y -= rotationOffset.y * aimProgress;
            targetPosition.z -= rotationOffset.z * aimProgress;
        }
        else
        {
            aimProgress = 0f;
        }

        distanceToAim = Vector3.Distance(transform.localPosition, targetPosition);

        if (distanceToAim <= 0.0001f)
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
            Quaternion.Euler(swayX, swayY, 0f);

        if (!Player.instance.isReloading)
            targetRotation *= Quaternion.Euler(moveRotationOffset);

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
        float swayZ = Mathf.Clamp(lookInput.y * swayAmount, -0.5f, 0.5f);

        Quaternion swayRotation = Quaternion.Euler(-swayX, swayY, swayZ);
        Quaternion targetRotation = startRotation * swayRotation;

        float smooth = aimProgress > 0.9 ? 3f : aimSmooth;

        Quaternion newRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * smooth
        );
        transform.localRotation = newRotation;
    }
}