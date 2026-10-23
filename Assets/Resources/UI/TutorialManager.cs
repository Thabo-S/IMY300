using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public GameObject startTutorialOverlay;
    public GameObject player;
    public Player playerScript;
    public InputMananger InputMananger;

    [Header("Player Warning")]
    public GameObject PlayerWarning;
    public GameObject spawnPoint;

    [Header("Lights References")]
    public GameObject officeLights;
    public GameObject artifactRoomLights;
    public GameObject displayRoomLights;
    public GameObject fourththRoomLights;

    [Header("Trigger References")]
    public GameObject Step2Trigger;

    [Header("Blockers References")]
    public GameObject blocker2;

    public GameObject keypadRef;

    [Header("Steps UI")]
    public GameObject step1Panel;
    public GameObject step2Panel;
    public GameObject step3Panel;
    public GameObject step3Panel_2;
    public GameObject step4Panel;
    public GameObject step5Panel;
    public GameObject step6Panel;
    public GameObject step7Panel;

    private Coroutine step2FailureRoutine;
    private bool inputWasEnabledBeforeStep2Failure;

    // ----------- Reference and Variables -------------
    // -------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }
    void Start()
    {
        startTutorialOverlay.SetActive(true);
        //player.SetActive(false);
        playerScript = player.GetComponent<Player>();
        InputMananger = player.GetComponent<InputMananger>();

        PlayerWarning.SetActive(false);
        step1Panel.SetActive(false);
        step2Panel.SetActive(false);
        step3Panel.SetActive(false);
        step3Panel_2.SetActive(false);
        step4Panel.SetActive(false);
        step5Panel.SetActive(false);
        step6Panel.SetActive(false);
        step7Panel.SetActive(false);

        if (CursorManager.instance != null)
            CursorManager.instance.UnlockCursor();

        int progress = PlayerPrefs.GetInt("LevelIndex");
        int currentLevelPrefKey = PlayerPrefs.GetInt("currentLevelPrefKey");

        Debug.Log("HIghest Level Complete is : " + progress);
        Debug.Log("Current Level Being played is : " + currentLevelPrefKey);
    }

    void Update()
    {


    }

    public void hideOverlay()
    {
        if (startTutorialOverlay != null)
            startTutorialOverlay.SetActive(false);

        if (CursorManager.instance != null)
            CursorManager.instance.LockCursor();

        RunDelayed(StartStep1, 3f);
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
        if (step2FailureRoutine != null)
            return;

        if (PlayerWarning != null)
            PlayerWarning.SetActive(true);

        inputWasEnabledBeforeStep2Failure = InputMananger != null && InputMananger.enabled;
        if (InputMananger != null)
            InputMananger.enabled = false;

        step2FailureRoutine = StartCoroutine(ResetAfterStep2Failure());
    }

    private IEnumerator ResetAfterStep2Failure()
    {
        // Let the warning register before moving the player back to the checkpoint.
        yield return new WaitForSeconds(1.25f);

        if (player != null && spawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = spawnPoint.transform.position;
            if (cc != null) cc.enabled = true;
        }

        yield return new WaitForSeconds(2.75f);

        if (PlayerWarning != null)
            PlayerWarning.SetActive(false);

        if (InputMananger != null)
            InputMananger.enabled = inputWasEnabledBeforeStep2Failure;

        step2FailureRoutine = null;
    }

    // ---------------- STEP 3: Picking Up ----------------

    public void StartStep3()
    {
        ShowPanel(step3Panel);
    }

    public void hideStep3()
    {
        HidePanel(step3Panel);
    }
    // ---------------- STEP 3.5: Throwing ----------------

    public void StartStep3_2()
    {
        ShowPanel(step3Panel_2);
    }

    public void hideStep3_2()
    {
        HidePanel(step3Panel_2);
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

        //GameObject.FindAnyObjectByType<Step4Trigger>().guard2.SetActive(false);
        Step4Trigger step4 = GameObject.FindAnyObjectByType<Step4Trigger>();
        if (step4 != null && step4.guard2 != null)
            step4.guard2.SetActive(false);
    }

    public void hideStep5()
    {
        HidePanel(step5Panel);

        TargetTrigger targetTrigger = GameObject.FindAnyObjectByType<TargetTrigger>();

        //targetTrigger.enableInputs();
        if (targetTrigger != null)
            targetTrigger.enableInputs();
    }

    // ---------------- STEP 6: Keypad Iteraction ----------------

    public void StartStep6()
    {
        blocker2.SetActive(false);
        keypadRef.SetActive(true);

        ShowPanel(step6Panel);

    }

    public void hideStep6()
    {
        HidePanel(step6Panel);

    }
    // ---------------- STEP 6: Keypad Iteraction ----------------

    public void StartStep7()
    {
        ShowPanel(step7Panel);
    }

    public void hideStep7()
    {
        HidePanel(step7Panel);
    }
    public IEnumerator TimeWait(float delay, GameObject panel)
    {
        yield return new WaitForSeconds(delay);

        panel.SetActive(false);

    }

}
