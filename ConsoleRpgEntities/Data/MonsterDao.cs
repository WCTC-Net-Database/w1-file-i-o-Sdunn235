using ConsoleRpgEntities.Models;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// Handles all data operations for MonsterBase entities.
/// Uses LINQ to query the in-memory monster list provided by IContext.
///
/// DIP: Depends on IContext (abstraction), never on GameContext directly.
/// SRP: This class only knows about Monster operations — nothing else.
/// LSP: Works with MonsterBase, so Goblin, Dragon, and any future monster substitutes safely.
/// </summary>
public class MonsterDao : IEntityDao<MonsterBase>
{
    private readonly IContext _context;

    public MonsterDao(IContext context)
    {
        _context = context;
    }

    public MonsterBase? GetById(int id) =>
        _context.Monsters.FirstOrDefault(m => m.Id == id);

    public IEnumerable<MonsterBase> GetAll() =>
        _context.Monsters;

    public MonsterBase? GetByName(string name) =>
        _context.Monsters.FirstOrDefault(m =>
            m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public void Add(MonsterBase entity) =>
        _context.AddEntity(entity);

    public void Update(MonsterBase entity)
    {
        var existing = GetById(entity.Id);
        if (existing == null) return;

        existing.Name = entity.Name;
        existing.Level = entity.Level;
        existing.Hp = entity.Hp;
        existing.AttackPower = entity.AttackPower;
    }

    public void Delete(int id)
    {
        var entity = GetById(id);
        if (entity != null)
            _context.RemoveEntity(entity);
    }
}
