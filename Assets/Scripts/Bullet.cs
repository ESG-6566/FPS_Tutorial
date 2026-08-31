using UnityEngine;
using UnityEngine.VFX;

public class Bullet : MonoBehaviour
{
    [SerializeField] private VisualEffect hitEffect;
    private Rigidbody rb;
    private SphereCollider sphereCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
            transform.position = contact.point;
        }

        if (sphereCollider != null)
        {
            sphereCollider.enabled = false;
        }

        if (hitEffect)
            hitEffect.Play();
    }
}