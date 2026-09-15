using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Step7Trigger : MonoBehaviour
{
    private TutorialManager tutorialManager;
    public GameObject blocker;

    private bool isTriggered = false;

    private void Awake()
    {
        blocker.SetActive(false);
        
    }
    private void Start()
    {
        tutorialManager = GameObject.FindAnyObjectByType<TutorialManager>();

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {

            StartCoroutine(TimeWait());

            isTriggered = true;

            blocker.SetActive(true);

        }
    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(2f);

        tutorialManager.StartStep7();

    }


}


