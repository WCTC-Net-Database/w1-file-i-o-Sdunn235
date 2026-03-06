namespace W6DependencyInversion.Models.Characters.Npcs;

/// <summary>
/// Abstract base class for all non-player characters.
/// Derives from Character, which extends CharacterBase.
/// Adds a Faction property for alignment (Hostile, Neutral, Friendly).
/// </summary>
public abstract class Npc : Character
{
    /// <summary>The NPC's faction alignment (e.g., Hostile, Neutral, Friendly).</summary>
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
