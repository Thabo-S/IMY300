using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the Slider under RightPanel here.")]
    public Slider progressSlider;

    [Tooltip("Drag the 'itemsCollected' TMP text object here.")]
    public TextMeshProUGUI itemsCollected;

    [Tooltip("Drag the 'TotalAmount' TMP text object here.")]
    public TextMeshProUGUI totalAmount;

    [Header("Progress")]
    [Tooltip("Total collectible items in this level. Set via SetTotalItems() " +
             "from a level manager, or just set this in the Inspector if the " +
             "count is fixed per-scene.")]
    [SerializeField] private int totalItems = 8;

    [SerializeField] private int collectedItems = 0;

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

    // Call this whenever the player picks up a collectible item.
    // See Inventory.AddItem() - call progressBarController.OnItemCollected()
    // there so every pickup path (world pickup, pickpocketing, etc.) updates
    // this automatically.
    public void OnItemCollected(int amount = 1)
    {
        collectedItems = Mathf.Clamp(collectedItems + amount, 0, totalItems);
        RefreshDisplay();
    }

    // Kept for backward compatibility if anything already calls this with an
    // absolute count rather than an incremental amount.
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
            totalAmount.text = totalItems.ToString();
    }
}