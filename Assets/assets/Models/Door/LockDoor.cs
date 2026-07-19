using UnityEngine;

public class LockDoor : MonoBehaviour
{
    [Header("References")]
    public doorMovement door;
    public LockDoorQTE qteController;
    public GameObject qtePanel;



    [Header("State")]
    public bool isLocked = true;

    private bool playerInRange = false;
    private bool qteInProgress = false; // blocks re-triggering while a QTE is already running

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // (pause guard)
        // Only open a NEW qte if one isn't already running - otherwise every
        // press during the minigame (including the success press) restarts it.
        if (playerInRange && isLocked && !qteInProgress && Input.GetKeyDown(KeyCode.E))
        {
            OpenQTE();
        }
    }

    void OpenQTE()
    {
        if (LockDoorQTE.IsAnyQTEActive) return;

        qteInProgress = true;
        qtePanel.SetActive(true);
        qteController.StartQTE(OnQTEResult);
    }

    // Called by PickUpScript when the player right-clicks this door while
    // the Key is selected in the hotbar. Skips the QTE minigame entirely.
    public void UnlockWithKey()
    {
        if (!isLocked) return;

        isLocked = false;
        door.ToggleDoor();
        Debug.Log("Door unlocked with key - QTE skipped.");
    }

    void OnQTEResult(bool success)
    {
        qtePanel.SetActive(false);
        qteInProgress = false; // free to try again (fail) or done (success)

        if (success)
        {
            isLocked = false;
            door.ToggleDoor();
            Debug.Log("QTE Success - door unlocked!");
        }
        else
        {
            Debug.Log("QTE Failed - door still locked, try again.");
        }
    }
}