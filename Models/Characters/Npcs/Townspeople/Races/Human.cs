using W6DependencyInversion.Models.Characters.Npcs.Townspeople;

namespace W6DependencyInversion.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Human - the most adaptable and widespread race.
/// No innate special interfaces - their strength is versatility.
/// </summary>
public class Human : Townsperson
{
    public string RacialTrait => "Adaptable - no innate bonuses, no innate weaknesses.";

    public Human() : base() { }

    public Human(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    /// <summary>Human draws on sheer willpower to push past their limits.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} grits their teeth and pushes past their limits through sheer willpower!");
    }
}
