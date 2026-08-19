using System.Collections;
using UnityEngine;

public class doorMovement : MonoBehaviour
{
    public enum DoorState { Closed, Open }

    [Header("Door State")]
    public DoorState currentState = DoorState.Closed;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float garageDoorAnimationDuration = 2f;
    [SerializeField] private float garageDoorHeight = 1.8f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    private AudioClip doorSound;
    private AudioClip keycardDoorSound;
    private AudioClip garageDoorSound;

    private Coroutine _animationCoroutine;

    private GameObject player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        LoadDoorSounds();
    }

    private void LoadDoorSounds()
    {
        // DON'T FORGET TO CHENGE THE AUDIO TO THE CORRECT ONE BUDDY
        // =========================================================
        doorSound = Resources.Load<AudioClip>("Audio/SFX/Door_Handle");
        keycardDoorSound = Resources.Load<AudioClip>("Audio/SFX/Door_Handle");
        garageDoorSound = Resources.Load<AudioClip>("Audio/SFX/Door_Handle");

        if (doorSound == null)
            Debug.LogWarning($"{name}: Could not find 'Audio/Door_Handle' in a Resources folder.");
        if (keycardDoorSound == null)
            Debug.LogWarning($"{name}: Could not find 'Audio/SFX/KeycardDoorSound' in a Resources folder.");
        if (garageDoorSound == null)
            Debug.LogWarning($"{name}: Could not find 'Audio/SFX/GarageDoorSound' in a Resources folder.");
    }

    public void ToggleDoor()
    {
        PlaySound(doorSound);

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        float yDelta = GetRotationDelta();

        if (currentState == DoorState.Closed)
        {
            _animationCoroutine = StartCoroutine(RotateDoor(yDelta));
            currentState = DoorState.Open;
        }
        else
        {
            _animationCoroutine = StartCoroutine(RotateDoor(-yDelta));
            currentState = DoorState.Closed;
        }
    }

    public void ToggleKeycardDoor()
    {
        PlaySound(keycardDoorSound);

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        float yDelta = GetRotationDelta();

        if (currentState == DoorState.Closed)
        {
            _animationCoroutine = StartCoroutine(RotateDoor(yDelta));
            currentState = DoorState.Open;
        }
        else
        {
            _animationCoroutine = StartCoroutine(RotateDoor(-yDelta));
            currentState = DoorState.Closed;
        }
    }

    public void ToggleGarageDoor()
    {
        PlaySound(garageDoorSound);

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        if (currentState == DoorState.Closed)
        {
            _animationCoroutine = StartCoroutine(MoveGarageDoor(garageDoorHeight));
            currentState = DoorState.Open;
        }
        else
        {
            _animationCoroutine = StartCoroutine(MoveGarageDoor(-garageDoorHeight));
            currentState = DoorState.Closed;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private float GetRotationDelta()
    {
        return -90f;
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

    private IEnumerator MoveGarageDoor(float yDelta)
    {
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = startPos + new Vector3(0f, yDelta, 0f);
        float elapsed = 0f;

        while (elapsed < garageDoorAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
    }
}