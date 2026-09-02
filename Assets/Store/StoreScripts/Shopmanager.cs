using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Drives the pre-level shop screen: lists purchasable ItemSOs, spawns a
/// ShopSlotUI per item, and handles the actual currency deduction /
/// ownership recording when a purchase is attempted.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("Shop Contents")]
    [Tooltip("Items shown in the shop. Only entries with Purchasable = true are displayed.")]
    public List<ItemSO> shopItems = new List<ItemSO>();

    [Header("UI References")]
    public Transform slotContainer;
    public ShopSlotUI slotPrefab;
    public TextMeshProUGUI balanceText;

    private readonly List<ShopSlotUI> spawnedSlots = new List<ShopSlotUI>();

    private void OnEnable()
    {
        CurrencyManager.OnCurrencyChanged += UpdateBalanceDisplay;
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= UpdateBalanceDisplay;
    }

    private void Start()
    {
        PopulateShop();
        UpdateBalanceDisplay(CurrencyManager.GetBalance());
    }

    private void PopulateShop()
    {
        if (slotContainer == null || slotPrefab == null)
        {
            Debug.LogWarning("[ShopManager] Slot Container or Slot Prefab not assigned.");
            return;
        }

        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedSlots.Clear();

        foreach (ItemSO item in shopItems)
        {
            if (item == null || !item.purchasable) continue;

            ShopSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(item, TryPurchase);
            spawnedSlots.Add(slot);
        }
    }

    /// <summary>
    /// Attempts to purchase the given item. Returns true if the purchase
    /// succeeded (funds deducted, ownership recorded).
    /// </summary>
    private bool TryPurchase(ItemSO item)
    {
        if (item == null) return false;

        if (ToolLoadout.IsOwned(item))
        {
            return false; // already owned, nothing to buy
        }

        if (!CurrencyManager.TrySpend(item.price))
        {
            return false; // insufficient funds
        }

        ToolLoadout.MarkOwned(item);
        return true;
    }

    private void UpdateBalanceDisplay(int balance)
    {
        if (balanceText != null)
        {
            balanceText.text = $"Balance: ${balance}";
        }
    }
}