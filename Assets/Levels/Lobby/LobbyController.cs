using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    public Button selectLevel;
    public Button blackMarket;
    public Button backToLobby;

    public GameObject selectLevelOverlay;
    public GameObject blackMarketOverlay;

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