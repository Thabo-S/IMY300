using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Attach to the keypad QTE panel (e.g. QuickTimeEventKeyPad). Generates a
// random 4-key sequence from keyPool, shows it via keySlots, and requires
// the player to press those keys in order before timeLimit runs out.
// Any wrong key, or the timer hitting 0, fails the attempt immediately.
public class KeyPadQTE : MonoBehaviour
{
    [Serializable]
    public class KeyOption
    {
        public KeyCode key;
        public Sprite sprite;
    }

    [Header("Key Pool")]
    [Tooltip("Every key this keypad can draw from, with its display sprite. " +
             "F, J, L, N, P, T, Y, Z - do NOT include C, it's reserved as the cancel key.")]
    public List<KeyOption> keyPool = new List<KeyOption>();

    [Header("Sequence Display")]
    [Tooltip("The UI Images that show the required sequence, left to right. " +
             "Size this to however many keys you want per attempt (4 by default). " +
             "Must be Image components (not SpriteRenderer) since this panel lives on a Canvas.")]
    public Image[] keySlots = new Image[4];

    [Tooltip("Tint applied to a slot once the player has correctly entered that key.")]
    public Color completedTint = Color.green;

    [Header("Timer")]
    [Tooltip("Slider used as the countdown timer - starts full, drains to empty. " +
             "The script drives its value using whatever Min/Max you've set on the Slider.")]
    public Slider timerSlider;

    [Tooltip("How many seconds the player has to enter the full sequence.")]
    public float timeLimit = 5f;

    private KeyCode[] sequence;
    private int currentIndex = 0;
    private float timeRemaining;
    private bool isActive = false;
    private Action<bool> onComplete;

    public static bool IsAnyQTEActive { get; private set; }

    public void StartQTE(Action<bool> callback)
    {
        onComplete = callback;

        sequence = new KeyCode[keySlots.Length];
        GenerateSequence();

        currentIndex = 0;
        timeRemaining = timeLimit;
        isActive = true;
        IsAnyQTEActive = true;

        UpdateTimerVisual();
    }

    private void GenerateSequence()
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            KeyOption option = keyPool[UnityEngine.Random.Range(0, keyPool.Count)];
            sequence[i] = option.key;

            if (i < keySlots.Length && keySlots[i] != null)
            {
                keySlots[i].sprite = option.sprite;
                keySlots[i].color = Color.white; // reset in case this slot showed "completed" green last time
            }
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            FinishQTE(false); // player manually cancelled
            return;
        }

        timeRemaining -= Time.deltaTime;
        UpdateTimerVisual();

        if (timeRemaining <= 0f)
        {
            FinishQTE(false); // ran out of time
            return;
        }

        // Check every key in the pool - whichever one was actually pressed
        // this frame gets validated against the current step of the sequence.
        foreach (KeyOption option in keyPool)
        {
            if (Input.GetKeyDown(option.key))
            {
                HandleKeyPress(option.key);
                break; // one key press per frame is enough
            }
        }
    }

    private void HandleKeyPress(KeyCode pressed)
    {
        if (pressed == sequence[currentIndex])
        {
            if (currentIndex < keySlots.Length && keySlots[currentIndex] != null)
                keySlots[currentIndex].color = completedTint;

            currentIndex++;

            if (currentIndex >= sequence.Length)
            {
                FinishQTE(true); // full sequence entered correctly in time
            }
        }
        else
        {
            FinishQTE(false); // wrong key - fail immediately, no partial credit
        }
    }

    private void FinishQTE(bool success)
    {
        isActive = false;
        IsAnyQTEActive = false;
        onComplete?.Invoke(success);
    }

    private void UpdateTimerVisual()
    {
        if (timerSlider != null)
            timerSlider.value = Mathf.Lerp(timerSlider.minValue, timerSlider.maxValue, Mathf.Clamp01(timeRemaining / timeLimit));
    }
}