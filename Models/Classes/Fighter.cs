using W6SolidDip.Interfaces;
using W6SolidDip.Models.Characters;

namespace W6SolidDip.Models.Classes;

/// <summary>
/// Fighter — a heavily armored melee combatant who attacks and defends.
/// Inherits IEntity from CharacterBase; also implements IDefendable.
/// </summary>
public class Fighter : CharacterBase, IDefendable
{
    public Fighter() : base("Fighter", "Fighter", 1, 30, "sword|shield") { }

    public Fighter(string name, int level, int hp, string equipment)
        : base(name, "Fighter", level, hp, equipment) { }

    public override void Attack() =>
        Console.WriteLine($"{Name} swings their sword in a powerful arc!");

    public void Defend() =>
        Console.WriteLine($"{Name} raises their shield and braces for impact!");

    /// <summary>Fighter performs a powerful overhead sword strike.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} leaps forward and delivers a devastating overhead sword strike!");
    }
}
