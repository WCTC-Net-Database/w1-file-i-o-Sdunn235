using CsvHelper.Configuration;

/// <summary>
/// Defines the CSV column mapping for the Character class.
/// This is used by CsvHelper to read and write CSV files correctly.
/// </summary>
public sealed class CharacterMap : ClassMap<Character>
{
    public CharacterMap()
    {
        // CsvHelper uses these mappings to match CSV column positions to Character properties.
        // Index(0) is the first column, Index(1) is the second, etc. This is used for both read and write.
        Map(m => m.Name).Index(0);
        Map(m => m.Class).Index(1);
        Map(m => m.Level).Index(2);
        Map(m => m.Hp).Index(3);
        Map(m => m.Equipment).Index(4);
    }
}
