using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters;

namespace W5SolidLsp.Models.Classes;

/// <summary>
/// Wizard — a spellcasting class that attacks with magic and can fly via levitation.
/// Corresponds to the "Wizard" class in the character data files.
/// Implements IEntity (arcane attack) and IFlyable (magical levitation).
/// </summary>
public class Wizard : Character, IEntity, IFlyable
{
    public Wizard() : base("Wizard", "Wizard", 1, 12, "staff|robe|book") { }

    public Wizard(string name, int level, int hp, string equipment)
        : base(name, "Wizard", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} hurls a crackling bolt of arcane energy!");

    public void Fly() =>
        Console.WriteLine($"{Name} rises off the ground on a shimmering magical current!");
}
