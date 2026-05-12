using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class DamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float interval = 1f;

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
            StartCoroutine(ApplyDamageOverTime(damageable, other.transform));
    }

    private void OnTriggerExit(Collider other)
    {
        StopAllCoroutines(); // Stop damage when enemy exits
    }

    private System.Collections.IEnumerator ApplyDamageOverTime(IDamageable target, Transform source)
    {
        while (true)
        {
            target.TakeDamage(damage, source);
            yield return new WaitForSeconds(interval);
        }
    }
}
}