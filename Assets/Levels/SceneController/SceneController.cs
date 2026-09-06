using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    private string[] Levels = 
        { "Tutorial", "Level 1", "Level 2" ,"Level 3"};

    private int LevelIndex;
    void Start()
    {
        LevelIndex = PlayerPrefs.GetInt("LevelIndex");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(Levels[LevelIndex]);
    }

    public void NextLevel()
    {
        LevelIndex++;

        PlayerPrefs.SetInt("LevelIndex", LevelIndex);

        SceneManager.LoadScene(Levels[LevelIndex]);
    }

    public void GoToStore()
    {
        SceneManager.LoadScene("Store");
    }


}
