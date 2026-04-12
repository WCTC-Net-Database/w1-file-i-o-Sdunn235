namespace ConsoleRpgEntities.Models.Abilities;

/// <summary>
/// Abstract base class for all abilities stored in the database via TPH.
/// PlayerAbility and GoblinAbility inherit from this — EF Core uses an AbilityType
/// discriminator column to distinguish between them in the shared Abilities table.
/// </summary>
public abstract class Ability
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Many-to-many: an ability can belong to multiple characters.</summary>
    public virtual ICollection<Character> Characters { get; set; } = new List<Character>();
}
