using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Cleric - a holy warrior who strikes with divine power and shields allies.
/// Implements IEntity (divine smite) and IDefendable (holy ward).
/// </summary>
public class Cleric : Character, IEntity, IDefendable
{
    public Cleric() : base("Cleric", "Cleric", 1, 22, "mace|armor|potion") { }

    public Cleric(string name, int level, int hp, string equipment)
        : base(name, "Cleric", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} calls down divine wrath in a smiting blow!");

    public void Defend() =>
        Console.WriteLine($"{Name} raises a shimmering holy ward against the attack!");

    /// <summary>Cleric calls on their deity to heal themselves or nearby allies.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} raises their holy symbol and channels divine healing energy into their allies!");
    }
}
