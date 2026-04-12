using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleRpgEntities.Models;

/// <summary>
/// Player character — inherits from Character for TPH storage.
/// Hp and MaxHp are Player-specific columns in the Characters table (NULL for non-Player rows).
/// Legacy JSON properties are [NotMapped] so EF ignores them while FileContext/combat still works.
/// </summary>
public class Player : Character
{
    public int Hp { get; set; }
    public int MaxHp { get; set; }

    [NotMapped]
    public string CharacterClass { get; set; } = string.Empty;
    [NotMapped]
    public AbilityScores AbilityScores { get; set; } = new();
    [NotMapped]
    public List<Item> Items { get; set; } = new();

    public override string ToString() =>
        $"[Lv {Level}] {Name} ({CharacterClass}) | HP: {Hp}/{MaxHp}";
}
