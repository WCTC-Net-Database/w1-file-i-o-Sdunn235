using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using W4Ocp.Interfaces;

namespace W4Ocp.Services;

/// <summary>
/// CSV implementation of IFileHandler.
/// Handles reading and writing character data to CSV files.
/// This class follows the Open/Closed Principle - it's one implementation
/// that can be swapped with other implementations (like JSON) without
/// modifying the code that uses it.
/// </summary>
public class CsvFileHandler : IFileHandler
{
    private readonly string _filePath;
    private readonly CsvConfiguration _csvConfig;

    /// <summary>
    /// Initializes a new instance of CsvFileHandler with the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    public CsvFileHandler(string filePath)
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
    /// Writes all characters to the CSV file, replacing any existing content.
    /// This is useful when you need to update the entire file (like after leveling up a character).
    /// </summary>
    /// <param name="characters">The list of characters to write.</param>
    public void WriteAll(List<Character> characters)
    {
        using StreamWriter writer = new(_filePath, false);
        using CsvWriter csv = new(writer, _csvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();
        csv.WriteRecords(characters);
    }

    /// <summary>
    /// Appends a single character to the end of the CSV file.
    /// This is more efficient than rewriting the entire file when adding one character.
    /// </summary>
    /// <param name="character">The character to append.</param>
    public void AppendCharacter(Character character)
    {
        // Check if file exists to determine if we need to write headers
        bool fileExists = File.Exists(_filePath);
        
        using StreamWriter writer = new(_filePath, append: true);
        using CsvWriter csv = new(writer, _csvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();
        
        // Only write header if file doesn't exist
        if (!fileExists)
        {
            csv.WriteHeader<Character>();
            csv.NextRecord();
        }
        
        csv.WriteRecord(character);
        csv.NextRecord();
    }
}
