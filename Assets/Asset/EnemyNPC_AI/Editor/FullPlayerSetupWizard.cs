using UnityEditor;
using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

    public class FullPlayerSetupWizard : EditorWindow
    {
        GameObject playerObj;
        GameObject healthBarSlider;
        bool addStarterAssetsNote = true;

        [MenuItem("Tools/Enemy NPC AI/Full Player Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<FullPlayerSetupWizard>("Full Player Setup Wizard");
        }

        void OnGUI()
        {
            GUILayout.Label("Complete Player Setup", EditorStyles.boldLabel);

            playerObj = (GameObject)EditorGUILayout.ObjectField("Player GameObject", playerObj, typeof(GameObject), true);
            healthBarSlider = (GameObject)EditorGUILayout.ObjectField("Health Bar Slider (UI)", healthBarSlider, typeof(GameObject), true);

            if (GUILayout.Button("Run Full Player Setup"))
            {
                if (playerObj == null || healthBarSlider == null)
                {
                    EditorUtility.DisplayDialog("Missing Fields", "Please assign both Player GameObject and Health Bar Slider.", "OK");
                }
                else
                {
                    SetupPlayerCore();
                    SetupPlayerHitboxes();
                    Debug.Log("✅ Full Player setup complete!", playerObj);
                }
            }

            GUILayout.Space(10);
            addStarterAssetsNote = EditorGUILayout.Toggle("Recommend Starter Assets?", addStarterAssetsNote);
            if (addStarterAssetsNote)
                EditorGUILayout.HelpBox("✅ Recommended: Use Unity Starter Assets (Third Person Controller) for camera & animation setup.", MessageType.Info);
        }

        void SetupPlayerCore()
        {
            Undo.RecordObject(playerObj, "Player Setup");

            AddIfMissing<PlayerHealth>();
            AddIfMissing<PlayerAttack>();
            AddIfMissing<PlayerStealthController>();
            AddIfMissing<StealthEvaluator>();
            if (playerObj.GetComponent<StealthEvaluator>() == null)
                playerObj.AddComponent<StealthEvaluator>();

            if (playerObj.GetComponent<PlayerStealthController>() == null)
                playerObj.AddComponent<PlayerStealthController>();

            if (playerObj.GetComponent<PlayerAttack>() == null)
                playerObj.AddComponent<PlayerAttack>();

            var attack = playerObj.GetComponent<PlayerAttack>();
            attack.attackOrigin = playerObj.transform.Find("AttackOrigin") ?? CreateAttackOrigin(playerObj);
            if (attack.animator == null)
                attack.animator = playerObj.GetComponent<Animator>();
            attack.enemyHitboxLayer = LayerMask.GetMask("EnemyHitbox");

            var health = playerObj.GetComponent<PlayerHealth>();
            var ui = healthBarSlider.GetComponentInChildren<UnityEngine.UI.Slider>();
            var uiComp = playerObj.GetComponentInChildren<UIHealthBar>();
            if (!uiComp) uiComp = playerObj.AddComponent<UIHealthBar>();
            uiComp.GetType().GetField("slider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(uiComp, ui);

            EditorUtility.SetDirty(playerObj);
        }

        void SetupPlayerHitboxes()
        {
            if (playerObj.GetComponent<PlayerDamageReceiver>() == null)
                playerObj.AddComponent<PlayerDamageReceiver>();

            PlayerDamageReceiver receiver = playerObj.GetComponent<PlayerDamageReceiver>();

            CreateHitbox("Head", new Vector3(0, 1.8f, 0), new Vector3(0.3f, 0.3f, 0.3f), PlayerHitbox.HitboxType.Head, 2);
            CreateHitbox("Chest", new Vector3(0, 1.4f, 0), new Vector3(0.5f, 0.5f, 0.5f), PlayerHitbox.HitboxType.Chest, 1);
            CreateHitbox("LeftArm", new Vector3(-0.5f, 1.4f, 0), new Vector3(0.3f, 0.4f, 0.3f), PlayerHitbox.HitboxType.Arm, 1);
            CreateHitbox("RightArm", new Vector3(0.5f, 1.4f, 0), new Vector3(0.3f, 0.4f, 0.3f), PlayerHitbox.HitboxType.Arm, 1);
            CreateHitbox("Legs", new Vector3(0, 0.7f, 0), new Vector3(0.4f, 0.6f, 0.4f), PlayerHitbox.HitboxType.Leg, 1);

            void CreateHitbox(string name, Vector3 localPosition, Vector3 scale, PlayerHitbox.HitboxType type, int multiplier)
            {
                GameObject hitboxObj = new GameObject(name);
                hitboxObj.transform.parent = playerObj.transform;
                hitboxObj.transform.localPosition = localPosition;
                hitboxObj.transform.localRotation = Quaternion.identity;
                hitboxObj.transform.localScale = scale;

                BoxCollider col = hitboxObj.AddComponent<BoxCollider>();
                col.isTrigger = true;

                PlayerHitbox hitbox = hitboxObj.AddComponent<PlayerHitbox>();
                hitbox.hitboxType = type;
                hitbox.damageMultiplier = multiplier;
                hitbox.receiver = receiver;

                hitboxObj.layer = LayerMask.NameToLayer("PlayerHitbox");
            }
        }

        void AddIfMissing<T>() where T : Component
        {
            if (playerObj.GetComponent<T>() == null)
            {
                Undo.AddComponent<T>(playerObj);
                Debug.Log($"Added {typeof(T).Name}");
            }
        }

        Transform CreateAttackOrigin(GameObject parent)
        {
            var go = new GameObject("AttackOrigin");
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }
    }
}
