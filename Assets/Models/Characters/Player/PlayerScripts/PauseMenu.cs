using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isGamePause;

    public GameObject pauseMenuUI;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Resuming game play");
    }
    public void PauseGamePlay()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isGamePause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

//using UnityEngine;

//public class PauseMenu : MonoBehaviour
//{
//    public static bool isGamePause;

//    public GameObject pauseMenuUI;

//    private void Start()
//    {
//        // NOTE: No longer force-locking the cursor here. This object (and
//        // Inventory) get enabled the instant "Start Tutorial" is pressed, so
//        // locking on Start() hid the cursor before any tutorial intro UI
//        // could use it. Call LockGameplayCursor() explicitly once gameplay
//        // should take control of the mouse (e.g. wire it to the
//        // "Start Tutorial" button's OnClick, or call it after an intro
//        // popup is dismissed).
//    }

//    // Call this once, explicitly, when gameplay should take control of the mouse.
//    public void LockGameplayCursor()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            if (isGamePause)
//            {
//                ResumeGamePlay();
//            }
//            else
//            {
//                PauseGamePlay();
//            }
//        }
//    }

//    public void ResumeGamePlay()
//    {
//        pauseMenuUI.SetActive(false);
//        Time.timeScale = 1f;
//        isGamePause = false;

//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//        Debug.Log("Resuming game play");
//    }
//    public void PauseGamePlay()
//    {
//        pauseMenuUI.SetActive(true);
//        Time.timeScale = 0f;
//        isGamePause = true;

//        Cursor.lockState = CursorLockMode.None;
//        Cursor.visible = true;
//    }
//}