using W6DependencyInversion.Models.Characters.Npcs;

namespace W6DependencyInversion.Models.Characters.Npcs.Monsters;

/// <summary>
/// Abstract base class for all hostile monster NPCs.
/// Derives from Npc (which derives from Character and CharacterBase).
/// Monsters default to Hostile faction.
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
