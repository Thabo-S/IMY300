using NUnit.Framework.Constraints;
using System;
using UnityEngine;
using static PlayerMovement;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Reference")]
    private CharacterController characterController;
    private Animator animator;
    private CameraPosition cameraScript;
    public Player playerScript;


    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool isCrouching = false;

    [Header("Movement Settings")]
    public float gravity = -9.8f;
    public float speed = 2.2f;
    public float walkSpeed = 2.2f;
    public float sprintSpeed = 3.4f;
    public float sneakSpeed = 1f;
    public float jumpHeight = 0.56f;

    [Header("Crouch Dimensions")]
    private float standingHeight;
    private Vector3 standingCenter;
    private float crouchHeight = 1.33f;
    private Vector3 crouchCenter = new Vector3(0f, -0.31f, -0.04f);


    [SerializeField] private float jumpDelay = 0.71f;

    [Header("Sound Emission")]
    public float walkVolume = 30f;
    public float runVolume = 60f;
    public float soundEmitInterval = 0.5f;
    private float soundTimer = 0f;

    private Vector3 currentVelocity = Vector3.zero;
    public static class AnimationParams
    {
        public const string IsWalking = "isWalking";
        public const string IsSprinting = "isRunning";
        public const string IsCrouching = "isCrouching";
        public const string IsSneaking = "isSneaking";
        public const string JumpTrigger = "Jump";
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();

        standingHeight = characterController.height;
        standingCenter = characterController.center;

        cameraScript = GetComponentInChildren<CameraPosition>();

        playerScript = GetComponent<Player>();

    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = characterController.isGrounded;
        EmitMovementSound();
    }




    // SO THIS FUNCTION BASCIALLY GETS THE INPUTS FROM THE INPUT MANAGER 
    // AND APPLIES THEM TO THE CHARACTER CONTROLLER TO MOVE THE PLAYER
    public void CalculatePlayerMovement(Vector2 movementInput)
    {
        if (PauseMenu.isGamePause) return;

        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        move = transform.TransformDirection(move);
        characterController.Move(move * speed * Time.deltaTime);

        currentVelocity = move * speed;

        // 2. Gravity Logic
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // Keeps player glued to slopes
        }

        playerVelocity.y += gravity * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);

        UpdateAnimations();
    }
    public void UpdateAnimations()
    {
        bool isMoving = currentVelocity.magnitude > 0.1f;

        if (animator != null)
        {
            // "isCrouching" should only be handled in the playerCrouch() function 
            // to tell the animator you are in the crouch STATE.

            if (!isCrouching)
            {
                animator.SetBool(AnimationParams.IsWalking, isMoving);
                animator.SetBool(AnimationParams.IsSneaking, false);
            }
            else
            {
                // When crouching, we don't change the "isCrouching" bool here.
                // Instead, we tell the animator if we are MOVING while crouching.
                animator.SetBool(AnimationParams.IsSneaking, isMoving);
                animator.SetBool(AnimationParams.IsWalking, false);
            }
        }
    }

    // THE PLAYER CAN ONLY JUMP IF THEY ARE ONLY GROUNDED
    // THIS BASICALLY PREVENTS DOUBLE JUMPING
    public void PlayerJump()
    {
        if (PauseMenu.isGamePause) return;

        // Check if grounded and not crouching
        if (isGrounded && !isCrouching)
        {
            StartCoroutine(JumpWithDelay());
        }
    }

    private IEnumerator JumpWithDelay()
    {
        if (animator != null)
        {
            animator.SetTrigger(AnimationParams.JumpTrigger);
        }

        yield return new WaitForSeconds(jumpDelay);

        playerVelocity.y = MathF.Sqrt(jumpHeight * gravity * -2.0f);
    }

    // THE PLAYER CAN ALSO ONLY SPRINT IF THEY ARE GROUNDED
    // THIS PREVENTS THE CASE WHERE THE PLAYER IS IN MID AIR
    // AND PRESSES THE SPRINT BUTTON, PREVENTS THEM FROM TURINING INTO SUPERMAN
    public void PlayerSprint(bool isSprinting)
    {
        if (PauseMenu.isGamePause) return;

        if (isSprinting)
        {
            // Only allow STARTING a sprint if grounded and not crouching
            if (isGrounded && !isCrouching)
            {
                speed = sprintSpeed;
                if (animator != null) animator.SetBool(AnimationParams.IsSprinting, true);
            }
        }
        else
        {
            // ALWAYS allow STOPPING a sprint, even in mid-air
            speed = isCrouching ? sneakSpeed : walkSpeed;
            if (animator != null) animator.SetBool(AnimationParams.IsSprinting, false);
        }
    }

    public void playerCrouch()
    {
        if (PauseMenu.isGamePause) return;


        //if (isCrouching)
        //{
        //    if (Physics.Raycast(transform.position, Vector3.up, standingHeight))
        //    {
        //        return; // ceiling above — stay crouched
        //    }
        //}

        isCrouching = !isCrouching;

        if (isCrouching)
        {
            characterController.height = crouchHeight;
            characterController.center = crouchCenter;
            speed = sneakSpeed;

            if (cameraScript != null)
            {
                // Raycast origin still drops to avoid ceiling clipping
                cameraScript.headPosition = 0.56f;
                // This now only lowers the Top-Down coordinates
                cameraScript.ToggleCrouchView(true);
            }
        }
        else
        {
            characterController.height = standingHeight;
            characterController.center = standingCenter;
            speed = walkSpeed;

            if (cameraScript != null)
            {
                cameraScript.headPosition = 1.29f;
                cameraScript.ToggleCrouchView(false);
            }
        }

        if (animator != null)
        {
            animator.SetBool(AnimationParams.IsCrouching, isCrouching);
        }
    }

    private void EmitMovementSound()
    {
        if (PauseMenu.isGamePause) return;

        bool isMoving = currentVelocity.magnitude > 0.1f;

        if (isCrouching || !isMoving)
        {
            //playerScript.StopFootsteps();
            return;
        }

        bool isSprinting = (speed == sprintSpeed);
        //playerScript.PlayFootsteps(isSprinting);

        float volume = isSprinting ? runVolume : walkVolume;
        soundTimer -= Time.deltaTime;
        if (soundTimer <= 0f)
        {
            SoundEmissionManager.EmitSound(transform.position, volume);
            soundTimer = soundEmitInterval;
        }
    }
}