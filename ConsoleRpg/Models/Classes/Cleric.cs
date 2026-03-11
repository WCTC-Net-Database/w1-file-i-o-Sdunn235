using ConsoleRpg.Interfaces;
using ConsoleRpg.Models.Characters;

namespace ConsoleRpg.Models.Classes;

/// <summary>
/// Cleric — a holy warrior who strikes with divine power and shields allies.
/// Inherits IEntity from CharacterBase; also implements IDefendable.
/// Future: IHealable when that interface is built.
/// </summary>
public class Cleric : CharacterBase, IDefendable
{
    public Cleric() : base("Cleric", "Cleric", 1, 22, "mace|armor|potion") { }

    public Cleric(string name, int level, int hp, string equipment)
        : base(name, "Cleric", level, hp, equipment) { }

    public override void Attack() =>
        Console.WriteLine($"{Name} calls down divine wrath in a smiting blow!");

    public void Defend() =>
        Console.WriteLine($"{Name} raises a shimmering holy ward against the attack!");

    /// <summary>Cleric channels divine energy to heal their wounds.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} channels divine light, mending wounds and restoring vitality!");
    }
}
