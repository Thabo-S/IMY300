using System;
using NUnit.Framework.Constraints;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool isCrouching = false;
    private Transform scaleTransform;

    public float gravity = -50f;
    public float speed = 20f;
    public float walkSpeed = 20f;
    public float sprintSpeed = 30f;
    public float jumpHeight = 5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        scaleTransform = GetComponent<Transform>();

        animator = GetComponentInChildren<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = characterController.isGrounded;

    
    }

    //public void UpdateAnimations()
    //{
    //    Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
    //    bool isMoving = horizontalVelocity.magnitude > 0f;

    //    if (animator != null)
    //    {
    //        Debug.Log($"isWalking: {isMoving}, velocity magnitude: {horizontalVelocity.magnitude}");
    //        animator.SetBool("isWalking", isMoving);
    //    }
    //}

    private Vector3 currentVelocity = Vector3.zero;

    // SO THIS FUNCTION BASCIALLY GETS THE INPUTS FROM THE INPUT MANAGER 
    // AND APPLIES THEM TO THE CHARACTER CONTROLLER TO MOVE THE PLAYER
    public void CalculatePlayerMovement(Vector2 movementInput)
    {

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
            Debug.Log($"isWalking: {isMoving}, velocity magnitude: {currentVelocity.magnitude}");
            animator.SetBool("isWalking", isMoving);
        }
    }

    // THE PLAYER CAN ONLY JUMP IF THEY ARE ONLY GROUNDED
    // THIS BASICALLY PREVENTS DOUBLE JUMPING
    public void PlayerJump()
    {
        if (isGrounded)
        {
            playerVelocity.y = MathF.Sqrt(jumpHeight * gravity * -2.0f);
        }
    }

    // THE PLAYER CAN ALSO ONLY SPRINT IF THEY ARE GROUNDED
    // THIS PREVENTS THE CASE WHERE THE PLAYER IS IN MID AIR
    // AND PRESSES THE SPRINT BUTTON, PREVENTS THEM FROM TURINING INTO SUPERMAN
    public void PlayerSprint(bool isSprinting)
    {
        if (isGrounded && !isCrouching)
        {
            speed = isSprinting ? sprintSpeed : walkSpeed;
        }
    }

    private Vector3 crouchValue = new Vector3(1, 0.5f, 1);

    public void playerCrouch()
    {

        if (!isCrouching) 
        {
            scaleTransform.localScale = crouchValue;
            isCrouching = !isCrouching;
            Debug.Log("currently crouching");
        }
        else
        {
            scaleTransform.localScale = new Vector3(1, 1, 1);
            isCrouching = !isCrouching;
            Debug.Log("standing");
        }

    }
}
