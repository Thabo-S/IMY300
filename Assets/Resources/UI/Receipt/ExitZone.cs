using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExitZone : MonoBehaviour
{
    public string playerTag = "Player";

    public List<ItemSO> requiredItems = new List<ItemSO>();

    public GameObject missionCompleteUI;
    public GameObject nextLevelButton;
    public GameObject restartButton;

    public List<GameObject> uiToHideOnComplete = new List<GameObject>();

    public ProgressBarController progressBarController;
    public ElapsedTimeDisplay elapsedTimeDisplay;
    public MissionStarsController missionStarsController;

    public InputMananger inputManager;

    [Header("Mission Complete")]
    public Image artifactIconImage;
    public TextMeshProUGUI artifactNameText;
    public TextMeshProUGUI artifactStatusText;   // "SECURED" / "NOT RECOVERED"
    public TextMeshProUGUI artifactRangeText;    // "$50,000 - $80,000"
    public TextMeshProUGUI sideLootCashText;     // known, already-paid-out total

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        inputManager = other.GetComponent<InputMananger>();

        hasTriggered = true;

        bool hasAllRequiredItems = HasAllRequiredItems();
        CompleteMission(hasAllRequiredItems);
    }

    private bool HasAllRequiredItems()
    {
        if (requiredItems.Count == 0) return true;
        if (Inventory.instance == null) return false;

        foreach (ItemSO required in requiredItems)
        {
            bool found = false;

            foreach (Slot slot in Inventory.instance.allSlots)
            {
                if (slot.HasItem() && slot.GetItem() == required)
                {
                    found = true;
                    break;
                }
            }

            if (!found) return false;
        }

        return true;
    }

    /// <summary>
    /// Sells every non-unique item currently in the inventory immediately
    /// (their price is always known, so there's no reason to hold them back
    /// for the Black Market) and clears those slots. Unique artifacts are
    /// deliberately left untouched - they stay in the inventory, carry over
    /// to the Black Market scene, and only pay out + get marked sold there.
    /// </summary>
    private int SellSideLootImmediately()
    {
        if (Inventory.instance == null) return 0;

        int total = 0;

        foreach (Slot slot in Inventory.instance.allSlots)
        {
            if (!slot.HasItem()) continue;

            ItemSO item = slot.GetItem();
            if (item.isUniqueArtifact) continue; // leave artifacts for the Black Market

            total += item.value * slot.GetAmount();
            slot.ClearSlot();
        }

        return total;
    }

    private void CompleteMission(bool hasRequiredItems)
    {
        if (inputManager != null) inputManager.enabled = false;

        if (CursorManager.instance != null)
            CursorManager.instance.UnlockCursor();

        foreach (GameObject ui in uiToHideOnComplete)
        {
            if (ui != null) ui.SetActive(false);
        }

        if (missionCompleteUI != null)
        {
            missionCompleteUI.SetActive(true);
        }

        if (nextLevelButton != null) nextLevelButton.SetActive(hasRequiredItems);
        if (restartButton != null) restartButton.SetActive(!hasRequiredItems);

        if (hasRequiredItems)
        {
            // Mark every required artifact as retrieved for THIS playthrough -
            // this is what BlackMarketEntryUI checks before allowing a sale.
            foreach (ItemSO required in requiredItems)
            {
                ArtifactRetrievalTracker.MarkRetrieved(required);
            }

            if (SceneController.Instance != null)
            {
                int completedLevelIndex = SceneController.Instance.GetCurrentLevelIndex();
                SceneController.Instance.UnlockNextLevel(completedLevelIndex);
            }
        }

        // Side loot pays out now, known values, no reveal needed.
        int sideLootCash = SellSideLootImmediately();
        if (sideLootCash > 0)
        {
            CurrencyManager.AddCurrency(sideLootCash);
        }

        int itemsCollected = progressBarController != null ? progressBarController.CollectedItems : 0;
        float elapsedSeconds = elapsedTimeDisplay != null ? elapsedTimeDisplay.ElapsedSeconds : Time.timeSinceLevelLoad;
        bool wasDetected = MissionStats.WasDetected;

        if (missionStarsController != null)
        {
            missionStarsController.EvaluateAndAwardStars(sideLootCash, elapsedSeconds, itemsCollected, wasDetected);
        }

        UpdateMissionCompleteText(hasRequiredItems, sideLootCash);

        Time.timeScale = 0f;
    }

    private void UpdateMissionCompleteText(bool hasRequiredItems, int sideLootCash)
    {
        if (requiredItems.Count > 0)
        {
            // Assumes a single primary artifact drives the name/status/range
            // display. If Level 5 needs both shown, extend this to loop over
            // requiredItems instead of just index 0.
            ItemSO primary = requiredItems[0];

            if (artifactNameText != null)
                artifactNameText.text = primary.itemName;

            if (artifactStatusText != null)
            {
                artifactStatusText.text = hasRequiredItems ? "SECURED"   : "NOT RECOVERED";

                if (hasRequiredItems) 
                {
                    artifactStatusText.color = Color.green;
                }
                else
                {
                    artifactStatusText.color = Color.red;
                }
            }



            if (artifactRangeText != null)
                artifactRangeText.text = $"${primary.priceRangeMin:N0} - ${primary.priceRangeMax:N0}";

            if (artifactIconImage != null)
                artifactIconImage.sprite = primary.icon;
        }

        if (sideLootCashText != null)
            sideLootCashText.text = $"${sideLootCash:N0}";


    }
}