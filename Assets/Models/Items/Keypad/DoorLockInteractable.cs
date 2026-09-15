using UnityEngine;
[RequireComponent(typeof(doorMovement))] // resepective door

public class DoorLockInteractable : MonoBehaviour
{
    [Header("Lock Settings")]
    public bool isLocked = true; // Is the door locked?
    [Header("QTE References")]
    [Tooltip("The parent panel that holds the whole QTE UI (e.g. 'QTeEvents').")]
    [SerializeField] private GameObject qteUI;
    [Tooltip("The LockDoorQte component, usually on the 'Pointer' object.")]

    [SerializeField] private LockDoorQte qteScript;

    private doorMovement door;

    private void Awake()
    {
        door = GetComponent<doorMovement>();

        // Make sure the QTE UI is hidden at runtime no matter how the scene was saved.
        if (qteUI != null)
            qteUI.SetActive(false);
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
    /// that's already wired to the "E" key) when the player is looking at this door.
    /// </summary>
    public void Interact()
    {
        // Ignore repeated interact presses while a QTE is already running on this door.
        if (qteUI != null && qteUI.activeSelf) return;

        if (!isLocked)
        {
            door.ToggleDoor();
            return;
        }

        StartQte();
    }
    private void StartQte()
    {
        if (qteUI == null || qteScript == null)
        {
            Debug.LogWarning($"{name}: QTE references are not assigned on DoorInteractable.");
            return;
        }

        qteUI.SetActive(true); // triggers LockDoorQte.OnEnable(), which resets the pointer
        //SetPlayerControlsEnabled(false);
    }
    private void HandleQteSuccess()
    {
        isLocked = false;
        qteUI.SetActive(false);
        //SetPlayerControlsEnabled(true);
        door.ToggleDoor(); // open the door now that it's unlocked
    }
    private void HandleQteFail()
    {
        // Door stays locked. Give the player another shot immediately by
        // re-triggering OnEnable on the QTE script, which resets the pointer.
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
