namespace W5SolidLsp.Interfaces;

/// <summary>
/// Interface for entities that can take a defensive stance.
/// Separated from IEntity (ISP) — not every entity needs a defend action.
///
/// GameEngine uses the 'is' keyword to check before calling:
///   if (entity is IDefendable defender) defender.Defend();
/// </summary>
public interface IDefendable
{
    /// <summary>Performs this entity's defensive action.</summary>
    void Defend();
}
