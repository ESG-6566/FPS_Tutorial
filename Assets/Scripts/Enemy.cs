using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    [SerializeField] public UIDocument healthBar;
    [SerializeField] public float healthBarScale = 2f;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private VisualElement healthLine;
    private Label damageCounter;
    private Animator animator;
    private bool isDeath = false;
    private float counterTime;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        VisualElement root = healthBar.rootVisualElement;
        healthLine = root.Q<VisualElement>("HealthLine");

        root = healthBar.rootVisualElement;
        damageCounter = root.Q<Label>("DamageCounter");
        damageCounter.text = "";
    }

    private void LateUpdate()
    {
        healthBar.transform.parent.LookAt(Camera.main.transform);

        Transform cam = Camera.main.transform;
        float distance = Vector3.Distance(transform.position, cam.position);

        healthBar.transform.localScale = Vector3.one * distance * (healthBarScale / 10f);
        healthBar.transform.localPosition = Vector3.up * distance * (healthBarScale / 50f);

        if (counterTime > 0f)
        {
            counterTime -= Time.deltaTime;

            if (counterTime <= 0f)
                damageCounter.text = "";
        }
    }

    public void Hit(float hitAmount)
    {
        currentHealth -= hitAmount;
        if (currentHealth <= 0 && !isDeath)
        {
            healthLine.style.width = Length.Percent(0f);
            animator.CrossFade("Death", 0.3f, 0);
            StartCoroutine(Reset());
            isDeath = true;
        }
        else if (!isDeath)
        {
            float healthPercent = 100 * currentHealth / maxHealth;
            healthLine.style.width = Length.Percent(healthPercent);

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(1);
            if (!state.IsName("Hit"))
                animator.CrossFade("Hit", 0.1f);
        }

        if (damageCounter.text == "")
            damageCounter.text = "0";

        float totalDamage = float.Parse(damageCounter.text) + hitAmount;
        damageCounter.text = totalDamage.ToString();

        counterTime = 1.5f;
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(3f);

        currentHealth = maxHealth;
        healthLine.style.width = Length.Percent(100f);
        animator.CrossFade("Edle", 0f, 0);
        isDeath = false;
    }
}