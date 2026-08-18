using UnityEngine;
using TMPro;

/// <summary>
/// Ticks up the 'Elapsed Time' text every frame using Time.timeSinceLevelLoad,
/// and exposes the current value for MissionStarsController's time objective.
/// Attach to the LevelCompleteUI panel (or anywhere active for the whole level).
/// </summary>
public class ElapsedTimeDisplay : MonoBehaviour
{
    [Tooltip("Drag the 'Elapsed Time:' value TMP text here.")]
    public TextMeshProUGUI elapsedTimeText;

    public float ElapsedSeconds => Time.timeSinceLevelLoad;

    private void Update()
    {
        if (elapsedTimeText == null) return;

        float t = ElapsedSeconds;
        int hours = Mathf.FloorToInt(t / 3600f);
        int minutes = Mathf.FloorToInt((t % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);

        elapsedTimeText.text = $"{hours:00}:{minutes:00}:{seconds:00}s";
    }
}