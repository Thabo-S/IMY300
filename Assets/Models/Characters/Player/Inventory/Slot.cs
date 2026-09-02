//using UnityEngine;
//using TMPro;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    public bool hovering;

//    public ItemSO heldItem;
//    private int itemAmount;

//    private Image iconImage;
//    private TextMeshProUGUI amountTxt;

//    public void Awake()
//    {
//        iconImage = transform.GetChild(0).GetComponent<Image>();
//        amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
//    }

//    public ItemSO GetItem()
//    {
//        return heldItem;
//    }

//    public int GetAmount()
//    {
//        return itemAmount;
//    }

//    public void SetItem(ItemSO item, int amount = 1)
//    {
//        heldItem = item;
//        itemAmount = amount;

//        UpdateSlot();
//    }

//    public void UpdateSlot()
//    {

//        if(iconImage == null)
//        {
//            iconImage = transform.GetChild(0).GetComponent<Image>();
//            amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
//        }

//        if (heldItem != null)
//        {
//            iconImage.enabled = true;
//            iconImage.sprite = heldItem.icon;
//            amountTxt.text = itemAmount.ToString();
//        }
//        else
//        {
//            iconImage.enabled = false;
//            amountTxt.text = "";
//        }
//    }

//    public int AddAmount(int amountToAdd)
//    {
//        itemAmount += amountToAdd;
//        UpdateSlot();
//        return itemAmount;
//    }
//    public int RemoveAmount(int amountToRemove)
//    {
//        itemAmount = itemAmount - amountToRemove;
//        if (amountToRemove <= 0)
//        {
//            ClearSlot();
//        }
//        else
//        {
//            UpdateSlot();
//        }
//        return itemAmount;
//    }

//    public void ClearSlot()
//    {
//        heldItem = null;
//        itemAmount = 0;
//        UpdateSlot();
//    }

//    public bool HasItem()
//    {
//        return heldItem != null;
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        hovering = true;
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        hovering = false;
//    }
//}
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering;

    public ItemSO heldItem;
    private int itemAmount;

    [Header("UI References")]
    [Tooltip("Must be a CHILD of this Slot (not the Slot's own background " +
             "Image, and not another Slot's Image). Assign directly in the " +
             "Inspector.")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountTxt;

    public void Awake()
    {
        // Validate manually-assigned references actually belong to THIS
        // slot's own children. With many slots to wire by hand, it's easy
        // to accidentally drag in a sibling slot's Image/Text instead of
        // this slot's own - which causes a slot's data to update while a
        // completely different slot's icon changes on screen, or "nothing"
        // visibly changes because the wrong Image got toggled instead.
        if (iconImage != null && !IsValidChild(iconImage.transform))
        {
            Debug.LogError($"[Slot] '{gameObject.name}': Icon Image is assigned to " +
                            $"'{iconImage.gameObject.name}' (path: {GetPath(iconImage.transform)}), " +
                            $"which is NOT a proper child of this Slot. Clearing and " +
                            $"re-fetching automatically - please fix this in the Inspector.", this);
            iconImage = null;
        }

        if (amountTxt != null && !IsValidChild(amountTxt.transform))
        {
            Debug.LogError($"[Slot] '{gameObject.name}': Amount Txt is assigned to " +
                            $"'{amountTxt.gameObject.name}' (path: {GetPath(amountTxt.transform)}), " +
                            $"which is NOT a proper child of this Slot. Clearing and " +
                            $"re-fetching automatically - please fix this in the Inspector.", this);
            amountTxt = null;
        }

        // Fallback search - explicitly skips this GameObject's own components
        // (e.g. the Slot's background tile Image) and only matches a genuine
        // descendant, so it can't silently grab the wrong Image.
        if (iconImage == null)
        {
            foreach (Image img in GetComponentsInChildren<Image>(true))
            {
                if (img.transform != transform)
                {
                    iconImage = img;
                    break;
                }
            }
        }

        if (amountTxt == null)
        {
            foreach (TextMeshProUGUI txt in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (txt.transform != transform)
                {
                    amountTxt = txt;
                    break;
                }
            }
        }

        if (iconImage == null)
            Debug.LogError($"[Slot] '{gameObject.name}': No valid child Image found - assign Icon Image in the Inspector.", this);

        if (amountTxt == null)
            Debug.LogError($"[Slot] '{gameObject.name}': No valid child TextMeshProUGUI found - assign Amount Text in the Inspector.", this);
    }

    private bool IsValidChild(Transform t)
    {
        return t != transform && t.IsChildOf(transform);
    }

    private string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    public ItemSO GetItem()
    {
        return heldItem;
    }

    public int GetAmount()
    {
        return itemAmount;
    }

    public void SetItem(ItemSO item, int amount = 1)
    {
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (iconImage == null || amountTxt == null)
        {
            // Already logged in Awake() - bail out instead of throwing.
            return;
        }

        if (heldItem != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;
            amountTxt.text = itemAmount.ToString();
        }
        else
        {
            iconImage.enabled = false;
            amountTxt.text = "";
        }
    }

    public int AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
        UpdateSlot();
        return itemAmount;
    }
    public int RemoveAmount(int amountToRemove)
    {
        itemAmount = itemAmount - amountToRemove;
        if (itemAmount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }
        return itemAmount;
    }

    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;
        UpdateSlot();
    }

    public bool HasItem()
    {
        return heldItem != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
