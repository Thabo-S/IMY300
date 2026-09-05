using UnityEngine;

public class LoadoutSpawner : MonoBehaviour
{
    public Inventory inventory;

    private void Start()
    {
        if (inventory == null)
        {
            Debug.LogWarning("[LoadoutSpawner] Inventory reference not assigned.");
            return;
        }

        foreach (ItemSO tool in LoadoutManager.SelectedTools)
        {
            inventory.AddItem(tool);
        }
    }
}