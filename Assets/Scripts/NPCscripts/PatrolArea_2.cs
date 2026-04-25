using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.NPCscripts
{
    public class PatrolArea_2 : MonoBehaviour
    {
        public float width = 10f;
        public float height = 10f;
        [SerializeField] private int waypointCount = 3;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(width, 5f, height));
        }

        public Vector3[] GetWaypoints()
        {
            Vector3[] points = new Vector3[waypointCount];
            for (int i = 0; i < waypointCount; i++)
                points[i] = GetRandomPoint();
            return points;
        }

        public Vector3 GetRandomPoint()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                float randomX = Random.Range(-width / 2f, width / 2f);
                float randomZ = Random.Range(-height / 2f, height / 2f);

                // ✅ Use the NavMesh floor Y, not PatrolArea Y
                Vector3 randomPoint = new Vector3(
                    transform.position.x + randomX,
                    transform.position.y,
                    transform.position.z + randomZ
                );

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
                {
                    if (IsInsideBox(hit.position))
                    {
                        Debug.Log($"✅ Valid waypoint found: {hit.position}");
                        return hit.position;
                    }
                }
            }

            Debug.LogWarning("❌ No valid point found — returning PatrolArea center");
            return transform.position;
        }

        // Ensures the point never escapes the red boundary box
        private bool IsInsideBox(Vector3 point)
        {
            Vector3 local = point - transform.position;
            return Mathf.Abs(local.x) <= width / 2f &&
                   Mathf.Abs(local.z) <= height / 2f;
        }
    }
}
