using UnityEngine;
using UnityEngine.UI;

public class PlayerLookAround : MonoBehaviour
{
    public Camera cam;

    public float xRotation = 0f;

    public Slider mouseSensitivity;

    public float xDirectionSensitivity = 8f;
    public float yDirectionSensitivity = 8f;

    [SerializeField] private float VerticalClamp = 20f;

    private void Start()
    {
        cam = Camera.main;

        mouseSensitivity.value = xDirectionSensitivity;

    }

    private void Update()
    {
        xDirectionSensitivity = yDirectionSensitivity = mouseSensitivity.value;
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
