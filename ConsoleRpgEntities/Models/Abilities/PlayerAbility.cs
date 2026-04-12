namespace ConsoleRpgEntities.Models.Abilities;

/// <summary>
/// Player-specific ability. Shove represents knockback force.
/// Stored in the Abilities table with AbilityType = "PlayerAbility".
/// </summary>
public class PlayerAbility : Ability
{
    public int Shove { get; set; }
}
