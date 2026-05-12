using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

    [CreateAssetMenu(fileName = "EnemyAIConfig", menuName = "EnemyAI/AI Config")]
    public class EnemyAIConfigSO : ScriptableObject
    {
        public enum BehaviorMode
        {
            Passive,
            Hostile,
            Friendly,  // Acts like a companion player
            Follow,    // Follows the player (for quests/events)
            Idle,      // Static or wandering with no player reaction
            Flee       // Escapes to flee points
        }

        [Header("Behavior")]
        public BehaviorMode mode = BehaviorMode.Hostile;

        public enum PatrolMode
        {
            Loop,
            PingPong,
            Random
        }

        [Header("Patrol Settings")]
        public PatrolMode patrolMode = PatrolMode.Loop;
        public float waitTimeAtWaypoint = 2f;

        [Header("Movement Settings")]
        public float moveSpeed = 3.5f;
        public float chaseSpeed = 6f;
        public float angularSpeed = 120f;
        public float acceleration = 8f;

        [Header("Follow Settings")]
        public float stopFollowDistance = 2f;

        [Header("Detection Settings")]
        public float viewRadius = 20f;
        public float viewAngle = 120f;

        [Header("Search Mode Settings")]
        public float searchViewRadius = 50f;
        public float searchViewAngle = 150f;
        public float searchDuration = 30f;

        [Header("Alert Mode Settings")]
        public float alertViewRadius = 35f;
        public float alertDuration = 20f;

        [Header("Combat Settings")]
        public int maxHealth = 100;
        public float attackRange = 2f;
        public float attackCooldown = 2f;

        [System.Serializable]
        public class EnemyAttackData
        {
            public string attackName = "Attack";
            public int damage = 10;
            public string animationTrigger = "Attack";
        }

        [Header("Attacks")]
        public EnemyAttackData[] attacks;

        [Header("Respawn Settings")]
        public bool isRespawnable;
        public float respawnDelay = 5f;
        public GameObject prefab;
    }
}