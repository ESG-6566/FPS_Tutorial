using UnityEngine;

public class WeaponHandIK : MonoBehaviour
{
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [SerializeField, Range(0f, 1f)] private float leftHandWeight = 1f;
    [SerializeField, Range(0f, 1f)] private float rightHandWeight = 1f;
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;

        ApplyHandIK(
            AvatarIKGoal.LeftHand,
            leftHandTarget,
            leftHandWeight
        );

        ApplyHandIK(
            AvatarIKGoal.RightHand,
            rightHandTarget,
            rightHandWeight
        );
    }

    private void ApplyHandIK(AvatarIKGoal hand, Transform target, float weight)
    {
        if (target == null)
            return;

        animator.SetIKPositionWeight(hand, weight);
        animator.SetIKRotationWeight(hand, weight);

        animator.SetIKPosition(hand, target.position);
        animator.SetIKRotation(hand, target.rotation);
    }
}