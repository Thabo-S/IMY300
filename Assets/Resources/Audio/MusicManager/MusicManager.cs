using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class SceneTrack
    {
        public string sceneName;
        public AudioClip clip;
    }

    [Header("Music per Scene")]
    [SerializeField] private List<SceneTrack> sceneTracks = new List<SceneTrack>();

    [Header("Fallback Playlist (used if a scene has no assigned track)")]
    [SerializeField] private List<AudioClip> musicClips = new List<AudioClip>();
    [SerializeField] private bool shufflePlaylist = false;
    [SerializeField] private bool loopPlaylist = true;

    [Header("Playback Settings")]
    [SerializeField][Range(0f, 1f)] private float volume = 0.2f;
    [SerializeField] private float fadeDuration = 1.5f;

    private AudioSource audioSource;
    private List<int> playOrder = new List<int>();
    private int currentIndex = -1;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        BuildPlayOrder();
        HandleSceneMusic(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneMusic(scene.name);
    }

    private void HandleSceneMusic(string sceneName)
    {
        foreach (var track in sceneTracks)
        {
            if (track.sceneName == sceneName && track.clip != null)
            {
                currentIndex = -1; // stop playlist auto-advance while a dedicated track plays
                PlayClip(track.clip);
                return;
            }
        }

        // No dedicated track for this scene — fall back to playlist
        if (musicClips.Count > 0)
        {
            PlayNext();
        }
    }

    private void Update()
    {
        if (audioSource.clip != null && !audioSource.isPlaying && currentIndex != -1 && fadeCoroutine == null)
        {
            PlayNext();
        }
    }

    private void BuildPlayOrder()
    {
        playOrder.Clear();
        for (int i = 0; i < musicClips.Count; i++)
            playOrder.Add(i);

        if (shufflePlaylist)
        {
            for (int i = playOrder.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (playOrder[i], playOrder[j]) = (playOrder[j], playOrder[i]);
            }
        }
    }

    public void PlayNext()
    {
        if (musicClips.Count == 0) return;

        currentIndex++;

        if (currentIndex >= playOrder.Count)
        {
            if (!loopPlaylist)
            {
                currentIndex = -1;
                return;
            }
            BuildPlayOrder();
            currentIndex = 0;
        }

        PlayClip(musicClips[playOrder[currentIndex]]);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource.clip == clip && audioSource.isPlaying) return; // already playing this one

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CrossfadeToClip(clip));
    }

    private IEnumerator CrossfadeToClip(AudioClip newClip)
    {
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            audioSource.Stop();
        }

        audioSource.clip = newClip;
        audioSource.Play();

        float fadeInT = 0f;
        while (fadeInT < fadeDuration)
        {
            fadeInT += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, fadeInT / fadeDuration);
            yield return null;
        }
        audioSource.volume = volume;

        fadeCoroutine = null;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (fadeCoroutine == null)
            audioSource.volume = volume;
    }

    public float GetVolume() => volume;

    public void Stop()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        audioSource.Stop();
        currentIndex = -1;
    }
}