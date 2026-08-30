using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PauseMenu : MonoBehaviour
{
    public static bool isGamePause;

    public GameObject pauseMenuUI;
    public GameObject player;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

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

    public void blurBackground()
    {
        if (CursorManager.instance != null)
        {
            if (isGamePause)
                CursorManager.instance.UnlockCursor();
            else
                CursorManager.instance.LockCursor();
        }


        if (player != null)
        {
            PostProcessVolume volume = player.GetComponent<PostProcessVolume>();

            if (volume != null)
            {
                DepthOfField dof;

                if (volume.profile.TryGetSettings(out dof))
                {
                    dof.focusDistance.value = isGamePause ? 0.1f : 50f;
                    dof.focalLength.value = isGamePause ? 50f : 1f;
                }
            }
            else
            {
                Debug.LogWarning("No PostProcessVolume found on the playerCamera!");
            }
        }
    }
}