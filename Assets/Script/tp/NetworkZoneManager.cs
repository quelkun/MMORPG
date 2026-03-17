using Invector.vCharacterController;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class NetworkZoneManager : NetworkBehaviour
{
    [Header("Zone Configuration")]
    [PurrScene] public string targetZoneName;

    [Header("Teleport Position")]
    public Vector3 teleportPosition = Vector3.zero;

    [Header("Network Optimization")]
    public float networkUpdateRadius = 50f;
    public bool showDebugGizmos = true;

    private static string currentPlayerZone;
    private static Dictionary<string, List<NetworkBehaviour>> zoneNetworkBehaviours = new Dictionary<string, List<NetworkBehaviour>>();
    private static Dictionary<string, bool> zoneNetworkActivity = new Dictionary<string, bool>();

    private void Start()
    {
        // Initialiser le dictionnaire pour cette zone si nécessaire
        if (!zoneNetworkBehaviours.ContainsKey(targetZoneName))
        {
            zoneNetworkBehaviours[targetZoneName] = new List<NetworkBehaviour>();
            zoneNetworkActivity[targetZoneName] = true; // Par défaut actif
        }

        // Enregistrer tous les NetworkBehaviours de cette scène
        RegisterSceneNetworkObjects();
    }

    private void RegisterSceneNetworkObjects()
    {
        Scene scene = gameObject.scene;
        NetworkBehaviour[] networkBehaviours = FindObjectsByType<NetworkBehaviour>(FindObjectsSortMode.None);

        foreach (NetworkBehaviour nb in networkBehaviours)
        {
            if (nb.gameObject.scene == scene)
            {
                // Vérifier que l'objet appartient à cette zone
                if (!zoneNetworkBehaviours[targetZoneName].Contains(nb))
                {
                    zoneNetworkBehaviours[targetZoneName].Add(nb);
                }
            }
        }

        Debug.Log($"Zone {targetZoneName}: {zoneNetworkBehaviours[targetZoneName].Count} objets réseau enregistrés");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && IsLocalPlayer(other.gameObject))
        {
            Debug.Log($"Joueur entre dans la zone: {targetZoneName}");
            StartCoroutine(SwitchPlayerZone(other.gameObject, targetZoneName));
        }
    }

    private IEnumerator SwitchPlayerZone(GameObject playerObject, string newZone)
    {
        string previousZone = currentPlayerZone;
        currentPlayerZone = newZone;

        // 1. Désactiver le contrôleur temporairement
        var controller = playerObject.GetComponent<vThirdPersonController>();
        if (controller != null) controller.enabled = false;

        // 2. Charger la nouvelle zone si nécessaire
        yield return StartCoroutine(LoadZoneIfNeeded(newZone));

        // 3. Déplacer le joueur
        Scene targetScene = SceneManager.GetSceneByName(newZone);
        SceneManager.MoveGameObjectToScene(playerObject, targetScene);
        playerObject.transform.position = teleportPosition;

        // 4. Mettre à jour l'activité réseau des zones
        UpdateNetworkZoneActivity(previousZone, newZone);

        // 5. Réactiver le contrôleur
        if (controller != null) controller.enabled = true;

        Debug.Log($"Joueur déplacé vers la zone: {newZone}");
    }

    private IEnumerator LoadZoneIfNeeded(string zoneName)
    {
        Scene zoneScene = SceneManager.GetSceneByName(zoneName);

        if (!zoneScene.IsValid() || !zoneScene.isLoaded)
        {
            Debug.Log($"Chargement de la zone: {zoneName}");
            var asyncLoad = SceneManager.LoadSceneAsync(zoneName, LoadSceneMode.Additive);
            yield return new WaitUntil(() => asyncLoad.isDone);

            // Enregistrer les objets réseau de la nouvelle zone
            yield return new WaitForSeconds(0.1f); // Laisser le temps aux objets de s'initialiser
            RegisterNewZoneNetworkObjects(zoneName);
        }
    }

    private void RegisterNewZoneNetworkObjects(string zoneName)
    {
        if (!zoneNetworkBehaviours.ContainsKey(zoneName))
        {
            zoneNetworkBehaviours[zoneName] = new List<NetworkBehaviour>();
        }

        Scene zoneScene = SceneManager.GetSceneByName(zoneName);
        NetworkBehaviour[] allNetworkBehaviours = FindObjectsByType<NetworkBehaviour>(FindObjectsSortMode.None);

        foreach (NetworkBehaviour nb in allNetworkBehaviours)
        {
            if (nb.gameObject.scene == zoneScene && !zoneNetworkBehaviours[zoneName].Contains(nb))
            {
                zoneNetworkBehaviours[zoneName].Add(nb);
            }
        }

        Debug.Log($"Nouvelle zone {zoneName} enregistrée: {zoneNetworkBehaviours[zoneName].Count} objets réseau");
    }

    private void UpdateNetworkZoneActivity(string previousZone, string newZone)
    {
        // Désactiver les mises à jour réseau de l'ancienne zone
        if (!string.IsNullOrEmpty(previousZone) && zoneNetworkBehaviours.ContainsKey(previousZone))
        {
            SetZoneNetworkActivity(previousZone, false);
            Debug.Log($"Zone {previousZone}: mises à jour réseau désactivées");
        }

        // Activer les mises à jour réseau de la nouvelle zone
        if (zoneNetworkBehaviours.ContainsKey(newZone))
        {
            SetZoneNetworkActivity(newZone, true);
            Debug.Log($"Zone {newZone}: mises à jour réseau activées");
        }

        // Mettre à jour les zones adjacentes si nécessaire
        UpdateAdjacentZones(newZone);
    }

    // Méthode statique pour résoudre l'erreur de référence
    private static void SetZoneNetworkActivity(string zoneName, bool active)
    {
        if (!zoneNetworkBehaviours.ContainsKey(zoneName)) return;

        zoneNetworkActivity[zoneName] = active;

        foreach (NetworkBehaviour nb in zoneNetworkBehaviours[zoneName])
        {
            if (nb != null)
            {
                // Désactiver/activer les composants réseau spécifiques
                SetNetworkBehaviourActivity(nb, active);
            }
        }
    }

    // Méthode statique pour résoudre l'erreur de référence
    private static void SetNetworkBehaviourActivity(NetworkBehaviour nb, bool active)
    {
        // Désactiver/activer les NetworkTransform
        NetworkTransform[] networkTransforms = nb.GetComponents<NetworkTransform>();
        foreach (NetworkTransform nt in networkTransforms)
        {
            if (nt != null)
            {
                nt.enabled = active;
            }
        }

        // Désactiver/activer les autres composants réseau selon vos besoins
        // Exemple: désactiver les RPC, les synchronisations, etc.

        // Pour les objets avec des scripts réseau custom, vous pouvez ajouter:
        if (nb is MonoBehaviour behaviour)
        {
            behaviour.enabled = active;
        }
    }

    private void UpdateAdjacentZones(string currentZone)
    {
        // Ici vous pouvez implémenter une logique pour garder actives
        // les zones adjacentes à la zone courante pour des transitions fluides
        foreach (string zone in zoneNetworkBehaviours.Keys)
        {
            if (zone != currentZone && IsZoneAdjacent(zone, currentZone))
            {
                SetZoneNetworkActivity(zone, true);
                Debug.Log($"Zone adjacente {zone} gardée active");
            }
        }
    }

    private bool IsZoneAdjacent(string zoneA, string zoneB)
    {
        // Implémentez votre logique d'adjacence entre zones
        // Par exemple basée sur la position, des connections prédéfinies, etc.
        return false; // À adapter
    }

    private bool IsLocalPlayer(GameObject playerObject)
    {
        var controller = playerObject.GetComponent<vThirdPersonController>();
        return controller != null && controller.isActiveAndEnabled;
    }

    // Méthodes utilitaires pour le debug et la gestion
    public static void PrintZoneStats()
    {
        Debug.Log("=== STATISTIQUES DES ZONES RÉSEAU ===");
        foreach (var entry in zoneNetworkBehaviours)
        {
            string status = zoneNetworkActivity.ContainsKey(entry.Key) && zoneNetworkActivity[entry.Key] ? "ACTIVE" : "INACTIVE";
            Debug.Log($"Zone: {entry.Key}, Objets: {entry.Value.Count}, Statut: {status}");
        }
    }

    public static void ForceActivateZone(string zoneName)
    {
        if (zoneNetworkBehaviours.ContainsKey(zoneName))
        {
            SetZoneNetworkActivity(zoneName, true);
        }
    }

    public static void ForceDeactivateZone(string zoneName)
    {
        if (zoneNetworkBehaviours.ContainsKey(zoneName) && zoneName != currentPlayerZone)
        {
            SetZoneNetworkActivity(zoneName, false);
        }
    }

    // Gizmos pour le debug
    private void OnDrawGizmos()
    {
        if (showDebugGizmos)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1f);

            // Afficher le nom de la zone
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, targetZoneName);
#endif
        }
    }
}