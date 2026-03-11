using ConsoleRpgEntities.Models;

namespace ConsoleRpg.Services;

/// <summary>
/// Handles all combat calculations using LINQ to sum item bonuses.
/// This is the exam's core LINQ pattern — see README examples.
///
/// SRP: Combat math lives here only — GameEngine asks for results, never calculates itself.
/// DIP: GameEngine depends on IBattleService, so this can be replaced without touching the engine.
/// </summary>
public class BattleService : IBattleService
{
    /// <summary>
    /// Calculates attack damage: Strength + sum of AttackBonus from all equipped weapons.
    /// </summary>
    public int CalculateAttackDamage(Player player)
    {
        int baseAttack = player.AbilityScores.Strength;

        int weaponBonus = player.Items
            .Where(item => item.IsEquipped && item.Type == "Weapon")
            .Sum(item => item.AttackBonus);

        return baseAttack + weaponBonus;
    }

    /// <summary>
    /// Calculates defense bonus: sum of DefenseBonus from all equipped armor.
    /// </summary>
    public int CalculateDefenseBonus(Player player)
    {
        return player.Items
            .Where(item => item.IsEquipped && item.Type == "Armor")
            .Sum(item => item.DefenseBonus);
    }

    /// <summary>
    /// Resolves one combat round: player attacks monster, monster attacks player.
    /// Defense reduces incoming damage. Returns a string for the UI to display.
    /// </summary>
    public string ResolveCombat(Player player, MonsterBase monster)
    {
        int playerAttack  = CalculateAttackDamage(player);
        int playerDefense = CalculateDefenseBonus(player);
        int monsterDamage = Math.Max(0, monster.AttackPower - playerDefense);

        monster.Hp -= playerAttack;
        player.Hp  -= monsterDamage;

        return $"{player.Name} deals {playerAttack} damage to {monster.Name}. " +
               $"{monster.Name} deals {monsterDamage} damage (blocked {playerDefense}). " +
               $"| {monster.Name} HP: {Math.Max(0, monster.Hp)}  {player.Name} HP: {player.Hp}";
    }
}
