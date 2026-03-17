// PlayerNetworkInput.cs
using Invector.vCharacterController;
using PurrNet;
using UnityEngine;

public class PlayerNetworkInput : NetworkBehaviour
{
    private vThirdPersonController controller;
    private vThirdPersonInput inputHandler;
    private Camera playerCamera;

    private void Start()
    {
        controller = GetComponent<vThirdPersonController>();
        inputHandler = GetComponent<vThirdPersonInput>();
        playerCamera = GetComponentInChildren<Camera>();

        // La caméra n'est active que pour le propriétaire
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(isOwner);

        // Note : Les composants Invector (controller, inputHandler) ne sont plus activés/désactivés ici.
        // Leur activation est gérée par un composant externe (ex: LocalOnly) ou par le module d'ownership.
        // Ils restent actifs par défaut, mais sur les clients non-possédés ils n'auront pas d'effet
        // car le NetworkTransform est en mode Server Authoritative et les inputs ne sont pas lus.
    }

    private void Update()
    {
        if (!isOwner) return; // Seul le propriétaire envoie des inputs

        // Lire les inputs Invector standards
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool strafe = Input.GetKeyDown(KeyCode.Tab);

        // Envoyer au serveur
        SendInputServerRpc(horizontal, vertical, jump, sprint, strafe);
    }

    [ServerRpc(PurrNet.Transports.Channel.Unreliable)]
    private void SendInputServerRpc(float horizontal, float vertical, bool jump, bool sprint, bool strafe)
    {
        if (controller == null) return;

        // Appliquer les inputs sur l'instance serveur du joueur
        controller.input.x = horizontal;
        controller.input.z = vertical;

        if (jump && controller.animator != null && controller._rigidbody != null)
            controller.Jump();

        if (sprint)
            controller.Sprint(sprint);

        if (strafe)
            controller.Strafe();
    }
}