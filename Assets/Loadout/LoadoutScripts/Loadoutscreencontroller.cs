using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadoutScreenController : MonoBehaviour
{
    [Header("Item Catalog")]
    [Tooltip("Every tool ItemSO that could possibly be owned - same master " +
             "list style as ShopManager's Shop Items. Only ones the player " +
             "actually owns (per ToolLoadout) are shown here.")]
    public List<ItemSO> allTools = new List<ItemSO>();

    [Header("UI References")]
    public Transform slotContainer;
    public LoadoutSlotUI slotPrefab;
    public TextMeshProUGUI loadoutCountText; 

    [Header("Mission Start")]
    [Tooltip("Scene to load once the player confirms their loadout.")]
    //public string missionSceneName;

    private string[] Levels = { "Tutorial", "Level 1", "Level 2" ,"Level 3"};

    private int LevelIndex;


    private readonly List<LoadoutSlotUI> spawnedSlots = new List<LoadoutSlotUI>();

    private void Start()
    {
        LevelIndex = PlayerPrefs.GetInt("LevelIndex");

        Debug.Log($"--- SAVED IN PLAYERPREFS ---");
        foreach (var name in ToolLoadout.GetOwnedItemNames())
        {
            Debug.Log($"Owned String: '{name}'");
        }

        Debug.Log($"--- CHECKING CATALOG ITEMS ---");
        foreach (ItemSO item in allTools)
        {
            if (item == null)
            {
                Debug.LogWarning("Found NULL item in allTools!");
                continue;
            }

            bool owned = ToolLoadout.IsOwned(item);
            Debug.Log($"SO Name: '{item.name}' | SO itemName field: '{item.itemName}' | IsOwned result: {owned}");
        }

        LoadoutManager.ClearSelection();
        PopulateLoadoutScreen();
        UpdateLoadoutCountText();
    }

    private void PopulateLoadoutScreen()
    {
        if (slotContainer == null || slotPrefab == null)
        {
            Debug.LogWarning("[LoadoutScreenController] Slot Container or Slot Prefab not assigned.");
            return;
        }

        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedSlots.Clear();

        foreach (ItemSO item in allTools)
        {
            if (item == null || !ToolLoadout.IsOwned(item)) continue;

            LoadoutSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(item);
            spawnedSlots.Add(slot);
        }
    }

    public void RefreshAllSlots()
    {
        foreach (LoadoutSlotUI slot in spawnedSlots)
        {
            slot.RefreshSelectedState();
        }

        UpdateLoadoutCountText();
    }

    private void Update()
    {

        UpdateLoadoutCountText();
    }

    private void UpdateLoadoutCountText()
    {
        if (loadoutCountText != null)
        {
            loadoutCountText.text = $"{LoadoutManager.SelectedTools.Count}/{LoadoutManager.MaxLoadoutSize} selected";
        }
    }

    public void OnStartMissionClicked()
    {
        if (string.IsNullOrEmpty(Levels[LevelIndex]))
        {
            Debug.LogWarning("[LoadoutScreenController] Mission Scene Name is not set.");
            return;
        }

        SceneManager.LoadScene(Levels[LevelIndex]);
    }

    
}