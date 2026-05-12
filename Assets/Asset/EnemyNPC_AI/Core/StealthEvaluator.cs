using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class StealthEvaluator : MonoBehaviour
{
    [Header("Stealth Control Flags")]
    [Tooltip("Automatically set by PlayerStealthController or AI logic.")]
    public bool isHidden = false;

    [Tooltip("If enabled, this object will always be ignored by enemy detection.")]
    public bool forceIgnore = false;

    [Tooltip("Optional: Tag to match for stealth evaluation (e.g., 'Player'). Leave empty to ignore.")]
    public string requiredTag = "";

    [Tooltip("Automatically toggled by systems like crouch, cover, or special stealth zones.")]
    public bool inStealthZone = false;

    /// <summary>
    /// Main evaluation method to determine if the object is currently hidden from AI.
    /// </summary>
    public bool IsCurrentlyHidden()
    {
        if (forceIgnore)
            return true;

        if (!string.IsNullOrEmpty(requiredTag) && !CompareTag(requiredTag))
            return false;

        return isHidden || inStealthZone;
    }

    /// <summary>
    /// Used to override stealth state dynamically (e.g., when entering stealth triggers).
    /// </summary>
    public void SetHiddenState(bool value)
    {
        isHidden = value;
    }

    /// <summary>
    /// Called by environmental zones to flag stealth state.
    /// </summary>
    public void SetStealthZoneState(bool value)
    {
        inStealthZone = value;
    }
}
}
