using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinnerCollider : MonoBehaviour
{
    public GameObject levelCompleteUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (levelCompleteUI != null)
            {
                levelCompleteUI.SetActive(true);
                levelCompleteUI.GetComponent<Animator>().SetTrigger("Show");
                StartCoroutine(ReloadScene());
            }
        }
    }
    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}