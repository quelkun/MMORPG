using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private Button respawnButton;

    [Header("Stats")]
    [SerializeField] public int _health = 100;
    public int maxHealth = 100;

    [Header("Respawn")]
    public string spawnPointTag = "Respawn";
    private Transform[] spawnPoints;

    [Header("Components to disable on death")]
    [SerializeField] private MonoBehaviour[] movementScripts; // vThirdPersonController, vThirdPersonInput, PlayerNetworkInput etc.
    [SerializeField] private Collider[] colliders;
    [SerializeField] private Renderer[] renderers; // pour le mesh

    private bool _isDead = false;

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    [ObserversRpc]
    private void SyncHealth(int newHealth)
    {
        _health = newHealth;
        if (_healthText != null)
            _healthText.text = _health.ToString();
    }

    [ObserversRpc]
    private void SyncDeath(bool dead)
    {
        _isDead = dead;

        // Désactive les renderers et colliders pour tout le monde (le joueur devient invisible et intangible)
        foreach (var col in colliders) if (col) col.enabled = !dead;
        foreach (var rend in renderers) if (rend) rend.enabled = !dead;

        // Pour le propriétaire seulement, désactive ses scripts de mouvement
        if (isOwner)
        {
            foreach (var script in movementScripts) if (script) script.enabled = !dead;
        }

        // Affiche l'écran de mort seulement pour le propriétaire
        if (isOwner && _deathScreen != null)
            _deathScreen.SetActive(dead);
    }

    private void Start()
    {
        // Récupère les points de spawn (identique)
        GameObject[] spawnObjs = GameObject.FindGameObjectsWithTag(spawnPointTag);
        spawnPoints = new Transform[spawnObjs.Length];
        for (int i = 0; i < spawnObjs.Length; i++)
            spawnPoints[i] = spawnObjs[i].transform;

        // Initialisation côté serveur
        if (isServer)
        {
            _health = maxHealth;
            SyncHealth(_health);
            _isDead = false;
            SyncDeath(false);
        }

        if (_deathScreen != null)
            _deathScreen.SetActive(false);

        if (respawnButton != null)
            respawnButton.onClick.AddListener(OnRespawnButtonClicked);
    }

    // Appelé par l'ennemi (côté serveur uniquement)
    [Server]
    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _health -= damage;
        Debug.Log($"Serveur : {name} a reçu {damage} dégâts, vie = {_health}");
        SyncHealth(_health);

        if (_health <= 0)
        {
            _health = 0;
            SyncDeath(true);
        }
    }

    [ServerRpc(requireOwnership: false)] // Si ta version supporte le paramètre, sinon enlève-le
    private void RequestRespawnServerRpc()
    {
        // Cette méthode est exécutée sur le serveur
        Respawn();
    }

    private void OnRespawnButtonClicked()
    {
        if (isOwner && _isDead)
        {
            // Envoie la demande au serveur (même si on est client)
            RequestRespawnServerRpc();
        }
    }

    // Méthode publique pour le respawn (doit être appelée côté serveur)
    public void Respawn()
    {
        if (!isServer) return;
        if (!_isDead) return;

        Transform spawn = GetRandomSpawnPoint();
        transform.position = spawn.position;
        transform.rotation = spawn.rotation;

        _health = maxHealth;
        _isDead = false;
        SyncHealth(_health);
        SyncDeath(false);
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform;
        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index];
    }

    // Pour tester avec la touche Q côté serveur
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && isServer && !_isDead)
            TakeDamage(10);
    }
}