using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One entry in this level's objective list. ObjectiveType is deliberately
/// an enum (same style as MissionStarsController.StarObjective) so more
/// trigger types can be added later without restructuring - CollectItem
/// covers your keycard example now.
/// </summary>
[System.Serializable]
public class Objective
{
    public enum ObjectiveType { CollectItem }

    public string description;
    public Sprite icon;
    public ObjectiveType type;

    [Tooltip("Used when Type is CollectItem - completes this objective the " +
             "moment the player picks up this exact item.")]
    public ItemSO targetItem;

    public bool isComplete = false;
}

/// <summary>
/// Per-level objectives list + the panel that displays it. Author this
/// level's objectives in the Inspector, and call NotifyItemCollected(item)
/// from wherever items are actually picked up (Inventory.cs) to cross the
/// matching objective out live.
/// </summary>
public class ObjectiveTracker : MonoBehaviour
{
    [Tooltip("This level's objectives, in display order.")]
    public List<Objective> objectives = new List<Objective>();

    [Header("UI References")]
    [Tooltip("The 'List Of Objectives' container.")]
    public Transform listContainer;
    [Tooltip("The ObjectiveEntryUI prefab (built from your existing '1' row).")]
    public ObjectiveEntryUI entryPrefab;

    private readonly List<ObjectiveEntryUI> spawnedEntries = new List<ObjectiveEntryUI>();

    private void Start()
    {
        PopulateList();
    }

    private void PopulateList()
    {
        if (listContainer == null || entryPrefab == null)
        {
            Debug.LogWarning("[ObjectiveTracker] List Container or Entry Prefab not assigned.");
            return;
        }

        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedEntries.Clear();

        foreach (Objective objective in objectives)
        {
            ObjectiveEntryUI entry = Instantiate(entryPrefab, listContainer);
            entry.Setup(objective);
            spawnedEntries.Add(entry);
        }
    }

    /// <summary>
    /// Call this whenever the player picks up an item (see Inventory.cs's
    /// ReportItemsCollected). Crosses out any not-yet-complete CollectItem
    /// objective whose Target Item matches.
    /// </summary>
    public void NotifyItemCollected(ItemSO item)
    {
        Debug.Log($"[ObjectiveTracker] Collected '{item.name}', checking {objectives.Count} objectives.");
        if (item == null) return;

        for (int i = 0; i < objectives.Count; i++)
        {
            Objective objective = objectives[i];

            if (objective.isComplete) continue;
            if (objective.type != Objective.ObjectiveType.CollectItem) continue;
            if (objective.targetItem != item) continue;

            objective.isComplete = true;

            if (i < spawnedEntries.Count)
            {
                spawnedEntries[i].SetCompleted(true);
            }
        }
    }
}
