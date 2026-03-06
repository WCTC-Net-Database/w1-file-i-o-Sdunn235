using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Models.Classes;

/// <summary>
/// Paladin - a holy warrior who combines martial prowess with divine magic.
/// Implements IEntity (divine strike) and IDefendable (sacred shield).
///
/// Stretch Goal: New character class with a unique PerformSpecialAction() override.
/// Special action: Lays on hands to heal a nearby ally.
/// </summary>
public class Paladin : Character, IEntity, IDefendable
{
    public Paladin() : base("Paladin", "Paladin", 1, 28, "holy sword|plate armor|holy symbol") { }

    public Paladin(string name, int level, int hp, string equipment)
        : base(name, "Paladin", level, hp, equipment) { }

    public void Attack() =>
        Console.WriteLine($"{Name} smites their foe with a divine-infused sword strike, searing them with holy light!");

    public void Defend() =>
        Console.WriteLine($"{Name} calls upon their deity and raises a blazing sacred shield that turns aside the blow!");

    /// <summary>
    /// Paladin lays hands on a nearby ally, channeling divine energy to heal them.
    /// This is a unique special action - no other class can do this.
    /// </summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} places a glowing hand on an ally and channels divine energy, closing their wounds instantly!");
    }
}
