using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fills in the Mission Stars row (StarContainer's 5 star Images) based on
/// configurable end-of-level objectives, playing a sound as each star fills.
/// Call EvaluateAndAwardStars(...) once, from ExitZone.CompleteMission()
/// (or wherever the mission-complete screen is shown), with the final stats
/// for the run.
/// </summary>
public class MissionStarsController : MonoBehaviour
{
    [System.Serializable]
    public class StarObjective
    {
        // Append new values to preserve serialized enum values in existing scenes.
        public enum ObjectiveType { CashCollected, TimeUnderSeconds, ItemsCollected, Undetected, MissionComplete }

        [Tooltip("Just for your own reference in the Inspector, e.g. 'Earn $300'.")]
        public string label;

        public ObjectiveType type;

        [Tooltip("Cash amount / time limit in seconds / item count, depending on Type. Unused for Undetected and MissionComplete.")]
        public float threshold;
    }

    [Header("Star UI (StarContainer's children, in order)")]
    [Tooltip("Drag 'Empty star', 'Empty star (1)' ... 'Empty star (4)' here, in order.")]
    public Image[] starImages;
    public Sprite emptyStarSprite;
    public Sprite fullStarSprite;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip starFillSound;
    [Tooltip("Delay between each star filling in, for a cascading effect.")]
    public float delayBetweenStars = 0.3f;

    [Header("Objectives")]
    [Tooltip("One entry per star, evaluated in order. Add up to starImages.Length entries.")]
    public List<StarObjective> objectives = new List<StarObjective>
    {
        new StarObjective { label = "Secure the required artifact", type = StarObjective.ObjectiveType.MissionComplete },
        new StarObjective { label = "Earn $300+",            type = StarObjective.ObjectiveType.CashCollected, threshold = 300 },
        new StarObjective { label = "Collect all loot",      type = StarObjective.ObjectiveType.ItemsCollected, threshold = 8 },
        new StarObjective { label = "Finish within 3 min",   type = StarObjective.ObjectiveType.TimeUnderSeconds, threshold = 180 },
        new StarObjective { label = "Stay undetected",       type = StarObjective.ObjectiveType.Undetected },
    };

    [Header("Results UI")]
    public TextMeshProUGUI objectiveResultsText;

    public void ResetStars()
    {
        StopAllCoroutines();
        foreach (Image img in starImages)
        {
            if (img != null) img.sprite = emptyStarSprite;
        }
        if (objectiveResultsText != null) objectiveResultsText.text = string.Empty;
    }

    public void EvaluateAndAwardStars(int cashCollected, float elapsedSeconds, int itemsCollected, bool wasDetected, bool missionCompleted = false)
    {
        int starsEarned = 0;
        StringBuilder results = new StringBuilder("<b>STAR REQUIREMENTS</b>\n");
        foreach (StarObjective objective in objectives)
        {
            bool met = false;
            switch (objective.type)
            {
                case StarObjective.ObjectiveType.CashCollected: met = cashCollected >= objective.threshold; break;
                case StarObjective.ObjectiveType.TimeUnderSeconds: met = elapsedSeconds <= objective.threshold; break;
                case StarObjective.ObjectiveType.ItemsCollected: met = itemsCollected >= objective.threshold; break;
                case StarObjective.ObjectiveType.Undetected: met = !wasDetected; break;
                case StarObjective.ObjectiveType.MissionComplete: met = missionCompleted; break;
            }
            if (met) starsEarned++;
            string status = met ? "<color=#64D68A>PASS</color>" : "<color=#FF7777>MISS</color>";
            results.AppendLine($"{status} {BuildObjectiveResult(objective, cashCollected, elapsedSeconds, itemsCollected, wasDetected, missionCompleted)}");
        }
        if (objectiveResultsText != null) objectiveResultsText.text = results.ToString().TrimEnd();
        StartCoroutine(AnimateStars(starsEarned));
    }

    private string BuildObjectiveResult(StarObjective objective, int cash, float elapsed, int items, bool detected, bool missionCompleted)
    {
        switch (objective.type)
        {
            case StarObjective.ObjectiveType.CashCollected: return $"{objective.label}  (${cash:N0} / ${objective.threshold:N0})";
            case StarObjective.ObjectiveType.TimeUnderSeconds: return $"{objective.label}  ({FormatTime(elapsed)} / {FormatTime(objective.threshold)})";
            case StarObjective.ObjectiveType.ItemsCollected: return $"{objective.label}  ({items} / {objective.threshold:0})";
            case StarObjective.ObjectiveType.Undetected: return detected ? $"{objective.label}  (Detected)" : $"{objective.label}  (Undetected)";
            case StarObjective.ObjectiveType.MissionComplete: return missionCompleted ? $"{objective.label}  (Artifact secured)" : $"{objective.label}  (Artifact not secured)";
            default: return objective.label;
        }
    }

    private static string FormatTime(float seconds) => $"{Mathf.FloorToInt(seconds / 60f)}:{Mathf.FloorToInt(seconds % 60f):00}";

    private IEnumerator AnimateStars(int starsEarned)
    {
        starsEarned = Mathf.Clamp(starsEarned, 0, starImages.Length);

        for (int i = 0; i < starsEarned; i++)
        {
            if (starImages[i] != null)
                starImages[i].sprite = fullStarSprite;

            if (audioSource != null && starFillSound != null)
                audioSource.PlayOneShot(starFillSound);

            // Realtime, not WaitForSeconds - ExitZone sets Time.timeScale = 0
            // right after this starts, which would otherwise freeze the fill.
            yield return new WaitForSecondsRealtime(delayBetweenStars);
        }
    }
}