using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    // Standing Coordinates (Standard 3rd Person)
    private Vector3 thirdPersonPos = new Vector3(4.11f, 12.59f, -16.21f);
    private Vector3 thirdPersonRot = new Vector3(15f, 0f, 0f);

    // Standing Coordinates (Standard Top-Down)
    private Vector3 topDownPos = new Vector3(0f, 11.57f, 0f);
    private Vector3 topDownRot = new Vector3(0f, 0f, 0f);

    private Vector3 crouchTopDownPos = new Vector3(0f, 8.22f, 1.26f);
    private Vector3 crouchTopDownRot = new Vector3(20f, 0f, 0f);

    private bool isTopDown = true;
    private bool isCrouching = false; // Keep track of crouch state here too
    private Transform player;
    public float headPosition = 11.5f;

    [SerializeField] private float smoothSpeed = 10f;
    void Start()
    {
        player = transform.parent;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isTopDown = !isTopDown;
            UpdateCameraTransform();
        }

        PreventCameraClipping();
    }

    // This is called by PlayerMovement.cs
    public void ToggleCrouchView(bool crouchingState)
    {
        isCrouching = crouchingState;
        UpdateCameraTransform(); // Refresh the position immediately
    }

    void UpdateCameraTransform()
    {
        if (isTopDown)
        {
            // If crouching in TopDown, use the specific screenshot values
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
            // 3rd Person stays at standard height as you requested
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