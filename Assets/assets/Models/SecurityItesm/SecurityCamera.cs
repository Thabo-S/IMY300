using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Assigned Guard")]
    [Tooltip("The single guard this camera calls in when it spots the player.")]
    [SerializeField] private Guard assignedGuard;

    [Header("Trigger")]
    public string playerTag = "Player";

    [SerializeField] public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("There is no AudioSource on " + gameObject.name);
        }
        else
        {
            Debug.Log("AudioSource found on " + gameObject.name);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[SecurityCamera] OnTriggerEnter fired by '{other.name}' (tag: {other.tag})");

        if (other.CompareTag(playerTag))
        {
            TriggerAlarm();
            AlertGuard(other.transform);
        }
    }

    void TriggerAlarm()
    {
        Debug.Log("ALARM! Camera spotted the player!");

        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void AlertGuard(Transform playerTransform)
    {
        if (assignedGuard == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Camera tripped, but no Guard is assigned in the Inspector.");
            return;
        }

        assignedGuard.TriggerLaserAlarm(playerTransform.position);

        Debug.Log($"[{gameObject.name}] Alerted {assignedGuard.gameObject.name} — investigating camera trip point.");
    }
}