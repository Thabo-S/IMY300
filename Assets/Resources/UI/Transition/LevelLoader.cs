using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.TimeZoneInfo;

/// <summary>
/// Persistent across scenes (DontDestroyOnLoad) - lives once in a bootstrap
/// scene. Plays a fade-to-black Animator transition, waits, loads the target
/// scene, then fades back in. SceneController's scene-loading methods should
/// route through LoadScene() here instead of calling SceneManager.LoadScene
/// directly.
/// </summary>
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    public Animator transitionAnimator;

    [Tooltip("How long to wait once fade-out starts before actually switching scenes - should roughly match your fade-out animation's length.")]
    public float transitionDelay = 1f;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        if (transitionAnimator != null)
            transitionAnimator.SetTrigger("LevelLoader");

        yield return new WaitForSecondsRealtime(transitionDelay);

        SceneManager.LoadScene(sceneName);
    }
}