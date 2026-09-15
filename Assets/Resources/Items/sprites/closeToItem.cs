using TMPro;
using UnityEngine;

public class closeToItem : MonoBehaviour
{
    public GameObject player;
    public float activationDistance = 3f;

    private Canvas canvasToToggle;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        canvasToToggle = GetComponent<Canvas>();
    }

    private void Update()
    {
        CheckPlayerDistance();
    }

    private void CheckPlayerDistance()
    {
        if (player != null && canvasToToggle != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            canvasToToggle.enabled = distance <= activationDistance;
        }
    }
}