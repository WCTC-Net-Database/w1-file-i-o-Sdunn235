namespace W6DependencyInversion.Models.Characters;

/// <summary>
/// Abstract base class for all characters in the game.
/// Defines common properties (Name, Class, Level, HP, Equipment) and enforces
/// that every concrete character provides a unique PerformSpecialAction().
///
/// W6 change: Made abstract so CsvHelper concerns live only in the Services layer.
/// Character (the concrete CSV record) extends this class and satisfies CsvHelper.
/// </summary>
public abstract class CharacterBase
{
    /// <summary>The character's name.</summary>
    public string Name { get; set; }

    /// <summary>The character's class/role (e.g., Fighter, Wizard, Rogue).</summary>
    public string Class { get; set; }

    /// <summary>The character's current level.</summary>
    public int Level { get; set; }

    /// <summary>The character's current hit points (health).</summary>
    public int Hp { get; set; }

    /// <summary>Pipe-delimited list of equipment items the character is carrying.</summary>
    public string Equipment { get; set; }

    /// <summary>
    /// Performs this character's unique special action.
    /// Every concrete subclass must provide its own implementation.
    /// This is the W6 polymorphism hook - GameEngine calls this without knowing the concrete type.
    /// </summary>
    public abstract void PerformSpecialAction();

    /// <summary>
    /// Gets a list of individual equipment items by splitting the Equipment string.
    /// </summary>
    public List<string> GetEquipmentList()
    {
        if (string.IsNullOrWhiteSpace(Equipment))
            return new List<string>();

        return Equipment.Split('|')
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrEmpty(item))
            .ToList();
    }

    /// <summary>Provides a formatted string representation of the character.</summary>
    public override string ToString()
    {
        return $"[{Level}] {Name} ({Class}) | HP: {Hp} | Equipment: {Equipment}";
    }

    /// <summary>Creates a new CharacterBase instance with default empty values.</summary>
    protected CharacterBase()
    {
        Name = string.Empty;
        Class = string.Empty;
        Equipment = string.Empty;
    }

    /// <summary>Creates a new CharacterBase instance with the specified values.</summary>
    protected CharacterBase(string name, string charClass, int level, int hp, string equipment)
    {
        Name = name;
        Class = charClass;
        Level = level;
        Hp = hp;
        Equipment = equipment;
    }
}
