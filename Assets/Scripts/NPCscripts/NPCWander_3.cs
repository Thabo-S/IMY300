using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.NPCscripts
{
    public class NPCWander_3 : NPCComponent
    {
        [SerializeField] private PatrolArea_3 patrolArea;
        [SerializeField] private float waitTime = 3f;
        [SerializeField] private float turnSpeed = 8f;

        private Vector3[] waypoints;
        private int currentIndex = 0;
        private int direction = 1;

        private bool isWaiting = false;
        private float waitTimer = 0f;

        private void Start()
        {
            if (patrolArea == null)
            {
                Debug.LogError("PatrolArea_3 is missing on NPCWander_3.");
                return;
            }

            waypoints = patrolArea.GetWaypoints();

            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("No patrol points found.");
                return;
            }

            currentIndex = 0;
            MoveToCurrentPoint();
        }

        private void Update()
        {
            if (NPC == null || NPC.agent == null || !NPC.agent.isOnNavMesh)
                return;

            if (isWaiting)
            {
                FaceNextPoint();

                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    GoToNextPoint();
                }

                return;
            }

            if (!NPC.agent.pathPending &&
                NPC.agent.remainingDistance <= NPC.agent.stoppingDistance + 0.1f)
            {
                StartWaiting();
            }
        }

        private void MoveToCurrentPoint()
        {
            NPC.agent.isStopped = false;
            NPC.agent.SetDestination(waypoints[currentIndex]);

            Debug.Log("Heading to patrol point " + currentIndex + ": " + waypoints[currentIndex]);
        }

        private void StartWaiting()
        {
            NPC.agent.isStopped = true;
            isWaiting = true;
            waitTimer = waitTime;
        }

        private void GoToNextPoint()
        {
            if (currentIndex == waypoints.Length - 1)
                direction = -1;
            else if (currentIndex == 0)
                direction = 1;

            currentIndex += direction;
            MoveToCurrentPoint();
        }

        private void FaceNextPoint()
        {
            int nextIndex = currentIndex + direction;

            if (nextIndex >= waypoints.Length)
                nextIndex = currentIndex - 1;
            else if (nextIndex < 0)
                nextIndex = currentIndex + 1;

            Vector3 directionToNext = waypoints[nextIndex] - transform.position;
            directionToNext.y = 0f;

            if (directionToNext.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(directionToNext);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}