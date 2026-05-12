using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class EnemyHealth : MonoBehaviour
{
    private int currentHealth;
    private AIController ai;

    public GameObject healthBarPrefab;
    private GameObject healthBarInstance;
    private EnemyHealthBar healthBar;

    [Header("Death & Respawn")]
    public GameObject modelToDisable;
    public Animator animator;
    public float respawnDelay = 5f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isDead = false;

    private void Awake()
    {
        ai = GetComponent<AIController>();
        currentHealth = ai != null ? ai.config.maxHealth : 100;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 2.2f, 0), Quaternion.identity);
            healthBar = healthBarInstance.GetComponent<EnemyHealthBar>();

            if (healthBar != null)
            {
                healthBar.target = transform;
                healthBar.SetHealth(CurrentHealthNormalized());
            }
        }
    }

    /// <summary>
    /// Apply raw damage to this enemy (from hitboxes or projectiles).
    /// </summary>
    public void ApplyDamage(int amount)
    {
        TakeDamage(amount);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{gameObject.name} took {amount} damage. Remaining HP: {currentHealth}");

        if (healthBar != null)
            healthBar.SetHealth(CurrentHealthNormalized());

        if (ai != null)
            ai.OnHit();

        if (currentHealth <= 0)
            Die();
    }

    public float CurrentHealthNormalized()
    {
        if (ai != null && ai.config != null && ai.config.maxHealth > 0)
            return (float)currentHealth / ai.config.maxHealth;
        return 0f;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} died.");

        if (animator)
            animator.SetTrigger("Die");

        if (ai != null)
        {
            ai.SetState(AIController.AIState.Idle);
            var agent = ai.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent) agent.isStopped = true;
        }

        if (modelToDisable)
            modelToDisable.SetActive(false);

        if (healthBarInstance)
            Destroy(healthBarInstance);

        if (ai != null && ai.config != null && ai.config.isRespawnable)
        {
            if (EnemyRespawner.Instance != null)
                EnemyRespawner.Instance.StartRespawn(ai.config, transform);
            else
                Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    private void Respawn()
    {
        isDead = false;
        currentHealth = ai != null ? ai.config.maxHealth : 100;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (modelToDisable)
            modelToDisable.SetActive(true);

        if (animator)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        ai?.ResetToDefaultState();

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 2.2f, 0), Quaternion.identity);
            healthBar = healthBarInstance.GetComponent<EnemyHealthBar>();

            if (healthBar != null)
            {
                healthBar.target = transform;
                healthBar.SetHealth(1f);
            }
        }
    }
}
}