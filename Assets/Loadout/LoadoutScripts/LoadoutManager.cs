using System.Collections.Generic;

public static class LoadoutManager
{
    public const int MaxLoadoutSize = 3;

    private static readonly List<ItemSO> selectedTools = new List<ItemSO>();

    public static IReadOnlyList<ItemSO> SelectedTools => selectedTools;

    public static bool IsSelected(ItemSO item)
    {
        return item != null && selectedTools.Contains(item);
    }

    public static bool ToggleSelection(ItemSO item)
    {
        if (item == null) return false;

        if (selectedTools.Contains(item))
        {
            selectedTools.Remove(item);
            return true;
        }

        if (selectedTools.Count >= MaxLoadoutSize)
        {
            return false; // loadout full
        }

        selectedTools.Add(item);
        return true;
    }

    public static void ClearSelection()
    {
        selectedTools.Clear();
    }
}
