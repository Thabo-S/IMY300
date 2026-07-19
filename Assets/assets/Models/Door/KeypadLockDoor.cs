using UnityEngine;

// Attach to the keypad's trigger volume (e.g. LockKeyPad). Player presses E
// while in range to open the keypad QTE - entering the correct 4-key
// sequence before the timer runs out unlocks the door. A wrong key or a
// timeout fails the attempt; the player has to interact again to retry.
public class KeypadLockDoor : MonoBehaviour
{
    [Header("References")]
    public doorMovement door;
    public KeyPadQTE qteController;
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
        // While a QTE is up, keep it hidden behind the pause menu whenever
        // the game is paused - the QTE itself already freezes (it checks
        // Time.timeScale), this just handles what's visible on screen.
        if (qteInProgress)
        {
            bool shouldShow = !PauseMenu.isGamePause;
            if (qtePanel.activeSelf != shouldShow)
                qtePanel.SetActive(shouldShow);
        }

        if (Time.timeScale == 0f) return; // (pause guard)

        if (playerInRange && isLocked && !qteInProgress && Input.GetKeyDown(KeyCode.E))
        {
            OpenQTE();
        }
    }

    void OpenQTE()
    {
        // Also checks LockDoorQTE so a keypad and a dial-style door QTE can
        // never both be open on screen at once.
        if (KeyPadQTE.IsAnyQTEActive || LockDoorQTE.IsAnyQTEActive) return;

        qteInProgress = true;
        qtePanel.SetActive(true);
        qteController.StartQTE(OnQTEResult);
    }

    void OnQTEResult(bool success)
    {
        qtePanel.SetActive(false);
        qteInProgress = false; // free to try again (fail) or done (success)

        if (success)
        {
            isLocked = false;
            door.ToggleDoor();
            Debug.Log("Keypad QTE Success - door unlocked!");
        }
        else
        {
            Debug.Log("Keypad QTE Failed - door still locked, try again.");
        }
    }
}