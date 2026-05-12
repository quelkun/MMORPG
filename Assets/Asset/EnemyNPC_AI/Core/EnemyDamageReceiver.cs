using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDamageReceiver : MonoBehaviour, IDamageable
{
    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        Hitbox[] hitboxes = GetComponentsInChildren<Hitbox>();
        foreach (var hitbox in hitboxes)
        {
            hitbox.receiver = this;
        }
    }

    public void TakeDamage(int amount, Transform attacker = null)
    {
        health.TakeDamage(amount);
        Debug.Log($"{gameObject.name} received {amount} damage from {attacker?.name ?? "unknown"}");
    }
}
}
