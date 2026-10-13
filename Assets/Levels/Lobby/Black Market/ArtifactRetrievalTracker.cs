using UnityEngine;

/// <summary>
/// Tracks whether each unique artifact has ever been retrieved (i.e. the
/// player successfully completed the level it belongs to with it in hand).
/// Persisted via PlayerPrefs, same pattern as ItemSaleTracker. Keyed by
/// ItemSO.itemName - keep those unique across 6 artifacts.
/// </summary>
public static class ArtifactRetrievalTracker
{
    private const string KeyPrefix = "ArtifactRetrieved_";

    public static bool IsRetrieved(ItemSO item)
    {
        if (item == null) return false;
        return PlayerPrefs.GetInt(KeyPrefix + item.itemName, 0) == 1;
    }

    public static void MarkRetrieved(ItemSO item)
    {
        if (item == null) return;
        PlayerPrefs.SetInt(KeyPrefix + item.itemName, 1);
        PlayerPrefs.Save();
    }
}