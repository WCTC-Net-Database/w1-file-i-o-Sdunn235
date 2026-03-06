using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Archer - a ranged combat specialist who can attack and shoot.
/// Implements IEntity (basic attack) and IShootable (ranged shot).
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

    /// <summary>Archer fires a volley of arrows in a wide arc.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} nocks three arrows at once and releases a devastating volley!");
    }
}
