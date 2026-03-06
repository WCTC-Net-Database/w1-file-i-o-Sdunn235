using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters.Npcs.Monsters;

namespace W6DependencyInversion.Models.Characters.Npcs.Monsters;

/// <summary>
/// A goblin - a small, hostile creature that attacks with crude weapons.
/// Implements IEntity (can attack) only.
/// </summary>
public class Goblin : Monster, IEntity
{
    public Goblin() : base("Goblin", "Monster", 1, 15, "crude dagger") { }

    public Goblin(string name, int level, int hp, string equipment)
        : base(name, "Monster", level, hp, equipment) { }

    /// <summary>Goblin attacks with its crude weapon.</summary>
    public void Attack()
    {
        Console.WriteLine($"{Name} swings its crude dagger wildly!");
    }

    /// <summary>Goblin lets out a piercing shriek to call nearby allies.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} lets out a piercing shriek, summoning nearby goblins!");
    }
}
