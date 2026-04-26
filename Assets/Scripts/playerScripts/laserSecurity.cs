using UnityEngine;

public class LaserSecurity : MonoBehaviour
{
    void Start()
    {
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
    }
}