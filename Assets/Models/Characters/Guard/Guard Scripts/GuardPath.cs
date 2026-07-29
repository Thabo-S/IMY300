using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Guardscripts; // for EndFacingDirection

#if UNITY_EDITOR
using UnityEditor;
#endif

// Context class in the Strategy pattern. GuardPath itself holds no traversal
// logic - it just picks a concrete IPatrolStrategy based on the Inspector
// dropdown and forwards every call to it. Waypoints are always this
// GameObject's direct children, in Hierarchy order - there is no manual list
// to fill in.
//
// Named GuardPath (not Path) deliberately - "Path" collides with the built-in
// System.IO.Path class and causes ambiguous-reference compile errors in any
// file that also has "using System.IO;".
public class GuardPath : MonoBehaviour
{
    [Header("Patrol Mode")]
    [Tooltip("Loop: walks 1 -> 2 -> 3 -> ... -> N -> 1 (wraps around). BackAndForth: walks 1 -> 2 -> ... -> N -> ... -> 2 -> 1 -> 2 ... (reverses at the ends).")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Loop;

    [Header("Waypoints (read-only, built from children)")]
    [Tooltip("Auto-filled from this GameObject's direct children every time the route is built. Add/remove/reorder waypoints by adding/removing/reordering child GameObjects in the Hierarchy - do not edit this list by hand.")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Header("End Point Facing (BackAndForth only)")]
    [Tooltip("Which way the Guard should turn to face once it reaches the LAST point in the route, relative to the direction it was walking when it got there.")]
    [SerializeField] private EndFacingDirection endPointFacing = EndFacingDirection.Forward;

    [Header("Gizmo Settings")]
    [SerializeField] private bool drawAsLoop;
    [SerializeField] private bool drawNumbers;
    [SerializeField] private float gizmoSphereRadius = 0.9f;
    public Color debugColour = Color.white;

    private List<Vector3> patrolPoints = new List<Vector3>();
    private IPatrolStrategy strategy;

    public PatrolMode Mode => patrolMode;

    private void Awake()
    {
        BuildStrategy();
        RebuildRoute();
    }

    // ---------------- STRATEGY SELECTION ----------------

    private void BuildStrategy()
    {
        switch (patrolMode)
        {
            case PatrolMode.BackAndForth:
                strategy = new BackAndForthPatrolStrategy(endPointFacing);
                break;
            case PatrolMode.Loop:
            default:
                strategy = new LoopPatrolStrategy();
                break;
        }
    }

    // ---------------- WAYPOINT COLLECTION (children only) ----------------

    private void CollectChildWaypoints()
    {
        waypoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            waypoints.Add(child);
        }

        if (waypoints.Count < 1)
        {
            Debug.LogError($"GuardPath on '{gameObject.name}' needs at least 2 child waypoints. Found {waypoints.Count}. Add child GameObjects under this GuardPath in the desired walking order.", this);
        }
    }

    private void RefreshPatrolPoints()
    {
        CollectChildWaypoints();

        patrolPoints = new List<Vector3>();
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                patrolPoints.Add(waypoint.position);
            }
        }
    }

    public Vector3[] GetPatrolPoints()
    {
        return patrolPoints == null ? new Vector3[0] : patrolPoints.ToArray();
    }

    // Call this if you change patrol mode, or add/remove/reorder waypoint
    // children, at runtime.
    public void RebuildRoute()
    {
        if (strategy == null) BuildStrategy();
        RefreshPatrolPoints();
        strategy.BuildRoute(patrolPoints);
    }

    // ---------------- DELEGATION TO STRATEGY ----------------

    public Vector3 GetCurrentWaypoint()
    {
        if (strategy == null) RebuildRoute();
        return strategy.GetCurrentWaypoint();
    }

    public Vector3 GetNextWaypoint()
    {
        if (strategy == null) RebuildRoute();
        return strategy.GetNextWaypoint();
    }

    // Only meaningful for BackAndForth - returns identity rotation from Loop
    // (which has no "end" to face), so calling this on a Loop guard is harmless.
    public Quaternion ComputeFacingRotation(Vector3 incomingDirection)
    {
        if (strategy is BackAndForthPatrolStrategy backAndForth)
        {
            return backAndForth.ComputeFacingRotation(incomingDirection, transform);
        }
        return transform.rotation;
    }

    // ---------------- GIZMOS ----------------
    // Always drawn, whether or not this GameObject is selected - OnDrawGizmos
    // already fires every Scene-view repaint with no selection requirement.
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        DrawPath();
    }

    public void DrawPath()
    {
        CollectChildWaypoints();

        if (waypoints == null) return;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 30;
            labelStyle.normal.textColor = debugColour;
            if (drawNumbers)
                Handles.Label(waypoints[i].position, i.ToString(), labelStyle);

            Gizmos.color = (i % 2 == 1) ? Color.red : debugColour;
            Gizmos.DrawSphere(waypoints[i].position, gizmoSphereRadius);

            if (i >= 1)
            {
                Gizmos.color = debugColour;
                Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);

                if (drawAsLoop)
                    Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
            }
        }
    }
#endif
}