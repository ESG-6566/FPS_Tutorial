using UnityEngine;

public class Body : MonoBehaviour
{
    [SerializeField] public Weapon FPWeapon;
    [SerializeField] public Transform TPWeaponPivot;
    [SerializeField] public Transform TPWeaponRig;

    private Animator animator;
    private Player player;
    private Transform neck;
    private float currentAngleClamp = 45f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        player = Player.instance;
        neck = animator.GetBoneTransform(HumanBodyBones.Neck);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveValue = player.moveInput;
        animator.SetFloat("velocityX", moveValue.x, 0.2f, Time.deltaTime);
        animator.SetFloat("velocityY", moveValue.y, 0.2f, Time.deltaTime);

        animator.SetBool("isMoving", player.isMoving);
        animator.SetBool("isReloading", player.isReloading);
        animator.SetBool("isFiring", player.isFiring);

        transform.position = player.transform.position;

        float targetClamp = player.isMoving ? 0f : 45f;

        float smoothT = 1f - Mathf.Exp(-0.2f * Time.deltaTime);
        currentAngleClamp = Mathf.Lerp(
            currentAngleClamp,
            targetClamp,
            smoothT
        );

        float playerY = player.transform.eulerAngles.y;
        float bodyY = transform.eulerAngles.y;

        // Positive: player is right of body. Negative: player is left of body.
        float angleDifference = Mathf.DeltaAngle(bodyY, playerY);

        // Limit how far the body can face away from the player.
        float clampedDifference = Mathf.Clamp(
            angleDifference,
            -currentAngleClamp,
            currentAngleClamp
        );

        float targetBodyY = player.isMoving ? playerY : playerY - clampedDifference;

        if (!Mathf.Approximately(bodyY, targetBodyY))
        {
            Vector3 rotation = transform.eulerAngles;
            rotation.y = targetBodyY;
            transform.eulerAngles = rotation;
        }
    }

    void LateUpdate()
    {
        TPWeaponPivot.SetPositionAndRotation(
            neck.position,
            FPWeapon.transform.parent.rotation
        );

        TPWeaponRig.localRotation = FPWeapon.weaponSway.transform.localRotation;

        Vector3 targetPosition =
            player.isAiming && !player.isReloading
                ? FPWeapon.aimOffset
                : FPWeapon.nonAimOffset;

        float t = 1f - Mathf.Exp(-FPWeapon.weaponSway.aimSmooth * Time.deltaTime);

        TPWeaponRig.localPosition = Vector3.Lerp(
            TPWeaponRig.localPosition,
            targetPosition,
            t
        );

        
    }
}