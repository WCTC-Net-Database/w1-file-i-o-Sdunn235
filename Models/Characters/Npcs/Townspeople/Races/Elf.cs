using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters.Npcs.Townspeople;

namespace W6DependencyInversion.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Elf - an ancient, graceful race with a natural affinity for magic and the sky.
/// IFlyable is a racial trait - any elf can fly.
/// </summary>
public class Elf : Townsperson, IFlyable
{
    public string RacialTrait => "Graceful - natural affinity for magic and aerial movement.";

    public Elf() : base() { }

    public Elf(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    public void Fly() =>
        Console.WriteLine($"{Name} rises gracefully on a current of magical wind!");

    /// <summary>Elf communes with nature spirits for guidance and protection.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} communes with the ancient forest spirits, gaining insight and protection!");
    }
}
