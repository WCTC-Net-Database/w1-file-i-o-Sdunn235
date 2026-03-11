using ConsoleRpg.Interfaces;
using ConsoleRpg.Models.Characters;

namespace ConsoleRpg.Models.Classes;

/// <summary>
/// Blacksmith — a skilled craftsperson who can hold their own in a fight.
/// Inherits IEntity from CharacterBase; also implements IDefendable.
/// Future: ICraftable when that interface is built.
/// </summary>
public class Blacksmith : CharacterBase, IDefendable
{
    public Blacksmith() : base("Blacksmith", "Blacksmith", 1, 20, "hammer|apron|tongs") { }

    public Blacksmith(string name, int level, int hp, string equipment)
        : base(name, "Blacksmith", level, hp, equipment) { }

    public override void Attack() =>
        Console.WriteLine($"{Name} swings their heavy forge hammer with practiced force!");

    public void Defend() =>
        Console.WriteLine($"{Name} hunches into a forge stance, using their thick apron to deflect the blow!");

    /// <summary>Blacksmith tempers their own weapon mid-battle, sharpening it for bonus damage.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} rapidly re-tempers their hammer, sharpening it for a devastating follow-up!");
    }
}
