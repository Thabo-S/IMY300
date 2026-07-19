using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUpScript : MonoBehaviour
{
    public Transform dropPosition;

    public float pickUpRange = 10f;
    [SerializeField] private float doorOpenRange = 25f;

    [Header("References")]
    public activePanel activePanelReferance;
    public List<GameObject> hotbarSlots;
    public Sprite emptySlotSprite;
    public HotbarItem[] hotbarItems = new HotbarItem[5];
    private GameObject currentHighlightedItem; // the ROOT item object, not the child collider hit
    private GameObject currentHighlightedDoor;
    private Player playerScript;

    [Header("Held Item (Viewmodel)")]
    [Tooltip("Empty transform on the camera where the currently selected hotbar item is shown, first-person style.")]
    public Transform handSocket;
    private GameObject currentlyEquippedObject;
    private int lastSelectedIndex = -1;

    [Header("Throwing")]
    public Transform throwPoint;
    public float throwForce = 30f;
    public float minThrowForce = 20f;
    public float maxThrowForce = 80F;
    public float scrollSensitivity = 20f;
    public string throwableTag = "Throwable";

    [Header("Trajectory Preview")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public float trajectoryTimeStep = 0.1f;

    public class HotbarItem
    {
        public GameObject heldObject;
        public Sprite icon;
    }

    private void Start()
    {
        playerScript = GetComponent<Player>();
    }


    void Update()
    {
        PerformContinuousDetection();
        UpdateEquippedItem();

        if (isAiming)
        {
            // Add scroll wheel logic
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0f)
            {
                // Modify throwForce based on scroll
                throwForce = Mathf.Clamp(throwForce + (scrollInput * scrollSensitivity), minThrowForce, maxThrowForce);
            }

            DrawTrajectory();
        }
    }

    private void PerformContinuousDetection()
    {
        RaycastHit hit;
        // We cast using the maximum of the two ranges so we don't miss anything
        float maxRange = Mathf.Max(pickUpRange, doorOpenRange);

        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRange))
        {
            GameObject hitObject = hit.transform.gameObject;
            float distance = hit.distance;

            // --- Handle Items ---
            // Walk up from whatever collider was hit (could be a child part)
            // to find the PickupItem marker on the root object.
            PickupItem pickupRoot = hit.transform.GetComponentInParent<PickupItem>();

            if (pickupRoot != null && distance <= pickUpRange)
            {
                GameObject rootObj = pickupRoot.gameObject;

                if (rootObj != currentHighlightedItem)
                {
                    ClearItemHighlight();
                    currentHighlightedItem = rootObj;
                    ApplyItemHighlight(currentHighlightedItem);
                }
            }
            else if (currentHighlightedItem != null) // If out of range or not hitting
            {
                ClearItemHighlight();
            }

            // --- Handle Doors ---
            if (hitObject.CompareTag("Door") && distance <= doorOpenRange)
            {
                if (hitObject != currentHighlightedDoor)
                {
                    ClearDoorHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyDoorHighlight(currentHighlightedDoor);
                }
            }
            else if (currentHighlightedDoor != null) // If out of range or not hitting
            {
                ClearDoorHighlight();
            }
        }
        else
        {
            ClearItemHighlight();
            ClearDoorHighlight();
        }
    }

    // Applies the outline to EVERY child renderer that has an Outline component,
    // since multi-part items (MedKit, FlashLight) often have the mesh split
    // across several children rather than one single renderer.
    private void ApplyItemHighlight(GameObject obj)
    {
        var outlines = obj.GetComponentsInChildren<Outline>(true);
        foreach (var outline in outlines)
        {
            outline.enabled = true;
        }
    }

    private void ClearItemHighlight()
    {
        if (currentHighlightedItem != null)
        {
            var outlines = currentHighlightedItem.GetComponentsInChildren<Outline>(true);
            foreach (var outline in outlines)
            {
                outline.enabled = false;
            }
            currentHighlightedItem = null;
        }
    }

    private void ApplyDoorHighlight(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            //outline.OutlineColor = Color.yellow;
            outline.enabled = true;
        }
    }

    private void ClearDoorHighlight()
    {
        if (currentHighlightedDoor != null)
        {
            var outline = currentHighlightedDoor.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            currentHighlightedDoor = null;
        }
    }

    // Watches the hotbar's selected slot and shows/hides the held item to match.
    private void UpdateEquippedItem()
    {
        if (activePanelReferance == null) return;

        int selectedIndex = activePanelReferance.SelectedIndex;
        if (selectedIndex == lastSelectedIndex) return;

        lastSelectedIndex = selectedIndex;
        EquipSlot(selectedIndex);
    }

    private void EquipSlot(int index)
    {
        // Put away whatever was previously in-hand
        if (currentlyEquippedObject != null)
        {
            currentlyEquippedObject.SetActive(false);
            currentlyEquippedObject.transform.SetParent(null);
            currentlyEquippedObject = null;
        }

        if (index < 0 || index >= hotbarItems.Length) return;

        HotbarItem item = hotbarItems[index];
        if (item == null || item.heldObject == null) return;

        if (handSocket == null)
        {
            Debug.LogWarning("[PickUpScript] Hand Socket is not assigned - cannot show held item.");
            return;
        }

        GameObject obj = item.heldObject;

        // worldPositionStays = true first, so the object's real-world size/rotation
        // carries over before we then move it to the hand-hold pose.
        obj.transform.SetParent(handSocket, true);
        obj.SetActive(true);

        PickupItem pickupInfo = obj.GetComponent<PickupItem>();
        if (pickupInfo != null)
        {
            obj.transform.localPosition = pickupInfo.holdLocalPosition;
            obj.transform.localRotation = Quaternion.Euler(pickupInfo.holdLocalEulerAngles);
            obj.transform.localScale *= pickupInfo.holdScaleMultiplier;
        }
        else
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }

        // Don't let the held item's own colliders interfere with pickup raycasts
        // or physics while it's floating in front of the camera.
        foreach (var col in obj.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }

        currentlyEquippedObject = obj;
    }

    public void HandleInteraction()
    {
        if (currentHighlightedDoor != null)
        {
            toggleDoorState();
        }
        else
        {
            runPickUpObject();

        }
    }
    public void toggleDoorState()
    {
        if (PauseMenu.isGamePause) return;

        if (currentHighlightedDoor == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, doorOpenRange))
            {
                if (hit.transform.CompareTag("Door"))
                {
                    currentHighlightedDoor = hit.transform.gameObject;
                }
            }

        }

        if (currentHighlightedDoor != null)
        {
            doorMovement targetDoor = currentHighlightedDoor.GetComponent<doorMovement>();
            if (targetDoor != null)
            {
                targetDoor.ToggleDoor();
            }

            AudioSource doorAudio = currentHighlightedDoor.GetComponent<AudioSource>();
            if (doorAudio != null)
            {
                doorAudio.Play();
            }
        }
    }

    public void runPickUpObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
        {
            PickupItem pickupRoot = hit.transform.GetComponentInParent<PickupItem>();
            if (pickupRoot != null)
            {
                PickUpObject(pickupRoot);
            }
        }
    }

    void PickUpObject(PickupItem pickupRoot)
    {
        if (PauseMenu.isGamePause) return;

        GameObject pickUpObj = pickupRoot.gameObject;
        string lookupName = pickupRoot.itemName;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            GameObject slot = hotbarSlots[i];
            Image slotImage = slot.GetComponentInChildren<Image>(true);
            if (slotImage == null) continue;

            if (slotImage.sprite == emptySlotSprite)
            {
                slot.SetActive(true);

                Sprite[] allSprites = Resources.LoadAll<Sprite>("Items/sprites/" + lookupName);
                Sprite itemSprite = allSprites.Length > 0 ? allSprites[0] : null;

                playerScript.PlaytInteraction();

                if (itemSprite != null)
                {
                    slotImage.sprite = itemSprite;

                    // Make sure the outline doesn't stay stuck "on" while the
                    // object is deactivated and later re-dropped.
                    ClearItemHighlight();

                    pickUpObj.SetActive(false);

                    hotbarItems[i] = new HotbarItem
                    {
                        heldObject = pickUpObj,
                        icon = itemSprite
                    };

                    break;
                }
                else
                {
                    Debug.LogWarning("No sprite found for: " + lookupName);
                }
            }
        }
    }

    public void DropSelectedSlot()
    {
        if (PauseMenu.isGamePause) return;

        int index = activePanelReferance.SelectedIndex;
        HotbarItem item = hotbarItems[index];

        if (item == null || item.heldObject == null) return;

        DropFromHotbar(index);
    }

    public void DropFromHotbar(int index)
    {
        HotbarItem item = hotbarItems[index];
        if (item == null || item.heldObject == null) return;

        GameObject obj = item.heldObject;

        if (obj == currentlyEquippedObject) currentlyEquippedObject = null;

        obj.transform.SetParent(null);
        obj.transform.position = dropPosition.position;
        obj.SetActive(true);

        foreach (var col in obj.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = true;
        }

        Image slotImage = hotbarSlots[index].GetComponentInChildren<Image>(true);
        slotImage.sprite = emptySlotSprite;
        hotbarItems[index] = null;

        hotbarSlots[index].SetActive(false);
    }

    private bool isAiming = false;

    public void StartThrowAim()
    {
        if (PauseMenu.isGamePause) return;

        int index = activePanelReferance.SelectedIndex;
        HotbarItem item = hotbarItems[index];
        if (item == null || item.heldObject == null) return;

        // Keys unlock whatever LockDoor is currently highlighted instead of
        // going through the generic consume flow - checked first so a Key
        // never accidentally falls into the IConsumable/heal path.
        KeyScript keyItem = item.heldObject.GetComponent<KeyScript>();
        if (keyItem != null)
        {
            UseKeyOnDoor(index, item.heldObject);
            return;
        }

        // Consumables (MedKit, Bandages, etc.) get used immediately on
        // right-click instead of entering the aim-and-throw flow.
        IConsumable consumable = item.heldObject.GetComponent<IConsumable>();
        if (consumable != null)
        {
            ConsumeSelectedItem(index, consumable);
            return;
        }

        PickupItem pickupInfo = item.heldObject.GetComponent<PickupItem>();
        if (pickupInfo != null && !pickupInfo.canThrow) return; // this item can only be dropped, not thrown

        isAiming = true;
    }

    private void ConsumeSelectedItem(int index, IConsumable consumable)
    {
        HotbarItem item = hotbarItems[index];
        if (item == null || item.heldObject == null) return;

        GameObject obj = item.heldObject;

        // Grab a human-readable label for the log - falls back to the
        // GameObject's name if there's no PickupItem/itemName for some reason.
        string itemLabel = obj.name;
        PickupItem pickupInfo = obj.GetComponent<PickupItem>();
        if (pickupInfo != null && !string.IsNullOrEmpty(pickupInfo.itemName))
            itemLabel = pickupInfo.itemName;

        consumable.Consume(playerScript);

        Debug.Log(itemLabel + " was consumed");

        if (obj == currentlyEquippedObject) currentlyEquippedObject = null;

        Image slotImage = hotbarSlots[index].GetComponentInChildren<Image>(true);
        slotImage.sprite = emptySlotSprite;
        hotbarItems[index] = null;
        hotbarSlots[index].SetActive(false);

        Destroy(obj); // consumed - permanently gone, unlike dropping/throwing
    }

    // Unlocks the door the player is currently looking at (if any) and
    // consumes the key. Does nothing if there's no door in range, or the
    // door in range isn't a LockDoor, or it's already unlocked.
    private void UseKeyOnDoor(int index, GameObject obj)
    {
        if (currentHighlightedDoor == null)
        {
            Debug.Log("No locked door in range to use the key on.");
            return;
        }

        LockDoor lockDoor = currentHighlightedDoor.GetComponent<LockDoor>();
        if (lockDoor == null)
        {
            Debug.Log("This door doesn't use a key.");
            return;
        }

        if (!lockDoor.isLocked)
        {
            Debug.Log("This door is already unlocked.");
            return;
        }

        lockDoor.UnlockWithKey();

        string itemLabel = obj.name;
        PickupItem pickupInfo = obj.GetComponent<PickupItem>();
        if (pickupInfo != null && !string.IsNullOrEmpty(pickupInfo.itemName))
            itemLabel = pickupInfo.itemName;

        Debug.Log(itemLabel + " was consumed");

        if (obj == currentlyEquippedObject) currentlyEquippedObject = null;

        Image slotImage = hotbarSlots[index].GetComponentInChildren<Image>(true);
        slotImage.sprite = emptySlotSprite;
        hotbarItems[index] = null;
        hotbarSlots[index].SetActive(false);

        Destroy(obj);
    }

    public void CancelThrowAim()
    {
        if (!isAiming) return;
        isAiming = false;
        HideTrajectory();
    }

    public void ConfirmThrow()
    {
        if (!isAiming) return;
        isAiming = false;

        HideTrajectory();
        ThrowSelectedSlot();
    }

    public void ThrowSelectedSlot()
    {
        if (PauseMenu.isGamePause) return;

        int index = activePanelReferance.SelectedIndex;
        HotbarItem item = hotbarItems[index];
        if (item == null || item.heldObject == null) return;

        GameObject obj = item.heldObject;
        string originalTag = obj.tag; // should be "canPickUp"

        if (obj == currentlyEquippedObject) currentlyEquippedObject = null;

        obj.transform.SetParent(null);
        obj.transform.position = throwPoint.position;
        obj.SetActive(true);
        obj.tag = throwableTag;

        foreach (var col in obj.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = true;
        }

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) rb = obj.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(throwPoint.forward * throwForce, ForceMode.VelocityChange);

        ThrownItem thrownScript = obj.GetComponent<ThrownItem>();
        if (thrownScript == null) thrownScript = obj.AddComponent<ThrownItem>();
        thrownScript.Setup(originalTag);

        Image slotImage = hotbarSlots[index].GetComponentInChildren<Image>(true);
        slotImage.sprite = emptySlotSprite;
        hotbarItems[index] = null;
        hotbarSlots[index].SetActive(false);
    }

    public void DrawTrajectory()
    {
        trajectoryLine.enabled = true;

        // --- Added Color Feedback ---
        float t = Mathf.Clamp01((throwForce - minThrowForce) / (maxThrowForce - minThrowForce));
        trajectoryLine.startColor = Color.Lerp(Color.green, Color.red, t);
        trajectoryLine.endColor = Color.Lerp(Color.green, Color.red, t);
        // ----------------------------

        Vector3 startPos = throwPoint.position;
        Vector3 startVelocity = throwPoint.forward * throwForce;

        trajectoryLine.positionCount = trajectoryResolution;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i * trajectoryTimeStep;
            Vector3 point = startPos + startVelocity * time + 0.5f * Physics.gravity * time * time;

            if (Physics.Linecast(startPos, point, out RaycastHit hit))
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, hit.point);
                return;
            }

            trajectoryLine.SetPosition(i, point);
        }
    }

    public void HideTrajectory()
    {
        trajectoryLine.enabled = false;
    }
}