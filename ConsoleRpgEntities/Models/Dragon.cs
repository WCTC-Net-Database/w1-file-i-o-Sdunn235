namespace ConsoleRpgEntities.Models;

/// <summary>
/// Dragon monster — the most powerful enemy in the game.
/// Loaded from monsters.json via the "$type": "dragon" discriminator.
///
/// This is the README's practice exercise monster. Notice how adding it only required:
///   - This class file
///   - A [JsonDerivedType] attribute on MonsterBase
///   - An entry in monsters.json
/// No other files changed — that is OCP in action.
/// </summary>
public class Dragon : MonsterBase
{
    /// <summary>Additional fire damage dealt when breathing fire.</summary>
    public int FireDamage { get; set; }

    /// <summary>Dragon breathes a torrent of fire across the battlefield.</summary>
    public void BreathFire()
    {
        Console.WriteLine($"{Name} unleashes a torrent of fire, dealing {FireDamage} fire damage!");
    }

    public override void PerformSpecialAction()
    {
        BreathFire();
    }
}
