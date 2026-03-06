namespace W6DependencyInversion.Models.Characters.Players;

/// <summary>
/// Represents the user-controlled player character.
/// Derives from Character, which extends CharacterBase.
/// PerformSpecialAction() can be overridden per player build in the future.
/// </summary>
public class Player : Character
{
    /// <summary>Creates a new Player with default empty values.</summary>
    public Player() : base() { }

    /// <summary>Creates a new Player with the specified values.</summary>
    public Player(string name, string charClass, int level, int hp, string equipment)
        : base(name, charClass, level, hp, equipment) { }

    /// <summary>The player uses their signature move.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} the {Class} channels all their focus into a decisive move!");
    }
}
