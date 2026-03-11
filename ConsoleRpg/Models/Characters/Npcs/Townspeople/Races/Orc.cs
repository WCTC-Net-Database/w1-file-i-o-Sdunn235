using ConsoleRpg.Models.Characters.Npcs.Townspeople;

namespace ConsoleRpg.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Orc — a powerful, aggressive race with immense physical strength.
/// Not all orcs are hostile — Faction determines alignment, not race.
/// Inherits IEntity from CharacterBase; combat-readiness is a racial trait.
/// </summary>
public class Orc : Townsperson
{
    public string RacialTrait => "Fierce — naturally powerful fighters with high physical strength.";

    public Orc() : base() { }

    public Orc(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    /// <summary>Orc charges with a ferocious war cry.</summary>
    public override void Attack() =>
        Console.WriteLine($"{Name} lets out a war cry and charges with ferocious strength!");

    /// <summary>Orc enters a berserker rage, doubling down on raw ferocity.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} enters a berserker rage, becoming a whirlwind of fury!");
    }
}
