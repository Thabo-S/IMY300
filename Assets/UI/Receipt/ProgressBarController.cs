using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the Slider under RightPanel here.")]
    public Slider progressSlider;

    [Tooltip("Drag the 'itemsCollected' TMP text object here. Shows e.g. '4/8'.")]
    public TextMeshProUGUI itemsCollected;

    [Tooltip("Drag the 'TotalAmount' TMP text object here. Shows the cash " +
             "value ('Takeaway Total') of everything collected so far - NOT " +
             "the item count.")]
    public TextMeshProUGUI totalAmount;

    [Header("Item Count Progress")]
    [Tooltip("Total collectible items in this level. Set via SetTotalItems() " +
             "from a level manager, or just set this in the Inspector if the " +
             "count is fixed per-scene.")]
    [SerializeField] private int totalItems = 8;

    [SerializeField] private int collectedItems = 0;

    [Header("Cash Value")]
    [Tooltip("Running cash total of every item collected so far (sum of " +
             "each ItemSO's value). Read-only at runtime, shown here for " +
             "debugging in the Inspector.")]
    [SerializeField] private int cashCollected = 0;

    // --- Public Getters for External Systems (e.g., ExitZone) ---
    public int CollectedItems => collectedItems;
    public int CashCollected => cashCollected;
    public int TotalItems => totalItems; // Optional bonus getter in case ExitZone needs the level max

    private void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = totalItems;
        }

        RefreshDisplay();
    }

    // Call this once at level start if the total isn't fixed/known until
    // runtime (e.g. counted from spawned loot objects in the scene).
    public void SetTotalItems(int total)
    {
        totalItems = total;

        if (progressSlider != null)
            progressSlider.maxValue = totalItems;

        RefreshDisplay();
    }

    // Call this whenever the player picks up a collectible item. Pass the
    // ItemSO so its cash value can be added to the running Takeaway Total.
    public void OnItemCollected(ItemSO item, int amount = 1)
    {
        collectedItems = Mathf.Clamp(collectedItems + amount, 0, totalItems);

        if (item != null)
            cashCollected += item.value * amount;

        RefreshDisplay();
    }

    // Kept for backward compatibility if anything already calls this with an
    // absolute item count and no cash value.
    public void OnProgressChanged(float numItems)
    {
        collectedItems = Mathf.Clamp((int)numItems, 0, totalItems);
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (progressSlider != null)
            progressSlider.value = collectedItems;

        if (itemsCollected != null)
            itemsCollected.text = $"{collectedItems}/{totalItems}";

        if (totalAmount != null)
            totalAmount.text = "$" + cashCollected.ToString();
    }
}