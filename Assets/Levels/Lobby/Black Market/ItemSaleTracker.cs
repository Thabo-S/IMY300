using UnityEngine;

/// <summary>
/// Tracks whether a unique artifact has already been sold, persisted via
/// PlayerPrefs so it survives between play sessions. Keyed by ItemSO.itemName
/// - make sure each unique artifact's Item Name is set and unique in the
/// Inspector, or two different items with the same name will collide.
/// </summary>
public static class ItemSaleTracker
{
    private const string KeyPrefix = "ItemSold_";

    public static bool IsSold(ItemSO item)
    {
        if (item == null) return false;
        return PlayerPrefs.GetInt(KeyPrefix + item.itemName, 0) == 1;
    }

    public static void MarkSold(ItemSO item)
    {
        if (item == null) return;
        PlayerPrefs.SetInt(KeyPrefix + item.itemName, 1);
        PlayerPrefs.Save();
    }
}