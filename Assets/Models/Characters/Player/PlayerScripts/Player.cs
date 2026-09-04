using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class Player : MonoBehaviour
{
    [Header("Health Variables")]
    public Slider HealthBarSlider;
    public float MaxHealth = 100;
    public float PlayerHealth;
    public Gradient gradient;
    public Image fill;

    [Header("Footstep Audio")]
    public AudioSource footstepAudioScource;
    public AudioClip footstepClip;
    //public AudioSource runningFootsteps;

    [Header("Interaction Sound")]
    public AudioSource interaction;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Toggle Controls")]
    [SerializeField] private GameObject ControlsPanel;

    [Header("Damage Overlay")]
    public Image damageOverlay;
    public float flashInAlpha = 0.5f;
    public float flashInTime = 1f;
    public float fadeOutTime = 1f;

    private Coroutine damageFlashCoroutine;

    private Camera cam;
    public List<Slot> hotbarSlots;

    //================= List Of PlayerPrefs ===================
    // LevelIndex : Use to determine game level
    //=========================================================

    // Gents, You'll add more if you wish to do so

    void Start()
    {
        cam = Camera.main;

        PlayerHealth = MaxHealth;

        HealthBarSlider.maxValue = MaxHealth;

        HealthBarSlider.value = MaxHealth;

        fill.color = gradient.Evaluate(1f);

        footstepAudioScource = GetComponents<AudioSource>()[1];

        hotbarSlots = FindAnyObjectByType<Inventory>().hotbarSlots;
    }




    public void TakeDamage(int damage)
    {
        PlayerHealth -= damage;

        HealthBarSlider.value = PlayerHealth;

        fill.color = gradient.Evaluate(HealthBarSlider.normalizedValue);

        Debug.Log("Player took damage: " + PlayerHealth);

        if (damageOverlay != null)
        {
            if (damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);

            damageFlashCoroutine = StartCoroutine(DamageFlash());
        }

        if(PlayerHealth < 40)
        {
            TutorialManager tutorial = Object.FindAnyObjectByType<TutorialManager>();

            if (tutorial != null)
                tutorial.StartStep5();
            
        }
    }

    public void RecoupHealth(int increaseHealtj)
    {

        PlayerHealth += increaseHealtj;

        if(PlayerHealth > 100)
            PlayerHealth = 100;

        HealthBarSlider.value = PlayerHealth;

        fill.color = gradient.Evaluate(HealthBarSlider.normalizedValue);

        Debug.Log("Player health increased: " + PlayerHealth);

        if(PlayerPrefs.GetInt("LevelIndex", 0) == 0)
        {
            TutorialManager tutorial = Object.FindAnyObjectByType<TutorialManager>();

            if (tutorial != null)
                tutorial.StartStep6();
        }
    }


    private IEnumerator DamageFlash()
    {
        Color c = damageOverlay.color;

        c.a = flashInAlpha;
        damageOverlay.color = c;

        float elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(flashInAlpha, 0f, elapsed / fadeOutTime);
            damageOverlay.color = c;
            yield return null;
        }

        c.a = 0f;
        damageOverlay.color = c;
    }

    public void OnFootstep()
    {
        footstepAudioScource.pitch = Random.Range(minPitch, maxPitch);
        footstepAudioScource.PlayOneShot(footstepClip);
    }

    public void Heal(float amount)
    {
        PlayerHealth = Mathf.Clamp(PlayerHealth + amount, 0, MaxHealth);

        HealthBarSlider.value = PlayerHealth;

        fill.color = gradient.Evaluate(HealthBarSlider.normalizedValue);

        Debug.Log("Player healed: " + PlayerHealth);
    }


    //public void PlayFootsteps(bool isSprinting)
    //{
    //    if (isSprinting)
    //    {
    //        if (!runningFootsteps.isPlaying)
    //        {
    //            walkingFootsteps.Stop();
    //            runningFootsteps.Play();
    //        }
    //    }
    //    else
    //    {
    //        if (!walkingFootsteps.isPlaying)
    //        {
    //            runningFootsteps.Stop();
    //            walkingFootsteps.Play();
    //        }
    //    }
    //}

    //public void StopFootsteps()
    //{
    //    walkingFootsteps.Stop();
    //    runningFootsteps.Stop();
    //}

    public void PlaytInteraction()
    {
        interaction.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        interaction.Play();
    }

    // ====================== DO NOT TOUCH ======================
    // ==========================================================
    // ======== ONLY REFERENCE THE CODE ,DON'T MODIFY ===========
    //
    // EDIT NOTE: One line changed below (Physics.Raycast -> Physics.SphereCast)
    // plus one new tunable field (detectionSphereRadius), to fix items only
    // being pickup-able/highlightable from certain angles. Everything else -
    // outline logic, door logic, keycard check - is untouched.

    private GameObject currentHighlightedItem;
    private GameObject currentHighlightedDoor;

    public float pickUpRange = 3f;

    [Tooltip("Radius of the SphereCast used for item/door detection. A plain " +
             "Raycast only registers a hit if it threads exactly through the " +
             "collider's geometry, which for thin/rotated/irregular objects " +
             "only works from certain angles. A small sphere is far more " +
             "forgiving. Start small (0.1-0.3) and increase if detection " +
             "still feels too finicky.")]
    public float detectionSphereRadius = 0.4f;

    void Update()
    {
        PerformContinuousDetection();

        if (Input.GetKeyDown(KeyCode.H) && !PauseMenu.isGamePause)
        {
            ControlsPanel.SetActive(!ControlsPanel.activeSelf);
        }
    }


    private void PerformContinuousDetection()
    {
        RaycastHit hit;
        // We cast using the maximum of the two ranges so we don't miss anything

        Debug.DrawRay(cam.transform.position, cam.transform.forward * pickUpRange, Color.red);

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, pickUpRange))
        {
            GameObject hitObject = hit.transform.gameObject;
            float distance = hit.distance;

            // --- Handle KeyPad ---
            //if (hitObject.CompareTag("KeyPad"))
            //{
            //    if (hitObject != currentHighlightedDoor)
            //    {
            //        ClearDoorHighlight();
            //        currentHighlightedItem = hitObject;
            //        ApplyDoorHighlight(currentHighlightedDoor);
            //    }

            //    Debug.Log("hitObject found = " + hitObject.name);
            //    //turn on the Qte Text on top of keypad

            //    if (Input.GetKeyDown(KeyCode.E))
            //    {
            //        //keypad interactable for the garage door QTE (Tadi)
            //        KeypadDoorInteractable keypadInteractable = hitObject.GetComponent<KeypadDoorInteractable>();

            //        if (keypadInteractable != null)
            //        {
            //            keypadInteractable.Interact();
            //        }
            //    }
            //}

            // --- Handle Doors ---
            if (hitObject.CompareTag("Door"))
            {
                if (hitObject != currentHighlightedDoor)
                {
                    ClearDoorHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyDoorHighlight(currentHighlightedDoor);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    hitObject.GetComponent<doorMovement>().ToggleDoor();
                }
            }
            else if (hitObject.CompareTag("Door_Keycard"))
            {
                // CHeck if the player has the keycard, else show the message that
                // says the Keycard is required.
                if (hitObject != currentHighlightedDoor)
                {
                    ClearDoorHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyDoorHighlight(currentHighlightedDoor);
                }


                if (Input.GetKeyDown(KeyCode.E))
                {

                    // Need to loop through the slot and look for an item with the
                    // name Keycard in order to open the door

                    bool hasKeycard = false;

                    foreach (Slot item in hotbarSlots)
                    {
                        if (item != null && item.heldItem != null && item.heldItem.itemName == "Keycard")
                        {
                            hasKeycard = true;
                            break;
                        }
                    }

                    if (hasKeycard)
                    {
                        hitObject.GetComponent<doorMovement>().ToggleKeycardDoor();
                        hitObject.GetComponent<doorMovement>().RemoveKeycardRequirement();

                        RemoveKeycardFromSlots();

                        Debug.Log("KEYCARD found — opening door");
                    }
                    else
                    {
                        hitObject.GetComponent<doorMovement>().showErrorMessage();
                    }

                }
            }


            // MOVE TADI'S KEYPAD CHECK HERE AND REMOVE GARAGE DOOR HIGHLIGHTING
            else if (hitObject.CompareTag("GarageDoor"))
            {
                // CHeck if the player has the keycard, else show the message that
                // says the Keycard is required.
                if (hitObject != currentHighlightedDoor)
                {
                    ClearDoorHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyDoorHighlight(currentHighlightedDoor);
                }


                if (Input.GetKeyDown(KeyCode.E))
                {
                    hitObject.GetComponent<doorMovement>().ToggleGarageDoor();

                    Debug.Log("Openning Garage Door!");
                }
            }
            else if (hitObject.CompareTag("KeyPad"))
            {
                if (hitObject != currentHighlightedDoor)
                {
                    ClearItemHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyItemHighlight(currentHighlightedDoor);
                }
                //turn on the Qte Text on top of keypad

                if (Input.GetKeyDown(KeyCode.E))
                {
                    //keypad interactable for the garage door QTE (Tadi)
                    KeypadDoorInteractable keypadInteractable = hitObject.GetComponent<KeypadDoorInteractable>();

                    if (keypadInteractable != null)
                    {
                        keypadInteractable.Interact();
                    }
                }
            }
            else if (currentHighlightedDoor != null) // If out of range or not hitting
            {
                ClearDoorHighlight();
            }
        }
        else
        {
            ClearItemHighlight();
            ClearDoorHighlight();
        }
    }

    private void RemoveKeycardFromSlots()
    {
        foreach (Slot item in hotbarSlots)
        {
            if (item != null && item.heldItem != null && item.heldItem.itemName == "Keycard")
            {
                item.heldItem = null; // THABO VERIFY WHETHER OR NOT THIS IS THE RIGHT WAY TO REMOVE SOMETHING FROM A SLOT
                break;
            }
        }
    }

    private void ApplyItemHighlight(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            //outline.OutlineColor = Color.white;
            outline.enabled = true;
        }
    }

    private void ClearItemHighlight()
    {
        if (currentHighlightedItem != null)
        {
            var outline = currentHighlightedItem.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            currentHighlightedItem = null;
        }
    }

    private void ApplyDoorHighlight(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            //outline.OutlineColor = Color.yellow;
            outline.enabled = true;
        }
    }

    private void ClearDoorHighlight()
    {
        if (currentHighlightedDoor != null)
        {
            var outline = currentHighlightedDoor.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            currentHighlightedDoor = null;
        }
    }
}