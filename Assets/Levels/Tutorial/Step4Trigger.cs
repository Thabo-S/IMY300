using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Step4Trigger : MonoBehaviour
{
    private TutorialManager tutorialManeger;

    public GameObject guardsCanHear;
    public GameObject RouteToStep3;
    public GameObject RouteToStep5;
    public bool isTriggered = false;
    private void Start()
    {
        tutorialManeger = GameObject.FindAnyObjectByType<TutorialManager>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            guardsCanHear.SetActive(false);

            RouteToStep3.SetActive(false);

            RouteToStep5.SetActive(true);

            isTriggered = true;
            
            StartCoroutine(TimeWait());

        }
    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(6);

        tutorialManeger.HideStep1Panel();


    }

}


