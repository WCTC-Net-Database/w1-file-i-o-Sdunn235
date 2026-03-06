namespace W6DependencyInversion.Interfaces;

/// <summary>
/// Base interface for all combat-capable entities in the game.
/// Both Player and NPC types can implement this — any entity that can attack.
///
/// LSP fix: Fly() has been removed from this interface.
/// Not all entities can fly, so putting Fly() here forced non-flying classes
/// to throw NotSupportedException — a direct LSP violation.
/// Flying is now handled by the separate IFlyable interface (ISP).
/// </summary>
public interface IEntity
{
    /// <summary>The entity's display name.</summary>
    string Name { get; }

    /// <summary>Performs this entity's basic attack action.</summary>
    void Attack();
}
