using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters.Npcs.Townspeople;

namespace W6DependencyInversion.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Halfling - a small, nimble race at home in rivers and rural settlements.
/// ISwimmable is a racial trait - halflings move through water with surprising ease.
/// </summary>
public class Halfling : Townsperson, ISwimmable
{
    public string RacialTrait => "Nimble - small and quick, naturally at home in rivers and streams.";

    public Halfling() : base() { }

    public Halfling(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    public void Swim() =>
        Console.WriteLine($"{Name} slips into the water and paddles with surprising speed!");

    /// <summary>Halfling vanishes from sight using their natural knack for stealth.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} disappears into the shadows with an almost supernatural knack for going unnoticed!");
    }
}
