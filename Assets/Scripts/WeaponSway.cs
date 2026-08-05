using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] private float swayAmount = 2f;
    [SerializeField] private float swaySmooth = 10f;
    [SerializeField] private float maxSwayAngle = 5f;

    private Quaternion startRotation;

    private void Awake()
    {
        startRotation = transform.localRotation;
    }

    public void ApplySway(Vector2 lookInput)
    {
        float swayX = Mathf.Clamp(-lookInput.y * swayAmount, -maxSwayAngle, maxSwayAngle);
        float swayY = Mathf.Clamp(lookInput.x * swayAmount, -maxSwayAngle, maxSwayAngle);

        Quaternion targetRotation = startRotation * Quaternion.Euler(swayX, swayY, 0f);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * swaySmooth
        );
    }
}