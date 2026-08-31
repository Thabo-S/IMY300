using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    [Header("Custom Cursor")]
    public Texture2D cursorTexture;
    public Vector2 hotSpot = Vector2.zero;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ApplyCursorTexture();
    }

    private void ApplyCursorTexture()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }

    /// Locks + hides the cursor (gameplay state) — player can look around
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (PlayerLookAround.instance != null)
        {
            PlayerLookAround.instance.updatingRotation = true;
        }
    }

    /// Unlocks + shows the cursor (menu/panel state) — camera stays frozen
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ApplyCursorTexture(); // re-apply in case OS reset it while locked

        if (PlayerLookAround.instance != null)
        {
            PlayerLookAround.instance.updatingRotation = false;
        }
    }
}