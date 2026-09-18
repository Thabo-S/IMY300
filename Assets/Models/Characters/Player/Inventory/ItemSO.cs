using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "NewItem")]

public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStackSize;
    public GameObject itemPrefab;
    public GameObject handItemPrefab;

    [Header("Black Market")]
    [Tooltip("Check for unique named artifact only (Mona Lisa, Crown of Thorns, Heart of the Amazon, Blood Crown, King's Heart, Queen's Halo)")]
    public bool isUniqueArtifact;

    [Tooltip("Shown to the player BEFORE selling a unique artifact, instead of the real value. Only relevant if isUniqueArtifact is checked.")]
    public int priceRangeMin;
    public int priceRangeMax;

    [Header("Market Value")]
    [Tooltip("Cash value of a single unit of this item, used for the Takeaway " +
             "Total on the progress UI.")]
    public int value;

    [Header("Shop")]
    [Tooltip("Whether this item appears as a purchasable tool in the shop.")]
    public bool purchasable;

    [Tooltip("Cost to buy this item in the shop.")]
    public int price;

    [Tooltip("Shown in the shop's item description text.")]
    [TextArea(2, 4)]
    public string description;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!purchasable) return;

        if (icon == null)
            Debug.LogWarning($"[ItemSO] '{name}' is purchasable but has no Icon set.", this);

        if (price <= 0)
            Debug.LogWarning($"[ItemSO] '{name}' is purchasable but Price is {price}.", this);

        if (string.IsNullOrWhiteSpace(description))
            Debug.LogWarning($"[ItemSO] '{name}' is purchasable but Description is empty.", this);

        if (string.IsNullOrWhiteSpace(itemName))
            Debug.LogWarning($"[ItemSO] '{name}' is purchasable but Item Name is empty.", this);
    }
#endif
}