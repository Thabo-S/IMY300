using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single row in the Objectives panel (icon + description). Matches your
/// existing "1"/"1 (1)" entries - Icon child + Text (TMP) child. Crossing an
/// objective out toggles TMP's built-in Strikethrough style and dims both
/// the text and icon, rather than hiding/removing the row.
/// </summary>
public class ObjectiveEntryUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI text;

    [Tooltip("Alpha applied to icon/text once this objective is completed.")]
    public float completedAlpha = 0.5f;

    public void Setup(Objective objective)
    {
        if (icon != null) icon.sprite = objective.icon;
        if (text != null) text.text = objective.description;

        SetCompleted(objective.isComplete);
    }

    public void SetCompleted(bool completed)
    {
        if (text != null)
        {
            text.fontStyle = completed
                ? (text.fontStyle | FontStyles.Strikethrough)
                : (text.fontStyle & ~FontStyles.Strikethrough);

            Color c = text.color;
            c.a = completed ? completedAlpha : 1f;
            text.color = c;
        }

        if (icon != null)
        {
            Color c = icon.color;
            c.a = completed ? completedAlpha : 1f;
            icon.color = c;
        }
    }
}
