using W6SolidDip.Models.Characters;

namespace W6SolidDip.Models.Classes;

/// <summary>
/// Healer — a support class dedicated to restoring allies and holding back death.
/// Inherits IEntity from CharacterBase so they can participate in combat turns.
///
/// Stretch Goal: Demonstrates DIP — GameEngine works with IEntity (abstraction)
/// and calls PerformSpecialAction() polymorphically without knowing the type.
/// </summary>
public class Healer : CharacterBase
{
    public Healer() : base("Healer", "Healer", 1, 18, "staff|bandages|herbs") { }

    public Healer(string name, int level, int hp, string equipment)
        : base(name, "Healer", level, hp, equipment) { }

    /// <summary>Healer strikes with their staff when pushed into melee range.</summary>
    public override void Attack() =>
        Console.WriteLine($"{Name} reluctantly swings their staff in a defensive strike!");

    /// <summary>Healer channels restorative magic to mend wounds on the battlefield.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} channels restorative light, mending wounds across the entire party!");
    }
}
