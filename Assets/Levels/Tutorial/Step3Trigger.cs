using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Step3Trigger : MonoBehaviour
{
    public TutorialManager tutorialManager;
    private bool isTriggered = false;

    public GameObject blocker;

    public GameObject step2Door;
    public doorMovement step2doorMovement;

    private void Start()
    {
        tutorialManager = GameObject.Find("TutorialManager").GetComponent<TutorialManager>();
        step2doorMovement = step2Door.GetComponent<doorMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;

            step2doorMovement.ToggleDoor();

            tutorialManager.officeLights.SetActive(false);

            tutorialManager.artifactRoomLights.SetActive(true);

            blocker.SetActive(false);

        }
    }

}