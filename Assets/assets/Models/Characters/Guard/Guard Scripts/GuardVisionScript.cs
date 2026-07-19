using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Guardscripts
{
    // Plain MonoBehaviour, not GuardComponent - this sits on a child "eye" object
    // and doesn't need its own NavMeshAgent/Animator, which GuardComponent would
    // otherwise force onto whatever GameObject it's attached to.
    public class GuardVisionScript : MonoBehaviour
    {
        [Header("Vision Settings")]
        private Transform player;
        [SerializeField] private Transform eyePoint;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private Guard guard;
        [SerializeField] private float visionRange = 28.9f;
        [SerializeField] private float visionAngle = 41.6f;

        [Header("HUD")]
        public Image eyeIcon;

        [Header("Debug")]
        [SerializeField] private bool playerVisible = false;

        private CharacterController playerController;

        void Start()
        {
            if (guard == null)
                guard = GetComponentInParent<Guard>();

            if (player == null)
            {
                GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
                if (foundPlayer != null)
                    player = foundPlayer.transform;
            }

            if (player == null)
            {
                Debug.LogError("GuardVisionScript: no GameObject tagged 'Player' found in the scene.");
                return;
            }

            playerController = player.GetComponent<CharacterController>();
        }

        void Update()
        {
            if (player == null) return;

            playerVisible = CanSeePlayer();

            if (eyeIcon != null)
            {
                eyeIcon.gameObject.SetActive(playerVisible);
            }
        }

        // Single source of truth for "can this guard currently see the player."
        // Guard.CanSeePlayer() calls straight into this.
        public bool CanSeePlayer()
        {
            Transform eye = eyePoint != null ? eyePoint : transform;
            Vector3 origin = eye.position;
            Vector3 target = GetPlayerTargetPosition();
            Vector3 directionToPlayer = target - origin;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer > visionRange)
                return false;

            float angleToPlayer = Vector3.Angle(eye.forward, directionToPlayer.normalized);
            if (angleToPlayer > visionAngle / 2f)
                return false;

            return !Physics.Raycast(origin, directionToPlayer.normalized, distanceToPlayer, obstacleMask);
        }

        private Vector3 GetPlayerTargetPosition()
        {
            if (playerController != null)
                return playerController.bounds.center;

            return player.position + Vector3.up;
        }

        private void OnDrawGizmosSelected()
        {
            Transform eye = eyePoint != null ? eyePoint : transform;
            Vector3 origin = eye.position;

            Gizmos.color = Color.blue;

            Vector3 leftRay = Quaternion.Euler(0, -visionAngle / 2f, 0) * eye.forward;
            Vector3 rightRay = Quaternion.Euler(0, visionAngle / 2f, 0) * eye.forward;

            Gizmos.DrawRay(origin, leftRay * visionRange);
            Gizmos.DrawRay(origin, rightRay * visionRange);

            if (player != null)
            {
                Gizmos.color = playerVisible ? Color.red : Color.blue;

                CharacterController cc = player.GetComponent<CharacterController>();
                Vector3 target = cc != null ? cc.bounds.center : player.position + Vector3.up;

                Gizmos.DrawLine(origin, target);
            }
        }
    }
}