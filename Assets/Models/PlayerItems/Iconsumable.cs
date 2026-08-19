// Implement this on any item script (MedKitScript, BnadagesScript, etc.)
// that should be "used up" - consumed and removed - via right-click instead
// of being thrown. PickUpScript looks for this component automatically.
public interface IConsumable
{
    void Consume(Player player);
}