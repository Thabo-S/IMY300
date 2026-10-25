using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class doorMovement : MonoBehaviour
{
    public enum DoorState { Closed, Open }

    [Header("Keycard UI")]
    [SerializeField] private Transform canvas;
    [SerializeField] private float messageDuration = 2f;

    private Coroutine _messageCoroutine;


    [Header("Door State")]
    public DoorState currentState = DoorState.Closed;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float garageDoorAnimationDuration = 2f;
    [SerializeField] private float garageDoorHeight = 1.8f;

    [Tooltip("Y-axis rotation delta applied when opening. Negative opens one way, positive opens the other. Set per-door.")]
    [SerializeField] private float openRotationY = -90f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    private AudioClip doorSound;
    private AudioClip keycardDoorSound;
    private AudioClip garageDoorSound;

    private Coroutine _animationCoroutine;

    private GameObject player;

    [SerializeField] private Outline outline;

    // Absolute poses, cached once. Every toggle animates to one of these, so
    // reversing mid-animation (spam clicking) can never drift out of bounds.
    private Quaternion closedLocalRotation;
    private Quaternion openLocalRotation;
    private Vector3 closedLocalPosition;
    private Vector3 openLocalPosition;

    private void Awake()
    {
        CacheDoorPoses();
    }

    private void CacheDoorPoses()
    {
        Quaternion openOffset = Quaternion.Euler(0f, GetRotationDelta(), 0f);
        Vector3 liftOffset = new Vector3(0f, garageDoorHeight, 0f);

        if (currentState == DoorState.Closed)
        {
            closedLocalRotation = transform.localRotation;
            openLocalRotation = closedLocalRotation * openOffset;

            closedLocalPosition = transform.localPosition;
            openLocalPosition = closedLocalPosition + liftOffset;
        }
        else
        {
            // Door was placed in the scene already open
            openLocalRotation = transform.localRotation;
            closedLocalRotation = openLocalRotation * Quaternion.Inverse(openOffset);

            openLocalPosition = transform.localPosition;
            closedLocalPosition = openLocalPosition - liftOffset;
        }
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f;
            }
        }

        if (outline == null)
            outline = GetComponent<Outline>();

        if (this.tag == "Door_Keycard")
        {
            if (canvas == null)
                canvas = transform.Find("Canvas");

            if (canvas == null)
            {
                Debug.LogWarning($"{name}: Could not find a child object named 'Canvas'.");
            }
            else
            {
                canvas.gameObject.SetActive(false);
            }
        }

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
        ToggleRotatingDoor();
    }

    public void ToggleKeycardDoor()
    {
        PlaySound(keycardDoorSound);
        ToggleRotatingDoor();
    }

    private void ToggleRotatingDoor()
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        bool opening = currentState == DoorState.Closed;
        currentState = opening ? DoorState.Open : DoorState.Closed;

        Quaternion target = opening ? openLocalRotation : closedLocalRotation;
        _animationCoroutine = StartCoroutine(RotateDoor(target));
    }

    public void ToggleGarageDoor()
    {
        PlaySound(garageDoorSound);

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        bool opening = currentState == DoorState.Closed;
        currentState = opening ? DoorState.Open : DoorState.Closed;

        Vector3 target = opening ? openLocalPosition : closedLocalPosition;
        _animationCoroutine = StartCoroutine(MoveGarageDoor(target));
    }

    public void RemoveKeycardRequirement()
    {
        gameObject.tag = "Door";

        if (outline != null)
        {
            outline.OutlineColor = Color.white;
        }
    }



    public void showErrorMessage()
    {
        if (canvas == null) return;

        canvas.gameObject.SetActive(true);

        if (_messageCoroutine != null)
            StopCoroutine(_messageCoroutine);

        _messageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        canvas.gameObject.SetActive(false);
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
        return openRotationY;
    }

    private IEnumerator RotateDoor(Quaternion targetRot)
    {
        Quaternion startRot = transform.localRotation;

        // Scale duration by how far there is left to go, so reversing a
        // half-open door takes half the time instead of the full duration.
        float fullAngle = Mathf.Max(Mathf.Abs(openRotationY), 0.01f);
        float fraction = Mathf.Clamp01(Quaternion.Angle(startRot, targetRot) / fullAngle);
        float duration = Mathf.Max(animationDuration * fraction, 0.01f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.localRotation = targetRot;
        _animationCoroutine = null;
    }

    private IEnumerator MoveGarageDoor(Vector3 targetPos)
    {
        Vector3 startPos = transform.localPosition;

        float fullDistance = Mathf.Max(Mathf.Abs(garageDoorHeight), 0.01f);
        float fraction = Mathf.Clamp01(Vector3.Distance(startPos, targetPos) / fullDistance);
        float duration = Mathf.Max(garageDoorAnimationDuration * fraction, 0.01f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
        _animationCoroutine = null;
    }
}