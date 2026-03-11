using ConsoleRpg.Interfaces;
using ConsoleRpg.Models.Characters;

namespace ConsoleRpg.Models.Classes;

/// <summary>
/// Wizard — a spellcasting class that attacks with magic and can fly via levitation.
/// Inherits IEntity from CharacterBase; also implements IFlyable.
/// </summary>
public class Wizard : CharacterBase, IFlyable
{
    public Wizard() : base("Wizard", "Wizard", 1, 12, "staff|robe|book") { }

    public Wizard(string name, int level, int hp, string equipment)
        : base(name, "Wizard", level, hp, equipment) { }

    public override void Attack() =>
        Console.WriteLine($"{Name} hurls a crackling bolt of arcane energy!");

    public void Fly() =>
        Console.WriteLine($"{Name} rises off the ground on a shimmering magical current!");

    /// <summary>Wizard casts a devastating fireball that engulfs the area.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} draws a circle of runes and releases a searing fireball!");
    }
}
