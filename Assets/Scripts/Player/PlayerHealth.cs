using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 8;
    [SerializeField] private float knockBackThrustAmount = 6f;
    [SerializeField] private float damageRecoveryTime = 1f;
    [SerializeField] private int regenAmount = 2; // Health regained every interval
    [SerializeField] private float minRegenInterval = 5f; // Minimum regen interval
    [SerializeField] private float maxRegenInterval = 20f; // Maximum regen interval

    private int currentHealth;
    private bool canTakeDamage = true;
    private Knockback knockback;
    private Flash flash;

    public Image healthBar;

    private int damageCount = 0; // Tracks how often the player takes damage
    private float regenInterval; // Current regen interval

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        regenInterval = minRegenInterval; // Start with the minimum regen time
        StartCoroutine(HealthRegenRoutine());
        StartCoroutine(DamageCheckRoutine()); // Routine to adjust regen interval
    }

    void Update()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = Mathf.Clamp((float)currentHealth / maxHealth, 0f, 1f);
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        EnemyAI enemy = other.gameObject.GetComponent<EnemyAI>();

        if (enemy)
        {
            int damage = enemy.GetDamageAmount();
            TakeDamage(damage, other.transform);
        }
    }

    public void TakeDamage(int damageAmount, Transform hitTransform)
    {
        if (!canTakeDamage) { return; }

        knockback.GetKnockedBack(hitTransform, knockBackThrustAmount);
        StartCoroutine(flash.FlashRoutine());
        canTakeDamage = false;
        currentHealth -= damageAmount;
        damageCount++; // Increment damage count whenever damage is taken

        Debug.Log($"Player took {damageAmount} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(DamageRecoveryRoutine());
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        canTakeDamage = true;
    }

    private void Die()
    {
        Debug.Log("Player died!");

        Destroy(gameObject);  // Remove player from scene
        healthBar.fillAmount = 0f;
    }

    private IEnumerator HealthRegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);

            if (currentHealth > 0 && currentHealth < maxHealth)
            {
                currentHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
                Debug.Log($"Player regenerated health. Current health: {currentHealth}");
            }
        }
    }

    private IEnumerator DamageCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f); // Check damage frequency every 15 seconds

            if (damageCount < 2) // Player took little damage in the last 10 seconds
            {
                regenInterval = Mathf.Min(regenInterval + 2f, maxRegenInterval); // Increase interval
            }
            else // Player took frequent damage
            {
                regenInterval = Mathf.Max(regenInterval - 3f, minRegenInterval); // Decrease interval
            }

            Debug.Log($"Adjusted regen interval: {regenInterval}s");
            damageCount = 0; // Reset the damage count for the next check
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
