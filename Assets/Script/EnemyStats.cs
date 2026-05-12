using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public int Enemy_health = 100;
    public int Enemy_maxHealth = 100;

    public void TakeDamage(int damage)
    {
        Enemy_health -= damage;
        if (Enemy_health <= 0)
        {
            EnnemyDie();
        }
    }

    private void EnnemyDie()
    {
        Destroy(gameObject);
    }
}
