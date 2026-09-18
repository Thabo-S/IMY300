using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class LaserSecurityScript : MonoBehaviour
{
    public static bool alarmOn = false;

    private bool isTriggered = false;

    public GameObject respawnPoint;

    public GameObject laserWarning;

    public GameObject alarmIconReference;

    public static GameObject alarmIcon;

    public GameObject[] guardsList;

    public AudioSource alarm;

    public AudioClip alarmSound;

    [Header("Alert Settings")]
    [Tooltip("Only guards within this distance of the laser will be alerted. " +
             "Set very high (e.g. 9999) to always alert the single nearest guard regardless of distance.")]
    public float alertRadius = 10f;

    private void Start()
    {
        if (laserWarning == null)
            Debug.LogWarning($"{name}: 'laserWarning' is not assigned in the inspector.");

        guardsList = GameObject.FindGameObjectsWithTag("Guard");

        //alarmIcon = GameObject.FindGameObjectWithTag("AlarmWarning");

        alarmIcon = alarmIconReference;

        if (alarmIcon != null)
            alarmIcon.SetActive(false);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            PlayAlarm();

            Debug.Log("Level index : " + SceneController.Instance.GetCurrentLevelIndex());

            if (SceneController.Instance.GetCurrentLevelIndex() != 0)
            {
                alertGuards();
                return;
            }

            // =============== ONLY RUNS IN THE TUTORIAL ===============
            // =========================================================

            StartCoroutine(TurnOffAlarmRoutine(3f));

            if (laserWarning != null)
            {
                laserWarning.SetActive(true);
            }

            GameObject player = other.gameObject;
            CharacterController cc = player.GetComponent<CharacterController>();

            if (cc != null) cc.enabled = false;

            player.transform.position = respawnPoint.transform.position;

            if (cc != null) cc.enabled = true;

            if (laserWarning != null)
                StartCoroutine(TimeWait(4f, laserWarning));
        }
    }

    private void PlayAlarm()
    {
        if (alarmOn) return;

        alarmOn = true;

        if(alarmIcon !=null) alarmIcon.SetActive(true);

        if (alarm != null)
        {
            if (alarmSound != null)
            {
                alarm.clip = alarmSound;
            }
            else
            {
                Debug.LogWarning($"{name}: 'alarmSound' AudioClip is not assigned in the inspector!");
            }

            alarm.Play();
        }
        else
        {
            Debug.LogWarning($"{name}: 'alarm' AudioSource is not assigned - nothing to play.");
        }
    }

    public static void StopAlarm()
    {
        alarmOn = false;

        alarmIcon.SetActive(false);
    }

    private void alertGuards()
    {
        GameObject nearestGuard = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject guard in guardsList)
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
            Debug.Log("[LASER] No guard within alert radius — nothing to alert.");
            isTriggered = false;
            return;
        }

        Guard guardScript = nearestGuard.GetComponent<Guard>();
        if (guardScript == null)
        {
            Debug.LogWarning($"[LASER] Nearest guard '{nearestGuard.name}' has no Guard component — cannot alert.");
            isTriggered = false;
            return;
        }

        guardScript.TriggerLaserAlarm(transform.position, this);
        Debug.Log($"[LASER] Alerted nearest guard '{nearestGuard.name}' ({nearestDistance:F1}m away) to investigate.");
    }

    public void resetAlertTrigger()
    {
        isTriggered = false;
    }

    public IEnumerator TimeWait(float delay, GameObject panel)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }
    private IEnumerator TurnOffAlarmRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (alarm != null && alarm.isPlaying)
        {
            alarm.Stop();
        }

        alarmOn = false;

        if (alarmIcon != null)
        {
            alarmIcon.SetActive(false);
        }

        Debug.Log("[LASER] Alarm silenced after 3 seconds.");
    }
}