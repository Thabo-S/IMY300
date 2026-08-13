using UnityEngine;
using TMPro;

public class facePlayer : MonoBehaviour
{
    private GameObject player;
    private GameObject canvas;
    private TextMeshProUGUI nameText;

    void Start()
    {
        player = GameObject.FindWithTag("Player");

        Canvas canvasComponent = GetComponentInChildren<Canvas>(true);

        if (canvasComponent != null)
        {
            canvas = canvasComponent.gameObject;

            PositionCanvasAboveObject();

            nameText = canvas.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = gameObject.name;
            }
            else
            {
                Debug.LogWarning($"{name}: No TextMeshProUGUI found in the children of the Canvas.");
            }
        }
        else
        {
            Debug.LogWarning($"{name}: No Canvas found in the children of facePlayer script dude.");
        }
    }

    void PositionCanvasAboveObject()
    {
        // Find the combined bounds of all renderers under this parent (excluding the canvas itself)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning($"{name}: No Renderer found to calculate object bounds.");
            return;
        }

        Bounds combinedBounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            // Skip the canvas's own renderer if it has one (e.g. from TMP text)
            if (r.transform.IsChildOf(canvas.transform)) continue;
            combinedBounds.Encapsulate(r.bounds);
        }

        float topY = combinedBounds.max.y;

        Vector3 worldPos = canvas.transform.position;
        worldPos.y = topY + 0.09f;
        canvas.transform.position = worldPos;
    }

    void Update()
    {
        if (player == null) findPlayerObject();
        if (player == null || canvas == null) return;

        lookAtPlayer();

        bool shouldBeActive = isPlayerClose(player);
        if (canvas.activeSelf != shouldBeActive)
        {
            canvas.SetActive(shouldBeActive);
        }
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
        return distance <= player.GetComponent<Player>().pickUpRange;
    }
}