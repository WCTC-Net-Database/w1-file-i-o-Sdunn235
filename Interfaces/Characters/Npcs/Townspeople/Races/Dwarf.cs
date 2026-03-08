using W6SolidDip.Interfaces;
using W6SolidDip.Models.Characters.Npcs.Townspeople;

namespace W6SolidDip.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Dwarf — a stout, resilient race built for underground labor and combat.
/// Dwarves live in mountain halls and mining towns.
/// IDefendable is a racial trait — their stocky build makes them natural defenders.
/// </summary>
public class Dwarf : Townsperson, IDefendable
{
    public string RacialTrait => "Resilient — naturally resistant to physical damage and knockback.";

    public Dwarf() : base() { }

    public Dwarf(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    public void Defend() =>
        Console.WriteLine($"{Name} digs in with powerful legs and shrugs off the blow!");

    /// <summary>Dwarf calls upon ancient resilience to endure devastating hits.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} invokes dwarven resilience, shrugging off the pain!");
    }
}
