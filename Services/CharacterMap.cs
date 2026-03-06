using CsvHelper.Configuration;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Services;

/// <summary>
/// Defines the CSV column mapping for the Character class.
/// Kept in the Services layer so that the Models layer has no CsvHelper dependency.
/// Used by CsvFileHandler to read and write CSV files correctly.
/// </summary>
public sealed class CharacterMap : ClassMap<Character>
{
    public CharacterMap()
    {
        // CsvHelper uses these mappings to match CSV column positions to Character properties.
        Map(m => m.Name).Index(0);
        Map(m => m.Class).Index(1);
        Map(m => m.Level).Index(2);
        Map(m => m.Hp).Index(3);
        Map(m => m.Equipment).Index(4);
    }
}
