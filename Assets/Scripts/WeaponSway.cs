using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] private float swayAmount = 2f;
    [SerializeField] private float swaySmooth = 10f;
    [SerializeField] private float maxSwayAngle = 5f;

    [Header("Move Offset")]
    [SerializeField] private Vector3 leftMoveRotationOffset = new Vector3(50f, 5f, 0f);
    [SerializeField] private Vector3 rightMoveRotationOffset = new Vector3(-30f, 5f, 0f);
    [SerializeField] private Vector3 backMoveRotationOffset = new Vector3(30f, -10f, -10f);
    [SerializeField] private float moveOffsetSmooth = 5f;

    private Quaternion startRotation;

    private void Awake()
    {
        startRotation = transform.localRotation;
    }

    public void ApplySway(Vector2 lookInput, Vector2 moveInput)
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
}