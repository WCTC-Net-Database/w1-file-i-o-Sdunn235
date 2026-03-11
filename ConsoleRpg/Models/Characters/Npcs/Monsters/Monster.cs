using ConsoleRpg.Models.Characters.Npcs;

namespace ConsoleRpg.Models.Characters.Npcs.Monsters;

/// <summary>
/// Abstract base class for all hostile monster NPCs.
/// Derives from Npc — monsters are NPCs that are hostile by default.
/// Concrete monsters (Ghost, Goblin, Troll) derive from this class and
/// implement IEntity plus any additional behavior interfaces they support.
/// </summary>
public abstract class Monster : Npc
{
    /// <summary>Creates a new Monster defaulting to Hostile faction.</summary>
    protected Monster() : base()
    {
        Faction = "Hostile";
    }

    /// <summary>Creates a new Monster with the specified values.</summary>
    protected Monster(string name, string charClass, int level, int hp, string equipment)
        : base(name, charClass, level, hp, equipment, "Hostile") { }
}
