using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture; // Assign your cursor texture here in the Inspector
    public Vector2 hotSpot = Vector2.zero; // The click point (0,0 is top-left)

    void Start()
    {
        // Change the cursor
        // CursorMode.Auto allows Unity to decide the best platform-specific method
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}