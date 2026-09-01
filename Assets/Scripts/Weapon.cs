using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private float hipFireAmount = 3f;

    [Header("Recoil")]
    [SerializeField] private AnimationCurve verticalRecoil;
    [SerializeField] private AnimationCurve horizontalRecoil;

    [SerializeField] private float verticalMultiplier = 1f;
    [SerializeField] private float horizontalMultiplier = 1f;

    private GameObject[] bulletPool;
    private int currentBulletIndex;
    private Coroutine fireRoutine;
    private VisualElement container;

    private float previousVertical;
    private float previousHorizontal;
    private float curveTime = 0.1f;
    private Coroutine resetRecoilRoutine;
    private float nextFireTime;

    private void Awake()
    {
        bulletPool = new GameObject[magazineSize];

        for (int i = 0; i < magazineSize; i++)
        {
            bulletPool[i] = Instantiate(bullet, transform);
            bulletPool[i].SetActive(false);
        }

        VisualElement root = crosshair.rootVisualElement;
        container = root.Q<VisualElement>("Container");

    }

    void Update()
    {
        container.style.width = hipFireAmount * 15f;
        container.style.height = hipFireAmount * 15f;
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

        if (resetRecoilRoutine != null)
            StopCoroutine(resetRecoilRoutine);

        resetRecoilRoutine = StartCoroutine(ResetRecoilDelay());
    }

    private IEnumerator ResetRecoilDelay()
    {
        float fireInterval = 60f / fireRate;

        yield return new WaitForSeconds(fireInterval);

        previousVertical = 0f;
        previousHorizontal = 0f;
        curveTime = 0.1f;

        resetRecoilRoutine = null;
    }

    private IEnumerator AutoFire()
    {
        float fireInterval = 60f / fireRate;

        while (true)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                ApplyRecoil();
                nextFireTime = Time.time + fireInterval;
            }

            yield return null;
        }
    }

    private void ApplyRecoil()
    {
        float verticalValue = verticalRecoil.Evaluate(curveTime);
        verticalValue *= verticalMultiplier;

        float horizontalValue = horizontalRecoil.Evaluate(curveTime);
        horizontalValue *= horizontalMultiplier;

        curveTime += 0.1f;

        float verticalDelta = verticalValue - previousVertical;
        float horizontalDelta = previousHorizontal - horizontalValue;

        previousVertical = verticalValue;
        previousHorizontal = horizontalValue;

        Player.instance.ApplyCameraPitch(verticalDelta);
        Player.instance.transform.Rotate(Vector3.up * horizontalDelta);
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
            float spreadAmount = hipFireAmount / 100;
            float randomRight = Random.Range(-spreadAmount, spreadAmount);
            float randomUp = Random.Range(-spreadAmount, spreadAmount);
            Vector3 randomOffset = camera.right * randomRight + camera.up * randomUp;

            rayDirection = (camera.forward + randomOffset).normalized;
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