using W6SolidDip.Models.Characters.Npcs.Monsters;

namespace W6SolidDip.Models.Characters.Npcs.Monsters;

/// <summary>
/// A goblin — a small, hostile creature that attacks with crude weapons.
/// Inherits IEntity from CharacterBase (via Monster ? Npc ? CharacterBase).
///
/// LSP: Goblin does NOT implement IFlyable — goblins can't fly, and that's fine.
/// No NotSupportedException, no empty stub. The hierarchy is honest.
/// </summary>
public class Goblin : Monster
{
    public Goblin() : base("Goblin", "Monster", 1, 15, "crude dagger") { }

    public Goblin(string name, int level, int hp, string equipment)
        : base(name, "Monster", level, hp, equipment) { }

    /// <summary>Goblin attacks with its crude weapon.</summary>
    public override void Attack()
    {
        Console.WriteLine($"{Name} swings its crude dagger wildly!");
    }

    /// <summary>Goblin lets out a shrill shriek, calling nearby goblins to aid it.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} lets out a shrill shriek, rallying nearby goblins!");
    }
}
