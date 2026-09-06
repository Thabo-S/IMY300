//using System.Collections.Generic;
//using System.Collections;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.UI;

//public class KepPadQTE : MonoBehaviour
//{
//    [Header("Key Pool")]
//    [Tooltip("Every letter that can appear in the generated sequence.")]
//    public string keyPool = "tyuiopghjklzxcvbnm";

//    [System.Serializable]
//    public struct KeySprite
//    {
//        public string key;
//        public Sprite sprite;
//    }

//    [Header("Key Sprites")]
//    [Tooltip("Assign a sprite for every letter in the key pool (used to populate the slots).")]
//    public List<KeySprite> keySprites;

//    [Header("Slots (in order, e.g. Slot_1..Slot_4)")]
//    [SerializeField] private Image[] slots;
//    [Header("Hacking Bar (plays before the keypad QTE)")]
//    [Tooltip("The GameObject holding the Slider - shown while 'hacking', hidden once the keypad QTE starts.")]
//    [SerializeField] private GameObject hackingBarObject;
//    [SerializeField] private Slider hackingSlider;
//    [Tooltip("How long the hacking bar takes to fill, in seconds.")]
//    [SerializeField] private float hackDuration = 3f;
//    private bool acceptingKeypadInput;
//    private Coroutine hackingCoroutine;

//    [Header("Keypad UI (hidden during hacking, shown once it's done)")]
//    [Tooltip("The 'Background' panel behind the slots.")]
//    [SerializeField] private GameObject backgroundObject;


//    [Header("Sequence Settings")]
//    [SerializeField] private int sequenceLength = 4;
//    [SerializeField] private bool allowRepeatKeys = false;
//    [SerializeField] private Color pendingColor = Color.white;
//    [SerializeField] private Color incorrectColour = Color.red;
//    [SerializeField] private Color completedColor = Color.gray;

//    [Header("Events")]
//    public UnityEvent OnQteSuccess;
//    public UnityEvent OnQteFail;
//    public UnityEvent OnQteCancel;

//    private Dictionary<char, Sprite> spriteLookup;
//    private char[] sequence;
//    private int currentIndex;

//    private void Awake()
//    {
//        BuildSpriteLookup();
//    }

//    private void OnEnable()
//    {
//        acceptingKeypadInput = false;

//        SetKeypadUIActive(false);
//        if (hackingBarObject != null) hackingBarObject.SetActive(true);
//        if (hackingSlider != null) hackingSlider.value = 0f;

//        if (hackingCoroutine != null) StopCoroutine(hackingCoroutine);
//        hackingCoroutine = StartCoroutine(PlayHackingBar());
//    }

//    private void OnDisable()
//    {
//        if (hackingCoroutine != null)
//        {
//            StopCoroutine(hackingCoroutine);
//            hackingCoroutine = null;
//        }
//    }

//    private IEnumerator PlayHackingBar()
//    {
//        float elapsed = 0f;

//        while (elapsed < hackDuration)
//        {
//            // Let the player back out while the bar is still filling.
//            if (Input.GetKeyDown(KeyCode.C))
//            {
//                CancelQte();
//                yield break;
//            }

//            elapsed += Time.deltaTime;
//            if (hackingSlider != null)
//                hackingSlider.value = Mathf.Clamp01(elapsed / hackDuration);

//            yield return null;
//        }

//        if (hackingSlider != null)
//            hackingSlider.value = 1f;

//        // Hacking done - swap to the keypad UI and start the real QTE.
//        if (hackingBarObject != null) hackingBarObject.SetActive(false);
//        SetKeypadUIActive(true);

//        GenerateSequence();
//        currentIndex = 0;
//        acceptingKeypadInput = true;
//        hackingCoroutine = null;
//    }

//    private void SetKeypadUIActive(bool active)
//    {
//        if (backgroundObject != null)
//            backgroundObject.SetActive(active);

//        if (slots == null) return;
//        foreach (var slot in slots)
//        {
//            if (slot != null)
//                slot.gameObject.SetActive(active);
//        }
//    }

//    private void BuildSpriteLookup()
//    {
//        spriteLookup = new Dictionary<char, Sprite>();
//        foreach (var ks in keySprites)
//        {
//            if (string.IsNullOrEmpty(ks.key)) continue;

//            char c = char.ToLower(ks.key[0]);
//            if (!spriteLookup.ContainsKey(c))
//                spriteLookup.Add(c, ks.sprite);
//        }
//    }

//    private void GenerateSequence()
//    {
//        sequence = new char[sequenceLength];
//        List<char> pool = keyPool.ToLower().ToList();

//        for (int i = 0; i < sequenceLength; i++)
//        {
//            char chosen;
//            if (allowRepeatKeys || pool.Count == 0)
//            {
//                chosen = char.ToLower(keyPool[Random.Range(0, keyPool.Length)]);
//            }
//            else
//            {
//                int idx = Random.Range(0, pool.Count);
//                chosen = pool[idx];
//                pool.RemoveAt(idx);
//            }
//            sequence[i] = chosen;
//        }

//        for (int i = 0; i < slots.Length && i < sequence.Length; i++)
//        {
//            if (spriteLookup.TryGetValue(sequence[i], out Sprite spr))
//            {
//                slots[i].sprite = spr;
//                slots[i].color = pendingColor;
//            }
//            else
//            {
//                Debug.LogWarning($"{name}: No sprite assigned for key '{sequence[i]}' in keySprites.");
//            }
//        }
//    }

//    private void Update()
//    {
//        if (sequence == null || !acceptingKeypadInput) return;

//        if (Input.GetKeyDown(KeyCode.C))
//        {
//            CancelQte();
//            return;
//        }

//        foreach (char c in keyPool)
//        {
//            if (Input.GetKeyDown(CharToKeyCode(c)))
//            {
//                HandleKeyPress(c);
//                break;
//            }
//        }
//    }

//    private KeyCode CharToKeyCode(char c)
//    {
//        return (KeyCode)System.Enum.Parse(typeof(KeyCode), c.ToString().ToUpper());
//    }

//    private void HandleKeyPress(char pressed)
//    {
//        if (pressed == sequence[currentIndex])
//        {
//            slots[currentIndex].color = completedColor;
//            currentIndex++;

//            if (currentIndex >= sequence.Length)
//            {
//                Debug.Log("Keypad QTE success!");
//                OnQteSuccess?.Invoke();
//            }
//        }
//        else
//        {
//            slots[currentIndex].color = incorrectColour;

//            Debug.Log("Keypad QTE failed - wrong key.");

//            if(currentIndex < sequence.Length)
//            {
//                OnQteFail?.Invoke();
//            }
//        }
//    }

//    private void CancelQte()
//    {
//        Debug.Log("Keypad QTE cancelled by player.");
//        OnQteCancel?.Invoke();
//    }
//}
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KepPadQTE : MonoBehaviour
{
    [Header("Key Pool")]
    [Tooltip("Every letter that can appear in the generated sequence.")]
    public string keyPool = "tyuiopghjklzxcvbnm";

    [Tooltip("Path under a Resources folder where key sprites live (e.g. Assets/Resources/Keyboard keys -> 'Keyboard keys'). Sprites must be named in all caps, e.g. A.png, B.png.")]
    [SerializeField] private string keySpriteResourcePath = "Keyboard keys";

    [Header("Slots (in order, e.g. Slot_1..Slot_4)")]
    [SerializeField] private Image[] slots;
    [Header("Hacking Bar (plays before the keypad QTE)")]
    [Tooltip("The GameObject holding the Slider - shown while 'hacking', hidden once the keypad QTE starts.")]
    [SerializeField] private GameObject hackingBarObject;
    [SerializeField] private Slider hackingSlider;
    [Tooltip("How long the hacking bar takes to fill, in seconds.")]
    [SerializeField] private float hackDuration = 3f;
    private bool acceptingKeypadInput;
    private Coroutine hackingCoroutine;

    [Header("Keypad UI (hidden during hacking, shown once it's done)")]
    [Tooltip("The 'Background' panel behind the slots.")]
    [SerializeField] private GameObject backgroundObject;


    [Header("Sequence Settings")]
    [SerializeField] private int sequenceLength = 4;
    [SerializeField] private bool allowRepeatKeys = false;
    [SerializeField] private Color pendingColor = Color.white;
    [SerializeField] private Color incorrectColour = Color.red;
    [SerializeField] private Color completedColor = Color.gray;

    [Header("Events")]
    public UnityEvent OnQteSuccess;
    public UnityEvent OnQteFail;
    public UnityEvent OnQteCancel;

    private Dictionary<char, Sprite> spriteLookup;
    private char[] sequence;
    private int currentIndex;

    private void Awake()
    {
        BuildSpriteLookup();
    }

    private void OnEnable()
    {
        acceptingKeypadInput = false;

        SetKeypadUIActive(false);
        if (hackingBarObject != null) hackingBarObject.SetActive(true);
        if (hackingSlider != null) hackingSlider.value = 0f;

        if (hackingCoroutine != null) StopCoroutine(hackingCoroutine);
        hackingCoroutine = StartCoroutine(PlayHackingBar());
    }

    private void OnDisable()
    {
        if (hackingCoroutine != null)
        {
            StopCoroutine(hackingCoroutine);
            hackingCoroutine = null;
        }
    }

    private IEnumerator PlayHackingBar()
    {
        float elapsed = 0f;

        while (elapsed < hackDuration)
        {
            // Let the player back out while the bar is still filling.
            if (Input.GetKeyDown(KeyCode.C))
            {
                CancelQte();
                yield break;
            }

            elapsed += Time.deltaTime;
            if (hackingSlider != null)
                hackingSlider.value = Mathf.Clamp01(elapsed / hackDuration);

            yield return null;
        }

        if (hackingSlider != null)
            hackingSlider.value = 1f;

        // Hacking done - swap to the keypad UI and start the real QTE.
        if (hackingBarObject != null) hackingBarObject.SetActive(false);
        SetKeypadUIActive(true);

        GenerateSequence();
        currentIndex = 0;
        acceptingKeypadInput = true;
        hackingCoroutine = null;
    }

    private void SetKeypadUIActive(bool active)
    {
        if (backgroundObject != null)
            backgroundObject.SetActive(active);

        if (slots == null) return;
        foreach (var slot in slots)
        {
            if (slot != null)
                slot.gameObject.SetActive(active);
        }
    }

    private void BuildSpriteLookup()
    {
        spriteLookup = new Dictionary<char, Sprite>();

        foreach (char c in keyPool)
        {
            char lower = char.ToLower(c);
            if (spriteLookup.ContainsKey(lower)) continue;

            string spriteName = lower.ToString().ToUpper(); // A, B, C, ...
            string fullPath = $"{keySpriteResourcePath}/{spriteName}";

            Sprite spr = Resources.Load<Sprite>(fullPath);

            if (spr != null)
            {
                spriteLookup.Add(lower, spr);
            }
            else
            {
                Debug.LogWarning($"{name}: No sprite found at Resources/{fullPath}. Make sure {spriteName}.png exists in Assets/Resources/{keySpriteResourcePath}.");
            }
        }
    }

    private void GenerateSequence()
    {
        sequence = new char[sequenceLength];
        List<char> pool = keyPool.ToLower().ToList();

        for (int i = 0; i < sequenceLength; i++)
        {
            char chosen;
            if (allowRepeatKeys || pool.Count == 0)
            {
                chosen = char.ToLower(keyPool[Random.Range(0, keyPool.Length)]);
            }
            else
            {
                int idx = Random.Range(0, pool.Count);
                chosen = pool[idx];
                pool.RemoveAt(idx);
            }
            sequence[i] = chosen;
        }

        for (int i = 0; i < slots.Length && i < sequence.Length; i++)
        {
            if (spriteLookup.TryGetValue(sequence[i], out Sprite spr))
            {
                slots[i].sprite = spr;
                slots[i].color = pendingColor;
            }
            else
            {
                Debug.LogWarning($"{name}: No sprite assigned for key '{sequence[i]}' in keySprites.");
            }
        }
    }

    private void Update()
    {
        if (sequence == null || !acceptingKeypadInput) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            CancelQte();
            return;
        }

        foreach (char c in keyPool)
        {
            if (Input.GetKeyDown(CharToKeyCode(c)))
            {
                HandleKeyPress(c);
                break;
            }
        }
    }

    private KeyCode CharToKeyCode(char c)
    {
        return (KeyCode)System.Enum.Parse(typeof(KeyCode), c.ToString().ToUpper());
    }

    private void HandleKeyPress(char pressed)
    {
        if (pressed == sequence[currentIndex])
        {
            slots[currentIndex].color = completedColor;
            currentIndex++;

            if (currentIndex >= sequence.Length)
            {
                Debug.Log("Keypad QTE success!");
                OnQteSuccess?.Invoke();
            }
        }
        else
        {
            slots[currentIndex].color = incorrectColour;

            Debug.Log("Keypad QTE failed - wrong key.");

            if (currentIndex < sequence.Length)
            {
                OnQteFail?.Invoke();
            }
        }
    }

    private void CancelQte()
    {
        Debug.Log("Keypad QTE cancelled by player.");
        OnQteCancel?.Invoke();
    }
}