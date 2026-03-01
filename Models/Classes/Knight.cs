using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters;

namespace W5SolidLsp.Models.Classes;

/// <summary>
/// Knight — a mounted, heavily armored warrior who attacks and holds the line.
/// Corresponds to the "Knight" class in the character data files.
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
}
