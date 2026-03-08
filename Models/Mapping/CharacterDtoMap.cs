using CsvHelper.Configuration;
using W6SolidDip.Models.DataTransfer;

namespace W6SolidDip.Models.Mapping;

/// <summary>
/// Defines the CSV column mapping for the CharacterDto class.
/// Used by CsvHelper to read and write CSV files correctly.
/// 
/// Note: This now maps to CharacterDto instead of Character,
/// separating serialization concerns from the domain model.
/// </summary>
public sealed class CharacterDtoMap : ClassMap<CharacterDto>
{
    public CharacterDtoMap()
    {
        // CsvHelper uses these mappings to match CSV column positions to CharacterDto properties.
        Map(m => m.Name).Index(0);
        Map(m => m.Class).Index(1);
        Map(m => m.Level).Index(2);
        Map(m => m.Hp).Index(3);
        Map(m => m.Equipment).Index(4);
    }
}
