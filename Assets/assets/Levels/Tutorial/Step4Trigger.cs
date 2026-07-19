using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Step3Trigger : MonoBehaviour
{
    public TutorialManager tutorialManeger;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadStep3();
        }
    }

    private void LoadStep3()
    {
        tutorialManeger.step1Panel.SetActive(true);

        tutorialManeger.step1Text.text = "Nice. Doors work like that throughout the game.";

        StartCoroutine(TimeWait());

    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(3);

        tutorialManeger.HideStep1Panel();
    }
}