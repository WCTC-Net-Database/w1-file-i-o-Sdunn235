using W6DependencyInversion.Models.Characters.Npcs;

namespace W6DependencyInversion.Models.Characters.Npcs.Townspeople;

/// <summary>
/// Abstract base class for friendly or neutral NPC townspeople.
/// Derives from Npc. Townspeople default to Neutral faction.
/// </summary>
public abstract class Townsperson : Npc
{
    /// <summary>Creates a new Townsperson defaulting to Neutral faction.</summary>
    protected Townsperson() : base()
    {
        Faction = "Neutral";
    }

    /// <summary>Creates a new Townsperson with the specified values.</summary>
    protected Townsperson(string name, string charClass, int level, int hp, string equipment, string faction = "Neutral")
        : base(name, charClass, level, hp, equipment, faction) { }
}
