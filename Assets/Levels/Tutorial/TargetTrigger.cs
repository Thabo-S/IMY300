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

        if (player == null)
            Debug.LogWarning($"{name}: No GameObject tagged 'Player' found in the scene at Start().");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            Debug.Log("[TARGET] Player in the target zone");

            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");


            isTriggered = true;

            disableInputs();
        }
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


