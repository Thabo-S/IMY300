//using System.Collections;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UIElements;

//public class Step5Trigger : MonoBehaviour
//{
//    private TutorialManager tutorialManeger;

//    public GameObject guard;
//    public GameObject RouteToStep5;
//    public GameObject healthBar;

//    private bool isTriggered = false;
//    private void Start()
//    {
//        tutorialManeger = GameObject.FindAnyObjectByType<TutorialManager>();
//    }


//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player") && !isTriggered)
//        {
//            guard.SetActive(false);
//            RouteToStep5.SetActive(false);

//            healthBar.SetActive(true);

//            //tutorialManeger.guard.SetActive(true);

//            isTriggered = true;

//            LoadStep();

//            StartCoroutine(TimeWait());

//        }
//    }
    

//    private void LoadStep()
//    {
//        tutorialManeger.step1Panel.SetActive(true);

//        tutorialManeger.step1Text.text = "Watch the eye icon above the guard.\r\nIt lights up before you're actually spotted — that's your warning.";

//    }

//    IEnumerator TimeWait()
//    {
//        yield return new WaitForSeconds(6);

//        //tutorialManeger.HideStep1Panel();

//    }

//}


