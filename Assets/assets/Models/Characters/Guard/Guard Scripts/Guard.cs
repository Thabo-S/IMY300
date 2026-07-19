using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Guard : MonoBehaviour
{
    [Header("References")]
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private Animator animator;
    public AudioSource audioSource;
    public NavMeshAgent Agent { get => agent; }
    public Animator Animator { get => animator; }

    [SerializeField] private string currentState;
    public Path path;

    public GameObject player;
    public Transform PlayerTransform => player.transform;
    public Vector3 LastKnownPlayerPosition { get; private set; }

    public int currentWaypointIndex = 0;

    [Header("Sight")]
    public float sightDistance = 50f;
    public float fieldOfView = 30f;
    public float eyeHeight = 12f;
    public float catchRadius = 20f;
    public float wanderRadius = 15f;

    [Header("Warning cone (eye icon, should be larger)")]
    public float warningSightDistance = 65f;
    public float warningFieldOfView = 45f;

    [Header("Sound")]
    public float runningFillRate = 100f;
    public float walkingFillRate = 60f;
    public float soundMemoryTime = 0.3f;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 0.8f;
    public AudioClip footstepClip;  

    [Header("Detection Meter (sound only)")]
    public float detection = 0f;
    public float maxDetection = 100f;
    public float decayRate = 15f;
    public Slider detectionSlider;
    public Image detectionSliderFill;

    [Header("HUD")]
    public Image eyeIcon;

    [Header("Weapons")]
    public GameObject gun;
    public Transform gunBarrel;
    public float bulletSpeed = 80f;

    [Range(0.1f, 10f)]
    public float fireRate;

    private float soundMemoryTimer = 0f;
    private float currentSoundStrength = 0f;
    private bool currentSoundIsRunning = false;

    public static class AnimationParams
    {
        public const string Guard_Idle = "Guard_Idle";
        public const string Guard_Walk = "Guard_Walk";
        public const string Guard_Look_Around = "Guard_Look_Around";
        public const string Guard_Shooting = "Guard_Shooting";
    }

    private void OnEnable() => SoundEmissionManager.OnSoundEmitted += HandleSound;
    private void OnDisable() => SoundEmissionManager.OnSoundEmitted -= HandleSound;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponent<Animator>();

        stateMachine.Initialise();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        currentState = stateMachine.activeState.ToString();
        UpdateSliderUI();
        UpdateEyeIcon();
    }

    public void UpdateAnimationParameters(bool isWalking, bool isLookingAround, bool isShooting = false)
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isLookingAround", isLookingAround);
            animator.SetBool("isShooting", isShooting);
        }

    }


    // ---------------- FOOTSTEPS AUDIO ----------------

    public void OnFootstep()
    {
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(footstepClip);
    }

    public void PlayShootAnimation()
    {
        if (animator != null)
        {
            animator.Play("Guard_Shooting", 0, 0f);
        }
    }
    public void SetGunActive(bool isActive)
    {
        if (gun != null)
        {
            gun.SetActive(isActive);
        }
    }
    public bool TickDetection()
    {
        bool heard = UpdateSoundDetection();

        if (!heard)
        {
            detection = Mathf.Clamp(detection - decayRate * Time.deltaTime, 0f, maxDetection);
            if (detection <= 0f) SetSliderColor(Color.green);
        }

        if (detection >= maxDetection)
        {
            detection = 0f;
            return true;
        }

        return false;
    }

    private bool UpdateSoundDetection()
    {
        if (soundMemoryTimer <= 0f) return false;

        soundMemoryTimer -= Time.deltaTime;
        float rate = currentSoundIsRunning ? runningFillRate : walkingFillRate;
        detection = Mathf.Clamp(detection + rate * currentSoundStrength * Time.deltaTime, 0f, maxDetection);
        SetSliderColor(Color.yellow);

        if (soundMemoryTimer <= 0f) currentSoundStrength = 0f;
        return true;
    }

    private void HandleSound(Vector3 soundPos, float volume, bool instantAlert)
    {
        float distance = Vector3.Distance(transform.position, soundPos);
        if (distance > volume) return;

        if (instantAlert)
        {
            detection = maxDetection;
            LastKnownPlayerPosition = soundPos;
            soundMemoryTimer = soundMemoryTime;
            return;
        }

        float strength = Mathf.Clamp01(1f - (distance / volume));
        if (strength >= currentSoundStrength)
        {
            currentSoundStrength = strength;
            currentSoundIsRunning = volume >= 60f;
        }

        LastKnownPlayerPosition = soundPos;
        soundMemoryTimer = soundMemoryTime;
    }

    // TODO: MAKE A SEPERATE METHOD FOR OBJECTS YOU CAN THROW, 
    // HandleSoundForObject

    public void UpdateSliderUI()
    {
        if (detectionSlider != null)
            detectionSlider.value = detection / maxDetection;

    }

    public void SetSliderColor(Color color)
    {
        if (detectionSliderFill != null)
            detectionSliderFill.color = color;
    }

    // ---------------- EYE ICON (warning indicator) ----------------

    private void UpdateEyeIcon()
    {
        if (eyeIcon == null) return;

        bool inWarningCone = IsPlayerInFieldOfViewCone();
        eyeIcon.gameObject.SetActive(inWarningCone);

        //Debug.Log($"warningDist:{warningSightDistance} warningFOV:{warningFieldOfView} | sightDist:{sightDistance} FOV:{fieldOfView} | inWarningCone:{inWarningCone}");
    }

    public bool IsPlayerInFieldOfViewCone()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > warningSightDistance) return false;

        Vector3 rayOrigin = transform.position + (Vector3.up * eyeHeight);
        Vector3 targetPoint = player.transform.position + (Vector3.up * 6.54f);
        Vector3 targetDirection = (targetPoint - rayOrigin).normalized;

        float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
        return angleToPlayer <= warningFieldOfView;
    }

    // ---------------- SIGHT RAYCAST (used for instant Attack trigger) ----------------

    public bool CanSeePlayer()
    {
        if (!IsPlayerInFieldOfViewCone()) return false;

        Vector3 rayOrigin = transform.position + (Vector3.up * eyeHeight);
        Vector3 targetPoint = player.transform.position + (Vector3.up * 6.54f);
        Vector3 targetDirection = (targetPoint - rayOrigin).normalized;

        Ray ray = new Ray(rayOrigin, targetDirection);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, sightDistance))
        {
            if (hitInfo.transform.gameObject == player || hitInfo.transform.root.gameObject == player)
            {
                LastKnownPlayerPosition = player.transform.position;
                return true;
            }
        }

        return false;
    }

    public void ResetGuard()
    {
        agent.isStopped = true;

        detection = 0f;
        UpdateSliderUI();
        SetSliderColor(Color.green);

        stateMachine.ChangeState(new PatrolState());

        agent.isStopped = false;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (stateMachine == null) return;

        AlertState alert = stateMachine.activeState as AlertState;
        if (alert == null) return;

        // Search radius (yellow wire circle around the search origin)
        Gizmos.color = Color.yellow;
        Vector3 origin = alert.HasArrived ? alert.SearchOrigin : alert.lastKnownPosition;
        DrawWireCircle(origin, catchRadius);

        // Last known / search origin point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(alert.lastKnownPosition, 0.5f);

        // Current wander target, if searching
        if (alert.HasArrived && alert.CurrentWanderTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(alert.CurrentWanderTarget, 0.4f);
            Gizmos.DrawLine(transform.position, alert.CurrentWanderTarget);
        }
    }

    private void DrawWireCircle(Vector3 center, float radius, int segments = 40)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
}