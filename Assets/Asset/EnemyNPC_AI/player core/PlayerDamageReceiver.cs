using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDamageReceiver : MonoBehaviour, IDamageable
{
    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    public void TakeDamage(int amount, Transform attacker = null)
    {
        health.TakeDamage(amount);
        Debug.Log($"{gameObject.name} took {amount} damage from {attacker?.name ?? "unknown"}");
    }
}
}