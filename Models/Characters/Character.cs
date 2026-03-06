namespace W6DependencyInversion.Models.Characters;

/// <summary>
/// Concrete character record used for CSV/JSON file I/O.
/// Kept non-abstract so CsvHelper and JsonSerializer can instantiate it directly.
/// Extends CharacterBase, which defines the shared properties and the abstract
/// PerformSpecialAction() contract.
///
/// Generic characters read from data files use this class. Specific RPG-class
/// characters (Fighter, Wizard, etc.) use their own concrete classes instead.
/// </summary>
public class Character : CharacterBase
{
    /// <summary>Creates a new Character instance with default empty values.</summary>
    public Character() : base() { }

    /// <summary>Creates a new Character instance with the specified values.</summary>
    public Character(string name, string charClass, int level, int hp, string equipment)
        : base(name, charClass, level, hp, equipment) { }

    /// <summary>
    /// Generic special action for characters read directly from data files.
    /// Specific subclasses (Fighter, Ghost, etc.) override this with unique behavior.
    /// </summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} the {Class} prepares for battle!");
    }
}
