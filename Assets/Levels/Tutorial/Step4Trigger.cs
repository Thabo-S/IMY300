using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Step4Trigger : MonoBehaviour
{
    private TutorialManager tutorialManager;

    public GameObject guard1;
    public GameObject guard2;


    public GameObject lights;

    public bool isTriggered = false;
    private void Start()
    {
        tutorialManager = GameObject.FindAnyObjectByType<TutorialManager>();

        guard2.SetActive(false);

        lights.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {

            isTriggered = true;

            Object.FindAnyObjectByType<Step3Trigger>().pathtostep4.SetActive(false);

            guard1.SetActive(false);

            guard2.SetActive(true);

            lights.SetActive(true);

            StartCoroutine(TimeWait());

        }
    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(2f);

        tutorialManager.StartStep4();

    }

}


