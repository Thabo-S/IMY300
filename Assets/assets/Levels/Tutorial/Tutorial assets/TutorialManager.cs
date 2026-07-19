using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject startTutorialOverlay;
    public GameObject player;
    public GameObject WelcomeCam;

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




    // ----------- Reference and Variables -------------
    // -------------------------------------------------



    void Start()
    {
        startTutorialOverlay.SetActive(true);
        player.SetActive(false);
        WelcomeCam.SetActive(true);
    }

    void Update()
    {
        if (step1Active)
        {
            CheckStep1Progress();
        }

        CheckDoorProximity();

    }

    public void hideOverlay()
    {
        startTutorialOverlay.SetActive(false);
        player.SetActive(true);
        WelcomeCam.SetActive(false);

        StartStep1();
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

    public void PlayerFailedStep3()
    {
        // 1. Move player to the safe spawn point for the room
        player.transform.position = step3SpawnPoint.position;

        // 2. Reset the guard
        guardScript.ResetGuard();

        // 3. Show feedback
        step1Panel.SetActive(true);
        step1Text.text = "You were spotted! Try walking to stay quiet.";

        // 4. Hide the message after a delay
        Invoke(nameof(HideStep1Panel), 3f);
    }

}