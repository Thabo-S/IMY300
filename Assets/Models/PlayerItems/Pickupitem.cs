using UnityEngine;
// Drop this on the ROOT object of any pickupable item (MedKit, Bandages,
// FlashLight, etc). Add Box/Mesh Colliders to the individual child parts
// as needed - PickUpScript will walk up from whichever child collider was
// hit and find this component, so you don't need one big collider that
// tries to wrap the whole mesh.
public class PickupItem : MonoBehaviour
{
    [Tooltip("Used for the sprite lookup in Resources/Items/sprites/<itemName>. " +
             "Leave empty to just use this GameObject's name.")]
    public string itemName;

    [Tooltip("If false, this item can still be picked up and dropped (R), " +
             "but cannot be aimed/thrown. Uncheck this for MedKit, Bandages, " +
             "FlashLight, etc.")]
    public bool canThrow = true;

    [Header("Held-In-Hand Pose")]
    [Tooltip("Local position relative to the Hand Socket when this item is the selected hotbar slot.")]
    public Vector3 holdLocalPosition = new Vector3(0.3f, -0.3f, 0.5f);

    [Tooltip("Local rotation (euler angles) relative to the Hand Socket when held.")]
    public Vector3 holdLocalEulerAngles = Vector3.zero;

    [Tooltip("Extra size multiplier on top of the item's real-world size, in case it still " +
             "looks too big or small in hand. 1 = no change.")]
    public float holdScaleMultiplier = 1f;

    private void Awake()
    {
        if (string.IsNullOrEmpty(itemName))
            itemName = gameObject.name;
    }
}