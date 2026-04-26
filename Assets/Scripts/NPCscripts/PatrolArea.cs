using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.NPCscripts
{
    public class PatrolArea : MonoBehaviour
    {
        public float width = 10f;
        public float height = 10f;
        [SerializeField] private float Radius = 10f;
        [SerializeField] private int waypointCount = 3;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            Vector3 size = new Vector3(width, 5f, height);
            Gizmos.DrawWireCube(center, size);
        }

        public Vector3[] GetWaypoints()
        {
            Vector3[] points = new Vector3[waypointCount];
            for (int i = 0; i < waypointCount; i++)
            {
                points[i] = GetRandomPoint();
            }
            return points;
        }

        public Vector3 GetRandomPoint()
        {
            // Try up to 10 times to find a valid NavMesh point
            for (int i = 0; i < 10; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere * Radius;
                randomDirection.y = 0;
                Vector3 randomPoint = transform.position + randomDirection;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, Radius, NavMesh.AllAreas))
                {
                    return hit.position; // ✅ Valid point found
                }
            }

            Debug.LogWarning("Could not find valid NavMesh point — check PatrolArea position and Radius");
            return transform.position; // Fallback to PatrolArea center, not NPC position
        }
    }
}


