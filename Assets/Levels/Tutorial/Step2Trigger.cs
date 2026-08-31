using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Step2Trigger : MonoBehaviour
{
    public TutorialManager tutorialManeger;
    public GameObject Step2UI;
    private bool isTriggered = false;

    private void Start()
    {
        tutorialManeger = GameObject.FindAnyObjectByType<TutorialManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            Debug.Log("Triggered Step 2");

            StartCoroutine(TimeWait());
        }
    }

    public void hideUi()
    {
        Step2UI.SetActive(true);
    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(1f);

        Step2UI.SetActive(true);
    }
}