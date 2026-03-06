using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Services;

/// <summary>
/// Responsible for reading Character data from CSV files.
/// Kept as a standalone helper class; main file I/O goes through CsvFileHandler.
/// </summary>
public class CharacterReader
{
    private readonly string _filePath;
    private readonly CsvConfiguration _csvConfig;

    /// <summary>
    /// Initializes a new instance of CharacterReader with the specified file path.
    /// </summary>
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
    /// Reads all characters from the CSV file as CharacterBase references.
    /// </summary>
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
    /// </summary>
    public CharacterBase? FindByName(string name)
    {
        List<CharacterBase> characters = ReadAll();
        return characters.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds all characters of a specific class using LINQ's Where.
    /// </summary>
    public List<CharacterBase> FindByClass(string className)
    {
        List<CharacterBase> characters = ReadAll();
        return characters
            .Where(c => c.Class.Equals(className, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
