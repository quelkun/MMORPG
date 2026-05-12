using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class EnemyRespawner : MonoBehaviour
{
    public static EnemyRespawner Instance;

    [Header("Respawn Points")]
    public Transform[] respawnPoints;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Respawn a new enemy using the original config and destroy the current one.
    /// </summary>
    /// <param name="config">Enemy's config</param>
    /// <param name="original">The original dead enemy's transform</param>
    public void RespawnEnemy(EnemyAIConfigSO config, Transform original)
    {
        GameObject originalPrefab = config.prefab;
        if (originalPrefab == null)
        {
            Debug.LogWarning("⚠️ [EnemyRespawner] Config prefab reference is missing.");
            return;
        }

        StartCoroutine(RespawnCoroutine(config, original, originalPrefab));
    }

    public void StartRespawn(EnemyAIConfigSO config, Transform original)
    {
        GameObject originalPrefab = config.prefab;
        if (originalPrefab == null)
        {
            Debug.LogWarning("⚠️ [EnemyRespawner] Config prefab reference is missing.");
            return;
        }

        StartCoroutine(RespawnCoroutine(config, original, originalPrefab));
    }

    private IEnumerator RespawnCoroutine(EnemyAIConfigSO config, Transform original, GameObject prefab)
    {
        yield return new WaitForSeconds(config.respawnDelay);

        if (respawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ [EnemyRespawner] No respawn points assigned.");
            yield break;
        }

        Transform randomPoint = respawnPoints[Random.Range(0, respawnPoints.Length)];

        GameObject newEnemy = Instantiate(prefab, randomPoint.position, randomPoint.rotation);
        var ai = newEnemy.GetComponent<AIController>();
        if (ai != null)
            ai.config = config;

        Debug.Log($"✅ [EnemyRespawner] Enemy respawned at: {randomPoint.name}");

        // Clean up the original corpse
        if (original != null)
            Destroy(original.gameObject);
    }
}
}
