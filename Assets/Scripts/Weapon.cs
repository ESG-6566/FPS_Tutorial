using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] public float onAimWalkWeight = 0.1f;
    [Range(0f, 1f)]
    [SerializeField] public float onAimFiringWeight = 0.2f;
    [SerializeField] public UIDocument crosshair;

    [SerializeField] private Transform reticle;
    [SerializeField] private WeaponSway weaponSway;
    [SerializeField] private Transform barrelEnd;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed = 150f;
    [SerializeField] private float fireRate = 360f;
    [SerializeField] private int magazineSize = 50;

    private GameObject[] bulletPool;
    private int currentBulletIndex;
    private Coroutine fireRoutine;

    private void Awake()
    {
        bulletPool = new GameObject[magazineSize];

        for (int i = 0; i < magazineSize; i++)
        {
            bulletPool[i] = Instantiate(bullet, transform);
            bulletPool[i].SetActive(false);
        }
    }

    public void StartFire()
    {
        if (fireRoutine == null)
            fireRoutine = StartCoroutine(AutoFire());
    }

    public void StopFire()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }
    }

    private IEnumerator AutoFire()
    {
        float fireInterval = 60f / fireRate;
        while (true)
        {
            Shoot();

            yield return new WaitForSeconds(fireInterval);
        }
    }

    private GameObject GetBulletFromPool()
    {
        GameObject bullet = bulletPool[currentBulletIndex];

        currentBulletIndex++;

        if (currentBulletIndex >= bulletPool.Length)
            currentBulletIndex = 0;

        bullet.transform.parent = null;
        bullet.SetActive(true);
        bullet.GetComponent<SphereCollider>().enabled = true;
        bullet.transform.position = barrelEnd.position;

        return bullet;
    }

    private void Shoot()
    {
        Transform camera = Camera.main.transform;

        Vector3 rayDirection;
        if (Player.instance.isAiming && weaponSway.distanceToAim <= 0.001f)
        {
            rayDirection = (reticle.position - camera.position).normalized;
        }
        else
        {
            rayDirection = camera.forward;
        }


        Ray ray = new(camera.position, rayDirection);
        Vector3 targetPoint;
        Vector3 direction;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
            direction = targetPoint - barrelEnd.position;
        }
        else
        {
            direction = camera.forward;
        }

        GameObject bulletToShoot = GetBulletFromPool();

        Rigidbody rb = bulletToShoot.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * bulletSpeed;
        }
    }
}