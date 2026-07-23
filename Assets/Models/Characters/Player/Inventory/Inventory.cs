//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class Inventory : MonoBehaviour
//{
//    [Header("Object testing")]
//    public ItemSO woodItem;
//    public ItemSO axeItem;

//    [Header("UI & Containers")]
//    public GameObject container;
//    public GameObject hotbarObject;
//    public GameObject inventorySlotParent;


//    [Header("Drag & Drop Settings")]
//    public Image dragIcon;
//    private Slot dragSlot = null;
//    private bool isDragging = false;

//    [Header("Camera Control")]
//    public Camera playerCamera; // Drag your Camera here in the Inspector
//    public PlayerLookAround playerLookAround;

//    public static bool IsOpen { get; private set; }

//    [Header("Pickup/Throw Interaction")]
//    public PickUpScript pickUpScript;

//    [Header("Pickup Settings")]
//    public float pickupRange = 3f;
//    private Item lookedAtItem = null;
//    public Material highlightMaterial;

//    private Material originalMaterial;
//    private Renderer lookedAtRenderer = null;

//    [Header("Hotbar & Equipment Settings")]
//    private int equippedHotbarIndex = 0;
//    public float equippedOpacity = 0.9f;
//    public float normalOpacity = 0.58f;

//    [Header("Hand Equipment")]
//    public Transform hand;
//    private GameObject currentHandItem;


//    // Internal slot management lists
//    private List<Slot> inventorySlots = new List<Slot>();
//    private List<Slot> hotbarSlots = new List<Slot>();
//    private List<Slot> allSlots = new List<Slot>();

//    private void Awake()
//    {
//        // Populate slot lists from parent hierarchies
//        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
//        hotbarSlots.AddRange(hotbarObject.GetComponentsInChildren<Slot>());

//        allSlots.AddRange(inventorySlots);
//        allSlots.AddRange(hotbarSlots);
//    }

//    private void Update()
//    {
//        // Toggle Inventory UI & Cursor Lock State
//        //if (Input.GetKeyDown(KeyCode.Tab))
//        //{
//        //    container.SetActive(!container.activeInHierarchy);
//        //    IsOpen = container.activeInHierarchy;

//        //    // If the player was mid-aim on a throw when they opened the
//        //    // inventory, cancel it so PickUpScript can't be left in a
//        //    // half-finished aiming state.
//        //    if (IsOpen && pickUpScript != null)
//        //    {
//        //        pickUpScript.CancelThrowAim();
//        //    }

//        //    // Toggle cursor lock mode
//        //    Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked)
//        //        ? CursorLockMode.None
//        //        : CursorLockMode.Locked;

//        //    // Toggle cursor visibility
//        //    Cursor.visible = !Cursor.visible;

//        //    // Toggle camera look rotation when inventory is toggled.
//        //    // (PlayerCam.instance never existed - talking to PlayerLookAround directly.)
//        //    if (playerLookAround != null)
//        //    {
//        //        playerLookAround.updatingRotation = !playerLookAround.updatingRotation;
//        //    }
//        //}

//        if (Input.GetKeyDown(KeyCode.Tab))
//        {
//            container.SetActive(!container.activeInHierarchy);
//            IsOpen = container.activeInHierarchy;

//            if (IsOpen && pickUpScript != null)
//            {
//                pickUpScript.CancelThrowAim();
//            }

//            // Explicitly set cursor based on inventory state
//            if (IsOpen)
//            {
//                Cursor.lockState = CursorLockMode.None;
//                Cursor.visible = true;
//            }
//            else
//            {
//                Cursor.lockState = CursorLockMode.Locked;
//                Cursor.visible = false;
//            }

//            if (playerLookAround != null)
//            {
//                playerLookAround.updatingRotation = !IsOpen;
//            }
//        }

//        // While the inventory is open, keep feeding the camera a zero look
//        // vector every frame. This locks it at whatever rotation it last had
//        // instead of letting any lingering mouse input nudge it. The moment
//        // Tab is pressed again, updatingRotation flips back to true above and
//        // real mouse input takes over again next frame.
//        if (playerLookAround != null && !playerLookAround.updatingRotation)
//        {
//            playerLookAround.CalculatePlayerLookAround(Vector2.zero);
//        }

//        // World Interactions & Hotbar Logic
//        DetectLookedAtItem();
//        Pickup();

//        //Drag and Drop Logic
//        StartDrag();
//        UpdateDragItemPosition();
//        EndDrag();

//        // Hotbar & Equipping Logic
//        HandleHotbarSelection();
//        HandleDropEquippedItem();
//        UpdateHotbarOpacity();
//    }

//    #region Add Item Logic

//    public void AddItem(ItemSO itemToAdd, int amount = 1)
//    {
//        int remaining = amount;

//        // Step 1: Try stacking into existing occupied slots first
//        foreach (Slot slot in allSlots)
//        {
//            if (slot.HasItem() && slot.GetItem() == itemToAdd)
//            {
//                int currentAmount = slot.GetAmount();
//                int maxStack = itemToAdd.maxStackSize;

//                if (currentAmount < maxStack)
//                {
//                    int spaceLeft = maxStack - currentAmount;
//                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

//                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
//                    remaining -= amountToAdd;

//                    if (remaining <= 0) return;
//                }
//            }
//        }

//        // Step 2: Fill remaining amount into empty slots
//        foreach (Slot slot in allSlots)
//        {
//            if (!slot.HasItem())
//            {
//                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
//                slot.SetItem(itemToAdd, amountToPlace);
//                remaining -= amountToPlace;

//                if (remaining <= 0) return;
//            }
//        }

//        // Step 3: Handle full inventory edge case
//        if (remaining > 0)
//        {
//            Debug.Log("Inventory is full! Could not add " + remaining + " of " + itemToAdd.itemName);
//        }
//    }

//    #endregion

//    #region Drag & Drop Logic

//    private void StartDrag()
//    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            Slot hovered = GetHoveredSlot();
//            if (hovered != null && hovered.HasItem())
//            {
//                dragSlot = hovered;
//                isDragging = true;

//                dragIcon.sprite = hovered.GetItem().icon;
//                dragIcon.color = new Color(1f, 1f, 1f, 0.5f);
//                dragIcon.enabled = true;
//            }
//        }
//    }

//    private void UpdateDragItemPosition()
//    {
//        if (isDragging)
//        {
//            dragIcon.transform.position = Input.mousePosition;
//        }
//    }

//    private void EndDrag()
//    {
//        if (Input.GetMouseButtonUp(0) && isDragging)
//        {
//            Slot hovered = GetHoveredSlot();
//            if (hovered != null)
//            {
//                HandleDrop(dragSlot, hovered);

//                dragIcon.enabled = false;
//                dragSlot = null;
//                isDragging = false;
//            }


//        }
//    }

//    private void HandleDrop(Slot from, Slot to)
//    {
//        if (from == to) return;

//        // Case 1: Stacking same items
//        if (to.HasItem() && to.GetItem() == from.GetItem())
//        {
//            int max = to.GetItem().maxStackSize;
//            int space = max - to.GetAmount();

//            if (space > 0)
//            {
//                int move = Mathf.Min(space, from.GetAmount());
//                to.SetItem(to.GetItem(), to.GetAmount() + move);
//                from.SetItem(from.GetItem(), from.GetAmount() - move);

//                if (from.GetAmount() <= 0)
//                {
//                    from.ClearSlot();
//                }
//            }
//            return;
//        }

//        // Case 2: Swapping different items
//        if (to.HasItem())
//        {
//            ItemSO tempItem = to.GetItem();
//            int tempAmount = to.GetAmount();

//            to.SetItem(from.GetItem(), from.GetAmount());
//            from.SetItem(tempItem, tempAmount);
//            return;
//        }

//        // Case 3: Moving to empty slot
//        to.SetItem(from.GetItem(), from.GetAmount());
//        from.ClearSlot();
//    }

//    private Slot GetHoveredSlot()
//    {
//        foreach (Slot s in allSlots)
//        {
//            if (s.hovering)
//            {
//                return s;
//            }
//        }
//        return null;
//    }

//    #endregion

//    #region World Pickup & Highlight

//    //private void DetectLookedAtItem()
//    //{
//    //    // Reset previous highlight
//    //    if (lookedAtRenderer != null)
//    //    {
//    //        lookedAtRenderer.material = originalMaterial;
//    //        lookedAtRenderer = null;
//    //        originalMaterial = null;
//    //    }

//    //    Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

//    //    if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
//    //    {
//    //        Item item = hit.collider.GetComponent<Item>();
//    //        if (item != null)
//    //        {
//    //            Renderer rend = item.GetComponent<Renderer>();
//    //            if (rend != null)
//    //            {
//    //                originalMaterial = rend.material;
//    //                rend.material = highlightMaterial;
//    //                lookedAtRenderer = rend;
//    //            }
//    //        }
//    //    }
//    //}

//    private void DetectLookedAtItem()
//    {
//        // Reset previous highlight
//        if (lookedAtRenderer != null)
//        {
//            lookedAtRenderer.material = originalMaterial;
//            lookedAtRenderer = null;
//            originalMaterial = null;
//        }

//        // Fallback to Camera.main if playerCamera is not set
//        Camera cam = playerCamera != null ? playerCamera : Camera.main;
//        if (cam == null) return; // Guard clause against null camera

//        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

//        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
//        {
//            Item item = hit.collider.GetComponent<Item>();
//            if (item != null)
//            {
//                Renderer rend = item.GetComponent<Renderer>();
//                if (rend != null)
//                {
//                    originalMaterial = rend.material;
//                    rend.material = highlightMaterial;
//                    lookedAtRenderer = rend;
//                }
//            }
//        }
//    }

//    private void Pickup()
//    {
//        if (lookedAtRenderer != null && Input.GetKeyDown(KeyCode.E))
//        {
//            Item item = lookedAtRenderer.GetComponent<Item>();
//            if (item != null)
//            {
//                AddItem(item.item, item.amount);
//                Destroy(item.gameObject);
//                //EquipHandItem();
//            }
//        }
//    }

//    #endregion

//    #region Hotbar & Hand Equipment

//    private void HandleHotbarSelection()
//    {
//        for (int i = 0; i < 6; i++)
//        {
//            if (Input.GetKeyDown((i + 1).ToString()))
//            {
//                equippedHotbarIndex = i;
//                UpdateHotbarOpacity();
//                EquipHandItem();
//            }
//        }
//    }

//    private void UpdateHotbarOpacity()
//    {
//        for (int i = 0; i < hotbarSlots.Count; i++)
//        {
//            Image icon = hotbarSlots[i].GetComponent<Image>();
//            if (icon != null)
//            {
//                icon.color = (i == equippedHotbarIndex)
//                    ? new Color(1f, 1f, 1f, equippedOpacity)
//                    : new Color(1f, 1f, 1f, normalOpacity);
//            }
//        }
//    }

//    //private void HandleDropEquippedItem()
//    //{
//    //    if (!Input.GetKeyDown(KeyCode.Q)) return;

//    //    Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
//    //    if (!equippedSlot.HasItem()) return;

//    //    ItemSO itemSO = equippedSlot.GetItem();
//    //    GameObject prefab = itemSO.itemPrefab;
//    //    if (prefab == null) return;

//    //    // Instantiate world drop in front of player
//    //    GameObject dropped = Instantiate(
//    //        prefab,
//    //        Camera.main.transform.position + Camera.main.transform.forward,
//    //        Quaternion.identity
//    //    );

//    //    Item item = dropped.GetComponent<Item>();
//    //    item.item = itemSO;
//    //    item.amount = equippedSlot.GetAmount();

//    //    equippedSlot.ClearSlot();
//    //    EquipHandItem();
//    //}

//    private void HandleDropEquippedItem()
//    {
//        // Return early if the drop key (Q) wasn't pressed
//        if (!Input.GetKeyDown(KeyCode.Q)) return;

//        // Retrieve the slot corresponding to the currently selected hotbar index
//        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
//        if (!equippedSlot.HasItem()) return;

//        ItemSO itemSO = equippedSlot.GetItem();
//        GameObject prefab = itemSO.itemPrefab;
//        if (prefab == null) return;

//        // Use playerCamera if assigned, otherwise fall back to Camera.main
//        Camera cam = playerCamera != null ? playerCamera : Camera.main;
//        if (cam == null) return;

//        // Instantiate world drop object slightly in front of the camera
//        GameObject dropped = Instantiate(
//            prefab,
//            cam.transform.position + cam.transform.forward,
//            Quaternion.identity
//        );

//        // Pass the item data and current stack quantity to the spawned world item
//        Item item = dropped.GetComponent<Item>();
//        if (item != null)
//        {
//            item.item = itemSO;
//            item.amount = equippedSlot.GetAmount();
//        }

//        // Clear the hotbar slot and update the player's held hand model
//        equippedSlot.ClearSlot();
//        EquipHandItem();
//    }

//    private void EquipHandItem()
//    {
//        // Destroy existing hand model
//        if (currentHandItem != null)
//        {
//            Destroy(currentHandItem);
//        }

//        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
//        if (!equippedSlot.HasItem()) return;

//        ItemSO item = equippedSlot.GetItem();
//        if (item.handItemPrefab == null) return;

//        // Instantiate held object model under the hand transform
//        currentHandItem = Instantiate(item.handItemPrefab, hand);
//        currentHandItem.transform.localPosition = Vector3.zero;
//        currentHandItem.transform.localRotation = Quaternion.identity;
//    }

//    #endregion
//}

//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class Inventory : MonoBehaviour
//{
//    [Header("UI & Containers")]
//    public GameObject container;
//    public GameObject hotbarObject;
//    public GameObject inventorySlotParent;

//    [Header("Drag & Drop Settings")]
//    public Image dragIcon;
//    private Slot dragSlot = null;
//    private bool isDragging = false;

//    [Header("Camera & Player Control")]
//    public Camera playerCamera;
//    public PlayerLookAround playerLookAround;

//    // Lets other scripts check whether the inventory UI is open
//    public static bool IsOpen { get; private set; }

//    [Header("Pickup & Interaction Settings")]
//    public float pickupRange = 3f;
//    public Material highlightMaterial;
//    public KeyCode pickupKey = KeyCode.E;
//    public KeyCode dropKey = KeyCode.Q;

//    private Item lookedAtItem = null;
//    private Material originalMaterial;
//    private Renderer lookedAtRenderer = null;

//    [Header("Hotbar & Equipment Settings")]
//    private int equippedHotbarIndex = 0;
//    public float equippedOpacity = 0.9f;
//    public float normalOpacity = 0.58f;

//    [Header("Hand Equipment")]
//    public Transform hand;
//    private GameObject currentHandItem;

//    // Internal slot management lists
//    private List<Slot> inventorySlots = new List<Slot>();
//    private List<Slot> hotbarSlots = new List<Slot>();
//    private List<Slot> allSlots = new List<Slot>();

//    private void Awake()
//    {
//        // Populate slot lists from parent hierarchies
//        if (inventorySlotParent != null)
//            inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());

//        if (hotbarObject != null)
//            hotbarSlots.AddRange(hotbarObject.GetComponentsInChildren<Slot>());

//        allSlots.AddRange(inventorySlots);
//        allSlots.AddRange(hotbarSlots);
//    }

//    private void Update()
//    {
//        // 1. Toggle Inventory UI & Cursor State
//        if (Input.GetKeyDown(KeyCode.Tab))
//        {
//            ToggleInventory();
//        }

//        // Keep camera locked in place while inventory is open
//        if (playerLookAround != null && !playerLookAround.updatingRotation)
//        {
//            playerLookAround.CalculatePlayerLookAround(Vector2.zero);
//        }

//        // 2. World Interactions (Only active when inventory is CLOSED)
//        if (!IsOpen)
//        {
//            DetectLookedAtItem();
//            Pickup();
//            HandleHotbarSelection();
//            HandleDropEquippedItem();
//        }
//        else
//        {
//            // Clear highlights if inventory is opened while looking at an item
//            ClearHighlight();
//        }

//        // 3. UI Drag and Drop Logic
//        StartDrag();
//        UpdateDragItemPosition();
//        EndDrag();

//        // 4. UI Hotbar Visual Update
//        UpdateHotbarOpacity();
//    }

//    #region Inventory Toggle Logic

//    //private void ToggleInventory()
//    //{
//    //    if (container == null) return;

//    //    container.SetActive(!container.activeInHierarchy);
//    //    IsOpen = container.activeInHierarchy;

//    //    // Set cursor visibility and lock state
//    //    Cursor.lockState = IsOpen ? CursorLockMode.None : CursorLockMode.Locked;
//    //    Cursor.visible = IsOpen;

//    //    // Toggle camera movement script
//    //    if (playerLookAround != null)
//    //    {
//    //        playerLookAround.updatingRotation = !IsOpen;
//    //    }
//    //}

//    private void ToggleInventory()
//    {
//        if (container == null) return;

//        container.SetActive(!container.activeInHierarchy);
//        IsOpen = container.activeInHierarchy;

//        // Toggle cursor state
//        Cursor.lockState = IsOpen ? CursorLockMode.None : CursorLockMode.Locked;
//        Cursor.visible = IsOpen;

//        // Video approach: Toggle updatingRotation directly through the static instance
//        if (PlayerLookAround.instance != null)
//        {
//            PlayerLookAround.instance.updatingRotation = !IsOpen;
//        }
//    }

//    #endregion

//    #region Add Item Logic

//    public void AddItem(ItemSO itemToAdd, int amount = 1)
//    {
//        if (itemToAdd == null) return;

//        int remaining = amount;

//        // Step 1: Try stacking into existing occupied slots first
//        foreach (Slot slot in allSlots)
//        {
//            if (slot.HasItem() && slot.GetItem() == itemToAdd)
//            {
//                int currentAmount = slot.GetAmount();
//                int maxStack = itemToAdd.maxStackSize;

//                if (currentAmount < maxStack)
//                {
//                    int spaceLeft = maxStack - currentAmount;
//                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

//                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
//                    remaining -= amountToAdd;

//                    if (remaining <= 0) return;
//                }
//            }
//        }

//        // Step 2: Fill remaining amount into empty slots
//        foreach (Slot slot in allSlots)
//        {
//            if (!slot.HasItem())
//            {
//                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
//                slot.SetItem(itemToAdd, amountToPlace);
//                remaining -= amountToPlace;

//                if (remaining <= 0) return;
//            }
//        }

//        if (remaining > 0)
//        {
//            Debug.Log("Inventory is full! Could not add " + remaining + " of " + itemToAdd.itemName);
//        }
//    }

//    #endregion

//    #region World Pickup & Highlight

//    private void DetectLookedAtItem()
//    {
//        ClearHighlight();

//        Camera cam = playerCamera != null ? playerCamera : Camera.main;
//        if (cam == null) return;

//        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

//        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
//        {
//            // Search parent or self for Item script in case collider is on a child object
//            Item item = hit.collider.GetComponentInParent<Item>();
//            if (item != null)
//            {
//                lookedAtItem = item;
//                Renderer rend = item.GetComponentInChildren<Renderer>();
//                if (rend != null)
//                {
//                    originalMaterial = rend.material;
//                    rend.material = highlightMaterial;
//                    lookedAtRenderer = rend;
//                }
//            }
//        }
//    }

//    private void ClearHighlight()
//    {
//        if (lookedAtRenderer != null)
//        {
//            lookedAtRenderer.material = originalMaterial;
//            lookedAtRenderer = null;
//            originalMaterial = null;
//        }
//        lookedAtItem = null;
//    }

//    private void Pickup()
//    {
//        if (lookedAtItem != null && Input.GetKeyDown(pickupKey))
//        {
//            AddItem(lookedAtItem.item, lookedAtItem.amount);
//            Destroy(lookedAtItem.gameObject);
//            ClearHighlight();
//            EquipHandItem();
//        }
//    }

//    #endregion

//    #region Drag & Drop Logic (UI)

//    private void StartDrag()
//    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            Slot hovered = GetHoveredSlot();
//            if (hovered != null && hovered.HasItem())
//            {
//                dragSlot = hovered;
//                isDragging = true;

//                if (dragIcon != null)
//                {
//                    dragIcon.sprite = hovered.GetItem().icon;
//                    dragIcon.color = new Color(1f, 1f, 1f, 0.5f);
//                    dragIcon.enabled = true;
//                }
//            }
//        }
//    }

//    private void UpdateDragItemPosition()
//    {
//        if (isDragging && dragIcon != null)
//        {
//            dragIcon.transform.position = Input.mousePosition;
//        }
//    }

//    private void EndDrag()
//    {
//        if (Input.GetMouseButtonUp(0) && isDragging)
//        {
//            Slot hovered = GetHoveredSlot();
//            if (hovered != null)
//            {
//                HandleDrop(dragSlot, hovered);
//            }

//            if (dragIcon != null) dragIcon.enabled = false;
//            dragSlot = null;
//            isDragging = false;

//            EquipHandItem();
//        }
//    }

//    private void HandleDrop(Slot from, Slot to)
//    {
//        if (from == to || from == null || to == null) return;

//        // Stacking same items
//        if (to.HasItem() && to.GetItem() == from.GetItem())
//        {
//            int max = to.GetItem().maxStackSize;
//            int space = max - to.GetAmount();

//            if (space > 0)
//            {
//                int move = Mathf.Min(space, from.GetAmount());
//                to.SetItem(to.GetItem(), to.GetAmount() + move);
//                from.SetItem(from.GetItem(), from.GetAmount() - move);

//                if (from.GetAmount() <= 0)
//                {
//                    from.ClearSlot();
//                }
//            }
//            return;
//        }

//        // Swapping items
//        if (to.HasItem())
//        {
//            ItemSO tempItem = to.GetItem();
//            int tempAmount = to.GetAmount();

//            to.SetItem(from.GetItem(), from.GetAmount());
//            from.SetItem(tempItem, tempAmount);
//            return;
//        }

//        // Moving to empty slot
//        to.SetItem(from.GetItem(), from.GetAmount());
//        from.ClearSlot();
//    }

//    private Slot GetHoveredSlot()
//    {
//        foreach (Slot s in allSlots)
//        {
//            if (s != null && s.hovering)
//            {
//                return s;
//            }
//        }
//        return null;
//    }

//    #endregion

//    #region Hotbar & Equipment Logic

//    private void HandleHotbarSelection()
//    {
//        for (int i = 0; i < hotbarSlots.Count && i < 6; i++)
//        {
//            if (Input.GetKeyDown((i + 1).ToString()))
//            {
//                equippedHotbarIndex = i;
//                UpdateHotbarOpacity();
//                EquipHandItem();
//            }
//        }
//    }

//    private void UpdateHotbarOpacity()
//    {
//        for (int i = 0; i < hotbarSlots.Count; i++)
//        {
//            Image icon = hotbarSlots[i].GetComponent<Image>();
//            if (icon != null)
//            {
//                icon.color = (i == equippedHotbarIndex)
//                    ? new Color(1f, 1f, 1f, equippedOpacity)
//                    : new Color(1f, 1f, 1f, normalOpacity);
//            }
//        }
//    }

//    private void HandleDropEquippedItem()
//    {
//        if (!Input.GetKeyDown(dropKey)) return;
//        if (equippedHotbarIndex < 0 || equippedHotbarIndex >= hotbarSlots.Count) return;

//        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
//        if (!equippedSlot.HasItem()) return;

//        ItemSO itemSO = equippedSlot.GetItem();
//        GameObject prefab = itemSO.itemPrefab;
//        if (prefab == null) return;

//        Camera cam = playerCamera != null ? playerCamera : Camera.main;
//        if (cam == null) return;

//        // Instantiate world drop object slightly in front of camera
//        GameObject dropped = Instantiate(
//            prefab,
//            cam.transform.position + cam.transform.forward * 1.5f,
//            Quaternion.identity
//        );

//        Item item = dropped.GetComponent<Item>();
//        if (item != null)
//        {
//            item.item = itemSO;
//            item.amount = equippedSlot.GetAmount();
//        }

//        equippedSlot.ClearSlot();
//        EquipHandItem();
//    }

//    private void EquipHandItem()
//    {
//        if (currentHandItem != null)
//        {
//            Destroy(currentHandItem);
//        }

//        if (equippedHotbarIndex < 0 || equippedHotbarIndex >= hotbarSlots.Count) return;

//        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
//        if (!equippedSlot.HasItem() || hand == null) return;

//        ItemSO item = equippedSlot.GetItem();
//        if (item.handItemPrefab == null) return;

//        currentHandItem = Instantiate(item.handItemPrefab, hand);
//        currentHandItem.transform.localPosition = Vector3.zero;
//        currentHandItem.transform.localRotation = Quaternion.identity;
//    }

//    #endregion
//}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("UI & Containers")]
    public GameObject container;
    public GameObject hotbarObject;
    public GameObject inventorySlotParent;

    [Header("Drag & Drop Settings")]
    public Image dragIcon;
    private Slot dragSlot = null;
    private bool isDragging = false;

    [Header("Camera & Player Control")]
    public Camera playerCamera;
    public PlayerLookAround playerLookAround;

    public static bool IsOpen { get; private set; }

    [Header("Pickup & Interaction Settings")]
    public float pickupRange = 3f;
    public Material highlightMaterial;
    public LayerMask pickupLayerMask = ~0; // Set this in Inspector to exclude Player layer

    private Item lookedAtItem = null;
    private Material originalMaterial;
    private Renderer lookedAtRenderer = null;

    [Header("Hotbar & Equipment Settings")]
    private int equippedHotbarIndex = 0;
    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;

    [Header("Hand Equipment")]
    public Transform hand;
    private GameObject currentHandItem;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private void Awake()
    {
        if (inventorySlotParent != null)
            inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());

        if (hotbarObject != null)
            hotbarSlots.AddRange(hotbarObject.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Toggle Inventory UI (Tab key)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (playerLookAround != null && !playerLookAround.updatingRotation)
        {
            playerLookAround.CalculatePlayerLookAround(Vector2.zero);
        }

        if (!IsOpen)
        {
            DetectLookedAtItem();

            // Legacy input check as fallback
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryPickupItem();
            }

            HandleHotbarSelection();
            HandleDropEquippedItem();
        }
        else
        {
            ClearHighlight();
        }

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        UpdateHotbarOpacity();
    }

    public void ToggleInventory()
    {
        if (container == null) return;

        container.SetActive(!container.activeInHierarchy);
        IsOpen = container.activeInHierarchy;

        Cursor.lockState = IsOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsOpen;

        if (PlayerLookAround.instance != null)
        {
            PlayerLookAround.instance.updatingRotation = !IsOpen;
        }
    }

    #region Add Item Logic

    public void AddItem(ItemSO itemToAdd, int amount = 1)
    {
        if (itemToAdd == null) return;

        int remaining = amount;

        // 1. Stack into existing occupied slots
        foreach (Slot slot in allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStackSize;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;

                    if (remaining <= 0) return;
                }
            }
        }

        // 2. Fill empty slots
        foreach (Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
                slot.SetItem(itemToAdd, amountToPlace);
                remaining -= amountToPlace;

                if (remaining <= 0) return;
            }
        }

        if (remaining > 0)
        {
            Debug.LogWarning($"Inventory is full! Could not add {remaining} of {itemToAdd.itemName}");
        }
    }

    #endregion

    #region World Pickup & Highlight

    private void DetectLookedAtItem()
    {
        ClearHighlight();

        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        // Draws a red ray in Scene View during Play mode showing where you are aiming
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayerMask))
        {
            // Debug log to confirm what the ray hits
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            Item item = hit.collider.GetComponentInParent<Item>();
            if (item != null)
            {
                lookedAtItem = item;
                Renderer rend = item.GetComponentInChildren<Renderer>();
                if (rend != null && highlightMaterial != null)
                {
                    originalMaterial = rend.material;
                    rend.material = highlightMaterial;
                    lookedAtRenderer = rend;
                }
            }
        }
    }

    private void ClearHighlight()
    {
        if (lookedAtRenderer != null && originalMaterial != null)
        {
            lookedAtRenderer.material = originalMaterial;
            lookedAtRenderer = null;
            originalMaterial = null;
        }
        lookedAtItem = null;
    }

    public void TryPickupItem()
    {
        if (lookedAtItem != null)
        {
            AddItem(lookedAtItem.item, lookedAtItem.amount);
            Destroy(lookedAtItem.gameObject);
            ClearHighlight();
            EquipHandItem();
        }
    }

    #endregion

    #region Drag & Drop Logic (UI)

    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Slot hovered = GetHoveredSlot();
            if (hovered != null && hovered.HasItem())
            {
                dragSlot = hovered;
                isDragging = true;

                if (dragIcon != null)
                {
                    dragIcon.sprite = hovered.GetItem().icon;
                    dragIcon.color = new Color(1f, 1f, 1f, 0.5f);
                    dragIcon.enabled = true;
                }
            }
        }
    }

    private void UpdateDragItemPosition()
    {
        if (isDragging && dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    private void EndDrag()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Slot hovered = GetHoveredSlot();
            if (hovered != null)
            {
                HandleDrop(dragSlot, hovered);
            }

            if (dragIcon != null) dragIcon.enabled = false;
            dragSlot = null;
            isDragging = false;

            EquipHandItem();
        }
    }

    private void HandleDrop(Slot from, Slot to)
    {
        if (from == to || from == null || to == null) return;

        if (to.HasItem() && to.GetItem() == from.GetItem())
        {
            int max = to.GetItem().maxStackSize;
            int space = max - to.GetAmount();

            if (space > 0)
            {
                int move = Mathf.Min(space, from.GetAmount());
                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);

                if (from.GetAmount() <= 0)
                {
                    from.ClearSlot();
                }
            }
            return;
        }

        if (to.HasItem())
        {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            to.SetItem(from.GetItem(), from.GetAmount());
            from.SetItem(tempItem, tempAmount);
            return;
        }

        to.SetItem(from.GetItem(), from.GetAmount());
        from.ClearSlot();
    }

    private Slot GetHoveredSlot()
    {
        foreach (Slot s in allSlots)
        {
            if (s != null && s.hovering)
            {
                return s;
            }
        }
        return null;
    }

    #endregion

    #region Hotbar & Equipment Logic

    private void HandleHotbarSelection()
    {
        for (int i = 0; i < hotbarSlots.Count && i < 6; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                equippedHotbarIndex = i;
                UpdateHotbarOpacity();
                EquipHandItem();
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
                icon.color = (i == equippedHotbarIndex)
                    ? new Color(1f, 1f, 1f, equippedOpacity)
                    : new Color(1f, 1f, 1f, normalOpacity);
            }
        }
    }

    private void HandleDropEquippedItem()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return;
        if (equippedHotbarIndex < 0 || equippedHotbarIndex >= hotbarSlots.Count) return;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem()) return;

        ItemSO itemSO = equippedSlot.GetItem();
        GameObject prefab = itemSO.itemPrefab;
        if (prefab == null) return;

        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        GameObject dropped = Instantiate(
            prefab,
            cam.transform.position + cam.transform.forward * 1.5f,
            Quaternion.identity
        );

        Item item = dropped.GetComponent<Item>();
        if (item != null)
        {
            item.item = itemSO;
            item.amount = equippedSlot.GetAmount();
        }

        equippedSlot.ClearSlot();
        EquipHandItem();
    }

    private void EquipHandItem()
    {
        if (currentHandItem != null)
        {
            Destroy(currentHandItem);
        }

        if (equippedHotbarIndex < 0 || equippedHotbarIndex >= hotbarSlots.Count) return;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem() || hand == null) return;

        ItemSO item = equippedSlot.GetItem();
        if (item.handItemPrefab == null) return;

        currentHandItem = Instantiate(item.handItemPrefab, hand);
        currentHandItem.transform.localPosition = Vector3.zero;
        currentHandItem.transform.localRotation = Quaternion.identity;
    }

    #endregion
}