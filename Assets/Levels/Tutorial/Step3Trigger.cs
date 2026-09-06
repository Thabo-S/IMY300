using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Step3Trigger : MonoBehaviour
{
    public TutorialManager tutorialManager;
    public bool isTriggered = false;

    public GameObject blocker;

    public GameObject step2Door;
    public doorMovement step2doorMovement;

    [Header("Directions")]
    public GameObject pathtostep3;
    public GameObject pathtostep4;


    private void Awake()
    {
        pathtostep4 = GameObject.Find("Path to step4");

        if(pathtostep4 != null)
            pathtostep4.SetActive(false);
    }
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


            pathtostep3.SetActive(false);
            pathtostep4.SetActive(true);

            step2doorMovement.ToggleDoor();

            tutorialManager.officeLights.SetActive(false);

            tutorialManager.artifactRoomLights.SetActive(true);

            blocker.SetActive(false);

            StartCoroutine(TimeWait());

        }
    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(2f);

        tutorialManager.StartStep3();

        Vector3 scale = gameObject.transform.localScale;
        scale.y = 0.25f;
        gameObject.transform.localScale = scale;
    }
}