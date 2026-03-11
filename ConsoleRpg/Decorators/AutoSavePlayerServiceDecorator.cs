using ConsoleRpg.Services;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models;

namespace ConsoleRpg.Decorators;

/// <summary>
/// Decorator that wraps any IPlayerService and automatically saves after write operations.
/// PlayerService never needs to know about auto-saving — it's layered on top cleanly.
///
/// Decorator Pattern: Adds behavior without modifying the wrapped class (OCP).
/// LSP: This IS an IPlayerService — substitutes perfectly wherever IPlayerService is expected.
/// DIP: GameEngine always sees IPlayerService — it never knows a decorator is in the chain.
///
/// Decorator chain:
///   GameEngine → IPlayerService (this) → PlayerService → PlayerDao → IContext
/// </summary>
public class AutoSavePlayerServiceDecorator : IPlayerService
{
    private readonly IPlayerService _inner;
    private readonly IContext _context;

    public AutoSavePlayerServiceDecorator(IPlayerService inner, IContext context)
    {
        _inner = inner;
        _context = context;
    }

    public Player? GetPlayer(int id) =>
        _inner.GetPlayer(id);

    public IEnumerable<Player> GetAllPlayers() =>
        _inner.GetAllPlayers();

    /// <summary>
    /// Updates the player then immediately persists all in-memory changes to disk.
    /// Auto-save is invisible to the caller — they just call UpdatePlayer normally.
    /// </summary>
    public void UpdatePlayer(Player player)
    {
        _inner.UpdatePlayer(player);
        _context.SaveChanges();
    }

    public void SaveChanges() =>
        _context.SaveChanges();
}
