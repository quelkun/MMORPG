using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

    [RequireComponent(typeof(Animator), typeof(AIController))]
    public class EnemyAttack : MonoBehaviour
    {
        private Transform player;
        private Animator animator;
        private AIController ai;
        private float cooldownTimer = 0f;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            ai = GetComponent<AIController>();
            player = ai.player;
        }

        private void Update()
        {
            if (ai == null || player == null) return;

            if (ai.config.mode != EnemyAIConfigSO.BehaviorMode.Hostile) return;

            cooldownTimer -= Time.deltaTime;

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= ai.config.attackRange && cooldownTimer <= 0f)
            {
                PerformAttack();
                cooldownTimer = ai.config.attackCooldown;
            }
        }

        private void PerformAttack()
        {
            var attacks = ai.config.attacks;
            if (attacks == null || attacks.Length == 0) return;

            var attack = attacks[Random.Range(0, attacks.Length)];
            if (!string.IsNullOrEmpty(attack.animationTrigger))
                animator.SetTrigger(attack.animationTrigger);

            Collider[] hits = Physics.OverlapSphere(transform.position, ai.config.attackRange);
            foreach (var hit in hits)
            {
                var hitbox = hit.GetComponent<PlayerHitbox>();
                if (hitbox != null)
                {
                    hitbox.ReceiveHit(attack.damage, transform);
                    break; // Only hit one hitbox per attack
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (ai == null || ai.config == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ai.config.attackRange);
        }
    }
}