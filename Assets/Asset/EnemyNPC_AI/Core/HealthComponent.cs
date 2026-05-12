using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class HealthComponent : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public bool IsDead => currentHealth <= 0f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        Debug.Log($"[Health] {gameObject.name} took {amount} damage. Remaining: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"☠️ {gameObject.name} has died.");
        // Optionally trigger death animation, disable components, etc.
        Destroy(gameObject, 2f); // Or handle through AIController death state
    }
}
}
