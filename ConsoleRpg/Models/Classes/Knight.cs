using ConsoleRpg.Interfaces;
using ConsoleRpg.Models.Characters;

namespace ConsoleRpg.Models.Classes;

/// <summary>
/// Knight — a mounted, heavily armored warrior who attacks and holds the line.
/// Inherits IEntity from CharacterBase; also implements IDefendable.
/// </summary>
public class Knight : CharacterBase, IDefendable
{
    public Knight() : base("Knight", "Knight", 1, 35, "sword|armor|lance") { }

    public Knight(string name, int level, int hp, string equipment)
        : base(name, "Knight", level, hp, equipment) { }

    public override void Attack() =>
        Console.WriteLine($"{Name} charges forward with a thunderous lance strike!");

    public void Defend() =>
        Console.WriteLine($"{Name} plants their feet and raises a disciplined shield wall!");

    /// <summary>Knight rallies nearby allies with a commanding battle cry.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} raises their banner and rallies nearby allies with a commanding battle cry!");
    }
}
