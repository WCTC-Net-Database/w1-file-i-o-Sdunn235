using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace W3Srp.Services;

/// <summary>
/// Responsible for reading Character data from CSV files.
/// This class follows the Single Responsibility Principle - it only handles reading operations.
/// </summary>
public class CharacterReader
{
    private readonly string _filePath;
    private readonly CsvConfiguration _csvConfig;

    /// <summary>
    /// Initializes a new instance of CharacterReader with the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the CSV file to read from.</param>
    public CharacterReader(string filePath)
    {
        _filePath = filePath;
        _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };
    }

    /// <summary>
    /// Reads all characters from the CSV file.
    /// </summary>
    /// <returns>A list of all characters in the file, or an empty list if file doesn't exist.</returns>
    public List<Character> ReadAll()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Character>();
        }

        using StreamReader reader = new(_filePath);
        using CsvReader csv = new(reader, _csvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();
        return csv.GetRecords<Character>().ToList();
    }

    /// <summary>
    /// Finds a character by name using LINQ's FirstOrDefault.
    /// This is case-insensitive for better user experience.
    /// </summary>
    /// <param name="name">The name of the character to find.</param>
    /// <returns>The first character with matching name, or null if not found.</returns>
    public Character? FindByName(string name)
    {
        List<Character> characters = ReadAll();
        return characters.FirstOrDefault(c => 
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds all characters of a specific class using LINQ's Where.
    /// This is case-insensitive for better user experience.
    /// </summary>
    /// <param name="className">The class name to filter by.</param>
    /// <returns>A list of all characters with the matching class.</returns>
    public List<Character> FindByClass(string className)
    {
        List<Character> characters = ReadAll();
        return characters
            .Where(c => c.Class.Equals(className, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
