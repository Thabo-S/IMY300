using UnityEngine;
using UnityEngine.InputSystem;

public class InputMananger : MonoBehaviour
{

    private PlayerInput playerInput;

    private PlayerInput.WalkingActions walking;

    private PlayerMovement movement;

    private PlayerLookAround playerLookAround;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInput = new PlayerInput();

        walking = playerInput.Walking;

        movement =  GetComponent<PlayerMovement>();

        playerLookAround = GetComponent<PlayerLookAround>();

        walking.Jump.performed += ctx => movement.PlayerJump();

        // MODIFIED THE SPRINT FUNTION TO HAVE A PARAMETER
        // THAT DETERMINES THE SPEED OF THE PLAYER BUT CHANGES 
        // BACK ON EVENT CANCELLED
        walking.Sprint.performed += ctx => movement.PlayerSprint(16f);

        walking.Sprint.canceled += ctx => movement.PlayerSprint(6f);


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
    }

    // And this turns the walking controls OFF when the player dies/leaves
    private void OnDisable()
    {
        walking.Disable(); 
    }
}
