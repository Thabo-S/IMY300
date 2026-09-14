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
    public float failCooldown = 1.5f;
    private bool isOnCooldown = false;

    [Header("Player Freeze (auto-found, no need to assign)")]
    private MonoBehaviour playerMovementToDisable;

    [Header("Events")]
    public UnityEvent OnQteSuccess;
    public UnityEvent OnQteFail;
    public UnityEvent OnQteCancel;

    private Vector3 targetPosition;
    private bool isActive = false;
    private bool isSetupValid = false;
    private bool hasLockCracked = false;

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

        pickLockButton.onClick.AddListener(CheckSuccess);
        cancelButton.onClick.AddListener(CancelQte);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"[LockDoorQte] ({gameObject.name}) No GameObject tagged 'Player' found - player will NOT be frozen during this QTE.");
        }
        else
        {
            playerMovementToDisable = player.GetComponent<PlayerMovement>();
            if (playerMovementToDisable == null)
                Debug.LogWarning($"[LockDoorQte] ({gameObject.name}) Player object found, but has no PlayerMovement component - player will NOT be frozen during this QTE.");
        }

        targetPosition = pointB.position;

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

        if (isOnCooldown) return;

        isActive = true;
        enabled = true;

        qteCanvas.enabled = true;

        pointerTransform.position = pointA.position;
        targetPosition = pointB.position;
        direction = 1f;

        if (playerMovementToDisable != null)
        {
            playerMovementToDisable.enabled = false;
            Debug.Log($"[LockDoorQte] Disabled {playerMovementToDisable.GetType().Name}. enabled is now {playerMovementToDisable.enabled}");
        }

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

    void Update()
    {
        if (!isActive) return;

        pointerTransform.position = Vector3.MoveTowards(pointerTransform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(pointerTransform.position, pointA.position) < 0.1f)
        {
            targetPosition = pointB.position;
            direction = 1f;
        }
        else if (Vector3.Distance(pointerTransform.position, pointB.position) < 0.1f)
        {
            targetPosition = pointA.position;
            direction = -1f;
        }

        // Success/Cancel are now driven entirely by the button OnClick
        // listeners wired in Start() - no keyboard polling needed here.
    }

    void CheckSuccess()
    {

        Debug.Log("[LockDoorQte] CheckSuccess() was called — button click WAS received.");

        Vector2 localPoint = safeZone.InverseTransformPoint(pointerTransform.position);
        bool isInSafeZone = safeZone.rect.Contains(localPoint);

        if (isInSafeZone)
        {
            Debug.Log("[LockDoorQte] Success! Pointer is in the safe zone.");
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
            Debug.Log("[LockDoorQte] Failure! Pointer is not in the safe zone.");
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

        qteCanvas.enabled = false;

        if (playerMovementToDisable != null) playerMovementToDisable.enabled = true;
        if (CursorManager.instance != null) CursorManager.instance.LockCursor();
    }
}