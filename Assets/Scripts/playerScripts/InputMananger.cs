using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputMananger : MonoBehaviour
{
    private PlayerInput.WalkingActions walking;

    private PlayerInput.PickUpActions pickUp;

    private PlayerInput playerInput;

    private PlayerMovement movement;

    private PlayerLookAround playerLookAround;

    public PickUpScript pickUpScript;

    private float sprintValue = 16f;

    private float sneakValue = 6f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInput = new PlayerInput();

        walking = playerInput.Walking;

        pickUp = playerInput.PickUp;

        movement =  GetComponent<PlayerMovement>();

        playerLookAround = GetComponent<PlayerLookAround>();

        walking.Jump.performed += ctx => movement.PlayerJump();

        // MODIFIED THE SPRINT FUNTION TO HAVE A PARAMETER
        // THAT DETERMINES THE SPEED OF THE PLAYER BUT CHANGES 
        // BACK ON EVENT CANCELLED
        walking.Sprint.performed += ctx => movement.PlayerSprint(sprintValue);

        walking.Sprint.canceled += ctx => movement.PlayerSprint(sneakValue);

        walking.Crouch.performed += ctx => movement.playerCrouch();

        //walking.Crouch.canceled += ctx => movement.playerCrouch();

        pickUp.PickUpObject.performed += ctx => pickUpScript.runPickUpObject();

        pickUp.ThrowObject.performed += ctx => pickUpScript.runThrowObject();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // so now i tell the function i made in PlayerMovement to move
        // based on the values read from the input system WALKING
        movement.CalculatePlayerMovement(walking.Movement.ReadValue<Vector2>());

        playerLookAround.CalculatePlayerLookAround(walking.LookAround.ReadValue<Vector2>());


    }

    private void LateUpdate()
    {
        //playerLookAround.CalculatePlayerLookAround(walking.LookAround.ReadValue<Vector2>());

    }

    // Basically turns ON the walking controls created before the player moves
    private void OnEnable()
    {
        walking.Enable();
        pickUp.Enable();
    }

    // And this turns the walking controls OFF when the player dies/leaves
    private void OnDisable()
    {
        walking.Disable();
        pickUp.Disable();
    }
}
