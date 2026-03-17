using PurrNet;
using UnityEngine;
using System.Collections.Generic;

public class GlobalNetworkOptimizer : MonoBehaviour
{
    [Header("Optimization Settings")]
    public float networkUpdateFrequency = 0.1f; // 10 fois par seconde au lieu de chaque frame
    public bool enableZoneOptimization = true;

    private static GlobalNetworkOptimizer instance;
    private Dictionary<NetworkBehaviour, float> lastUpdateTimes = new Dictionary<NetworkBehaviour, float>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!enableZoneOptimization) return;

        // Implémenter une logique de throttling des mises à jour réseau
        OptimizeNetworkUpdates();
    }

    private void OptimizeNetworkUpdates()
    {
        float currentTime = Time.time;

        // Parcourir tous les NetworkBehaviours et ajuster la fréquence des mises à jour
        // Cette partie dépend de l'API spécifique de PurrNet
        NetworkBehaviour[] allNetworkBehaviours = FindObjectsByType<NetworkBehaviour>(FindObjectsSortMode.None);

        foreach (NetworkBehaviour nb in allNetworkBehaviours)
        {
            // Implémentez votre logique d'optimisation ici
            // Par exemple, réduire la fréquence des mises à jour pour les objets éloignés
        }
    }

    public static void RegisterNetworkBehaviour(NetworkBehaviour nb, string zoneName)
    {
        if (instance != null && instance.enableZoneOptimization)
        {
            // Enregistrer le comportement pour optimisation
            // Cette méthode peut être appelée depuis NetworkZoneManager
        }
    }

    public static void SetNetworkUpdateFrequency(float frequency)
    {
        if (instance != null)
        {
            instance.networkUpdateFrequency = frequency;
        }
    }
}