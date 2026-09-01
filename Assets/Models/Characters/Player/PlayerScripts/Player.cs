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


    private Camera cam;
    public List<Slot> hotbarSlots;

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
    public float detectionSphereRadius = 0.2f;

    void Update()
    {
        PerformContinuousDetection();
    }


    private void PerformContinuousDetection()
    {
        RaycastHit hit;
        // We cast using the maximum of the two ranges so we don't miss anything

        Debug.DrawRay(cam.transform.position, cam.transform.forward * pickUpRange, Color.red);

        if (Physics.SphereCast(cam.transform.position, detectionSphereRadius, cam.transform.forward, out hit, pickUpRange))
        {
            GameObject hitObject = hit.transform.gameObject;
            float distance = hit.distance;

            // --- Handle Items ---
            if (hitObject.CompareTag("canPickUp"))
            {
                if (hitObject != currentHighlightedItem)
                {
                    ClearItemHighlight();
                    currentHighlightedItem = hitObject;
                    ApplyItemHighlight(currentHighlightedItem);
                }


                if (hitObject.name == "Keycard")
                {
                    //Debug.Log(hitObject.name);

                    ClearItemHighlight();
                    currentHighlightedItem = hitObject;
                    ApplyItemHighlight(currentHighlightedItem);

                }
            }
            else if (currentHighlightedItem != null) // If out of range or not hitting
            {
                ClearItemHighlight();
            }

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