namespace ConsoleRpg.Models.Characters;

/// <summary>
/// Concrete implementation of Character for general-purpose use.
/// Used when we need a Character instance but don't need combat capabilities.
/// 
/// Examples of use:
/// - Reading/writing characters from files before they're assigned to specific classes
/// - Displaying character data in UI
/// - Temporary character objects during data manipulation
/// 
/// Design Note: This allows Character to be abstract while still supporting
/// scenarios where we need a simple character without combat behavior.
/// </summary>
public class BasicCharacter : Character
{
    /// <summary>Creates a new BasicCharacter with default empty values.</summary>
    public BasicCharacter() : base() { }

    /// <summary>Creates a new BasicCharacter with the specified values.</summary>
    public BasicCharacter(string name, string charClass, int level, int hp, string equipment)
        : base(name, charClass, level, hp, equipment) { }
}
