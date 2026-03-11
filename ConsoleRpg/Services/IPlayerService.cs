using ConsoleRpgEntities.Models;

namespace ConsoleRpg.Services;

/// <summary>
/// Contract for game-level player operations.
/// DIP: GameEngine and decorators depend on this — never on PlayerDao directly.
/// </summary>
public interface IPlayerService
{
    Player? GetPlayer(int id);
    IEnumerable<Player> GetAllPlayers();
    void UpdatePlayer(Player player);
    void SaveChanges();
}
