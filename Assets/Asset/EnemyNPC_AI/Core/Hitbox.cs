using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    public enum HitboxType
    {
        Head,
        Chest,
        Arm,
        Leg,
        Other
    }

    [Header("Hitbox Info")]
    public HitboxType hitboxType = HitboxType.Other;
    public int damageMultiplier = 1;

    [Header("Auto Assigned")]
    public EnemyDamageReceiver receiver;

    private void Reset()
    {
        // Auto assign trigger collider and receiver
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (receiver == null)
            receiver = GetComponentInParent<EnemyDamageReceiver>();

        gameObject.layer = LayerMask.NameToLayer("EnemyHitbox");
    }

    public void ReceiveHit(int baseDamage, Transform attacker = null)
    {
        if (receiver != null)
        {
            int finalDamage = baseDamage * damageMultiplier;
            receiver.TakeDamage(finalDamage, attacker);
            Debug.Log($"[Hitbox] {hitboxType} hit: {baseDamage} x {damageMultiplier} = {finalDamage}");
        }
        else
        {
            Debug.LogWarning($"[Hitbox] Receiver not assigned on {gameObject.name}");
        }
    }
}
}
