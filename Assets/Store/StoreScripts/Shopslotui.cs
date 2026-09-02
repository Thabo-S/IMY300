using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlotUI : MonoBehaviour
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


    private void Awake()
    {
        if (icon == null) icon = FindComponentInChildByName<Image>("Icon");
        if (nameText == null) nameText = FindComponentInChildByName<TextMeshProUGUI>("ItemName");
        if (priceText == null) priceText = FindComponentInChildByName<TextMeshProUGUI>("Price");
        if (buyButton == null) buyButton = FindComponentInChildByName<Button>("BuyBTN");
        if (ownedIndicator == null) ownedIndicator = FindChildByName("Owned");
        if (infoButton == null) infoButton = FindComponentInChildByName<Button>("DetailsBTN");
        if (normalViewRoot == null) normalViewRoot = FindChildByName("NormalView");
        if (descriptionViewRoot == null) descriptionViewRoot = FindChildByName("DetailsView");

        if (descriptionText == null && descriptionViewRoot != null)
        {
            // Description text lives inside DetailsView rather than being
            // named uniquely itself, so search within that subtree.
            descriptionText = descriptionViewRoot.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        LogMissingReferences();
    }

    /// <summary>Recursive, inactive-inclusive search for a child GameObject by name.</summary>
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
            RefreshOwnedState();
        }
        else
        {
            // TODO: hook up feedback here for "can't afford" vs "already owned"
            // (e.g. a shake animation or a rejection sound) once art/audio assets
            // for the shop exist.
        }
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

        if (ownedIndicator != null) ownedIndicator.SetActive(owned);
        if (buyButton != null) buyButton.interactable = !owned;
    }
}