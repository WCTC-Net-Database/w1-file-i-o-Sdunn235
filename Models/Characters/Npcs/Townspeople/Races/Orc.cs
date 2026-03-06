using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters.Npcs.Townspeople;

namespace W6DependencyInversion.Models.Characters.Npcs.Townspeople.Races;

/// <summary>
/// Orc - a powerful, aggressive race with immense physical strength.
/// IEntity is a racial trait - orcs are battle-ready by nature.
/// </summary>
public class Orc : Townsperson, IEntity
{
    public string RacialTrait => "Fierce - naturally powerful fighters with high physical strength.";

    public Orc() : base() { }

    public Orc(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }

    public void Attack() =>
        Console.WriteLine($"{Name} lets out a war cry and charges with ferocious strength!");

    /// <summary>Orc enters a battle rage, ignoring pain and dealing devastating blows.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} enters a blood rage, their eyes going red as they ignore all pain!");
    }
}
