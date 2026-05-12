using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class PlayerHitbox : MonoBehaviour
{
    public enum HitboxType { Head, Chest, Arm, Leg, Other }
    public HitboxType hitboxType = HitboxType.Other;
    public int damageMultiplier = 1;

    [HideInInspector] public PlayerDamageReceiver receiver;

    public void ReceiveHit(int baseDamage, Transform attacker = null)
    {
        int finalDamage = baseDamage * damageMultiplier;
        receiver.TakeDamage(finalDamage, attacker);
        Debug.Log($"[PlayerHitbox] {hitboxType} hit: {baseDamage} x{damageMultiplier} = {finalDamage}");
    }
}
}
