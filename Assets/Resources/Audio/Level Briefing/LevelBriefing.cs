using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelBriefing : MonoBehaviour
{
    [System.Serializable]
    public class LevelBriefingEntry
    {
        [Tooltip("Matches PlayerPrefs 'currentLevelPrefKey': 0 = Tutorial, 1-5 = Level 1-5.")]
        public int levelIndex;

        public string levelTitle;

        [Tooltip("Usually one artifact. Level 5 needs two (King's Heart & Queen's Halo) - just add both.")]
        public List<ItemSO> targetArtifacts = new List<ItemSO>();

        [TextArea(4, 10)]
        public string briefingText;

        public AudioClip briefingAudio;
    }

    [Header("Per-Level Briefing Data")]
    [Tooltip("Pre-filled with all 6 levels' titles and text - just drag in each entry's ItemSO(s) and audio clip.")]
    public List<LevelBriefingEntry> entries = new List<LevelBriefingEntry>();

    [Header("UI References")]
    public TextMeshProUGUI levelTitleUI;
    public Image artifactIconImage;
    public TextMeshProUGUI artifactNameUI;
    public TextMeshProUGUI artifactRangeUI;
    public GameObject briefingPanel;
    public TextMeshProUGUI briefingTextUI;
    public AudioSource audioSource;

    [Header("Buttons")]
    public Button startButton;
    public Button skipButton;

    [Tooltip("If true, Start Mission stays non-interactable until the audio clip finishes playing. Skip Briefing always works immediately regardless.")]
    public bool requireFullAudioForStart = true;

    private InputMananger inputManager;
    private PlayerMovement playerMovement;
    private bool isShowing = false;
    private bool audioFinished = false;

    /// <summary>
    /// Fills in the default 6-level text/title data the first time this
    /// component is added, or when you click Reset in the Inspector's
    /// context menu. Leaves targetArtifacts/briefingAudio empty for you to
    /// assign manually - only the text is auto-populated.
    /// </summary>
    private void Reset()
    {
        entries = new List<LevelBriefingEntry>
        {
            new LevelBriefingEntry
            {
                levelIndex = 0,
                levelTitle = "TUTORIAL",
                briefingText = "Let's get one thing straight. You owe me a million, and I need to know you won't get yourself locked up on your first night out. Look around, you're standing in a dry run we set up for you. There's a replica of the Hope Diamond sitting in a mock display up ahead. Learn how to keep quiet, grab the rock, and find the exit. Consider this your audition. Show me you actually have what it takes, and we'll start talking about the real jobs."
            },
            new LevelBriefingEntry
            {
                levelIndex = 1,
                levelTitle = "LEVEL 1",
                briefingText = "Alright, listen up. You owe me a little over a million, and I'm not the patient type. Word on the street is the Mona Lisa's sitting in some small museum with barely a guard in sight. Grab it, get it to the Black Market, sell it for whatever it's worth, and that comes off your tab. Small job, small security. Consider it a warm-up."
            },
            new LevelBriefingEntry
            {
                levelIndex = 2,
                levelTitle = "LEVEL 2",
                briefingText = "You're moving in the right direction. Next up's the Crown of Thorns, worth more than the last piece, and the buyers on the Black Market are already asking about it. Place has a keypad now, so it's not the walk in the park the last one was. Get it, sell it yourself, keep chipping away at what you owe me."
            },
            new LevelBriefingEntry
            {
                levelIndex = 3,
                levelTitle = "LEVEL 3",
                briefingText = "This one's a gemstone, the Heart of the Amazon. Real money if you can move it right. It's locked behind a door and a safe this time, so you'll need to hack your way in and then crack it open the old-fashioned way. Sell it same as the rest. Your debt's shrinking, but not fast enough for my liking."
            },
            new LevelBriefingEntry
            {
                levelIndex = 4,
                levelTitle = "LEVEL 4",
                briefingText = "The Blood Crown. Don't ask about the name, just take it and sell it. This place has lasers installed, probably by people who read your old reports, funny enough. Keypad, then the beams, then the safe. Whatever it goes for on the Black Market comes straight off what you owe."
            },
            new LevelBriefingEntry
            {
                levelIndex = 5,
                levelTitle = "LEVEL 5",
                briefingText = "This is it. Last job. Two pieces this time, the King's Heart and the Queen's Halo, and I need both, not one. Miss either and this trip was for nothing. You'll need a keycard off one of the guards, which means hacking your way into their break room first. That same room runs the lasers, so once you're in there, you're in control. After that, it's the vault, two locks standing between you and both prizes. Sell them right, and we're square. You walk. Don't, and we won't be having a conversation next time. Go."
            }
        };
    }

    private void Start()
    {
        FindPlayerScripts();
        ShowBriefing();
    }

    private void Update()
    {
        if (!isShowing) return;

        // Poll for the audio finishing naturally (not stopped/skipped) so we
        // can unlock Start Mission. Time.timeScale = 0 doesn't affect
        // AudioSource playback progress, so this works fine while paused.
        if (requireFullAudioForStart && !audioFinished && audioSource != null && audioSource.clip != null)
        {
            if (!audioSource.isPlaying && audioSource.time == 0f)
            {
                OnAudioFinished();
            }
        }
    }

    private void FindPlayerScripts()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("[LevelBriefing] No GameObject tagged 'Player' found in the scene.");
            return;
        }

        inputManager = player.GetComponent<InputMananger>();
        playerMovement = player.GetComponent<PlayerMovement>();

        if (inputManager == null)
            Debug.LogWarning("[LevelBriefing] Player has no InputMananger component.");

        if (playerMovement == null)
            Debug.LogWarning("[LevelBriefing] Player has no PlayerMovement component.");
    }

    /// <summary>
    /// Finds the entry matching the current 'currentLevelPrefKey' PlayerPref
    /// (0 = Tutorial, 1-5 = Level 1-5). Falls back to the first entry with
    /// a warning if no match is found, so the briefing never shows blank.
    /// </summary>
    private LevelBriefingEntry GetCurrentEntry()
    {
        int currentLevel = PlayerPrefs.GetInt("currentLevelPrefKey", 0);

        foreach (LevelBriefingEntry entry in entries)
        {
            if (entry.levelIndex == currentLevel) return entry;
        }

        Debug.LogWarning($"[LevelBriefing] No entry found for currentLevelPrefKey = {currentLevel}. Falling back to entries[0].");
        return entries.Count > 0 ? entries[0] : null;
    }

    private void ShowBriefing()
    {
        LevelBriefingEntry entry = GetCurrentEntry();
        if (entry == null)
        {
            Debug.LogError("[LevelBriefing] No briefing entries configured.");
            return;
        }

        isShowing = true;
        audioFinished = false;
        Time.timeScale = 0f;

        if (briefingPanel != null) briefingPanel.SetActive(true);
        if (briefingTextUI != null) briefingTextUI.text = entry.briefingText;

        string artifactNames = "";

        if (entry.targetArtifacts != null && entry.targetArtifacts.Count > 0)
        {
            List<string> names = new List<string>();
            List<string> ranges = new List<string>();

            foreach (ItemSO artifact in entry.targetArtifacts)
            {
                if (artifact == null) continue;
                names.Add(artifact.itemName);
                ranges.Add($"${artifact.priceRangeMin:N0} - ${artifact.priceRangeMax:N0}");
            }

            artifactNames = string.Join(" & ", names);

            if (artifactIconImage != null && entry.targetArtifacts[0] != null)
                artifactIconImage.sprite = entry.targetArtifacts[0].icon;

            if (artifactNameUI != null)
                artifactNameUI.text = artifactNames;

            if (artifactRangeUI != null)
                artifactRangeUI.text = string.Join("   /   ", ranges);
        }

        if (levelTitleUI != null)
            levelTitleUI.text = string.IsNullOrEmpty(artifactNames)
                ? entry.levelTitle
                : entry.levelTitle + " - The " + artifactNames;

        if (inputManager != null) inputManager.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;

        if (CursorManager.instance != null)
            CursorManager.instance.UnlockCursor();

        if (audioSource != null && entry.briefingAudio != null)
        {
            audioSource.clip = entry.briefingAudio;
            audioSource.Play();

        }
        else
        {
            // No audio assigned for this level - nothing to wait for, so
            // Start is immediately available regardless of requireFullAudioForStart.
            OnAudioFinished();
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(DismissBriefing);
            startButton.interactable = !requireFullAudioForStart;
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(DismissBriefing);
        }
    }

    private void OnAudioFinished()
    {
        audioFinished = true;
        if (startButton != null) startButton.interactable = true;
    }

    /// <summary>
    /// Closes the briefing, resumes time (which is what naturally starts
    /// ElapsedTimeDisplay counting again, since it reads Time.timeSinceLevelLoad),
    /// re-enables player control, and stops the audio if it's still playing.
    /// Called by both Start and Skip - Skip can fire this even mid-audio;
    /// Start is gated by requireFullAudioForStart via startButton.interactable.
    /// </summary>
    public void DismissBriefing()
    {
        if (!isShowing) return;

        isShowing = false;
        Time.timeScale = 1f;

        if (audioSource != null) audioSource.Stop();
        if (briefingPanel != null) briefingPanel.SetActive(false);

        if (inputManager != null) inputManager.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;

        if (PlayerPrefs.GetInt("currentLevelPrefKey", 0) == 0)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.hideOverlay();
            }
        }

        if (CursorManager.instance != null)
            CursorManager.instance.LockCursor();
    }
}