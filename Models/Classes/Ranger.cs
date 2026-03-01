using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters;

namespace W5SolidLsp.Models.Classes;

/// <summary>
/// Ranger — a wilderness scout comfortable in any terrain.
/// Corresponds to the "Ranger" class in the character data files.
/// Implements IEntity (melee strike), IShootable (ranged shot), and ISwimmable
/// (rangers operate in swamps, rivers, and coastlines).
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
}
