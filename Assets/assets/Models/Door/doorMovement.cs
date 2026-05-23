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

        if (currentState == DoorState.Closed)
        {
            _animationCoroutine = StartCoroutine(RotateDoor(90f));
            currentState = DoorState.Open;
        }
        else
        {
            _animationCoroutine = StartCoroutine(RotateDoor(-90f));
            currentState = DoorState.Closed;
        }
    }

    private IEnumerator RotateDoor(float yDelta)
    {
        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, yDelta, 0f);
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