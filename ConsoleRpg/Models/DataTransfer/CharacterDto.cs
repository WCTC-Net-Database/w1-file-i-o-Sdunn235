namespace ConsoleRpg.Models.DataTransfer;

/// <summary>
/// Data Transfer Object (DTO) for character serialization.
/// Used exclusively for reading/writing to CSV and JSON files.
/// This separates persistence concerns from domain model concerns (SRP).
/// 
/// Why a DTO?
/// - Domain models (Character, CharacterBase) can be abstract
/// - Serialization libraries need concrete classes to instantiate
/// - Changes to file format don't affect domain model
/// - Changes to domain model don't affect file format
/// </summary>
public class CharacterDto
{
    /// <summary>The character's name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The character's class/role (e.g., Fighter, Wizard, Rogue).</summary>
    public string Class { get; set; } = string.Empty;

    /// <summary>The character's current level.</summary>
    public int Level { get; set; }

    /// <summary>The character's current hit points (health).</summary>
    public int Hp { get; set; }

    /// <summary>Pipe-delimited list of equipment items the character is carrying.</summary>
    public string Equipment { get; set; } = string.Empty;
}
