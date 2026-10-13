using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveEntryUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI text;

    [Tooltip("Alpha applied to icon/text once this objective is completed.")]
    public float completedAlpha = 0.5f;

    public void Setup(Objective objective)
    {
        Debug.Log($"[ObjectiveEntryUI] '{objective.description}' isComplete={objective.isComplete}");

        if (icon != null) icon.sprite = objective.icon;
        if (text != null) text.text = objective.description;

        SetCompleted(objective.isComplete);
    }

    public void SetCompleted(bool completed)
    {
        if (text != null)
        {
            text.fontStyle = completed ? FontStyles.Strikethrough : FontStyles.Normal;

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
