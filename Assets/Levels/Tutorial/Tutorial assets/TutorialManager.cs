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
    public InputMananger InputMananger;

    [Header("Lights References")]
    public GameObject PlayerWarning;
    public GameObject spawnPoint;

    [Header("Lights References")]
    public GameObject officeLights;
    public GameObject artifactRoomLights;
    public GameObject displayRoomLights;
    public GameObject fourththRoomLights;

    [Header("Trigger References")]
    public GameObject Step2Trigger;

    [Header("Steps UI")]
    public GameObject step1Panel;
    public GameObject step2Panel;
    public GameObject step3Panel;
    public GameObject step4Panel;
    public GameObject step5Panel;

    // ----------- Reference and Variables -------------
    // -------------------------------------------------

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        spawnPoint = GameObject.Find("SpawnPoint");
    }

    void Start()
    {
        startTutorialOverlay.SetActive(true);
        player.SetActive(false);
        playerScript = player.GetComponent<Player>();
        InputMananger = player.GetComponent<InputMananger>();

        PlayerWarning.SetActive(false);
        step1Panel.SetActive(false);
        step2Panel.SetActive(false);
        step3Panel.SetActive(false);
        step4Panel.SetActive(false);
        step5Panel.SetActive(false);
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

        if (InputMananger != null)
            InputMananger.enabled = false;

        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;

        if (CursorManager.instance != null)
        {
            CursorManager.instance.UnlockCursor();
        }
    }

    private void HidePanel(GameObject panel)
    {
        panel.SetActive(false);

        if (InputMananger != null)
            InputMananger.enabled = true;

        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
            animator.enabled = true;

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

    public void hideStep1()
    {
        HidePanel(step1Panel);
    }


    // ---------------- STEP 2: SOUND AWARENESS ----------------

    public void StartStep2()
    {
        ShowPanel(step2Panel);
    }

    public void hideStep2()
    {
        HidePanel(step2Panel);
    }

    public void PlayerFailedStep2()
    {
        PlayerWarning.SetActive(true);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = spawnPoint.transform.position;

        if (cc != null) cc.enabled = true;

        StartCoroutine(TimeWait(4f, PlayerWarning));
    }

    // ---------------- STEP 3: Picking Up & Throwing ----------------

    public void StartStep3()
    {
        ShowPanel(step3Panel);
    }

    public void hideStep3()
    {
        HidePanel(step3Panel);
    }

    // ---------------- STEP 4: Warning Zone and Getting Caught ----------------

    public void StartStep4()
    {
        ShowPanel(step4Panel);
    }

    public void hideStep4()
    {
        HidePanel(step4Panel);
    }
    // ---------------- STEP 5: Health & Recouping ----------------

    public void StartStep5()
    {
        ShowPanel(step5Panel);

        GameObject.FindAnyObjectByType<Step4Trigger>().guard2.SetActive(false);
    }

    public void hideStep5()
    {
        HidePanel(step5Panel);

        TargetTrigger targetTrigger = GameObject.FindAnyObjectByType<TargetTrigger>();

        targetTrigger.enableInputs();
    }

    // ---------------- STEP 6: Keypad Iteraction ----------------

    public void StartStep6()
    {
        ShowPanel(step5Panel);
    }

    public void hideStep6()
    {
        HidePanel(step5Panel);

    }


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

    public IEnumerator TimeWait(float delay, GameObject panel)
    {
        yield return new WaitForSeconds(delay);

        panel.SetActive(false);

    }

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