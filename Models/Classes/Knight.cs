using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Knight - a mounted, heavily armored warrior who attacks and holds the line.
/// Implements IEntity (lance charge) and IDefendable (shield wall).
/// </summary>
public class Knight : Character, IEntity, IDefendable
{
    public Knight() : base("Knight", "Knight", 1, 35, "sword|armor|lance") { }

    public Knight(string name, int level, int hp, string equipment)
        : base(name, "Knight", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} charges forward with a thunderous lance strike!");

    public void Defend() =>
        Console.WriteLine($"{Name} plants their feet and raises a disciplined shield wall!");

    /// <summary>Knight issues a rallying cry that inspires all nearby allies.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} raises their banner high and issues a rallying cry, inspiring all nearby allies!");
    }
}
