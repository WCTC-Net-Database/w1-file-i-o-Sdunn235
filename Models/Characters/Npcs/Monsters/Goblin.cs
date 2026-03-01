using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters.Npcs.Monsters;

namespace W5SolidLsp.Models.Characters.Npcs.Monsters;

/// <summary>
/// A goblin — a small, hostile creature that attacks with crude weapons.
/// Implements IEntity (can attack) only.
///
/// LSP demo: Goblin does NOT implement IFlyable — goblins can't fly, and that's fine.
/// No NotSupportedException, no empty stub. The hierarchy is honest.
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
}
