using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUpScript : MonoBehaviour
{
    public Transform dropPosition;
    public activePanel activePanelReferance;

    public float pickUpRange = 10f;
    [SerializeField] private float doorOpenRange = 25f;

    public List<GameObject> hotbarSlots;
    public Sprite emptySlotSprite;
    public HotbarItem[] hotbarItems = new HotbarItem[5];

    private GameObject currentHighlightedItem;
    private GameObject currentHighlightedDoor;

    public class HotbarItem
    {
        public GameObject heldObject;
        public Sprite icon;
    }


    void Update()
    {
        PerformContinuousDetection();
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
}