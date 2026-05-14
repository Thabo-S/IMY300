using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    // These will hold the values from your screenshots
    private Vector3 thirdPersonPos = new Vector3(4.11f, 12.59f, -16.21f);
    private Vector3 thirdPersonRot = new Vector3(15f, 0f, 0f);

    private Vector3 topDownPos = new Vector3(0f, 11.57f, 0f);
    private Vector3 topDownRot = new Vector3(0f, 0f, 0f);

    private bool isTopDown = true;

    void Update()
    {
        // Check for the "C" key toggle
        if (Input.GetKeyDown(KeyCode.C))
        {
            isTopDown = !isTopDown;
            UpdateCameraTransform();
        }
    }

    void UpdateCameraTransform()
    {
        if (isTopDown)
        {
            transform.localPosition = topDownPos;
            transform.localRotation = Quaternion.Euler(topDownRot);
        }
        else
        {
            transform.localPosition = thirdPersonPos;
            transform.localRotation = Quaternion.Euler(thirdPersonRot);
        }
    }
}