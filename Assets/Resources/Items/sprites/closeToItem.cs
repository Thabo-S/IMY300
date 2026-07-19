using TMPro;
using UnityEngine;

public class closeToItem : MonoBehaviour
{
    public GameObject player;
    public GameObject worldSpaceText;
    public float activationDistance = 12f;

    private void Update()
    {
        CheckPlayerDistance();
    }

    private void CheckPlayerDistance()
    {
        if (player != null && worldSpaceText != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= activationDistance)
            {
                worldSpaceText.SetActive(true);
            }
            else
            {
                worldSpaceText.SetActive(false);
            }
        }
    }
}