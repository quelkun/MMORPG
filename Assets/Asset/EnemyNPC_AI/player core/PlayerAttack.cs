using System.Collections.Generic;
using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class PlayerAttack : MonoBehaviour
{
    [System.Serializable]
    public class AttackData
    {
        public string name = "Punch";
        public int baseDamage = 10;
        public float range = 2f;
        public Vector3 boxSize = new Vector3(0.5f, 0.5f, 0.5f);
        public float cooldown = 1f;
        public KeyCode inputKey = KeyCode.Mouse0;
        public string animationTrigger = "Attack";
    }

    [Header("Attack Settings")]
    public List<AttackData> attacks = new List<AttackData>();

    [Header("References")]
    public Transform attackOrigin;       // Empty GameObject in front of the player (e.g., hand or chest)
    public LayerMask enemyHitboxLayer;
    public Animator animator;

    private float[] cooldownTimers;

    private void Start()
    {
        cooldownTimers = new float[attacks.Count];
    }

    private void Update()
    {
        for (int i = 0; i < attacks.Count; i++)
        {
            cooldownTimers[i] -= Time.deltaTime;

            if (Input.GetKeyDown(attacks[i].inputKey) && cooldownTimers[i] <= 0f)
            {
                PerformAttack(attacks[i]);
                cooldownTimers[i] = attacks[i].cooldown;
            }
        }
    }

    private void PerformAttack(AttackData attack)
    {
        if (animator && !string.IsNullOrEmpty(attack.animationTrigger))
            animator.SetTrigger(attack.animationTrigger);

        Vector3 center = attackOrigin.position + attackOrigin.forward * attack.range;
        Quaternion rotation = attackOrigin.rotation;

        Collider[] hits = Physics.OverlapBox(center, attack.boxSize * 0.5f, rotation, enemyHitboxLayer, QueryTriggerInteraction.Collide);

        Debug.Log($"[PlayerAttack] {attack.name} hit {hits.Length} target(s)");

        foreach (var hit in hits)
        {
            Hitbox hitbox = hit.GetComponent<Hitbox>();
            if (hitbox != null)
            {
                hitbox.ReceiveHit(attack.baseDamage, transform);
                break; // Only damage one hitbox per attack
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null || attacks == null) return;

        Gizmos.color = Color.red;

        foreach (var attack in attacks)
        {
            Vector3 center = attackOrigin.position + attackOrigin.forward * attack.range;
            Gizmos.matrix = Matrix4x4.TRS(center, attackOrigin.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, attack.boxSize);
        }
    }
}
}
