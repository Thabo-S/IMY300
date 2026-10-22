using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsTabController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;
        public GameObject panel;
    }

    [Header("Tabs")]
    [SerializeField] private List<Tab> tabs = new List<Tab>();

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color32(0x94, 0x6A, 0x00, 0xFF);   // 946A00
    [SerializeField] private Color selectedColor = new Color32(0xB8, 0xE6, 0xFF, 0xFF); // B8E6FF

    [Header("Default")]
    [SerializeField] private int defaultTabIndex = 0;

    private void Awake()
    {
        // Hook up click listeners once in Awake to avoid closure duplication issues
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i; // capture for closure
            if (tabs[i].button != null)
            {
                tabs[i].button.onClick.AddListener(() => SelectTab(index));
            }
        }
    }

    private void OnEnable()
    {
        // Select the default tab every time the settings menu is opened
        SelectTab(defaultTabIndex);
    }

    public void SelectTab(int selectedIndex)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            bool isSelected = (i == selectedIndex);

            // Toggle panel visibility
            if (tabs[i].panel != null)
            {
                tabs[i].panel.SetActive(isSelected);
            }

            // Toggle button color
            if (tabs[i].button != null)
            {
                var image = tabs[i].button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = isSelected ? selectedColor : normalColor;
                }
            }
        }
    }
}