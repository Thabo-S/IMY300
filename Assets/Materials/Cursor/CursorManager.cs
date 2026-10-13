using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    [Header("Custom Cursor")]
    public Texture2D cursorTexture;
    public Vector2 hotSpot = Vector2.zero;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Debug - New Game Reset")]
    public List<ItemSO> allArtifactsForReset = new List<ItemSO>();

    public bool resetProgressOnStart = false;
    private void Awake()
    {
        instance = this;

        // Auto-fetch AudioSource on the same GameObject if not assigned in Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (resetProgressOnStart)
        {
            ResetPlayerProgress();
        }

        ApplyCursorTexture();
    }

    private void Update()
    {
        // Plays sound only when cursor is unlocked (in menus/panels)
        if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0))
        {
            PlayClickSound();
        }
    }

    public void ResetPlayerProgress()
    {
        foreach (ItemSO artifact in allArtifactsForReset)
        {
            if (artifact == null) continue;

            PlayerPrefs.DeleteKey("ArtifactRetrieved_" + artifact.itemName);
            PlayerPrefs.DeleteKey("ItemSold_" + artifact.itemName);
        }

        // Debt owed to the boss
        PlayerPrefs.DeleteKey("MobDebtRemaining");

        // Player's cash balance
        PlayerPrefs.DeleteKey("PlayerCurrency");

        PlayerPrefs.DeleteKey("LevelIndex");
        PlayerPrefs.DeleteKey("currentLevelPrefKey");

        PlayerPrefs.Save();

        Debug.Log("[CursorManager] Player progress reset - PlayerPrefs cleared for a fresh start.");
    }


    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
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