using UnityEngine;
using UnityEngine.Audio;
public class LaserSecurity : MonoBehaviour
{

    private AudioSource audioSource;
    void Start()
    {

        audioSource = GetComponent<AudioSource>();


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

        audioSource.Play();
    }
}