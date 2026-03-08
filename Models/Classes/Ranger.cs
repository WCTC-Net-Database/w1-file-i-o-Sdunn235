using W6SolidDip.Interfaces;
using W6SolidDip.Models.Characters;

namespace W6SolidDip.Models.Classes;

/// <summary>
/// Ranger — a wilderness scout comfortable in any terrain.
/// Inherits IEntity from CharacterBase; also implements IShootable and ISwimmable.
/// </summary>
public class Ranger : CharacterBase, IShootable, ISwimmable
{
    public Ranger() : base("Ranger", "Ranger", 1, 22, "bow|quiver|cloak") { }

    public Ranger(string name, int level, int hp, string equipment)
        : base(name, "Ranger", level, hp, equipment) { }

    public override void Attack() =>
        Console.WriteLine($"{Name} draws a hunting blade for a swift close-quarters strike!");

    public void Shoot() =>
        Console.WriteLine($"{Name} steadies their breath and looses a precisely aimed arrow!");

    public void Swim() =>
        Console.WriteLine($"{Name} slips into the water and moves through it with practiced ease!");

    /// <summary>Ranger marks a target, ensuring every following strike lands true.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} marks the target — every arrow will find its mark until the hunt ends!");
    }
}
