using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters;

namespace W5SolidLsp.Models.Classes;

/// <summary>
/// Cleric — a holy warrior who strikes with divine power and shields allies.
/// Corresponds to the "Cleric" class in the character data files.
/// Implements IEntity (divine smite) and IDefendable (holy ward).
/// Future: IHealable when that interface is built.
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
}
