using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private Vector3 playerVelocity;
    private bool isGrounded;

    public float gravity = -9.8f;
    public float speed = 6f;
    public float jumpHeight = 2f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();

    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = characterController.isGrounded;
    }

    // SO THIS FUNCTION BASCIALLY GETS THE INPUTS FROM THE INPUT MANAGER 
    // AND APPLIES THEM TO THE CHARACTER CONTROLLER TO MOVE THE PLAYER
    public void CalculatePlayerMovement(Vector2 movementInput)
    {
        Vector3 playerMovementDirection = Vector3.zero;

        playerMovementDirection.x = movementInput.x;

        // since it's a 3D sort of game this might be confusing but
        // basically what i am doing here is taking the up/vertical movement
        // from the input (when you press W on WASD) and transforming it 
        // to foward movement
        playerMovementDirection.z = movementInput.y;

        // So without TransformDirection, the player would basically move
        // foward towards the Worlds North and not relative to the player's rotation
        characterController.Move(transform.TransformDirection(playerMovementDirection) * speed * Time.deltaTime);


        //this applied a downward motion to the player
        playerVelocity.y += gravity * Time.deltaTime;


        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        characterController.Move(playerVelocity * Time.deltaTime);  
    }

    public void PlayerJump()
    {
        if (isGrounded)
        {
            playerVelocity.y = MathF.Sqrt(jumpHeight * gravity * -3.0f);
        }
    }
}
