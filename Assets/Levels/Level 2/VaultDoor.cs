using UnityEngine;

public class VaultDoor : MonoBehaviour
    {
        [Header("State")]
        public bool isLocked = true;
        public bool isOpen = false;

        [Header("Optional")]
        public bool closeAutomatically = false;
        public float autoCloseDelay = 5f;

        private doorMovement doorMovement;
        private float autoCloseTimer;

    private void Start()
        {
            doorMovement = GetComponent<doorMovement>();
            if (doorMovement == null)
            {
                Debug.LogError("VaultDoor: doorMovement component not found!");
            }
        }

    private void Update()
        {
            if (closeAutomatically && isOpen)
            {
                autoCloseTimer += Time.deltaTime;

                if (autoCloseTimer >= autoCloseDelay)
                {
                    CloseDoor();
                }
            }
        }

    public void Unlock()
    {
        isLocked = false;
    }

    public void Lock()
    {
        if (!isOpen)
        {
            isLocked = true;
        }
    }

    public void OpenDoor()
        {
            if (isLocked)
            {
                Debug.Log("Vault door is locked.");
                return;
            }

            if (doorMovement != null)
            {
                doorMovement.ToggleDoor();
            }
            isOpen = true;
            autoCloseTimer = 0f;
        }

    public void CloseDoor()
        {
            if (doorMovement != null && doorMovement.currentState == doorMovement.DoorState.Open)
            {
                doorMovement.ToggleDoor();
            }
            isOpen = false;
            autoCloseTimer = 0f;
        }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    public void UnlockAndOpen()
    {
        Unlock();
        OpenDoor();
    }
}