using ConsoleRpg.Interfaces;
using ConsoleRpg.Models.Characters;

namespace ConsoleRpg.Models.Classes;

/// <summary>
/// An archer — a ranged combat specialist who can attack and shoot.
/// Inherits IEntity from CharacterBase; also implements IShootable.
///
/// ISP: Archer implements exactly what it needs — nothing more.
/// DIP: GameEngine depends on IEntity (abstraction), not Archer directly.
/// </summary>
public class Archer : CharacterBase, IShootable
{
    public Archer() : base("Archer", "Ranger", 2, 30, "longbow|quiver") { }

    public Archer(string name, int level, int hp, string equipment)
        : base(name, "Ranger", level, hp, equipment) { }

    /// <summary>Archer attacks with a swift close-range strike.</summary>
    public override void Attack()
    {
        Console.WriteLine($"{Name} draws a short blade for a quick melee strike!");
    }

    /// <summary>Archer fires an arrow at the target.</summary>
    public void Shoot()
    {
        Console.WriteLine($"{Name} draws the bowstring and fires an arrow!");
    }

    /// <summary>Archer takes careful aim, lining up a guaranteed critical shot.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} takes careful aim and lines up a guaranteed critical shot!");
    }
}
