using System.Text.Json;
using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Services;

/// <summary>
/// JSON implementation of IFileHandler.
/// Handles reading and writing character data to JSON files.
/// Internally uses the concrete Character type for JsonSerializer,
/// but exposes CharacterBase through the IFileHandler interface (DIP).
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
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Reads all characters from the JSON file.
    /// Returns CharacterBase references for DIP compliance.
    /// </summary>
    public List<CharacterBase> ReadAll()
    {
        if (!File.Exists(_filePath))
        {
            Console.WriteLine($"Warning: JSON file not found at '{_filePath}'");
            return new List<CharacterBase>();
        }

        try
        {
            string json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("Warning: JSON file is empty");
                return new List<CharacterBase>();
            }

            var characters = JsonSerializer.Deserialize<List<Character>>(json, _options);

            if (characters == null)
            {
                Console.WriteLine("Warning: JSON deserialization returned null");
                return new List<CharacterBase>();
            }

            return characters.Cast<CharacterBase>().ToList();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
            return new List<CharacterBase>();
        }
    }

    /// <summary>
    /// Finds a character by name using LINQ's FirstOrDefault.
    /// This is case-insensitive for better user experience.
    /// </summary>
    public CharacterBase? FindByName(List<CharacterBase> characters, string name)
    {
        return characters.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds all characters of a specific class using LINQ's Where.
    /// This is case-insensitive for better user experience.
    /// </summary>
    public List<CharacterBase> FindByClass(List<CharacterBase> characters, string className)
    {
        return characters
            .Where(c => c.Class.Equals(className, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Writes all characters to the JSON file, replacing any existing content.
    /// Converts CharacterBase references to concrete Character records for JsonSerializer.
    /// </summary>
    public void WriteAll(List<CharacterBase> characters)
    {
        var records = characters.Select(c => new Character(c.Name, c.Class, c.Level, c.Hp, c.Equipment)).ToList();
        string json = JsonSerializer.Serialize(records, _options);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Appends a single character to the JSON file.
    /// JSON doesn't support simple append - must read, add, then write.
    /// </summary>
    public void AppendCharacter(CharacterBase character)
    {
        var characters = ReadAll();
        characters.Add(character);
        WriteAll(characters);
    }
}
