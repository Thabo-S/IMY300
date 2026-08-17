using UnityEngine;

// Attach to the Key item root (the same object that has PickupItem on it).
// This is just a marker - it doesn't implement IConsumable like MedKit/Bandages,
// because using a key needs to know WHICH door you're looking at. PickUpScript
// checks for this component specifically and, if found, tries to unlock
// whatever door is currently highlighted instead of running the normal
// consume-and-heal flow.
public class KeyScript : MonoBehaviour
{

}