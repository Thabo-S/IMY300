using UnityEngine;
using UnityEngine.UI;

public class ActionKeysToggle : MonoBehaviour
{
    public GameObject targetObject;
    private Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();

        toggle.onValueChanged.AddListener(OnToggleChanged);

        targetObject.SetActive(toggle.isOn);
    }

    private void OnToggleChanged(bool isOn)
    {
        targetObject.SetActive(isOn);
    }
}
