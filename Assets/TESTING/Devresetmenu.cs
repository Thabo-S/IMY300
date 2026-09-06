#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only menu items for quickly resetting saved player data during
/// testing. None of this runs in a build - purely a dev convenience.
/// </summary>
public static class DevResetMenu
{
    [MenuItem("Tools/Reset Save Data/Currency + Owned Items")]
    private static void ResetCurrencyAndOwnership()
    {
        CurrencyManager.ResetBalance(0);
        ToolLoadout.ClearAll();
        Debug.Log("[DevResetMenu] Balance reset to $0 and all owned items cleared.");
    }

    [MenuItem("Tools/Reset Save Data/Everything (all PlayerPrefs)")]
    private static void ResetEverything()
    {
        if (EditorUtility.DisplayDialog(
                "Reset ALL Save Data",
                "This clears every PlayerPrefs key in the project - currency, " +
                "owned items, AND level progress (LevelIndex). Are you sure?",
                "Yes, clear everything",
                "Cancel"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[DevResetMenu] All PlayerPrefs cleared.");
        }
    }
}
#endif