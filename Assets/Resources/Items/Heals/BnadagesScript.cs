using UnityEngine;

// Attach to the Bandages item root (the same object that has PickupItem on it).
// Heals the player a partial amount (tweak in the Inspector) and is
// permanently removed from the scene when used.
public class BnadagesScript : MonoBehaviour, IConsumable
{
    [Tooltip("How much health this restores when used.")]
    public float healAmount = 25f;

    public void Consume(Player player)
    {
        if (player == null) return;

        player.Heal(healAmount);
    }
}