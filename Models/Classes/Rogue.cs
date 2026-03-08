using W6SolidDip.Models.Characters;

namespace W6SolidDip.Models.Classes;

/// <summary>
/// Rogue — a stealthy class that strikes from the shadows.
/// Inherits IEntity from CharacterBase.
/// Future: IStealthable when that interface is built.
/// </summary>
public class Rogue : CharacterBase
{
    public Rogue() : base("Rogue", "Rogue", 1, 18, "dagger|lockpick|cloak") { }

    public Rogue(string name, int level, int hp, string equipment)
        : base(name, "Rogue", level, hp, equipment) { }

    public override void Attack() =>
        Console.WriteLine($"{Name} slips from the shadows and lands a precise dagger strike!");

    /// <summary>Rogue vanishes into the shadows, becoming invisible until they strike.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} melts into the shadows, becoming completely invisible!");
    }
}
