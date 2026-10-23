using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Game")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityValueText;

    [Header("Graphics")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Feedback sound (played while dragging a volume slider)")]
    [SerializeField] private AudioSource demoSource;
    [SerializeField] private AudioClip demoClip;
    [SerializeField] private float demoCooldown = 0.15f;

    [Header("Optional: route SFX through a mixer")]
    [Tooltip("Create an AudioMixer, add an 'SFX' group, expose its volume and name it below. " +
             "Then set the Output of your SFX AudioSources to that group.")]
    [SerializeField] private AudioMixer sfxMixer;
    [SerializeField] private string sfxMixerParameter = "SFXVolume";

    private float nextDemoTime;

    private void Awake()
    {
        if (demoSource != null)
        {
            demoSource.ignoreListenerPause = true; // still audible while the game is paused
            demoSource.playOnAwake = false;
        }
    }


    private void OnDisable()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMaster);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusic);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfx);
        if (sensitivitySlider != null) sensitivitySlider.onValueChanged.RemoveListener(OnSensitivity);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreen);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveListener(OnQuality);
        GameSettings.Save();
    }

    private void OnEnable()
    {
        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(GameSettings.Master);
            masterSlider.onValueChanged.AddListener(OnMaster);
        }
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(GameSettings.Music);
            musicSlider.onValueChanged.AddListener(OnMusic);
        }
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(GameSettings.Sfx);
            sfxSlider.onValueChanged.AddListener(OnSfx);
        }
        if (sensitivitySlider != null)
        {
            float currentSens = Mathf.Clamp(GameSettings.MouseSensitivity, sensitivitySlider.minValue, sensitivitySlider.maxValue);
            sensitivitySlider.SetValueWithoutNotify(currentSens);
            UpdateSensitivityText(currentSens); // <--- Sets initial text on open
            sensitivitySlider.onValueChanged.AddListener(OnSensitivity);
        }
        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreen);
        }
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.SetValueWithoutNotify(GameSettings.Quality);
            qualityDropdown.onValueChanged.AddListener(OnQuality);
        }
        ApplySfxMixer();
    }

    private void OnSensitivity(float v)
    {
        GameSettings.MouseSensitivity = v;
        UpdateSensitivityText(v); // <--- Updates text live while dragging
    }

    private void UpdateSensitivityText(float v)
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = v.ToString("F1"); // Displays with 1 decimal place (e.g., "8.0")
        }
    }

    private void OnMaster(float v) { GameSettings.Master = v; PlayDemo(); }
    private void OnMusic(float v) { GameSettings.Music = v; }
    private void OnSfx(float v) { GameSettings.Sfx = v; ApplySfxMixer(); PlayDemo(); }
    private void OnFullscreen(bool on) { GameSettings.Fullscreen = on; }
    private void OnQuality(int index) { GameSettings.Quality = index; }

    private void ApplySfxMixer()
    {
        if (sfxMixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(GameSettings.Sfx, 0.0001f)) * 20f;
        sfxMixer.SetFloat(sfxMixerParameter, dB);
    }

    private void PlayDemo()
    {
        if (demoSource == null || demoClip == null) return;
        if (Time.unscaledTime < nextDemoTime) return;
        nextDemoTime = Time.unscaledTime + demoCooldown;

        // Stop any currently playing demo sound and play the new one cleanly
        demoSource.Stop();
        demoSource.clip = demoClip;
        demoSource.Play();
    }
}
