using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardSpeechManager : MonoBehaviour
{
    public static GuardSpeechManager Instance { get; private set; }

    [Header("Audio Resources folders")]
    private string patrolFolder = "Audio/GuardRadio/Patrol";
    private string alertFolder = "Audio/GuardRadio/Alert";
    private string attackFolder = "Audio/GuardRadio/Attack";

    [Header("Talking Timing")]
    public float minChatterInterval = 15f;
    public float maxChatterInterval = 35f;

    [Header("Volume")]
    [Range(0f, 1f)] public float chatterVolume = 0.8f;
    [Range(0f, 1f)] public float barkVolume = 1f;

    private AudioClip[] patrolClips;
    private AudioClip[] alertClips;
    private AudioClip[] attackClips;

    private Guard lastSpeaker;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        patrolClips = Resources.LoadAll<AudioClip>(patrolFolder);
        alertClips = Resources.LoadAll<AudioClip>(alertFolder);
        attackClips = Resources.LoadAll<AudioClip>(attackFolder);

        Debug.Log($"[GuardSpeech] Loaded {patrolClips.Length} patrol, {alertClips.Length} alert, {attackClips.Length} attack clips.");

        if (patrolClips.Length == 0)
            Debug.LogWarning($"[GuardSpeech] No clips found at Resources/{patrolFolder}");
        if (alertClips.Length == 0)
            Debug.LogWarning($"[GuardSpeech] No clips found at Resources/{alertFolder}");
        if (attackClips.Length == 0)
            Debug.LogWarning($"[GuardSpeech] No clips found at Resources/{attackFolder}");
    }

    private void Start()
    {
        StartCoroutine(AmbientChatterLoop());
    }

    private IEnumerator AmbientChatterLoop()
    {
        while (true)
        {
            float wait = Random.Range(minChatterInterval, maxChatterInterval);
            yield return new WaitForSeconds(wait);

            TryPlayPatrolChatter();
        }
    }

    private void TryPlayPatrolChatter()
    {
        if (patrolClips.Length == 0) return;

        List<Guard> patrollingGuards = GetGuardsInState(Guard.GuardVocalState.Patrolling);
        if (patrollingGuards.Count == 0) return;

        Guard speaker = PickRandomGuard(patrollingGuards, lastSpeaker);
        if (speaker == null) return;

        AudioClip clip = patrolClips[Random.Range(0, patrolClips.Length)];
        PlayClipOnGuard(speaker, clip, chatterVolume);
        lastSpeaker = speaker;
    }

    /// <summary>Call from AlertState.Enter().</summary>
    public void PlayAlertBark(Guard guard)
    {
        if (alertClips.Length == 0 || guard == null) return;

        AudioClip clip = alertClips[Random.Range(0, alertClips.Length)];
        PlayClipOnGuard(guard, clip, barkVolume);
        lastSpeaker = guard;
    }

    /// <summary>Call from AttackState.Enter().</summary>
    public void PlaySpottedBark(Guard guard)
    {
        if (attackClips.Length == 0 || guard == null) return;

        AudioClip clip = attackClips[Random.Range(0, attackClips.Length)];
        PlayClipOnGuard(guard, clip, barkVolume);
        lastSpeaker = guard;
    }

    private void PlayClipOnGuard(Guard guard, AudioClip clip, float volume)
    {
        if (guard == null || guard.speechAudioSource == null || clip == null) return;

        guard.speechAudioSource.PlayOneShot(clip, volume);
        Debug.Log($"[GuardSpeech] {guard.gameObject.name} played '{clip.name}'");
    }

    private List<Guard> GetGuardsInState(Guard.GuardVocalState state)
    {
        List<Guard> result = new List<Guard>();
        foreach (Guard g in Guard.AllGuards)
        {
            if (g != null && g.CurrentVocalState == state)
                result.Add(g);
        }
        return result;
    }

    private Guard PickRandomGuard(List<Guard> guards, Guard exclude)
    {
        if (guards.Count == 0) return null;

        List<Guard> eligible = new List<Guard>(guards);
        if (exclude != null && eligible.Count > 1)
        {
            eligible.Remove(exclude);
        }

        return eligible[Random.Range(0, eligible.Count)];
    }
}