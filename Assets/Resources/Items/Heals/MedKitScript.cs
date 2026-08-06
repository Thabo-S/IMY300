using UnityEngine;

// Attach to the MedKit item root (the same object that has PickupItem on it).
// Fully heals the player and is permanently removed from the scene when used.
public class MedKitScript : MonoBehaviour, IConsumable
{
    public void Consume(Player player)
    {
        if (player == null) return;

        player.Heal(player.MaxHealth); // Heal() clamps to MaxHealth anyway,
                                       // but this reads clearly as "full heal"
    }
}