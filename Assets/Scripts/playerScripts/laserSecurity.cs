using UnityEngine;
using UnityEngine.Audio;
public class LaserSecurity : MonoBehaviour
{

    private AudioSource audioSource;
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

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0f, 0f, 0.5f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerAlarm();
        }
    }

    void TriggerAlarm()
    {
        Debug.Log("ALARM! Laser tripped by the player!");

        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}