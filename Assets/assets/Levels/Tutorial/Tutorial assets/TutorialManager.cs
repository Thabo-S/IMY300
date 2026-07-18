using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject startTutorialOverlay;
    public GameObject player;
    public GameObject WelcomeCam;

    void Start()
    {
        startTutorialOverlay.SetActive(true);
        player.SetActive(false);
        WelcomeCam.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void hideOverlay()
    {
        startTutorialOverlay.SetActive(false);
        player.SetActive(true);
        WelcomeCam.SetActive(false);
    }


}
