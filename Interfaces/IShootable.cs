namespace W6DependencyInversion.Interfaces;

/// <summary>
/// Interface for entities that can shoot a ranged attack.
/// Focused single-purpose interface (ISP) — only ranged attackers implement this.
///
/// GameEngine uses the 'is' keyword to check before calling:
///   if (entity is IShootable shooter) shooter.Shoot();
/// </summary>
public interface IShootable
{
    /// <summary>Performs this entity's ranged shoot action.</summary>
    void Shoot();
}
