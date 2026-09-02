using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TargetTrigger : MonoBehaviour
{
    private TutorialManager tutorialManeger;

    public GameObject player;

    private bool isTriggered = false;
    private void Start()
    {
        tutorialManeger = GameObject.FindAnyObjectByType<TutorialManager>();

        player = GameObject.FindGameObjectWithTag("Player");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            Debug.Log("[TARGET] Player in the target zone");

            disableInputs();
        }
    }


    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(3);

    }

    public void disableInputs()
    {
        InputMananger input = player.GetComponent<InputMananger>();

        if (input != null)
            input.enabled= false;

        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;
    }

    public void enableInputs()
    {

        InputMananger input = player.GetComponent<InputMananger>();

        if (input != null)
            input.enabled = true;

        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
            animator.enabled = true;
    }
}


