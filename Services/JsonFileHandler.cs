using System.Text.Json;
using W4Ocp.Interfaces;

namespace W4Ocp.Services;

/// <summary>
/// JSON implementation of IFileHandler.
/// Handles reading and writing character data to JSON files.
/// This class follows the Open/Closed Principle - it's a new implementation
/// added without modifying any existing code.
/// </summary>
public class JsonFileHandler : IFileHandler
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of JsonFileHandler with the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    public JsonFileHandler(string filePath)
    {
        _filePath = filePath;
        _options = new JsonSerializerOptions 
        { 
            WriteIndented = true,  // Makes JSON human-readable
            PropertyNameCaseInsensitive = true  // Allows flexible JSON property casing
        };
    }

    /// <summary>
    /// Reads all characters from the JSON file.
    /// </summary>
    /// <returns>A list of all characters in the file, or an empty list if file doesn't exist.</returns>
    public List<Character> ReadAll()
    {
        if (!File.Exists(_filePath))
        {
            Console.WriteLine($"Warning: JSON file not found at '{_filePath}'");
            return new List<Character>();
        }

        try
        {
            string json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("Warning: JSON file is empty");
                return new List<Character>();
            }

            var characters = JsonSerializer.Deserialize<List<Character>>(json, _options);

            if (characters == null)
            {
                Console.WriteLine("Warning: JSON deserialization returned null");
                return new List<Character>();
            }

            return characters;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
            return new List<Character>();
        }
    }

    /// <summary>
    /// Finds a character by name using LINQ's FirstOrDefault.
    /// This is case-insensitive for better user experience.
    /// NOTE: This method is IDENTICAL to CsvFileHandler - the interface ensures consistency!
    /// </summary>
    /// <param name="characters">The list of characters to search.</param>
    /// <param name="name">The name of the character to find.</param>
    /// <returns>The first character with matching name, or null if not found.</returns>
    public Character? FindByName(List<Character> characters, string name)
    {
        return characters.FirstOrDefault(c => 
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds all characters of a specific class using LINQ's Where.
    /// This is case-insensitive for better user experience.
    /// NOTE: This method is IDENTICAL to CsvFileHandler - the interface ensures consistency!
    /// </summary>
    /// <param name="characters">The list of characters to search.</param>
    /// <param name="className">The class name to filter by.</param>
    /// <returns>A list of all characters with the matching class.</returns>
    public List<Character> FindByClass(List<Character> characters, string className)
    {
        return characters
            .Where(c => c.Class.Equals(className, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Writes all characters to the JSON file, replacing any existing content.
    /// </summary>
    /// <param name="characters">The list of characters to write.</param>
    public void WriteAll(List<Character> characters)
    {
        string json = JsonSerializer.Serialize(characters, _options);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Appends a single character to the JSON file.
    /// NOTE: JSON doesn't support simple append - must read, add, then write.
    /// </summary>
    /// <param name="character">The character to append.</param>
    public void AppendCharacter(Character character)
    {
        // JSON doesn't support simple append - must read, add, write
        var characters = ReadAll();
        characters.Add(character);
        WriteAll(characters);
    }
}
