using System.Collections;
using UnityEngine;

public class doorMovement : MonoBehaviour
{
    public enum DoorState { Closed, Open }

    [Header("Door State")]
    public DoorState currentState = DoorState.Closed;

    [Header("Locking")]
    [Tooltip("Optional. If assigned and isLocked is true, ToggleDoor() calls are ignored. " +
             "LockDoor sets isLocked = false itself right before calling ToggleDoor() on a " +
             "successful QTE, so its own call still goes through.")]
    [SerializeField] private LockDoor lockDoor;

    [Tooltip("Optional. Same idea as Lock Door above, but for a keypad-locked door. " +
             "A door can use one, the other, both, or neither - whichever applies.")]
    [SerializeField] private KeypadLockDoor keypadLockDoor;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;

    private Coroutine _animationCoroutine;

    public void ToggleDoor()
    {
        if (lockDoor != null && lockDoor.isLocked)
        {
            // Locked doors can only be opened via LockDoor's own QTE flow.
            return;
        }

        if (keypadLockDoor != null && keypadLockDoor.isLocked)
        {
            // Locked doors can only be opened via KeypadLockDoor's own QTE flow.
            return;
        }

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