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

    [Tooltip("Per-keypad override for the QTE countdown in seconds. Leave at 0 to use the value configured on the QTE UI itself (KepPadQTE.qteTimeLimit).")]
    [SerializeField] private float qteTimeLimitOverride = 0f;

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

    private void OnEnable()
    {
        if (qteScript == null) return;

                qteScript.OnQteSuccess.AddListener(HandleQteSuccess);
        qteScript.OnQteFail.AddListener(HandleQteFail);
        qteScript.OnQteCancel.AddListener(HandleQteCancel);
        qteScript.OnQteTimeout.AddListener(HandleQteTimeout);
    }

    private void OnDisable()
    {
        if (qteScript == null) return;

                qteScript.OnQteSuccess.RemoveListener(HandleQteSuccess);
        qteScript.OnQteFail.RemoveListener(HandleQteFail);
        qteScript.OnQteCancel.RemoveListener(HandleQteCancel);
        qteScript.OnQteTimeout.RemoveListener(HandleQteTimeout);
    }

        // True only while this keypad's own QTE is the one on screen.
    // Set in StartQte(), cleared in HideQte(). Used to filter the
    // shared KepPadQTE singleton's events so multiple keypads in the
    // scene (which all bind to the same QTeEventsKepPad UI by tag) don't
    // open every door when just one is hacked.
    private bool qteIsActive;

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

        qteCanvas.enabled = true;
        qteIsActive = true;

        // Per-keypad timer override: if this keypad specifies one, push it onto
        // the shared QTE UI right before starting. (Done via a tiny shim field
        // on KepPadQTE so we don't have to change its serialized state.)
        if (qteTimeLimitOverride > 0f)
        {
            qteScript.SetQteTimeLimit(qteTimeLimitOverride);
        }

        qteScript.StartQTE(); // hacking bar + sequence generation only begin NOW, on E press
        //SetPlayerControlsEnabled(false);
    }

    private void HandleQteSuccess()
    {
        if (!qteIsActive) return; // another keypad's QTE just succeeded - ignore
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
        if (!qteIsActive) return; // ignore failures from other keypads
        // Door stays locked. Give the player another shot immediately by
        // restarting the hacking sequence directly (no more relying on
        // toggling component.enabled to re-trigger OnEnable).
        qteScript.StartQTE();
    }

    private void HandleQteCancel()
    {
        if (!qteIsActive) return; // ignore cancels from other keypads
        HideQte();
        //SetPlayerControlsEnabled(true);
        // isLocked is left untouched - door stays locked.
    }

    private void HandleQteTimeout()
    {
        if (!qteIsActive) return; // ignore timeouts from other keypads
        // Timer ran out - dismiss the keypad UI so it doesn't linger on screen.
        // Door stays locked. Player can re-interact to try again (same as a fail).
        HideQte();
    }

    private void HideQte()
    {
        qteIsActive = false;
        if (qteCanvas != null) qteCanvas.enabled = false;
        if (qteScript != null) qteScript.StopQTE();
    }
}