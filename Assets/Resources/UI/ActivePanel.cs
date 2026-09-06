using UnityEngine;

public class activePanel : MonoBehaviour
{
    public int SelectedIndex { get; private set; } = 0;

    void Update()
    {
        if (PauseMenu.isGamePause) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetXPosition(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetXPosition(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetXPosition(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetXPosition(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetXPosition(4);
    }

    public void SetXPosition(int slotIndex)
    {
        SelectedIndex = slotIndex;

        float newXPosition = -250f + (slotIndex * 125f);

        Vector3 pos = transform.localPosition;
        pos.x = newXPosition;
        transform.localPosition = pos;
    }
}