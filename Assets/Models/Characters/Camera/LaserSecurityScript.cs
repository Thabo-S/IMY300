using UnityEngine;
using System.Collections;
using System;
public class LaserSecurityScript : MonoBehaviour
{
    private bool isTriggered = false;
    public GameObject respawnPoint;

    public GameObject laserWarning;

    private void Start()
    {
        if (laserWarning == null)
            Debug.LogWarning($"{name}: 'laserWarning' is not assigned in the inspector.");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;

            Debug.Log("[LASER] Player triggered the laser, guards alerted!");

            if (laserWarning != null)
                laserWarning.SetActive(true);

            GameObject player = other.gameObject;

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = respawnPoint.transform.position;

            if (cc != null) cc.enabled = true;

            if (laserWarning != null)
                StartCoroutine(TimeWait(4f, laserWarning));
        }
    }

    public IEnumerator TimeWait(float delay, GameObject panel)
    {
        yield return new WaitForSeconds(delay);

        panel.SetActive(false);
    }
}