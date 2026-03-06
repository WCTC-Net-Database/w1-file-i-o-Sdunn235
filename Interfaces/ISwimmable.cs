namespace W6DependencyInversion.Interfaces;

/// <summary>
/// Interface for entities that can swim.
/// Focused single-purpose interface (ISP) — only aquatic or amphibious entities implement this.
///
/// GameEngine uses the 'is' keyword to check before calling:
///   if (entity is ISwimmable swimmer) swimmer.Swim();
/// </summary>
public interface ISwimmable
{
    /// <summary>Performs this entity's swim action.</summary>
    void Swim();
}
