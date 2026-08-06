using UnityEngine;
using UnityEngine.Audio;
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
    public GameObject[] keycards;

    void Start()
    {
        cam = Camera.main;

        PlayerHealth = MaxHealth;

        HealthBarSlider.maxValue = MaxHealth;

        HealthBarSlider.value = MaxHealth;

        fill.color = gradient.Evaluate(1f);

        footstepAudioScource = GetComponents<AudioSource>()[1];
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

    private GameObject currentHighlightedItem;
    private GameObject currentHighlightedDoor;

    public float pickUpRange = 2f;

    void Update()
    {
        PerformContinuousDetection();
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

            // --- Handle Items ---
            if (hitObject.CompareTag("canPickUp"))
            {
                if (hitObject != currentHighlightedItem)
                {
                    ClearItemHighlight();
                    currentHighlightedItem = hitObject;
                    ApplyItemHighlight(currentHighlightedItem);
                }


                if(hitObject.name == "Keycard")
                {
                    Debug.Log(hitObject.name);

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
            } else if (hitObject.CompareTag("Door_Keycard"))
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
                    //hitObject.GetComponent<doorMovement>().ToggleKeycardDoor();
                    Debug.Log("KEYCARD IS REQUIRED");
                }
            }else if (hitObject.CompareTag("GarageDoor"))
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
            }else if (currentHighlightedDoor != null) // If out of range or not hitting
                {
                    ClearDoorHighlight();
                }
            }else
        {
            ClearItemHighlight();
            ClearDoorHighlight();
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
