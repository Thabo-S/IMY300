using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Step4Trigger : MonoBehaviour
{
    public TutorialManager tutorialManeger;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadStep4();
        }
    }

    private void LoadStep4()
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