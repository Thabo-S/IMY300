//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//public class ExitZone : MonoBehaviour
//{
//    [Header("Trigger")]
//    public string playerTag = "Player";

//    [Header("Escape Requirement (optional)")]
//    [Tooltip("Leave empty if simply reaching this zone is enough to win. " +
//             "Assign an ItemSO here if the player must be carrying a specific " +
//             "item (e.g. a stolen artifact) to complete the mission.")]
//    public ItemSO requiredItem;

//    [Header("Mission Complete UI")]
//    [Tooltip("Root GameObject of the Mission Complete screen. Will be " +
//             "SetActive(true) and have its Animator 'Show' trigger fired.")]
//    public GameObject missionCompleteUI;

//    [Tooltip("Optional: shown instead if requiredItem is set and the player " +
//             "doesn't have it. Leave empty to just do nothing (player can " +
//             "keep playing and try again).")]
//    public GameObject missingItemUI;

//    [Tooltip("Other UI elements (HealthBar, crosshair, hotbar, Action Keys, " +
//             "etc.) to hide once the mission completes, so only the results " +
//             "screen is visible. List them explicitly rather than hiding " +
//             "everything under the Canvas automatically.")]
//    public List<GameObject> uiToHideOnComplete = new List<GameObject>();

//    [Header("Mission Results")]
//    [Tooltip("Drag the ProgressBarController here (tracks cash + items collected).")]
//    public ProgressBarController progressBarController;

//    [Tooltip("Drag the ElapsedTimeDisplay here (tracks run time).")]
//    public ElapsedTimeDisplay elapsedTimeDisplay;

//    [Tooltip("Drag the MissionStarsController here (fills stars + plays sound).")]
//    public MissionStarsController missionStarsController;

//    [Header("Player Freeze")]
//    [Tooltip("Disabling these stops player input/look while the Mission " +
//             "Complete screen is up. Assign in the Inspector.")]
//    public MonoBehaviour playerMovement; // drag the PlayerMovement component
//    public PlayerCam playerCam;          // drag the PlayerCam component

//    private bool hasTriggered = false;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (hasTriggered) return;
//        if (!other.CompareTag(playerTag)) return;

//        if (requiredItem != null && !PlayerHasItem(other, requiredItem))
//        {
//            if (missingItemUI != null)
//                missingItemUI.SetActive(true);
//            return; // don't set hasTriggered - player can try again with the item
//        }

//        hasTriggered = true;
//        CompleteMission();
//    }

//    private bool PlayerHasItem(Collider playerCollider, ItemSO item)
//    {
//        Inventory inventory = playerCollider.GetComponentInParent<Inventory>();
//        if (inventory == null) return false;

//        foreach (Slot slot in inventory.hotbarSlots)
//        {
//            if (slot.HasItem() && slot.GetItem() == item) return true;
//        }

//        return false;
//    }

//    private void CompleteMission()
//    {
//        // Freeze the player where they stand.
//        if (playerMovement != null) playerMovement.enabled = false;
//        if (playerCam != null) playerCam.updatingRotation = false;

//        // Release the cursor so the player can interact with any UI buttons
//        // (Restart / Main Menu) on the Mission Complete screen.
//        Cursor.lockState = CursorLockMode.None;
//        Cursor.visible = true;

//        // Hide everything else so only the results screen is visible.
//        foreach (GameObject ui in uiToHideOnComplete)
//        {
//            if (ui != null) ui.SetActive(false);
//        }

//        if (missionCompleteUI != null)
//        {
//            missionCompleteUI.SetActive(true);

//            Animator anim = missionCompleteUI.GetComponent<Animator>();
//            if (anim != null)
//                anim.SetTrigger("Show");
//        }

//        // Pull the final run stats and award mission stars.
//        if (missionStarsController != null)
//        {
//            int cashCollected = progressBarController != null ? progressBarController.CashCollected : 0;
//            int itemsCollected = progressBarController != null ? progressBarController.CollectedItems : 0;
//            float elapsedSeconds = elapsedTimeDisplay != null ? elapsedTimeDisplay.ElapsedSeconds : Time.timeSinceLevelLoad;
//            bool wasDetected = MissionStats.WasDetected;

//            missionStarsController.EvaluateAndAwardStars(cashCollected, elapsedSeconds, itemsCollected, wasDetected);
//        }

//        // Optional: stop guards from continuing to chase/patrol once the
//        // mission is over. Comment out if you'd rather leave them running.
//        Time.timeScale = 0f;
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExitZone : MonoBehaviour
{
    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Escape Requirement (optional)")]
    [Tooltip("Leave empty if simply reaching this zone is enough to win. " +
             "Assign an ItemSO here if the player must be carrying a specific " +
             "item (e.g. a stolen artifact) to complete the mission.")]
    public ItemSO requiredItem;

    [Header("Mission Complete UI")]
    [Tooltip("Root GameObject of the Mission Complete screen. Will be " +
             "SetActive(true) and have its Animator 'Show' trigger fired.")]
    public GameObject missionCompleteUI;

    [Tooltip("Optional: shown instead if requiredItem is set and the player " +
             "doesn't have it. Leave empty to just do nothing (player can " +
             "keep playing and try again).")]
    public GameObject missingItemUI;

    [Tooltip("Other UI elements (HealthBar, crosshair, hotbar, Action Keys, " +
             "etc.) to hide once the mission completes, so only the results " +
             "screen is visible. List them explicitly rather than hiding " +
             "everything under the Canvas automatically.")]
    public List<GameObject> uiToHideOnComplete = new List<GameObject>();

    [Header("Mission Results")]
    [Tooltip("Drag the ProgressBarController here (tracks cash + items collected). " +
             "MUST be wired per-scene - if left None, cash earned this run " +
             "will be silently treated as $0.")]
    public ProgressBarController progressBarController;

    [Tooltip("Drag the ElapsedTimeDisplay here (tracks run time).")]
    public ElapsedTimeDisplay elapsedTimeDisplay;

    [Tooltip("Drag the MissionStarsController here (fills stars + plays sound).")]
    public MissionStarsController missionStarsController;

    [Header("Player Freeze")]
    [Tooltip("Disabling these stops player input/look while the Mission " +
             "Complete screen is up. Assign in the Inspector.")]
    public MonoBehaviour playerMovement; // drag the PlayerMovement component
    public PlayerCam playerCam;          // drag the PlayerCam component

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        if (requiredItem != null && !PlayerHasItem(other, requiredItem))
        {
            if (missingItemUI != null)
                missingItemUI.SetActive(true);
            return; // don't set hasTriggered - player can try again with the item
        }

        hasTriggered = true;
        CompleteMission();
    }

    private bool PlayerHasItem(Collider playerCollider, ItemSO item)
    {
        Inventory inventory = playerCollider.GetComponentInParent<Inventory>();
        if (inventory == null) return false;

        foreach (Slot slot in inventory.hotbarSlots)
        {
            if (slot.HasItem() && slot.GetItem() == item) return true;
        }

        return false;
    }

    private void CompleteMission()
    {
        // Freeze the player where they stand.
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCam != null) playerCam.updatingRotation = false;

        // Release the cursor so the player can interact with any UI buttons
        // (Restart / Main Menu) on the Mission Complete screen.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide everything else so only the results screen is visible.
        foreach (GameObject ui in uiToHideOnComplete)
        {
            if (ui != null) ui.SetActive(false);
        }

        if (missionCompleteUI != null)
        {
            missionCompleteUI.SetActive(true);

            Animator anim = missionCompleteUI.GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("Show");
        }

        if (progressBarController == null)
        {
            Debug.LogWarning("[ExitZone] Progress Bar Controller is not assigned - " +
                              "cash earned this run will NOT be paid out to CurrencyManager.", this);
        }

        int cashCollected = progressBarController != null ? progressBarController.CashCollected : 0;
        int itemsCollected = progressBarController != null ? progressBarController.CollectedItems : 0;
        float elapsedSeconds = elapsedTimeDisplay != null ? elapsedTimeDisplay.ElapsedSeconds : Time.timeSinceLevelLoad;
        bool wasDetected = MissionStats.WasDetected;

        // Pay out this run's earned cash into the persistent CurrencyManager
        // balance, so it's spendable in the Store once the player leaves
        // this scene. THIS is the piece that was missing.
        if (cashCollected > 0)
        {
            CurrencyManager.AddCurrency(cashCollected);
        }

        // Award mission stars using the same stats.
        if (missionStarsController != null)
        {
            missionStarsController.EvaluateAndAwardStars(cashCollected, elapsedSeconds, itemsCollected, wasDetected);
        }

        // Optional: stop guards from continuing to chase/patrol once the
        // mission is over. Comment out if you'd rather leave them running.
        Time.timeScale = 0f;
    }
}