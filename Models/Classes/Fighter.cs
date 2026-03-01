using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters;

namespace W5SolidLsp.Models.Classes;

/// <summary>
/// Fighter — a heavily armored melee combatant who attacks and defends.
/// Corresponds to the "Fighter" class in the character data files.
/// Implements IEntity (melee attack) and IDefendable (shield stance).
/// </summary>
public class Fighter : Character, IEntity, IDefendable
{
    public Fighter() : base("Fighter", "Fighter", 1, 30, "sword|shield") { }

    public Fighter(string name, int level, int hp, string equipment)
        : base(name, "Fighter", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} swings their sword in a powerful arc!");

    public void Defend() =>
        Console.WriteLine($"{Name} raises their shield and braces for impact!");
}
