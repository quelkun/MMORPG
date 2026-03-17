using UnityEngine;
using PurrNet;

public class CameraManager : NetworkBehaviour
{
    public static CameraManager Instance { get; private set; }

    private vThirdPersonCamera tpCamera;
    private Transform _localPlayer; // Référence au joueur local déjà assigné

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Ne pas détruire au chargement si votre scène change (optionnel)
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        tpCamera = GetComponent<vThirdPersonCamera>();
        if (tpCamera == null)
            tpCamera = Object.FindAnyObjectByType<vThirdPersonCamera>();
    }

    /// <summary>
    /// Appelé par le joueur local pour que la caméra le suive.
    /// </summary>
    public void SetLocalPlayer(Transform playerTransform)
    {
        // Si on a déjà un joueur local, on ignore tout appel ultérieur
        if (localPlayer != null)
        {
            Debug.LogWarning($"Camera déjà assignée à {_localPlayer.name}, nouvel appel de {playerTransform.name} ignoré.");
            return;
        }

        _localPlayer = playerTransform;
        if (tpCamera != null)
        {
            tpCamera.SetMainTarget(_localPlayer);
            Debug.Log($"Caméra cible désormais : {_localPlayer.name}");
        }
    }
}