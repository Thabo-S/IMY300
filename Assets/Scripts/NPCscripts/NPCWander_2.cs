using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.NPCscripts
{
    public class NPCWander_2 : NPCComponent
    {
        [Header("Patrol Settings")]
        [SerializeField] private PatrolArea_2 patrolArea;

        private Vector3[] waypoints;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private int currentWaypointIndex = 0;
        private bool isMoving = false;
        private bool isPatrolling = true;

        // ── Rotation at each waypoint ──────────────────────────────────────
        private bool isRotating = false;
        private float rotationTimer = 0f;
        [SerializeField] private float rotationDuration = 1f;
        private float currentYRotation;
        private float targetYRotation;
        // ──────────────────────────────────────────────────────────────────

        [Header("Debugging: NPC State")]
        [SerializeField] private State npcState = State.Wandering;

        [SerializeField] private float SuspiciousDuration = 5f;
        [SerializeField] private float IdleDuration = 3f;
        [SerializeField] private float maxIdleTime = 3f;
        [SerializeField] private float startIdleTime = 5f;
        [SerializeField] private float suspiciousTimer = 0f;

        private void Start()
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                NPC.agent.Warp(hit.position);
                Debug.Log($"✅ Snapped to NavMesh at: {hit.position}");
            }
            else
            {
                Debug.LogError("❌ Not on NavMesh! Move the NPC onto the baked NavMesh surface.");
                return;
            }

            // Save AFTER warp so startPosition is guaranteed on NavMesh
            startPosition = NPC.agent.nextPosition;
            startRotation = transform.rotation;
            Debug.Log($"✅ Start position saved: {startPosition}");

            GenerateWaypoints();
            ChangeState(State.Wandering);          // idle first, then start moving
        }

        private void GenerateWaypoints()
        {
            if (patrolArea == null) { Debug.LogWarning("❌ PatrolArea is null!"); return; }

            Vector3[] randomPoints = patrolArea.GetWaypoints();

            // Random waypoints + start position as the final "return home" waypoint
            waypoints = new Vector3[randomPoints.Length + 1];
            for (int i = 0; i < randomPoints.Length; i++)
                waypoints[i] = randomPoints[i];
            waypoints[waypoints.Length - 1] = startPosition;

            for (int i = 0; i < waypoints.Length; i++)
                Debug.Log($"📍 Waypoint {i}: {waypoints[i]}");
        }

        private void Update()
        {
            if (!isPatrolling) return;

            // ── Smooth rotation at each waypoint ──────────────────────────
            if (isRotating)
            {
                rotationTimer += Time.deltaTime;
                float t = Mathf.Clamp01(rotationTimer / rotationDuration);
                float newY = Mathf.Lerp(currentYRotation, targetYRotation, t);
                transform.eulerAngles = new Vector3(0f, newY, 0f);

                if (t >= 1f)
                {
                    isRotating = false;
                    rotationTimer = 0f;

                    // ✅ FIX: Don't disable the agent — just let it idle
                    bool isAtStart = (currentWaypointIndex == 0);
                    IdleDuration = isAtStart ? startIdleTime : maxIdleTime;
                    npcState = State.Waiting;

                    Debug.Log($"✅ Rotation done — idling {IdleDuration}s. Next waypoint index: {currentWaypointIndex}");
                }
                return;
            }
            // ──────────────────────────────────────────────────────────────

            if (npcState == State.Waiting)
            {
                IdleDuration -= Time.deltaTime;
                if (IdleDuration <= 0f)
                    ChangeState(State.Wandering);
            }
            else if (npcState == State.Wandering)
            {
                // ✅ FIX: isMoving is set true in SetDestinationToCurrentWaypoint()
                // so we only need the arrival check here
                if (isMoving && HasArrived())
                {
                    isMoving = false;

                    bool isAtStart = (currentWaypointIndex == waypoints.Length - 1);

                    // ── Stop agent and begin rotation ──────────────────────
                    NPC.agent.isStopped = true;

                    // Face the NEXT waypoint
                    int nextIndex = isAtStart ? 0 : currentWaypointIndex + 1;
                    Vector3 directionToNext = waypoints[nextIndex] - transform.position;
                    directionToNext.y = 0f;

                    currentYRotation = transform.eulerAngles.y;
                    targetYRotation = (directionToNext.magnitude > 0.1f)
                        ? Quaternion.LookRotation(directionToNext).eulerAngles.y
                        : currentYRotation + 180f;

                    rotationTimer = 0f;
                    isRotating = true;
                    // ──────────────────────────────────────────────────────

                    if (isAtStart)
                    {
                        // Completed full loop — regenerate for next cycle
                        currentWaypointIndex = 0;
                        GenerateWaypoints();
                        Debug.Log("🔄 Returned to start — new waypoints generated");
                    }
                    else
                    {
                        AdvanceWaypoint();
                    }
                }
            }
            else if (npcState == State.Suspicious)
            {
                suspiciousTimer -= Time.deltaTime;
                if (suspiciousTimer <= 0f)
                {
                    currentWaypointIndex = 0;
                    GenerateWaypoints();
                    ChangeState(State.Wandering);
                }
            }
        }

        public void ChangeState(State state)
        {
            npcState = state;

            if (npcState == State.Waiting)
            {
                IdleDuration = maxIdleTime;
                NPC.agent.isStopped = true;
                Debug.Log($"⏸ State → Waiting ({IdleDuration}s)");
            }
            else if (npcState == State.Wandering)
            {
                Debug.Log($"🚶 State → Wandering — heading to waypoint {currentWaypointIndex}");
                NPC.agent.isStopped = false;
                SetDestinationToCurrentWaypoint();
            }
            else if (npcState == State.Suspicious)
            {
                isPatrolling = false;
                isRotating = false;
                NPC.agent.isStopped = true;
                suspiciousTimer = SuspiciousDuration;   // ✅ FIX: reset timer on entry
                Debug.Log("👀 State → Suspicious — patrol paused");
            }
            else if (npcState == State.Alerted)
            {
                isPatrolling = false;
                isRotating = false;
                NPC.agent.isStopped = true;
                Debug.Log("🚨 State → Alerted");
            }
        }

        // ── Called externally (e.g. from suspicion script) to resume patrol ──
        public void ResumePatrol()
        {
            NPC.agent.Warp(startPosition);
            transform.rotation = startRotation;

            currentWaypointIndex = 0;
            isMoving = false;
            isRotating = false;
            isPatrolling = true;

            GenerateWaypoints();
            ChangeState(State.Wandering);
        }
        // ──────────────────────────────────────────────────────────────────

        private void SetDestinationToCurrentWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogWarning("❌ No waypoints — regenerating");
                GenerateWaypoints();
            }
            if (NPC == null || NPC.agent == null) { Debug.LogWarning("❌ NPC or agent is null!"); return; }
            if (!NPC.agent.isOnNavMesh)
            {
                // ✅ FIX: Try to recover before giving up
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                {
                    NPC.agent.Warp(hit.position);
                    Debug.Log($"🔧 Auto-warped back to NavMesh at {hit.position}");
                }
                else
                {
                    Debug.LogWarning("❌ Cannot place agent on NavMesh!");
                    return;
                }
            }

            // ✅ FIX: Set isMoving = true immediately — don't wait for remainingDistance check
            isMoving = true;
            NPC.agent.isStopped = false;
            NPC.agent.SetDestination(waypoints[currentWaypointIndex]);
            Debug.Log($"➡️ Heading to waypoint {currentWaypointIndex}: {waypoints[currentWaypointIndex]}");
        }

        private void AdvanceWaypoint()
        {
            currentWaypointIndex++;
            Debug.Log($"⏩ Advanced to waypoint index {currentWaypointIndex}");
        }

        private bool HasArrived()
        {
            if (NPC == null || NPC.agent == null) return false;
            if (!NPC.agent.isActiveAndEnabled) return false;
            if (!NPC.agent.isOnNavMesh) return false;
            if (NPC.agent.pathPending) return false;

            return NPC.agent.remainingDistance <= NPC.agent.stoppingDistance + 0.1f;
        }
    }
}