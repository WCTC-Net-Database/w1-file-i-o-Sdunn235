namespace W4Ocp.Interfaces;

/// <summary>
/// Interface that defines the contract for file handling operations.
/// Combines reading and writing operations for character data.
/// This interface follows the Open/Closed Principle - new implementations
/// can be added without modifying existing code.
/// </summary>
public interface IFileHandler
{
    /// <summary>
    /// Reads all characters from the data source.
    /// </summary>
    /// <returns>A list of all characters.</returns>
    List<Character> ReadAll();

    /// <summary>
    /// Finds a character by name from the provided list of characters.
    /// </summary>
    /// <param name="characters">The list of characters to search.</param>
    /// <param name="name">The name of the character to find.</param>
    /// <returns>The character if found, otherwise null.</returns>
    Character? FindByName(List<Character> characters, string name);

    /// <summary>
    /// Finds all characters of a specific class from the provided list of characters.
    /// </summary>
    /// <param name="characters">The list of characters to search.</param>
    /// <param name="className">The class name to filter by.</param>
    /// <returns>A list of characters matching the specified class.</returns>
    List<Character> FindByClass(List<Character> characters, string className);

    /// <summary>
    /// Writes all characters to the data source, replacing any existing content.
    /// </summary>
    /// <param name="characters">The list of characters to write.</param>
    void WriteAll(List<Character> characters);

    /// <summary>
    /// Appends a single character to the data source.
    /// </summary>
    /// <param name="character">The character to append.</param>
    void AppendCharacter(Character character);
}
