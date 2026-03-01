using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters;

namespace W5SolidLsp.Models.Classes;

/// <summary>
/// Rogue — a stealthy class that strikes from the shadows.
/// Corresponds to the "Rogue" class in the character data files.
/// Implements IEntity (sneak attack) only.
/// Future: IStealthable when that interface is built.
/// </summary>
public class Rogue : Character, IEntity
{
    public Rogue() : base("Rogue", "Rogue", 1, 18, "dagger|lockpick|cloak") { }

    public Rogue(string name, int level, int hp, string equipment)
        : base(name, "Rogue", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} slips from the shadows and lands a precise dagger strike!");
}
