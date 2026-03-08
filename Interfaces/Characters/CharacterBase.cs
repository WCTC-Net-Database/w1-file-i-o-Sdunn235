using W6SolidDip.Interfaces;

namespace W6SolidDip.Models.Characters;

/// <summary>
/// Abstract base class for all combat-capable characters in the game.
/// Extends Character for shared data properties and implements IEntity so
/// every concrete character can participate in GameEngine turns.
///
/// DIP: GameEngine depends on IEntity (abstraction), never on a concrete character type.
/// Abstract Class: Provides shared code (virtual Attack) while enforcing
/// that every derived type defines its own unique special action.
/// </summary>
public abstract class CharacterBase : Character, IEntity
{
    /// <summary>Creates a new CharacterBase with default empty values.</summary>
    protected CharacterBase() : base() { }

    /// <summary>Creates a new CharacterBase with the specified values.</summary>
    protected CharacterBase(string name, string charClass, int level, int hp, string equipment)
        : base(name, charClass, level, hp, equipment) { }

    /// <summary>
    /// Default attack behavior. Derived classes can override for custom attack messages.
    /// Virtual allows specialization; the base implementation is a safe fallback.
    /// </summary>
    public virtual void Attack()
    {
        Console.WriteLine($"{Name} attacks!");
    }

    /// <summary>
    /// Each character type MUST implement its own unique special action.
    /// This is the abstract method that enforces derived-class responsibility.
    /// </summary>
    public abstract void PerformSpecialAction();
}
