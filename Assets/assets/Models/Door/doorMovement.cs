using System.Collections;
using UnityEngine;

public class doorMovement : MonoBehaviour
{
    public enum DoorState { Closed, Open }

    [Header("Door State")]
    public DoorState currentState = DoorState.Closed;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;

    private Coroutine _animationCoroutine;

    public void ToggleDoor()
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        float zDelta = GetRotationDelta();

        if (currentState == DoorState.Closed)
        {
            _animationCoroutine = StartCoroutine(RotateDoor(zDelta));
            currentState = DoorState.Open;
        }
        else
        {
            _animationCoroutine = StartCoroutine(RotateDoor(-zDelta));
            currentState = DoorState.Closed;
        }
    }

    private float GetRotationDelta()
    {
        if (CompareTag("Door_Left_Swing"))
        {
            return -90f;
        }
        else if (CompareTag("Door_Right_Swing"))
        {
            return 90f;
        }

        Debug.LogWarning($"[doorMovement] {gameObject.name} has no recognized door tag (Door_Left_Swing / Door_Right_Swing) - defaulting to -90.");
        return -90f;
    }

    private IEnumerator RotateDoor(float zDelta)
    {
        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, 0f, zDelta);
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.localRotation = targetRot;
    }
}