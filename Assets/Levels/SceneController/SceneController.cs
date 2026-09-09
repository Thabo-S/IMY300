using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "Main Menu";
    [SerializeField] private string storeScene = "Store";
    [SerializeField] private string loadoutScene = "Loadout";

    // Index-mapped playable levels. Index 0 = Tutorial, 1 = Level 1, etc.
    [SerializeField] private string[] levels = { "Tutorial", "Level 1", "Level 2", "Level 3" };

    private const string CURRENT_LEVEL_KEY = "currentLevelPrefKey"; // which level is queued/being played
    private const string HIGHEST_UNLOCKED_KEY = "LevelIndex";       // highest level unlocked so far

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Always resume normal time and set the correct cursor state on load.
        // This prevents a reload/restart from inheriting a paused/frozen state
        // left over from the previous scene (e.g. Mission Complete screen).
        Time.timeScale = 1f;

        if (IsGameplayScene(scene.name))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private bool IsGameplayScene(string sceneName)
    {
        foreach (string level in levels)
        {
            if (level == sceneName) return true;
        }
        return false;
    }

    // ---------- Main Menu navigation ----------

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void GoToStore()
    {
        SceneManager.LoadScene(storeScene);
    }

    /// <summary>Tutorial is playable directly from Main Menu — no Loadout step.</summary>
    public void GoToTutorial()
    {
        int tutorialIndex = System.Array.IndexOf(levels, "Tutorial");
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, tutorialIndex);
        SceneManager.LoadScene(levels[tutorialIndex]);
    }

    // ---------- Level selection flow ----------

    /// <summary>
    /// Call this when the player picks a level from the level-select overlay.
    /// Stores which level was picked, then routes to Loadout to equip before playing.
    /// </summary>
    public void SelectLevel(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex)) return;

        if (!IsLevelUnlocked(levelIndex))
        {
            Debug.LogWarning($"[SceneController] Level {levelIndex} ({levels[levelIndex]}) is locked.");
            return;
        }

        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, levelIndex);
        SceneManager.LoadScene(loadoutScene);
    }

    /// <summary>Call this from the Loadout scene's "Start" button.</summary>
    public void StartSelectedLevel()
    {
        int levelIndex = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);
        LoadLevelByIndex(levelIndex);
    }

    // ---------- In-level results (win/lose screen) ----------

    public void RestartLevel()
    {
        int levelIndex = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);
        LoadLevelByIndex(levelIndex);
    }

    /// <summary>
    /// Call this when the player completes a level successfully.
    /// Unlocks the next level (if this was the highest unlocked) and loads it.
    /// </summary>
    /// 
    public void UnlockNextLevel(int completedLevelIndex)
    {
        int nextIndex = completedLevelIndex + 1;
        if (!IsValidLevelIndex(nextIndex)) return; // already at the last level

        int highestUnlocked = PlayerPrefs.GetInt(HIGHEST_UNLOCKED_KEY, 0);
        if (nextIndex > highestUnlocked)
        {
            PlayerPrefs.SetInt(HIGHEST_UNLOCKED_KEY, nextIndex);
        }
    }
    public void NextLevel()
    {
        int currentIndex = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);

        UnlockNextLevel(currentIndex);

        int nextIndex = currentIndex + 1;
        if (!IsValidLevelIndex(nextIndex))
        {
            Debug.Log("[SceneController] No more levels — returning to Main Menu.");
            GoToMainMenu();
            return;
        }

        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, nextIndex);
        LoadLevelByIndex(nextIndex);
    }

    // ---------- Helpers ----------

    private void LoadLevelByIndex(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex)) return;
        SceneManager.LoadScene(levels[levelIndex]);
    }

    private bool IsValidLevelIndex(int index) => index >= 0 && index < levels.Length;

    public bool IsLevelUnlocked(int levelIndex)
    {
        int highestUnlocked = PlayerPrefs.GetInt(HIGHEST_UNLOCKED_KEY, 0);
        return levelIndex <= highestUnlocked;
    }

    public int GetHighestUnlockedIndex() => PlayerPrefs.GetInt(HIGHEST_UNLOCKED_KEY, 0);
    public int GetCurrentLevelIndex() => PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);
}