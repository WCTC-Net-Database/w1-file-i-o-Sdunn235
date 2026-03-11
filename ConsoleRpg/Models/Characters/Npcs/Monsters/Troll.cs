using ConsoleRpg.Interfaces;

namespace ConsoleRpg.Models.Characters.Npcs.Monsters;

/// <summary>
/// A troll — a large, hostile creature that regenerates and can swim.
/// Inherits IEntity from CharacterBase (via Monster → Npc → CharacterBase).
/// Also implements ISwimmable — trolls wade through water with ease.
///
/// ISP: Troll only implements what it genuinely can do.
/// LSP: Troll safely substitutes for IEntity anywhere.
/// </summary>
public class Troll : Monster, ISwimmable
{
    public Troll() : base("Troll", "Monster", 4, 60, "club") { }

    public Troll(string name, int level, int hp, string equipment)
        : base(name, "Monster", level, hp, equipment) { }

    /// <summary>Troll attacks with a heavy overhead club swing.</summary>
    public override void Attack()
    {
        Console.WriteLine($"{Name} smashes down with its enormous club!");
    }

    /// <summary>Troll wades through water with powerful strokes.</summary>
    public void Swim()
    {
        Console.WriteLine($"{Name} surges through the water with powerful strokes!");
    }

    /// <summary>Troll regenerates hit points, healing its wounds mid-battle.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} regenerates, knitting its wounds closed before your eyes!");
    }
}
