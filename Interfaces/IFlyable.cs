namespace W5SolidLsp.Interfaces;

/// <summary>
/// Interface for entities that can fly.
/// Separated from IEntity (ISP) — only entities that genuinely fly implement this.
///
/// GameEngine uses the 'is' keyword to check before calling:
///   if (entity is IFlyable flier) flier.Fly();
///
/// This satisfies LSP: no entity is ever forced to fake a Fly() it can't perform.
/// </summary>
public interface IFlyable
{
    /// <summary>Performs this entity's fly action.</summary>
    void Fly();
}
