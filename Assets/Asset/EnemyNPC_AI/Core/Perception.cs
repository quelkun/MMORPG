using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

[RequireComponent(typeof(AIController))]
public class Perception : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 20f;

    [Range(0, 360)]
    public float viewAngle = 120f;

    [Header("Hearing Settings")]
    public float hearingRadius = 5f;

    [Header("Detection Masks")]
    public LayerMask targetMask;      // Should include Player
    public LayerMask obstructionMask; // Should include walls and obstacles

    [Header("Stealth Settings")]
    public float crouchIgnoreDistance = 4f;

    [HideInInspector]
    public bool isPlayerDetected = false;

    public bool CanSeeTarget(Transform target)
    {
        if (target == null)
        {
            isPlayerDetected = false;
            return false;
        }

        Vector3 origin = transform.position + Vector3.up;
        Vector3 dirToTarget = (target.position + Vector3.up) - origin;
        float distToTarget = dirToTarget.magnitude;
        dirToTarget.Normalize();

        if (distToTarget <= viewRadius)
        {
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            if (angleToTarget <= viewAngle / 2f)
            {
                if (!Physics.Raycast(origin, dirToTarget, distToTarget, obstructionMask))
                {
                    StealthEvaluator stealth = target.GetComponent<StealthEvaluator>();

                    if (stealth == null || !stealth.forceIgnore)
                    {
                        if (stealth != null && stealth.isHidden)
                        {
                            if (distToTarget < crouchIgnoreDistance)
                            {
                                isPlayerDetected = true;
                                return true;
                            }
                            else
                            {
                                isPlayerDetected = false;
                                return false;
                            }
                        }

                        isPlayerDetected = true;
                        return true;
                    }

                    isPlayerDetected = false;
                    return false;
                }
            }
        }

        isPlayerDetected = false;
        return false;
    }

    public void OnNoiseHeard(Vector3 noiseSource, float noiseRadius)
    {
        float distance = Vector3.Distance(transform.position, noiseSource);

        if (distance <= noiseRadius && !isPlayerDetected)
        {
            if (TryGetComponent(out AIController ai))
            {
                ai.SetSearchLocation(noiseSource);
                ai.SetState(AIController.AIState.Search);
                Debug.Log($"[Perception] Noise heard by {gameObject.name} at {noiseSource}");
            }
        }
    }

    public bool CanHearTarget(Transform target)
    {
        if (target == null)
        {
            isPlayerDetected = false;
            return false;
        }

        var stealth = target.GetComponent<StealthEvaluator>();
        if (stealth != null && stealth.IsCurrentlyHidden())
        {
            isPlayerDetected = false;
            return false;
        }

        float dist = Vector3.Distance(transform.position, target.position);
        isPlayerDetected = dist <= hearingRadius;
        return isPlayerDetected;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        Vector3 origin = transform.position;
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2f);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + viewAngleA * viewRadius);
        Gizmos.DrawLine(origin, origin + viewAngleB * viewRadius);

#if UNITY_EDITOR
        AIController controller = GetComponent<AIController>();
        if (controller != null)
        {
            UnityEditor.Handles.Label(origin + Vector3.up * 2f, $"State: {controller.GetCurrentState()}");
        }
#endif
    }

    public Vector3 DirFromAngle(float angleInDegrees)
    {
        return Quaternion.Euler(0, angleInDegrees, 0) * transform.forward;
    }
}
}
