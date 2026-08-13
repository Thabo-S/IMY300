using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    private Vector3 thirdPersonPos = new Vector3(4.11f, 12.59f, -16.21f);
    private Vector3 thirdPersonRot = new Vector3(15f, 0f, 0f);

    private Vector3 topDownPos = new Vector3(0f, 0.614f, -0.235f);
    private Vector3 topDownRot = new Vector3(0f, 0f, 0f);

    private Vector3 crouchTopDownPos = new Vector3(0f, 0.165f, 0.1f);
    private Vector3 crouchTopDownRot = new Vector3(20f, 0f, 0f);

    private bool isCrouching = false;
    private Transform player;
    public float headPosition = 0.614f;

    private SkinnedMeshRenderer playerMeshRenderer;

    //[SerializeField] private float smoothSpeed = 10f;

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
        if (Input.GetKeyDown(KeyCode.C))
        {
            UpdateCameraTransform();
        }

        //PreventCameraClipping();
    }

    public void ToggleCrouchView(bool crouchingState)
    {
        isCrouching = crouchingState;
        UpdateCameraTransform();
    }

    void UpdateCameraTransform()
    {
        if (isCrouching)
        {
            transform.localPosition = crouchTopDownPos;
            transform.localRotation = Quaternion.Euler(crouchTopDownRot);

            if (playerMeshRenderer != null)
            {
                playerMeshRenderer.enabled = isCrouching;
            }
        }
        else
        {
            transform.localPosition = topDownPos;
            transform.localRotation = Quaternion.Euler(topDownRot);

            if (playerMeshRenderer != null)
            {
                playerMeshRenderer.enabled = isCrouching;
            }
        }
    }

}