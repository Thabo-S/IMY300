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
    [Tooltip("The parent panel that holds the whole keypad QTE UI (e.g. 'QTeEventsKepPad'). Found via tag at runtime - keep its GameObject active in the scene and gate visibility via the Canvas instead, or tag lookup will fail.")]
    [SerializeField] private GameObject qteUI;
    [SerializeField] private Canvas qteCanvas;
    [Tooltip("The KepPadQTE component, usually on the object holding the Slot_1..4 images.")]
    [SerializeField] private KepPadQTE qteScript;

    private bool isSubscribed = false;

    private void Awake()
    {
        if (door == null)
            Debug.LogWarning($"{name}: 'Door' reference is not assigned on KeypadDoorInteractable.");

        qteUI = GameObject.FindGameObjectWithTag("QTeEventsKepPad");

        if (qteUI == null)
        {
            Debug.LogWarning($"{name}: Could not find a GameObject tagged 'QTeEventsKepPad' in the scene.");
            return;
        }

        qteCanvas = qteUI.GetComponent<Canvas>();
        qteScript = qteUI.GetComponent<KepPadQTE>();

        if (qteCanvas != null)
            qteCanvas.enabled = false;
        else
            Debug.LogWarning($"{name}: No Canvas component found on '{qteUI.name}'.");
    }

    private void OnDisable()
    {
        // Safety net - if this door gets disabled mid-QTE for any reason,
        // make sure it doesn't stay subscribed to the shared QTE instance.
        Unsubscribe();
    }

    /// <summary>
    /// Call this from your existing interact system (e.g. the raycast + "Interact" action
    /// that's already wired to the "E" key) when the player is looking at this keypad/door.
    /// </summary>
    public void Interact()
    {
        // Ignore repeated interact presses while a QTE is already running on this door.
        if (qteCanvas != null && qteCanvas.enabled) return;

        if (!isLocked)
        {
            ToggleAssignedDoor();
            return;
        }

        StartQte();
    }

    private void StartQte()
    {
        if (qteUI == null || qteScript == null || qteCanvas == null)
        {
            Debug.LogWarning($"{name}: QTE references are not assigned on KeypadDoorInteractable.");
            return;
        }

        // Check cooldown BEFORE showing the canvas - otherwise the canvas
        // would flash open even though StartQTE() internally refuses to run.
        if (qteScript.IsOnCooldown)
        {
            qteScript.NotifyCooldownActive();
            return;
        }

        // This is the critical fix: only the door that's actually starting
        // a QTE subscribes to the shared instance's events. With multiple
        // keypads in a level all sharing ONE KepPadQTE, subscribing in
        // OnEnable() (old behavior) meant every door listened forever, so
        // ANY keypad succeeding fired every door's success handler at once.
        Subscribe();

        qteCanvas.enabled = true;
        qteScript.StartQTE(); // hacking bar + sequence generation only begin NOW, on E press
        //SetPlayerControlsEnabled(false);
    }

    private void Subscribe()
    {
        if (isSubscribed || qteScript == null) return;

        qteScript.OnQteSuccess.AddListener(HandleQteSuccess);
        qteScript.OnQteFail.AddListener(HandleQteFail);
        qteScript.OnQteCancel.AddListener(HandleQteCancel);
        qteScript.OnQteTimeout.AddListener(HandleQteTimeout);
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || qteScript == null) return;

        qteScript.OnQteSuccess.RemoveListener(HandleQteSuccess);
        qteScript.OnQteFail.RemoveListener(HandleQteFail);
        qteScript.OnQteCancel.RemoveListener(HandleQteCancel);
        qteScript.OnQteTimeout.RemoveListener(HandleQteTimeout);
        isSubscribed = false;
    }

    private void HandleQteSuccess()
    {
        Unsubscribe();
        isLocked = false;
        HideQte();
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
        Unsubscribe();
        // Door stays locked. Cooldown has already been started inside
        // KepPadQTE itself - just close the UI, the player has to
        // re-interact (and wait out the cooldown) to try again.
        HideQte();
    }

    private void HandleQteTimeout()
    {
        Unsubscribe();
        // Ran out of time on the sequence - cooldown already started inside
        // KepPadQTE. Close the UI here; wire the alarm/guard alert to
        // OnQteTimeout in the Inspector if you haven't already.
        HideQte();
    }

    private void HandleQteCancel()
    {
        Unsubscribe();
        HideQte();
        //SetPlayerControlsEnabled(true);
        // isLocked is left untouched - door stays locked.
    }

    private void HideQte()
    {
        if (qteCanvas != null) qteCanvas.enabled = false;
        if (qteScript != null) qteScript.StopQTE();
    }
}