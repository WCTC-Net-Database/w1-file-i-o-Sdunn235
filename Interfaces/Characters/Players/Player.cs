using W6SolidDip.Models.Characters;

namespace W6SolidDip.Models.Characters.Players;

/// <summary>
/// Represents the user-controlled player character.
/// Derives from CharacterBase — the player shares combat capability and the
/// requirement to perform a special action just like every NPC.
///
/// DIP: Player is an IEntity abstraction that GameEngine works with,
/// never referencing the concrete Player type directly.
/// </summary>
public class Player : CharacterBase
{
    /// <summary>Creates a new Player with default empty values.</summary>
    public Player() : base() { }

    /// <summary>Creates a new Player with the specified values.</summary>
    public Player(string name, string charClass, int level, int hp, string equipment)
        : base(name, charClass, level, hp, equipment) { }

    /// <summary>The player rallies their courage for a decisive strike.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} rallies their courage and lands a decisive strike!");
    }
}

