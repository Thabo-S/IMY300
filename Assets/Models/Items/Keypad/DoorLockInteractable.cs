using UnityEngine;

[RequireComponent(typeof(doorMovement))] // respective door
public class DoorLockInteractable : MonoBehaviour
{
    [Header("Lock Settings")]
    public bool isLocked = true; // Is the door locked?

    [Tooltip("The LockDoorQte component - found automatically via the 'QTeEventsLockDoor' tag if left unassigned.")]
    [SerializeField] private LockDoorQte qteScript;

    private doorMovement door;
    private bool isSubscribed = false;

    private void Awake()
    {
        door = GetComponent<doorMovement>();

        if (qteScript == null)
        {
            GameObject qteObj = GameObject.FindGameObjectWithTag("QTeEventsLockDoor");
            if (qteObj != null) qteScript = qteObj.GetComponent<LockDoorQte>();
        }

        if (qteScript == null)
            Debug.LogWarning($"{name}: No LockDoorQte found (tag 'QTeEventsLockDoor') and none assigned in Inspector.");
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    /// <summary>
    /// Call this from your existing interact system (e.g. the raycast + "Interact" action
    /// that's already wired to the "E" key) when the player is looking at this door.
    /// </summary>
    public void Interact()
    {
        if (qteScript == null) return;

        if (!isLocked)
        {
            door.ToggleDoor();
            return;
        }

        if (qteScript.IsOnCooldown)
        {
            qteScript.NotifyCooldownActive();
            return;
        }

        // Same fix as KeypadDoorInteractable - only subscribe when THIS
        // door is the one actually starting a QTE on the shared instance.
        Subscribe();
        qteScript.StartQte();
    }

    private void Subscribe()
    {
        if (isSubscribed || qteScript == null) return;

        qteScript.OnQteSuccess.AddListener(HandleQteSuccess);
        qteScript.OnQteFail.AddListener(HandleQteFail);
        qteScript.OnQteCancel.AddListener(HandleQteCancel);
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || qteScript == null) return;

        qteScript.OnQteSuccess.RemoveListener(HandleQteSuccess);
        qteScript.OnQteFail.RemoveListener(HandleQteFail);
        qteScript.OnQteCancel.RemoveListener(HandleQteCancel);
        isSubscribed = false;
    }

    private void HandleQteSuccess()
    {
        Unsubscribe();
        isLocked = false;
        door.ToggleDoor();
    }

    private void HandleQteFail()
    {
        Unsubscribe();
        // Door stays locked. Cooldown already started inside LockDoorQte.
    }

    private void HandleQteCancel()
    {
        Unsubscribe();
        // isLocked is left untouched - door stays locked.
    }
}