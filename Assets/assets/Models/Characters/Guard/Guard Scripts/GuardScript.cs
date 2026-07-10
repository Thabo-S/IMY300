using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GuardState
{
    Patrolling,
    Investigating
}

public class GuardScript : MonoBehaviour
{
    private GameObject player;

    [Header("Sight")]
    public float sightDistance = 50f;
    public float fieldOfView = 30f;
    public float eyeHeight = 12f;
    public float sightFillRate = 80f; // detection points per second while directly seen
    public float catchRadius = 50f;    // how close counts as "found" at the investigate spot

    [Header("Patrol Settings")]
    public Path path;
    public float waitAtWaypoint = 2f;
    public float guardSpeed = 10f;
    public float investigateSpeed = 16f;

    [Header("Sound")]
    public float runningFillRate = 100f;  // detection points per second, closest/loudest running sound
    public float walkingFillRate = 60f;  // detection points per second, closest/loudest walking sound
    public float soundMemoryTime = 0.3f; // how long a sound keeps filling after last heard (covers gaps between footstep events)

    [Header("Detection Meter")]
    public float detection = 0f;
    public float maxDetection = 100f;
    public float decayRate = 15f; // per second, when no sound/sight this frame
    public Slider detectionSlider;
    public Image detectionSliderFill; // optional, drag the Slider's Fill image here for color feedback

    [Header("Game Over")]
    public GameObject gameOverUI;

    private NavMeshAgent agent;
    public NavMeshAgent Agent { get => agent;  }
    private Animator animator;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    private GuardState currentState = GuardState.Patrolling;
    [Header("Debug")]
    [SerializeField] private Vector3 lastKnownPos;

    // Sound tracking (for continuous fill between discrete sound events)
    private float soundMemoryTimer = 0f;
    private float currentSoundStrength = 0f; // 0-1, based on distance
    private bool currentSoundIsRunning = false;

    private void OnEnable() => SoundEmissionManager.OnSoundEmitted += HandleSound;
    private void OnDisable() => SoundEmissionManager.OnSoundEmitted -= HandleSound;

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
        bool seenThisFrame = UpdateSightDetection();
        bool heardThisFrame = UpdateSoundDetection();
        UpdateDetectionDecay(seenThisFrame || heardThisFrame);
        UpdateSliderUI();

        switch (currentState)
        {
            case GuardState.Patrolling:
                HandlePatrol();
                break;

            case GuardState.Investigating:
                HandleInvestigate();
                break;
        }

        UpdateAnimations();
    }

    // ---------------- SOUND ----------------

    // Called whenever the player emits a sound event (e.g. footstep). Just records
    // the stimulus; the actual continuous fill happens in UpdateSoundDetection().
    private void HandleSound(Vector3 soundPos, float volume)
    {
        if (currentState == GuardState.Investigating) return;

        float distance = Vector3.Distance(transform.position, soundPos);

        if (distance <= volume)
        {
            float strength = Mathf.Clamp01(1f - (distance / volume));

            // Keep the strongest signal if multiple sounds overlap
            if (strength >= currentSoundStrength)
            {
                currentSoundStrength = strength;
                currentSoundIsRunning = volume > 60f;
            }

            lastKnownPos = soundPos;
            soundMemoryTimer = soundMemoryTime; // refresh the "still hearing it" window
        }
    }

    private bool UpdateSoundDetection()
    {
        if (currentState == GuardState.Investigating) return false;

        if (soundMemoryTimer > 0f)
        {
            soundMemoryTimer -= Time.deltaTime;

            float rate = currentSoundIsRunning ? runningFillRate : walkingFillRate;
            detection = Mathf.Clamp(detection + rate * currentSoundStrength * Time.deltaTime, 0f, maxDetection);

            SetSliderColor(Color.yellow);

            if (detection >= maxDetection)
                EnterInvestigate();

            if (soundMemoryTimer <= 0f)
                currentSoundStrength = 0f; // fully faded, next event starts fresh

            return true;
        }

        return false;
    }

    // ---------------- SIGHT ----------------

    private bool UpdateSightDetection()
    {
        if (currentState == GuardState.Investigating) return false;

        if (CanSeePlayer())
        {
            detection = Mathf.Clamp(detection + sightFillRate * Time.deltaTime, 0f, maxDetection);
            lastKnownPos = player.transform.position; // sight overrides sound as most recent info

            SetSliderColor(Color.red);

            if (detection >= maxDetection)
                EnterInvestigate();

            return true;
        }

        return false;
    }

    // ---------------- DECAY / UI ----------------

    private void UpdateDetectionDecay(bool stimulusThisFrame)
    {
        if (currentState != GuardState.Patrolling) return;
        if (stimulusThisFrame) return;

        detection = Mathf.Clamp(detection - decayRate * Time.deltaTime, 0f, maxDetection);

        if (detection <= 0f)
            SetSliderColor(Color.green);
    }

    private void UpdateSliderUI()
    {
        if (detectionSlider != null)
            detectionSlider.value = detection / maxDetection;
    }

    private void SetSliderColor(Color color)
    {
        if (detectionSliderFill != null)
            detectionSliderFill.color = color;
    }

    // ---------------- STATES ----------------

    private void EnterInvestigate()
    {
        currentState = GuardState.Investigating;
        isWaiting = false;
        agent.speed = investigateSpeed;
        agent.SetDestination(lastKnownPos);
    }

    private void HandleInvestigate()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            CheckForPlayerAtSpot();
        }
    }

    private void CheckForPlayerAtSpot()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= catchRadius && CanSeePlayer())
        {
            Debug.Log("Player found");
            GameOver();
        }
        else
        {
            detection = 0f;
            currentSoundStrength = 0f;
            soundMemoryTimer = 0f;
            currentState = GuardState.Patrolling;
            agent.speed = guardSpeed;
            SetSliderColor(Color.green);

            if (path != null && path.waypoints.Count > 0)
                agent.SetDestination(path.waypoints[currentWaypointIndex].position);
        }
    }

    // ---------------- PATROL ----------------

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

    // ---------------- ANIMATION ----------------

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isMoving && !isWaiting);
    }

    // ---------------- GAME OVER ----------------

    private void GameOver()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        StartCoroutine(ReloadScene());
    }

    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ---------------- SIGHT RAYCAST ----------------

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        if (Vector3.Distance(transform.position, player.transform.position) < sightDistance)
        {
            Vector3 rayOrigin = transform.position + (Vector3.up * eyeHeight);
            Vector3 targetPoint = player.transform.position + (Vector3.up * 6.54f);
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