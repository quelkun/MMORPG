using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class NoiseEmitter : MonoBehaviour
{
    public float runNoiseRadius = 8f;
    public float jumpNoiseRadius = 10f;
    public float attackNoiseRadius = 12f;

    public void EmitNoise(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            Perception perception = hit.GetComponent<Perception>();
            if (perception != null)
            {
                perception.OnNoiseHeard(transform.position, radius);
            }
        }
    }
}
}
