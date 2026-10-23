using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Single place that stores and applies player settings (saved with PlayerPrefs).
/// Applies saved values automatically on game start and after every scene load,
/// so volume / graphics settings persist between sessions and levels.
/// </summary>
public static class GameSettings
{
    private const string MasterKey = "settings.master";
    private const string MusicKey = "settings.music";
    private const string SfxKey = "settings.sfx";
    private const string SensKey = "settings.sensitivity";
    private const string FullscreenKey = "settings.fullscreen";
    private const string QualityKey = "settings.quality";

    public const float DefaultMusic = 0.2f;     // matches MusicManager's default volume
    public const float DefaultSensitivity = 8f; // matches PlayerLookAround's default

    public static float Master
    {
        get => PlayerPrefs.GetFloat(MasterKey, 1f);
        set { PlayerPrefs.SetFloat(MasterKey, Mathf.Clamp01(value)); ApplyAudio(); }
    }

    public static float Music
    {
        get => PlayerPrefs.GetFloat(MusicKey, DefaultMusic);
        set { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); ApplyAudio(); }
    }

    public static float Sfx
    {
        get => PlayerPrefs.GetFloat(SfxKey, 1f);
        set { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); }
    }

    public static float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat(SensKey, DefaultSensitivity);
        set => PlayerPrefs.SetFloat(SensKey, value);
    }

    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        set { PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0); Screen.fullScreen = value; }
    }

    public static int Quality
    {
        get => PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
        set
        {
            value = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
            PlayerPrefs.SetInt(QualityKey, value);
            QualitySettings.SetQualityLevel(value, true);
        }
    }

    public static void Save() => PlayerPrefs.Save();

    public static void ApplyAudio()
    {
        AudioListener.volume = Master;
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(Music);
    }

    public static void ApplyAll()
    {
        ApplyAudio();
        if (PlayerPrefs.HasKey(QualityKey))
            QualitySettings.SetQualityLevel(Quality, true);
        if (PlayerPrefs.HasKey(FullscreenKey))
            Screen.fullScreen = Fullscreen;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        ApplyAll();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyAll();
}
