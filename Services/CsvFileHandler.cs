using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Services;

/// <summary>
/// CSV implementation of IFileHandler.
/// Handles reading and writing character data to CSV files.
/// Internally uses the concrete Character class with CharacterMap for CsvHelper,
/// but exposes CharacterBase through the IFileHandler interface (DIP).
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
    /// Uses the concrete Character type internally; returns CharacterBase for DIP compliance.
    /// </summary>
    /// <returns>A list of all characters in the file, or an empty list if file doesn't exist.</returns>
    public List<CharacterBase> ReadAll()
    {
        if (!File.Exists(_filePath))
        {
            return new List<CharacterBase>();
        }

        using StreamReader reader = new(_filePath);
        using CsvReader csv = new(reader, _csvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();
        return csv.GetRecords<Character>().Cast<CharacterBase>().ToList();
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
    /// Writes all characters to the CSV file, replacing any existing content.
    /// Converts CharacterBase references to concrete Character records for CsvHelper.
    /// </summary>
    public void WriteAll(List<CharacterBase> characters)
    {
        var records = ToCharacterRecords(characters);
        using StreamWriter writer = new(_filePath, false);
        using CsvWriter csv = new(writer, _csvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();
        csv.WriteRecords(records);
    }

    /// <summary>
    /// Appends a single character to the end of the CSV file.
    /// This is more efficient than rewriting the entire file when adding one character.
    /// </summary>
    public void AppendCharacter(CharacterBase character)
    {
        bool fileExists = File.Exists(_filePath);
        var record = ToCharacterRecord(character);

        using StreamWriter writer = new(_filePath, append: true);
        using CsvWriter csv = new(writer, _csvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();

        if (!fileExists)
        {
            csv.WriteHeader<Character>();
            csv.NextRecord();
        }

        csv.WriteRecord(record);
        csv.NextRecord();
    }

    // --- Private helpers ---

    /// <summary>Converts a CharacterBase to a concrete Character record for CsvHelper.</summary>
    private static Character ToCharacterRecord(CharacterBase c) =>
        new(c.Name, c.Class, c.Level, c.Hp, c.Equipment);

    /// <summary>Converts a list of CharacterBase to concrete Character records for CsvHelper.</summary>
    private static List<Character> ToCharacterRecords(List<CharacterBase> characters) =>
        characters.Select(ToCharacterRecord).ToList();
}
