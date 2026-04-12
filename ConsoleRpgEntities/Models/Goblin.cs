namespace ConsoleRpgEntities.Models;

/// <summary>
/// Goblin character — inherits from Character for TPH storage.
/// Sneakiness is a Goblin-specific column in the Characters table (NULL for non-Goblin rows).
/// </summary>
public class Goblin : Character
{
    public int Sneakiness { get; set; }
}
