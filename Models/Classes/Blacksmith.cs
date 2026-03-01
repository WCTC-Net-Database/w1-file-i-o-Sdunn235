using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters;

namespace W5SolidLsp.Models.Classes;

/// <summary>
/// Blacksmith — a skilled craftsperson who can hold their own in a fight.
/// Corresponds to the "Blacksmith" class in the character data files.
/// Implements IEntity (hammer strike) and IDefendable (forge stance).
/// Future: ICraftable when that interface is built.
/// </summary>
public class Blacksmith : Character, IEntity, IDefendable
{
    public Blacksmith() : base("Blacksmith", "Blacksmith", 1, 20, "hammer|apron|tongs") { }

    public Blacksmith(string name, int level, int hp, string equipment)
        : base(name, "Blacksmith", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} swings their heavy forge hammer with practiced force!");

    public void Defend() =>
        Console.WriteLine($"{Name} hunches into a forge stance, using their thick apron to deflect the blow!");
}
