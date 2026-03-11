using ConsoleRpg.Models.Characters.Npcs.Townspeople;

namespace ConsoleRpg.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Human — the most adaptable and widespread race in the world.
/// Humans have no innate special abilities — their strength is versatility.
/// They can adopt any CharacterClass and any interface capability.
/// </summary>
public class Human : Townsperson
{
    public string RacialTrait => "Adaptable — no innate bonuses, no innate weaknesses.";

    public Human() : base() { }

    public Human(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    /// <summary>Human draws on sheer determination to push past their limits.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} draws on sheer determination and pushes past their limits!");
    }
}
