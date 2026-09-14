using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public TextMeshProUGUI interactionTextUI;

    [Header("Health Variables")]
    public Slider HealthBarSlider;
    public float MaxHealth = 100;
    public float PlayerHealth;
    public Gradient gradient;
    public Image fill;
    public GameObject deathUI;

    [Header("Stamina Variables")]
    public Slider staminaBarSlider;
    public float MaxStamina = 100;
    public float PlayerStamina;
    public Image staminaFill;

    [Header("Stamina Settings")]
    [Tooltip("Stamina drained per second while sprinting normally (100 / this = seconds to empty).")]
    public float staminaDrainRate = 20f; // ~5s to empty
    [Tooltip("Multiplier applied to drain rate while ANY guard currently sees the player.")]
    public float detectedDrainMultiplier = 1.75f;
    [Tooltip("Stamina regained per second while not sprinting.")]
    public float staminaRegenRate = 12.5f; // ~8s to refill from empty
    [Tooltip("Delay after releasing sprint before regen starts.")]
    public float regenDelay = 1f;
    [Tooltip("Speed multiplier applied when stamina is empty (0.7 = 70% of normal move speed, not a hard walk-lock).")]
    public float emptyStaminaSpeedMultiplier = 0.7f;
    [Tooltip("Extra stamina lost per hit taken - punishes getting shot while fleeing.")]
    public float damageStaminaPenalty = 15f;

    private float regenTimer = 0f;
    public bool IsSprinting { get; private set; }
    public float SpeedMultiplier { get; private set; } = 1f;

    [Header("Footstep Audio")]
    public AudioSource footstepAudioScource;
    public AudioClip footstepClip;

    [Header("Interaction Sound")]
    public AudioSource interaction;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Damage Audio")]
    public AudioSource damageAudioSource;
    private AudioClip damageClip;
    private AudioClip deathClip;

    [Header("Toggle Controls")]
    [SerializeField] private GameObject ControlsPanel;

    [Header("Damage Overlay")]
    public Image damageOverlay;
    public float flashInAlpha = 0.5f;
    public float flashInTime = 1f;
    public float fadeOutTime = 1f;

    private Coroutine damageFlashCoroutine;

    private PlayerMovement playerMovement;

    private Camera cam;
    public List<Slot> hotbarSlots;

    private GameObject currentHighlightedItem;
    private GameObject currentHighlightedDoor;

    public float pickUpRange = 3f;

    [Tooltip("Radius of the SphereCast used for item/door detection.")]
    public float detectionSphereRadius = 0.4f;


    //================= List Of PlayerPrefs ===================
    // LevelIndex : Use to determine game level
    // currentLevelPrefKey : Use for the currentlevel being played
    //=========================================================

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = Camera.main;

        PlayerHealth = MaxHealth;

        HealthBarSlider.maxValue = MaxHealth;

        HealthBarSlider.value = MaxHealth;

        fill.color = gradient.Evaluate(1f);

        PlayerStamina = MaxStamina;

        if (staminaBarSlider != null)
        {
            staminaBarSlider.maxValue = MaxStamina;
            staminaBarSlider.value = MaxStamina;
        }
        AudioSource[] audioSources = GetComponents<AudioSource>();

        if (audioSources.Length >= 3)
        {
            footstepAudioScource = audioSources[0];
            interaction = audioSources[1];
            damageAudioSource = audioSources[2];
        }
        else
        {
            Debug.LogError($"[Player] Expected 3 AudioSource components (footstep, interaction, damage) " +
                            $"but found {audioSources.Length}. Add AudioSource components until there are 3, " +
                            $"in that order, in the Inspector.");
        }

        hotbarSlots = FindAnyObjectByType<Inventory>().hotbarSlots;

        playerMovement = GetComponent<PlayerMovement>();

        damageClip = Resources.Load<AudioClip>("Audio/SFX/PlayerAudio/damage_grunt_male");

        deathClip = Resources.Load<AudioClip>("Audio/SFX/PlayerAudio/death_groan_male");
    }
    public void TakeDamage(int damage)
    {
        PlayerHealth -= damage;

        HealthBarSlider.value = PlayerHealth;

        fill.color = gradient.Evaluate(HealthBarSlider.normalizedValue);

        Debug.Log("Player took damage: " + PlayerHealth);

        AudioClip clipToPlay = (PlayerHealth > 0) ? damageClip : deathClip;

        if (damageAudioSource != null && clipToPlay != null)
        {
            damageAudioSource.PlayOneShot(clipToPlay);
        }

        PlayerStamina = Mathf.Clamp(PlayerStamina - damageStaminaPenalty, 0f, MaxStamina);
        UpdateStaminaUI();

        if (damageOverlay != null)
        {
            if (damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);

            damageFlashCoroutine = StartCoroutine(DamageFlash());
        }

        if (PlayerPrefs.GetInt("currentLevelPrefKey") == 0 && PlayerHealth < 60)
        {
            TutorialManager tutorial = Object.FindAnyObjectByType<TutorialManager>();

            if (tutorial != null)
                tutorial.StartStep5();
        }

        if (PlayerHealth <= 0)
        {
            deathUI.SetActive(true);

            if (playerMovement != null) playerMovement.CalculatePlayerMovement(Vector2.zero);

            if (CursorManager.instance != null) CursorManager.instance.UnlockCursor();

            GameObject[] allGuards = GameObject.FindGameObjectsWithTag("Guard");

            foreach (GameObject guard in allGuards)
            {
                if (guard != null)
                {
                    guard.SetActive(false);
                }
            }
        }
    }

    public void RecoupHealth(int increaseHealtj)
    {
        PlayerHealth += increaseHealtj;

        if (PlayerHealth > 100)
            PlayerHealth = 100;

        HealthBarSlider.value = PlayerHealth;

        fill.color = gradient.Evaluate(HealthBarSlider.normalizedValue);

        Debug.Log("Player health increased: " + PlayerHealth);

        if (PlayerPrefs.GetInt("currentLevelPrefKey", 0) == 0)
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

    public void PlaytInteraction()
    {
        interaction.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        interaction.Play();
    }

    // ---------------- STAMINA ----------------

    /// <summary>
    /// Checked every frame - true if ANY guard currently has direct sight
    /// of the player, regardless of state (Patrol spotting, Attack, etc.).
    /// </summary>
    private bool IsSeenByAnyGuard()
    {
        foreach (Guard g in Guard.AllGuards)
        {
            if (g != null && g.CanSeePlayer()) return true;
        }
        return false;
    }

    private void UpdateStamina()
    {
        bool sprintKeyHeld = Input.GetKey(KeyCode.LeftShift);
        bool detected = IsSeenByAnyGuard();

        bool currentlyCrouching = playerMovement != null && playerMovement.isCrouching;

        if (sprintKeyHeld && !currentlyCrouching && PlayerStamina > 0f)
        {
            IsSprinting = true;

            float drain = staminaDrainRate * (detected ? detectedDrainMultiplier : 1f);
            PlayerStamina = Mathf.Clamp(PlayerStamina - drain * Time.deltaTime, 0f, MaxStamina);

            regenTimer = regenDelay; // reset regen delay while actively sprinting
        }
        else
        {
            IsSprinting = false;

            if (regenTimer > 0f)
            {
                regenTimer -= Time.deltaTime;
            }
            else
            {
                PlayerStamina = Mathf.Clamp(PlayerStamina + staminaRegenRate * Time.deltaTime, 0f, MaxStamina);
            }
        }

        SpeedMultiplier = PlayerStamina <= 0f ? emptyStaminaSpeedMultiplier : 1f;

        UpdateStaminaUI();
    }
    private void UpdateStaminaUI()
    {
        if (staminaBarSlider != null)
            staminaBarSlider.value = PlayerStamina;

        if (staminaFill != null)
            staminaFill.fillAmount = PlayerStamina / MaxStamina;
    }

    // ====================== DO NOT TOUCH ======================
    // ==========================================================
    // ======== ONLY REFERENCE THE CODE ,DON'T MODIFY ===========

    void Update()
    {
        PerformContinuousDetection();
        UpdateStamina();

        if (Input.GetKeyDown(KeyCode.H) && !PauseMenu.isGamePause)
        {
            ControlsPanel.SetActive(!ControlsPanel.activeSelf);
        }
    }

    private void PerformContinuousDetection()
    {
        RaycastHit hit;

        Debug.DrawRay(cam.transform.position, cam.transform.forward * pickUpRange, Color.red);

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, pickUpRange))
        {
            GameObject hitObject = hit.transform.gameObject;
            float distance = hit.distance;

            if (hitObject.CompareTag("Door"))
            {
                if (hitObject != currentHighlightedDoor)
                {
                    interactionTextUI.text = "[E] Open Door";
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
                if (hitObject != currentHighlightedDoor)
                {
                    interactionTextUI.text = "[E] Use Keycard";
                    ClearDoorHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyDoorHighlight(currentHighlightedDoor);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
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
            else if (hitObject.CompareTag("KeyPad"))
            {
                if (hitObject != currentHighlightedDoor)
                {
                    interactionTextUI.text = "[E] Hack Keypad";
                    ClearItemHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyItemHighlight(currentHighlightedDoor);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    KeypadDoorInteractable keypadInteractable = hitObject.GetComponent<KeypadDoorInteractable>();

                    if (keypadInteractable != null)
                    {
                        keypadInteractable.Interact();
                    }
                }
            }
            else if (hitObject.CompareTag("LockedDoor"))
            {
                if (hitObject != currentHighlightedDoor)
                {
                    interactionTextUI.text = "[E] Pick Lock";
                    ClearItemHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyItemHighlight(currentHighlightedDoor);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    LockDoorQte doorQte = hitObject.GetComponent<LockDoorQte>();

                    if (doorQte != null)
                    {
                        doorQte.StartQte();
                        Debug.Log("[Player] Starting lock QTE.");
                    }
                }
            }
            else if (hitObject.CompareTag("Lever"))
            {
                if (hitObject != currentHighlightedDoor)
                {
                    interactionTextUI.text = "[E] Disable Security";
                    ClearItemHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyItemHighlight(currentHighlightedDoor);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    hitObject.GetComponent<laser_security_switch>().ToggleSwitch();
                }
            }
            else if (hitObject.GetComponentInParent<Item>() != null)
            {
                Item item = hitObject.GetComponentInParent<Item>();

                if (hitObject != currentHighlightedDoor)
                {
                    ClearDoorHighlight();
                    currentHighlightedDoor = hitObject;
                    ApplyDoorHighlight(currentHighlightedDoor);
                }

                interactionTextUI.text = "[E] Pick up " + item.name;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Inventory.instance.TryPickupItem();
                }
            }
            else if (currentHighlightedDoor != null)
            {
                interactionTextUI.text = "";
                ClearDoorHighlight();
            }
        }
        else
        {
            interactionTextUI.text = "";
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
                item.RemoveAmount(1);

                GameObject inventory = GameObject.FindGameObjectWithTag("Inventory");

                inventory.GetComponent<Inventory>().EquipHandItem();

                break;
            }
        }
    }

    private void ApplyItemHighlight(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
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