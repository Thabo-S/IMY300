using UnityEngine;

public class KeypadDoorInteractable : MonoBehaviour
{
    public enum DoorType { Standard, Garage }

    [Header("Lock Settings")]
    public bool isLocked = true; // Is the door locked?
    [Tooltip("Which doorMovement method this keypad should trigger on success.")]
    [SerializeField] private DoorType doorType = DoorType.Garage;

    [Header("Door Reference")]
    [Tooltip("The doorMovement component on the actual door this keypad unlocks (NOT necessarily on this GameObject).")]
    [SerializeField] private doorMovement door;

    [Header("QTE References")]
    [Tooltip("The parent panel that holds the whole keypad QTE UI (e.g. 'QTeEventsKepPad').")]
    [SerializeField] private GameObject qteUI;
    [Tooltip("The KepPadQTE component, usually on the object holding the Slot_1..4 images.")]
    [SerializeField] private KepPadQTE qteScript;

    private void Awake()
    {
        // Make sure the QTE UI is hidden at runtime no matter how the scene was saved.
        if (qteUI != null)
            qteUI.SetActive(false);

        if (door == null)
            Debug.LogWarning($"{name}: 'Door' reference is not assigned on KeypadDoorInteractable.");
    }

    private void OnEnable()
    {
        if (qteScript == null) return;

        qteScript.OnQteSuccess.AddListener(HandleQteSuccess);
        qteScript.OnQteFail.AddListener(HandleQteFail);
        qteScript.OnQteCancel.AddListener(HandleQteCancel);
    }

    private void OnDisable()
    {
        if (qteScript == null) return;

        qteScript.OnQteSuccess.RemoveListener(HandleQteSuccess);
        qteScript.OnQteFail.RemoveListener(HandleQteFail);
        qteScript.OnQteCancel.RemoveListener(HandleQteCancel);
    }

    /// <summary>
    /// Call this from your existing interact system (e.g. the raycast + "Interact" action
    /// that's already wired to the "E" key) when the player is looking at this keypad/door.
    /// </summary>
    public void Interact()
    {
        // Ignore repeated interact presses while a QTE is already running on this door.
        if (qteUI != null && qteUI.activeSelf) return;

        if (!isLocked)
        {
            ToggleAssignedDoor();
            return;
        }

        StartQte();
    }

    private void StartQte()
    {
        if (qteUI == null || qteScript == null)
        {
            Debug.LogWarning($"{name}: QTE references are not assigned on KeypadDoorInteractable.");
            return;
        }

        qteUI.SetActive(true); // triggers KepPadQTE.OnEnable(), which rerolls the sequence
        //SetPlayerControlsEnabled(false);
    }

    private void HandleQteSuccess()
    {
        isLocked = false;
        qteUI.SetActive(false);
        //SetPlayerControlsEnabled(true);
        ToggleAssignedDoor(); // open the door now that it's unlocked
    }

    private void ToggleAssignedDoor()
    {
        if (doorType == DoorType.Garage)
            door.ToggleGarageDoor();
        else
            door.ToggleDoor();
    }

    private void HandleQteFail()
    {
        // Door stays locked. Give the player another shot immediately by
        // re-triggering OnEnable on the QTE script, which rerolls the sequence.
        qteScript.enabled = false;
        qteScript.enabled = true;
    }

    private void HandleQteCancel()
    {
        qteUI.SetActive(false);
        //SetPlayerControlsEnabled(true);
        // isLocked is left untouched - door stays locked.
    }
}