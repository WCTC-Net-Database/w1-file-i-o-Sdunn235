namespace ConsoleRpgEntities.Models;

/// <summary>
/// Goblin monster — weak individually but dangerous in numbers.
/// Loaded from monsters.json via the "$type": "goblin" discriminator.
/// Inherits all base stats from MonsterBase.
/// </summary>
public class Goblin : MonsterBase
{
    /// <summary>Goblin shrieks to summon reinforcements.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} lets out a shrill shriek, rallying nearby goblins!");
    }
}
