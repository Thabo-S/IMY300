using UnityEngine;
using TMPro;

namespace Assets.Scripts.NPCscripts
{
    public class NPCSensors : NPCComponent
    {
        [Header("Vision Settings")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform eyePoint;
        [SerializeField] private float visionRange = 30f;
        [SerializeField] private float visionAngle = 60f;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Suspicion Settings")]
        [SerializeField] private float suspiciousTimeToGameOver = 3f;

        [Header("Debug")]
        [SerializeField] private int timesPlayerSeen = 0;
        [SerializeField] private float currentSeenTimer = 0f;
        [SerializeField] private bool playerVisible = false;

        [SerializeField] private TextMeshProUGUI seenCountText;

        private bool wasPlayerVisibleLastFrame = false;
        private NPCWander wanderScript;
        private CharacterController playerController;

        protected override void Awake()
        {
            base.Awake();
            wanderScript = GetComponent<NPCWander>();

            if (eyePoint == null)
                eyePoint = transform;
        }

        private void Start()
        {
            if (player == null)
            {
                GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
                if (foundPlayer != null)
                    player = foundPlayer.transform;
            }

            if (player == null)
            {
                Debug.LogError("NPCSensors: Player is missing.");
                return;
            }

            playerController = player.GetComponent<CharacterController>();
            UpdateSeenUI();

            Debug.Log("NPCSensors started on: " + gameObject.name);
        }

        private void Update()
        {
            if (player == null) return;

            playerVisible = CanSeePlayer();

            if (playerVisible)
            {
                currentSeenTimer += Time.deltaTime;

                if (!wasPlayerVisibleLastFrame)
                {
                    timesPlayerSeen++;
                    UpdateSeenUI();
                    Debug.Log("PLAYER DETECTED by " + gameObject.name + " | Instance: " + GetEntityId() + " | Seen count: " + timesPlayerSeen);
                }

                if (currentSeenTimer >= suspiciousTimeToGameOver)
                    GameOver();
            }
            else
            {
                currentSeenTimer = 0f;
            }

            wasPlayerVisibleLastFrame = playerVisible;
        }

        private bool CanSeePlayer()
        {
            Vector3 origin = eyePoint.position;

            Vector3 target;
            if (playerController != null)
                target = playerController.bounds.center;
            else
                target = player.position + Vector3.up;

            Vector3 directionToPlayer = target - origin;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer > visionRange)
                return false;

            float angleToPlayer = Vector3.Angle(eyePoint.forward, directionToPlayer.normalized);

            if (angleToPlayer > visionAngle / 2f)
                return false;

            // First check if obstacle blocks view
            if (Physics.Raycast(origin, directionToPlayer.normalized, out RaycastHit obstacleHit, distanceToPlayer, obstacleMask))
            {
                Debug.Log("Vision blocked by: " + obstacleHit.collider.name);
                return false;
            }

            // Then check player directly
            Collider playerCollider = player.GetComponent<Collider>();

            if (playerCollider != null)
            {
                Ray ray = new Ray(origin, directionToPlayer.normalized);

                if (playerCollider.Raycast(ray, out RaycastHit playerHit, visionRange))
                {
                    Debug.Log("PLAYER DETECTED by direct player collider raycast!");
                    return true;
                }
            }

            // Fallback: distance + angle says visible, and no wall blocked it
            return true;
        }

        private void GameOver()
        {
            Debug.Log("GAME OVER: Player was seen for 5 seconds.");
            
        }

        private void UpdateSeenUI()
        {
            if (seenCountText != null)
                seenCountText.text = "Caught: " + timesPlayerSeen;
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