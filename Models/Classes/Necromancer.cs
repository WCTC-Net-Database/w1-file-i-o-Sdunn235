using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Necromancer - a dark spellcaster who bends the laws of life and death.
/// Implements IEntity (necrotic bolt attack) and IFlyable (death shroud levitation).
///
/// Stretch Goal: New character class with a unique PerformSpecialAction() override.
/// Special action: Raises a fallen enemy as an undead servant.
/// </summary>
public class Necromancer : Character, IEntity, IFlyable
{
    public Necromancer() : base("Necromancer", "Necromancer", 1, 14, "skull staff|dark robe|grimoire") { }

    public Necromancer(string name, int level, int hp, string equipment)
        : base(name, "Necromancer", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} hurls a bolt of necrotic energy that drains the life from their target!");

    public void Fly() =>
        Console.WriteLine($"{Name} rises on a shroud of writhing shadow energy, hovering above the ground!");

    /// <summary>
    /// Necromancer raises a fallen enemy as an undead servant.
    /// This is a unique special action - no other class can do this.
    /// </summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} chants a forbidden incantation and raises a fallen enemy as a loyal undead servant!");
    }
}
