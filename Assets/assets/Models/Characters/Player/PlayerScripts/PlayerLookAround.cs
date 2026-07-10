using UnityEngine;

public class PlayerLookAround : MonoBehaviour
{

    //Keep the camera perspective in 3rd person

    public Camera cam;

    public float xRotation = 0f;

    public float xDirectionSensitivity = 20f;
    public float yDirectionSensitivity = 20f;

    [SerializeField] private float VerticalClamp = 20f;

    //TEMPORARY WAY MAKE THE CURSOR NOT VISIBLE
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //}

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;
        //}
    }

    public void CalculatePlayerLookAround(Vector2 PlayerLookAround)
    {
        float mouseX = PlayerLookAround.x;
        float mouseY = PlayerLookAround.y;

        // CALCULATE THE CAMERA ROUTAION BASED ON THE MOUSE INPUTS
        xRotation -= (mouseY * Time.deltaTime) * yDirectionSensitivity;
        xRotation = Mathf.Clamp(xRotation, -VerticalClamp + 10 , VerticalClamp);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xDirectionSensitivity);

    }
}
