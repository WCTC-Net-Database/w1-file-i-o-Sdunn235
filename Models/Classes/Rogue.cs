using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Rogue - a stealthy class that strikes from the shadows.
/// Implements IEntity (sneak attack) only.
/// </summary>
public class Rogue : Character, IEntity
{
    public Rogue() : base("Rogue", "Rogue", 1, 18, "dagger|lockpick|cloak") { }

    public Rogue(string name, int level, int hp, string equipment)
        : base(name, "Rogue", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} slips from the shadows and lands a precise dagger strike!");

    /// <summary>Rogue poisons their blade with a fast-acting toxin.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} coats their blade in a fast-acting poison, ready to strike a vital point!");
    }
}
