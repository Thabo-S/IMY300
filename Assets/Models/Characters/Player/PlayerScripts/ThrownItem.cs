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
        hasLanded = true;

        SoundEmissionManager.EmitSound(transform.position, impactVolume, true);

        gameObject.tag = originalTag;
    }
}