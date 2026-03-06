using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Services;

/// <summary>
/// Responsible for writing Character data to CSV files.
/// Kept as a standalone helper class; main file I/O goes through CsvFileHandler.
/// </summary>
public class CharacterWriter
{
    private readonly string _filePath;
    private readonly CsvConfiguration _csvConfig;

    /// <summary>
    /// Initializes a new instance of CharacterWriter with the specified file path.
    /// </summary>
    public CharacterWriter(string filePath)
    {
        _filePath = filePath;
        _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };
    }

    /// <summary>
    /// Writes all characters to the CSV file, replacing any existing content.
    /// Accepts CharacterBase references; converts to Character records for CsvHelper.
    /// </summary>
    public void WriteAll(List<CharacterBase> characters)
    {
        var records = characters.Select(c => new Character(c.Name, c.Class, c.Level, c.Hp, c.Equipment)).ToList();
        using StreamWriter writer = new(_filePath, false);
        using CsvWriter csv = new(writer, _csvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();
        csv.WriteRecords(records);
    }

    /// <summary>
    /// Appends a single character to the end of the CSV file.
    /// </summary>
    public void AppendCharacter(CharacterBase character)
    {
        bool fileExists = File.Exists(_filePath);
        var record = new Character(character.Name, character.Class, character.Level, character.Hp, character.Equipment);

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
}
