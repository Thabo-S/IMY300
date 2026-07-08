using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GuardScript : MonoBehaviour
{
    private GameObject player;
    public float sightDistance = 50f;
    public float fieldOfView = 30f;
    public float eyeHeight = 12f;
    public float waitAtWaypoint = 2f;
    public float gameOverSightTime = 3f;

    public Path path;

    public GameObject gameOverUI;

    private NavMeshAgent agent;
    private Animator animator;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float playerSpottedTimer = 0f;

    public float guardSpeed = 10f;

    private void OnEnable() => SoundEmissionManager.OnSoundEmitted += HandleSound;
    private void OnDisable() => SoundEmissionManager.OnSoundEmitted -= HandleSound;

    private void HandleSound(Vector3 soundPos, float volume)
    {
        float distance = Vector3.Distance(transform.position, soundPos);
        //Debug.Log($"[Guard] Heard sound event. Distance: {distance}, Volume: {volume}");


        if (distance <= volume)
        {
            if (volume > 60f)
                Debug.Log("Running sound heard! Distance: " + distance);
            else
                Debug.Log("Walking sound heard! Distance: " + distance);
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            animator = GetComponent<Animator>();

        agent.speed = guardSpeed;

        if (path != null && path.waypoints.Count > 0)
            agent.SetDestination(path.waypoints[0].position);
    }
    void Update()
    {
        HandleSight();
        HandlePatrol();
        UpdateAnimations();
    }

    private void HandlePatrol()
    {
        if (isWaiting || path == null || path.waypoints.Count == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        if (animator != null) animator.SetBool("isWalking", false);
        if (animator != null) animator.SetBool("isLookingAround", true);

        yield return new WaitForSeconds(waitAtWaypoint);

        if (animator != null) animator.SetBool("isLookingAround", false);

        currentWaypointIndex = (currentWaypointIndex + 1) % path.waypoints.Count;
        agent.SetDestination(path.waypoints[currentWaypointIndex].position);

        isWaiting = false;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isMoving && !isWaiting);
    }

    private void HandleSight()
    {
        if (canSeePlayer())
        {
            playerSpottedTimer += Time.deltaTime;

            if (playerSpottedTimer >= gameOverSightTime)
            {
                if (gameOverUI != null)
                    gameOverUI.SetActive(true);

                StartCoroutine(ReloadScene());
                playerSpottedTimer = 0f;
            }
        }
        else
        {
            playerSpottedTimer = 0f;
        }
    }

    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool canSeePlayer()
    {
        if (player == null) return false;

        if (Vector3.Distance(transform.position, player.transform.position) < sightDistance)
        {
            Vector3 rayOrigin = transform.position + (Vector3.up * eyeHeight);
            Vector3 targetPoint = player.transform.position + (Vector3.up * 6.54f); // center of character controller
            Vector3 targetDirection = (targetPoint - rayOrigin).normalized;

            float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);

            if (angleToPlayer <= fieldOfView)
            {
                Ray ray = new Ray(rayOrigin, targetDirection);
                RaycastHit hitInfo;

                Debug.DrawRay(ray.origin, ray.direction * sightDistance, Color.red);

                if (Physics.Raycast(ray, out hitInfo, sightDistance))
                {
                    if (hitInfo.transform.gameObject == player || hitInfo.transform.root.gameObject == player)
                        return true;
                }

            }
        }

        return false;
    }


}