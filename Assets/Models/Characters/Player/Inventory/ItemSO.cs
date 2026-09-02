using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "NewItem")]

public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStackSize;
    public GameObject itemPrefab;
    public GameObject handItemPrefab;

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


}