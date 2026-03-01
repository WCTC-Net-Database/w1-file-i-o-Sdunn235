namespace W5SolidLsp.Models.Characters;

/// <summary>
/// Represents the base data for any character in the game — player or NPC.
/// Kept concrete (non-abstract) so CsvHelper can instantiate it directly for file I/O.
/// All entities in the game derive from this class.
///
/// Core design philosophy: anything a Player can do, an NPC can do too.
/// Capabilities are granted by interfaces (IEntity, IFlyable, etc.), not by class position.
/// </summary>
public class Character
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

    /// <summary>Creates a new Character instance with default empty values.</summary>
    public Character()
    {
        Name = string.Empty;
        Class = string.Empty;
        Equipment = string.Empty;
    }

    /// <summary>Creates a new Character instance with the specified values.</summary>
    public Character(string name, string charClass, int level, int hp, string equipment)
    {
        Name = name;
        Class = charClass;
        Level = level;
        Hp = hp;
        Equipment = equipment;
    }
}
