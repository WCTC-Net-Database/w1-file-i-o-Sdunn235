namespace ConsoleRpgEntities.Models.Abilities;

/// <summary>
/// Goblin-specific ability. Taunt represents provocation strength.
/// Stored in the Abilities table with AbilityType = "GoblinAbility".
/// </summary>
public class GoblinAbility : Ability
{
    public int Taunt { get; set; }
}
