using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // --- NEW: Required for TextMeshProUGUI ---

public class LobbyController : MonoBehaviour
{
    public Button selectLevel;
    public Button blackMarket;
    public Button backToLobby;
    public Button mainMenu;
    public Button store;

    public GameObject selectLevelOverlay;
    public GameObject blackMarketOverlay;

    [Header("Records UI")]
    public TextMeshProUGUI bestMoneyText;
    public TextMeshProUGUI fastestTimeText;
    public GameObject Achievements;

    private void Start()
    {
        // --- NEW: Fetch and display the records when the Lobby loads ---
        if (bestMoneyText != null)
        {
            int bestMoney = PlayerPrefs.GetInt("BestMoney", 0);
            bestMoneyText.text = $"Best Haul: ${bestMoney:N0}";
        }

        if (fastestTimeText != null)
        {
            float fastestTime = PlayerPrefs.GetFloat("FastestTime", 999999f);

            if (fastestTime == 999999f)
            {
                // If the player hasn't beaten a level yet
                fastestTimeText.text = "Fastest Time: --:--";
            }
            else
            {
                // Convert the raw seconds back into minutes and seconds
                int min = Mathf.FloorToInt(fastestTime / 60);
                int sec = Mathf.FloorToInt(fastestTime % 60);
                fastestTimeText.text = $"Fastest Time: {min:00}:{sec:00}";
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Achievements != null)
            {
                RectTransform rectTransform = Achievements.GetComponent<RectTransform>();

                if (rectTransform != null)
                {
                    if (Mathf.Approximately(rectTransform.sizeDelta.y, 80f))
                    {
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 250f);
                    }
                    else
                    {
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 80f);
                    }
                }
                else
                {
                    Debug.LogWarning("[LobbyController] 'Achievements' object does not have a RectTransform component!");
                }
            }
        }
    }

    private void Awake()
    {
        if (selectLevel != null)
        {
            selectLevel.onClick.AddListener(ToggleSelectLevelOverlay);
        }

        if (blackMarket != null)
        {
            blackMarket.onClick.AddListener(ToggleBlackMarketOverlay);
        }

        if (backToLobby != null)
        {
            backToLobby.onClick.AddListener(ToggleBlackMarketOverlay);
        }

        if (mainMenu != null)
        {
            mainMenu.onClick.AddListener(SceneController.Instance.GoToMainMenu);
        }

        if (store != null)
        {
            store.onClick.AddListener(SceneController.Instance.GoToStore);
        }
    }

    private void OnDestroy()
    {
        if (selectLevel != null)
        {
            selectLevel.onClick.RemoveListener(ToggleSelectLevelOverlay);
        }

        if (blackMarket != null)
        {
            blackMarket.onClick.RemoveListener(ToggleBlackMarketOverlay);
        }
    }

    public void ToggleSelectLevelOverlay()
    {
        if (selectLevelOverlay != null)
        {
            selectLevelOverlay.SetActive(!selectLevelOverlay.activeSelf);
        }
        else
        {
            Debug.LogWarning("[LobbyController] 'selectLevelOverlay' is not assigned!");
        }
    }

    public void ToggleBlackMarketOverlay()
    {
        if (blackMarketOverlay != null)
        {
            blackMarketOverlay.SetActive(!blackMarketOverlay.activeSelf);
        }
        else
        {
            Debug.LogWarning("[LobbyController] 'blackMarketOverlay' is not assigned!");
        }
    }
}