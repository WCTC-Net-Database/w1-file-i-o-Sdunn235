namespace ConsoleRpgEntities.Interfaces;

/// <summary>
/// Contract specific to monster entities.
/// ISP: Monsters get their own focused interface separate from IEntity in ConsoleRpg,
/// so only monster-relevant behavior is defined here.
/// </summary>
public interface IMonster
{
    int Id { get; }
    string Name { get; }
    int Level { get; }
    int Hp { get; set; }
    int AttackPower { get; }

    /// <summary>Triggers the monster's unique special ability.</summary>
    void PerformSpecialAction();
}
