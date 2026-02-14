using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace W3Srp.Services;

/// <summary>
/// Responsible for writing Character data to CSV files.
/// This class follows the Single Responsibility Principle - it only handles writing operations.
/// </summary>
public class CharacterWriter
{
    private readonly string _filePath;
    private readonly CsvConfiguration _csvConfig;

    /// <summary>
    /// Initializes a new instance of CharacterWriter with the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the CSV file to write to.</param>
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
