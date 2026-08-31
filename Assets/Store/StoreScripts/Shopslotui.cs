using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a single purchasable ItemSO and handles the buy button click,
/// delegating the actual purchase logic back to ShopManager via callback.
/// </summary>
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

    private ItemSO item;
    private Func<ItemSO, bool> onPurchaseAttempt;

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

        RefreshOwnedState();
    }

    private void OnBuyClicked()
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

    private void RefreshOwnedState()
    {
        bool owned = ToolLoadout.IsOwned(item);

        if (ownedIndicator != null) ownedIndicator.SetActive(owned);
        if (buyButton != null) buyButton.interactable = !owned;
    }
}