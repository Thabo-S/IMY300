using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using UnityEngine.UI;


public class Inventory : MonoBehaviour
{
    public ItemSO woodItem;
    public ItemSO Axe;


    public GameObject hotbarObj;
    public GameObject inventorySlotParent;
    public GameObject container;

    public Image dragIcon;

    public float pickupRange = 3f;
    private Item lookedAtItem = null;
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer lookedAtRenderer = null;

    private int equippedHotbarIndex = 0; //0-2
    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;


    public List<Slot> inventorySlots = new List<Slot>();
    public List<Slot> hotbarSlots = new List<Slot>();
    public List<Slot> allSlots = new List<Slot>();

    public Slot draggedSlot = null;
    public bool isDragging = false;

    public void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            container.SetActive(!container.activeInHierarchy);
            Cursor.lockState = CursorLockMode.Locked == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
            PlayerCam.instance.updatingRotation = !PlayerCam.instance.updatingRotation;
        }
        DetectLookedAtItem();
        Pickup();

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        HandleHotBarSelection();
        HandleDropEquippedItem();
        UpdateHotbarOpacity();
    }

    public void Additem(ItemSO itemToAdd, int amount) 
    {
        int remainingAmount = amount;

        foreach(Slot slot in allSlots)
        {
            if(slot.HasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetItemAmount();
                int maxStack = itemToAdd.maxStackSize;

                if(currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remainingAmount);

                    slot.SetItemAmount(itemToAdd, currentAmount + amountToAdd);
                    remainingAmount -= amountToAdd;

                    if(remainingAmount <= 0)
                    {
                        return;
                    }
                }
            }
        }

        foreach(Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize,remainingAmount);
                slot.SetItemAmount(itemToAdd,amountToPlace);
                remainingAmount -= amountToPlace;

                if(remainingAmount <= 0)
                {
                    return;
                }
            }
        }

        if(remainingAmount > 0)
        {
            Debug.Log("Inventory Is full could not add " + remainingAmount + " of " + itemToAdd.itemName);
        }
    }
    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Slot hovered = GetHoverdSlot();

            if(hovered != null && hovered.HasItem())
            {
                draggedSlot = hovered;
                isDragging = true;

                //show dragged item
                dragIcon.sprite = hovered.GetItem().icon;
                dragIcon.color = new Color(1, 1, 1, 0.5f);
                dragIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if(Input.GetMouseButtonUp(0) && isDragging)
        {
            Slot hovered = GetHoverdSlot();

            if(hovered != null)
            {
                HandleDrop(draggedSlot, hovered);

                dragIcon.enabled = false;

                draggedSlot = null;
                isDragging = false;
            }
        }
    }

    private Slot GetHoverdSlot()
    {
        foreach(Slot s in allSlots)
        {
            if (s.hovering)
            {
                return s;
            }
        }
        return null;
    }

    private void HandleDrop(Slot from, Slot to)
    {
        if (from == to) return;


        //Stacking items
        if(to.HasItem() && to.GetItem() == from.GetItem())
        {
            int max = to.GetItem().maxStackSize;
            int space = max - to.GetItemAmount();

            if(space > 0)
            {
                int move = Mathf.Min(space, from.GetItemAmount());

                to.SetItemAmount(to.GetItem(),to.GetItemAmount() + move);
                from.SetItemAmount(from.GetItem(),from.GetItemAmount() - move);

                if(from.GetItemAmount() <= 0)
                {
                    from.ClearSlot();
                }
                return;
            }
        }

        //Different items
        if (to.HasItem())
        {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetItemAmount();

            to.SetItemAmount(from.GetItem(), from.GetItemAmount());
            from.SetItemAmount(tempItem, tempAmount);

            return;
        }

        //Empty Slot
        to.SetItemAmount(from.GetItem(), from.GetItemAmount());
        from.ClearSlot();
    }

    private void UpdateDragItemPosition()
    {
        if (isDragging)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    private void Pickup()
    {
        if (lookedAtRenderer != null && Input.GetKeyDown(KeyCode.E))
        {
            Item item = lookedAtRenderer.GetComponent<Item>();
            if (item != null)
            {
                Additem(item.item, item.amount);
                Destroy(item.gameObject);
            }
        }
    }

    private void DetectLookedAtItem()
    {
        if(lookedAtRenderer != null)
        {
            lookedAtRenderer.material = originalMaterial;
            lookedAtRenderer = null;
            originalMaterial = null;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            Item item = hit.collider.GetComponent<Item>();
            if(item != null)
            {
                Renderer rend = item.GetComponent<Renderer>();
                if(rend != null)
                {
                    originalMaterial = rend.material;
                    rend.material = highlightMaterial;
                    lookedAtRenderer = rend;
                }
            }
            
        }
    }

    private void UpdateHotbarOpacity()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Image icon = hotbarSlots[i].GetComponent<Image>();
            if (icon != null)
            {
                icon.color = (i == equippedHotbarIndex) ? new Color(1, 1, 1, equippedOpacity) : new Color(1, 1, 1, normalOpacity);
            }
        }
    }

    private void HandleHotBarSelection()
    {
        for(int i = 0; i < 3; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                equippedHotbarIndex = i;
                UpdateHotbarOpacity();
            }
        }
    }

    private void HandleDropEquippedItem()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];

        if (!equippedSlot.HasItem()) return;

        ItemSO itemSO = equippedSlot.GetItem();
        GameObject prefab = itemSO.itemPrefab;

        if (prefab == null) return;

        GameObject dropped = Instantiate(prefab, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity);

        Item item = dropped.GetComponent<Item>();
        item.item = itemSO;
        item.amount = equippedSlot.GetItemAmount();

        equippedSlot.ClearSlot();
    }

}
