
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//To ther Person wh is current seeing this code .This is a message from the developer of the code
//WHABDLE THIS SCRIPT WITH SOO0 CAREFULL, AS ,I ALMOST LOST MY SOUL DOING THOS
//RIGHT NOW , ONLY GOD AND AI knows whats writtne , as ,i feared,this is spagathetti code , and i dont want to touch it anymore. 
//Add hours to help warn the next developer 

// hours = 24 hrs 

namespace Assets.Scripts.NPCscripts
{
    public class NPCWander : NPCComponent
    {
        [Header("Patrol Settings")]
        [SerializeField] private PatrolArea patrolArea;

        private Vector3[] waypoints;
        private Vector3 startPosition;// Original spawn position
        private Quaternion startRotation;// Original spawn rotation
        private int currentWaypointIndex = 0;
        private bool isMoving = false;
        private bool isPatrolling = true;
        private bool isRotating = false;// True while turning at start position
        private float rotationTimer = 0f;
        private float rotationDuration = 2f; // seconds to complete turn
        private Quaternion fromRotation;
        private Quaternion toRotation;

        private float currentYRotation;
        private float targetYRotation;


        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 90f;// Degrees per second
        private Quaternion targetRotation;// The rotation to turn to

        [Header("Debugging: NPC State")]
        [SerializeField] private State npcState = State.Wandering;

        [SerializeField] private float SuspiciousDuration = 5f;
        [SerializeField] private float IdleDuration = 3f;
        [SerializeField] private float maxIdleTime = 5f;
        [SerializeField] private float suspiciousTimer = 0f;

        [SerializeField] private float startIdleTime = 1000f;// change ideal st

        // ── External class references ──────────────────────────────────────
        // TODO: Assign these in the Inspector when suspicion script is ready
        // [SerializeField] private NPCSuspicion suspicionHandler;
        // ──────────────────────────────────────────────────────────────────

        private void Start()
        {
            // Save exact original transform so we can fully reset each loop
            startPosition = transform.position;
            startRotation = transform.rotation;

            GenerateWaypoints();
            SetDestinationToCurrentWaypoint();
            ChangeState(State.Waiting);
        }

        private void GenerateWaypoints()
        {
            if (patrolArea == null) { Debug.Log("PatrolArea is null!"); return; }

            Vector3[] randomPoints = patrolArea.GetWaypoints();

            // 3 random points + start position as the final destination
            waypoints = new Vector3[randomPoints.Length + 1];
            for (int i = 0; i < randomPoints.Length; i++)
                waypoints[i] = randomPoints[i];

            waypoints[waypoints.Length - 1] = startPosition;
        }

        private void Update()
        {
            // If patrol is suspended (suspicious/alerted), skip all patrol logic
            if (!isPatrolling )
            {
                return;
            }
            if (isRotating)
            {
                rotationTimer += Time.deltaTime;
                float t = Mathf.Clamp01(rotationTimer / rotationDuration);

                // Use the float Y values, not the Quaternions
                float newY = Mathf.Lerp(currentYRotation, targetYRotation, t);
                transform.eulerAngles = new Vector3(0f, newY, 0f);

                Debug.Log($"Rotating Y: {newY:F1}");

                if (t >= 1f)
                {
                    isRotating = false;
                    rotationTimer = 0f;
                    GetComponent<CharacterController>().enabled = true;
                    NPC.agent.enabled = true;
                    NPC.agent.isStopped = true;

                    Debug.Log("✅ Rotation done — entering idle");

                    IdleDuration = startIdleTime;
                    npcState = State.Waiting;
                }

                return;
            }

            if (npcState == State.Waiting)
            {
                IdleDuration -= Time.deltaTime;
                if (IdleDuration <= 0f)
                    ChangeState(State.Wandering);
            }
            else if (npcState == State.Wandering)
            {
                if (NPC.agent.remainingDistance > NPC.agent.stoppingDistance)
                {
                    isMoving = true;
                }
                if (isMoving && hasArrived())
                {
                    isMoving = false;

                    // ── Returned to start position ─────────────────────────
                    
                    if (currentWaypointIndex == waypoints.Length - 1)
                    {
                        NPC.agent.isStopped = true;
                        NPC.agent.enabled = false;
                        GetComponent<CharacterController>().enabled = false;
                        GetComponent<Animator>().applyRootMotion = false; // ← force disable root motion

                        transform.position = startPosition;

                        currentYRotation = transform.eulerAngles.y;
                        targetYRotation = currentYRotation + 180f; // Just add 180 degrees

                        rotationTimer = 0f;
                        isRotating = true;

                        currentWaypointIndex = 0;
                        GenerateWaypoints();
                        return;
                    }
                    // ──────────────────────────────────────────────────────

                    AdvanceWaypoint();
                    ChangeState(State.Waiting);
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
        //Helper functions 

        public void ChangeState(State state)
        {
            npcState = state;

            if (npcState == State.Waiting)
            {
                IdleDuration = maxIdleTime;
                NPC.agent.isStopped = true;
            }
            else if (npcState == State.Wandering)
            {
                NPC.agent.isStopped = false;
                SetDestinationToCurrentWaypoint();
            }
            else if (npcState == State.Suspicious)
            {
                // ── Suspicious triggered ───────────────────────────────────
                // Halt patrol and rotation entirely
                isPatrolling = false;
                isRotating = false;
                NPC.agent.isStopped = true;

                // TODO: Hand control to the suspicion script
                // suspicionHandler.OnSuspiciousTriggered();
                Debug.Log(" Suspicious triggered — patrol paused, handing off to suspicion handler");
                // ──────────────────────────────────────────────────────────
            }
            else if (npcState == State.Alerted)
            {
                isPatrolling = false;
                isRotating = false;
                NPC.agent.isStopped = true;
            }
        }

        // ── Called externally to resume patrol ────────────────────────────
        // Call this from your suspicion/detection script when investigation ends
        // e.g. npcWander.ResumePatrol();
        public void ResumePatrol()
        {
            Debug.Log(" ResumePatrol called — resetting to start and restarting patrol");

            // Full transform reset back to original position and rotation
            GetComponent<CharacterController>().enabled = true;
            NPC.agent.enabled = true;
            NPC.agent.Warp(startPosition);
            transform.rotation = startRotation;
            NPC.agent.updateRotation = true;

            // Reset patrol state
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
            if (waypoints == null || waypoints.Length == 0) { Debug.Log(" No waypoints!"); return; }
            if (NPC == null || NPC.agent == null) { Debug.Log(" NPC or agent is null!"); return; }
            if (!NPC.agent.isOnNavMesh) { Debug.Log(" Agent not on NavMesh!"); return; }

            isMoving = false;
            NPC.agent.SetDestination(waypoints[currentWaypointIndex]);
            Debug.Log($"Heading to waypoint {currentWaypointIndex}: {waypoints[currentWaypointIndex]}");
        }

        private void AdvanceWaypoint()
        {
            currentWaypointIndex++;
        }

        private bool hasArrived()
        {
            if (NPC == null || NPC.agent == null) return false;
            if (!NPC.agent.isActiveAndEnabled) return false;
            if (!NPC.agent.isOnNavMesh) return false;
            if (NPC.agent.pathPending) return false;

            return NPC.agent.remainingDistance <= NPC.agent.stoppingDistance;
        }

        // Replace isRotating bool and all rotation code in Update() with this coroutine

        private IEnumerator RotateToStart()
        {
            Debug.Log(" Starting rotation coroutine");

            // Fully disable agent so it cannot interfere at all
            NPC.agent.enabled = false;

            Quaternion from = transform.rotation;
            Quaternion to = startRotation * Quaternion.Euler(0f, 180f, 0f);

            float elapsed = 0f;
            float duration = 2f; // How many seconds the rotation takes — adjust as needed

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Smooth the rotation using a curve so it eases in/out
                t = Mathf.SmoothStep(0f, 1f, t);

                transform.rotation = Quaternion.Slerp(from, to, t);
                yield return null; // Wait one frame
            }

            transform.rotation = to; // Snap to exact final rotation

            // Re-enable agent after rotation is done
            NPC.agent.enabled = true;

            Debug.Log("Rotation done — entering idle");

            // Now enter idle
            IdleDuration = startIdleTime;
            npcState = State.Waiting;
            NPC.agent.isStopped = true;
        }




    }//end of class

}//end of namespace 


