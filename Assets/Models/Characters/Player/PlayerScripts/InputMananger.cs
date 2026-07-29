using UnityEngine;
using UnityEngine.InputSystem;

public class InputMananger : MonoBehaviour
{
    private PlayerInput.WalkingActions walking;
    private PlayerInput.PickUpActions pickUp;
    private PlayerInput playerInput;

    private PlayerMovement movement;
    private PlayerLookAround playerLookAround;

    [SerializeField] private Inventory inventory;

    private void Awake()
    {
        playerInput = new PlayerInput();

        walking = playerInput.Walking;
        pickUp = playerInput.PickUp;

        movement = GetComponent<PlayerMovement>();
        playerLookAround = GetComponent<PlayerLookAround>();

        if (inventory == null)
            inventory = GetComponent<Inventory>();

        // Movement events...
        walking.Jump.performed += ctx => movement.PlayerJump();
        walking.Sprint.performed += ctx => movement.PlayerSprint(true);
        walking.Sprint.canceled += ctx => movement.PlayerSprint(false);
        walking.Crouch.performed += ctx => movement.playerCrouch();

        // Bind New Input System PickUp action (Key: E)
        pickUp.PickUpObject.performed += ctx => {
            if (inventory != null)
            {
                inventory.TryPickupItem();
            }
        };
    }

    private void FixedUpdate()
    {
        // Passes movement and camera look vectors to their respective scripts
        movement.CalculatePlayerMovement(walking.Movement.ReadValue<Vector2>());
        playerLookAround.CalculatePlayerLookAround(walking.LookAround.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        walking.Enable();
        pickUp.Enable();
    }

    private void OnDisable()
    {
        walking.Disable();
        pickUp.Disable();
    }
}