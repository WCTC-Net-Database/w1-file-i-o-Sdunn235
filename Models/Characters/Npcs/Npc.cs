namespace W5SolidLsp.Models.Characters.Npcs;

/// <summary>
/// Abstract base class for all non-player characters.
/// Derives from Character — NPCs share the same base data as the player.
///
/// Any NPC subtype can implement any behavior interface (IEntity, IFlyable, etc.)
/// just as a Player can. The hierarchy defines what they ARE; interfaces define what they DO.
/// </summary>
public abstract class Npc : Character
{
    /// <summary>
    /// The NPC's faction alignment (e.g., Hostile, Neutral, Friendly).
    /// Used in future weeks to drive combat and dialogue decisions.
    /// </summary>
    public string Faction { get; set; }

    /// <summary>Creates a new Npc with default empty values.</summary>
    protected Npc() : base()
    {
        Faction = string.Empty;
    }

    /// <summary>Creates a new Npc with the specified values.</summary>
    protected Npc(string name, string charClass, int level, int hp, string equipment, string faction)
        : base(name, charClass, level, hp, equipment)
    {
        Faction = faction;
    }
}
