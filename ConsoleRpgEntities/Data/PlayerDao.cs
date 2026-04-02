using ConsoleRpgEntities.Models;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// Handles all data operations for Player entities.
/// Uses LINQ to query the in-memory player list provided by IContext.
///
/// DIP: Depends on IContext (abstraction), never on GameContext directly.
/// SRP: This class only knows about Player operations — nothing else.
/// </summary>
public class PlayerDao : IEntityDao<Player>
{
    private readonly IContext _context;

    public PlayerDao(IContext context)
    {
        _context = context;
    }

    public Player? GetById(int id) =>
        _context.Players.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Player> GetAll() =>
        _context.Players;

    public Player? GetByName(string name) =>
        _context.Players.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public void Add(Player entity) =>
        _context.AddEntity(entity);

    public void Update(Player entity)
    {
        var existing = GetById(entity.Id);
        if (existing == null) return;

        existing.Name = entity.Name;
        existing.CharacterClass = entity.CharacterClass;
        existing.Level = entity.Level;
        existing.Hp = entity.Hp;
        existing.MaxHp = entity.MaxHp;
        existing.AbilityScores = entity.AbilityScores;
        existing.Items = entity.Items;
    }

    public void Delete(int id)
    {
        var entity = GetById(id);
        if (entity != null)
            _context.RemoveEntity(entity);
    }
}
