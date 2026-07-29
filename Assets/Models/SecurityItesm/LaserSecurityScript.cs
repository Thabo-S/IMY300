using UnityEngine;

public class LaserSecurityScript : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Only objects with this tag trip the laser.")]
    public string playerTag = "Player";

    [Tooltip("If true, the laser can only ever fire once. If false, it re-arms " +
             "as soon as the player leaves the trigger volume (walking through " +
             "again will alert the guards again).")]
    public bool triggerOnce = false;

    [Header("Alarm Sound")]
    [Tooltip("AudioSource that plays the alarm when the laser is tripped. " +
             "Assign this in the Inspector - typically a source sitting on the " +
             "laser emitter/panel.")]
    public AudioSource alarmAudioSource;

    [Tooltip("Alarm clip to play. If left empty, whatever clip is already set " +
             "on the AudioSource is used instead.")]
    public AudioClip alarmClip;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;

        Vector3 alarmPosition = other.transform.position;

        PlayAlarmSound();
        AlertAllGuards(alarmPosition);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!triggerOnce) hasTriggered = false; // re-arm for next crossing
    }

    private void PlayAlarmSound()
    {
        if (alarmAudioSource == null) return;

        if (alarmClip != null)
            alarmAudioSource.PlayOneShot(alarmClip);
        else
            alarmAudioSource.Play();
    }

    private void AlertAllGuards(Vector3 alarmPosition)
    {
        foreach (Guard guard in Guard.AllGuards)
        {
            if (guard == null) continue; // guard may have been destroyed
            guard.TriggerLaserAlarm(alarmPosition);
        }
    }
}