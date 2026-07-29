//using UnityEngine;
//using UnityEngine.UI;

//public class PlayerLookAround : MonoBehaviour
//{
//    public Camera cam;

//    public float xRotation = 0f;

//    public Slider mouseSensitivity;

//    public float xDirectionSensitivity = 8f;
//    public float yDirectionSensitivity = 8f;

//    [SerializeField] private float VerticalClamp = 20f;

//    public bool updatingRotation = true;
//    private void Start()
//    {
//        mouseSensitivity.value = xDirectionSensitivity;
//    }

//    private void Update()
//    {
//        if (!updatingRotation) return;
//        xDirectionSensitivity = yDirectionSensitivity = mouseSensitivity.value;
//    }

//    //public void CalculatePlayerLookAround(Vector2 PlayerLookAround)
//    //{
//    //    if (!updatingRotation) return;

//    //    float mouseX = PlayerLookAround.x;
//    //    float mouseY = PlayerLookAround.y;

//    //    // CALCULATE THE CAMERA ROUTAION BASED ON THE MOUSE INPUTS
//    //    xRotation -= (mouseY * Time.deltaTime) * yDirectionSensitivity;
//    //    xRotation = Mathf.Clamp(xRotation, -VerticalClamp + 10 , VerticalClamp);

//    //    cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

//    //    transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xDirectionSensitivity);

//    //}
//    public void CalculatePlayerLookAround(Vector2 PlayerLookAround)
//    {
//        // If rotation updates are disabled, only allow zero-vector inputs (used by Inventory to freeze camera)
//        if (!updatingRotation && PlayerLookAround != Vector2.zero) return;

//        float mouseX = PlayerLookAround.x;
//        float mouseY = PlayerLookAround.y;

//        // CALCULATE THE CAMERA ROTATION BASED ON THE MOUSE INPUTS
//        xRotation -= (mouseY * Time.deltaTime) * yDirectionSensitivity;
//        xRotation = Mathf.Clamp(xRotation, -VerticalClamp + 10, VerticalClamp);

//        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

//        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xDirectionSensitivity);
//    }
//}

using UnityEngine;
using UnityEngine.UI;

public class PlayerLookAround : MonoBehaviour
{
    // Static singleton instance so Inventory can easily access it (just like the video)
    public static PlayerLookAround instance;

    public Camera cam;
    public float xRotation = 0f;
    public Slider mouseSensitivity;

    public float xDirectionSensitivity = 8f;
    public float yDirectionSensitivity = 8f;

    [SerializeField] private float VerticalClamp = 20f;

    public bool updatingRotation = true;

    private void Awake()
    {
        // Set the static instance so other scripts can access PlayerLookAround.instance
        instance = this;
    }

    private void Start()
    {

        // Force rotation state active when the player loads in
        updatingRotation = true;

        if (mouseSensitivity != null)
        {
            mouseSensitivity.value = xDirectionSensitivity;
        }

        cam = Camera.main;

        mouseSensitivity.value = xDirectionSensitivity;


    }

    private void Update()
    {
        // Return early if camera rotation updates are paused (e.g. inventory is open)
        if (!updatingRotation) return;

        if (mouseSensitivity != null)
        {
            xDirectionSensitivity = yDirectionSensitivity = mouseSensitivity.value;
        }
    }

    public void CalculatePlayerLookAround(Vector2 input)
    {
        if (!updatingRotation) return;

        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime) * yDirectionSensitivity;
        xRotation = Mathf.Clamp(xRotation, -VerticalClamp + 10, VerticalClamp);

        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xDirectionSensitivity);
    }
}