using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isGamePause;

    public GameObject pauseMenuUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePause)
                ResumeGamePlay();
            else
                PauseGamePlay();
        }
    }

    public void ResumeGamePlay()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isGamePause = false;

        if (CursorManager.instance != null)
            CursorManager.instance.LockCursor();

        Debug.Log("Resuming game play");
    }

    public void PauseGamePlay()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isGamePause = true;

        if (CursorManager.instance != null)
            CursorManager.instance.UnlockCursor();
    }
}