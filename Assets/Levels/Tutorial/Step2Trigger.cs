using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Step2Trigger : MonoBehaviour
{
    public TutorialManager tutorialManager;
    public GameObject Step2UI;

    // THIS IS FOR ALLOWING THE TRIGGER TO ONLY WORK ONCE
    public bool isTriggered = false;

    private void Start()
    {
        tutorialManager = GameObject.FindAnyObjectByType<TutorialManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            Debug.Log("Triggered Step 2");

            isTriggered = true;

            tutorialManager.displayRoomLights.SetActive(true);

            StartCoroutine(TimeWait());
        }
    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(2f);

        Step2UI.SetActive(true);

        tutorialManager.StartStep2();
    }
}