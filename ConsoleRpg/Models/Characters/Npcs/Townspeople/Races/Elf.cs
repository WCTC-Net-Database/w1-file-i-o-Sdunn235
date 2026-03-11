using ConsoleRpg.Interfaces;
using ConsoleRpg.Models.Characters.Npcs.Townspeople;

namespace ConsoleRpg.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Elf — an ancient, graceful race with a natural affinity for magic and the sky.
/// Elves commonly live in forest settlements and cities.
/// IFlyable is a racial trait — any elf, regardless of CharacterClass, can fly.
/// </summary>
public class Elf : Townsperson, IFlyable
{
    public string RacialTrait => "Graceful — natural affinity for magic and aerial movement.";

    public Elf() : base() { }

    public Elf(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    public void Fly() =>
        Console.WriteLine($"{Name} rises gracefully on a current of magical wind!");

    /// <summary>Elf communes with nature, sensing hidden threats nearby.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} communes with the forest, sensing all threats within the grove!");
    }
}
