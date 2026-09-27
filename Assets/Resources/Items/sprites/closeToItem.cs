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

        private float checkTimer;
    private const float CheckInterval = 0.2f;

    private void Update()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = CheckInterval;
            CheckPlayerDistance();
        }
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