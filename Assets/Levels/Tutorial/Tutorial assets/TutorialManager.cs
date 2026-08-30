using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class TutorialManager : MonoBehaviour
{
    public GameObject startTutorialOverlay;
    public GameObject player;
    public Player playerScript;
    //public GameObject guard;

    [Header("Step 1 - Movement")]
    public GameObject step1Panel;

    // ----------- Reference and Variables -------------
    // -------------------------------------------------

    private void Awake()
    {
        
    }

    void Start()
    {
        startTutorialOverlay.SetActive(true);
        player.SetActive(false);
        playerScript = player.GetComponent<Player>();
        step1Panel.SetActive(false);
    }

    void Update()
    {


    }

    public void hideOverlay()
    {
        startTutorialOverlay.SetActive(false);
        player.SetActive(true);
        //WelcomeCam.SetActive(false);

        LockCursor();

        RunDelayed(StartStep1, 3f);
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------------- DELAYED CALL HELPER ----------------

    private void RunDelayed(Action action, float delaySeconds)
    {
        StartCoroutine(DelayedCoroutine(action, delaySeconds));
    }

    private IEnumerator DelayedCoroutine(Action action, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        action?.Invoke();
    }

    // ---------------- PANEL HELPERS ----------------

    private void ShowPanel(GameObject panel)
    {
        panel.SetActive(true);

        if (CursorManager.instance != null)
        {
            CursorManager.instance.UnlockCursor();
        }
    }

    private void HidePanel(GameObject panel)
    {
        panel.SetActive(false);

        if (CursorManager.instance != null)
        {
            CursorManager.instance.LockCursor();
        }
    }

    // ---------------- STEP 1: MOVEMENT & CAMERA ----------------


    public void StartStep1()
    {
        ShowPanel(step1Panel);
    }

    public void hideStartStep1()
    {
        HidePanel(step1Panel);
    }


    // ---------------- STEP 2: DOOR PROXIMITY PROMPT ----------------

    //private void CheckDoorProximity()
    //{
    //    if (doorWaypoint == null || player == null) return;
    //    if (doorPromptShown) return; // only trigger once

    //    float distance = Vector3.Distance(player.transform.position, doorWaypoint.position);

    //    if (distance <= doorPromptRange)
    //    {
    //        ShowDoorPrompt();
    //    }
    //}

    //private void ShowDoorPrompt()
    //{
    //    doorPromptShown = true;

    //    if (step1Panel != null) step1Panel.SetActive(true);
    //    if (step1Text != null)
    //    {
    //        step1Text.text = "[E] Open Door";
    //    }
    //}

    //public void pickUpItems()
    //{
    //    step1Panel.SetActive(true);

    //    step1Text.text = "[E] Pick up the items.\r\n Watch them fill up you hotbar";

    //    Invoke(nameof(HideStep1Panel), 3f);

    //    greekSculpture.SetActive(true);

    //    crown2.SetActive(true);

    //    actionKeys.SetActive(true);

    //    StartCoroutine(TimeWait());

    //    HotbarInteractionText();

    //}

    //public void HotbarInteractionText()
    //{
    //    step1Panel.SetActive(true);

    //    step1Text.text = "[1-5] Select a hotbar slot\r\n [G] Drop selected item\r\n\r\n You can carry, swap, and drop items anytime.";

    //    Invoke(nameof(HideStep1Panel), 6f);
    //}

    //IEnumerator TimeWait()
    //{
    //    yield return new WaitForSeconds(3);

    //}

    //public void PlayerFailedStep3()
    //{
    //    CharacterController cc = player.GetComponent<CharacterController>();
    //    if (cc != null) cc.enabled = false;

    //    player.transform.position = step3SpawnPoint.position;

    //    if (cc != null) cc.enabled = true;

    //    guardScript.ResetGuard();


    //    step1Panel.SetActive(true);
    //    step1Text.text = "You were heard! Try walking to stay quiet.";

    //    Invoke(nameof(HideStep1Panel), 3f);
    //}

    //public bool guardShots = false;

    //public void playerSpottedByGuard()
    //{
    //    if (guardShots) return;

    //    step1Panel.SetActive(true);

    //    step1Text.text = "You were spotted!\r\n Guards will shoot at you and you'll lose health \r\n You restart at 0 healt.";

    //    Invoke(nameof(HideStep1Panel), 3f);

    //    guardShots = true;
    //}
}