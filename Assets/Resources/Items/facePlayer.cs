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
            Debug.LogWarning($"{name}: No Canvas found in the children of facePlayer script dude.");
        }
    }

    void Update()
    {
        if (player == null)
        {
            findPlayerObject();
        }

        if (player == null || canvas == null) return;

        lookAtPlayer();

        canvas.SetActive(isPlayerClose(player));

    }

    void findPlayerObject()
    {
        player = GameObject.FindWithTag("Player");
    }

    void lookAtPlayer()
    {
        Vector3 directionToPlayer = canvas.transform.position - player.transform.position;

        canvas.transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private bool isPlayerClose(GameObject player)
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        //Debug.Log("Distance from player: " +  distance);

        return distance <= player.GetComponent<Player>().pickUpRange;
    }
}