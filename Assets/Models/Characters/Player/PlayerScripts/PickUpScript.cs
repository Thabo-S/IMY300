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
    private GameObject currentHighlightedItem;
    private GameObject currentHighlightedDoor;
    private Player playerScript;

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
            if (hitObject.CompareTag("canPickUp") && distance <= pickUpRange)
            {
                if (hitObject != currentHighlightedItem)
                {
                    ClearItemHighlight();
                    currentHighlightedItem = hitObject;
                    ApplyItemHighlight(currentHighlightedItem);
                }
            }
            else if (currentHighlightedItem != null) // If out of range or not hitting
            {
                ClearItemHighlight();
            }

            // --- Handle Doors ---
            if (hitObject.CompareTag("Door_Left_Swing") || hitObject.CompareTag("Door_Right_Swing") && distance <= doorOpenRange)
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
    private void ApplyItemHighlight(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            //outline.OutlineColor = Color.white;
            outline.enabled = true;
        }
    }

    private void ClearItemHighlight()
    {
        if (currentHighlightedItem != null)
        {
            var outline = currentHighlightedItem.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
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
                if (hit.transform.CompareTag("Door_Left_Swing") || hit.transform.CompareTag("Door_Left_Swing"))
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
            if (hit.transform.gameObject.tag == "canPickUp")
            {
                PickUpObject(hit.transform.gameObject);
            }
        }
    }

    void PickUpObject(GameObject pickUpObj)
    {
        if (PauseMenu.isGamePause) return;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            GameObject slot = hotbarSlots[i];
            Image slotImage = slot.GetComponentInChildren<Image>(true);
            if (slotImage == null) continue;

            if (slotImage.sprite == emptySlotSprite)
            {
                slot.SetActive(true);

                Sprite[] allSprites = Resources.LoadAll<Sprite>("Items/sprites/" + pickUpObj.name);
                Sprite itemSprite = allSprites.Length > 0 ? allSprites[0] : null;

                playerScript.PlaytInteraction();

                if (itemSprite != null)
                {
                    slotImage.sprite = itemSprite;
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
                    Debug.LogWarning("No sprite found for: " + pickUpObj.name);
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

        item.heldObject.transform.position = dropPosition.position;
        item.heldObject.SetActive(true);

        Image slotImage = hotbarSlots[index].GetComponentInChildren<Image>(true);
        slotImage.sprite = emptySlotSprite;
        hotbarItems[index] = null;

        hotbarSlots[index].SetActive(false);
    }

    private bool isAiming = false;

    public void StartThrowAim()
    {
        if (PauseMenu.isGamePause) return;

        HotbarItem item = hotbarItems[activePanelReferance.SelectedIndex];
        if (item == null || item.heldObject == null) return;

        isAiming = true;
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

        obj.transform.position = throwPoint.position;
        obj.SetActive(true);
        obj.tag = throwableTag;

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