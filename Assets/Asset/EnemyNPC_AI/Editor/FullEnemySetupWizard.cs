using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

    public class FullEnemySetupWizard : EditorWindow
    {
        private GameObject enemyObject;
        private EnemyAIConfigSO configSO;
        private Transform patrolPath;
        private Transform playerTransform;

        [MenuItem("Tools/Enemy NPC AI/Full Enemy Setup")]
        public static void ShowWindow()
        {
            GetWindow<FullEnemySetupWizard>("Full Enemy Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Complete Enemy AI Setup", EditorStyles.boldLabel);

            enemyObject = (GameObject)EditorGUILayout.ObjectField("Enemy GameObject", enemyObject, typeof(GameObject), true);
            configSO = (EnemyAIConfigSO)EditorGUILayout.ObjectField("AI Config (SO)", configSO, typeof(EnemyAIConfigSO), false);
            patrolPath = (Transform)EditorGUILayout.ObjectField("Patrol Path", patrolPath, typeof(Transform), true);
            playerTransform = (Transform)EditorGUILayout.ObjectField("Player Transform", playerTransform, typeof(Transform), true);

            EditorGUILayout.Space();

            if (enemyObject == null)
            {
                EditorGUILayout.HelpBox("Assign an enemy GameObject to configure.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Run Full Setup"))
            {
                SetupAIComponents();
                SetupHitboxes();
            }

            if (GUILayout.Button("Create Default AI Config SO"))
            {
                CreateDefaultConfig();
            }
        }

        private void SetupAIComponents()
        {
            Undo.RegisterCompleteObjectUndo(enemyObject, "Enemy AI Setup");

            EnsureComponent<NavMeshAgent>();
            EnsureComponent<Animator>();
            EnsureComponent<AIController>();
            EnsureComponent<Perception>();
            EnsureComponent<EnemyHealth>();
            EnsureComponent<EnemyAttack>();
            EnsureComponent<EnemyDamageReceiver>();

            var controller = enemyObject.GetComponent<AIController>();
            var perception = enemyObject.GetComponent<Perception>();

            if (controller != null)
            {
                controller.config = configSO;
                controller.perception = perception;

                if (patrolPath && patrolPath.GetComponent<PatrolPath>() != null)
                    controller.patrolPath = patrolPath.GetComponent<PatrolPath>();

                if (playerTransform != null)
                    controller.player = playerTransform;
            }

            Debug.Log($"✅ AI setup completed for: {enemyObject.name}", enemyObject);
        }

        private void SetupHitboxes()
        {
            var receiver = enemyObject.GetComponent<EnemyDamageReceiver>();
            if (!receiver)
            {
                Debug.LogError("❌ Enemy must have EnemyDamageReceiver component.");
                return;
            }

            CreateHitbox("Head", new Vector3(0, 1.8f, 0), new Vector3(0.3f, 0.3f, 0.3f), Hitbox.HitboxType.Head, 2);
            CreateHitbox("Chest", new Vector3(0, 1.3f, 0), new Vector3(0.6f, 0.6f, 0.5f), Hitbox.HitboxType.Chest, 1);
            CreateHitbox("Arm_L", new Vector3(-0.4f, 1.3f, 0), new Vector3(0.3f, 0.4f, 0.3f), Hitbox.HitboxType.Arm, 1);
            CreateHitbox("Arm_R", new Vector3(0.4f, 1.3f, 0), new Vector3(0.3f, 0.4f, 0.3f), Hitbox.HitboxType.Arm, 1);
            CreateHitbox("Leg_L", new Vector3(-0.3f, 0.6f, 0), new Vector3(0.4f, 0.6f, 0.4f), Hitbox.HitboxType.Leg, 1);
            CreateHitbox("Leg_R", new Vector3(0.3f, 0.6f, 0), new Vector3(0.4f, 0.6f, 0.4f), Hitbox.HitboxType.Leg, 1);

            Debug.Log("✅ Enemy hitboxes created and configured.", enemyObject);
        }

        private void CreateHitbox(string name, Vector3 localPos, Vector3 scale, Hitbox.HitboxType type, int multiplier)
        {
            GameObject hitboxObj = new GameObject(name);
            hitboxObj.transform.parent = enemyObject.transform;
            hitboxObj.transform.localPosition = localPos;
            hitboxObj.transform.localRotation = Quaternion.identity;
            hitboxObj.transform.localScale = scale;

            BoxCollider col = hitboxObj.AddComponent<BoxCollider>();
            col.isTrigger = true;

            Hitbox hitbox = hitboxObj.AddComponent<Hitbox>();
            hitbox.hitboxType = type;
            hitbox.damageMultiplier = multiplier;
            hitbox.receiver = enemyObject.GetComponent<EnemyDamageReceiver>();

            hitboxObj.layer = LayerMask.NameToLayer("EnemyHitbox");
        }

        private void CreateDefaultConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save AI Config", "EnemyAIConfig", "asset", "Choose location to save the AI config.");
            if (!string.IsNullOrEmpty(path))
            {
                var newConfig = ScriptableObject.CreateInstance<EnemyAIConfigSO>();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                configSO = newConfig;
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newConfig;
                Debug.Log("✅ Created default EnemyAIConfigSO.");
            }
        }

        private void EnsureComponent<T>() where T : Component
        {
            if (enemyObject.GetComponent<T>() == null)
            {
                enemyObject.AddComponent<T>();
                Debug.Log($"➕ Added missing component: {typeof(T).Name}");
            }
        }
    }
}