using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters.Npcs.Monsters;

namespace W6DependencyInversion.Models.Characters.Npcs.Monsters;

/// <summary>
/// A troll - a large, hostile creature that regenerates and can swim.
/// Implements IEntity (can attack) and ISwimmable (can swim).
/// </summary>
public class Troll : Monster, IEntity, ISwimmable
{
    public Troll() : base("Troll", "Monster", 4, 60, "club") { }

    public Troll(string name, int level, int hp, string equipment)
        : base(name, "Monster", level, hp, equipment) { }

    /// <summary>Troll attacks with a heavy overhead club swing.</summary>
    public void Attack()
    {
        Console.WriteLine($"{Name} smashes down with its enormous club!");
    }

    /// <summary>Troll wades through water with powerful strokes.</summary>
    public void Swim()
    {
        Console.WriteLine($"{Name} surges through the water with powerful strokes!");
    }

    /// <summary>Troll regenerates hit points through thick, rubbery hide.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} roars and regenerates, wounds knitting closed before your eyes!");
    }
}
