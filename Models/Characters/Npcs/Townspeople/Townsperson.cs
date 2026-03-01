using W5SolidLsp.Models.Characters.Npcs;

namespace W5SolidLsp.Models.Characters.Npcs.Townspeople;

/// <summary>
/// Abstract base class for friendly or neutral NPC townspeople.
/// Derives from Npc — townspeople share the same Character base as monsters and the player.
///
/// Townspeople default to Neutral faction. They CAN implement IEntity and fight
/// if the game calls for it — anything the player can do, an NPC can do too.
///
/// Races (Elf, Dwarf, Human, Orc, etc.) derive from Townsperson.
/// A race defines appearance and lore, not capability — capabilities come from interfaces.
///
/// NOTE: Townsperson and Races are Brain Dump / future week content.
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
