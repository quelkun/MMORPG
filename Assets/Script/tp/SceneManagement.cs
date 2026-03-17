using Invector.vCharacterController;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SimpleSceneTeleporter : NetworkBehaviour
{
    [Header("Scene Configuration")]
    [PurrScene] public string sceneToChange;

    [Header("Teleport Position")]
    public Vector3 teleportPosition = Vector3.zero;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Suivi des scènes déjà chargées
    private static Dictionary<string, Scene> loadedScenes = new Dictionary<string, Scene>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugLogs)
                Debug.Log("Joueur entré dans le téléporteur: " + other.gameObject.name);

            // Vérifier si c'est le joueur local
            if (IsLocalPlayer(other.gameObject))
            {
                if (showDebugLogs)
                    Debug.Log("Téléportation du joueur local initiée");

                TeleportLocalPlayer(other.gameObject);
            }
        }
    }

    private void TeleportLocalPlayer(GameObject playerObject)
    {
        StartCoroutine(TeleportCoroutine(playerObject));
    }

    private IEnumerator TeleportCoroutine(GameObject playerObject)
    {
        // Désactiver le contrôleur
        var controller = playerObject.GetComponent<vThirdPersonController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Vérifier si la scène est déjà chargée
        Scene targetScene = SceneManager.GetSceneByName(sceneToChange);

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            if (showDebugLogs)
                Debug.Log($"Chargement de la scène {sceneToChange}...");

            // Charger la scène de manière additive
            var asyncLoad = SceneManager.LoadSceneAsync(sceneToChange, LoadSceneMode.Additive);
            yield return new WaitUntil(() => asyncLoad.isDone);

            targetScene = SceneManager.GetSceneByName(sceneToChange);

            // Mettre à jour le dictionnaire des scènes chargées
            if (!loadedScenes.ContainsKey(sceneToChange))
            {
                loadedScenes.Add(sceneToChange, targetScene);
            }

            if (showDebugLogs)
                Debug.Log($"Scène {sceneToChange} chargée avec succès");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"Scène {sceneToChange} déjà chargée, réutilisation");
        }

        // Déplacer le joueur vers la nouvelle scène
        SceneManager.MoveGameObjectToScene(playerObject, targetScene);

        // Déplacer à la position de téléportation
        playerObject.transform.position = teleportPosition;

        // Réactiver le contrôleur
        if (controller != null)
        {
            controller.enabled = true;
        }

        if (showDebugLogs)
            Debug.Log($"Joueur téléporté vers {sceneToChange} à la position {teleportPosition}");

        // Optionnel: Décharger l'ancienne scène si vous voulez
        // StartCoroutine(UnloadPreviousScene(playerObject.scene));
    }

    // Méthode optionnelle pour décharger l'ancienne scène
    private IEnumerator UnloadPreviousScene(Scene previousScene)
    {
        // Attendre un peu pour être sûr que le joueur a bien été déplacé
        yield return new WaitForSeconds(1f);

        // Ne pas décharger la scène principale (celle avec l'index 0)
        if (previousScene.buildIndex != 0 && previousScene.isLoaded)
        {
            if (showDebugLogs)
                Debug.Log($"Déchargement de l'ancienne scène: {previousScene.name}");

            yield return SceneManager.UnloadSceneAsync(previousScene);
        }
    }

    private bool IsLocalPlayer(GameObject playerObject)
    {
        // Vérifier via le contrôleur de personnage
        var controller = playerObject.GetComponent<vThirdPersonController>();
        return controller != null && controller.isActiveAndEnabled;
    }

    // Méthode statique pour décharger une scène spécifique si nécessaire
    public static void UnloadScene(string sceneName)
    {
        if (loadedScenes.ContainsKey(sceneName))
        {
            Scene sceneToUnload = loadedScenes[sceneName];
            if (sceneToUnload.IsValid() && sceneToUnload.isLoaded)
            {
                SceneManager.UnloadSceneAsync(sceneToUnload);
                loadedScenes.Remove(sceneName);
            }
        }
    }

    // Méthode pour vider le cache si nécessaire
    public static void ClearLoadedScenesCache()
    {
        loadedScenes.Clear();
    }
}