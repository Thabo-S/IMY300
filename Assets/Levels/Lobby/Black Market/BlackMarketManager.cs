using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackMarketManager : MonoBehaviour
{
    [Header("Item List")]
    public Transform contentParent;
    public BlackMarketEntryUI itemTemplate;
    public GameObject noArtifactsMessage;

    [Tooltip("All 6 unique artifact ItemSOs across the game, assigned once here. The Black Market shows every one of these regardless of whether it's been retrieved yet.")]
    public List<ItemSO> allArtifacts = new List<ItemSO>();

    [Header("Debt & Balance")]
    public TextMeshProUGUI debtRemainingText;
    public TextMeshProUGUI balanceText;
    public int startingDebt = 1067000;

    [Header("Pay Boss")]
    public TMP_InputField payAmountInput;
    public Button payButton;
    public TextMeshProUGUI balanceErrorText;
    public float errorDisplayDuration = 2.5f;

    [Header("Navigation")]
    public Button backToLobbyButton;

    private const string DebtPrefKey = "MobDebtRemaining";
    private Coroutine errorHideCoroutine;

    private void Awake()
    {
        if (itemTemplate != null)
            itemTemplate.gameObject.SetActive(false);

        if (payButton != null)
            payButton.onClick.AddListener(TryPayAmount);

        if (backToLobbyButton != null)
            backToLobbyButton.onClick.AddListener(CloseBlackMarket);

        if (payAmountInput != null)
            payAmountInput.contentType = TMP_InputField.ContentType.IntegerNumber;

        if (balanceErrorText != null)
            balanceErrorText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PopulateSellableItems();
        RefreshDebtAndBalanceDisplay();
    }

    private void PopulateSellableItems()
    {
        if (contentParent == null || itemTemplate == null) return;

        foreach (Transform child in contentParent)
        {
            if (child == itemTemplate.transform) continue;
            if (noArtifactsMessage != null && child == noArtifactsMessage.transform) continue;
            Destroy(child.gameObject);
        }

        int sellableCount = 0;

        foreach (ItemSO item in allArtifacts)
        {
            if (item == null) continue;

            BlackMarketEntryUI entry = Instantiate(itemTemplate, contentParent);
            entry.gameObject.SetActive(true);
            entry.Setup(item, this);

            if (ArtifactRetrievalTracker.IsRetrieved(item) && !ItemSaleTracker.IsSold(item))
                sellableCount++;
        }

        if (noArtifactsMessage != null)
            noArtifactsMessage.SetActive(sellableCount == 0);
    }

    public void SellItem(ItemSO item, BlackMarketEntryUI entryUI)
    {
        if (!ArtifactRetrievalTracker.IsRetrieved(item)) return; // shouldn't happen - button is disabled in this case
        if (ItemSaleTracker.IsSold(item)) return;                // shouldn't happen - button is disabled in this case

        int payout = item.value;

        CurrencyManager.AddCurrency(payout);
        ItemSaleTracker.MarkSold(item);

        entryUI.ShowSoldFor(payout);
        RefreshDebtAndBalanceDisplay();
    }

    private void TryPayAmount()
    {
        if (payAmountInput == null) return;

        string raw = payAmountInput.text;

        if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out int amount))
        {
            ShowError("Enter a valid number.");
            return;
        }

        if (amount <= 0)
        {
            ShowError("Amount must be greater than zero.");
            return;
        }

        int debtRemaining = GetDebtRemaining();
        int actualPayment = Mathf.Min(amount, debtRemaining);

        if (!CurrencyManager.TrySpend(actualPayment))
        {
            ShowError("Insufficient balance.");
            return;
        }

        SetDebtRemaining(debtRemaining - actualPayment);

        payAmountInput.text = "";
        RefreshDebtAndBalanceDisplay();
    }

    private void ShowError(string message)
    {
        if (balanceErrorText == null) return;

        balanceErrorText.text = message;
        balanceErrorText.gameObject.SetActive(true);

        if (errorHideCoroutine != null) StopCoroutine(errorHideCoroutine);
        errorHideCoroutine = StartCoroutine(HideErrorAfterDelay());
    }

    private System.Collections.IEnumerator HideErrorAfterDelay()
    {
        yield return new WaitForSeconds(errorDisplayDuration);
        if (balanceErrorText != null) balanceErrorText.gameObject.SetActive(false);
        errorHideCoroutine = null;
    }

    private int GetDebtRemaining()
    {
        if (!PlayerPrefs.HasKey(DebtPrefKey))
            PlayerPrefs.SetInt(DebtPrefKey, startingDebt);

        return PlayerPrefs.GetInt(DebtPrefKey, startingDebt);
    }

    private void SetDebtRemaining(int value)
    {
        PlayerPrefs.SetInt(DebtPrefKey, Mathf.Max(0, value));
        PlayerPrefs.Save();
    }

    private void RefreshDebtAndBalanceDisplay()
    {
        if (debtRemainingText != null)
            debtRemainingText.text = $"${GetDebtRemaining():N0}";

        if (balanceText != null)
            balanceText.text = $"${CurrencyManager.GetBalance():N0}";
    }

    public void CloseBlackMarket()
    {
        gameObject.SetActive(false);
    }
}