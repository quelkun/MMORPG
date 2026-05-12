using UnityEngine;
using UnityEngine.Events;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public UnityEvent OnDeath;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        UIHealthBar.Instance?.SetHealth(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath.Invoke();
            gameObject.SetActive(false);
        }
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log($"Player took {amount} damage. HP: {currentHealth}");

        if (currentHealth == 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UIHealthBar.Instance?.SetHealth(currentHealth, maxHealth);
    }
    private void Die()
    {
        Debug.Log("Player Died");
        // Add death animation, disable controls, respawn, etc.
        Destroy(gameObject);
    }
}
}