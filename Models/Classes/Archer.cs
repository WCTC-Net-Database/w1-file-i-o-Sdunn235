using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters;

namespace W5SolidLsp.Models.Classes;

/// <summary>
/// An archer — a ranged combat specialist who can attack and shoot.
/// Implements IEntity (basic attack) and IShootable (ranged shot).
///
/// Note: Models/Classes/ holds RPG class templates (like D&D classes), not C# classes in the OOP sense.
/// These will eventually power a character creation system where a Player picks a CharacterClass.
/// For now, Archer acts as a standalone IEntity for GameEngine demonstration.
///
/// ISP demo: Archer implements exactly what it needs — nothing more.
/// </summary>
public class Archer : Character, IEntity, IShootable
{
    public Archer() : base("Archer", "Ranger", 2, 30, "longbow|quiver") { }

    public Archer(string name, int level, int hp, string equipment)
        : base(name, "Ranger", level, hp, equipment) { }

    /// <summary>Archer attacks with a swift close-range strike.</summary>
    public void Attack()
    {
        Console.WriteLine($"{Name} draws a short blade for a quick melee strike!");
    }

    /// <summary>Archer fires an arrow at the target.</summary>
    public void Shoot()
    {
        Console.WriteLine($"{Name} draws the bowstring and fires an arrow!");
    }
}
