namespace ConsoleRpgEntities.Models;

/// <summary>
/// Represents a player entity loaded from players.json.
/// Carries a list of Items used in LINQ combat calculations in BattleService:
///
///   int attackBonus = player.Items
///       .Where(item => item.IsEquipped &amp;&amp; item.Type == "Weapon")
///       .Sum(item => item.AttackBonus);
///
/// DIP: ConsoleRpg depends on this model through IContext — never loads JSON directly.
/// </summary>
public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CharacterClass { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public AbilityScores AbilityScores { get; set; } = new();
    public List<Item> Items { get; set; } = new();

    public override string ToString() =>
        $"[Lv {Level}] {Name} ({CharacterClass}) | HP: {Hp}/{MaxHp}";
}
