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

}