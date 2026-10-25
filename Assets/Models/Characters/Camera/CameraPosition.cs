using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    private Vector3 topDownPos = new Vector3(0f, 0.614f, -0.08f);
    private Vector3 topDownRot = new Vector3(0f, 0f, 0f);

    private Vector3 crouchTopDownPos = new Vector3(0f, 0.165f, 0f - 0.08f);
    private Vector3 crouchTopDownRot = new Vector3(20f, 0f, 0f);

    private bool isCrouching = false;
    private Transform player;
    public float headPosition = 0.614f;

    private SkinnedMeshRenderer playerMeshRenderer;

    [Header("Crouch Smoothing")]
    [Tooltip("Approx. time (seconds) for the camera to reach its new height. Lower = snappier.")]
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Head Bob")]
    [SerializeField] private bool enableHeadBob = true;
    [Tooltip("Up/down movement per step (local units).")]
    [SerializeField] private float verticalAmplitude = 0.02f;
    [Tooltip("Side-to-side sway (local units). Usually smaller than vertical.")]
    [SerializeField] private float horizontalAmplitude = 0.012f;
    [Tooltip("How fast the bob cycles per unit of movement speed. Higher = quicker steps.")]
    [SerializeField] private float bobFrequency = 2.5f;
    [Tooltip("Speed that counts as a normal walk. Faster than this bobs harder (sprint).")]
    [SerializeField] private float referenceSpeed = 2.2f;
    [SerializeField] private float maxSpeedScale = 1.5f;
    [Tooltip("Bob strength while crouched (1 = same as standing).")]
    [SerializeField] private float crouchBobMultiplier = 0.5f;
    [Tooltip("Ignore movement slower than this (stops bob when pushing against walls).")]
    [SerializeField] private float minMoveSpeed = 0.1f;
    [Tooltip("How quickly the bob fades in/out when you start or stop moving.")]
    [SerializeField] private float bobBlendSpeed = 8f;

    private Vector3 positionVelocity = Vector3.zero;
    private Vector3 basePosition;

    private CharacterController controller;
    private Vector3 lastPlayerPos;
    private float horizontalSpeed;
    private bool grounded;

    private float bobTimer;
    private float bobBlend;

    void Start()
    {
        player = transform.parent;
        controller = GetComponentInParent<CharacterController>();

        if (controller != null)
        {
            lastPlayerPos = controller.transform.position;
        }

        // Find the player mesh by tag
        GameObject meshObj = GameObject.FindGameObjectWithTag("PlayerMesh");
        if (meshObj != null)
        {
            playerMeshRenderer = meshObj.GetComponent<SkinnedMeshRenderer>();
        }

        if (playerMeshRenderer != null)
        {
            playerMeshRenderer.enabled = false;
        }

        // Start exactly at the correct position (no smoothing on spawn)
        SnapCameraTransform();
    }

    // Movement is applied in FixedUpdate (via InputManager), so the player's
    // real speed is measured here. Measuring in Update would read 0 on frames
    // between physics steps and make the bob stutter.
    void FixedUpdate()
    {
        if (controller == null) return;

        Vector3 current = controller.transform.position;
        Vector3 delta = current - lastPlayerPos;
        delta.y = 0f;

        horizontalSpeed = delta.magnitude / Time.fixedDeltaTime;
        grounded = controller.isGrounded;
        lastPlayerPos = current;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SnapCameraTransform();
        }

        if (PauseMenu.isGamePause) return;

        SmoothCameraPosition();
        ApplyHeadBob();
    }

    public void ToggleCrouchView(bool crouchingState)
    {
        // Only record the new state - Update() eases the camera toward it.
        isCrouching = crouchingState;
    }

    private void SmoothCameraPosition()
    {
        Vector3 targetPos = isCrouching ? crouchTopDownPos : topDownPos;

        basePosition = Vector3.SmoothDamp(
            basePosition,
            targetPos,
            ref positionVelocity,
            smoothTime);
    }

    private void ApplyHeadBob()
    {
        Vector3 offset = Vector3.zero;

        if (enableHeadBob)
        {
            bool isMoving = grounded && horizontalSpeed > minMoveSpeed;

            // Fade the bob in and out so it never pops on/off
            float targetBlend = isMoving ? 1f : 0f;
            bobBlend = Mathf.MoveTowards(bobBlend, targetBlend, bobBlendSpeed * Time.deltaTime);

            if (bobBlend > 0f)
            {
                // Cycle speed follows how fast the player is really moving
                bobTimer += horizontalSpeed * bobFrequency * Time.deltaTime;

                float speedScale = Mathf.Clamp(horizontalSpeed / referenceSpeed, 0f, maxSpeedScale);
                float crouchScale = isCrouching ? crouchBobMultiplier : 1f;
                float strength = bobBlend * speedScale * crouchScale;

                // Vertical bobs twice per full cycle (one dip per footstep),
                // sideways sway once per cycle (left foot / right foot)
                float vertical = Mathf.Sin(bobTimer * 2f) * verticalAmplitude * strength;
                float horizontal = Mathf.Sin(bobTimer) * horizontalAmplitude * strength;

                offset = new Vector3(horizontal, vertical, 0f);
            }
        }

        transform.localPosition = basePosition + offset;
    }

    // Instantly places the camera at the current state's position.
    private void SnapCameraTransform()
    {
        positionVelocity = Vector3.zero;
        bobTimer = 0f;
        bobBlend = 0f;

        if (isCrouching)
        {
            basePosition = crouchTopDownPos;
            transform.localRotation = Quaternion.Euler(crouchTopDownRot);
        }
        else
        {
            basePosition = topDownPos;
            transform.localRotation = Quaternion.Euler(topDownRot);
        }

        transform.localPosition = basePosition;
    }
}