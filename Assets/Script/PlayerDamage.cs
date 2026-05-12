using UnityEngine;
using PurrNet;
using System.Collections;
public class PlayerDamage : NetworkBehaviour
{
    [Header("Dégâts colision")]
    public int damageAmountColision = 10;
    public float attackCooldownColision = 1f;

    private bool canAttackColision = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;
        if (collision.gameObject.CompareTag("enemy") && canAttackColision)
        {
            EnemyStats enemyStats = collision.gameObject.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damageAmountColision);
                StartCoroutine(AttackCooldown());
            }
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttackColision = false;
        yield return new WaitForSeconds(attackCooldownColision);
        canAttackColision = true;
    }
}
