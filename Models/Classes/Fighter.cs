using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Fighter - a heavily armored melee combatant who attacks and defends.
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

    /// <summary>Fighter unleashes a powerful battle cry that boosts their attack.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} lets out a battle cry and enters a combat stance, ready for anything!");
    }
}
