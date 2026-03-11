namespace ConsoleRpgEntities.Models;

/// <summary>
/// The six core D&amp;D-style ability scores for any character.
/// Stored on Player and used by BattleService for combat modifiers.
/// </summary>
public class AbilityScores
{
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
}
