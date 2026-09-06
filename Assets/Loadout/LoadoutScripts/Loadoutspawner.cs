using UnityEngine;

public class LoadoutSpawner : MonoBehaviour
{
    public Inventory inventory;

    private void Start()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();
        }

        if (inventory == null)
        {
            Debug.LogError("[LoadoutSpawner] Inventory reference is MISSING in this scene!");
            return;
        }

        var selected = LoadoutManager.SelectedTools;
        Debug.Log($"[LoadoutSpawner] Start triggered. LoadoutManager contains {selected.Count} selected items.");

        if (selected.Count == 0)
        {
            Debug.LogWarning("[LoadoutSpawner] SelectedTools is empty. Did you launch directly into this scene without selecting items in the Loadout scene?");
            return;
        }

        foreach (ItemSO tool in selected)
        {
            if (tool != null)
            {
                Debug.Log($"[LoadoutSpawner] Sending '{tool.itemName}' to Inventory.AddItem()");
                inventory.AddItem(tool, 1);
            }
            else
            {
                Debug.LogWarning("[LoadoutSpawner] Encountered a null tool entry in SelectedTools.");
            }
        }
    }
}