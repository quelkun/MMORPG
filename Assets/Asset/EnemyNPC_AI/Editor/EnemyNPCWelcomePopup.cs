using UnityEditor;
using UnityEngine;

namespace SuperHorizon.EnemyNPCai.Editor
{
    [InitializeOnLoad]
    public class EnemyNPCWelcomePopup : EditorWindow
    {
        private const string HasSeenPopupKey = "SuperHorizon_EnemyNPC_WelcomeSeen";

        static EnemyNPCWelcomePopup()
        {
            if (!EditorPrefs.GetBool(HasSeenPopupKey, false))
            {
                ShowWindow();
                EditorPrefs.SetBool(HasSeenPopupKey, true);
            }
        }

        [MenuItem("Tools/Enemy NPC AI/Help")]
        public static void ShowWindow()
        {
            var window = GetWindow<EnemyNPCWelcomePopup>(true, "Welcome to Enemy & NPC AI!", true);
            window.position = new Rect(200, 200, 450, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("🎮 Enemy & NPC AI System", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.HelpBox("Thank you for downloading Enemy & NPC AI by SuperHorizon Studios!\n\nThis system enables advanced AI, stealth, and FPS/TPP combat behavior for any Unity project.", MessageType.Info);

            GUILayout.Space(10);
            if (GUILayout.Button("🛠️  Open Full Setup Wizard", GUILayout.Height(30)))
            {
                FullEnemySetupWizard.ShowWindow(); // Replace with your setup wizard
                Close();
            }

            if (GUILayout.Button("📖  View Online Documentation", GUILayout.Height(25)))
            {
                Application.OpenURL("https://docs.google.com/document/d/1W_-sd7xaeMhBldFLFThUUmAZo_f2weTKE2oNuFEx3R8/edit?usp=sharing"); // Replace with your docs URL
                Close();
            }

            GUILayout.Space(15);
            GUILayout.Label("📦 Required Dependencies", EditorStyles.boldLabel);

            if (GUILayout.Button("▶️  Install Third Person Starter Asset", GUILayout.Height(25)))
                Application.OpenURL("https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526");

            if (GUILayout.Button("▶️  Install First Person Starter Asset", GUILayout.Height(25)))
                Application.OpenURL("https://assetstore.unity.com/packages/essentials/starter-assets-firstperson-updates-in-new-charactercontroller-pa-196525");

            GUILayout.FlexibleSpace();
            GUILayout.Space(10);
            GUILayout.Label("© 2025 SuperHorizon Studios", EditorStyles.centeredGreyMiniLabel);
        }
    }
}
