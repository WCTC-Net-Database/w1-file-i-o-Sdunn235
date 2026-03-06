using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Ranger - a wilderness scout comfortable in any terrain.
/// Implements IEntity (melee strike), IShootable (ranged shot), and ISwimmable.
/// </summary>
public class Ranger : Character, IEntity, IShootable, ISwimmable
{
    public Ranger() : base("Ranger", "Ranger", 1, 22, "bow|quiver|cloak") { }

    public Ranger(string name, int level, int hp, string equipment)
        : base(name, "Ranger", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} draws a hunting blade for a swift close-quarters strike!");

    public void Shoot() =>
        Console.WriteLine($"{Name} steadies their breath and looses a precisely aimed arrow!");

    public void Swim() =>
        Console.WriteLine($"{Name} slips into the water and moves through it with practiced ease!");

    /// <summary>Ranger sets a trap that ensnares enemies attempting to pass through.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} swiftly sets a hidden trap, ready to ensnare any enemy who steps near!");
    }
}
