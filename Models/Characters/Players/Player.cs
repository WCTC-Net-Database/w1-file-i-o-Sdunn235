namespace W5SolidLsp.Models.Characters.Players;

/// <summary>
/// Represents the user-controlled player character.
/// Derives from Character — the player shares the same base data as every NPC.
///
/// Core design philosophy: anything a Player can do, an NPC can do too.
/// Capabilities (IFlyable, IShootable, etc.) are added via interfaces, not subclassing.
/// </summary>
public class Player : Character
{
    /// <summary>Creates a new Player with default empty values.</summary>
    public Player() : base() { }

    /// <summary>Creates a new Player with the specified values.</summary>
    public Player(string name, string charClass, int level, int hp, string equipment)
        : base(name, charClass, level, hp, equipment) { }
}
