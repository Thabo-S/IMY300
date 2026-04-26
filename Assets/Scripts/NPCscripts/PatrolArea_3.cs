using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.NPCscripts
{
    public class PatrolArea_3 : MonoBehaviour
    {
        public float width = 20f;
        public float height = 10f;

        public Vector3[] GetWaypoints()
        {
            Vector3 center = transform.position;

            Vector3 leftPoint = center + new Vector3(-width / 2f, 0f, 0f);
            Vector3 middlePoint = center;
            Vector3 rightPoint = center + new Vector3(width / 2f, 0f, 0f);

            return new Vector3[]
            {
                GetNavMeshPoint(leftPoint),
                GetNavMeshPoint(middlePoint),
                GetNavMeshPoint(rightPoint)
            };
        }

        private Vector3 GetNavMeshPoint(Vector3 point)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(point, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return point;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            Vector3 center = transform.position;
            Vector3 size = new Vector3(width, 0.2f, height);

            Gizmos.DrawWireCube(center, size);

            Vector3 leftPoint = center + new Vector3(-width / 2f, 0f, 0f);
            Vector3 middlePoint = center;
            Vector3 rightPoint = center + new Vector3(width / 2f, 0f, 0f);

            Gizmos.DrawSphere(leftPoint, 0.4f);
            Gizmos.DrawSphere(middlePoint, 0.4f);
            Gizmos.DrawSphere(rightPoint, 0.4f);

            Gizmos.DrawLine(leftPoint, middlePoint);
            Gizmos.DrawLine(middlePoint, rightPoint);
        }
    }
}