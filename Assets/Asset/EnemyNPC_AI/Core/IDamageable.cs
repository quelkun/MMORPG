using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

    public interface IDamageable
    {
        void TakeDamage(int amount, Transform attacker = null);
    }
}