using System.Collections.Generic;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public string playerTag = "Player";

    public ItemSO requiredItem;

    public GameObject missionCompleteUI;
    public GameObject nextLevelButton;
    public GameObject restartButton;

    public List<GameObject> uiToHideOnComplete = new List<GameObject>();

    public ProgressBarController progressBarController;
    public ElapsedTimeDisplay elapsedTimeDisplay;
    public MissionStarsController missionStarsController;

    public InputMananger inputManager;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        inputManager = other.GetComponent<InputMananger>();

        hasTriggered = true;

        bool hasRequiredItem = requiredItem == null || PlayerHasItem(requiredItem);
        CompleteMission(hasRequiredItem);
    }

    private bool PlayerHasItem(ItemSO item)
    {
        if (Inventory.instance == null) return false;

        foreach (Slot slot in Inventory.instance.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == item) return true;
        }

        return false;
    }

    private void CompleteMission(bool hasRequiredItem)
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

            //Animator anim = missionCompleteUI.GetComponent<Animator>();
            //if (anim != null)
            //    anim.SetTrigger("Show");
        }

        if (nextLevelButton != null) nextLevelButton.SetActive(hasRequiredItem);
        if (restartButton != null) restartButton.SetActive(!hasRequiredItem);

        if (hasRequiredItem && SceneController.Instance != null)
        {
            int completedLevelIndex = SceneController.Instance.GetCurrentLevelIndex();
            SceneController.Instance.UnlockNextLevel(completedLevelIndex);
        }

        int cashCollected = progressBarController != null ? progressBarController.CashCollected : 0;
        int itemsCollected = progressBarController != null ? progressBarController.CollectedItems : 0;
        float elapsedSeconds = elapsedTimeDisplay != null ? elapsedTimeDisplay.ElapsedSeconds : Time.timeSinceLevelLoad;
        bool wasDetected = MissionStats.WasDetected;

        if (cashCollected > 0)
        {
            CurrencyManager.AddCurrency(cashCollected);
        }

        if (missionStarsController != null)
        {
            missionStarsController.EvaluateAndAwardStars(cashCollected, elapsedSeconds, itemsCollected, wasDetected);
        }

        Time.timeScale = 0f;
    }
}