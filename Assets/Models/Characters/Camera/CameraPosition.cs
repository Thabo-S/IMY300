using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    private Vector3 thirdPersonPos = new Vector3(4.11f, 12.59f, -16.21f);
    private Vector3 thirdPersonRot = new Vector3(15f, 0f, 0f);

    private Vector3 topDownPos = new Vector3(0f, 11.57f, -0.16f);
    private Vector3 topDownRot = new Vector3(0f, 0f, 0f);

    private Vector3 crouchTopDownPos = new Vector3(0f, 8.22f, 1f);
    private Vector3 crouchTopDownRot = new Vector3(20f, 0f, 0f);

    private bool isTopDown = true;
    private bool isCrouching = false;
    private Transform player;
    public float headPosition = 11.5f;

    private SkinnedMeshRenderer playerMeshRenderer;

    [SerializeField] private float smoothSpeed = 10f;

    void Start()
    {
        player = transform.parent;

        // Find the player mesh by tag
        GameObject meshObj = GameObject.FindGameObjectWithTag("PlayerMesh");
        if (meshObj != null)
        {
            playerMeshRenderer = meshObj.GetComponent<SkinnedMeshRenderer>();
        }

        // Set initial state
        UpdateCameraTransform();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    isTopDown = !isTopDown;
        //    UpdateCameraTransform();
        //}

        PreventCameraClipping();
    }

    public void ToggleCrouchView(bool crouchingState)
    {
        isCrouching = crouchingState;
        UpdateCameraTransform();
    }

    void UpdateCameraTransform()
    {

        if (playerMeshRenderer != null)
        {
            playerMeshRenderer.enabled = !isTopDown;
        }

        if (isTopDown)
        {
            if (isCrouching)
            {
                transform.localPosition = crouchTopDownPos;
                transform.localRotation = Quaternion.Euler(crouchTopDownRot);
            }
            else
            {
                transform.localPosition = topDownPos;
                transform.localRotation = Quaternion.Euler(topDownRot);
            }
        }
        else
        {
            transform.localPosition = thirdPersonPos;
            transform.localRotation = Quaternion.Euler(thirdPersonRot);
        }
    }

    void PreventCameraClipping()
    {
        if (isTopDown) return;

        Vector3 desiredWorldPos = player.TransformPoint(thirdPersonPos);
        Vector3 cameraRayOrigin = player.position + Vector3.up * headPosition;
        Vector3 directionToTarget = desiredWorldPos - cameraRayOrigin;
        float maxDistance = directionToTarget.magnitude;

        RaycastHit hit;
        Vector3 targetPosition;

        if (Physics.Raycast(cameraRayOrigin, directionToTarget.normalized, out hit, maxDistance))
        {
            targetPosition = cameraRayOrigin + directionToTarget.normalized * (hit.distance - 0.5f);
        }
        else
        {
            targetPosition = desiredWorldPos;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}