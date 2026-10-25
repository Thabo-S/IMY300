using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class NoiseMakerItem : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Played when the noise actually tiggers, so that the player hears it too.")]
    public AudioClip noiseClip;
    private AudioSource audioSource;


    [Tooltip("Maximum guard-hearing radius in Unity world units; deliberately larger than a normal thrown item.")]
    public float noiseRadius = 28f;

    [Tooltip("Seconds between landing and the sound actually going off.")]
    public float delayAfterLanding = 1.5f;

    private string originalTag;
    private bool hasLanded = false;

    public void Setup(string tagToRestore)
    {
        originalTag = tagToRestore;
        hasLanded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        if (collision.collider.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            return;
        }

        hasLanded = true;
        gameObject.tag = originalTag;

        Invoke(nameof(EmitNoise), delayAfterLanding);
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void EmitNoise()
    {
        SoundEmissionManager.EmitSound(transform.position, noiseRadius, true);

        if (audioSource != null && noiseClip != null)
        { 
            audioSource.PlayOneShot(noiseClip);
        }

    }
}