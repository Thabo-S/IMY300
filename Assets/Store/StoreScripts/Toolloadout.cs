using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which purchasable tools (ItemSO) the player currently owns.
/// Persisted via PlayerPrefs so ownership carries between the shop scene
/// and gameplay scenes.
///
/// NOTE: Uses ItemSO.itemName as the identity key. If you rename an item's
/// itemName after players have already bought it, their ownership record
/// for that item will be lost - treat itemName as a stable ID once shipped.
/// </summary>
public static class ToolLoadout
{
    private const string OwnedToolsKey = "OwnedTools";
    private const char Separator = '|';

    public static bool IsOwned(ItemSO item)
    {
        if (item == null) return false;
        return GetOwnedNames().Contains(item.itemName);
    }

    public static void MarkOwned(ItemSO item)
    {
        if (item == null) return;

        HashSet<string> owned = GetOwnedNames();
        if (owned.Add(item.itemName))
        {
            SaveOwnedNames(owned);
        }
    }

    public static IReadOnlyCollection<string> GetOwnedItemNames()
    {
        return GetOwnedNames();
    }

    /// <summary>Editor/debug helper - do not call from shipping gameplay code.</summary>
    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(OwnedToolsKey);
        PlayerPrefs.Save();
    }

    private static HashSet<string> GetOwnedNames()
    {
        string raw = PlayerPrefs.GetString(OwnedToolsKey, string.Empty);
        if (string.IsNullOrEmpty(raw)) return new HashSet<string>();
        return new HashSet<string>(raw.Split(Separator));
    }

    private static void SaveOwnedNames(HashSet<string> owned)
    {
        PlayerPrefs.SetString(OwnedToolsKey, string.Join(Separator.ToString(), owned));
        PlayerPrefs.Save();
    }
}