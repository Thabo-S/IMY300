using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class MainMenuController : MonoBehaviour
{
    public Button[] levelButtons;

    void Start()
    {
        int progress = PlayerPrefs.GetInt("LevelIndex", 0);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = (i <= progress);

            int levelIndex = i;
            levelButtons[i].onClick.AddListener(() => LoadLevel(levelButtons[levelIndex].tag));
        }
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}