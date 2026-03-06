using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Blacksmith - a skilled craftsperson who can hold their own in a fight.
/// Implements IEntity (hammer strike) and IDefendable (forge stance).
/// </summary>
public class Blacksmith : Character, IEntity, IDefendable
{
    public Blacksmith() : base("Blacksmith", "Blacksmith", 1, 20, "hammer|apron|tongs") { }

    public Blacksmith(string name, int level, int hp, string equipment)
        : base(name, "Blacksmith", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} swings their heavy forge hammer with practiced force!");

    public void Defend() =>
        Console.WriteLine($"{Name} hunches into a forge stance, using their thick apron to deflect the blow!");

    /// <summary>Blacksmith repairs and reinforces their own armor on the spot.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} quickly repairs and reinforces their armor with expert hands, restoring their defenses!");
    }
}
