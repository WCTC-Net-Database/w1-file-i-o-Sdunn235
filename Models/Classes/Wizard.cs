using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Wizard - a spellcasting class that attacks with magic and can fly via levitation.
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

    /// <summary>Wizard releases a burst of arcane energy in all directions.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} channels raw arcane power and releases a devastating area-burst spell!");
    }
}
