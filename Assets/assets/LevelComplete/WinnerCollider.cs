using UnityEngine;

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
            }
        }
    }
}