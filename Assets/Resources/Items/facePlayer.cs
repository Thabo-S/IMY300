using UnityEngine;

public class facePlayer : MonoBehaviour
{
    private GameObject player;
    private GameObject canvas;

    void Start()
    {
        player = GameObject.FindWithTag("Player");

        Canvas canvasComponent = GetComponentInChildren<Canvas>();
        if (canvasComponent != null)
        {
            canvas = canvasComponent.gameObject;
        }
        else
        {
            Debug.LogWarning($"{name}: No Canvas found in children of facePlayer.");
        }
    }

    void Update()
    {
        if (player == null || canvas == null) return;

        lookAtPlayer();
        canvas.SetActive(isPlayerClose(player));
    }

    void lookAtPlayer()
    {
        Vector3 directionToPlayer = canvas.transform.position - player.transform.position;
        canvas.transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private bool isPlayerClose(GameObject player)
    {
        if (player == null) return false;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        return distance <= player.GetComponent<Player>().pickUpRange;
    }
}