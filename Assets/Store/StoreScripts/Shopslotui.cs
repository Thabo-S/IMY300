using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descriptionText;
    public Button buyButton;
    [Tooltip("Optional - shown instead of/alongside the buy button once owned.")]
    public GameObject ownedIndicator;

    [Header("Info Toggle")]
    public Button infoButton;
    public GameObject normalViewRoot;
    public GameObject descriptionViewRoot;

    private ItemSO item;
    private Func<ItemSO, bool> onPurchaseAttempt;
    private bool showingDescription = false;

    [Header("Highlight Settings")]
    public GameObject selectionBar;
    public Image highlightTarget;
    public Material highlightMaterial;

    [Header("Purchase Feedback - Visual")]
    public Color failFlashColor = Color.red;
    public float flashDuration = 0.4f;

    [Header("Purchase Feedback - Message")]
    [Tooltip("A small TMP text (e.g. below the Buy button) that shows WHY a " +
             "purchase failed, visible to the player - not just the console. " +
             "Auto-hides after Message Duration. Leave unassigned to skip.")]
    public TextMeshProUGUI feedbackText;
    public float messageDuration = 1.5f;

    [Header("Purchase Feedback - Audio")]
    [Tooltip("Auto-adds an AudioSource if none is found on this GameObject.")]
    public AudioSource audioSource;
    public AudioClip buySuccessClip;
    public AudioClip buyFailClip;

    private Material defaultMaterial;
    private Color buyButtonDefaultColor;
    private Coroutine flashRoutine;
    private Coroutine messageRoutine;

    private bool isHovering = false;
    private bool isSelected = false;

    private static ShopSlotUI currentlySelected;

    private void Awake()
    {
        if (highlightTarget != null)
        {
            defaultMaterial = highlightTarget.material;
        }

        if (icon == null) icon = FindComponentInChildByName<Image>("Icon");
        if (nameText == null) nameText = FindComponentInChildByName<TextMeshProUGUI>("ItemName");
        if (priceText == null) priceText = FindComponentInChildByName<TextMeshProUGUI>("Price");
        if (buyButton == null) buyButton = FindComponentInChildByName<Button>("BuyBTN");
        if (ownedIndicator == null) ownedIndicator = FindChildByName("Owned");
        if (infoButton == null) infoButton = FindComponentInChildByName<Button>("DetailsBTN");
        if (normalViewRoot == null) normalViewRoot = FindChildByName("NormalView");
        if (descriptionViewRoot == null) descriptionViewRoot = FindChildByName("DetailsView");
        if (selectionBar == null) selectionBar = FindChildByName("SelectionBar");
        if (feedbackText == null) feedbackText = FindComponentInChildByName<TextMeshProUGUI>("FeedbackText");

        if (descriptionText == null && descriptionViewRoot != null)
        {
            descriptionText = descriptionViewRoot.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (buyButton != null && buyButton.image != null)
        {
            buyButtonDefaultColor = buyButton.image.color;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        RefreshSelectionBar();
        LogMissingReferences();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        RefreshSelectionBar();

        if (highlightTarget != null && highlightMaterial != null)
        {
            highlightTarget.material = highlightMaterial;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        RefreshSelectionBar();

        if (highlightTarget != null)
        {
            highlightTarget.material = defaultMaterial;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetSelected(true);
    }

    private void SetSelected(bool selected)
    {
        if (selected)
        {
            if (currentlySelected != null && currentlySelected != this)
            {
                currentlySelected.SetSelected(false);
            }
            currentlySelected = this;
        }
        else if (currentlySelected == this)
        {
            currentlySelected = null;
        }

        isSelected = selected;
        RefreshSelectionBar();
    }

    private void RefreshSelectionBar()
    {
        if (selectionBar != null)
        {
            selectionBar.SetActive(isHovering || isSelected);
        }
    }

    private GameObject FindChildByName(string childName)
    {
        Transform result = FindTransformByName(transform, childName);
        return result != null ? result.gameObject : null;
    }

    private T FindComponentInChildByName<T>(string childName) where T : Component
    {
        Transform result = FindTransformByName(transform, childName);
        return result != null ? result.GetComponent<T>() : null;
    }

    private Transform FindTransformByName(Transform root, string childName)
    {
        foreach (Transform child in root)
        {
            if (child.name == childName) return child;

            Transform nested = FindTransformByName(child, childName);
            if (nested != null) return nested;
        }
        return null;
    }

    private void LogMissingReferences()
    {
        if (icon == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'icon' (expected child named \"Icon\").", this);
        if (nameText == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'nameText' (expected child named \"ItemName\").", this);
        if (priceText == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'priceText' (expected child named \"Price\").", this);
        if (buyButton == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'buyButton' (expected child named \"BuyBTN\").", this);
        if (infoButton == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'infoButton' (expected child named \"DetailsBTN\").", this);
        if (normalViewRoot == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'normalViewRoot' (expected child named \"NormalView\").", this);
        if (descriptionViewRoot == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'descriptionViewRoot' (expected child named \"DetailsView\").", this);
        if (descriptionText == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'descriptionText' (expected a TMP text under \"DetailsView\").", this);
        if (selectionBar == null) Debug.LogWarning($"[ShopSlotUI] '{name}': could not auto-wire 'selectionBar' (expected child named \"SelectionBar\").", this);
        // feedbackText is optional - no warning if missing, since not every
        // slot design needs an inline failure message.
    }

    public void Setup(ItemSO itemToDisplay, Func<ItemSO, bool> purchaseCallback)
    {
        item = itemToDisplay;
        onPurchaseAttempt = purchaseCallback;

        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = $"${item.price}";
        if (descriptionText != null) descriptionText.text = item.description;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        if (infoButton != null)
        {
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(ToggleDescription);
        }

        showingDescription = false;
        RefreshViewState();
        RefreshOwnedState();
    }

    public void OnBuyClicked()
    {
        if (onPurchaseAttempt == null || item == null) return;

        bool success = onPurchaseAttempt.Invoke(item);

        if (success)
        {
            buyButton.image.color = Color.green;
            RefreshOwnedState();
            PlaySound(buySuccessClip);
        }
        else
        {
            bool alreadyOwned = ToolLoadout.IsOwned(item);
            bool canAfford = CurrencyManager.GetBalance() >= item.price;

            string reason;
            if (alreadyOwned)
            {
                reason = "Already owned";
            }
            else if (!canAfford)
            {
                int shortBy = item.price - CurrencyManager.GetBalance();
                reason = $"Need ${shortBy} more";
            }
            else
            {
                reason = "Can't buy this right now";
            }

            ShowFeedbackMessage(reason);
            FlashBuyButtonRed();
            PlaySound(buyFailClip);
        }
    }

    private void ShowFeedbackMessage(string message)
    {
        if (feedbackText == null) return;

        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(message));
    }

    private IEnumerator MessageRoutine(string message)
    {
        feedbackText.text = message;
        yield return new WaitForSeconds(messageDuration);
        feedbackText.text = "";
        messageRoutine = null;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void FlashBuyButtonRed()
    {
        if (buyButton == null || buyButton.image == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        buyButton.image.color = failFlashColor;
        yield return new WaitForSeconds(flashDuration);
        buyButton.image.color = buyButtonDefaultColor;
        flashRoutine = null;
    }

    public void ToggleDescription()
    {
        showingDescription = !showingDescription;
        RefreshViewState();
    }

    private void RefreshViewState()
    {
        if (normalViewRoot != null)
            normalViewRoot.SetActive(!showingDescription);

        if (descriptionViewRoot != null)
            descriptionViewRoot.SetActive(showingDescription);
    }

    private void RefreshOwnedState()
    {
        bool owned = ToolLoadout.IsOwned(item);

        if (ownedIndicator != null) ownedIndicator.SetActive(owned);
        if (buyButton != null) buyButton.interactable = !owned;
    }
}