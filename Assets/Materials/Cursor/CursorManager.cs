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

    /// Locks + hides the cursor (gameplay state)
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// Unlocks + shows the cursor, using the custom texture (menu/UI state)
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ApplyCursorTexture(); // re-apply in case OS reset it while locked
    }
}