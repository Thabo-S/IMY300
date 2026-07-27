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
    public LayerMask pickupLayerMask = ~0; // Set in Inspector to exclude Player layer

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

    [Header("Throwing System")]
    public Transform throwPoint;
    public float throwForce = 30f;
    public float minThrowForce = 15f;
    public float maxThrowForce = 60f;
    public float scrollSensitivity = 15f;

    [Header("Trajectory Preview")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public float trajectoryTimeStep = 0.1f;
    private bool isAiming = false;

    // Internal slot management lists
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

        // Force closed state
        IsOpen = false;
        if (container != null) container.SetActive(false);

        // NOTE: Cursor locking is no longer forced here. This object gets
        // SetActive(true) the moment "Start Tutorial" is pressed, so locking
        // the cursor in Start() hid it before any tutorial intro UI could use
        // the mouse. Call PauseMenu.LockGameplayCursor() explicitly once
        // gameplay should actually take control of the mouse (e.g. from the
        // Start Tutorial button, or after an intro popup is dismissed).

        // Explicitly enable rotation on startup
        SetPlayerRotationState(true);

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
    }

    private void Update()
    {
        // 1. Toggle Inventory UI (Tab Key)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        // Lock camera rotation if inventory is open
        if (!GetPlayerRotationState() && playerLookAround != null)
        {
            playerLookAround.CalculatePlayerLookAround(Vector2.zero);
        }

        // 2. Gameplay Controls (Only active when Inventory is CLOSED)
        if (!IsOpen)
        {
            DetectLookedAtItem();

            // NOTE: Pickup is now handled exclusively via the new Input System
            // (see InputMananger.cs -> pickUp.PickUpObject.performed -> TryPickupItem()).
            // Having a second binding here caused a single E press to fire
            // TryPickupItem() twice, duplicating/over-stacking picked up items.

            HandleHotbarSelection();
            HandleDropEquippedItem();
            HandleThrowingLogic();
        }
        else
        {
            ClearHighlight();
            CancelThrowAim();
        }

        // 3. UI Interactions
        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        UpdateHotbarOpacity();
    }

    #region Inventory Toggle Logic

    public void ToggleInventory()
    {
        if (container == null) return;

        IsOpen = !container.activeInHierarchy;
        container.SetActive(IsOpen);

        // Lock/Unlock cursor state
        Cursor.lockState = IsOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsOpen;

        SetPlayerRotationState(!IsOpen);

        if (IsOpen)
        {
            CancelThrowAim();
        }
    }

    private void SetPlayerRotationState(bool enableRotation)
    {
        if (playerLookAround != null)
        {
            playerLookAround.updatingRotation = enableRotation;
        }
        else if (PlayerLookAround.instance != null)
        {
            PlayerLookAround.instance.updatingRotation = enableRotation;
        }
    }

    private bool GetPlayerRotationState()
    {
        if (playerLookAround != null) return playerLookAround.updatingRotation;
        if (PlayerLookAround.instance != null) return PlayerLookAround.instance.updatingRotation;
        return true;
    }

    #endregion

    #region Throwing Mechanics

    private void HandleThrowingLogic()
    {
        Slot currentSlot = GetEquippedSlot();
        if (currentSlot == null || !currentSlot.HasItem())
        {
            if (isAiming) CancelThrowAim();
            return;
        }

        // Aiming trigger (Hold Right Mouse Button)
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            CancelThrowAim();
        }

        if (isAiming)
        {
            // Scroll to change force
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0f)
            {
                throwForce = Mathf.Clamp(throwForce + (scrollInput * scrollSensitivity), minThrowForce, maxThrowForce);
            }

            DrawTrajectory();

            // Left Click to throw object
            if (Input.GetMouseButtonDown(0))
            {
                ThrowEquippedItem();
            }
        }
    }

    private void ThrowEquippedItem()
    {
        Slot equippedSlot = GetEquippedSlot();
        if (equippedSlot == null || !equippedSlot.HasItem()) return;

        ItemSO itemSO = equippedSlot.GetItem();
        if (itemSO == null || itemSO.itemPrefab == null) return;

        Transform spawnPoint = throwPoint != null ? throwPoint : playerCamera.transform;

        // Instantiate world item
        GameObject thrownObj = Instantiate(itemSO.itemPrefab, spawnPoint.position, spawnPoint.rotation);

        Item itemComponent = thrownObj.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.item = itemSO;
            itemComponent.amount = 1;
        }

        // Rigidbody physics
        Rigidbody rb = thrownObj.GetComponent<Rigidbody>();
        if (rb == null) rb = thrownObj.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(spawnPoint.forward * throwForce, ForceMode.VelocityChange);

        // Deduct 1 item from slot stack
        int remaining = equippedSlot.GetAmount() - 1;
        if (remaining > 0)
        {
            equippedSlot.SetItem(itemSO, remaining);
        }
        else
        {
            equippedSlot.ClearSlot();
        }

        EquipHandItem();
        CancelThrowAim();
    }

    private void DrawTrajectory()
    {
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;

        Transform spawnPoint = throwPoint != null ? throwPoint : playerCamera.transform;

        // Color gradient based on force
        float t = Mathf.Clamp01((throwForce - minThrowForce) / (maxThrowForce - minThrowForce));
        Color trajectoryColor = Color.Lerp(Color.green, Color.red, t);
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = trajectoryColor;

        Vector3 startPos = spawnPoint.position;
        Vector3 startVelocity = spawnPoint.forward * throwForce;

        trajectoryLine.positionCount = trajectoryResolution;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i * trajectoryTimeStep;
            Vector3 point = startPos + startVelocity * time + 0.5f * Physics.gravity * time * time;

            if (Physics.Linecast(startPos, point, out RaycastHit hit, pickupLayerMask))
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, hit.point);
                return;
            }

            trajectoryLine.SetPosition(i, point);
        }
    }

    public void CancelThrowAim()
    {
        isAiming = false;
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
    }

    #endregion

    #region Add Item Logic

    public void AddItem(ItemSO itemToAdd, int amount = 1)
    {
        if (itemToAdd == null) return;

        int remaining = amount;

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

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayerMask))
        {
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
        if (Input.GetMouseButtonDown(0) && IsOpen)
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

    private Slot GetEquippedSlot()
    {
        if (equippedHotbarIndex >= 0 && equippedHotbarIndex < hotbarSlots.Count)
        {
            return hotbarSlots[equippedHotbarIndex];
        }
        return null;
    }

    private void HandleHotbarSelection()
    {
        for (int i = 0; i < hotbarSlots.Count && i < 6; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                equippedHotbarIndex = i;
                UpdateHotbarOpacity();
                EquipHandItem();
                CancelThrowAim();
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

        Slot equippedSlot = GetEquippedSlot();
        if (equippedSlot == null || !equippedSlot.HasItem()) return;

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
        CancelThrowAim();
    }

    private void EquipHandItem()
    {
        if (currentHandItem != null)
        {
            Destroy(currentHandItem);
        }

        Slot equippedSlot = GetEquippedSlot();
        if (equippedSlot == null || !equippedSlot.HasItem() || hand == null) return;

        ItemSO item = equippedSlot.GetItem();
        if (item.handItemPrefab == null) return;

        currentHandItem = Instantiate(item.handItemPrefab, hand);
        currentHandItem.transform.localPosition = Vector3.zero;
        currentHandItem.transform.localRotation = Quaternion.identity;
    }

    #endregion
}