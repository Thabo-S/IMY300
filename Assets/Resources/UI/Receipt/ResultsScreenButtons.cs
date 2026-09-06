using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultsScreenButtons : MonoBehaviour
{
    [Header("Next Level")]
    [Tooltip("Scene name to load when 'Next Level' is pressed. Set per-level " +
             "in the Inspector - NOT derived from Build Settings order, since " +
             "that order doesn't match gameplay progression in this project.")]
    public string nextLevelSceneName;

    [Header("Store")]
    [Tooltip("Scene name of the full-screen Store scene to load when 'Store' " +
             "is pressed. Add this scene to Build Settings once it exists.")]
    public string storeSceneName;

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void OnNextLevelClicked()
    {
        if (string.IsNullOrEmpty(nextLevelSceneName))
        {
            Debug.LogWarning("[ResultsScreenButtons] Next Level Scene Name is not set in the Inspector.");
            return;
        }

        int currentProgress = PlayerPrefs.GetInt("LevelIndex", 0);
        int nextIndex = currentProgress + 1;
        PlayerPrefs.SetInt("LevelIndex", Mathf.Max(currentProgress, nextIndex));
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelSceneName);
    }

    public void OnStoreClicked()
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene("Store");
    }
}