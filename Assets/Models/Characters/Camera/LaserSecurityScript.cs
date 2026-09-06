using UnityEngine;

public class LaserSecurityScript : MonoBehaviour
{
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;


            Debug.Log("[LASER] Player triggered the laser, guards alerted!");
        }



    }
}