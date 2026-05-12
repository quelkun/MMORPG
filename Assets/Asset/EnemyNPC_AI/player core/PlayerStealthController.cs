using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{
[RequireComponent(typeof(StealthEvaluator))]
public class PlayerStealthController : MonoBehaviour
{
    [Header("Stealth Settings")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public bool toggleCrouch = false;
    public float crouchHeightMultiplier = 0.5f;

    [Header("References")]
    public CharacterController controller; // Optional but recommended
    private StealthEvaluator stealthEvaluator;

    private bool isCrouching = false;
    private float originalHeight;

    private void Awake()
    {
        stealthEvaluator = GetComponent<StealthEvaluator>();
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (controller != null)
            originalHeight = controller.height;
    }

    private void Update()
    {
        if (toggleCrouch)
        {
            if (Input.GetKeyDown(crouchKey))
                ToggleCrouch();
        }
        else
        {
            bool crouchHeld = Input.GetKey(crouchKey);
            SetCrouch(crouchHeld);
        }
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        SetCrouch(isCrouching);
    }

    private void SetCrouch(bool crouch)
    {
        isCrouching = crouch;
        stealthEvaluator.isHidden = crouch;

        if (controller != null)
        {
            controller.height = crouch ? originalHeight * crouchHeightMultiplier : originalHeight;
            Vector3 center = controller.center;
            center.y = controller.height / 2f;
            controller.center = center;
        }
    }
}
}
