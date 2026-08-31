using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        public enum ObjectiveType { CashCollected, TimeUnderSeconds, ItemsCollected, Undetected }

        [Tooltip("Just for your own reference in the Inspector, e.g. 'Earn $300'.")]
        public string label;

        public ObjectiveType type;

        [Tooltip("Cash amount / time limit in seconds / item count, depending on Type. Unused for Undetected.")]
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
        new StarObjective { label = "Complete the mission", type = StarObjective.ObjectiveType.ItemsCollected, threshold = 1 },
        new StarObjective { label = "Earn $300+",            type = StarObjective.ObjectiveType.CashCollected, threshold = 300 },
        new StarObjective { label = "Collect all loot",      type = StarObjective.ObjectiveType.ItemsCollected, threshold = 8 },
        new StarObjective { label = "Finish within 3 min",   type = StarObjective.ObjectiveType.TimeUnderSeconds, threshold = 180 },
        new StarObjective { label = "Stay undetected",       type = StarObjective.ObjectiveType.Undetected },
    };

    public void ResetStars()
    {
        StopAllCoroutines();
        foreach (Image img in starImages)
        {
            if (img != null) img.sprite = emptyStarSprite;
        }
    }

    public void EvaluateAndAwardStars(int cashCollected, float elapsedSeconds, int itemsCollected, bool wasDetected)
    {
        int starsEarned = 0;

        foreach (StarObjective objective in objectives)
        {
            bool met = false;

            switch (objective.type)
            {
                case StarObjective.ObjectiveType.CashCollected:
                    met = cashCollected >= objective.threshold;
                    break;
                case StarObjective.ObjectiveType.TimeUnderSeconds:
                    met = elapsedSeconds <= objective.threshold;
                    break;
                case StarObjective.ObjectiveType.ItemsCollected:
                    met = itemsCollected >= objective.threshold;
                    break;
                case StarObjective.ObjectiveType.Undetected:
                    met = !wasDetected;
                    break;
            }

            if (met) starsEarned++;
        }

        StartCoroutine(AnimateStars(starsEarned));
    }

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