using UnityEngine;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class PatrolPath : MonoBehaviour
{
    [Tooltip("Assign empty GameObjects as patrol points.")]
    public Transform[] waypoints;

    public Transform GetWaypoint(int index)
    {
        if (waypoints == null || waypoints.Length == 0) return null;
        return waypoints[index % waypoints.Length];
    }

    public int WaypointCount => waypoints?.Length ?? 0;
}
}
