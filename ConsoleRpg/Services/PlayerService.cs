using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models;

namespace ConsoleRpg.Services;

/// <summary>
/// Handles game-level player operations by coordinating with the DAO layer.
///
/// SRP: Player operations only — no combat logic, no UI concerns.
/// DIP: Depends on IContext and IEntityDao abstractions, never on GameContext or PlayerDao directly.
/// </summary>
public class PlayerService : IPlayerService
{
    private readonly IContext _context;
    private readonly IEntityDao<Player> _playerDao;

    public PlayerService(IContext context, IEntityDao<Player> playerDao)
    {
        _context = context;
        _playerDao = playerDao;
    }

    public Player? GetPlayer(int id) =>
        _playerDao.GetById(id);

    public IEnumerable<Player> GetAllPlayers() =>
        _playerDao.GetAll();

    public void UpdatePlayer(Player player) =>
        _playerDao.Update(player);

    public void SaveChanges() =>
        _context.SaveChanges();
}
