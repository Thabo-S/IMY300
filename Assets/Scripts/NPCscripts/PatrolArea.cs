using UnityEngine;
using UnityEngine.AI;

// NameSpace for the NPC scripts, which can be used to organize related classes and avoid naming conflicts
namespace Assets.Scripts.NPCscripts
{
    public class PatrolArea : MonoBehaviour
    {
        public float width = 10f;
        public float height = 10f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            Vector3 size = new Vector3(width, 0.1f, height);
            Gizmos.DrawWireCube(center, size); //draw a Wire Cube to visualize the patrol area in the editor
        }

        [SerializeField] private float Radius = 10f; //radius of the patrol area
        public Vector3 GetRandomPoint()
        {
            Vector3 randomDirection = Random.insideUnitSphere * Radius;
            randomDirection.y = 0; //keep the point on the same plane as the NPC

            Vector3 randomPoint = transform.position + randomDirection; //offset the random point from the NPC's current position

            NavMeshHit hit;
            Vector3 finalPos = transform.position;

            if (NavMesh.SamplePosition(randomPoint, out hit, Radius, NavMesh.AllAreas))
            {
                finalPos = hit.position;
            }

            return finalPos;
        }
    }
}
