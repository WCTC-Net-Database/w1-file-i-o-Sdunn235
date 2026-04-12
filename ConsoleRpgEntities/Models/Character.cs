using ConsoleRpgEntities.Models.Abilities;

namespace ConsoleRpgEntities.Models;

/// <summary>
/// Abstract base class for all characters stored in the database via TPH.
/// Player and Goblin inherit from this — EF Core uses a Discriminator column
/// to distinguish between them in the shared Characters table.
/// </summary>
public abstract class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }

    public int RoomId { get; set; }
    public virtual Room Room { get; set; } = null!;

    /// <summary>Many-to-many: a character can have multiple abilities.</summary>
    public virtual ICollection<Ability> Abilities { get; set; } = new List<Ability>();

    /// <summary>
    /// Stretch goal: executes an ability during combat.
    /// Virtual so subclasses can override with type-specific behavior.
    /// </summary>
    public virtual void ExecuteAbility(Ability ability)
    {
        Console.WriteLine($"{Name} uses {ability.Name}!");
    }
}
