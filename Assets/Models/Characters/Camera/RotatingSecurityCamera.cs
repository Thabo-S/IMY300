using UnityEngine;

public class RotatingSecurityCamera : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f; // Degrees per second
    [SerializeField] private float maxRotationAngle = 90f; // Pan 90 degrees each direction

    [Header("Detection Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float alertRadius = 15f;
    [Tooltip("Only guards within this distance will be alerted.")]

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip alarmClip;

    private float currentRotation = 0f;
    private int rotationDirection = 1; // 1 for right, -1 for left
    private bool isTriggered = false;

    private void Start()
    {
        // Get or create AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (alarmClip != null)
        {
            audioSource.clip = alarmClip;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No alarm clip assigned. Alarm sound won't play.");
        }
    }

    private void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        // Update rotation angle
        currentRotation += rotationSpeed * rotationDirection * Time.deltaTime;

        // Reverse direction at limits
        if (currentRotation >= maxRotationAngle)
        {
            currentRotation = maxRotationAngle;
            rotationDirection = -1;
        }
        else if (currentRotation <= -maxRotationAngle)
        {
            currentRotation = -maxRotationAngle;
            rotationDirection = 1;
        }

        // Apply rotation around Y axis (pan left/right)
        transform.localRotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isTriggered)
        {
            isTriggered = true;
            TriggerAlarm();
            AlertNearbyGuards();
        }
    }

    private void TriggerAlarm()
    {
        Debug.Log($"[{gameObject.name}] Alarm triggered! Player detected!");

        if (audioSource != null && alarmClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void AlertNearbyGuards()
    {
        // Find all guards in the scene
        Guard[] allGuards = FindObjectsOfType<Guard>();

        if (allGuards.Length == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] No guards found in scene to alert.");
            return;
        }

        // Find the nearest guard within alert radius
        Guard nearestGuard = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Guard guard in allGuards)
        {
            if (guard == null) continue;

            float distance = Vector3.Distance(transform.position, guard.transform.position);
            if (distance < nearestDistance && distance <= alertRadius)
            {
                nearestDistance = distance;
                nearestGuard = guard;
            }
        }

        if (nearestGuard == null)
        {
            Debug.Log($"[{gameObject.name}] No guard within alert radius ({alertRadius}m).");
            isTriggered = false;
            return;
        }

        // Alert the nearest guard
        nearestGuard.TriggerLaserAlarm(transform.position);
        Debug.Log($"[{gameObject.name}] Alerted guard '{nearestGuard.gameObject.name}' ({nearestDistance:F1}m away).");
    }

    public void ResetTrigger()
    {
        isTriggered = false;
    }
}
