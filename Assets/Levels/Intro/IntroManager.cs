using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("References")]
    public AudioSource introAudio;

    [Header("Settings")]
    public string nextSceneName = "Lobby";

    private void Awake()
    {
        CursorManager.instance.LockCursor();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            LoadGame();
        }
    }
    void Start()
    {
        //CursorManager.instance.LockCursor();

        if (!introAudio.isPlaying)
        {
            introAudio.Play();
        }

        StartCoroutine(WaitForAudioToEnd());
    }

    IEnumerator WaitForAudioToEnd()
    {
        // Wait for the exact duration of the audio clip
        yield return new WaitForSeconds(introAudio.clip.length);

        // Once the time is up, load the game
        LoadGame();
    }

    // You will link this method to your Skip Button
    public void SkipIntro()
    {
        LoadGame();
    }

    private void LoadGame()
    {
        // Loads the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}