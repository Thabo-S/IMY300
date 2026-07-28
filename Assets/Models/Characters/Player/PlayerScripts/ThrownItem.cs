using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrownItem : MonoBehaviour
{
    public float impactVolume = 100f;
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

        // Ignore the player who threw it - items are often spawned close
        // enough to still overlap the player's own collider for the first
        // physics step, which was causing "landed" to fire instantly at the
        // moment of throw instead of at the actual landing spot.
        if (collision.collider.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            return;
        }

        hasLanded = true;

        SoundEmissionManager.EmitSound(transform.position, impactVolume, true);

        gameObject.tag = originalTag;
    }
}