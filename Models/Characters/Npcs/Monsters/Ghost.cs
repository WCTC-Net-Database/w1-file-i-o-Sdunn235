using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters.Npcs.Monsters;

namespace W6DependencyInversion.Models.Characters.Npcs.Monsters;

/// <summary>
/// A ghost - a hostile, undead spirit that can fly.
/// Implements IEntity (can attack) and IFlyable (can fly).
/// </summary>
public class Ghost : Monster, IEntity, IFlyable
{
    public Ghost() : base("Ghost", "Undead", 3, 20, "none") { }

    public Ghost(string name, int level, int hp)
        : base(name, "Undead", level, hp, "none") { }

    /// <summary>Ghost attacks by chilling the target with an icy touch.</summary>
    public void Attack()
    {
        Console.WriteLine($"{Name} reaches out with an icy touch!");
    }

    /// <summary>Ghost floats silently through the air.</summary>
    public void Fly()
    {
        Console.WriteLine($"{Name} drifts silently through the air!");
    }

    /// <summary>Ghost phases through a solid object, becoming briefly intangible.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} phases through solid matter, becoming completely intangible!");
    }
}
