using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    [SerializeField] public UIDocument healthBar;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private VisualElement healthLine;
    private Animator animator;
    private bool isDeath = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        VisualElement root = healthBar.rootVisualElement;
        healthLine = root.Q<VisualElement>("HealthLine");
    }

    private void LateUpdate()
    {
        healthBar.transform.parent.LookAt(Camera.main.transform);
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
        else if(!isDeath)
        {
            float healthPercent = 100 * currentHealth / maxHealth;
            healthLine.style.width = Length.Percent(healthPercent);

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(1);
            if (!state.IsName("Hit"))
                animator.CrossFade("Hit", 0.1f);
        }
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