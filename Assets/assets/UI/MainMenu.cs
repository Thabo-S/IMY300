using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Scene1");
    }

    public void QuitGame ()
    {
        Application.Quit();
    }
}
