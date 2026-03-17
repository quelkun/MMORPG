//emyDamage.cs
using PurrNet;
using System.Collections;
using UnityEngine;

public class EnemyDamage : NetworkBehaviour
{
    [Header("Dégâts")]
    public int damageAmount = 10;
    public float attackCooldown = 1f;

    private bool canAttack = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;
        if (collision.gameObject.CompareTag("Player") && canAttack)
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damageAmount);
                StartCoroutine(AttackCooldown());
            }
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
