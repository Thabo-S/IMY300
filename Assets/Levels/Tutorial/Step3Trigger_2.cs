using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Step3Trigger_2 : MonoBehaviour
{
    public TutorialManager tutorialManager;
    public bool isTriggered = false;

    private void Start()
    {
        tutorialManager = GameObject.Find("TutorialManager").GetComponent<TutorialManager>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;


            StartCoroutine(TimeWait());

        }
    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(0.5f);

        tutorialManager.StartStep3_2();

    }
}