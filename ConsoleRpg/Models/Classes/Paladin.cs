using ConsoleRpg.Interfaces;
using ConsoleRpg.Models.Characters;

namespace ConsoleRpg.Models.Classes;

/// <summary>
/// Paladin — a holy warrior who combines martial prowess with divine magic.
/// Inherits IEntity from CharacterBase; also implements IDefendable.
///
/// Stretch Goal: A creative class demonstrating CharacterBase inheritance and DIP.
/// PerformSpecialAction casts a divine blessing that buffs all nearby allies.
/// </summary>
public class Paladin : CharacterBase, IDefendable
{
    public Paladin() : base("Paladin", "Paladin", 2, 32, "sword|shield|holy symbol") { }

    public Paladin(string name, int level, int hp, string equipment)
        : base(name, "Paladin", level, hp, equipment) { }

    /// <summary>Paladin strikes with holy fire infused into their weapon.</summary>
    public override void Attack() =>
        Console.WriteLine($"{Name} strikes with holy fire, searing the unholy with divine energy!");

    /// <summary>Paladin raises a holy shield that protects against dark magic.</summary>
    public void Defend() =>
        Console.WriteLine($"{Name} raises a blessed shield, warding off dark and physical attacks!");

    /// <summary>Paladin calls down a divine blessing, strengthening all nearby allies.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} calls down a divine blessing, strengthening all nearby allies with holy power!");
    }
}
