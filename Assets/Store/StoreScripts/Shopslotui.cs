using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descriptionText;
    public Button buyButton;
    [Tooltip("Optional - shown instead of/alongside the buy button once owned.")]
    public GameObject ownedIndicator;

    [Header("Info Toggle")]
    [Tooltip("The Button that toggles between normal view and description view.")]
    public Button infoButton;
    [Tooltip("Container holding everything shown normally - name, price, buy " +
             "button. Hidden while the description is showing.")]
    public GameObject normalViewRoot;
    [Tooltip("Container holding just the description text. Hidden by default, " +
             "shown in place of normalViewRoot when Info is clicked.")]
    public GameObject descriptionViewRoot;

    private ItemSO item;
    private Func<ItemSO, bool> onPurchaseAttempt;
    private bool showingDescription = false;

    [Header("Highlight Settings")]
    [Tooltip("The border/background shown while hovering OR while this slot " +
             "is the selected one. Expected child named \"SelectionBar\".")]
    public GameObject selectionBar;
    [Tooltip("Optional - the Image component used for a material-based hover " +
             "swap instead of/alongside SelectionBar. Leave unassigned if " +
             "you're only using SelectionBar.")]
    public Image highlightTarget;
    public Material highlightMaterial;

    [Header("Purchase Feedback")]
    [Tooltip("Color the Buy button flashes when a purchase fails (e.g. can't afford it).")]
    public Color failFlashColor = Color.red;
    [Tooltip("How long the fail flash lasts before returning to normal.")]
    public float flashDuration = 0.4f;

    private Material defaultMaterial;
    private Color buyButtonDefaultColor;
    private Coroutine flashRoutine;

    private bool isHovering = false;
    private bool isSelected = false;

    // Only one slot across the whole grid is "selected" (clicked) at a time -
    // clicking a new one deselects whichever was selected before.
    private static ShopSlotUI currentlySelected;

    private void Awake()
    {

        if (highlightTarget != null)
        {
            defaultMaterial = highlightTarget.material;
        }

        if (icon == null) icon = FindComponentInChildByName<Image>("Icon");
        if (nameText == null) nameText = FindComponentInChildByName<TextMeshProUGUI>("ItemName");
        if (priceText == null) priceText = FindComponentInChildByName<TextMeshProUGUI>("Price");
        if (buyButton == null) buyButton = FindComponentInChildByName<Button>("BuyBTN");
        if (ownedIndicator == null) ownedIndicator = FindChildByName("Owned");
        if (infoButton == null) infoButton = FindComponentInChildByName<Button>("DetailsBTN");
        if (normalViewRoot == null) normalViewRoot = FindChildByName("NormalView");
        if (descriptionViewRoot == null) descriptionViewRoot = FindChildByName("DetailsView");
        if (selectionBar == null) selectionBar = FindChildByName("SelectionBar");

        if (descriptionText == null && descriptionViewRoot != null)
        {
            // Description text lives inside DetailsView rather than being
            // named uniquely itself, so search within that subtree.
            descriptionText = descriptionViewRoot.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (buyButton != null && buyButton.image != null)
        {
            buyButtonDefaultColor = buyButton.image.color;
        }

        RefreshSelectionBar();
        LogMissingReferences();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        RefreshSelectionBar();

        //if (highlightTarget != null && highlightMaterial != null)
        //{
        //    highlightTarget.material = highlightMaterial;
        //}
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        RefreshSelectionBar();

        //if (highlightTarget != null)
        //{
        //    highlightTarget.material = defaultMaterial;
        //}
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Clicking directly on BuyBTN/DetailsBTN is handled by their own
        // Button.onClick listeners and won't reach here (Unity's EventSystem
        // sends the click to the topmost raycast target only). This fires
        // for clicks anywhere else on the tile - selecting it for highlight
        // purposes, separate from buying or viewing details.
        SetSelected(true);
    }

    private void SetSelected(bool selected)
    {
        if (selected)
        {
            if (currentlySelected != null && currentlySelected != this)
            {
                currentlySelected.SetSelected(false);
            }
            currentlySelected = this;
        }
        else if (currentlySelected == this)
        {
            currentlySelected = null;
        }

        isSelected = selected;
        RefreshSelectionBar();
    }

    private void RefreshSelectionBar()
    {
        if (selectionBar != null)
        {
            selectionBar.SetActive(isHovering || isSelected);
        }
    }


    private GameObject FindChildByName(string childName)
    {
        Transform result = FindTransformByName(transform, childName);
        return result != null ? result.gameObject : null;
    }

    private T FindComponentInChildByName<T>(string childName) where T : Component
    {
        Transform result = FindTransformByName(transform, childName);
        return result != null ? result.GetComponent<T>() : null;
    }

    private Transform FindTransformByName(Transform root, string childName)
    {
        foreach (Transform child in root)
        {
            if (child.name == childName) return child;

            Transform nested = FindTransformByName(child, childName);
            if (nested != null) return nested;
        }
        return null;
    }

    private void LogMissingReferences()
    {
        if (icon == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'icon' (expected child named \"Icon\").", this);
        if (nameText == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'nameText' (expected child named \"ItemName\").", this);
        if (priceText == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'priceText' (expected child named \"Price\").", this);
        if (buyButton == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'buyButton' (expected child named \"BuyBTN\").", this);
        if (infoButton == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'infoButton' (expected child named \"DetailsBTN\").", this);
        if (normalViewRoot == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'normalViewRoot' (expected child named \"NormalView\").", this);
        if (descriptionViewRoot == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'descriptionViewRoot' (expected child named \"DetailsView\").", this);
        if (descriptionText == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'descriptionText' (expected a TMP text under \"DetailsView\").", this);
        if (selectionBar == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'selectionBar' (expected child named \"SelectionBar\").", this);

    }

    public void Setup(ItemSO itemToDisplay, Func<ItemSO, bool> purchaseCallback)
    {
        item = itemToDisplay;
        onPurchaseAttempt = purchaseCallback;

        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = $"${item.price}";
        if (descriptionText != null) descriptionText.text = item.description;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        if (infoButton != null)
        {
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(ToggleDescription);
        }

        showingDescription = false;
        RefreshViewState();
        RefreshOwnedState();
    }

    public void OnBuyClicked()
    {
        if (onPurchaseAttempt == null || item == null) return;

        bool success = onPurchaseAttempt.Invoke(item);

        if (success)
        {
            buyButton.image.color = Color.green;
            RefreshOwnedState();
        }
        else
        {
            bool alreadyOwned = ToolLoadout.IsOwned(item);
            bool canAfford = CurrencyManager.GetBalance() >= item.price;

            if (alreadyOwned)
            {
                Debug.Log($"[ShopSlotUI] '{item.itemName}' is already owned.");
            }
            else if (!canAfford)
            {
                Debug.Log($"[ShopSlotUI] Can't afford '{item.itemName}' - " +
                          $"balance ${CurrencyManager.GetBalance()}, price ${item.price}.");
            }

            FlashBuyButtonRed();
        }
    }

    private void FlashBuyButtonRed()
    {
        if (buyButton == null || buyButton.image == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        buyButton.image.color = failFlashColor;
        yield return new WaitForSeconds(flashDuration);
        buyButton.image.color = buyButtonDefaultColor;
        flashRoutine = null;
    }

    public void ToggleDescription()
    {
        showingDescription = !showingDescription;
        RefreshViewState();
    }

    private void RefreshViewState()
    {
        if (normalViewRoot != null)
            normalViewRoot.SetActive(!showingDescription);

        if (descriptionViewRoot != null)
            descriptionViewRoot.SetActive(showingDescription);
    }

private void RefreshOwnedState()
{
    bool owned = ToolLoadout.IsOwned(item);

    if (ownedIndicator != null) 
        ownedIndicator.SetActive(owned);

    if (buyButton != null)
    {
        buyButton.gameObject.SetActive(!owned);
    }
}
}