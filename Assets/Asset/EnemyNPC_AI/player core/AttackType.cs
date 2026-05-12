using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

[System.Serializable]
public class AttackType
{
    public string attackName;
    public KeyCode attackKey = KeyCode.Mouse0;
    public int damage = 10;
    public float range = 2f;
    public float cooldown = 1f;
    public LayerMask hitMask;
}
}
