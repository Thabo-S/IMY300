using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadoutSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI nameText;
    [Tooltip("Shown/highlighted while this tool is selected for the mission.")]
    public GameObject selectedHighlight;
    public Button selectButton;

    private ItemSO item;

    public void Setup(ItemSO itemToDisplay)
    {
        item = itemToDisplay;

        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnClicked);
        }

        RefreshSelectedState();
    }

    private void OnClicked()
    {
        bool changed = LoadoutManager.ToggleSelection(item);

        if (!changed)
        {
            // Loadout was already full and this item wasn't in it - nothing
            // to do here beyond optionally flashing feedback later.
            return;
        }

        RefreshSelectedState();
    }

    public void RefreshSelectedState()
    {
        if (selectedHighlight != null)
            selectedHighlight.SetActive(LoadoutManager.IsSelected(item));
    }
}