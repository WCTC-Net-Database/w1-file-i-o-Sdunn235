using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Interfaces;

/// <summary>
/// Interface that defines the contract for file handling operations.
/// Depends on CharacterBase (abstraction), not the concrete Character type.
/// This satisfies DIP: high-level modules and this interface depend on abstractions.
///
/// New implementations (XML, Database, etc.) can be added without modifying existing code.
/// </summary>
public interface IFileHandler
{
    /// <summary>
    /// Reads all characters from the data source.
    /// </summary>
    /// <returns>A list of all characters as CharacterBase references.</returns>
    List<CharacterBase> ReadAll();

    /// <summary>
    /// Finds a character by name from the provided list of characters.
    /// </summary>
    /// <param name="characters">The list of characters to search.</param>
    /// <param name="name">The name of the character to find.</param>
    /// <returns>The character if found, otherwise null.</returns>
    CharacterBase? FindByName(List<CharacterBase> characters, string name);

    /// <summary>
    /// Finds all characters of a specific class from the provided list of characters.
    /// </summary>
    /// <param name="characters">The list of characters to search.</param>
    /// <param name="className">The class name to filter by.</param>
    /// <returns>A list of characters matching the specified class.</returns>
    List<CharacterBase> FindByClass(List<CharacterBase> characters, string className);

    /// <summary>
    /// Writes all characters to the data source, replacing any existing content.
    /// </summary>
    /// <param name="characters">The list of characters to write.</param>
    void WriteAll(List<CharacterBase> characters);

    /// <summary>
    /// Appends a single character to the data source.
    /// </summary>
    /// <param name="character">The character to append.</param>
    void AppendCharacter(CharacterBase character);
}
