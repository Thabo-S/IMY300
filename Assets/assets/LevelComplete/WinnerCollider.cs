using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinnerCollider : MonoBehaviour
{
    public GameObject levelCompleteUI;
    public GameObject gameRestartUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PickUpScript pickUp = other.GetComponent<PickUpScript>();
            bool hasCrown = pickUp != null && pickUp.IsHolding("Crown");

            if (hasCrown)
            {
                if (levelCompleteUI != null)
                {
                    levelCompleteUI.SetActive(true);
                    levelCompleteUI.GetComponent<Animator>().SetTrigger("Show");
                }
            }
            else
            {
                if (gameRestartUI != null)
                    gameRestartUI.SetActive(true);
            }

            StartCoroutine(ReloadScene());
        }
    }

    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}