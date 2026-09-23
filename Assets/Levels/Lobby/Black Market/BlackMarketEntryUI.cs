using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackMarketEntryUI : MonoBehaviour
{
    [Header("Drag from this Item's children")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI estimatedValueText; // shows the price range OR status text
    public TextMeshProUGUI soldForText;
    public GameObject soldForRow;
    public Button sellButton;

    private ItemSO item;
    private BlackMarketManager manager;

    public void Setup(ItemSO itemSO, BlackMarketManager mgr)
    {
        item = itemSO;
        manager = mgr;

        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;

        bool retrieved = ArtifactRetrievalTracker.IsRetrieved(item);
        bool sold = ItemSaleTracker.IsSold(item);

        if (!retrieved)
        {
            // Never obtained this artifact - nothing to sell yet.
            if (estimatedValueText != null)
            {
                estimatedValueText.gameObject.SetActive(true);
                estimatedValueText.text = "Not Retrieved";
                estimatedValueText.color = new Color32(179, 72, 60, 255);
            }
            if (soldForRow != null) soldForRow.SetActive(false);
            if (sellButton != null) sellButton.interactable = false;
            return;
        }

        if (sold)
        {
            // Retrieved AND already sold - locked out permanently.
            if (estimatedValueText != null) estimatedValueText.gameObject.SetActive(false);
            if (soldForRow != null) soldForRow.SetActive(true);
            if (soldForText != null) soldForText.text = "Already Sold";
            if (sellButton != null) sellButton.interactable = false;
            return;
        }

        // Retrieved, not sold - sellable.
        if (estimatedValueText != null)
        {
            estimatedValueText.gameObject.SetActive(true);
            estimatedValueText.text = $"${item.priceRangeMin:N0} - ${item.priceRangeMax:N0}";
        }
        if (soldForRow != null) soldForRow.SetActive(false);

        if (sellButton != null)
        {
            sellButton.interactable = true;
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => manager.SellItem(item, this));
        }
    }

    public void ShowSoldFor(int amount)
    {
        if (estimatedValueText != null) estimatedValueText.gameObject.SetActive(false);
        if (soldForRow != null) soldForRow.SetActive(true);
        if (soldForText != null) soldForText.text = $"Sold For: ${amount:N0}";
        if (sellButton != null) sellButton.interactable = false;
    }
}