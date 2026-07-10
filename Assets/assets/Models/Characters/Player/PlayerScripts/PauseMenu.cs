using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isGamePause;

    public GameObject pauseMenuUI;

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePause)
            {
                ResumeGamePlay();
            }
            else
            {
                PauseGamePlay();
            }
        }
    }

    public void ResumeGamePlay()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isGamePause = false;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        Debug.Log("Resuming game play");
    }
    public void PauseGamePlay()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isGamePause = true;

        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }
}

