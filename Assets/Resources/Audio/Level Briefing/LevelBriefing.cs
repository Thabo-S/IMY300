using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelBriefing : MonoBehaviour
{
    [Header("Level Info")]
    public string levelTitle = "LEVEL 1";
    public TextMeshProUGUI levelTitleUI;

    [Header("Target Artifact (drives icon, name, price range)")]
    public ItemSO targetArtifact;
    public Image artifactIconImage;
    public TextMeshProUGUI artifactNameUI;
    public TextMeshProUGUI artifactRangeUI;

    [Header("Briefing Content (set per level)")]
    [TextArea(4, 10)]
    public string briefingText;
    public AudioClip briefingAudio;

    [Header("UI References")]
    public GameObject briefingPanel;
    public TextMeshProUGUI briefingTextUI;
    public AudioSource audioSource;

    [Header("Waveform (optional)")]
    [Tooltip("The GameObject holding the waveform bars - shown only while audio is playing.")]
    public GameObject waveformContainer;

    [Header("Buttons")]
    public Button startButton;
    public Button skipButton;

    [Tooltip("If true, Start Mission stays non-interactable until the audio clip finishes playing. Skip Briefing always works immediately regardless.")]
    public bool requireFullAudioForStart = true;

    private InputMananger inputManager;
    private PlayerMovement playerMovement;
    private bool isShowing = false;
    private bool audioFinished = false;

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
        if (requireFullAudioForStart && !audioFinished && audioSource != null && briefingAudio != null)
        {
            if (!audioSource.isPlaying && audioSource.time == 0f)
            {
                // isPlaying went false because it finished naturally, not
                // because it never started - guard with a small delay check
                // isn't needed since we only enter ShowBriefing once and
                // immediately Play(), so !isPlaying here means it's done.
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

    private void ShowBriefing()
    {
        isShowing = true;
        audioFinished = false;
        Time.timeScale = 0f;

        if (briefingPanel != null) briefingPanel.SetActive(true);
        if (briefingTextUI != null) briefingTextUI.text = briefingText;


        if (targetArtifact != null)
        {
            if (artifactIconImage != null) artifactIconImage.sprite = targetArtifact.icon;
            if (artifactNameUI != null) artifactNameUI.text = targetArtifact.itemName;
            if (artifactRangeUI != null)
                artifactRangeUI.text = $"${targetArtifact.priceRangeMin:N0} - ${targetArtifact.priceRangeMax:N0}";
        }

        if (levelTitleUI != null) levelTitleUI.text = levelTitle + " - The " + artifactNameUI.text;

        if (inputManager != null) inputManager.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;

        if (CursorManager.instance != null)
            CursorManager.instance.UnlockCursor();

        if (audioSource != null && briefingAudio != null)
        {
            audioSource.clip = briefingAudio;
            audioSource.Play();

            if (waveformContainer != null) waveformContainer.SetActive(true);
        }
        else
        {
            // No audio assigned - nothing to wait for, so Start is
            // immediately available regardless of requireFullAudioForStart.
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
        if (waveformContainer != null) waveformContainer.SetActive(false);
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
        if (waveformContainer != null) waveformContainer.SetActive(false);
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