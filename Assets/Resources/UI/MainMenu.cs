using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public GameObject LevelSelectOverlay;
    public GameObject mainMenu;
    public GameObject startBtn;
    public GameObject Logo;

    public float logoMoveDuration = 0.6f;
    public float targetLogoY = 230f;

    private Coroutine logoMoveCoroutine;

    public string store = "Store";



    private void Start()
    {
        Logo = GameObject.Find("Logo");
    }

    public void StartGame()
    {
        Debug.Log("started game");
        startBtn.SetActive(false);

        if (logoMoveCoroutine != null)
            StopCoroutine(logoMoveCoroutine);

        logoMoveCoroutine = StartCoroutine(MoveLogoY(targetLogoY, logoMoveDuration));

        mainMenu.SetActive(true);
    }

    private IEnumerator MoveLogoY(float targetY, float duration)
    {
        RectTransform logoRect = Logo.GetComponent<RectTransform>();
        Vector2 startPos = logoRect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, targetY);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            logoRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        logoRect.anchoredPosition = endPos;
    }

    public void showLevelSelectOverlay()
    {
        LevelSelectOverlay.SetActive(true);
    }

    public void StoreLoad()
    {
        if (string.IsNullOrEmpty(store))
        {
            Debug.LogWarning("[MainMenu] Store Scnene name is not set.");
            return;
        }
        SceneManager.LoadScene(store);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}