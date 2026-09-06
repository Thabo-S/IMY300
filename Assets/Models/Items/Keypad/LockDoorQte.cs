using UnityEngine;

using UnityEngine.Events;

public class LockDoorQte : MonoBehaviour
{
   [SerializeField] public Transform pointA;
   [SerializeField] public  Transform pointB;
   [SerializeField] public RectTransform safeZone;
    public float moveSpeed = 1000f;

    public float direction = 1f;

    [Header("Events")]
    public UnityEvent OnQteSuccess;
    public UnityEvent OnQteFail;
    public UnityEvent OnQteCancel;

    private RectTransform pointerTransform;
    private Vector3 targetPosition;
    
    void Start()
    {
        pointerTransform = GetComponent<RectTransform>();
        targetPosition = pointB.position; //move pointer to target 
    }
    void Update()
    {
            pointerTransform.position = Vector3.MoveTowards(pointerTransform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(pointerTransform.position , pointA.position) < 0.1f)
        {
            targetPosition = pointB.position;
            direction = 1f;
        }
        else if (Vector3.Distance(pointerTransform.position , pointB.position) < 0.1f)
        {
            targetPosition = pointA.position;
            direction = -1f;
        }
        //check if pointer is in safe zone
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckSuccess();
        }if (Input.GetKeyDown(KeyCode.C))
        {
            CancelQte();
        }
    }

    void CheckSuccess()
    {
        if(RectTransformUtility.RectangleContainsScreenPoint(safeZone, pointerTransform.position, null))
        {
            Debug.Log("Success! Pointer is in the safe zone.");
            // Add success logic here (e.g., unlock the door)
            OnQteSuccess?.Invoke();
        }
        else
        {
            Debug.Log("Failure! Pointer is not in the safe zone.");
            // Add failure logic here (e.g., reset the pointer position)
            OnQteFail?.Invoke();

        }
    }
    void CancelQte()
    {
        Debug.Log("QTE cancelled by player.");
        OnQteCancel?.Invoke();
    }

}
