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
    public Transform PlayerTransform => player.transform;
    public Vector3 LastKnownPlayerPosition { get; private set; }

    // Cached once in Start() instead of via GetComponent<CharacterController>()
    // on every sight check - CanSeePlayer() runs every frame for every guard
    // in every state (Patrol/Alert/Attack), so this was previously the
    // single hottest GetComponent call in the whole game.
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

    // Sight checks (CanSeePlayer / IsPlayerInFieldOfViewCone) fire from
    // every state's Perform() every single frame and each one performs a
    // Physics.Raycast on the hot path. With one guard that's a few raycasts
    // a frame; with several guards (multi-guard levels) it adds up fast.
    // The player's position doesn't meaningfully change within ~100ms, so
    // we throttle the expensive checks to ~10 Hz and reuse the cached
    // result for any other query in the same window. The first call still
    // returns immediately so state transitions (spot the player) feel
    // instant on the frame they happen.
    private const float SightCheckInterval = 0.1f;
    private float nextSightCheckTime;
    private bool hasCachedSightResult;
    private bool cachedSeesPlayer;
    private bool cachedInWarningCone;

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
        AllGuards.Add(this);
    }

    private void OnDestroy()
    {
        AllGuards.Remove(this);
    }

    private void OnEnable() => SoundEmissionManager.OnSoundEmitted += HandleSound;
    private void OnDisable() => SoundEmissionManager.OnSoundEmitted -= HandleSound;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerController = player.GetComponent<CharacterController>();

        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponent<Animator>();

        stateMachine.Initialise();

        // GetComponents<AudioSource>() allocates a new array each call -
        // fetch it once and reuse instead of calling it three times.
        AudioSource[] audioSources = GetComponents<AudioSource>();
        footstepAudioSource = audioSources[0];
        speechAudioSource = audioSources[1];
        gunshotAudioSource = audioSources[1];
    }

    private BaseState lastLoggedState;

    private void Update()
    {
        // ToString() on the active state allocated a new string every single
        // frame for every guard, purely to feed the inspector-only debug
        // field below. Only recompute it when the state actually changes.
        if (stateMachine.activeState != lastLoggedState)
        {
            lastLoggedState = stateMachine.activeState;
            currentState = lastLoggedState != null ? lastLoggedState.ToString() : "None";
        }

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

    public void GunshotAudio()
    {
        gunshotAudioSource.pitch = Random.Range(minPitch, maxPitch);
        gunshotAudioSource.PlayOneShot(gunshotAudioClip);
    }


    // ---------------- FOOTSTEPS AUDIO ----------------

    public void OnFootstep()
    {
        footstepAudioSource.pitch = Random.Range(minPitch, maxPitch);
        footstepAudioSource.PlayOneShot(footstepClip);
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
        if (soundMemoryTimer <= 0f)
        {
            // Sound fully faded — clear the strength so the next emission
            // starts from zero and doesn't leak a stale fill rate.
            currentSoundStrength = 0f;
            return false;
        }

        float rate = currentSoundIsRunning ? runningFillRate : walkingFillRate;
        detection = Mathf.Clamp(detection + rate * currentSoundStrength * Time.deltaTime, 0f, maxDetection);
        SetSliderColor(Color.yellow);
        return true;
    }

    private void HandleSound(Vector3 soundPos, float volume, bool instantAlert)
    {
        // sqrMagnitude avoids a sqrt for what is purely a threshold check -
        // this runs on every player footstep / thrown item / noise-maker.
        float sqrDistance = (transform.position - soundPos).sqrMagnitude;
        if (sqrDistance > volume * volume) return;
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

    // TODO: MAKE A SEPERATE METHOD FOR OBJECTS YOU CAN THROW, 
    // HandleSoundForObject

    // ---------------- LASER / CAMERA ALARM ----------------
    // Called by SecurityCamera / LaserSecurityScript when the player trips them.
    // Instantly maxes detection and sends the guard straight into AlertState,
    // same as a maxed-out sound detection would.
    public void TriggerLaserAlarm(Vector3 position, LaserSecurityScript sourceLaser = null)
    {
        detection = maxDetection;
        LastKnownPlayerPosition = position;
        soundMemoryTimer = soundMemoryTime;
        SetSliderColor(Color.red);

        if (stateMachine != null)
        {
            // Don't re-alert if the guard is already mid-Attack; that state
            // already tracks/loses the player on its own terms.
            if (stateMachine.activeState is AttackState) return;

            AlertState alert = new AlertState();
            alert.lastKnownPosition = position;
            alert.sourceLaser = sourceLaser;
            stateMachine.ChangeState(alert);
        }
    }

        // World Space Canvases force a mesh rebuild every time you assign a
    // new value to a Slider / toggle a UI Image's active state, and this
    // Canvas is parented to the guard's head every level. Setting the
    // slider value once per frame (even with the same number) was the
    // largest single frame-time cost in playtests - it spiked the world
    // canvas rebuild from "on change" to "60 Hz". Only push the new
    // value when it actually moved, and round to the renderer's pixel
    // grid so the slider doesn't dirty itself on floating point drift.
    private float lastSliderValueSent = -1f;
    public void UpdateSliderUI()
    {
        if (detectionSlider == null) return;
        float normalized = detection / maxDetection;
        if (!Mathf.Approximately(normalized, lastSliderValueSent))
        {
            detectionSlider.value = normalized;
            lastSliderValueSent = normalized;
        }
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

        // Same reason as UpdateSliderUI: SetActive on a World Space Canvas
        // child forces a rebuild. Skip the call when the desired state
        // already matches.
        bool inWarningCone = IsPlayerInFieldOfViewCone();
        if (eyeIcon.gameObject.activeSelf != inWarningCone)
        {
            eyeIcon.gameObject.SetActive(inWarningCone);
        }

        //Debug.Log($"warningDist:{warningSightDistance} warningFOV:{warningFieldOfView} | sightDist:{sightDistance} FOV:{fieldOfView} | inWarningCone:{inWarningCone}");
    }

        public bool IsPlayerInFieldOfViewCone()
    {
        if (player == null) return false;

        // Throttle: sight checks (cone + raycast) are called from every state's
        // Perform() each frame and each raycast is one of the more expensive
        // physics calls. The player doesn't meaningfully move within ~100ms,
        // so reuse the cached cone result for any other query in the same
        // window. The first call still runs immediately so the very first
        // frame after entering a state sees fresh data.
        if (hasCachedSightResult && Time.time < nextSightCheckTime)
        {
            return cachedInWarningCone;
        }

        // sqrMagnitude avoids a sqrt for what's just a threshold check -
        // this runs every frame per guard, in every state.
        float sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance > warningSightDistance * warningSightDistance)
        {
            CacheSightResult(false, false);
            return false;
        }

        Vector3 rayOrigin = transform.position + (Vector3.up * eyeHeight);
        Vector3 targetPoint = PlayerBoundsCenter();
        Vector3 targetDirection = (targetPoint - rayOrigin).normalized;

        float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
        bool inCone = angleToPlayer <= warningFieldOfView;
        CacheSightResult(inCone, inCone ? cachedSeesPlayer : false);
        return inCone;
    }

    private void CacheSightResult(bool inCone, bool sees)
    {
        hasCachedSightResult = true;
        cachedInWarningCone = inCone;
        cachedSeesPlayer = sees;
        nextSightCheckTime = Time.time + SightCheckInterval;
    }

    private void InvalidateSightCache() => hasCachedSightResult = false;

    // ---------------- SIGHT RAYCAST (used for instant Attack trigger) ----------------

        public bool CanSeePlayer()
    {
        if (!IsPlayerInFieldOfViewCone()) return false;

        // Reuse the cached result if the throttled cone check ran this window
        // and we already have a confirmed sight result. A missed check still
        // gets the raycast, but a confirmed sight check skips the physics
        // raycast on subsequent calls inside the same window.
        if (hasCachedSightResult && Time.time < nextSightCheckTime)
        {
            if (cachedSeesPlayer)
            {
                LastKnownPlayerPosition = player.transform.position;
                return true;
            }
            return false;
        }

        Vector3 rayOrigin = transform.position + (Vector3.up * eyeHeight);
        Vector3 targetPoint = PlayerBoundsCenter();
        Vector3 targetDirection = (targetPoint - rayOrigin).normalized;

        Ray ray = new Ray(rayOrigin, targetDirection);
        RaycastHit hitInfo;

#if UNITY_EDITOR
        Debug.DrawRay(rayOrigin, targetDirection * sightDistance, Color.red);
#endif

        bool seesPlayer = false;
        if (Physics.Raycast(ray, out hitInfo, sightDistance))
        {
            if (hitInfo.transform.CompareTag("Player") || hitInfo.transform.root.gameObject == player)
            {
                LastKnownPlayerPosition = player.transform.position;
                seesPlayer = true;
            }
        }

        // Cache the combined cone+sight result so other sight queries in the
        // same throttle window skip the raycast entirely.
        CacheSightResult(cachedInWarningCone, seesPlayer);
        return seesPlayer;
    }

    // Anything that changes the guard's perception of the player should
    // invalidate the throttle cache so the next sight query re-evaluates
    // immediately instead of waiting up to SightCheckInterval.
    public void NotifyPlayerStateChanged() => InvalidateSightCache();

    // Cached CharacterController instead of a fresh GetComponent<CharacterController>()
    // call on every sight check.
    private Vector3 PlayerBoundsCenter()
    {
        return playerController != null ? playerController.bounds.center : player.transform.position;
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