using ConsoleRpgEntities.Models;

namespace ConsoleRpg.Services;

/// <summary>
/// Contract for all combat calculations.
/// DIP: GameEngine depends on this abstraction — swap any IBattleService in without touching the engine.
/// </summary>
public interface IBattleService
{
    /// <summary>Calculates total attack damage: base Strength + equipped weapon bonuses.</summary>
    int CalculateAttackDamage(Player player);

    /// <summary>Calculates total defense: sum of equipped armor bonuses.</summary>
    int CalculateDefenseBonus(Player player);

    /// <summary>Resolves one round of combat and returns a description of the outcome.</summary>
    string ResolveCombat(Player player, MonsterBase monster);
}
