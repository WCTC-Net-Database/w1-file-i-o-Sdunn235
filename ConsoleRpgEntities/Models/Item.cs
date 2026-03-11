namespace ConsoleRpgEntities.Models;

/// <summary>
/// Represents an item or piece of equipment a player can carry and equip.
/// AttackBonus and DefenseBonus are used in LINQ combat calculations via BattleService.
/// </summary>
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Item category: "Weapon", "Armor", or "Consumable".</summary>
    public string Type { get; set; } = string.Empty;

    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public bool IsEquipped { get; set; }

    public override string ToString() =>
        $"{Name} ({Type}) | ATK+{AttackBonus} DEF+{DefenseBonus} | Equipped: {IsEquipped}";
}
