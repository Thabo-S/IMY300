using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject LevelSelectOverlay;

    private void Start()
    {

    }

    public void showLevelSelectOverlay()
    {
        LevelSelectOverlay.SetActive(true);
    }

    public void QuitGame ()
    {
        Application.Quit();
    }
}
