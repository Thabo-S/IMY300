

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Guard : MonoBehaviour
{
    public enum GuardVocalState { Patrolling, Alerted, Spotted }
    public GuardVocalState CurrentVocalState { get; private set; } = GuardVocalState.Patrolling;
    public void SetVocalState(GuardVocalState state) => CurrentVocalState = state;

    [Header("References")]
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private Animator animator;

    public AudioSource footstepAudioSource;
    public AudioSource speechAudioSource;
    public AudioSource gunshotAudioSource;

    public NavMeshAgent Agent { get => agent; }
    public Animator Animator { get => animator; }

    [SerializeField] private string currentState;
    public Path path;

    public GameObject player;
    public Transform PlayerTransform => player != null ? player.transform : null;
    public Vector3 LastKnownPlayerPosition { get; private set; }

    private CharacterController playerController;
    public CharacterController PlayerController => playerController;

    public int currentWaypointIndex = 0;

    [Header("Sight")]
    public float sightDistance = 8f;
    public float fieldOfView = 15f;
    public float eyeHeight = 0.7f;
    public float catchRadius = 10f;
    public float wanderRadius = 10f;

    [Header("Warning cone (eye icon, should be larger)")]
    public float warningSightDistance = 12f;
    public float warningFieldOfView = 25f;

    [Header("Sound")]
    public float runningFillRate = 100f;
    public float walkingFillRate = 60f;
    public float soundMemoryTime = 0.3f;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;
    public AudioClip footstepClip;
    public AudioClip gunshotAudioClip;

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
    public float bulletSpeed = 10f;

    [Range(0.1f, 10f)]
    public float fireRate;

    private float soundMemoryTimer = 0f;
    private float currentSoundStrength = 0f;
    private bool currentSoundIsRunning = false;

    public float idleFacingYRotation = 90f;

    // ------------------------------------------------------------
    // PERFORMANCE SETTINGS
    // ------------------------------------------------------------

    // Perception does not need to run every rendered frame.
    // One complete perception pass every 0.10 seconds is 10 checks/sec.
    private const float SightCheckInterval = 0.10f;

    // The world-space detection UI also does not need to be rebuilt
    // every rendered frame.
    private const float DetectionUIInterval = 0.10f;

    private float nextSightCheckTime;
    private bool hasCachedSightResult;
    private bool cachedSeesPlayer;
    private bool cachedInWarningCone;

    private float nextDetectionUIUpdateTime;

    // Only update the inspector state string when the state changes.
    private BaseState lastLoggedState;

    // Avoid repeated Animator.SetBool calls when values have not changed.
    private bool lastWalking;
    private bool lastLookingAround;
    private bool lastShooting;
    private bool animationStateInitialized;

    // Avoid repeated slider/color writes.
    private float lastSliderValueSent = -1f;
    private Color lastSliderColor;
    private bool sliderColorInitialized;

    // Avoid repeated eye icon SetActive calls.
    private bool lastEyeIconState;
    private bool eyeIconStateInitialized;

    public static class AnimationParams
    {
        public const string Guard_Idle = "Guard_Idle";
        public const string Guard_Walk = "Guard_Walk";
        public const string Guard_Look_Around = "Guard_Look_Around";
        public const string Guard_Shooting = "Guard_Shooting";
    }

    public static readonly List<Guard> AllGuards = new List<Guard>();

    private void Awake()
    {
        if (!AllGuards.Contains(this))
            AllGuards.Add(this);
    }

    private void OnDestroy()
    {
        AllGuards.Remove(this);
    }

    private void OnEnable()
    {
        SoundEmissionManager.OnSoundEmitted += HandleSound;
    }

    private void OnDisable()
    {
        SoundEmissionManager.OnSoundEmitted -= HandleSound;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerController = player.GetComponent<CharacterController>();

        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (stateMachine != null)
            stateMachine.Initialise();

        // Get all AudioSources once.
        AudioSource[] audioSources = GetComponents<AudioSource>();

        if (audioSources.Length > 0)
            footstepAudioSource = audioSources[0];

        if (audioSources.Length > 1)
            speechAudioSource = audioSources[1];

        // Preserve the existing behaviour from the supplied script:
        // gunshotAudioSource uses the second AudioSource.
        if (audioSources.Length > 1)
            gunshotAudioSource = audioSources[1];

        // Make the first perception request happen immediately.
        nextSightCheckTime = Time.time;

        // Force the first UI update.
        nextDetectionUIUpdateTime = Time.time;
    }

    private void Update()
    {
        UpdateStateDebugString();

        // Detection itself still updates every frame so gameplay timing
        // remains smooth. Only the expensive UI refresh is throttled.
        UpdateDetection();

        if (Time.time >= nextDetectionUIUpdateTime)
        {
            nextDetectionUIUpdateTime = Time.time + DetectionUIInterval;
            UpdateSliderUI();
            UpdateEyeIcon();
        }
    }

    private void UpdateStateDebugString()
    {
        if (stateMachine == null)
            return;

        BaseState activeState = stateMachine.activeState;

        if (activeState != lastLoggedState)
        {
            lastLoggedState = activeState;
            currentState = lastLoggedState != null
                ? lastLoggedState.ToString()
                : "None";
        }
    }

    private void UpdateDetection()
    {
        bool heard = UpdateSoundDetection();

        if (!heard)
        {
            detection = Mathf.Clamp(
                detection - decayRate * Time.deltaTime,
                0f,
                maxDetection
            );

            if (detection <= 0f)
                SetSliderColor(Color.green);
        }
    }

    public void UpdateAnimationParameters(
        bool isWalking,
        bool isLookingAround,
        bool isShooting = false)
    {
        if (animator == null)
            return;

        // Do not call Animator.SetBool if the value has not changed.
        if (!animationStateInitialized ||
            lastWalking != isWalking)
        {
            animator.SetBool("isWalking", isWalking);
            lastWalking = isWalking;
        }

        if (!animationStateInitialized ||
            lastLookingAround != isLookingAround)
        {
            animator.SetBool("isLookingAround", isLookingAround);
            lastLookingAround = isLookingAround;
        }

        if (!animationStateInitialized ||
            lastShooting != isShooting)
        {
            animator.SetBool("isShooting", isShooting);
            lastShooting = isShooting;
        }

        animationStateInitialized = true;
    }

    public void GunshotAudio()
    {
        if (gunshotAudioSource == null || gunshotAudioClip == null)
            return;

        gunshotAudioSource.pitch = Random.Range(minPitch, maxPitch);
        gunshotAudioSource.PlayOneShot(gunshotAudioClip);
    }

    // ---------------- FOOTSTEPS AUDIO ----------------

    public void OnFootstep()
    {
        if (footstepAudioSource == null || footstepClip == null)
            return;

        footstepAudioSource.pitch = Random.Range(minPitch, maxPitch);
        footstepAudioSource.PlayOneShot(footstepClip);
    }

    public void PlayShootAnimation()
    {
        if (animator != null)
            animator.Play("Guard_Shooting", 0, 0f);
    }

    public void SetGunActive(bool isActive)
    {
        if (gun != null && gun.activeSelf != isActive)
            gun.SetActive(isActive);
    }

    // ------------------------------------------------------------
    // SOUND DETECTION
    // ------------------------------------------------------------

    public bool TickDetection()
    {
        bool heard = UpdateSoundDetection();

        if (!heard)
        {
            detection = Mathf.Clamp(
                detection - decayRate * Time.deltaTime,
                0f,
                maxDetection
            );

            if (detection <= 0f)
                SetSliderColor(Color.green);
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
        if (soundMemoryTimer <= 0f)
            return false;

        soundMemoryTimer -= Time.deltaTime;

        if (soundMemoryTimer <= 0f)
        {
            currentSoundStrength = 0f;
            return false;
        }

        float rate = currentSoundIsRunning
            ? runningFillRate
            : walkingFillRate;

        detection = Mathf.Clamp(
            detection + rate * currentSoundStrength * Time.deltaTime,
            0f,
            maxDetection
        );

        SetSliderColor(Color.yellow);
        return true;
    }

    private void HandleSound(Vector3 soundPos, float volume, bool instantAlert)
    {
        if (volume <= 0f)
            return;

        Vector3 offset = transform.position - soundPos;
        float sqrDistance = offset.sqrMagnitude;
        float sqrVolume = volume * volume;

        if (sqrDistance > sqrVolume)
            return;

        float distance = Mathf.Sqrt(sqrDistance);

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

    // ------------------------------------------------------------
    // LASER / CAMERA ALARM
    // ------------------------------------------------------------

    public void TriggerLaserAlarm(
        Vector3 position,
        LaserSecurityScript sourceLaser = null)
    {
        detection = maxDetection;
        LastKnownPlayerPosition = position;
        soundMemoryTimer = soundMemoryTime;

        SetSliderColor(Color.red);

        if (stateMachine == null)
            return;

        if (stateMachine.activeState is AttackState)
            return;

        AlertState alert = new AlertState();
        alert.lastKnownPosition = position;
        alert.sourceLaser = sourceLaser;
        stateMachine.ChangeState(alert);
    }

    // ------------------------------------------------------------
    // DETECTION UI
    // ------------------------------------------------------------

    public void UpdateSliderUI()
    {
        if (detectionSlider == null)
            return;

        if (maxDetection <= 0f)
            return;

        float normalized = detection / maxDetection;

        // Avoid touching the Canvas when the value has not meaningfully changed.
        if (lastSliderValueSent < 0f ||
            Mathf.Abs(normalized - lastSliderValueSent) > 0.005f)
        {
            detectionSlider.value = normalized;
            lastSliderValueSent = normalized;
        }
    }

    public void SetSliderColor(Color color)
    {
        if (detectionSliderFill == null)
            return;

        if (!sliderColorInitialized ||
            lastSliderColor != color)
        {
            detectionSliderFill.color = color;
            lastSliderColor = color;
            sliderColorInitialized = true;
        }
    }

    // ------------------------------------------------------------
    // EYE ICON
    // ------------------------------------------------------------

    private void UpdateEyeIcon()
    {
        if (eyeIcon == null)
            return;

        bool inWarningCone = IsPlayerInFieldOfViewCone();

        if (!eyeIconStateInitialized ||
            lastEyeIconState != inWarningCone)
        {
            eyeIcon.gameObject.SetActive(inWarningCone);
            lastEyeIconState = inWarningCone;
            eyeIconStateInitialized = true;
        }
    }

    // ------------------------------------------------------------
    // SIGHT / PERCEPTION
    // ------------------------------------------------------------

    public bool IsPlayerInFieldOfViewCone()
    {
        EnsureSightCache();

        return cachedInWarningCone;
    }

    public bool CanSeePlayer()
    {
        EnsureSightCache();

        if (cachedSeesPlayer && player != null)
            LastKnownPlayerPosition = player.transform.position;

        return cachedSeesPlayer;
    }

    private void EnsureSightCache()
    {
        if (player == null)
        {
            cachedInWarningCone = false;
            cachedSeesPlayer = false;
            hasCachedSightResult = true;
            nextSightCheckTime = Time.time + SightCheckInterval;
            return;
        }

        // Reuse the complete result until the next perception tick.
        if (hasCachedSightResult &&
            Time.time < nextSightCheckTime)
        {
            return;
        }

        PerformSightCheck();
    }

    private void PerformSightCheck()
    {
        hasCachedSightResult = true;
        nextSightCheckTime = Time.time + SightCheckInterval;

        Vector3 guardPosition = transform.position;
        Vector3 playerPosition = player.transform.position;

        Vector3 offset = playerPosition - guardPosition;
        float sqrDistance = offset.sqrMagnitude;

        float warningDistanceSqr =
            warningSightDistance * warningSightDistance;

        // Outside the warning range.
        if (sqrDistance > warningDistanceSqr)
        {
            cachedInWarningCone = false;
            cachedSeesPlayer = false;
            return;
        }

        Vector3 rayOrigin =
            guardPosition + (Vector3.up * eyeHeight);

        Vector3 targetPoint = PlayerBoundsCenter();
        Vector3 targetDirection = targetPoint - rayOrigin;

        if (targetDirection.sqrMagnitude <= 0.0001f)
        {
            cachedInWarningCone = true;
            cachedSeesPlayer = true;
            LastKnownPlayerPosition = playerPosition;
            return;
        }

        targetDirection.Normalize();

        // Vector3.Angle is relatively expensive and requires acos.
        // Compare the dot product instead.
        float dot = Vector3.Dot(
            targetDirection,
            transform.forward
        );

        float warningCos =
            Mathf.Cos(warningFieldOfView * Mathf.Deg2Rad);

        cachedInWarningCone = dot >= warningCos;

        if (!cachedInWarningCone)
        {
            cachedSeesPlayer = false;
            return;
        }

        // Only perform the physics raycast when the player is actually
        // inside the warning cone.
        RaycastHit hitInfo;

#if UNITY_EDITOR
        Debug.DrawRay(
            rayOrigin,
            targetDirection * sightDistance,
            Color.red
        );
#endif

        if (Physics.Raycast(
            rayOrigin,
            targetDirection,
            out hitInfo,
            sightDistance))
        {
            if (hitInfo.transform.CompareTag("Player") ||
                hitInfo.transform.root.gameObject == player)
            {
                cachedSeesPlayer = true;
                LastKnownPlayerPosition = playerPosition;
                return;
            }
        }

        cachedSeesPlayer = false;
    }

    private void InvalidateSightCache()
    {
        hasCachedSightResult = false;
        nextSightCheckTime = Time.time;
    }

    public void NotifyPlayerStateChanged()
    {
        InvalidateSightCache();
    }

    // ------------------------------------------------------------
    // PLAYER BOUNDS
    // ------------------------------------------------------------

    private Vector3 PlayerBoundsCenter()
    {
        if (playerController != null)
            return playerController.bounds.center;

        return player.transform.position;
    }

    // ------------------------------------------------------------
    // DEBUG GIZMOS
    // ------------------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (stateMachine == null)
            return;

        AlertState alert = stateMachine.activeState as AlertState;

        if (alert == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3 origin =
            alert.HasArrived
                ? alert.SearchOrigin
                : alert.lastKnownPosition;

        DrawWireCircle(origin, catchRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(alert.lastKnownPosition, 0.5f);

        if (alert.HasArrived &&
            alert.CurrentWanderTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(
                alert.CurrentWanderTarget,
                0.4f
            );

            Gizmos.DrawLine(
                transform.position,
                alert.CurrentWanderTarget
            );
        }
    }

    private void DrawWireCircle(
        Vector3 center,
        float radius,
        int segments = 40)
    {
        float angleStep = 360f / segments;

        Vector3 prevPoint =
            center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle =
                angleStep * i * Mathf.Deg2Rad;

            Vector3 newPoint =
                center +
                new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
}
