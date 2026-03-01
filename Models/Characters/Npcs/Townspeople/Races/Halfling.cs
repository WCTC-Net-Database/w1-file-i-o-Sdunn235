using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters.Npcs.Townspeople;

namespace W5SolidLsp.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Halfling — a small, nimble race at home in rivers and rural settlements.
/// ISwimmable is a racial trait — halflings move through water with surprising ease.
/// </summary>
public class Halfling : Townsperson, ISwimmable
{
    public string RacialTrait => "Nimble — small and quick, naturally at home in rivers and streams.";

    public Halfling() : base() { }

    public Halfling(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    public void Swim() =>
        Console.WriteLine($"{Name} slips into the water and paddles with surprising speed!");
}
