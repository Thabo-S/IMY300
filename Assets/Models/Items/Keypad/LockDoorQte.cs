using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LockDoorQte : MonoBehaviour
{
    [Header("QTE UI (auto-found, no need to assign)")]
    [Tooltip("Root object tagged 'QTeEventsLockDoor', found automatically at runtime.")]
    public GameObject qteUIRoot;
    private Canvas qteCanvas;

    private Button pickLockButton;
    private Button cancelButton;
    private Transform pointA;
    private Transform pointB;
    private RectTransform safeZone;
    private RectTransform pointerTransform;

    public float moveSpeed = 1000f;
    public float direction = 1f;

    [Header("Fail Behavior")]
    [Tooltip("Seconds the QTE is disabled after a failed attempt before it can be retried.")]
    public float failCooldown = 3.5f;
    private bool isOnCooldown = false;
    private float cooldownEndTime;

    /// <summary>Whether this lock is currently locked out after a recent failure.</summary>
    public bool IsOnCooldown => isOnCooldown;

    [Header("Player Input (auto-found, no need to assign)")]
    private InputMananger playerInputToDisable;
    private bool playerInputWasEnabledBeforeQte;
    private bool isPlayerInputBlocked;

    [Header("Events")]
    public UnityEvent OnQteSuccess;
    public UnityEvent OnQteFail;
    public UnityEvent OnQteCancel;

    private float targetX;
    private bool isMovingToPointB = true;
    private bool waitForInteractKeyRelease;
    private bool isActive = false;
    private bool isSetupValid = false;
    private bool hasLockCracked = false;
    private string previousInteractionPrompt;

    void Start()
    {
        qteUIRoot = GameObject.FindGameObjectWithTag("QTeEventsLockDoor");

        qteCanvas = qteUIRoot.GetComponent<Canvas>();

        qteCanvas.enabled = false;

        pickLockButton = qteUIRoot.transform.Find("PickLock")?.GetComponent<Button>();
        cancelButton = qteUIRoot.transform.Find("Cancel")?.GetComponent<Button>();
        pointA = qteUIRoot.transform.Find("PointA");
        pointB = qteUIRoot.transform.Find("PointB");
        safeZone = qteUIRoot.transform.Find("SafeZone")?.GetComponent<RectTransform>();
        pointerTransform = qteUIRoot.transform.Find("Pointer")?.GetComponent<RectTransform>();

        // The lock-pick QTE is controlled from the keyboard, not by clicking UI.
        if (pickLockButton != null) pickLockButton.enabled = false;
        if (cancelButton != null) cancelButton.enabled = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"[LockDoorQte] ({gameObject.name}) No GameObject tagged 'Player' found - player will NOT be frozen during this QTE.");
        }
        else
        {
            playerInputToDisable = player.GetComponent<InputMananger>();
            if (playerInputToDisable == null)
                Debug.LogWarning($"[LockDoorQte] ({gameObject.name}) Player object found, but has no InputMananger component - player input will NOT be frozen during this QTE.");
        }

        targetX = pointB.localPosition.x;

        isSetupValid = true;
        enabled = false;
    }

    public void StartQte()
    {
        if (!isSetupValid)
        {
            Debug.LogWarning($"[LockDoorQte] ({gameObject.name}) StartQte() called but setup failed earlier — see prior error in Console.");
            return;
        }

        if (hasLockCracked)
        {
            Debug.Log("[LockDoorQte] Door is already unlocked! Skipping QTE.");
            doorMovement door = GetComponent<doorMovement>();
            if (door != null) door.ToggleDoor();

            return;
        }

        if (isOnCooldown)
        {
            NotifyCooldownActive();
            return;
        }

        if (Inventory.IsOpen && Inventory.instance != null)
            Inventory.instance.ToggleInventory();
        if (Player.Instance != null)
            Player.Instance.CloseOpenPanels();

        isActive = true;
        enabled = true;
        QteState.Lockpick = true;
        qteCanvas.enabled = true;

        if (Player.Instance != null && Player.Instance.interactionTextUI != null)
        {
            previousInteractionPrompt = Player.Instance.interactionTextUI.text;
            Player.Instance.interactionTextUI.text = "Press [E] to pick the lock  |  [C] to cancel";
        }

        if (playerInputToDisable != null)
        {
            playerInputWasEnabledBeforeQte = playerInputToDisable.enabled;
            playerInputToDisable.enabled = false;
            isPlayerInputBlocked = true;
        }

        Vector3 pointerPosition = pointerTransform.localPosition;
        pointerPosition.x = pointA.localPosition.x;
        pointerTransform.localPosition = pointerPosition;
        targetX = pointB.localPosition.x;
        isMovingToPointB = true;
        waitForInteractKeyRelease = Input.GetKey(KeyCode.E);
        direction = Mathf.Sign(targetX - pointerPosition.x);

        if (CursorManager.instance != null)
        {
            CursorManager.instance.UnlockCursor();
            Debug.Log($"[LockDoorQte] Called UnlockCursor(). Cursor.lockState={Cursor.lockState}, Cursor.visible={Cursor.visible}");
        }
        else
        {
            Debug.LogWarning("[LockDoorQte] CursorManager.instance is NULL — cursor was never unlocked, buttons cannot be clicked!");
        }
    }

    /// <summary>
    /// Shows a notification with time remaining if a caller tries to
    /// interact with this lock while it's on cooldown.
    /// </summary>
    public void NotifyCooldownActive()
    {
        float remaining = Mathf.Max(0f, cooldownEndTime - Time.time);
        Inventory.instance?.ShowNotification($"Lock jammed - try again in {Mathf.CeilToInt(remaining)}s.");
    }

    void Update()
    {
        if (!isActive) return;

        Vector3 pointerPosition = pointerTransform.localPosition;
        pointerPosition.x = Mathf.MoveTowards(pointerPosition.x, targetX, moveSpeed * Time.deltaTime);

        if (Mathf.Abs(pointerPosition.x - targetX) <= 0.01f)
        {
            pointerPosition.x = targetX;
            isMovingToPointB = !isMovingToPointB;
            targetX = isMovingToPointB ? pointB.localPosition.x : pointA.localPosition.x;
            direction = Mathf.Sign(targetX - pointerPosition.x);
        }

        pointerTransform.localPosition = pointerPosition;

        // The E press that opened the lock must not also count as a pick attempt.
        if (waitForInteractKeyRelease)
        {
            if (!Input.GetKey(KeyCode.E))
                waitForInteractKeyRelease = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
            CheckSuccess();
        else if (Input.GetKeyDown(KeyCode.C))
            CancelQte();
    }

    void CheckSuccess()
    {
        Debug.Log("[LockDoorQte] Pick attempt triggered by the E key.");

        // 1. Calculate the true visual half-widths by multiplying the base width by the inspector scale
        float safeZoneHalfWidth = (safeZone.rect.width * safeZone.localScale.x) / 2f;
        float pointerHalfWidth = (pointerTransform.rect.width * pointerTransform.localScale.x) / 2f;

        // 2. Find the exact horizontal distance between the centers of both objects
        // (Since both are children of the QTE UI Root, we can safely compare their local X positions)
        float horizontalDistance = Mathf.Abs(pointerTransform.localPosition.x - safeZone.localPosition.x);

        // 3. If the distance is less than their combined half-widths, they are visually overlapping!
        bool isInSafeZone = horizontalDistance <= (safeZoneHalfWidth + pointerHalfWidth);

        if (isInSafeZone)
        {
            Debug.Log("[LockDoorQte] Success! Pointer is overlapping the safe zone.");
            EndQte();

            doorMovement door = GetComponent<doorMovement>();
            if (door != null)
            {
                door.ToggleDoor();
                hasLockCracked = true;
            }
            else Debug.LogWarning("[LockDoorQte] This door has no doorMovement component.");

            OnQteSuccess?.Invoke();
        }
        else
        {
            Debug.Log("[LockDoorQte] Failure! Pointer is outside the safe zone.");
            HandleFail();
        }
    }

    private void HandleFail()
    {
        EndQte();
        OnQteFail?.Invoke();
        StartCoroutine(FailCooldownRoutine());
    }

    private System.Collections.IEnumerator FailCooldownRoutine()
    {
        isOnCooldown = true;
        cooldownEndTime = Time.time + failCooldown;
        yield return new WaitForSeconds(failCooldown);
        isOnCooldown = false;
    }

    void CancelQte()
    {
        Debug.Log("[LockDoorQte] QTE cancelled by player.");
        EndQte();
        OnQteCancel?.Invoke();
    }

    private void EndQte()
    {
        isActive = false;
        enabled = false;
        QteState.Lockpick = false;
        qteCanvas.enabled = false;

        if (Player.Instance != null && Player.Instance.interactionTextUI != null)
            Player.Instance.interactionTextUI.text = previousInteractionPrompt ?? string.Empty;
        previousInteractionPrompt = null;

        if (isPlayerInputBlocked && playerInputToDisable != null)
            playerInputToDisable.enabled = playerInputWasEnabledBeforeQte;
        isPlayerInputBlocked = false;
        if (CursorManager.instance != null) CursorManager.instance.LockCursor();
    }
}
