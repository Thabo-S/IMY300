using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public GameObject startTutorialOverlay;
    public GameObject player;
    public GameObject WelcomeCam;
    public Player playerScript;
    public GameObject guard;


    public GameObject crown2;
    public GameObject greekSculpture;
    public GameObject actionKeys;

    [Header("Step 1 - Movement")]
    public GameObject step1Panel;
    public TMP_Text step1Text;
    public Transform doorWaypoint;

    [Header("Step 2 - Door Prompt")]
    public float doorPromptRange = 20f;

    private bool step1Active = false;
    private bool hasMoved = false;
    private bool hasLookedAround = false;
    private Vector3 playerStartPos;
    private bool doorPromptShown = false;

    [Header("Step 3 -  Sound awareness")]
    public GameObject step3door;
    public Transform step3SpawnPoint;
    public Guard guardScript;


    private bool itemsPromptShown = false;


    // ----------- Reference and Variables -------------
    // -------------------------------------------------



    void Start()
    {
        startTutorialOverlay.SetActive(true);
        player.SetActive(false);
        WelcomeCam.SetActive(true);
        playerScript = player.GetComponent<Player>();
    }

    void Update()
    {
        if (step1Active)
        {
            CheckStep1Progress();
        }

        CheckDoorProximity();

        if (playerScript != null && playerScript.PlayerHealth < 75f)
        {
            if (guard != null && guard.activeSelf)
            {
                guard.SetActive(false);

                if (!itemsPromptShown)
                {
                    pickUpItems();

                    itemsPromptShown = true;
                }
            }
        }

    }

    public void hideOverlay()
    {
        startTutorialOverlay.SetActive(false);
        player.SetActive(true);
        WelcomeCam.SetActive(false);

        LockCursor();

        StartStep1();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------------- STEP 1: MOVEMENT & CAMERA ----------------

    private void StartStep1()
    {
        step1Active = true;
        hasMoved = false;
        hasLookedAround = false;
        playerStartPos = player.transform.position;

        Time.timeScale = 0f;

        if (step1Panel != null) step1Panel.SetActive(true);
        if (step1Text != null)
        {
            step1Text.text = "Use WASD to move.\nMove the mouse to look around.";
        }
    }

    private void CheckStep1Progress()
    {
        if (Time.timeScale == 0f)
        {
            bool anyMoveKey = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A)
                || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
            bool anyMouseMove = Mathf.Abs(Input.GetAxisRaw("Mouse X")) > 0.01f
                || Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > 0.01f;

            if (anyMoveKey)
            {
                Time.timeScale = 1f;
            }
            else
            {
                return;
            }
        }

        float distanceMoved = Vector3.Distance(playerStartPos, player.transform.position);
        if (distanceMoved > 3f)
        {
            hasMoved = true;
        }

        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.05f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.05f)
        {
            hasLookedAround = true;
        }

        if (hasMoved && hasLookedAround)
        {
            CompleteStep1();
        }
    }

    private void CompleteStep1()
    {
        step1Active = false;

        if (step1Text != null)
        {
            step1Text.text = "Head to the door ahead.";
        }

        Invoke(nameof(HideStep1Panel), 3f);
    }

    public void HideStep1Panel()
    {
        if (step1Panel != null) step1Panel.SetActive(false);

        Time.timeScale = 1f;
    }

    // ---------------- STEP 2: DOOR PROXIMITY PROMPT ----------------

    private void CheckDoorProximity()
    {
        if (doorWaypoint == null || player == null) return;
        if (doorPromptShown) return; // only trigger once

        float distance = Vector3.Distance(player.transform.position, doorWaypoint.position);

        if (distance <= doorPromptRange)
        {
            ShowDoorPrompt();
        }
    }

    private void ShowDoorPrompt()
    {
        doorPromptShown = true;

        if (step1Panel != null) step1Panel.SetActive(true);
        if (step1Text != null)
        {
            step1Text.text = "[E] Open Door";
        }
    }

    public void pickUpItems()
    {
        step1Panel.SetActive(true);

        step1Text.text = "[E] Pick up the items.\r\n Watch them fill up you hotbar";

        Invoke(nameof(HideStep1Panel), 3f);

        greekSculpture.SetActive(true);

        crown2.SetActive(true);

        actionKeys.SetActive(true);

        StartCoroutine(TimeWait());

        HotbarInteractionText();

    }

    public void HotbarInteractionText()
    {
        step1Panel.SetActive(true);

        step1Text.text = "[1-5] Select a hotbar slot\r\n [G] Drop selected item\r\n\r\n You can carry, swap, and drop items anytime.";

        Invoke(nameof(HideStep1Panel), 6f);
    }

    IEnumerator TimeWait()
    {
        yield return new WaitForSeconds(3);

    }

    public void PlayerFailedStep3()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = step3SpawnPoint.position;

        if (cc != null) cc.enabled = true;

        guardScript.ResetGuard();


        step1Panel.SetActive(true);
        step1Text.text = "You were heard! Try walking to stay quiet.";

        Invoke(nameof(HideStep1Panel), 3f);
    }

    public bool guardShots = false;

    public void playerSpottedByGuard()
    {
        if (guardShots) return;

        step1Panel.SetActive(true);

        step1Text.text = "You were spotted!\r\n Guards will shoot at you and you'll lose health \r\n You restart at 0 healt.";

        Invoke(nameof(HideStep1Panel), 3f);

        guardShots = true;
    }
}