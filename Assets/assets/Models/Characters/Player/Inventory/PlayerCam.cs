using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCam : MonoBehaviour
{
    public static PlayerCam instance;

    [Header("Sensitivity")]
    public float sensX;
    public float sensY;

    public Transform oritentation;
    public Transform modelRotation;

    public bool updatingRotation;

    private float xRotation;
    private float yRotation;

    public void Awake()
    {
        instance = this;
    }

    void Start()
    {
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
    }


    void Update()
    {
        if (!updatingRotation) return;
        float mouseX = Input.GetAxisRaw("MouseX") * sensX;
        float mouseY = Input.GetAxisRaw("MouseY") * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp( xRotation,-90f, 90f);

        transform.rotation = Quaternion.Euler( xRotation, yRotation, 0 );
        oritentation.rotation = Quaternion.Euler(0, yRotation, 0);
        modelRotation.rotation = oritentation.rotation;


    }
}
