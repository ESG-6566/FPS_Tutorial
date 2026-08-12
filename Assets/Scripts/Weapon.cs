using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Range(0f,1f)]
    [SerializeField] public float onAimWalkWeight = 0.1f;
    [Range(0f,1f)]
    [SerializeField] public float onAimFiringWeight = 0.2f;
}