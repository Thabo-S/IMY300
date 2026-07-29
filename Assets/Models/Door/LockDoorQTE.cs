using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LockDoorQTE : MonoBehaviour
{
    public Transform pointA; // refence to start point
    public Transform pointB; //refence to end point
    public RectTransform safezone; // Refernec to the safe zone RecTransform
    public float moveSpped = 100f;

    [Header("Progress Bar")]
    [Tooltip("The Slider used as the progress bar. The script drives its " +
             "value based on your Min/Max Value settings on the Slider itself " +
             "(e.g. 0-100, Whole Numbers on works fine) - it doesn't touch those settings.")]
    public Slider progressSlider;

    [Tooltip("How much a single successful press adds to the progress bar, 0-1. " +
             "e.g. 0.2 means 5 successful presses (with no decay) fills the bar.")]
    [Range(0f, 1f)]
    public float successAmount = 0.15f;

    [Tooltip("How much progress drains per second while the QTE is running, 0-1. " +
             "Higher = the bar shrinks faster and the player has to keep succeeding " +
             "just to hold their ground.")]
    public float decayRatePerSecond = 0.05f;

    private float progress = 0f; // 0 = empty, 1 = full -> door unlocks

    private RectTransform pointerTransform;
    private Vector3 targetPostion;

    private bool isActive = false;
    private Action<bool> onComplete;
    private bool skipInputThisFrame = false; // consumed after exactly one Update
    public static bool IsAnyQTEActive { get; private set; }

    void Awake()
    {
        pointerTransform = GetComponent<RectTransform>();
    }
    public void StartQTE(Action<bool> callback)
    {
        onComplete = callback;
        pointerTransform.position = pointA.position;
        targetPostion = pointB.position;
        isActive = true;
        IsAnyQTEActive = true;
        skipInputThisFrame = true; // ignore the E press that just opened us

        progress = 0f;
        UpdateProgressVisual();
    }

    // Update is called once per frame
    void Update()
    {
        //float direction = 1f;
        if (Time.timeScale == 0f) return;// game is paused
        if (!isActive) return; //Do nothing at all unless a door has actually started the QTE.

        if (Input.GetKeyDown(KeyCode.C))
        {
            FinishQTE(false); // player manually cancelled
            return;
        }

        //move the pointer towards the target postion
        pointerTransform.position = Vector3.MoveTowards(pointerTransform.position, targetPostion, moveSpped * Time.deltaTime);

        //change if pointer has reached one of the points
        if (Vector3.Distance(pointerTransform.position, pointA.position) < 0.1f)
        {
            targetPostion = pointB.position;
            //direction = 1f;
        }
        else if (Vector3.Distance(pointerTransform.position, pointB.position) < 0.1f)
        {
            targetPostion = pointA.position;
            // direction = -1f;
        }

        // Progress bar drains continuously while the QTE is active - the
        // player has to keep landing hits just to maintain it, let alone fill it.
        progress -= decayRatePerSecond * Time.deltaTime;
        progress = Mathf.Clamp01(progress);
        UpdateProgressVisual();

        if (skipInputThisFrame)
        {
            skipInputThisFrame = false; // consume the guard, resume normal checks next frame
            return;
        }

        //check for Input
        if (Input.GetKeyDown(KeyCode.E)) //press E 
        {
            checkSucess();
        }
    }
    void checkSucess()
    {
        bool success = RectTransformUtility.RectangleContainsScreenPoint(
            safezone, pointerTransform.position, null);

        if (success)
        {
            progress = Mathf.Clamp01(progress + successAmount);
            UpdateProgressVisual();

            if (progress >= 1f)
            {
                FinishQTE(true); // bar filled - door unlocks
            }
            // otherwise keep going - the pointer is still bouncing, player
            // needs to land another hit before decay eats the progress back
        }
        // A miss doesn't end the QTE anymore - it just costs the player the
        // decay time they wasted lining up the press. The bar keeps draining.
    }

    private void FinishQTE(bool success)
    {
        isActive = false;
        IsAnyQTEActive = false;
        onComplete?.Invoke(success); // hands the result back to LockDoor.cs
    }

    private void UpdateProgressVisual()
    {
        if (progressSlider != null)
            progressSlider.value = Mathf.Lerp(progressSlider.minValue, progressSlider.maxValue, progress);
    }

}